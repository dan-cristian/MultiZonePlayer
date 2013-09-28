using System;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;

namespace MultiZonePlayer
{
        public class MZPState
        {
            private static MZPState m_sysState = null;
          
            private ArrayList m_systemInputDeviceList = null;
            private Hashtable m_systemInputDeviceNames = null;

            public String powerControlStatusCommand;
            public Hashtable iniUserList = new Hashtable();
            private List<ControlDevice> m_iniControlList = new List<ControlDevice>();
            public List<Playlist> m_playlist = new List<Playlist>();
            public Hashtable zoneDefaultInputs;

            //private List<Utilities.WAVEOUTCAPS> m_waveoutdeviceList;

            private static RemotePipi[] remotePipi;
            private static int maxRemotes = 10;
            private List<ControlDevice> m_systemAvailableControlDevices = new List<ControlDevice>();

            private static BasePowerControl m_powerControlDenkovi, m_powerControlNumato;

            private Hashtable m_zoneInputDeviceNames = null;

            private Hashtable m_playListOLD = null;//list with all songs from current playlist

            private List<Metadata.ZoneDetails> m_zoneList;
            private List<MoodMusic> m_moodMusicList;

            private MultiZonePlayer.Tail m_Tail;
            private ZoneEvents m_zoneEvents;
            private Alarm m_systemAlarm;
            private Boolean m_isFollowMeMusic = false;
            private Boolean m_isPowerFailure = false;

			private Boolean m_isWinloadLoading = false;

			public Boolean IsWinloadLoading
			{
				get { return m_isWinloadLoading; }
				set { m_isWinloadLoading = value; }
			}

            public List<IMessenger> m_messengerList = new List<IMessenger>();
            private NotifyState m_notifyState;

            private List<MusicScheduleEntry> m_musicScheduleList;

            private DateTime m_initMZPStateDateTime = DateTime.Now;
            private DateTime m_lastBuzzDateTime = DateTime.Now;

            private List<Display> m_displayList = new List<Display>();
            private List<ZoneGeneric> m_activeZones = new List<ZoneGeneric>();

			//private USB_RC2.ELROUsbRC2 m_remoteControl = new USB_RC2.ELROUsbRC2();

			private List<Metadata.MacroEntry> m_macroList;
			private List<GenericUPS> m_upsList;

			private DateTime m_lastRulesFileModifiedDate = DateTime.MinValue;
			private DateTime m_lastScheduleFileModifiedDate = DateTime.MinValue;

			private WDIO m_wdio;
			private OneWire m_oneWire;

			public OneWire OneWire
			{
				get { return m_oneWire; }
			}

			/*internal USB_RC2.ELROUsbRC2 RemoteControl
			{
				get { return m_remoteControl; }
				set { m_remoteControl = value; }
			}*/

            public List<ZoneGeneric> ActiveZones
            {
                get { return m_activeZones; }
                set { m_activeZones = value; }
            }

            public List<Display> DisplayList
            {
                get { return m_displayList; }
                set { m_displayList = value; }
            }

			public List<Metadata.MacroEntry> MacroList
			{
				get { return m_macroList; }
			}

            public NotifyState NotifyState
            {
                get { return m_notifyState; }
                set { m_notifyState = value; }
            }
            public Boolean IsPowerFailure
            {
                get { return m_isPowerFailure; }
                set { m_isPowerFailure = value; }
            }

			public WDIO Gpio
			{
				get { return m_wdio; }
			}

            public MZPState()
            {
                m_sysState = this;
                m_notifyState = new NotifyState();
				MLog.LoadFromIni();
                MLog.Log(this, "\r\n-----------------START--------------------");

                m_systemAlarm = new Alarm(1);
                InitRemotes();
                m_powerControlDenkovi = new DenkoviPowerControl("8 Relay Brd USB");
				m_powerControlNumato = new NumatoLPTControl(888);

                MLog.Log(this, "Retrieving system available audio output devices");
                
				String deviceName;
                MLog.Log(this, "Loading zones from ini");
                m_zoneList = new List<Metadata.ZoneDetails>();
                Metadata.ZoneDetails.LoadFromIni(ref m_zoneList);

                m_moodMusicList = new List<MoodMusic>();
                MoodMusic.LoadFromIni(ref m_moodMusicList);

                LoadIniParams();
                LoadIniSections();
                LoadIniUsers();

				LoadSystemAndUserControls();
                
                MLog.Log(this, "Retrieving system available audio input devices");
                DShowUtility.GetDeviceOfCategory(DShowUtility.Clsid_AudioInput, out m_systemInputDeviceList);
                if (m_systemInputDeviceNames == null) m_systemInputDeviceNames = new Hashtable(); else m_systemInputDeviceNames.Clear();

                m_musicScheduleList = new List<MusicScheduleEntry>();
                MusicScheduleEntry.LoadFromIni(ref m_musicScheduleList);

                MLog.Log(this, "Retrieving system audio input devices details");
                int index = 0;
                if (m_systemInputDeviceList != null)
                {
                    foreach (Object m in MZPState.Instance.m_systemInputDeviceList)
                    {
                        deviceName = DShowUtility.GetDisplayName((IMoniker)m);
                        m_systemInputDeviceNames.Add(index.ToString(), deviceName);
                        index++;
                    }
                }

                MLog.Log(this, "Loading other settings from ini");

                LoadIniInput();
                LoadPlaylist();
                

                m_zoneEvents = new ZoneEvents();
               
                WebServer.Initialise();
                m_Tail = new MultiZonePlayer.Tail(IniFile.PARAM_PARADOX_WINLOAD_DATA_FILE[1]);
                m_Tail.MoreData += new MultiZonePlayer.Tail.MoreDataHandler(m_zoneEvents.Tail_MoreData_PARADOX);

                

				RFXDeviceDefinition.LoadFromIni();
				m_wdio = new WDIO();//new GPIO();
				m_oneWire = new OneWire();
                m_messengerList.Add(new GTalkMessengers(IniFile.PARAM_GTALK_USERNAME[1], IniFile.PARAM_GTALK_USERPASS[1]));
                m_messengerList.Add(new SMS());
				m_messengerList.Add(new Modem());
				m_messengerList.Add(new RFXCom());
				m_messengerList.Add(m_wdio);
				m_messengerList.Add(m_oneWire);

				//Thread th = new Thread(() => m_wdio.LoopForEvents());
				//th.Name = "WDIO Event Loop";
				//th.Start();

				LoadMacrosandRules();

				m_upsList = new List<GenericUPS>();
				m_upsList.Add(new MustekUPS(IniFile.PARAM_UPS_MUSTEK_STATUS_URL[1]));
				m_upsList.Add(new APCUPS("Application", IniFile.PARAM_UPS_APC_LOG_SOURCE[1]));
				InitMediaLibrary();
                LogEvent(MZPEvent.EventSource.System, "System started", MZPEvent.EventType.Functionality, MZPEvent.EventImportance.Informative, null);
            }

			public void LoadSystemAndUserControls()
			{
				PopulateSystemControlDevices();
				//loading user selected controls for each zone
				ControlDevice.LoadFromIni(m_iniControlList);
			}

			private void LoadMacrosandRules()
			{
				DateTime fileModified = System.IO.File.GetLastWriteTime(IniFile.CurrentPath() + IniFile.SCHEDULER_FILE);
				if (fileModified != m_lastScheduleFileModifiedDate)
				{
					m_macroList = Metadata.MacroEntry.LoadFromIni();
					RFXDeviceDefinition.LoadFromIni();
					MLog.LoadFromIni();
					m_lastScheduleFileModifiedDate = fileModified;
				}

				fileModified = System.IO.File.GetLastWriteTime(IniFile.CurrentPath() + IniFile.RULES_FILE);
				if (fileModified != m_lastRulesFileModifiedDate)
				{
					Rules.LoadFromIni();
					m_lastRulesFileModifiedDate = fileModified;
				}
			}

			public List<Metadata.MacroEntry> GetZoneMacros(int zoneId)
			{
				List<Metadata.MacroEntry> macros = null;
				String zoneName = GetZoneById(zoneId).ZoneName;
				if (m_macroList != null)
				{
					macros = m_macroList.FindAll(x =>
						x.CommandList != null && x.CommandList.Find(y => y.ZoneName == zoneName) != null
						||
						x.CommandList != null &&
						x.CommandList.Find(z => z.ParameterValueList != null 
							&& (z.ParameterValueList.ToLower().Contains("zoneid=" + zoneId+";")
								|| z.ParameterValueList.ToLower().Contains("zoneid=" + zoneId + "\"") 
								|| z.ParameterValueList.ToLower().Contains("zonename=" + zoneName))) != null
						);
				}
				return macros;
			}

			public static void RestartGenericProc(string procName, string procPath, System.Diagnostics.ProcessWindowStyle startState)
			{
				MLog.Log(null, "Restarting " + procName);
				Utilities.CloseProcSync(procName);
				Utilities.RunProcessWait(procPath,	System.Diagnostics.ProcessWindowStyle.Normal);
			}
            

            public void Shutdown()
            {
                try
                {
                    foreach (string file in Directory.GetFiles(IniFile.CurrentPath(), "*" + IniFile.TEMP_EXTENSION))
                    {
                        File.Delete(file);
                    }
                    PowerControlOff();
                    WebServer.Shutdown();
                    m_Tail.Stop();
                    foreach (Display disp in m_displayList)
                    {
                        disp.Disconnect();
                    }
                    foreach (IMessenger msg in m_messengerList)
                    {
                        msg.Close();
                    }
                    m_sysState = null;
                }
                catch (Exception)
                {
                    m_sysState = null;
                }

            }

            ~MZPState()
            {
                Shutdown();
            }

            public static void RestartComputer(String reason)
            {
                MLog.Log(null, "RESTARTING COMPUTER");
                MZPState.Instance.LogEvent(MZPEvent.EventSource.System, "RESTARTING COMPUTER for reason " + reason, MZPEvent.EventType.Security, MZPEvent.EventImportance.Critical,null);
                Thread.Sleep(3000);
                System.Diagnostics.Process.Start("shutdown.exe", "-r -f -t 0");
            }

            public static void Initialise()
            {
                if (m_sysState != null)
                {
                    m_sysState.Shutdown();
                }
                m_sysState = new MZPState();
            }

            public static bool IsInitialised
            {
                get { return m_sysState != null; }
            }
            public static MZPState Instance
            {
                get
                {
                    return m_sysState;
                }
            }

            public List<Metadata.ZoneDetails> ZoneDetails
            {
                get{return m_zoneList;}
            }

			public int ZonesCount
			{
				get { return ZoneDetails.Count; }
			}

            public ZoneEvents ZoneEvents
            {
                get { return m_zoneEvents; }
            }

            public Alarm SystemAlarm
            {
                get { return m_systemAlarm; }
            }

            public List<MoodMusic> MoodMusicList
            {
                get { return m_moodMusicList; }
            }

            public List<MusicScheduleEntry> MusicScheduleList
            {
                get { return m_musicScheduleList; }
            }
            public Metadata.ZoneDetails GetZoneById(int zoneId)
            {
                return ZoneDetails.Find(item => item.ZoneId == zoneId);
            }

			public Metadata.ZoneDetails GetZoneIdByContainsName(String zoneName)
			{
				if (zoneName != null)
					return ZoneDetails.Find(item => item.ZoneName.Contains(zoneName));
				else
					return null;
			}
            public List<ControlDevice> IniControlDevices
            {
                get {return m_iniControlList;}
            }

            public List<ControlDevice> SystemAvailableControlDevices
            {
                get { return m_systemAvailableControlDevices; }
            }

            public BasePowerControl PowerControl(int zoneid)
            {
				if (GetZoneById(zoneid).PowerType == Metadata.PowerType.Denkovi.ToString())
					return m_powerControlDenkovi;
				else
					return m_powerControlNumato;
            }

			public void PowerControlOn(int zoneid)
			{
				if (GetZoneById(zoneid).PowerType == Metadata.PowerType.Denkovi.ToString())
					m_powerControlDenkovi.PowerOn(zoneid);
				else
					if (GetZoneById(zoneid).PowerType == Metadata.PowerType.Numato.ToString())
						m_powerControlNumato.PowerOn(zoneid);
			}

			public void PowerControlOff(int zoneid)
			{
				if (GetZoneById(zoneid).PowerType == Metadata.PowerType.Denkovi.ToString())
					m_powerControlDenkovi.PowerOff(zoneid);
				else
					if (GetZoneById(zoneid).PowerType == Metadata.PowerType.Numato.ToString())
						m_powerControlNumato.PowerOff(zoneid);
			}

			public void PowerControlOff()
			{
				m_powerControlDenkovi.PowerOff();
				m_powerControlNumato.PowerOff();
			}
			public bool PowerControlIsOn(int zoneid)
			{
				if (GetZoneById(zoneid).PowerType == Metadata.PowerType.Denkovi.ToString())
					return m_powerControlDenkovi.IsPowerOn(zoneid);
				else
					if (GetZoneById(zoneid).PowerType == Metadata.PowerType.Numato.ToString())
						return m_powerControlNumato.IsPowerOn(zoneid);
					else return false;
			}

            public void ToogleFollowMeMusic()
            {
                m_isFollowMeMusic = !m_isFollowMeMusic;
            }

            public Boolean IsFollowMeMusic
            {
                get{return m_isFollowMeMusic;}
            }
            /*public List<String> SystemOutputDeviceNames
            {
                get{return m_systemOutputDeviceNames;}
            }*/

            public Hashtable SystemInputDeviceNames
            {
                get{return m_systemInputDeviceNames;}
            }
            public Hashtable ZoneInputDeviceEntries
            {
                get{return m_zoneInputDeviceNames;}
            }

            public List<Playlist> Playlist
            {
                get { return m_playlist; }
            }

			public bool HasNotifyMove
			{
				get{
					bool move = false;
					move = m_zoneList.Find(x => x.HasRecentMove || x.HasImmediateMove) != null;
					return move;
				}
			}

			public ZoneGeneric GetZoneIfActive(int zoneId)
			{
				ZoneGeneric zp = ActiveZones.Find(item => item.ZoneDetails.ZoneId == zoneId);
				if (zp != null && IsZoneActive(zoneId))
					return zp;
				else
					return null;
			}

			public ZoneGeneric GetZone(int zoneId)
			{
				ZoneGeneric zp = ActiveZones.Find(item => item.ZoneDetails.ZoneId == zoneId);
				return zp;
			}

			public void OpenZone(int zoneId)
			{
				if (!IsZoneActive(zoneId))
				{
					ActiveZones.Add(new ZoneGeneric(zoneId));
				}
			}
			public bool IsZoneActive(int zoneId)
			{
				Metadata.ZoneDetails zone = ZoneDetails.Find(item => item.ZoneId == zoneId);
				if (zone != null)
					return zone.IsActive;
				else return false; ;
			}

            public Hashtable GetAPlayList(int zoneId)
            {
                Hashtable table;
                if (m_playListOLD.Count > 0)
                {

                    table = m_playListOLD[zoneId] as Hashtable;
                    if (table == null)
                        table = m_playListOLD["0"] as Hashtable;
                }
                else
                {
                    table = new Hashtable();
                    table.Add("0", "");
                }

                return table;
            }

            public int GetZoneByControlDevice(String deviceName, string key)
            {
				List<ControlDevice> devices = m_iniControlList.FindAll(x=>x.DeviceName==deviceName);
				switch (devices.Count)
				{
					case 0:
						return -1;
					case 1:
						return devices[0].ZoneId;
					default:
						foreach (ControlDevice dev in devices)
						{
							if (GetZoneById(dev.ZoneId).ClosureIdList == key)
								return dev.ZoneId;
						}
						return -1;
				}
            }

            public String GetControlDeviceByZone(int zoneId)
            {
                foreach (ControlDevice ctrl in MZPState.Instance.m_iniControlList)
                {
                    if (ctrl.ZoneId.Equals(zoneId))
                        return ctrl.DeviceName;
                }
                return null;
            }

            public void InitRemotes()
            {
                //this.multiPlayerDataSet.Remotes.Count;
                remotePipi = new RemotePipi[maxRemotes];
                remotePipi[0] = new RemotePipi();
                //remotePipi[0] = new RemotePipi(frmOptions.GetCommands());
            }

            private void PopulateSystemControlDevices()
            {
                RawInputDevice.EnumerateDevices();
                IDictionaryEnumerator enumerator = RawInputDevice.GetDeviceList().GetEnumerator();
				m_systemAvailableControlDevices.Clear();
                while (enumerator.MoveNext())
                {
                    m_systemAvailableControlDevices.Add(new ControlDevice(-1, 
                        ((RawInputDevice.DeviceInfo)enumerator.Value).deviceName, 
						((RawInputDevice.DeviceInfo)enumerator.Value).Name));
                }
            }

            private void LoadIniParams()
            {
                String pname;
                String pvalue;
                String[] param;

                for (int r = 0; r < IniFile.PARAMS.Length; r++)
                {
                    param = IniFile.PARAMS[r] as String[];
                    pname = param[0] as String;

                    pvalue = IniFile.IniReadValue(IniFile.INI_SECTION_PARAMS, pname);
                    if (pvalue != "")
                        param[1] = pvalue;
                }
            }

            private void LoadIniUsers()
            {
                String id, name, code;
                bool loop = true;
                int r = 0;

                while (loop)
                {
                    id = IniFile.IniReadValue(IniFile.INI_SECTION_USERS + r.ToString(), "UserId");
                    name = IniFile.IniReadValue(IniFile.INI_SECTION_USERS + r.ToString(), "UserName");
                    code = IniFile.IniReadValue(IniFile.INI_SECTION_USERS + r.ToString(), "UserCode");
                    if (id != "")
                    {
                        iniUserList.Add(code, new Users(id, name, code));
                    }
                    else loop = false;
                    r++;
                }
            }

            private void LoadIniSections()
            {
                zoneDefaultInputs = IniFile.LoadAllIniEntriesByKey(IniFile.INI_SECTION_INPUTS);
                powerControlStatusCommand = IniFile.LoadIniEntryByKey(IniFile.INI_SECTION_PARAMS, IniFile.PARAM_POWER_CONTROL_STATUS_APP_GEMBIRD[0]);
            }

            private void LoadPlaylist()
            {
                m_playListOLD = IniFile.LoadAllIniEntriesByKey(IniFile.INI_SECTION_PLAYLIST + "0");
                foreach (String key in m_playListOLD.Keys)
                {
                    m_playlist.Add(new Playlist(m_playListOLD[key].ToString()));
                }
            }

            private void LoadIniInput()
            {
                String inputName, inputType, inputInZoneId, inputDeviceName;
                int r = 0;
                if (m_zoneInputDeviceNames == null) m_zoneInputDeviceNames = new Hashtable(); else m_zoneInputDeviceNames.Clear();

                while (true)
                {
                    inputName = IniFile.IniReadValue(IniFile.INI_SECTION_INPUTDEVICE + r.ToString(), "InputName");
                    if (inputName == "")
                        break;

                    inputType = IniFile.IniReadValue(IniFile.INI_SECTION_INPUTDEVICE + r.ToString(), "InputTypeId_Inputs");
                    inputInZoneId = IniFile.IniReadValue(IniFile.INI_SECTION_INPUTDEVICE + r.ToString(), "InZoneId");
                    inputDeviceName = IniFile.IniReadValue(IniFile.INI_SECTION_INPUTDEVICE + r.ToString(), "InputDeviceName");

                    if ((inputName != "") && (!DShowUtility.ValidateDevice(inputDeviceName)))
                    {
                        MLog.Log(null, "Invalid input device " + inputName + "in ini file " + inputDeviceName);
                    }
                    m_zoneInputDeviceNames.Add(r, new InputEntry(inputName, inputType, inputInZoneId, inputDeviceName));
                    r++;
                }
            }

            private void InitMediaLibrary()
            {
                Thread thmu = new Thread(() => MediaLibrary.InitialiseMusic());
                thmu.Name = "MediaLibrary Music";
                thmu.Start();

				Application.DoEvents();

				Thread thpi = new Thread(() => MediaLibrary.InitialisePictures());
				thpi.Name = "MediaLibrary Pictures";
				thpi.Start();

				Application.DoEvents();

				Thread thmo = new Thread(() => MediaLibrary.InitialiseVideos());
				thmo.Name = "MediaLibrary Movies";
				thmo.Start();
            }

            public int GetZoneIdByAlarmZoneId(int alarmZoneId)
            {
                Metadata.ZoneDetails zone = m_zoneList.Find(x => x.AlarmZoneId.Equals(alarmZoneId));
                if (zone != null)
                    return zone.ZoneId;
                else
                    return -1;
            }

            public int GetZoneIdByCamZoneId(int camId)
            {
                Metadata.ZoneDetails zone = m_zoneList.Find(x => x.CameraId.Equals(camId.ToString()));
                if (zone != null)
                    return zone.ZoneId;
                else
                    return -1;

            }

            //return most recent zone that had an activity
            public int MostRecentZoneWithContext
            {
                get
                {
                    Metadata.ZoneDetails zone = ZoneDetails.OrderByDescending(x => x.LastLocalCommandDateTime).ToList().Find(x =>
                        (x.ActivityType.Equals(Metadata.GlobalCommands.music) 
						|| x.ActivityType.Equals(Metadata.GlobalCommands.streammp3)
                        || x.ActivityType.Equals(Metadata.GlobalCommands.tv) 
						|| x.ActivityType.Equals(Metadata.GlobalCommands.xbmc)) 
                        && x.IsActive);
                    if (zone != null)
                    {
                        MLog.Log(this, "Found most recent active zone id=" + zone.ZoneId + " name="+zone.ZoneName);
                        return zone.ZoneId;
                    }
                    else
                    {
                        zone = ZoneDetails.OrderByDescending(x => x.LastAlarmMovementDateTime).ToList().Find(x => x.HasSpeakers);
                        if (zone != null)
                        {
                            MLog.Log(this, "Found most recent zone with speakers id=" + zone.ZoneId);
                            return zone.ZoneId;
                        }
                        else
                            return -1;
                    }
                }
            }

			//Music zone to clone
			public ZoneGeneric GetFirstZoneMusic()
			{
				ZoneGeneric zone = null;
				Metadata.ZoneDetails zonedetails = ZoneDetails.OrderBy(x => x.LastLocalCommandDateTime).ToList().Find(x =>
                        (x.ActivityType.Equals(Metadata.GlobalCommands.music) && x.IsActive));
				if (zonedetails != null)
					zone = ActiveZones.Find(x => x.GetZoneId() == zonedetails.ZoneId);
				return zone;
			}

            public int GetActiveChildZone(int parentZoneId)
            {
                Metadata.ZoneDetails zone = ZoneDetails.OrderByDescending(x => x.LastLocalCommandDateTime).ToList()
					.Find(x => (x.ParentZoneId==parentZoneId)&&x.IsActive);
                if (zone != null)
                {
                    MLog.Log(this, "Found most recent active child zone id=" + zone.ZoneId + " name=" + zone.ZoneName);
                    return zone.ZoneId;
                }
                else
                {
                    return MostRecentZoneWithContext;
                }
               
            }

            public MoodMusic GetScheduledMood(int zoneId)
            {
                String time, weekday;
                time = DateTime.Now.ToString(IniFile.DATETIME_DAYHR_FORMAT);
                weekday = DateTime.Now.DayOfWeek.ToString().Substring(0, 2);

                foreach(MusicScheduleEntry entry in m_musicScheduleList)
                {
                    if ((entry.ZoneId.Equals(zoneId)||entry.ZoneId.Equals(-1)) && (time.CompareTo(entry.StartTime) >= 0) && (time.CompareTo(entry.EndTime) <= 0) 
                            && entry.WeekDays.Contains(weekday))
                    {
                        return m_moodMusicList.Find(x => x.Name.Equals(entry.Mood));
                    }
                }

                return null;
            }


            public List<MoodMusic> GetScheduledMoodList(int zoneId)
            {
                String time, weekday;
                time = DateTime.Now.ToString(IniFile.DATETIME_DAYHR_FORMAT);
                weekday = DateTime.Now.DayOfWeek.ToString().Substring(0, 2);
                List<MoodMusic> result = new List<MoodMusic>();

                foreach (MusicScheduleEntry entry in m_musicScheduleList)
                {
                    if ((entry.ZoneId.Equals(zoneId) || entry.ZoneId.Equals(-1)) && (time.CompareTo(entry.StartTime) >= 0) && (time.CompareTo(entry.EndTime) <= 0)
                            && entry.WeekDays.Contains(weekday))
                    {
                        MoodMusic mood = m_moodMusicList.Find(x => x.Name.Equals(entry.Mood));
                        if (mood != null) result.Add(mood);
                    }
                }

                return result;
            }

			public string GetZonesStatus()
			{
				String result = "Status at " + DateTime.Now.ToLongDateString() 
					+ " "+DateTime.Now.ToLongTimeString();
				result +=  "\r\n" + MZPState.Instance.SystemAlarm.AreaState;
				result += "\r\nPower Failed=" + MZPState.Instance.IsPowerFailure;
				foreach (Metadata.ZoneDetails zone in MZPState.Instance.ZoneDetails)
				{
					result += "\r\n" + zone.SummaryStatus;
				}
				return result;
			}

			private void HealthCheckiSpy()
			{
				if (DateTime.Now.Subtract(m_initMZPStateDateTime).Duration().TotalSeconds > Convert.ToInt16(IniFile.PARAM_BOOT_TIME_SECONDS[1]))
				{
					if (!Utilities.IsProcAlive(IniFile.PARAM_ISPY_PROCNAME[1]))
					{
						MLog.Log(this, "iSpy proc not running, restarting. Searched for proc:" + IniFile.PARAM_ISPY_PROCNAME[1]);
						Utilities.CloseProcSync(IniFile.PARAM_ISPY_OTHERPROC[1]);
						RestartGenericProc(IniFile.PARAM_ISPY_PROCNAME[1], IniFile.PARAM_ISPY_APP_PATH[1], System.Diagnostics.ProcessWindowStyle.Minimized);
					}
				}
			}
            private void HealthCheckWinload()
            {
                if (DateTime.Now.Subtract(m_initMZPStateDateTime).Duration().TotalSeconds > Convert.ToInt16(IniFile.PARAM_BOOT_TIME_SECONDS[1]))
                {
                    if (!Utilities.IsProcAlive(IniFile.PARAM_PARADOX_WINLOAD_PROCNAME[1]))
                    {
                        MLog.Log(this, "WINLOAD proc not running, restarting");
						m_isWinloadLoading = true;
						RestartGenericProc(IniFile.PARAM_PARADOX_WINLOAD_PROCNAME[1],
							IniFile.PARAM_PARADOX_WINLOAD_APP_PATH[1], System.Diagnostics.ProcessWindowStyle.Normal);
                    }
                    else
                    {
                        if ((DateTime.Now.Subtract(SystemAlarm.LastAreaStateChange).Duration().TotalDays > 1)
                            && (DateTime.Now.Subtract(SystemAlarm.LastAlarmEventDateTime).Duration().TotalDays > 1) )
                        {
                            //MLog.Log(this, "WINLOAD suspected to be not connected");
                        }

                        DateTime lastCamEvent = m_initMZPStateDateTime;
                        foreach (Metadata.ZoneDetails zone in m_zoneList)
                        {
                            if (zone.HasMotionSensor && zone.HasCamera)
                            {
                                if (zone.LastCamAlertDateTime.CompareTo(lastCamEvent) > 0)
                                    lastCamEvent = zone.LastCamAlertDateTime;
                            }
                        }
                        DateTime alarmRefTime = DateTime.Compare(m_initMZPStateDateTime, m_systemAlarm.LastAlarmEventDateTime) > 0
                            ? m_initMZPStateDateTime : m_systemAlarm.LastAlarmEventDateTime;
                        double diff = lastCamEvent.Subtract(alarmRefTime).TotalMinutes;
                        if (diff > 60)
                        {
                            //MLog.Log(this, "WINLOAD potential error, not running, time dif in min=" + diff);
                            //LogEvent(MZPEvent.EventSource.System, "Winload restarting, was not responsive", MZPEvent.EventType.Security, MZPEvent.EventImportance.Error);
                            //m//_systemAlarm.LastAlarmEventDateTime = DateTime.Now;
                            //RestartWinload();
                        }
                        //else MLog.Log(this, "WINLOAD move diff OK, min=" + diff);
                    }
                }
            }

            private void AutoArmCheck()
            {
                if (SystemAlarm.AreaState.Equals(Alarm.EnumAreaState.ready) && (DateTime.Now.Subtract(m_initMZPStateDateTime).Duration().TotalSeconds > Convert.ToInt16(IniFile.PARAM_BOOT_TIME_SECONDS[1])))
                {
                    List<Metadata.ZoneDetails> openZones;
                    openZones = m_zoneList.FindAll(x => x.HasMotionSensor && (x.HasRecentMove || x.HasImmediateMove));

                    if (openZones.Count == 0)
                    {
                        //LogEvent(MZPEvent.EventSource.System, "AUTOARMING", MZPEvent.EventImportance.Security);
                        /*
                        Metadata.ValueList val = new Metadata.ValueList(Metadata.GlobalParams.command, Metadata.GlobalCommands.alarmarm.ToString(),
                                        Metadata.CommandSources.Internal);
                        Metadata.ValueList retval;
                        API.DoCommandFromWeb(val, out retval);
                         */
                    }
                }
            }

            private void HealthCheckMessengers()
            {
                if (DateTime.Now.Subtract(m_initMZPStateDateTime).Duration().TotalSeconds > Convert.ToInt16(IniFile.PARAM_BOOT_TIME_SECONDS[1]))
                {
                    Boolean ok;
                    foreach (IMessenger m in m_messengerList)
                    {
						if (!m.IsFaulty())
						{
							ok = m.TestConnection();
							if (!ok)
							{
								MLog.Log(this, "Error found on test messenger health, reinitialise: " + m.ToString());
								m.Reinitialise();
							}
						}
                    }
                }
            }

            public bool SendMessengerMessageToOne(String message)
            {
				EmailNotifier.SendEmail(message);
                foreach (IMessenger m in m_messengerList)
                {
                    //if (m.IsTargetAvailable())
                    {
                        m.SendMessageToTarget(message);
                        return true;
                    }
                }

                return false;
            }

            public void SendMessengerMessageToAll(String message)
            {
				EmailNotifier.SendEmail(message);
                foreach (IMessenger m in m_messengerList)
                {
                    m.SendMessageToTarget(message);
                }
            }

            public void MessengerMakeBuzz()
            {
                double lastBuzz = DateTime.Now.Subtract(m_lastBuzzDateTime).TotalSeconds;

                if (lastBuzz >= 20)
                {
                    foreach (IMessenger mess in MZPState.Instance.m_messengerList)
                    {
						//if (mess.IsTargetAvailable())
						{
							mess.MakeBuzz();
							m_lastBuzzDateTime = DateTime.Now;
						}
                    }
                }
                
            }

			public void NotifyEventToUsers(MZPEvent mzpevent, string cause, bool excludeSource, bool notifyMessenger)
			{
				MLog.Log(this, "NotifyEvent "+ mzpevent.DisplayMessage() + " cause:" + cause);
				if (notifyMessenger)
				{
					SendMessengerMessageToOne(cause + "; " + mzpevent.DisplayMessage());
					MessengerMakeBuzz();
				}

				if (mzpevent != null)
				{
					List<Metadata.ZoneDetails> zonesToNotify = null;
					zonesToNotify = m_zoneList.FindAll(x => x.HasSpeakers && (x.IsActive || x.HasImmediateMove || x.HasRecentMove || x.LastLocalCommandAgeInSeconds < 600))
						.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.HasImmediateMove).ThenBy(x => x.LastLocalCommandAgeInSeconds).ToList();

					if (excludeSource && mzpevent.ZoneDetails != null)
						zonesToNotify.RemoveAll(x => x.ZoneId == mzpevent.ZoneDetails.ZoneId);

					MLog.Log(this, "NotifyEvent to zones count=" + zonesToNotify.Count);

					Metadata.ValueList vals = new Metadata.ValueList();
					vals.Add(Metadata.GlobalParams.command, Metadata.GlobalCommands.notifyuser.ToString());

					foreach (Metadata.ZoneDetails zone in zonesToNotify)
					{
						if (!mzpevent.ZoneDetails.IsNearbyZone(zone.ZoneId) && !zone.IsNearbyZone(mzpevent.ZoneDetails.ZoneId))
						{
							vals.Set(Metadata.GlobalParams.zoneid, zone.ZoneId.ToString());
							vals.Set(Metadata.GlobalParams.sourcezoneid, mzpevent.ZoneDetails.ZoneId.ToString());
							API.DoCommand(vals);
						}
					}
				}
			}

			public Metadata.CommandResult ExecuteMacro(int macroId)
			{
				Metadata.CommandResult cmdresult = new Metadata.CommandResult();
				cmdresult.Result = Metadata.ResultEnum.ERR;
				Metadata.MacroEntry entry = m_macroList.Find(x => x.Id == macroId);
				if (entry != null)
				{
					cmdresult.ErrorMessage = "macro found but empty command list";
					foreach (Metadata.MacroEntryCommand cmd in entry.CommandList)
					{
						MLog.Log(this, "Executing macro event " + cmd.Command + " in zone=" + cmd.ZoneName);
						Metadata.ValueList val = new Metadata.ValueList(Metadata.GlobalParams.command,
								cmd.Command.ToString(), Metadata.CommandSources.system);
						Metadata.ZoneDetails zone = GetZoneIdByContainsName(cmd.ZoneName);
						if (zone != null)
						{
							val.Add(Metadata.GlobalParams.zoneid, zone.ZoneId.ToString());
						}
						//parameters
						Metadata.MacroEntry.AddParams(cmd.ParameterValueList, ref val);
						cmdresult = API.DoCommand(val);
						if (cmd.DelayMiliSec != 0)
							Thread.Sleep(cmd.DelayMiliSec);
					}
				}
				else
				{
					cmdresult.ErrorMessage = "Macro not found id=" + macroId;
					MLog.Log(this, cmdresult.ErrorMessage);
				}
				return cmdresult;
			}

			public Metadata.CommandResult ExecuteRFXCmd(string cmd)
			{
				Metadata.CommandResult cmdresult = new Metadata.CommandResult();
				cmdresult.Result = Metadata.ResultEnum.ERR;
				if (cmd != null && cmd.Length > 0)
				{
					RFXCom rfx = (RFXCom)m_messengerList.Find(x => x.GetType() == typeof(RFXCom));
					if (rfx != null)
					{
						cmdresult.Command = Metadata.GlobalCommands.rfxcmd.ToString() + ":"+ cmd;
						cmdresult.OutputMessage = rfx.SendCommand(cmd);
						cmdresult.Result = Metadata.ResultEnum.OK;
					}
					else
						cmdresult.ErrorMessage = "RFXcomm instance not found";
				}
				else
					cmdresult.ErrorMessage = "RFXcomm empty cmd";
				return cmdresult;
			}
            public void CheckForWakeTimers()
            {
                String dt = DateTime.Now.ToString(IniFile.DATETIME_DAYHR_FORMAT);
                String weekday = DateTime.Now.DayOfWeek.ToString().Substring(0, 2);
                foreach (Metadata.ZoneDetails zone in MZPState.Instance.ZoneDetails)
                {
                    if ((zone.WakeTime.Length > 0) && (zone.WakeTime.CompareTo(dt) == 0) && zone.WakeWeekDay.Contains(weekday))
                    {
                        Metadata.ValueList val = new Metadata.ValueList(Metadata.GlobalParams.command, 
                            Metadata.GlobalCommands.musicalarm.ToString(), Metadata.CommandSources.system);
                        val.Add(Metadata.GlobalParams.zoneid, zone.ZoneId.ToString());
                        API.DoCommand(val);
                    }
                }
            }

			public void CheckForScheduleMacroEvents()
			{
				LoadMacrosandRules();
				String entrymonth, entryday;
				String hrmin = DateTime.Now.ToString(IniFile.DATETIME_DAYHR_FORMAT);
				String weekday = DateTime.Now.DayOfWeek.ToString().Substring(0, 2).ToUpper();
				string month = DateTime.Now.Month.ToString(IniFile.DATETIME_MONTH_FORMAT).ToUpper();
				foreach (Metadata.MacroEntry entry in m_macroList)
				{
					entrymonth = entry.RepeatMonth != null ? entry.RepeatMonth.ToUpper() : "";
					entryday = entry.RepeatWeekDay != null ? entry.RepeatWeekDay.ToUpper() : "";
					if ((entrymonth == "ALL" || entrymonth == month)
						&& (entryday =="ALL" || entryday.Contains(weekday))
						&& (entry.RepeatTime==hrmin)
						&& (DateTime.Now.Subtract(entry.ExecutedDateTime).TotalSeconds>60))
					{
						ExecuteMacro(entry.Id);
						entry.ExecutedDateTime = DateTime.Now;
					}
				}
			}

			public void CheckForUpsStatus()
			{
				foreach (GenericUPS ups in m_upsList)
				{
					ups.GetStatus();
				}
			}

			public int GetMacroIdByShortcut(string shortcut, string deviceName)
			{
				if (shortcut != null)
				{
					shortcut = shortcut.ToLower();
					deviceName = deviceName.ToLower();
					Metadata.MacroEntry entry = m_macroList.Find(x =>
						x.ShortcutList != null
						&& x.ShortcutList.Find(y => y.Shortcut == shortcut) != null
						&& (x.ShortcutList.Find(y => deviceName.Contains(y.DeviceName)) != null));
					if (entry != null)
						return entry.Id;
				}
				return -1;
			}

            public void CheckForAlarm()
            {
                MZPState.Instance.SystemAlarm.IsMonitoringActive = Utilities.IsProcAlive("WinLoad");
            }

            public void CheckForExternalZoneEvents()
            {
                Display display;
                foreach (Metadata.ZoneDetails zone in MZPState.Instance.ZoneDetails)
                {
                    if (!zone.IsActive)
                    {
                        if (zone.DisplayType.Equals(Display.DisplayTypeEnum.LGTV.ToString()))
                        {
                            display = m_displayList.Find(x => x.Connection.Equals(zone.DisplayConnection));
                            if (display == null)
                            {
                                display = new DisplayLGTV(zone.DisplayConnection, zone);
                                m_displayList.Add(display);
                            }

                            if (display.GetType().Equals(typeof(DisplayLGTV)))
                            {
                                if (((DisplayLGTV)display).IsOn)
                                {
                                    Metadata.ValueList val = new Metadata.ValueList(Metadata.GlobalParams.command,
                                        Metadata.GlobalCommands.tv.ToString(), Metadata.CommandSources.system);
                                    val.Add(Metadata.GlobalParams.zoneid, zone.ZoneId.ToString());
                                    API.DoCommand(val);
                                }
                            }
                        }

                        if (zone.DisplayType.Equals(Display.DisplayTypeEnum.XBMC.ToString()))
                        {
                            if (Utilities.IsProcAlive(IniFile.PARAM_XBMC_PROCESS_NAME[1]))
                            {
                                Metadata.ValueList val = new Metadata.ValueList(Metadata.GlobalParams.command,
                                        Metadata.GlobalCommands.xbmc.ToString(), Metadata.CommandSources.system);
                                val.Add(Metadata.GlobalParams.zoneid, zone.ZoneId.ToString());
                                API.DoCommand(val);
                            }
                        }
                    }
                }
            }

            private void LogEvent(MZPEvent mzpevent)
            {
				string cause;
                MLog.LogEvent(mzpevent);

				if (mzpevent.ZoneDetails != null)
				{
					if (mzpevent.ZoneDetails.IsArmed)
					{
						cause = "Event detected on armed zone "+mzpevent.ZoneDetails.ZoneName;
						
						NotifyEventToUsers(mzpevent, cause, true, true);
					}

					if ((mzpevent.ZoneDetails.IsClosureArmed) 
						&& (mzpevent.Source == MZPEvent.EventSource.Closure)
						&& (mzpevent.ZoneDetails.ClosureOpenCloseRelay.RelayState==Metadata.ClosureOpenCloseRelay.EnumState.ContactClosed))
					{
						cause = "Closure event detected on closure armed zone" + mzpevent.ZoneDetails.ZoneName;
						NotifyEventToUsers(mzpevent, cause, false, false);
					}

					if (mzpevent.ZoneDetails.MovementAlert && (mzpevent.ZoneDetails.IsArmed ||
							(MZPState.Instance.SystemAlarm.AreaState.Equals(Alarm.EnumAreaState.armed) 
							&& (mzpevent.ZoneDetails.AlarmAreaId == MZPState.Instance.SystemAlarm.AreaId))))
					{
						cause = "Event detected on armed area";
						NotifyEventToUsers(mzpevent, cause, false, true);
					}
					else
					{
						/*  MLog.Log(this, "Ignoring alarm event on " + mzpevent.ZoneDetails.ZoneName + " movementalert=" + mzpevent.ZoneDetails.MovementAlert
							  + " zonealarmareaid=" + mzpevent.ZoneDetails.AlarmAreaId + " systemareaid=" + MZPState.Instance.SystemAlarm.AreaId
							  + " areastate=" + MZPState.Instance.SystemAlarm.AreaState);
						 */
					}
				}
				else
				{//area event
					if (SystemAlarm.IsArmed)
					{
						switch (m_systemAlarm.AreaState)
						{
							case Alarm.EnumAreaState.entrydelay:
								cause = "Area entry when area is armed";
								NotifyEventToUsers(mzpevent, cause, false, true);
								break;
						}
					}
				}

                if ((mzpevent.TypeEv.Equals(MZPEvent.EventType.Security) && mzpevent.Importance.Equals(MZPEvent.EventImportance.Critical)))
                {
					cause = "Security critical event";
					NotifyEventToUsers(mzpevent, cause, false, true);
                }

                if (NotifyState.GTalkEnabled)
                {
                    MLog.Log(this, mzpevent.Source + " event optional GTalk notify required");
                    m_messengerList.Find(x => x.GetType().Equals(typeof(GTalkMessengers))).SendMessageToTarget(mzpevent.DisplayMessage());
                }

                if (NotifyState.SMSEnabled)
                {
                    MLog.Log(this, mzpevent.Source + " event optional SMS notify required");
                    m_messengerList.Find(x => x.GetType().Equals(typeof(SMS))).SendMessageToTarget(mzpevent.DisplayMessage());
                }
            }

            public void LogEvent(DateTime dateTime, MZPEvent.EventSource source, String message, MZPEvent.EventType type, MZPEvent.EventImportance importance, Metadata.ZoneDetails zonedetails)
            {
                LogEvent(new MZPEvent(dateTime, source, message, type, importance, zonedetails));
            }

            public void LogEvent(MZPEvent.EventSource source, String message, MZPEvent.EventType type, MZPEvent.EventImportance importance, Metadata.ZoneDetails zonedetails)
            {
                LogEvent(new MZPEvent(DateTime.Now, source, message, type, importance, zonedetails));
            }

            public void Tick()
            {
				HealthCheckiSpy();
                HealthCheckWinload();
                HealthCheckMessengers();
                AutoArmCheck();
                CheckForWakeTimers();
                CheckForAlarm();
                CheckForExternalZoneEvents();
				CheckForScheduleMacroEvents();
				CheckForUpsStatus();
                m_powerControlDenkovi.timerPowerSaving_Tick();
				m_powerControlNumato.timerPowerSaving_Tick();
				m_oneWire.Tick();

				if (MediaLibrary.AllAudioFiles != null)
					MediaLibrary.AllAudioFiles.SaveUpdatedItems();
            }
        }
    }

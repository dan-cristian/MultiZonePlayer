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
            private ArrayList m_systemOutputDeviceList = null;
            private List<String> m_systemOutputDeviceNames = null;
            private ArrayList m_systemInputDeviceList = null;
            private Hashtable m_systemInputDeviceNames = null;

            public String powerControlStatusCommand;
            public Hashtable iniUserList = new Hashtable();
            private List<ControlDevice> m_iniControlList = new List<ControlDevice>();
            public List<Playlist> m_playlist = new List<Playlist>();
            public Hashtable zoneDefaultInputs;

            private List<Utilities.WAVEOUTCAPS> m_waveoutdeviceList;

            private static RemotePipi[] remotePipi;
            private static int maxRemotes = 10;
            private List<ControlDevice> m_systemAvailableControlDevices = new List<ControlDevice>();

            private static BasePowerControl m_powerControl;

            private Hashtable m_zoneInputDeviceNames = null;

            private Hashtable m_playListOLD = null;//list with all songs from current playlist

            private List<Metadata.ZoneDetails> m_zoneList;
            private List<MoodMusic> m_moodMusicList;

            private MultiZonePlayer.Tail m_Tail;
            private ZoneEvents m_zoneEvents;
            private Alarm m_systemAlarm;
            private Boolean m_isFollowMeMusic = false;
            private Boolean m_isPowerFailure = false;

            

            public List<IMessenger> m_messengerList = new List<IMessenger>();
            private NotifyState m_notifyState;

            private List<MusicScheduleEntry> m_musicScheduleList;

            private DateTime m_initMZPStateDateTime = DateTime.Now;
            private DateTime m_lastBuzzDateTime = DateTime.Now;

            private WinEventLogReader m_winEventLogReader;

            private List<Display> m_displayList = new List<Display>();
            private List<ZoneGeneric> m_activeZones = new List<ZoneGeneric>();

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

            public MZPState()
            {
                m_sysState = this;
                m_notifyState = new NotifyState();

                MLog.Log(this, "\r\n-----------------START--------------------");

                m_systemAlarm = new Alarm(1);
                InitRemotes();
                m_powerControl = new DenkoviPowerControl("8 Relay Brd USB");

                

                MLog.Log(this, "Retrieving system available audio output devices");
                DShowUtility.GetDeviceOfCategory(DShowUtility.Clsid_AudioOutRender, out m_systemOutputDeviceList);
                if (m_systemOutputDeviceNames == null) m_systemOutputDeviceNames = new List<String>(); else m_systemOutputDeviceNames.Clear();

                m_waveoutdeviceList = Utilities.GetDevCapsPlayback();

                MLog.Log(this, "Retrieving system audio output devices details");
                String deviceName;
                if (m_systemOutputDeviceList != null)
                {
                    foreach (Object m in MZPState.Instance.m_systemOutputDeviceList)
                    {
                        deviceName = DShowUtility.GetDisplayName((IMoniker)m);
                        m_systemOutputDeviceNames.Add(deviceName);
                    }
                }
                m_systemOutputDeviceNames.Add(IniFile.DEFAULT_AUTO_DEV_NAME);


                MLog.Log(this, "Loading zones from ini");
                m_zoneList = new List<Metadata.ZoneDetails>();
                Metadata.ZoneDetails.Initialise(m_systemOutputDeviceNames, m_waveoutdeviceList);
                Metadata.ZoneDetails.LoadFromIni(ref m_zoneList);

                m_moodMusicList = new List<MoodMusic>();
                MoodMusic.LoadFromIni(ref m_moodMusicList);

                LoadIniParams();
                LoadIniSections();
                LoadIniUsers();
                PopulateSystemControlDevices();

                ControlDevice.LoadFromIni(m_iniControlList);

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

                InitMediaLibrary();

                m_zoneEvents = new ZoneEvents();
               
                WebServer.Initialise();
                m_Tail = new MultiZonePlayer.Tail(IniFile.PARAM_PARADOX_WINLOAD_DATA_FILE[1]);
                m_Tail.MoreData += new MultiZonePlayer.Tail.MoreDataHandler(m_zoneEvents.Tail_MoreData_PARADOX);

                m_winEventLogReader = new WinEventLogReader("Application");
                m_winEventLogReader.AddSource("APC UPS Service");

                m_messengerList.Add(new GTalkMessengers(IniFile.PARAM_GTALK_USERNAME[1], //IniFile.PARAM_GTALK_SERVER[1], 
                    IniFile.PARAM_GTALK_USERPASS[1]));
                m_messengerList.Add(new SMS());

                LogEvent(MZPEvent.EventSource.System, "System started", MZPEvent.EventType.Functionality, MZPEvent.EventImportance.Informative, null);
            }

            public static void RestartWinload()
            {
                MLog.Log(null, "Restarting Winload");
                Utilities.CloseProcSync(IniFile.PARAM_PARADOX_WINLOAD_PROCNAME[1]);
                Utilities.RunProcessWait(IniFile.PARAM_PARADOX_WINLOAD_APP_PATH[1]);
            }

            public static void RestartXBMC()
            {
                MLog.Log(null, "Restarting XBMC");
                Utilities.CloseProcSync(IniFile.PARAM_XBMC_PROCESS_NAME[1]);
                Utilities.RunProcessWait(IniFile.PARAM_XBMC_APP_PATH[1]);
            }

            public void Shutdown()
            {
                try
                {
                    foreach (string file in Directory.GetFiles(IniFile.CurrentPath(), "*" + IniFile.TEMP_EXTENSION))
                    {
                        File.Delete(file);
                    }
                    PowerControl.PowerOff();
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
                    //if ((m_sysState == null))
                    //    m_sysState = new MZPState();
                    return m_sysState;
                }
            }

            public List<Metadata.ZoneDetails> ZoneDetails
            {
                get
                {
                    return m_zoneList;
                }
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

            public List<ControlDevice> IniControlDevices
            {
                get {return m_iniControlList;}
            }

            public List<ControlDevice> SystemAvailableControlDevices
            {
                get { return m_systemAvailableControlDevices; }
            }

            public BasePowerControl PowerControl
            {
                get
                {
                    return m_powerControl;
                }
            }

            public void ToogleFollowMeMusic()
            {
                m_isFollowMeMusic = !m_isFollowMeMusic;
            }

            public Boolean IsFollowMeMusic
            {
                get
                {
                    return m_isFollowMeMusic;
                }
            }
            public List<String> SystemOutputDeviceNames
            {
                get
                {
                    return m_systemOutputDeviceNames;
                }
            }

            public Hashtable SystemInputDeviceNames
            {
                get
                {
                    return m_systemInputDeviceNames;
                }
            }
            public Hashtable ZoneInputDeviceEntries
            {
                get
                {
                    return m_zoneInputDeviceNames;
                }
            }

            public List<Playlist> Playlist
            {
                get { return m_playlist; }
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

            public int GetZoneByControlDevice(String deviceName)
            {
                foreach (ControlDevice ctrl in MZPState.Instance.m_iniControlList)
                {
                    if (ctrl.DeviceName.Equals(deviceName))
                        return ctrl.ZoneId;
                }
                return -1;
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
                while (enumerator.MoveNext())
                {
                    m_systemAvailableControlDevices.Add(new ControlDevice(-1, 
                        ((RawInputDevice.DeviceInfo)enumerator.Value).deviceName, ((RawInputDevice.DeviceInfo)enumerator.Value).Name));
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
                Thread th = new Thread(() => MediaLibrary.Initialise());
                th.Start();
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
                        (x.ActivityType.Equals(Metadata.GlobalCommands.music) || x.ActivityType.Equals(Metadata.GlobalCommands.streammp3) ||
                        x.ActivityType.Equals(Metadata.GlobalCommands.tv) || x.ActivityType.Equals(Metadata.GlobalCommands.xbmc)) 
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

            public int GetChildZone(int parentZoneId)
            {
                Metadata.ZoneDetails zone = ZoneDetails.OrderByDescending(x => x.LastLocalCommandDateTime).ToList().Find(x => x.ParentZoneId==parentZoneId);
                if (zone != null)
                {
                    MLog.Log(this, "Found most recent child zone id=" + zone.ZoneId + " name=" + zone.ZoneName);
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
                time = DateTime.Now.ToString(IniFile.DATETIME_FORMAT);
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
                time = DateTime.Now.ToString(IniFile.DATETIME_FORMAT);
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


            private void HealthCheckWinload()
            {
                if (DateTime.Now.Subtract(m_initMZPStateDateTime).Duration().TotalSeconds > Convert.ToInt16(IniFile.PARAM_BOOT_TIME_SECONDS[1]))
                {
                    if (!Utilities.IsProcAlive(IniFile.PARAM_PARADOX_WINLOAD_PROCNAME[1]))
                    {
                        MLog.Log(this, "WINLOAD proc not running, restarting");
                        RestartWinload();
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
                            MLog.Log(this, "WINLOAD potential error, not running, time dif in min=" + diff);
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
                        ok = m.TestConnection();
                        if (!ok)
                        {
                            MLog.Log(this, "Error found on test mesenger health, reinitialise: " + m.ToString());
                            m.Reinitialise();
                        }
                    }
                }
            }

            

            public bool SendMessengerMessageToOne(String message)
            {
                foreach (IMessenger m in m_messengerList.FindAll(x => x.GetType().Equals(typeof(GTalkMessengers))))
                {
                    if (m.IsTargetAvailable())
                    {
                        m.SendMessage(message, IniFile.PARAM_GTALK_TARGETUSER[1]);
                        return true;
                    }
                }

                foreach (IMessenger m in m_messengerList.FindAll(x => x.GetType().Equals(typeof(SMS))))
                {
                    if (m.IsTargetAvailable())
                    {
                        m.SendMessage(message, IniFile.PARAM_SMS_TARGETNUMBER[1]);
                        return true;
                    }
                }

                return false;
            }

            public void SendMessengerMessageToAll(String message)
            {
                foreach (IMessenger m in m_messengerList)
                {
                    if (m.GetType() == typeof(GTalkMessengers))
                        m.SendMessage(message, IniFile.PARAM_GTALK_TARGETUSER[1]);
                    if (m.GetType() == typeof(SMS))
                        m.SendMessage(message, IniFile.PARAM_SMS_TARGETNUMBER[1]);
                }
            }

            public void MessengerMakeBuzz()
            {
                double lastBuzz = DateTime.Now.Subtract(m_lastBuzzDateTime).TotalSeconds;

                if (lastBuzz >= 20)
                {
                    foreach (IMessenger mess in MZPState.Instance.m_messengerList)
                    {
                        mess.MakeBuzz();
                    }
                }
                m_lastBuzzDateTime = DateTime.Now;
            }

            public void CheckForWakeTimers()
            {
                String dt = DateTime.Now.ToString(IniFile.DATETIME_FORMAT);
                String weekday = DateTime.Now.DayOfWeek.ToString().Substring(0, 2);
                foreach (Metadata.ZoneDetails zone in MZPState.Instance.ZoneDetails)
                {
                    if ((zone.WakeTime.Length > 0) && (zone.WakeTime.CompareTo(dt) == 0) && zone.WakeWeekDay.Contains(weekday))
                    {
                        Metadata.ValueList val = new Metadata.ValueList(Metadata.GlobalParams.command, 
                            Metadata.GlobalCommands.musicalarm.ToString(), Metadata.CommandSources.system);
                        val.Add(Metadata.GlobalParams.zoneid, zone.ZoneId.ToString());
                        Metadata.ValueList retval;
                        API.DoCommandFromWeb(val, out retval);
                    }
                }
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
                                    Metadata.ValueList retval;
                                    API.DoCommandFromWeb(val, out retval);
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
                                Metadata.ValueList retval;
                                API.DoCommandFromWeb(val, out retval);
                            }
                        }
                    }
                }
            }


            private void LogEvent(MZPEvent mzpevent)
            {
                String message = mzpevent.Source + " | " + mzpevent.Message + " | " + mzpevent.TypeEv + " | " + mzpevent.Importance + " | " + mzpevent.DateTime;
                MLog.LogEvent(mzpevent);

                if (mzpevent.ZoneDetails != null)
                {
                    if (mzpevent.ZoneDetails.IsArmed)
                    {
                        MLog.Log(this, mzpevent.Source + " event on " + mzpevent.ZoneDetails.ZoneName + " when zone is armed, buzz required");
                        SendMessengerMessageToOne(message);
                        MessengerMakeBuzz();
                    }

                    if (mzpevent.ZoneDetails != null && mzpevent.ZoneDetails.MovementAlert && (mzpevent.ZoneDetails.IsArmed ||
                            (MZPState.Instance.SystemAlarm.AreaState.Equals(Alarm.EnumAreaState.armed) && (mzpevent.ZoneDetails.AlarmAreaId == MZPState.Instance.SystemAlarm.AreaId))))
                    {
                        MLog.Log(this, mzpevent.Source + " event on " + mzpevent.ZoneDetails.ZoneName + " when zone armed, hard notify required");
                        SendMessengerMessageToOne(message);
                        MessengerMakeBuzz();
                    }
                    else
                        MLog.Log(this, "Ignoring alarm event on " + mzpevent.ZoneDetails.ZoneName + " movementalert=" + mzpevent.ZoneDetails.MovementAlert
                            + " zonealarmareaid=" + mzpevent.ZoneDetails.AlarmAreaId + " systemareaid=" + MZPState.Instance.SystemAlarm.AreaId
                            + " areastate=" + MZPState.Instance.SystemAlarm.AreaState);
                }

                if ((mzpevent.TypeEv.Equals(MZPEvent.EventType.Security) && mzpevent.Importance.Equals(MZPEvent.EventImportance.Critical)))
                {
                    MLog.Log(this, mzpevent.Source + " security critical event hard notify required");
                    SendMessengerMessageToOne(message);
                    MessengerMakeBuzz();
                }

                if (NotifyState.GTalkEnabled)
                {
                    MLog.Log(this, mzpevent.Source + " event optional GTalk notify required");
                    m_messengerList.Find(x => x.GetType().Equals(typeof(GTalkMessengers))).SendMessage(message, IniFile.PARAM_GTALK_TARGETUSER[1]);
                }

                if (NotifyState.SMSEnabled)
                {
                    MLog.Log(this, mzpevent.Source + " event optional SMS notify required");
                    m_messengerList.Find(x => x.GetType().Equals(typeof(SMS))).SendMessage(message, IniFile.PARAM_SMS_TARGETNUMBER[1]);
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
                /*if (DateTime.Now.ToString(IniFile.DATETIME_FORMAT).Equals(IniFile.PARAM_LIBRARY_AUTOUPDATE_HOUR[1]))
                {
                    if (MediaLibrary.IsInitialised)
                        InitMediaLibrary();
                    else
                        MLog.Log(null, "medialib not initialised, cannot do another init");
                }*/

                HealthCheckWinload();
                HealthCheckMessengers();
                AutoArmCheck();
                CheckForWakeTimers();
                CheckForAlarm();
                CheckForExternalZoneEvents();
                PowerControl.timerPowerSaving_Tick();

            }
        }

    }

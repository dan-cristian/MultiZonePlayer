using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;


namespace MultiZonePlayer
{

    class UtilityClasses
    {
    }


    /*
     * Interface collection of classes used for mobile client comm 
     */
    public class Metadata
    {
        //public static String ERR = "NotOK";
        //public static String OK = "OK";
        public enum ResultEnum
        {
            ERR,
            OK
        }

        public enum GlobalCommands
        {
            nul,
            k0,
            k1,
            k2,
            k3,
            k4,
            k5,
            k6,
            k7,
            k8,
            k9,
            help,
            status,
            selectzone,
            sleep,
            shutdown,
            music,
            video,
            tv,
            xbmc,
            play,
            stop,
            pause,
            next,
            previous,
            rewind,
            ffwd,
            up,
            down,
            left,
            right,
            volumeset,
            volumedown,
            volumeup,
            mute,
            repeat,
            fullscreen,
            ratingset,
            enter,
            record,
            guide,
            chup,
            chdown,
            photo,
            dvd,
            radio,
            streammp3,
            back,
            musicclone,
            microphone,
            genrelist,
            setgenrelist,
            sleeptimer,
            artistlist,
            setartistlist,
            setwaketimer,
            medialist,
            setmediaitem,
            cameraevent,
            dismisscameraalert,
            togglecameraalert,
            setmoodmusic,
            getmoodmusiclist,
            searchmediaitem,
            musicalarm,
            alarmevent,
            alarmarm,
            alarmdisarm,
            alarmkeypadbeep,
            alarmstay,
            alarmautoarm,
            micrecord,
            micplay,
            followmemusic,
            restartispy,
            restartwinload,
            holdcriteria,
            setnotify,
            restartsystem,
            sendsms,
            zonearm,
            zonedisarm,
            makebuzz,
            powerevent,
            tvsetinput,
            powercycle,
			remotepoweron,
			remotepoweroff,
			remoteadjustdim,
			getpicture
        }
        public enum GlobalParams
        {
            command,
            zoneid,
            zonename,
            msg,
            result,
            selectedindex,
            datetime,
            volumelevel,
            ratingvalue,
            indexarray,
            activity,
            alertsource,
            alertindex,
            oid,//=camid
            pass,
            searchvalue,
            action,
            scope,
            status,
            areaid,
            weekday,
            cmdsource,
			contenttype,
			count,
			interval,
			face,
			remoteid,
			moodname,
			moodindex,
			dimvalue,
			r//random no
        }

        public enum GlobalCommandsUniversal
        {
            volumedown = GlobalCommands.volumedown,
            volumeup = GlobalCommands.volumeup,
            mute = GlobalCommands.mute,
            chup = GlobalCommands.chup,
            chdown = GlobalCommands.chdown,
            enter = GlobalCommands.enter,
            next = GlobalCommands.next,
            previous = GlobalCommands.previous,
            stop = GlobalCommands.stop,
            up = GlobalCommands.up,
            down = GlobalCommands.down,
            right = GlobalCommands.right,
            left = GlobalCommands.left,
        }

        public enum CommandSources
        {
            web,
            rawinput,
            gui,
            events,
            mobileslow,
            system
        }

        public class CommandSyntax
        {
            GlobalCommands Command;
            List<GlobalParams> Params;
            
            public CommandSyntax(GlobalCommands command)
            {
                Command = command;
            }

            public CommandSyntax(GlobalCommands command, params GlobalParams[] parameters)
            {
                Command = command;
                Params = new List<GlobalParams>();
                foreach (GlobalParams param in parameters)
                {
                    Params.Add(param);
                }
            }

            public static Boolean Validate(ValueList vals)
            {
                String cmdName = vals.GetValue(Metadata.GlobalParams.command);
                if (Enum.IsDefined(typeof(Metadata.GlobalCommands), cmdName))
                {
                    Metadata.GlobalCommands apicmd = (Metadata.GlobalCommands)Enum.Parse(typeof(Metadata.GlobalCommands), cmdName);
                    CommandSyntax cmdSynt = SystemCommands.Find(x => x.Command.Equals(apicmd));
                    if (cmdSynt != null)
                    {
                        foreach (GlobalParams param in cmdSynt.Params)
                        {
                            if (vals.GetValue(param) == null)
                            {
                                MLog.Log(null, "Expected parameter not found in cmd=" + cmdName + " param=" + param.ToString());
                                return false;
                            }
                        }
                        return true;
                    }
                    else
                        return true;//cmd without parameters
                }
                else MLog.Log(null, "Invalid command=" + cmdName);
                return false;
            }
        }

        public static List<CommandSyntax> SystemCommands = new List<CommandSyntax>(){
            new CommandSyntax(GlobalCommands.alarmarm,          GlobalParams.areaid),
            new CommandSyntax(GlobalCommands.alarmdisarm,       GlobalParams.areaid),
            new CommandSyntax(GlobalCommands.alarmevent,        GlobalParams.status, GlobalParams.datetime),
            new CommandSyntax(GlobalCommands.alarmkeypadbeep,   GlobalParams.areaid),
            new CommandSyntax(GlobalCommands.alarmstay,         GlobalParams.areaid),
            new CommandSyntax(GlobalCommands.volumeset,         GlobalParams.volumelevel),
            new CommandSyntax(GlobalCommands.ratingset,         GlobalParams.ratingvalue),
            new CommandSyntax(GlobalCommands.remotepoweron,		GlobalParams.remoteid),
			new CommandSyntax(GlobalCommands.remotepoweroff,	GlobalParams.remoteid),
			new CommandSyntax(GlobalCommands.remoteadjustdim,	GlobalParams.remoteid, GlobalParams.dimvalue)
            /*
            genrelist,
            setgenrelist,
            sleeptimer,
            artistlist,
            setartistlist,
            setwaketimer,
            medialist,
            setmediaitem,
            cameraevent,
            dismisscameraalert,
            togglecameraalert,
            setmoodmusic,
            getmoodmusiclist,
            searchmediaitem,
            musicalarm,
            alarmevent,
            alarmarm,
            alarmdisarm,
            alarmkeypadbeep,
            alarmstay,
            alarmautoarm,
            micrecord,
            micplay,
            followmemusic,
            restartispy,
            restartwinload,
            holdcriteria,
            setnotify,
            restartsystem,
            sendsms,
            zonearm,
            zonedisarm,
            makebuzz,
            powerevent,
            tvsetinput,
            powercycle
             */
        };

        public class CommandResult
        {
            public ResultEnum Result;
            public String ErrorMessage = "";
            public ServerStatus ServerStatus;
            public ValueList ValueList;

            public CommandResult()
            {
            }

            public CommandResult(ResultEnum p_result, String message)
            {
                Result = p_result;
                ErrorMessage = message;
            }
        }

        public class ValueList
        {
            
            public List<String> Values;
            public List<String> Keys;
            public List<String> IndexList;
            public List<String> IndexValueList;
            public CommandSources CommandSource;
            
            public byte[] BinaryData;

            public ValueList()
            {
                Values = new List<String>();
                Keys = new List<String>();
            }

            public ValueList(CommandSources source)
            {
                Values = new List<String>();
                Keys = new List<String>();
                CommandSource = source;
            }

            /*public ValueList(String firstParamValue)
            {
                Values = new List<String>();
                Values.Add(firstParamValue);
            }*/

            public ValueList(GlobalParams firstParamKey, String firstParamValue, CommandSources source)
            {
                Values = new List<String>();
                Keys = new List<String>();
                Values.Add(firstParamValue);
                Keys.Add(firstParamKey.ToString());
                CommandSource = source;
            }

            /*
            public void Add(String value)
            {
                Values.Add(value);
            }*/

            public void Add(GlobalParams key, String value)
            {
                if (Values.Count == Keys.Count)
                {
                    Values.Add(value);
                    Keys.Add(key.ToString());
                }
                else
                    throw new Exception("Number of values is not equal to number of ids, incorrect use");
            }

            public void Add(String globalparamkey, String value)
            {
                if (Enum.IsDefined(typeof(Metadata.GlobalParams), globalparamkey))
                {
                    Metadata.GlobalParams gp = (Metadata.GlobalParams)Enum.Parse(typeof(Metadata.GlobalParams), globalparamkey);
                    if (Values.Count == Keys.Count)
                    {
                        Values.Add(value);
                        Keys.Add(gp.ToString());
                    }
                    else
                        throw new Exception("Number of values is not equal to number of ids, incorrect use");
                }
                else
                    throw new Exception("Undefined enum value for GlobalParams = " + globalparamkey);
                
            }

            public void AddIndexValue(String value)
            {
                if (IndexValueList == null) IndexValueList = new List<string>();
                IndexValueList.Add(value);
            }

            public String GetValue(GlobalParams key)
            {
                int index = Keys.IndexOf(key.ToString());
                if (index >= 0)
                    return Values[index];
                else
                    return null;
            }

            public String GetValue(String key)
            {
                int index = Keys.IndexOf(key);
                if (index >= 0)
                    return Values[index];
                else
                    return null;
            }

            public void SetBulk(List<String> p_indexes, List<String> p_values)
            {
                IndexList = p_indexes;
                IndexValueList = p_values;
            }

            public bool ContainsValue(String value)
            {
                return Values.Contains(value);
            }

            public bool ContainsIndexValue(String value)
            {
                return IndexValueList.Contains(value);
            }

            public bool ContainsKey(GlobalParams key)
            {
                return Keys.Contains(key.ToString());
            }

            public List<String> KeyEnumerator()
            {
                return Keys;
            }

            public void SetIndexValues(List<String> list)
            {
                IndexValueList = list;
            }
        }

        public enum ZoneState
        {
            NotInitialised,
            NotStarted,
            Running,
            Paused,
            Closing,
            Undefined,
            Error
        }

        public class VolumeLevels
        {
            public static int VolumeFull = 0;
            //public static int VolumeDefault = -4500;
            public static int VolumeSilence = -10000;
        }

		public enum ZoneNotifyState
		{
			Open, 
			Closed
		}

        public class ZoneDetails
        {
            public int ZoneId = 0;
            public Boolean IsActive = false;
            //public int MinutesUntilSleep = -1;
            public String SleepHourMin = "";//format HH:MM
            public String ZoneName;
            public ZoneState ZoneState;
            public int ParentZoneId = -1;
            public int PowerIndex;
            public String WakeTime = "";
            public String WakeWeekDay = "";
            public String CameraId = "";
            public Boolean HasSpeakers = false;
            public int DefaultVolumePercent;
            public String OutputKeywords;
            public String OutputDeviceUserSelected;
            
            public String OutputDeviceNameWaveVLC;
            public int WavedeviceIndex;
            public Boolean HasCamera = false;
            public Boolean IsArmed = false;
            public int AlarmZoneId = -1;
            public int AlarmAreaId=-1;
            public Boolean HasMotionSensor = false;
            public Boolean HasMicrophone = false;
            public Boolean HasDisplay = false;
            public String DisplayConnection = "";
            public String DisplayType = "";
            public Boolean RequirePower = false;
            
            public int VolumeLevel;
            public long Position = 0;
            public int PositionPercent = 0;
            public GlobalCommands ActivityType = GlobalCommands.nul;
            public String Title;
            public int Rating = 0;
            public int Playcount = 0;
            public int PlaylistCount = 0;
            public String Playlist;
            public String Author;
            public String Genre;
            public String Album;
            public String Year;
            public String SourceURL;

            public Boolean CameraAlertActive = true;
            public Boolean MovementAlert = false;
            public DateTime LastAlarmMovementDateTime = DateTime.MinValue;
            public DateTime LastCamAlertDateTime = DateTime.MinValue;
            public DateTime LastLocalCommandDateTime = DateTime.MinValue;
			public ZoneNotifyState NotifyZoneEventTriggered = ZoneNotifyState.Closed;
			public DateTime LastNotifyZoneEventTriggered;
            // not serializable, hidden from json
            
            
            protected static int m_intervalImmediate, m_intervalRecent, m_intervalPast;

            public ZoneDetails()
            {
            }

            /*public static void Initialise(List<String> p_systemOutputDevices, List<Utilities.WAVEOUTCAPS> p_systemWaveOutputDevices)
            {
                m_systemOutputDevices = p_systemOutputDevices;
                m_systemWaveOutputDevices = p_systemWaveOutputDevices;
            }*/

            public ZoneDetails(int p_zoneId, String p_zoneName)
            {
                ZoneId = p_zoneId;
                ZoneName = p_zoneName;
                ZoneState = ZoneState.NotInitialised;
                ActivityType = GlobalCommands.nul;
                LoadStateFromIni();
            }

            public override string ToString()
            {
                return "ID="+ZoneId+";Name="+ZoneName;
            }
            #region getters
                        
            public String SummaryStatus
            {
                get {
                    String val = "#"+ ZoneId +" "+ ZoneName + (IsActive?" Active":"") 
                        + (ActivityType.Equals(GlobalCommands.nul)?"":" "+ActivityType.ToString()) 
                        + (IsArmed?" Armed ":" ") 
                        + (HasImmediateMove? " ImmediateMove ":" ")
                        + (HasRecentMove? " RecentMove ":" ")
                        + SourceURL;
                    return val;
                }
            }
            public DateTime LastMovementDate
            {
                get
                {
                    //todo add localcommand in compare
                    if ((LastAlarmMovementDateTime).CompareTo(LastCamAlertDateTime) >= 1)
                        return LastAlarmMovementDateTime;
                    else
                        return LastCamAlertDateTime;
                }
            }

			public double LastLocalCommandAgeInSeconds
			{
				get { return DateTime.Now.Subtract(LastLocalCommandDateTime).TotalSeconds; }
			}
            public Boolean HasImmediateMove
            {
                get
                {
                    double span = DateTime.Now.Subtract(LastMovementDate).TotalMinutes;
                    return (span <= m_intervalImmediate);
                }
            }

            public Boolean HasRecentMove
            {
                get
                {
                    double span = DateTime.Now.Subtract(LastMovementDate).TotalMinutes;
                    return (span > m_intervalImmediate && span<=m_intervalRecent);
                }
            }
            public Boolean HasPastMove
            {
                get
                {
                    double span = DateTime.Now.Subtract(LastMovementDate).TotalMinutes;
                    return (span > m_intervalRecent && span <= m_intervalPast);
                }
            }
            public Boolean HasNoMove
            {
                get
                {
                    double span = DateTime.Now.Subtract(LastMovementDate).TotalMinutes;
                    return (span > m_intervalPast);
                }
            }

            public Boolean HasPastActivity
            {
                get
                {
                    double span = DateTime.Now.Subtract(LastLocalCommandDateTime).TotalMinutes;
                    return (HasPastMove || (span > m_intervalRecent && span <= m_intervalPast));
                }
            }

            public String OutputDeviceDirectXName
            {
                get
                {
                    String[] split;
                    split = OutputDeviceAutoCompleted().Split(new String[]{"\\DirectSound: "}, 
						StringSplitOptions.RemoveEmptyEntries);
                    if (split.Length >= 2)
                        return split[1];
                    else
                        return "";
                }
            }
            private const String DIV_SHOW = "block";
            private const String DIV_HIDE = "none";
			private const String IMG_TAG = "img";
			private const String VIDEO_TAG = "video";

            public String HTMLDIVStateMusicOrRadio
            {
                get
                {
                    String res;
                    if (IsActive && (ActivityType.Equals(GlobalCommands.music) 
						|| ActivityType.Equals(GlobalCommands.streammp3)
						|| ActivityType.Equals(GlobalCommands.xbmc)))
                        res = DIV_SHOW;
                    else
                        res = DIV_HIDE;
                    return res;
                }
            }

			public String HasMediaActiveAsDiv
			{
				get
				{
					String res;
					if (IsActive && (ActivityType.Equals(GlobalCommands.music)
						|| ActivityType.Equals(GlobalCommands.streammp3)
						|| ActivityType.Equals(GlobalCommands.xbmc)))
						res = DIV_SHOW;
					else
						res = DIV_HIDE;
					return res;
				}
			}

			public String HasNotifyMoveAsImg
			{
				get { return HasImmediateMove || HasRecentMove ? IMG_TAG : "inactive_"+IMG_TAG; }
			}

			public String HasNotifyMoveAsVideo
			{
				get { return HasImmediateMove || HasRecentMove ? VIDEO_TAG : "inactive_" + VIDEO_TAG; }
			}

			public String IsActiveAsDiv
			{
				get {return IsActive ? DIV_SHOW : DIV_HIDE;}
			}

			public String HasImmediateMoveAsDiv
			{
				get { return HasImmediateMove ? DIV_SHOW : DIV_HIDE; }
			}

			public String HasRecentMoveAsDiv
			{
				get { return HasRecentMove ? DIV_SHOW : DIV_HIDE; }
			}

			public String HasPastMoveAsDiv
			{
				get { return HasPastMove ? DIV_SHOW : DIV_HIDE; }
			}

			public String HasNotifyMoveAsDiv
			{
				get { return HasImmediateMove || HasRecentMove ? DIV_SHOW : DIV_HIDE; }
			}


            #endregion
            public static void LoadFromIni(ref List<ZoneDetails> zones)
            {
                Hashtable zoneValues = IniFile.LoadAllIniEntriesByIntKey(IniFile.INI_SECTION_ZONES);
               
                ZoneDetails zone;

                m_intervalImmediate = Convert.ToInt16(IniFile.PARAM_GENERIC_INTERVAL_SPLIT[1].Split('-')[0]);
                m_intervalRecent = Convert.ToInt16(IniFile.PARAM_GENERIC_INTERVAL_SPLIT[1].Split('-')[1]);
                m_intervalPast = Convert.ToInt16(IniFile.PARAM_GENERIC_INTERVAL_SPLIT[1].Split('-')[2]);

                foreach (int id in zoneValues.Keys)
                {
                    zone = new ZoneDetails(id, zoneValues[id].ToString());
                    
                    zones.Add(zone);
                }

                zones.Sort(delegate(Metadata.ZoneDetails a1, Metadata.ZoneDetails a2)
                {
                    return a1.ZoneId.CompareTo(a2.ZoneId);
                });
            }

            public void LoadStateFromIni()
            {
                String json = IniFile.LoadIniEntryByKey(IniFile.INI_SECTION_ZONESTATE, ZoneId.ToString());

                if (json != "")
                {
                    ZoneDetails zonestorage = fastJSON.JSON.Instance.ToObject<ZoneDetails>(json);

                    ZoneName = zonestorage.ZoneName;
                    ParentZoneId = zonestorage.ParentZoneId;
                    PowerIndex = zonestorage.PowerIndex;
                    DefaultVolumePercent = zonestorage.DefaultVolumePercent;

                    CameraId = zonestorage.CameraId;
                    if (CameraId != "") HasCamera = true;
                    AlarmZoneId = zonestorage.AlarmZoneId;
                    if (AlarmZoneId != -1) HasMotionSensor = true;
                    AlarmAreaId = zonestorage.AlarmAreaId;

                    OutputDeviceUserSelected = zonestorage.OutputDeviceUserSelected;
                    OutputKeywords = zonestorage.OutputKeywords;
                    //OutputDeviceAutoCompleted = GetOutputDeviceNameAutocompleted(OutputDeviceUserSelected, OutputKeywords);
                    if (!OutputDeviceAutoCompleted().Equals(""))
                    {
                        HasSpeakers = true;
                    }

                    WavedeviceIndex = GetWaveOutDeviceIndex(OutputKeywords);
                    OutputDeviceNameWaveVLC = GetVLCAudioWaveDeviceName(WavedeviceIndex);

                    WakeTime = zonestorage.WakeTime;
                    WakeWeekDay = zonestorage.WakeWeekDay;
                    SleepHourMin = zonestorage.SleepHourMin;

                    DisplayType = zonestorage.DisplayType;
                    DisplayConnection = zonestorage.DisplayConnection;
                    if (DisplayType.Equals(Display.DisplayTypeEnum.LGTV.ToString()))
                    {
                        HasDisplay = true;
                    }
                    //IsArmed = zonestorage.IsArmed;
                }

            }

            public void SaveStateToIni()
            {
                String json = fastJSON.JSON.Instance.ToJSON(this, false);
                IniFile.IniWriteValuetoFinal(IniFile.INI_SECTION_ZONESTATE, ZoneId.ToString(), json);
            }

			

			public bool HasOutputDeviceAvailable()
			{
				return GetOutputDeviceNameAutocompleted(OutputDeviceUserSelected, OutputKeywords) != "";
			}

			public String OutputDeviceAutoCompleted()
			{
				return GetOutputDeviceNameAutocompleted(OutputDeviceUserSelected, OutputKeywords);
			}

            public static String GetOutputDeviceNameAutocompleted(String p_outputDevice, String p_outputKeywords)
            {
				List<String> systemOutputDevices = DShowUtility.SystemDeviceNameList;
                String matchValue;
				String result = "";//"NONE-outdev=" + p_outputDevice + "|keys=" + p_outputKeywords + "|";
                if (p_outputKeywords != null)
                {
                    String[] keys = p_outputKeywords.Split(',');

                    if (p_outputDevice.Equals(IniFile.DEFAULT_AUTO_DEV_NAME))
                    {
                        foreach (String device in systemOutputDevices)
                        {
                            matchValue = "";
                            foreach (String key in keys)
                            {
                                if (device.ToLower().Contains(key.ToLower()))
                                    matchValue = device;
                                else
                                {
                                    matchValue = "";
                                    break;
                                }
                            }
                            if (matchValue != "")
                            {
                                result = matchValue;
                                break;
                            }
                        }
                    }
                    else
                        result = p_outputDevice;
                }

				//MLog.Log(null, "Device user select="+p_outputDevice+" keywords="+p_outputKeywords+" res="+result);
                return result;
            }

            private static int GetWaveOutDeviceIndex(String p_outputKeywords)
            {
				List<DShowUtility.WAVEOUTCAPS> systemWaveOutputDevices;
				systemWaveOutputDevices = DShowUtility.GetDevCapsPlayback();
                int result = -1;
                if (p_outputKeywords != null)
                {
                    String[] keys = p_outputKeywords.Split(new String[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < systemWaveOutputDevices.Count; i++)
                    {
                        result = -1;
                        foreach (String key in keys)
                        {
                            if (!key.Equals("directsound"))
                            {
                                if (systemWaveOutputDevices[i].szPname.ToLower().Contains(key.ToLower()))
                                    result = i;
                                else
                                {
                                    result = -1;
                                    break;
                                }
                            }
                        }
                        if (result != -1)
                            break;
                    }
                }
                return result;
            }

            private static String GetVLCAudioWaveDeviceName(int wavedeviceindex)
            {
                String result;
                DShowUtility.WAVEOUTCAPS wave;
                if (wavedeviceindex != -1)
                {
                    wave = DShowUtility.GetDevCapsPlayback()[wavedeviceindex];
                    result = wave.szPname + " ($" + String.Format("{0:x}", wave.wMid) + ",$" + String.Format("{0:x}", wave.wPid) + ")";
                }
                else
                    result = "no wave device";
                return result;
            }

            public void Close()
            {
                ZoneClose();
                SaveStateToIni();
            }

            public int GetDefaultVolume()
            {
                return GetDefaultVolume(DefaultVolumePercent);
            }

            public static int GetDefaultVolume(int percent)
            {
                String dt = DateTime.Now.ToString(IniFile.DATETIME_DAYHR_FORMAT);
                String[] interval = IniFile.PARAM_SILENCE_SAFE_HOUR_INTERVAL[1].Split('-');
                //reduce volume during silence interval - night
                if (dt.CompareTo(interval[0]) >= 0 || dt.CompareTo(interval[1]) <= 0)
                    percent = 25;
                double val = ((100 - Convert.ToDouble(percent)) / 100);
                return Convert.ToInt16(VolumeLevels.VolumeSilence * val);
            }

			public void ZoneStop()
			{
				Album = null;
				Author = null;
				Year = null;
				Title = null;
				Genre = null;
				SourceURL = null;
				RequirePower = false;
				ZoneState = Metadata.ZoneState.NotStarted;
			}

            public void ZoneClose()
            {
				ZoneStop();
                IsActive = false;
                ActivityType = GlobalCommands.nul;
                ZoneState = Metadata.ZoneState.NotInitialised;
                
            }
        }

		public class MacroEntryCommand
		{
			public GlobalCommands Command;
			public String ZoneName;
			public String ParameterValueList;
			public int DelayMiliSec=0;

			public MacroEntryCommand()
			{}
		}
		public class MacroShortcut
		{
			public String Shortcut;
			public String DeviceName;
			public MacroShortcut() { }
		}

		public class MacroEntry
		{
			public int Id;
			public String RepeatMonth;
			public String RepeatWeekDay;
			public String RepeatTime;
			public List<MacroShortcut> ShortcutList;
			public List<MacroEntryCommand> CommandList;
			public DateTime ExecutedDateTime;
			public MacroEntry()
			{}

			public static void SaveToIni(List<MacroEntry> list)
			{
				String json;
				int line = 0;
				foreach (MacroEntry entry in list)
				{
					json = fastJSON.JSON.Instance.ToJSON(entry, false);
					Utilities.WritePrivateProfileString(IniFile.SCHEDULER_SECTION_MAIN, line.ToString(),
						json, IniFile.CurrentPath() + IniFile.SCHEDULER_FILE);
				}
			}

			public static List<MacroEntry> LoadFromIni()
			{
				String json;
				MacroEntry entry;
				List<MacroEntry> list = new List<MacroEntry>();
				int line=0;
				do
				{
					json = Utilities.IniReadValue(IniFile.SCHEDULER_SECTION_MAIN, 
						line.ToString(), IniFile.CurrentPath()+IniFile.SCHEDULER_FILE);
					if (json != "")
					{
						entry = fastJSON.JSON.Instance.ToObject<MacroEntry>(json);
						entry.ExecutedDateTime = DateTime.MinValue;
						list.Add(entry);
					}
					line++;
				}
				while (json!="");
				MLog.Log(null, "Loaded " + (line-1) +" scheduler events");
				return list;
			}

			public static void AddParams(String parameters, ref Metadata.ValueList vals)
			{
				if (parameters != null && parameters != "")
				{
					String[] atoms, pair;
					atoms = parameters.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

					for (int i = 0; i < atoms.Length; i++)
					{
						pair = atoms[i].Split('=');
						if (pair.Length >= 2)
						{
							vals.Add(pair[0], pair[1]);
						}
						else
							MLog.Log(null, "At AddParams Invalid parameter in " + atoms[i]);
					}
				}
			}
		}
        public class CamAlert
        {
            private static int startIndex = 0;
            public int Index;
            public String CamId;
            public DateTime AlarmTime;
            public String AlarmSource;
            public String CustomMessage;
            public bool WasAcknowledged;
            public bool WasIgnored;
            public int ParentZoneId;

            public CamAlert()
            {
            }

            public CamAlert(String alarmSource, String customMessage, String camId, int parentZoneId, bool isAlertActive)
            {
                AlarmTime = DateTime.Now;
                AlarmSource = alarmSource;
                CustomMessage = customMessage;
                WasAcknowledged = false;
                WasIgnored = !isAlertActive;
                CamId = camId;
                Index = startIndex;
                ParentZoneId = parentZoneId;
                startIndex++;
            }
        }

        public class ServerStatus
        {
            public Boolean IsServerOn = true;
            public ZoneDetails[] ZoneDetails;
            public List<CamAlert> CamAlertList;

            public ServerStatus()
            {}
        }
    }



    public class CmdParams
    {
        public String Name;
        public Object Value;
        public CmdParams(String p_name, Object p_value)
        {
            Name = p_name;
            Value = p_value;
        }
    }

    public class KeyDetail
    {
        public String Key;
        public String Device;
		public String DeviceName;
        public bool IsKeyDown;
        public bool IsKeyUp;
        public KeyDetail(String key, String device, String deviceName, bool isKeyDown, bool isKeyUp)
        {
            this.Key = key;
            this.Device = device;
			this.DeviceName = deviceName;
            this.IsKeyDown = isKeyDown;
            this.IsKeyUp = isKeyUp;
        }
    }


    public class Users
    {
        public String Id;
        public String Name;
        public String Code;
        public Users(String id, String name, String code)
        {
            this.Id = id;
            this.Name = name;
            this.Code = code;
        }
    }

    public class MoodMusic
    {
        public enum LogicalSearchOperatorEnum
        {
            Union,
            Intersect
        }
        //private static int m_index = 0;
        public String Name;
        public String NumericCode;//two numeric chars
        public int Index;
        public List<String> Genres;
        //public List<String> Albums;
        public List<String> Authors;
        public List<int> Ratings;
        public List<int> AgeInWeeks;
        public LogicalSearchOperatorEnum LogicalSearchOperator;
        public Boolean IsGroupByTop;
        public int TopCount;
        public Boolean IsRandom;

        public MoodMusic()
        {
            Genres = new List<string>();
            Authors = new List<string>();
            Ratings = new List<int>();
            AgeInWeeks = new List<int>();
            //Index = m_index++;
        }

        public MoodMusic(int index, String name, String genres_list, String authors_list, String ratings_list, String ageinweeks_list, 
            String logicalOperation, String groupbytop, String topcount, String israndom, String numericcode)
        {
            Genres = new List<string>();
            Authors = new List<string>();
            Ratings = new List<int>();
            AgeInWeeks = new List<int>();
            Index = index;

            Name = name;
            IsGroupByTop = groupbytop.ToLower().Equals("true");
            IsRandom = israndom.ToLower().Equals("true");
            TopCount = Convert.ToInt16(topcount);
            NumericCode = numericcode;

            if (Enum.IsDefined(typeof(LogicalSearchOperatorEnum), logicalOperation))
                LogicalSearchOperator = (LogicalSearchOperatorEnum)Enum.Parse(typeof(LogicalSearchOperatorEnum), logicalOperation);
            else
                LogicalSearchOperator = LogicalSearchOperatorEnum.Union;

            IEnumerator en = genres_list.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries).GetEnumerator();
            while (en.MoveNext())
            {
                Genres.Add(en.Current.ToString());
            }
            /*
            en = albums_list.Split(';').GetEnumerator();
            while (en.MoveNext())
            {
                Albums.Add(en.Current.ToString());
            }*/

            en = authors_list.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries).GetEnumerator();
            while (en.MoveNext())
            {
                Authors.Add(en.Current.ToString());
            }

            en = ratings_list.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries).GetEnumerator();
            while (en.MoveNext())
            {
                Ratings.Add(Convert.ToInt16(en.Current));
            }

            en = ageinweeks_list.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries).GetEnumerator();
            while (en.MoveNext())
            {
                AgeInWeeks.Add(Convert.ToInt16(en.Current));
            }
        }

        public String GenreList()
        {
            StringBuilder sb = new StringBuilder();
            foreach (String val in Genres)
            {
                sb.Append(val);
                sb.Append(";");
            }
            return sb.ToString();
        }

        public String AuthorList()
        {
            StringBuilder sb = new StringBuilder();
            foreach (String val in Authors)
            {
                sb.Append(val);
                sb.Append(";");
            }
            return sb.ToString();
        }

        public String RatingList()
        {
            StringBuilder sb = new StringBuilder();
            foreach (int val in Ratings)
            {
                sb.Append(val);
                sb.Append(";");
            }
            return sb.ToString();
        }

        public String AgeInWeeksList()
        {
            StringBuilder sb = new StringBuilder();
            foreach (int val in AgeInWeeks)
            {
                sb.Append(val);
                sb.Append(";");
            }
            return sb.ToString();
        }

        public static void LoadFromIni(ref List<MoodMusic> list)
        {
            Hashtable values = IniFile.LoadAllIniEntriesByIntKey(IniFile.INI_SECTION_MUSICMOOD);
            MoodMusic mood;

            foreach (String json in values.Values)
            {
                mood = fastJSON.JSON.Instance.ToObject(json) as MoodMusic;
                mood.Genres.Sort(delegate(String a1, String a2) { return a2.CompareTo(a1); });
                mood.Authors.Sort(delegate(String a1, String a2) { return a2.CompareTo(a1); });
                list.Add(mood);
            }
        }

        public static void SaveToIni(List<MoodMusic> list)
        {
            String json;
            int r = 0;
            foreach (MoodMusic mood in list)
            {
                json = fastJSON.JSON.Instance.ToJSON(mood);
                IniFile.IniWriteValuetoTemp(IniFile.INI_SECTION_MUSICMOOD, r.ToString(), json);
                r++;
            }
        }
    }

    public class MusicScheduleEntry
    {
        public int ZoneId;
        public String StartTime;//hh:mm
        public String EndTime;//hh:mm
        public String WeekDays;//MoTuWeThFrSaSu
        public String Mood;

        public MusicScheduleEntry()//for json
        {
        }

        public MusicScheduleEntry(int zoneId, String startTime, String endTime, String weekDays, String mood)
        {
            ZoneId = zoneId;
            StartTime = startTime;
            EndTime = endTime;
            WeekDays = weekDays;
            Mood = mood;
        }

        public static void LoadFromIni(ref List<MusicScheduleEntry> list)
        {
            Hashtable values = IniFile.LoadAllIniEntriesByIntKey(IniFile.INI_SECTION_MUSICSCHEDULE);
            MusicScheduleEntry entry;

            foreach (String json in values.Values)
            {
                entry = fastJSON.JSON.Instance.ToObject(json) as MusicScheduleEntry;
                list.Add(entry);
            }
        }

        public static void SaveToIni(List<MusicScheduleEntry> list)
        {
            String json;
            int r = 0;
            foreach (MusicScheduleEntry entry in list)
            {
                json = fastJSON.JSON.Instance.ToJSON(entry);
                IniFile.IniWriteValuetoTemp(IniFile.INI_SECTION_MUSICSCHEDULE, r.ToString(), json);
                r++;
            }
        }
    }

    public class InputEntry
    {
        public String inputName, inputType, inputInZoneId, inputDeviceName;
        public InputEntry(String inputName, String inputType, String inputInZoneId, String inputDeviceName)
        {
            this.inputName = inputName;
            this.inputType = inputType;
            this.inputInZoneId = inputInZoneId;
            this.inputDeviceName = inputDeviceName;
        }
    }
    public class ControlDevice
    {
        public int ZoneId;
        public String DeviceName;
        public String DisplayName;

        public ControlDevice(int zoneId, String deviceName, String displayName)
        {
            this.ZoneId = zoneId;
            this.DeviceName = deviceName;
            this.DisplayName = displayName;
        }

        public static void LoadFromIni(List<ControlDevice> p_list)
        {
            int zoneId;
            String deviceName, devDisplayName;
            ControlDevice dev;
            foreach (Metadata.ZoneDetails zone in MZPState.Instance.ZoneDetails)
            {
                zoneId = zone.ZoneId;//(int)enumerator.Key;
                int r = 0;
                bool loop = true;

                while (loop)
                {
                    deviceName = IniFile.IniReadValue(IniFile.INI_SECTION_ZONECONTROLDEVICENAME + zoneId, r.ToString());
                    if (deviceName != "")
                    {
                        dev = MZPState.Instance.SystemAvailableControlDevices.Find(x=>x.DeviceName.Equals(deviceName));
                        if (dev != null) devDisplayName = dev.DisplayName; 
                            else devDisplayName = "unknown device";
                        p_list.Add(new ControlDevice(Convert.ToInt16(zoneId), deviceName, devDisplayName));
                    }
                    else loop = false;
                    r++;
                }
            }
        }

        public static void SaveToIni(List<ControlDevice> p_list)
        {
            int r =0, lastzoneid =-1;
            p_list.Sort(delegate (ControlDevice c1, ControlDevice c2) {return c1.ZoneId.CompareTo(c2.ZoneId);});
            foreach (ControlDevice ctrl in p_list)
            {
                if (lastzoneid != ctrl.ZoneId) { r = 0; lastzoneid = ctrl.ZoneId; };
                IniFile.IniWriteValuetoTemp(IniFile.INI_SECTION_ZONECONTROLDEVICENAME + ctrl.ZoneId, r.ToString(), ctrl.DeviceName);
                r++;
            }
        }
    }

    public class Alarm
    {
        public enum EnumScope
        { area, zone, trouble };
        public enum EnumAreaState
        { opened, armed, disarmed, stayarmed, sleeparm, ready, exitdelay, exitdelayfinished, entrydelay, entrydelayfinished, entrydelayend, remotearmed,
        sirenon, sirenonrestore, alarm, alarminmemory, alarmoutofmemory, zonesbypassed, UNKNOWN
        };
        public enum EnumZoneState
        { opened, closed };
        public enum EnumZoneAction
        { new_, restore };

        public int AreaId;

        public EnumAreaState AreaState = EnumAreaState.ready;
        public Boolean IsMonitoringActive = false;
        public DateTime LastAlarmEventDateTime = DateTime.MinValue;
        public DateTime LastAreaStateChange = DateTime.MinValue;
		public bool IsArmed = false;

        public Alarm(int areaid)
        {
            AreaId = areaid;
        }
    }

    public class MZPEvent
    {
        public enum EventSource
        {
            Alarm,
            GUI,
            Cam,
            System,
            Environment,
            Modem,
            Web,
            Keyboard,
            RawInput
        }

        public enum EventType
        {
            Security,
            Functionality
            
        }

        public enum EventImportance
        {
            Critical,
            Informative,
            Error,
            Debug
        }

        public DateTime DateTime;
        public EventSource Source;
        public String Message;
        public EventType TypeEv;
        public EventImportance Importance;
        public Metadata.ZoneDetails ZoneDetails;

        public MZPEvent(DateTime dateTime, EventSource source, String message, EventType type, EventImportance importance, Metadata.ZoneDetails zonedetails)
        {
            DateTime = dateTime;
            Source = source;
            Message = message;
            TypeEv = type;
            Importance = importance;
            ZoneDetails = zonedetails;
        }
    }

    public class NotifyState
    {
        public bool GTalkEnabled = false;
        public bool SMSEnabled = false;
        public bool MobileCallEnabled = false;
    }

    public class Playlist
    {
        public int ZoneId;
        public int UserId;
        public String MediaURL;

        public Playlist(String p_mediaURL)
        {
            MediaURL = p_mediaURL;
        }
    }

    public interface IStorageOnIni
    {
        void LoadFromIni();
        void SaveToIni();
    }


}

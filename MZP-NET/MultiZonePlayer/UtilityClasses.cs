﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using fastJSON;

namespace MultiZonePlayer
{
    /*
     * Interface collection of classes used for mobile client comm 
     */
	

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
		system,
		messenger
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
		dismissalert,
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
		getpicture,
		macro,
		rfxcmd,
		r,//repeat last command
		notifyuser,
		closure,
		closurearm,
		closuredisarm,
		runscript,
		poweron,
		poweroff,
		powertoggle,
		setfield,
		generategraph,
		generatefullgraph
	}
	public enum GlobalParams
	{
		command,
		zoneid,
		sourcezoneid,
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
		singleparamvalue,
		id, iscontactmade,
		name, text, field,
		type,zoneidlist,
		r//random no
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
			String cmdName = vals.GetValue(GlobalParams.command);
			if (Enum.IsDefined(typeof(GlobalCommands), cmdName))
			{
				GlobalCommands apicmd = (GlobalCommands)Enum.Parse(typeof(GlobalCommands), cmdName);
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
			new CommandSyntax(GlobalCommands.remoteadjustdim,	GlobalParams.remoteid, GlobalParams.dimvalue),
			new CommandSyntax(GlobalCommands.rfxcmd,			GlobalParams.action),
			new CommandSyntax(GlobalCommands.powercycle,		GlobalParams.interval),
			new CommandSyntax(GlobalCommands.notifyuser,		GlobalParams.sourcezoneid),
			new CommandSyntax(GlobalCommands.runscript,			GlobalParams.name),
			new CommandSyntax(GlobalCommands.closure,			GlobalParams.id, GlobalParams.iscontactmade),
			//new CommandSyntax(GlobalCommands.poweron,			GlobalParams.zonename),
			//new CommandSyntax(GlobalCommands.poweroff,			GlobalParams.zonename),
			new CommandSyntax(GlobalCommands.powertoggle,		GlobalParams.zoneid),

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
            dismissalert,
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
	}



	public class CommandResult
	{
		public string Command;
		public string User;
		public ResultEnum Result;
		public String ErrorMessage = "";
		public String OutputMessage = "";
		public ServerStatus ServerStatus;
		public ValueList ValueList;

		public CommandResult()
		{
		}

		public CommandResult(ResultEnum p_result, String outputMessage, String errorMessage)
		{
			Result = p_result;
			ErrorMessage = errorMessage;
			OutputMessage = outputMessage;
		}

		public string ValuesToString()
		{
			String result = "";
			if (ValueList != null)
			{
				if (ValueList.Keys != null)
				{
					result += " Keys:";
					foreach (string key in ValueList.Keys)
					{
						result += key + "=" + ValueList.GetValue(key) + ";";
					}
				}
				if (ValueList.IndexValueList != null)
				{
					result += " Vals:";
					foreach (string value in ValueList.IndexValueList)
					{
						result += value + ",";
					}
				}
			}
				 
				
			return result;
		}

		public override String ToString()
		{
			string result = "Out=" + OutputMessage;
			if (ErrorMessage != "") result += " Err=" + ErrorMessage;
			if (ValueList != null) result += " "+ValuesToString();
			return result;
		}
	}

	public class ZoneList
	{
		public static List<ZoneDetails> List(ZoneType value, String operation)
		{
			//List<ZoneDetails> zones;
			return MZPState.Instance.ZoneDetails.FindAll(x => x.Type == value);
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

		public void Set(GlobalParams key, String value)
		{
			int index = Keys.FindIndex(x=>x==key.ToString());
			if (index!=-1)
				Values[index]=value;
			else
				Add(key,value);
		}

		public void Add(String globalparamkey, String value)
		{
			if (Enum.IsDefined(typeof(GlobalParams), globalparamkey))
			{
				GlobalParams gp = (GlobalParams)Enum.Parse(typeof(GlobalParams), globalparamkey);
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

		//
		public bool ContainsIndexValue(String value, bool exactMatch)
		{
			if (exactMatch)
				return IndexValueList.Contains(value);
			else
			{
				foreach (string s in IndexValueList)
				{
					if (value.Contains(s)) return true;
				}
				return false;
			}
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

		public static void ParseStringToValues(string text, string pairSeparator, string keySeparator, ref ValueList vals)
		{
			GlobalParams param;
			String[] pairs= text.Split(new String[]{pairSeparator}, StringSplitOptions.RemoveEmptyEntries);
			String[] keyval;
			String key,val;

			foreach (string pair in pairs)
			{
				keyval = pair.SplitTwo(keySeparator);
				if (keyval.Length>1)
				{
					key = keyval[0];
					val = keyval[1];
				
					if (Enum.IsDefined(typeof (GlobalParams), key))
					{
						param = (GlobalParams) Enum.Parse(typeof (GlobalParams), key);
						vals.Add(param, val);
					}
					else
					{
						MLog.Log(null, "Parsed unknown parameter:" + key);
					}
				}
			}
		}
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

	public class ClosureOpenCloseRelay
	{
		public enum EnumState
		{
			Undefined=-1,
			ContactOpen=0,
			ContactClosed=1
		}

		public enum EnumRelayType
		{
			Undefined,
			NormalOpen,
			NormalClosed,
			Button
		}

		//public string Key;
		public DateTime LastChange;
		private EnumState m_relayState = EnumState.Undefined;
		private EnumState m_relayStateLast = EnumState.Undefined;
		public EnumRelayType RelayType = EnumRelayType.Undefined;
		private bool m_contactMade = false;

		public ClosureOpenCloseRelay()
		{
		}
		public ClosureOpenCloseRelay(bool isRelayContactMade)
		{
			//Key = key;
			RelayContactMade = isRelayContactMade;
		}

		public EnumState RelayState
		{
			get { return m_relayState; }
			//set { m_relayState = value; }
		}

		public bool RelayContactMade
		{
			get { return m_contactMade; }
			set
			{
				m_contactMade = value;
				m_relayState = GetRelayState(m_contactMade);
				LastChange = DateTime.Now;

				if (m_relayState != m_relayStateLast)
				{
					Rules.ExecuteRule(this, "contactmade="+m_relayState);
					m_relayStateLast= m_relayState;
				}
			}
		}

		public EnumState GetRelayState(bool isRelayContactMade)
		{
			switch (RelayType)
			{
				case EnumRelayType.NormalOpen:
				case EnumRelayType.Button:
					return isRelayContactMade ? ClosureOpenCloseRelay.EnumState.ContactClosed : ClosureOpenCloseRelay.EnumState.ContactOpen;
				case EnumRelayType.NormalClosed:
					return isRelayContactMade ? ClosureOpenCloseRelay.EnumState.ContactOpen: ClosureOpenCloseRelay.EnumState.ContactClosed;
				default:
					//MLog.Log(this, "Error, undefined relay type");
					return EnumState.Undefined;
			}
		}

		public void ResetState()
		{
			m_relayState = EnumState.Undefined;
			m_relayStateLast = EnumState.Undefined;
		}
	}

	public enum PowerType
	{
		Denkovi,
		Numato,
		None
	}

	public enum ZoneType
	{
		Undefined,
		Space,
		Component,
		Heat,
		Light,
		Alarm,
		Closure
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
		public List<String> AllowUserList;
		public DateTime ExecutedDateTime;
		public MacroEntry()
		{}

		public String Description
		{
			get
			{
				if (ShortcutList != null && ShortcutList.Count > 0)
					return ShortcutList[0].Shortcut;
				else
					if (CommandList != null && CommandList.Count > 0)
						return CommandList[0].Command.ToString();
					else
						return Id.ToString();
			}
		}
		public String Details
		{
			get
			{
				String result="";
				if (RepeatMonth != "" || RepeatWeekDay != "" || RepeatTime != "")
					result = "Schedule Month="+RepeatMonth + " WeekDay="+RepeatWeekDay+ " Time="+RepeatTime;
				
				return result;
			}
		}
		public static void SaveToIni(List<MacroEntry> list)
		{
			String json;
			int line = 0;
			foreach (MacroEntry entry in list)
			{
				fastJSON.JSONParameters param = new fastJSON.JSONParameters(); param.UseExtensions = false;
				json = JSON.Instance.ToJSON(entry, param);
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
					entry = JSON.Instance.ToObject<MacroEntry>(json);
					entry.ExecutedDateTime = DateTime.MinValue;
					entry.Id = line;
					list.Add(entry);
				}
				line++;
			}
			while (json!="");
			MLog.Log(null, "Loaded " + (line-1) +" scheduler events");
			return list;
		}

		public static void AddParams(String parameters, ref ValueList vals)
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
					{
						if (i == 0)
							vals.Add(GlobalParams.singleparamvalue, atoms[i]);
						else
							MLog.Log(null, "At AddParams Invalid parameter in " + atoms[i]);
					}
				}
			}
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

	public enum MoveTypeEnum
	{
		Camera,
		Closure,
		Command,
		Alarm
	}



	

	public enum ResultEnum
	{
		ERR,
		OK
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

	public class Constants
	{
		public const string CAPABILITY_TEMP = "temp", CAPABILITY_HUM = "hum", EVENT_TYPE_POWER="power";
		public const string EVENT_TYPE_CAMALERT = "cam", EVENT_TYPE_SENSORALERT = "sensor", EVENT_TYPE_CLOSURE = "closure";
	}

	public class ZoneDetails
	{
		public int ZoneId = 0;
		[Description("Edit")] public String Description;
		public Boolean IsActive = false;
		//public int MinutesUntilSleep = -1;
		[Description("Edit")] public String SleepHourMin = "";//format HH:MM
		public String ZoneName;
		private ZoneState m_zoneState, m_zoneStateLast=MultiZonePlayer.ZoneState.Undefined;
		public int ParentZoneId = -1;
		public int PowerIndex = -1;
		public String PowerType;
		public int PowerOnDelay;
		public String WakeTime = "";
		public String WakeWeekDay = "";
		public String CameraId = "";
		public Boolean HasSpeakers = false;
		[Description("Edit")] public int DefaultVolumePercent;
		public String OutputKeywords;
		public String OutputDeviceUserSelected;
		public ZoneType Type = ZoneType.Undefined;
		public String OutputDeviceNameWaveVLC;
		public int WavedeviceIndex;
		public Boolean HasCamera = false;
		public Boolean IsArmed = false;
		public int AlarmZoneId = -1;
		public int AlarmAreaId=-1;
		public Boolean HasMotionSensor = false;
		public Boolean HasMicrophone = false;
		public Boolean HasDisplay = false;
		public Boolean HasVideoPlayer = false;
		public String DisplayConnection = "";
		public String DisplayType = "";
		public Boolean RequirePower = false;
		public Boolean IsClosureArmed = false;
		[Description("Edit")] public String NearbyZonesIdList = "";//zone id list separated by ;
		[Description("Edit")] public string ClosureIdList = "";//separated by ; iopin=2 / for gpio
		//public EnumRelayType ClosureRelayType = EnumRelayType.Undefined;
		public ClosureOpenCloseRelay ClosureOpenCloseRelay;
		public ulong ClosureCounts = 0;
		[Description("Edit")] public String TemperatureDeviceId;

		[Description("Edit")] public double TemperatureMaxAlarm = 1000;
		[Description("Edit")] public double TemperatureMinAlarm = -1000;
		[Description("Edit")] public double TemperatureTarget = -1000;
		public String CronSchedule="";
		[Description("Edit")] public String Color;

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
		public LastFmMeta Meta;

		public Boolean CameraAlertActive = true;
		private Boolean m_movementAlert = false, m_movementAlertLast = false;
			
		public DateTime LastAlarmMovementDateTime = DateTime.MinValue;
		public DateTime LastCamAlertDateTime = DateTime.Now;
		public DateTime LastLocalCommandDateTime = DateTime.MinValue;
		public DateTime LastClosureEventDateTime = DateTime.MinValue;
		public ZoneNotifyState NotifyZoneEventTriggered = ZoneNotifyState.Closed;
		public DateTime LastNotifyZoneEventTriggered;
		protected const double DEFAULT_TEMP_HUM = -1000;
		
		protected double m_temperature=DEFAULT_TEMP_HUM, m_humidity=DEFAULT_TEMP_HUM;
		protected double m_temperatureLast = DEFAULT_TEMP_HUM, m_humidityLast = DEFAULT_TEMP_HUM;
		protected DateTime m_lastTempSet = DateTime.MinValue, m_lastHumSet = DateTime.MinValue;
		// not serializable, hidden from json
		protected static int m_intervalImmediate, m_intervalRecent, m_intervalPast;
		

		public ZoneDetails()
		{
			ClosureOpenCloseRelay = new ClosureOpenCloseRelay(false);
		}

		public ZoneDetails(int p_zoneId, String p_zoneName)
		{
			ZoneId = p_zoneId;
			ZoneName = p_zoneName;
			ZoneState = MultiZonePlayer.ZoneState.NotInitialised;
			ActivityType = GlobalCommands.nul;
			LoadStateFromIni();
		}

		public override string ToString()
		{
			return "ID="+ZoneId+";Name="+ZoneName;
		}
		#region getters

		public ZoneState ZoneState
		{
			get { return m_zoneState; }
			set { m_zoneState = value;
				if (m_zoneState != m_zoneStateLast)
				{
					Rules.ExecuteRule(this,"zonestate="+m_zoneState);
					m_zoneStateLast = m_zoneState;
				}
			}
		}

		public Boolean MovementAlert
		{
			get { return m_movementAlert; }
			set { m_movementAlert = value;
				if (m_movementAlert != m_movementAlertLast)
				{
					Rules.ExecuteRule(this,"movement="+m_movementAlert);
					m_movementAlertLast = m_movementAlert;
				}
			}
		}
		public String SummaryStatus
		{
			get {
				String val = "#"+ ZoneId +" "+ ZoneName + (IsActive?" Active":"") 
				             + (ActivityType.Equals(GlobalCommands.nul)?"":" "+ActivityType.ToString()) 
				             + (IsArmed?" Armed ":" ") 
				             + Utilities.DurationAsTimeSpan(LastMovementAge)
				             + (HasImmediateMove? " ImmediateMove ":"")
				             + (HasRecentMove? " RecentMove ":"")
				             + (ClosureOpenCloseRelay.RelayType!=ClosureOpenCloseRelay.EnumRelayType.Undefined? " " + ClosureState +"@ "+LastClosureEventDateTime :" ")
				             + Title
				             + (Temperature!=DEFAULT_TEMP_HUM?" " + Temperature + "C":"")
				             + (Humidity != DEFAULT_TEMP_HUM?" " + Humidity+"%":"")
				             + (IsPowerOn?" PowerIsOn":"");
				return val;
			}
		}
		public DateTime LastMovementDate
		{
			get
			{
				List<DateTime> dates = new List<DateTime>();
				dates.Add(LastAlarmMovementDateTime);
				dates.Add(LastCamAlertDateTime);
				dates.Add(LastClosureEventDateTime);
				dates = dates.OrderByDescending(x=>x.Ticks).ToList();
				return dates[0];
			}
		}

		public TimeSpan LastMovementAge
		{
			get { return DateTime.Now.Subtract(LastMovementDate); }
		}

		public string LastMovementAgeAsTimeSpan
		{
			get { return Utilities.DurationAsTimeSpan(LastMovementAge); }
		}

		public double LastLocalCommandAgeInSeconds
		{
			get { return Math.Round(DateTime.Now.Subtract(LastLocalCommandDateTime).TotalSeconds); }
		}
		public Boolean HasImmediateMove
		{
			get
			{
				double span = DateTime.Now.Subtract(LastMovementDate).TotalMinutes;
				return (span <= m_intervalImmediate);
			}
		}

		public Boolean HasImmediateCamMove
		{
			get
			{
				double span = DateTime.Now.Subtract(LastCamAlertDateTime).TotalMinutes;
				return (span <= m_intervalImmediate);
			}
		}
		public Boolean HasImmediateAlarmMove
		{
			get
			{
				double span = DateTime.Now.Subtract(LastAlarmMovementDateTime).TotalMinutes;
				return (span <= m_intervalImmediate);
			}
		}
		public Boolean HasImmediateClosureMove
		{
			get
			{
				double span = DateTime.Now.Subtract(LastClosureEventDateTime).TotalMinutes;
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

		public Boolean HasAction
		{
			get
			{
				bool action = HasImmediateMove || IsActive || IsPowerOn || HasAlarm;
				ZoneDetails child;
				if (!action)
					for (int i = 0; i < ChildZonesCount; i++)
					{
						child = ChildZone(i);
						action = child.HasImmediateMove || child.IsActive || child.IsPowerOn || child.HasAlarm;
						if (action)
							break;
					}
				return action;
			}
		}

		public Boolean HasAlarm
		{
			get
			{
				return IsArmed && HasImmediateMove;
			}
		}

		public Boolean HasTemperatureAlarm
		{
			get
			{
				return (m_temperature>TemperatureMaxAlarm) || (m_temperature<TemperatureMinAlarm);
			}
		}

		public Boolean IsPowerOn
		{
			get { return MZPState.Instance.PowerControlIsOn(ZoneId); }
		}
		public Boolean HasPowerCapabilities
		{
			get
			{
				return PowerIndex != -1 && PowerType != "";
			}
		}
		public Boolean HasClosures
		{
			get { return ClosureIdList != "";}
		}
		public Boolean HasTemperatureSensor
		{
			get { 
				RFXDeviceDefinition.RFXDevice device = RFXDeviceDefinition.GetDevice(ZoneId);
				return (TemperatureDeviceId != "" || 
				        (device!=null && device.DeviceType == RFXDeviceDefinition.DeviceTypeEnum.temp_hum));
			}
		}
		public Boolean HasHumiditySensor
		{
			get
			{
				RFXDeviceDefinition.RFXDevice device = RFXDeviceDefinition.GetDevice(ZoneId);
				return (device != null && device.DeviceType == RFXDeviceDefinition.DeviceTypeEnum.temp_hum);
			}
		}
		public Boolean ContainsDisplay
		{
			get { return MZPState.Instance.ZoneDetails.Find(x=>x.ParentZoneId==ZoneId && x.HasDisplay)!=null
			             || HasDisplay; }
		}

		public String ClosureState
		{
			get {
				return ClosureOpenCloseRelay.RelayState.ToString();
			}
		}

		public int MacroCount
		{
			get { 
				List<MacroEntry> macros = MZPState.Instance.GetZoneMacros(ZoneId);
				return macros != null ? macros.Count : 0;
			}
		}
		
		public MacroEntry Macro(int macroIndex)
		{
			List<MacroEntry> macros = MZPState.Instance.GetZoneMacros(ZoneId);
			if (macros != null && macroIndex < macros.Count)
				return macros[macroIndex];
			else
				return null;
		}

		public int ChildZonesCount
		{
			get
			{
				List<ZoneDetails> childs = MZPState.Instance.ZoneDetails.FindAll(x => x.ParentZoneId == ZoneId);
				return childs != null ? childs.Count : 0;
			}
		}

		public ZoneDetails ChildZone(int index)
		{
			List<ZoneDetails> childs = MZPState.Instance.ZoneDetails.FindAll(x => x.ParentZoneId == ZoneId);
			return childs != null && index < childs.Count ? childs[index] : null;
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
		
		public Boolean IsClosureActivated
		{
			get {
				return ClosureOpenCloseRelay.RelayContactMade;
			}
		}
	
		public Boolean HasNotifyMove
		{
			get { return HasImmediateMove || HasRecentMove; }
		}
		
		public double Temperature
		{
			get { return m_temperature; }//return Math.Round(m_temperature, 2).ToString(); }
			set
			{
				m_temperature = value;
				m_lastTempSet = DateTime.Now;

				if (Temperature > TemperatureMaxAlarm)
				{
					Alert.CreateAlert("Max temperature [" + TemperatureMaxAlarm + "] exceeded on zone "
						+ ZoneName + ", temp is " + Temperature, this, false,
						Alert.NotificationFlags.NotifyUserAfterXHours,1);
				}
				else
					if (Temperature < TemperatureMinAlarm)
					{
						Alert.CreateAlert("Min temperature [" + TemperatureMinAlarm + "] exceeded on zone "
							+ ZoneName + ", temp is " + Temperature, this, false,
							Alert.NotificationFlags.NotifyUserAfterXHours, 1);
					}

				if (m_temperature != m_temperatureLast)
				{
					Utilities.AppendToCsvFile(IniFile.CSV_TEMPERATURE_HUMIDITY, ",", ZoneName, 
						Constants.CAPABILITY_TEMP, DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT), Temperature.ToString(), ZoneId.ToString());
					Rules.ExecuteRule(this, "temp=" + m_temperature);
					m_temperatureLast = m_temperature;
				}
			}
		}

		public double TemperatureTargetTreshhold
		{
			get { return TemperatureTarget + 0.2; }
		}
			
		public TimeSpan TemperatureAge
		{
			get { return DateTime.Now.Subtract(m_lastTempSet); }
		}

		public String TemperatureAgeAsTimeSpan
		{
			get { return Utilities.DurationAsTimeSpan(TemperatureAge); }
		}

		public double Humidity
		{
			get
			{	return m_humidity;}
			set
			{
				m_humidity = value;
				m_lastHumSet = DateTime.Now;

				if (m_humidity != m_humidityLast)
				{
					Utilities.AppendToCsvFile(IniFile.CSV_TEMPERATURE_HUMIDITY, ",", ZoneName, 
						Constants.CAPABILITY_HUM, DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT), Humidity.ToString(), ZoneId.ToString());
					Rules.ExecuteRule(this,"humid="+m_humidity);
					m_humidityLast = m_humidity;
				}
			}
		}

		public String HumidityAgeInMinutes
		{
			get { return Math.Round(DateTime.Now.Subtract(m_lastHumSet).TotalMinutes).ToString(); }
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

			zones.Sort(delegate(ZoneDetails a1, ZoneDetails a2)
			{
				return a1.ZoneId.CompareTo(a2.ZoneId);
			});
		}

		public void LoadStateFromIni()
		{
			String json = IniFile.LoadIniEntryByKey(IniFile.INI_SECTION_ZONESTATE, ZoneId.ToString());
			try
			{
				if (json != "")
				{
					ZoneDetails zonestorage = JSON.Instance.ToObject<ZoneDetails>(json);
					Description = zonestorage.Description;
					ZoneName = zonestorage.ZoneName;
					ParentZoneId = zonestorage.ParentZoneId;
					PowerIndex = zonestorage.PowerIndex;
					PowerOnDelay = zonestorage.PowerOnDelay;
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
					else
						if (DisplayType.Equals(Display.DisplayTypeEnum.XBMC.ToString()))
							HasVideoPlayer = true;

					if (zonestorage.ClosureOpenCloseRelay != null)
					{
						ClosureOpenCloseRelay = zonestorage.ClosureOpenCloseRelay;
						ClosureOpenCloseRelay.RelayType = zonestorage.ClosureOpenCloseRelay.RelayType;
					}
					else
					{
						ClosureOpenCloseRelay = new ClosureOpenCloseRelay(false);
						ClosureOpenCloseRelay.RelayType = ClosureOpenCloseRelay.EnumRelayType.Undefined;
					}

					ClosureIdList = zonestorage.ClosureIdList.Trim();
					ClosureCounts = zonestorage.ClosureCounts;
					if (ClosureOpenCloseRelay.RelayType == ClosureOpenCloseRelay.EnumRelayType.NormalOpen)
						IsClosureArmed = true;
					NearbyZonesIdList = zonestorage.NearbyZonesIdList;
					if (NearbyZonesIdList.Length > 0 && NearbyZonesIdList[NearbyZonesIdList.Length - 1] != ';')
						NearbyZonesIdList += ";";
					TemperatureDeviceId = zonestorage.TemperatureDeviceId;
					PowerType = zonestorage.PowerType;
					Type = zonestorage.Type;
					CronSchedule = zonestorage.CronSchedule;
					TemperatureTarget = zonestorage.TemperatureTarget;
					TemperatureMaxAlarm = zonestorage.TemperatureMaxAlarm;
					TemperatureMinAlarm = zonestorage.TemperatureMinAlarm;
					Color = zonestorage.Color;
					//Temperature = "1";
				}
			}
			catch (Exception ex)
			{
				MLog.Log(ex, "Unable to load zone");
				throw new Exception("ZoneLoad Exception",ex);
			}

		}

		public void SaveStateToIni()
		{
			MLog.Log(this, "Saving state to ini zone="+ZoneName);
			//remove fields that generate serialisation problems
			this.Meta = null;
			fastJSON.JSONParameters param = new fastJSON.JSONParameters(); param.UseExtensions = false;
			String json = JSON.Instance.ToJSON(this, param);
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
		public int GetDefaultAlarmVolume()
		{
			return GetDefaultVolume(DefaultVolumePercent / 2);
		}
		public int GetDefaultNotifyUserVolume()
		{
			int defvol = GetDefaultVolume(DefaultVolumePercent);
			defvol = defvol - defvol * Convert.ToInt16(IniFile.PARAM_NOTIFY_VOLUME_INCREASE[1]) / 100;
			return defvol;
		}

		private static int GetDefaultVolume(int percent)
		{
			String dt = DateTime.Now.ToString(IniFile.DATETIME_DAYHR_FORMAT);
			String[] interval = IniFile.PARAM_SILENCE_SAFE_HOUR_INTERVAL[1].Split('-');
			//reduce volume during silence interval - night
			if (dt.CompareTo(interval[0]) >= 0 || dt.CompareTo(interval[1]) <= 0)
				percent = Convert.ToInt16(IniFile.PARAM_SILENCE_SAFE_VOLUME[1]);
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
			ZoneState = ZoneState.NotStarted;
		}

		public void ZoneClose()
		{
			ZoneStop();
			IsActive = false;
			ActivityType = GlobalCommands.nul;
			ZoneState = ZoneState.NotInitialised;
                
		}

		public Boolean IsNearbyZone(int zoneId)
		{
			return NearbyZonesIdList.Contains(zoneId.ToString()+";");
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



    public class User
    {
		private static List<User> m_userList = new List<User>();
		public static List<User> UserList {
			get { return User.m_userList; }
		}
        public int Id;
        public String Name;
        public String Code;
		public String PhoneBTAddress;
		public String Email;
		public String MobileNumber;
		public String WifiMACAddress;
		public UserRole Role;
		public DateTime NearbyPresentSince;
		public UserPresence.PresenceType NearbyPresenceType;
        
		public enum UserRole {
			Owner,
			Family,
			Friend,
			Public
		}

		public User(){
		}

		public User(int id, String name, String code) {
            this.Id = id;
            this.Name = name;
            this.Code = code;
        }
		
		public static User GetUser(int id){
			return m_userList.Find(x=>x.Id==id);
		}
		public static User GetUserByBT(String BTAddress) {
			return m_userList.Find(x => x.PhoneBTAddress == BTAddress);
		}
		public static User GetUserByWifi(String WifiAddress) {
			return m_userList.Find(x => x.WifiMACAddress == WifiAddress);
		}
		public static void Add(User user){
			m_userList.Add(user);
		}
		public static void LoadFromIni()
		{
			Hashtable values = IniFile.LoadAllIniEntriesByIntKey(IniFile.INI_SECTION_USERS);
            User user;

			try	{
				foreach (String json in values.Values)	{
					user = fastJSON.JSON.Instance.ToObject<User>(json);
					Add(user);
				}
			}
			catch (Exception ex)
			{
				MLog.Log(ex, "Error loading User");
				throw new Exception("Error load User", ex);
			}
		}

		public static void SaveToIni()
		{
			String json;
			int r = 0;
			foreach (User user in m_userList) {
				json = fastJSON.JSON.Instance.ToJSON(user);
				IniFile.IniWriteValuetoTemp(IniFile.INI_SECTION_USERS, r.ToString(), json);
				r++;
			}
		}
    }

	public class UserPresence{
		private static List<UserPresence> m_presenceList = new List<UserPresence>();
		public User User;
		public DateTime LastBTActiveDate, LastWifiActiveDate;
		public DateTime LastMessengerActiveDate;
		public PresenceType Type;
		public int BTLostContactCount = 0, WifiLostContactCount = 0;
		private static List<Bluetooth.Device> m_lastBTDeviceList = new List<Bluetooth.Device>();
		private static List<String> m_lastWifiAddressList = new List<string>();
		public enum PresenceType {
			Bluetooth,
			Messenger,
			Wifi,
			Internet
		}

		public class OpenWrtWifiAssociations {
			public String ifname, encryption, bssid, mode;
			public int quality, noise;
			public String ssid, link, id;
			public int txpoweroff;
			public WifiDevice device;
			public String country;
			public double bitrate;
			public int txpower;
			public String name;
			public int channel;
			public AssocList[] assoclist;
			public int signal;
			public Boolean up;
			public float frequency;

			public class WifiDevice {
				public String device, name, up;
				public WifiDevice() {
				}
			}
			public class AssocList {
				public String Address;
				public Boolean rx_short_gi;
				public int noise, rx_mcs;
				public Boolean tx_40mhz, rx_40mhz;
				public int tx_rate, tx_packets;
				public Boolean tx_short_gi;
				public int rx_packets, tx_mcs, inactive, rx_rate, signal;
				public AssocList() {
				}
			}
		}

		public static List<UserPresence> PresenceList {
			get { return m_presenceList; }
		}
		public static List<User> UserIsNearList {
			get { return m_presenceList.FindAll(x => x.Type == UserPresence.PresenceType.Bluetooth || x.Type==UserPresence.PresenceType.Wifi)
				.Select(x => x.User).Distinct().ToList(); }
		}
		public static String UserIsNearSummary {
			get{
				String result="";
				foreach (User user in UserIsNearList) {
					result += user.Name + ",";
				}
				return result;
			}
		}
		public UserPresence(User user, PresenceType presenceType){
			User = user;
			switch (presenceType) {
				case PresenceType.Bluetooth:
					LastBTActiveDate = DateTime.Now;
					break;
				case PresenceType.Wifi:
					LastWifiActiveDate = DateTime.Now;
					break;
			}
			Type = presenceType;
		}
		public static void AddPresence(User user, PresenceType type){
			UserPresence up = m_presenceList.Find(x => x.User == user && x.Type == type);
			if (up == null) {
				switch (type)
				{
					case PresenceType.Bluetooth:
						m_presenceList.Add(new UserPresence(user, PresenceType.Bluetooth));
						break;
					case PresenceType.Wifi:
						m_presenceList.Add(new UserPresence(user, PresenceType.Wifi));
						break;
				}
				user.NearbyPresentSince = DateTime.Now;
				user.NearbyPresenceType = type;
				MLog.Log(null, "NEW "+type+" DEVICE FOUND User: " + user.Name);
				
			}
		}
		//returns true if presence was removed/user left
		public static bool RemovePresence(User user, PresenceType type) {
			Boolean left = false; 
			int lostCount = 0;
			UserPresence up = m_presenceList.Find(x => x.User == user && x.Type == type);
			MLog.Assert(up != null, "cannot remove null device " + type);

			switch (type) {
				case PresenceType.Bluetooth:
					up.BTLostContactCount++;
					lostCount = up.BTLostContactCount;
					break;
				case PresenceType.Wifi:
					up.WifiLostContactCount++;
					lostCount = up.WifiLostContactCount;
					break;
			}
			if (lostCount > 2) {
				m_presenceList.Remove(up);
				if (user.NearbyPresenceType == type) {
					user.NearbyPresentSince = DateTime.MaxValue;
				}
				left = true;
			}
			if (left)
				MLog.Log(null, "DEVICE "+type+"LEFT user: " + user.Name);
			else
				MLog.Log(null, "DEVICE "+type+" lost contact count= "+lostCount+" user=" + user.Name);

			return left;
		}
		public static void UpdatePresence(User user, PresenceType type) {
			UserPresence up = m_presenceList.Find(x => x.User == user && x.Type == type);
			MLog.Assert(up != null, "presence cannot be null at update " + type);
			switch (type) {
				case PresenceType.Bluetooth:
					up.LastBTActiveDate = DateTime.Now;
					up.BTLostContactCount = 0;
					break;
				case PresenceType.Wifi:
					up.LastWifiActiveDate = DateTime.Now;
					up.WifiLostContactCount = 0;
					break;
			}
			
		}

		public static void CheckBluetooth() {
			User user;
			List<Bluetooth.Device> currentList, newList, leftList, existingList;
			try {
				currentList = Bluetooth.DiscoverDevices();
				newList = currentList.Except(m_lastBTDeviceList).ToList();
				leftList = m_lastBTDeviceList.Except(currentList).ToList();
				existingList = m_lastBTDeviceList.Intersect(currentList).ToList();

				foreach (Bluetooth.Device dev in newList) {
					user = User.GetUserByBT(dev.Address.ToString());
					if (user != null)
						UserPresence.AddPresence(user, PresenceType.Bluetooth);
					else
						MLog.Log(null, "Unknown user with Bluetooth device=" + dev.DeviceName + " Addr=" + dev.Address);
					m_lastBTDeviceList.Add(dev);
				}
				foreach (Bluetooth.Device dev in leftList) {
					user = User.GetUserByBT(dev.Address.ToString());
					if (user != null && UserPresence.RemovePresence(User.GetUserByBT(dev.Address.ToString()),PresenceType.Bluetooth))
						m_lastBTDeviceList.Remove(dev);
				}
				foreach (Bluetooth.Device dev in existingList) {
					user = User.GetUserByBT(dev.Address.ToString());
					if (user != null)
						UserPresence.UpdatePresence(User.GetUserByBT(dev.Address.ToString()), PresenceType.Bluetooth);
				}
			}
			catch (Exception ex) {
				MLog.Log(null, "Check Bluetooth error " + ex.Message);
			}
		}

		public static void CheckWifi() {
			User user;
			WebPostRequest post = new WebPostRequest(IniFile.PARAM_ROUTER_HOST[1] + IniFile.PARAM_ROUTER_ACTIVE_WIFI_CLIENTS_URL[1], null);
			post.Add("username", IniFile.PARAM_ROUTER_USER_NAME[1]);
			post.Add("password", IniFile.PARAM_ROUTER_USER_PASS[1]);
			String json = post.GetResponse();
			/*UserPresence.OpenWrtWifiAssociations[] wrt = fastJSON.JSON.Instance.ToObject<UserPresence.OpenWrtWifiAssociations[]>(json);
			if (wrt[0].assoclist != null) {
				foreach (UserPresence.OpenWrtWifiAssociations.AssocList assoc in wrt[0].assoclist){
					user = User.GetUserByWifi(assoc.Address);
				}
			}*/
			String regex = "([0-9A-F]{2}[:-]){5}([0-9A-F]{2})";
			try {
				System.Text.RegularExpressions.MatchCollection match = System.Text.RegularExpressions.Regex.Matches(json, regex);
				List<String> currentList, newList, leftList, existingList;
				currentList = new List<string>();
				if (match.Count > 0) {
					foreach (System.Text.RegularExpressions.Match mat in match) {
						currentList.Add(mat.Groups[0].Value);
					}
				}
				newList = currentList.Except(m_lastWifiAddressList).ToList();
				leftList = m_lastWifiAddressList.Except(currentList).ToList();
				existingList = m_lastWifiAddressList.Intersect(currentList).ToList();

				foreach (String dev in newList) {
					user = User.GetUserByWifi(dev);
					if (user != null)
						UserPresence.AddPresence(user, PresenceType.Wifi);
					else
						MLog.Log(null, "Unknown user with Wifi addr=" + dev);
					m_lastWifiAddressList.Add(dev);
				}
				foreach (String dev in leftList) {
					user = User.GetUserByWifi(dev);
					if (user != null && UserPresence.RemovePresence(User.GetUserByWifi(dev),PresenceType.Wifi))
						m_lastWifiAddressList.Remove(dev);
				}
				foreach (String dev in existingList) {
					user = User.GetUserByBT(dev);
					if (user != null)
						UserPresence.UpdatePresence(User.GetUserByWifi(dev), PresenceType.Wifi);
				}
			}
			catch (Exception ex) {
				MLog.Log(null, "Check Wifi error " + ex.Message);
			}
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

			try
			{
				foreach (String json in values.Values)
				{
					mood = fastJSON.JSON.Instance.ToObject<MoodMusic>(json);
					mood.Genres.Sort(delegate(String a1, String a2) { return a2.CompareTo(a1); });
					mood.Authors.Sort(delegate(String a1, String a2) { return a2.CompareTo(a1); });
					list.Add(mood);
				}
			}
			catch (Exception ex)
			{
				MLog.Log(ex, "Error loading Moods");
				throw new Exception("Error load moods", ex);
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
				entry = fastJSON.JSON.Instance.ToObject<MusicScheduleEntry>(json);
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

	public class RFXDeviceDefinition
	{
		public enum DeviceTypeEnum
		{
			temp_hum,
			lighting1,
			UKNOWN
		}

		public enum DeviceAttributes
		{
			temperature,
			humidity
		}

		public static List<RFXDevice> DeviceList;

		public class RFXDevice
		{
			public int EntryId;
			public int ZoneId=-1;
			public string DeviceId;//DDDD
			public DeviceTypeEnum DeviceType;
			public string DeviceDefinition;// = "OregonSensor THGN132N-01/THGR122N-01";
			public string DeviceName;
			public string ResponseFormatPattern;//="DDDDDD-QQ-IIII-XX-TT-HH-XX-S-B";
			public string FieldMap;// = "D=DeviceType;Q=Sequence;I=ID";
			public string ResponseTypeMap;//D=String;Q=Hex;I=
			public int ResponseLength;
			public List<FieldValue> FieldValues;

			private class Fields
			{
				public string Key;
				public string Name;
				public int StartPos;
				public int Length;
			};

			public class FieldValue
			{
				public string Name;
				public string Value;
				public FieldValue()
				{
				}
			}
			private List<Fields> FieldDef;

			public RFXDevice()
			{
			}

			public void PopulateFieldDefs()
			{
				FieldDef = new List<Fields>();
				Fields field;

				string[] fieldPair=FieldMap.Split(';');
				string[] attrib;
				string responsepattern = ResponseFormatPattern.Replace("-", "").ToLower();
				ResponseLength = responsepattern.Length;
				foreach (string s in fieldPair)
				{
					attrib = s.Split('=');
					if (attrib.Length>1)
					{
						field = new Fields();
						field.Key = attrib[0].ToLower();
						field.Name = attrib[1].ToLower();
						field.StartPos = responsepattern.IndexOf(field.Key);
						field.Length = responsepattern.LastIndexOf(field.Key) - field.StartPos + 1;
						FieldDef.Add(field);
					}
				}
			}

			public void GetFieldValues(ref string response)
			{
				FieldValues = new List<FieldValue>();
				foreach(Fields f in FieldDef)
				{
					FieldValue fv = new FieldValue();
					fv.Name = f.Name;
					if (f.StartPos + f.Length > response.Length)
					{
						MLog.Log(this, "Error string boundary rfx get values");
					}
					else
					{
						fv.Value = response.Substring(f.StartPos, f.Length);
						fv.Value = int.Parse(fv.Value, System.Globalization.NumberStyles.HexNumber).ToString();
						FieldValues.Add(fv);
					}
				}
				if (ResponseLength < response.Length)
					response = response.Substring(ResponseLength);
				else
					response = "";
			}

			/*public string GetCommandString(params string[] nameValuePair)
			{
				string commandString = ResponseFormatPattern.Replace("-", "");
				string result = "", name, value,key;
				string[] atoms;
				foreach (string s in nameValuePair)
				{
					atoms = s.Split('=');
					name = atoms[0].ToLower();
					value = atoms[1].ToLower();
					
					key = FieldDef.Find(x=>x.Name==name).Key;
					if (key != null)
					{
						
					}
				}
				return result;
			}*/
			public string DisplayValues()
			{
				String result = DeviceName;
				foreach (FieldValue f in FieldValues)
				{
					result += "; " + f.Name + "=" + f.Value;
				}
				return result;
			}
		}
		
		public static RFXDevice GetDevice(ref string response)
		{
			string resp = response;
			RFXDevice result = DeviceList.Find(x => resp.Contains(x.DeviceId));
			if (result != null)
				result.GetFieldValues(ref response);
			else
			{
				MLog.Log(null, "Unknown RFX response:" + response);
				response = "";
			}
			return result;
		}

		public static RFXDevice GetDevice(string deviceName)
		{
			RFXDevice result = DeviceList.Find(x => x.DeviceName.Contains(deviceName));
			if (result != null)
				return result;
			else
			{
				MLog.Log(null, "Cannot find rfxdev " + deviceName);
				return null;
			}
		}

		public static RFXDevice GetDevice(int zoneId)
		{
			RFXDevice result = DeviceList.Find(x => x.ZoneId==(zoneId));
			if (result != null)
				return result;
			else
			{
				//MLog.Log(null, "Cannot find rfxdev for zoneid= " + zoneId);
				return null;
			}
		}

		public static void LoadFromIni()
		{
			String json;
			RFXDevice entry;
			List<RFXDevice> list = new List<RFXDevice>();
			int line = 0;
			do
			{
				json = Utilities.IniReadValue(IniFile.SCHEDULER_SECTION_RFX,
					line.ToString(), IniFile.CurrentPath() + IniFile.SCHEDULER_FILE);
				if (json != "")
				{
					entry = fastJSON.JSON.Instance.ToObject<RFXDevice>(json);
					entry.EntryId = line;
					entry.PopulateFieldDefs();
					list.Add(entry);
				}
				line++;
			}
			while (json != "");
			MLog.Log(null, "Loaded " + (line - 1) + " scheduler events");
			DeviceList = list;
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
			p_list.Clear();
			foreach (ZoneDetails zone in MZPState.Instance.ZoneDetails)
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
		private EnumAreaState m_areaState = EnumAreaState.ready, m_areaStateLast = EnumAreaState.ready;
        public Boolean IsMonitoringActive = false;
        public DateTime LastAlarmEventDateTime = DateTime.MinValue;
        public DateTime LastAreaStateChange = DateTime.MinValue;
		//private bool m_isArmed = false, m_isArmedLast=false;

		public bool IsArmed
		{
			get {
				bool result = false;
				switch (m_areaState)
				{
					case Alarm.EnumAreaState.armed:
						result = true;
						break;
					case Alarm.EnumAreaState.entrydelayfinished:
						result = false;
						break;
				}
				return result; 
			}
			/*set { m_isArmed = value;
				if (m_isArmed != m_isArmedLast)
					Metadata.Rules.ExecuteRule(this);
				m_isArmedLast = m_isArmed;
			}*/
		}

		public EnumAreaState AreaState
		{
			get { return m_areaState; }
			set { m_areaState = value;
				if (m_areaState != m_areaStateLast)
					Rules.ExecuteRule(this, "areastate="+m_areaState);
				m_areaStateLast = m_areaState;
			}
		}
        public Alarm(int areaid)
        {
            AreaId = areaid;
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
	public class Alert
	{	
		[Flags]
		public enum NotificationFlags
		{
			NeedsImmediateUserAck,//int(retryinterval minutes)
			SystemError,
			NotifyUserAfterXOccurences,//int(occurence)
			NotifyUserAfterXSeconds,
			NotifyUserAfterXMinutes,
			NotifyUserAfterXHours
		}

		private static List<Alert> m_alertList = new List<Alert>();
		private static int m_id = 0;
		private int id;
		
		public String UniqueId;
		public ZoneDetails Zone;
		public Boolean UserAcknowledged = false;
		public DateTime LastSendOK = DateTime.MinValue;
		public DateTime LastSendAttempt = DateTime.MinValue;
		public DateTime AcknowledgeDate;
		public Boolean Archived = false;
		public List<DateTime> When = new List<DateTime>();
		public String InitialCause=null, LastCause=null;
		public DateTime FirstOccurence, LastOccurence;
		public int OccurenceCount=0, SendAttemptCount=0, SendOKCount=0;
		public List<NotificationFlags> Flags;
		public List<Object> FlagVars;
		//private Object[] m_flagVars;

		public Alert(String cause, ZoneDetails zone, String uniqueId, params Object[] flagVars)
		{
			
			Zone = zone;
			id = m_id;
			m_id++;
			UniqueId = uniqueId;
			//Flags = flags;
			//m_flagVars = flagVars;
			Flags = new List<NotificationFlags>();
			FlagVars = new List<object>();

			foreach (Object o in flagVars)
			{
				if (o.GetType() == typeof(NotificationFlags))
					Flags.Add((NotificationFlags)o);
				else
					FlagVars.Add(o);
			}
			Update(cause, zone);
		}

		public int Id
		{
			get { return id; }
		}

		public void Update(String cause, ZoneDetails zone)
		{
			if (InitialCause == null)
			{
				InitialCause = cause;
				LastCause = "";
			}
			else
				LastCause = cause;
			When.Add(DateTime.Now);
			FirstOccurence = When[0];
			LastOccurence = When[When.Count-1];
			OccurenceCount = When.Count;
			UserAcknowledged = false;
		}

		public static Alert CreateAlert(String cause, ZoneDetails zone, Boolean dismissPrevAlert, params Object[] flagVars)
		{
			var callingFrame = new System.Diagnostics.StackTrace(1, false).GetFrame(0);
			String uniqueId = "Native-" + callingFrame.GetNativeOffset() + "-IL-" + callingFrame.GetILOffset();
			Alert alert = m_alertList.Find(x => x.UniqueId == uniqueId && !x.Archived);
			if (dismissPrevAlert && alert != null)
			{
				alert.UserAcknowledged = true;
				alert.Archived = true;
				alert.AcknowledgeDate = DateTime.Now;
				alert.InitialCause += "AUTO DISMISSED";
			}
			else
			{
				if (alert == null)
				{
					alert = new Alert(cause, zone, uniqueId, flagVars);
					m_alertList.Add(alert);
				}
				else
				{
					alert.Update(cause, zone);
				}
			}
			return alert;
		}

		public static List<Alert> AlertList
		{
			get { return m_alertList; }
		}
		public static List<Alert> ActiveAlertList
		{
			get { return m_alertList.FindAll(x => !x.UserAcknowledged && !x.Archived); }
		}

		public static void DismissAlert(int alertId)
		{
			Alert alert = m_alertList.Find(x => x.Id == alertId);
			if (alert != null)
			{
				alert.UserAcknowledged = true;
				alert.AcknowledgeDate = DateTime.Now;
				alert.Archived = true;
			}
		}

		private static int GetEnumFlagIndex(Enum input)
		{
			int i = 0;
			foreach (Enum value in Enum.GetValues(input.GetType()))
			{
				if (input.HasFlag(value))
					return i;
				i++;
			}
			return -1;
		}

		public static List<Alert> GetAlertsToSend()
		{
			List<Alert> alerts = new List<Alert>();
			int flagVar;
			foreach (Alert alert in ActiveAlertList)
			{
				for (int i = 0; i < alert.Flags.Count; i++)
				{
					switch(alert.Flags[i])
					{
						case NotificationFlags.NotifyUserAfterXSeconds:
							{
								flagVar = Convert.ToInt16(alert.FlagVars[i]);
								if (alert.LastOccurence.Subtract(alert.FirstOccurence).TotalSeconds >= flagVar && alert.LastSendOK == DateTime.MinValue)
									alerts.Add(alert);
								break;
							}
						case NotificationFlags.NotifyUserAfterXMinutes:
						{
							flagVar = Convert.ToInt16(alert.FlagVars[i]);
							if (alert.LastOccurence.Subtract(alert.FirstOccurence).TotalMinutes >= flagVar && alert.SendOKCount == 0)
								alerts.Add(alert);
							break;
						}
						case NotificationFlags.NotifyUserAfterXHours:
						{
							flagVar = Convert.ToInt16(alert.FlagVars[i]);
							if (alert.LastOccurence.Subtract(alert.FirstOccurence).TotalHours >= flagVar && alert.SendOKCount == 0)
								alerts.Add(alert);
							break;
						}
						case  NotificationFlags.NeedsImmediateUserAck:
						{
							flagVar = Convert.ToInt16(alert.FlagVars[i]);
							if (DateTime.Now.Subtract(alert.LastSendOK).TotalMinutes >= flagVar && alert.SendOKCount== 0)
								alerts.Add(alert);
							break;
						}
						case NotificationFlags.NotifyUserAfterXOccurences:
						{
							flagVar = Convert.ToInt16(alert.FlagVars[i]);
							if (alert.OccurenceCount >= flagVar && alert.SendOKCount == 0)
								alerts.Add(alert);
							break;
						}
					}
				}
			}
			return alerts;
		}
	}

    public class MZPEvent
    {
        public enum EventSource
        {
            Alarm,
			Closure,
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
        public ZoneDetails ZoneDetails;

        public MZPEvent(DateTime dateTime, EventSource source, String message, EventType type, EventImportance importance, ZoneDetails zonedetails)
        {
            DateTime = dateTime;
            Source = source;
            Message = message;
            TypeEv = type;
            Importance = importance;
            ZoneDetails = zonedetails;
        }

		public string DisplayMessage()
		{
			return Source + " | " + Message + " | " + TypeEv + " | " + Importance + " | " + DateTime;
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

    

	
		

}

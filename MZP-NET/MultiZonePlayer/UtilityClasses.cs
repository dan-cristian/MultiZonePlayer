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
		movielist,
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
		generatefullgraph,
		doorring,
		doorentry
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
			new CommandSyntax(GlobalCommands.doorring,			GlobalParams.sourcezoneid),
			new CommandSyntax(GlobalCommands.doorentry,			GlobalParams.sourcezoneid),

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

	public enum PowerType
	{
		Denkovi,
		Numato,
		None
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

	public class Constants
	{
		public const string CAPABILITY_TEMP = "temp", CAPABILITY_HUM = "hum", EVENT_TYPE_POWER="power";
		public const string EVENT_TYPE_CAMALERT = "cam", EVENT_TYPE_SENSORALERT = "sensor", EVENT_TYPE_CLOSURE = "closure";
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
			///<summary>Integer, Retry interval in minutes</summary>
			NeedsImmediateUserAck,
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

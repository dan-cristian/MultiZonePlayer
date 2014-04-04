using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.ComponentModel;
using fastJSON;
using System.IO;

namespace MultiZonePlayer {

	public class ZoneDetails : Singleton{
		//private static List<ZoneDetails> m_valueList = new List<ZoneDetails>();
		public int ZoneId = 0;
		[Description("Edit")]
		public String Description;
		//public Boolean IsActive = false;
		//public int MinutesUntilSleep = -1;
		[Description("Edit")]
		public String SleepHourMin = "";//format HH:MM
		[Description("Edit")]
		public String ZoneName;
		private ZoneState m_zoneState, m_zoneStateLast = MultiZonePlayer.ZoneState.Undefined;
		[Description("Edit")]
		public int ParentZoneId = -1;
		[Description("Edit")]
		public String LocationName = "Default Location";
		[Description("Edit")]
		public int PowerIndex = -1;
		public String PowerType;
		[Description("Edit")]
		public int PowerOnDelay;
		public String WakeTime = "";
		public String WakeWeekDay = "";
		[Description("Edit")]
		public String CameraId = "";
		[Description("Edit")]
		public String CameraResolution = "";
		public Boolean HasSpeakers = false;
		[Description("Edit")]
		public int DefaultVolumePercent;
		[Description("Edit")]
		public String OutputKeywords;
		[Description("Edit")]
		public String OutputDeviceUserSelected;
		[Description("Edit")]
		public ZoneType Type = ZoneType.Undefined;
		public String OutputDeviceNameWaveVLC;
		public int WavedeviceIndex;
		//public Boolean HasCamera = false;
		public Boolean IsArmed = false;
		[Description("Edit")]
		public int AlarmZoneId = -1;
		[Description("Edit")]
		public int AlarmAreaId = -1;
		public Boolean HasMotionSensor = false;
		public Boolean HasMicrophone = false;
		public Boolean HasDisplay = false;
		public Boolean HasVideoPlayer = false;
		public String DisplayConnection = "";
		public String DisplayType = "";
		public Boolean RequirePowerForced = false;
		[Description("Edit")]
		public String NearbyZonesIdList = "";//zone id list separated by ;
		[Description("Edit")]
		public string ClosureIdList = "";//separated by ; iopin=2 / for gpio
		//public ClosureOpenCloseRelay ClosureOpenCloseRelay;
		[Description("Edit")]
		public ulong ClosureCount = 0;
		
		public Boolean IsClosureArmed = false;
		[Description("Edit")]
		public EnumClosureType ClosureType = EnumClosureType.Undefined;
		private Boolean m_isClosureContactMade = false, m_isClosureContactMadeLast = false;
		public EnumRelayState RelayState = EnumRelayState.Undefined;
		[Description("Edit")]
		public EnumRelayType RelayType = EnumRelayType.Undefined;

		[Description("Edit")]
		public EnumUtilityType UtilityType = EnumUtilityType.Undefined;
		public ulong PulseCountInTimeSample = 0;
		//public ulong CounterCountInSample = 0;
		[Description("Edit")]
		public int PulseSampleMinutesFrequency = 1;//in minutes
		[Description("Edit")]
		public uint PulseSubUnits = 1;//how many subunits in a main unit 100 e.g. flashes in a kwh
		[Description("Edit")]
		public double PulseMainUnitsCount = 0;//main units, e.g. 3 kwh
		//[Description("Edit")]
		//public double CounterMainUnitsCount = 0;//main units, e.g. 4 kwh
		[Description("Edit")]
		public string CounterPageNameToInclude = "1";//counter page name to consider, ignore rest (e.g. 1)
		[Description("Edit")]
		public string ClosureLevelNameToInclude = "1,2";//counter page name to consider, ignore rest (e.g. 1,2)
		[Description("Edit")]
		public DateTime PulseMainUnitsStartDate = DateTime.Now;
		public double PulseLastMainUnitsCount = 0;
		//public double CounterLastMainUnitsCount = 0;
		public ulong LastCounterCount = 0;
		public DateTime LastPulseSamplingStart = DateTime.Now;
		//private DateTime m_lastCounterSamplingStart = DateTime.Now;
		[Description("Edit")]
		public String PulseMainUnitType = "";

		[Description("Edit")]
		public String TemperatureDeviceId;
		//[Description("Edit")]
		//public String OtherOneWireDeviceIdList;//separated by ;

		[Description("Edit")]
		public double TemperatureMaxAlarm = 1000;
		[Description("Edit")]
		public double TemperatureMinAlarm = -1000;
		[Description("Edit")]
		public double TemperatureTarget = -1000;
		public Boolean ScheduledHeatActive = false;
		public String CronSchedule = "";
		[Description("Edit")]
		public String Color;

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
		private List<PictureSnapshot> m_pictureList = new List<PictureSnapshot>();
		private Boolean m_movementAlert = false, m_movementAlertLast = false;

		public DateTime LastAlarmMovementDateTime = DateTime.MinValue;
		public DateTime LastCamAlertDateTime = DateTime.Now;
		public DateTime LastLocalCommandDateTime = DateTime.MinValue;
		public DateTime LastClosureEventDateTime = DateTime.MinValue;
		private ZoneNotifyState m_notifyZoneEventTriggered = ZoneNotifyState.Closed;
		
		public DateTime LastNotifyZoneEventTriggered = DateTime.MinValue;

		private double[] m_voltage = new double[5];
		protected const double DEFAULT_TEMP_HUM = -1000;

		protected double m_temperature = DEFAULT_TEMP_HUM, m_humidity = DEFAULT_TEMP_HUM;
		//protected double m_temperatureLast = DEFAULT_TEMP_HUM, m_humidityLast = DEFAULT_TEMP_HUM;
		protected DateTime m_lastTempSet = DateTime.MinValue, m_lastHumSet = DateTime.MinValue;
		// not serializable, hidden from json
		protected static int m_intervalImmediate, m_intervalRecent, m_intervalPast;
		protected static new List<Singleton> m_valueList = new List<Singleton>();
		protected Boolean m_hasOneWireTempSensor = false,m_hasOneWireIODevice=false,m_hasOneWireVoltageSensor=false;

		public ZoneDetails() {
			//ClosureOpenCloseRelay = new ClosureOpenCloseRelay(false);
		}

		public ZoneDetails(int p_zoneId, String p_zoneName) {
			ZoneId = p_zoneId;
			ZoneName = p_zoneName;
			ZoneState = MultiZonePlayer.ZoneState.NotInitialised;
			ActivityType = GlobalCommands.nul;
			LoadStateFromIni();
		}
		public static ZoneDetails StaticInstance {
			get {
				if (m_valueList != null && m_valueList.Count > 0)
					return (ZoneDetails)m_valueList[0];
				else return new ZoneDetails();
			}
		}
		public override List<Singleton> ValueList {
			get { return m_valueList; }
		}
		public override Singleton Instance {
			get { return ZoneDetails.StaticInstance; }
		}
		public override string ToString() {
			return "ID=" + ZoneId + ";Name=" + ZoneName;
		}
		#region getters

		public ZoneNotifyState NotifyZoneEventTriggered {
			get { return m_notifyZoneEventTriggered; }
			set { m_notifyZoneEventTriggered = value; }
		}

		public Boolean IsClosureContactMade {
			get { return m_isClosureContactMade; }
			set {
				m_isClosureContactMade = value;
				ClosureCount++;
				if (value && RelayType==EnumRelayType.NormalOpen) {
					RelayState = EnumRelayState.ContactClosed;
				}
				else {
					RelayState = EnumRelayState.ContactOpen;
				}
				MLog.Log(this, "Contact event=" + value + " on zone="+ ZoneName +" relaystate=" + RelayState + " relaytype="+RelayType);

				LastClosureEventDateTime = DateTime.Now;
				if (m_isClosureContactMade != m_isClosureContactMadeLast) {
					Rules.ExecuteRule(this, "isclosurecontact=" + m_isClosureContactMade);
					m_isClosureContactMadeLast= m_isClosureContactMade;
				}
			}
		}

		public void RecordPulse(string level) {
			if (ClosureLevelNameToInclude.Contains(level)) {
				if (DateTime.Now.Subtract(LastPulseSamplingStart).TotalMinutes >= PulseSampleMinutesFrequency) {
					PulseLastMainUnitsCount = (double)PulseCountInTimeSample / PulseSubUnits;
					PulseMainUnitsCount += PulseLastMainUnitsCount;
					double cost = PulseLastMainUnitsCount * UtilityCost.UtilityCostList.Find(x => x.Name.Equals(EnumUtilityType.Electricity)).UnitCost;
					Utilities.AppendToCsvFile(IniFile.CSV_UTILITIES, ",", ZoneName, DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT),
						PulseLastMainUnitsCount.ToString(), ZoneId.ToString(), UtilityType.ToString(), PulseMainUnitsCount.ToString(),
						cost.ToString());
					LastPulseSamplingStart = DateTime.Now;
					PulseCountInTimeSample = 0;
				}
				else {
					PulseCountInTimeSample++;
				}
			}
			else {
				MLog.Log(this, "Ignoring closure level " + level);
			}//ignoring other closure levels
		}
		public void RecordCounter(string id, ulong counter) {
			if (id.Contains(CounterPageNameToInclude)) {
				double lapsedMinutes = DateTime.Now.Subtract(LastPulseSamplingStart).TotalMinutes;
				if ( lapsedMinutes >= PulseSampleMinutesFrequency) {
					PulseLastMainUnitsCount = (double)PulseCountInTimeSample / PulseSubUnits;
					if (PulseLastMainUnitsCount > 1) {
						Alert.CreateAlert("WARNING: Large count, counter=" + counter + " deltaPulses=" + PulseCountInTimeSample,
							this, false, Alert.NotificationFlags.NotifyUserAfterXMinutes, 1);
					}
					if (lapsedMinutes > 2 * PulseSampleMinutesFrequency)
					{
						Alert.CreateAlert("Long pulse counter period detected, minutes=" + lapsedMinutes
							+ " counterdelta=" + PulseCountInTimeSample,
							this, false, Alert.NotificationFlags.NotifyUserAfterXMinutes, 1);
					}
					PulseMainUnitsCount += PulseLastMainUnitsCount;
					double unitCost = UtilityCost.UtilityCostList.Find(x => x.Name.Equals(EnumUtilityType.Electricity)).UnitCost;
					double cost = PulseLastMainUnitsCount * unitCost;
					double watts = 1000 * PulseLastMainUnitsCount/(PulseSampleMinutesFrequency/60d);
					Utilities.AppendToCsvFile(IniFile.CSV_UTILITIES, ",", ZoneName, DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT),
						PulseLastMainUnitsCount.ToString(), ZoneId.ToString(), UtilityType.ToString(), PulseMainUnitsCount.ToString(),
						cost.ToString(), unitCost.ToString(), watts.ToString(), counter.ToString());
					LastPulseSamplingStart = DateTime.Now;
					PulseCountInTimeSample = 0;
				}
				else {
					ulong units = counter - LastCounterCount;
					if (units < 0) {
						MLog.Log(this, "WARNING, counter reset at LastCount="+LastCounterCount);
						units = 0;
					}
					if (LastCounterCount != 0)
						PulseCountInTimeSample += units;
					LastCounterCount = counter;
				}
			}
			else {
				//MLog.Log(this, "Ignoring counter id " + id);
			}//ignoring other counters
		}

		public void ResetPulse() {
			PulseMainUnitsCount = 0;
			PulseMainUnitsStartDate = DateTime.Now;
		}

		public double PulseTotalCost {
			get { return PulseMainUnitsCount * UtilityCost.UtilityCostList.Find(x => x.Name.Equals(EnumUtilityType.Electricity)).UnitCost; }
		}

		public double PulseFrequencyPerMinute {
			get { return (double) PulseCountInTimeSample/PulseSampleMinutesFrequency; }
		}

		public double PulseTotalUnitsAgeInHours {
			get { return Math.Round(DateTime.Now.Subtract(PulseMainUnitsStartDate).TotalHours, 2); }
		}

		public Boolean IsActive {
			get {
				return ZoneState == MultiZonePlayer.ZoneState.Running;
					//|| LastLocalCommandAgeInSeconds<120; 
			}
		}

		/*public Boolean IsIdle {
			get {
				return IsActive || LastLocalCommandAgeInSeconds<120; 
			}
		}*/

		public ZoneState ZoneState {
			get { return m_zoneState; }
			set {
				m_zoneState = value;
				if (m_zoneState != m_zoneStateLast) {
					Rules.ExecuteRule(this, "zonestate=" + m_zoneState);
					m_zoneStateLast = m_zoneState;
				}
			}
		}
		public Boolean HasCamera {
			get { return CameraId != ""; }
		}
		public Boolean MovementAlert {
			get { return m_movementAlert; }
			set {
				m_movementAlert = value;
				if (m_movementAlert != m_movementAlertLast) {
					Rules.ExecuteRule(this, "movement=" + m_movementAlert);
					m_movementAlertLast = m_movementAlert;
				}
			}
		}
		public String SummaryStatus {
			get {
				String val = "#" + ZoneId + " " + ZoneName + (IsActive ? " Active" : "")
							 + (ActivityType.Equals(GlobalCommands.nul) ? "" : " " + ActivityType.ToString())
							 + (IsArmed ? " Armed " : " ")
							 + Utilities.DurationAsTimeSpan(LastMovementAge)
							 + (HasImmediateMove ? " ImmediateMove " : "")
							 + (HasRecentMove ? " RecentMove " : "")
							 + (RelayType != EnumRelayType.Undefined ? " " + ClosureState + "@ " + LastClosureEventDateTime : " ")
							 + Title
							 + (Temperature != DEFAULT_TEMP_HUM ? " " + Temperature + "C" : "")
							 + (Humidity != DEFAULT_TEMP_HUM ? " " + Humidity + "%" : "")
							 + (IsPowerOn ? " POWERON" : "");
				return val;
			}
		}
		public DateTime LastMovementDate {
			get {
				List<DateTime> dates = new List<DateTime>();
				dates.Add(LastAlarmMovementDateTime);
				dates.Add(LastCamAlertDateTime);
				dates.Add(LastClosureEventDateTime);
				dates = dates.OrderByDescending(x => x.Ticks).ToList();
				return dates[0];
			}
		}
		public string LastCamAlertAgeAsTimeSpan {
			get { return Utilities.DurationAsTimeSpan(DateTime.Now.Subtract(LastCamAlertDateTime)); }
		}

		public TimeSpan LastMovementAge {
			get { return DateTime.Now.Subtract(LastMovementDate); }
		}

		public string LastMovementAgeAsTimeSpan {
			get { return Utilities.DurationAsTimeSpan(LastMovementAge); }
		}

		public double LastLocalCommandAgeInSeconds {
			get { return Math.Round(DateTime.Now.Subtract(LastLocalCommandDateTime).TotalSeconds); }
		}
		public Boolean HasImmediateMove {
			get {
				double span = DateTime.Now.Subtract(LastMovementDate).TotalMinutes;
				//if presence contact is active means user is at home (e.g. on bed)
				bool isPresenceContactMade = (ClosureType == EnumClosureType.PresenceContact) && IsClosureContactMade;
				return (span <= m_intervalImmediate) || isPresenceContactMade;
			}
		}

		public Boolean HasImmediateCamMove {
			get {
				double span = DateTime.Now.Subtract(LastCamAlertDateTime).TotalMinutes;
				return (span <= m_intervalImmediate);
			}
		}
		public Boolean HasImmediateAlarmMove {
			get {
				double span = DateTime.Now.Subtract(LastAlarmMovementDateTime).TotalMinutes;
				return (span <= m_intervalImmediate);
			}
		}
		public Boolean HasImmediateClosureMove {
			get {
				double span = DateTime.Now.Subtract(LastClosureEventDateTime).TotalMinutes;
				return (span <= m_intervalImmediate);
			}
		}

		public Boolean HasRecentMove {
			get {
				double span = DateTime.Now.Subtract(LastMovementDate).TotalMinutes;
				return (span > m_intervalImmediate && span <= m_intervalRecent);
			}
		}
		public Boolean HasPastMove {
			get {
				double span = DateTime.Now.Subtract(LastMovementDate).TotalMinutes;
				return (span > m_intervalRecent && span <= m_intervalPast);
			}
		}
		public Boolean HasNoMove {
			get {
				double span = DateTime.Now.Subtract(LastMovementDate).TotalMinutes;
				return (span > m_intervalPast);
			}
		}

		public Boolean HasPastActivity {
			get {
				double span = DateTime.Now.Subtract(LastLocalCommandDateTime).TotalMinutes;
				return (HasPastMove && (span > m_intervalRecent && span <= m_intervalPast));
			}
		}

		public Boolean HasAction {
			get {
				bool action = HasImmediateMove || IsActive || IsPowerOn || HasAlarm;
				ZoneDetails child;
				if (!action)
					for (int i = 0; i < ChildZonesCount; i++) {
						child = ChildZone(i);
						action = child.HasImmediateMove || child.IsActive || child.IsPowerOn || child.HasAlarm;
						if (action)
							break;
					}
				return action;
			}
		}

		public Boolean HasAlarm {
			get {
				return IsArmed && HasImmediateMove;
			}
		}

		public Boolean HasTemperatureAlarm {
			get {
				return (m_temperature > TemperatureMaxAlarm) || (m_temperature < TemperatureMinAlarm);
			}
		}

		public Boolean IsPowerOn {
			get { return MZPState.Instance.PowerControlIsOn(ZoneId); }
		}
		public Boolean HasPowerCapabilities {
			get {
				return PowerIndex != -1 && PowerType != "";
			}
		}
		public Boolean HasClosures {
			get { return ClosureIdList != "" || HasOneWireIODevice; }
		}

		public Boolean HasOneWireTemperatureSensor {
			get { return m_hasOneWireTempSensor; }
			set { m_hasOneWireTempSensor = value; }
		}

		public Boolean HasOneWireIODevice {
			get { return m_hasOneWireIODevice; }
			set { m_hasOneWireIODevice = value; }
		}

		public Boolean HasOneWireVoltageSensor {
			get { return m_hasOneWireVoltageSensor; }
			set { m_hasOneWireVoltageSensor = value; }
		}

		public Boolean HasTemperatureSensor {
			get {
				RFXDeviceDefinition.RFXDevice device = RFXDeviceDefinition.GetDevice(ZoneId);
				return (HasOneWireTemperatureSensor ||
						(device != null && device.DeviceType == RFXDeviceDefinition.DeviceTypeEnum.temp_hum));
			}
		}
		public Boolean HasHumiditySensor {
			get {
				RFXDeviceDefinition.RFXDevice device = RFXDeviceDefinition.GetDevice(ZoneId);
				return (device != null && device.DeviceType == RFXDeviceDefinition.DeviceTypeEnum.temp_hum);
			}
		}
		public Boolean ContainsDisplay {
			get {
				return ZoneDetails.ZoneDetailsList.Find(x => x.ParentZoneId == ZoneId && x.HasDisplay) != null
					   || HasDisplay;
			}
		}

		public String ClosureState {
			get {
				return RelayState.ToString();
			}
		}

		public Boolean RequireHeat {
			get {
				return (Temperature < TemperatureTargetTreshhold) && ScheduledHeatActive;
			}
		}
		public Boolean RequirePower {
			get {
				bool powerfortoolong = (RequirePowerForced || IsActive) 
					&& (LastMovementAge.TotalMinutes>15)
					&& (LastLocalCommandAgeInSeconds > 60 * 15);
				
				bool powerforparentheat = false;

				if (Type == ZoneType.Heat) {//for zones with child heat component
					ZoneDetails parent = ZoneDetails.GetZoneById(ParentZoneId);
					if (parent!=null)
						powerforparentheat = parent.RequireHeat;
				}
				bool regularstate = powerforparentheat
					//|| (RequireHeat && ) //for zones with own heat and temp sensor
					|| (ZoneState == MultiZonePlayer.ZoneState.Running)
					|| RequirePowerForced;
				
				bool exclude = ActivityType!=GlobalCommands.tv;

				return HasPowerCapabilities && regularstate && (!powerfortoolong) && exclude;
			}
		}

		public int MacroCount {
			get {
				List<MacroEntry> macros = MZPState.Instance.GetZoneMacros(ZoneId);
				return macros != null ? macros.Count : 0;
			}
		}

		public MacroEntry Macro(int macroIndex) {
			List<MacroEntry> macros = MZPState.Instance.GetZoneMacros(ZoneId);
			if (macros != null && macroIndex < macros.Count)
				return macros[macroIndex];
			else
				return null;
		}

		public int ChildZonesCount {
			get {
				List<ZoneDetails> childs = ZoneDetails.ZoneDetailsList.FindAll(x => x.ParentZoneId == ZoneId);
				return childs != null ? childs.Count : 0;
			}
		}

		public ZoneDetails ChildZone(int index) {
			List<ZoneDetails> childs = ZoneDetails.ZoneDetailsList.FindAll(x => x.ParentZoneId == ZoneId);
			return childs != null && index < childs.Count ? childs[index] : null;
		}


		public String OutputDeviceDirectXName {
			get {
				String[] split;
				split = OutputDeviceAutoCompleted().Split(new String[] { "\\DirectSound: " },
					StringSplitOptions.RemoveEmptyEntries);
				if (split.Length >= 2)
					return split[1];
				else
					return "";
			}
		}

		/*public Boolean IsClosureActivated {
			get {
				return ClosureOpenCloseRelay.RelayContactMade;
			}
		}*/

		public Boolean HasNotifyMove {
			get { return HasImmediateMove || HasRecentMove; }
		}

		public double Temperature {
			get { return m_temperature; }//return Math.Round(m_temperature, 2).ToString(); }
			set {
				if (Temperature != value) {
					Utilities.AppendToCsvFile(IniFile.CSV_TEMPERATURE_HUMIDITY, ",", ZoneName,
						Constants.CAPABILITY_TEMP, DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT), value.ToString(), ZoneId.ToString());
					Rules.ExecuteRule(this, "temp=" + value);
					//m_temperatureLast = m_temperature;
				}

				m_temperature = value;
				m_lastTempSet = DateTime.Now;

				if (Temperature > TemperatureMaxAlarm) {
					Alert.CreateAlert("Max temperature [" + TemperatureMaxAlarm + "] exceeded on zone "
						+ ZoneName + ", temp is " + Temperature, this, false,
						Alert.NotificationFlags.NotifyUserAfterXHours, 1);
				}
				else
					if (Temperature < TemperatureMinAlarm) {
						Alert.CreateAlert("Min temperature [" + TemperatureMinAlarm + "] exceeded on zone "
							+ ZoneName + ", temp is " + Temperature, this, false,
							Alert.NotificationFlags.NotifyUserAfterXHours, 1);
					}

				
			}
		}

		public double TemperatureTargetTreshhold {
			get { return TemperatureTarget + 0.2; }
		}

		public TimeSpan TemperatureAge {
			get { return DateTime.Now.Subtract(m_lastTempSet); }
		}

		public String TemperatureAgeAsTimeSpan {
			get { return Utilities.DurationAsTimeSpan(TemperatureAge); }
		}

		public double Humidity {
			get { return m_humidity; }
			set {
				if (Humidity != value) {
					Utilities.AppendToCsvFile(IniFile.CSV_TEMPERATURE_HUMIDITY, ",", ZoneName,
						Constants.CAPABILITY_HUM, DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT), value.ToString(), ZoneId.ToString());
					Rules.ExecuteRule(this, "humid=" + value);
				}
				m_humidity = value;
				m_lastHumSet = DateTime.Now;
			}
		}

		public String HumidityAgeInMinutes {
			get { return Math.Round(DateTime.Now.Subtract(m_lastHumSet).TotalMinutes).ToString(); }
		}


		#endregion
		public static void LoadFromIni() {
			Hashtable zoneValues = IniFile.LoadAllIniEntriesByIntKey(IniFile.INI_SECTION_ZONES);

			ZoneDetails zone;

			m_intervalImmediate = Convert.ToInt16(IniFile.PARAM_GENERIC_INTERVAL_SPLIT[1].Split('-')[0]);
			m_intervalRecent = Convert.ToInt16(IniFile.PARAM_GENERIC_INTERVAL_SPLIT[1].Split('-')[1]);
			m_intervalPast = Convert.ToInt16(IniFile.PARAM_GENERIC_INTERVAL_SPLIT[1].Split('-')[2]);

			foreach (int id in zoneValues.Keys) {
				zone = new ZoneDetails(id, zoneValues[id].ToString());
				m_valueList.Add(zone);
			}

			m_valueList.Sort(delegate(Singleton a1, Singleton a2) {
				return ((ZoneDetails)a1).ZoneId.CompareTo(((ZoneDetails)a2).ZoneId);
			});
		}

		public void LoadStateFromIni() {
			try {
				String json = IniFile.LoadIniEntryByKey(IniFile.INI_SECTION_ZONESTATE, ZoneId.ToString());
				if (json != "") {
					MLog.Log(this, "Loading state for zone=" + ZoneId);
					if (ZoneId == 20)
						MultiZonePlayer.MZPState.Instance.TestCond = true;
					ZoneDetails zonestorage = JSON.Instance.ToObject<ZoneDetails>(json);
					Description = zonestorage.Description;
					ZoneName = zonestorage.ZoneName;
					ParentZoneId = zonestorage.ParentZoneId;
					PowerIndex = zonestorage.PowerIndex;
					PowerOnDelay = zonestorage.PowerOnDelay;
					DefaultVolumePercent = zonestorage.DefaultVolumePercent;

					CameraId = zonestorage.CameraId;
					CameraResolution = zonestorage.CameraResolution;
					//if (CameraId != "") HasCamera = true;
					AlarmZoneId = zonestorage.AlarmZoneId;
					if (AlarmZoneId != -1) HasMotionSensor = true;
					AlarmAreaId = zonestorage.AlarmAreaId;

					OutputDeviceUserSelected = zonestorage.OutputDeviceUserSelected;
					OutputKeywords = zonestorage.OutputKeywords;
					//OutputDeviceAutoCompleted = GetOutputDeviceNameAutocompleted(OutputDeviceUserSelected, OutputKeywords);
					if (!OutputDeviceAutoCompleted().Equals("")) {
						HasSpeakers = true;
					}

					WavedeviceIndex = GetWaveOutDeviceIndex(OutputKeywords);
					OutputDeviceNameWaveVLC = GetVLCAudioWaveDeviceName(WavedeviceIndex);

					WakeTime = zonestorage.WakeTime;
					WakeWeekDay = zonestorage.WakeWeekDay;
					SleepHourMin = zonestorage.SleepHourMin;

					DisplayType = zonestorage.DisplayType;
					DisplayConnection = zonestorage.DisplayConnection;
					if (DisplayType.Equals(Display.DisplayTypeEnum.LGTV.ToString())) {
						HasDisplay = true;
					}
					else
						if (DisplayType.Equals(Display.DisplayTypeEnum.XBMC.ToString()))
							HasVideoPlayer = true;

					/*if (zonestorage.ClosureOpenCloseRelay != null) {
						ClosureOpenCloseRelay = zonestorage.ClosureOpenCloseRelay;
						ClosureOpenCloseRelay.RelayType = zonestorage.ClosureOpenCloseRelay.RelayType;
					}
					else {
						ClosureOpenCloseRelay = new ClosureOpenCloseRelay(false);
						ClosureOpenCloseRelay.RelayType = ClosureOpenCloseRelay.EnumRelayType.Undefined;
					}*/

					ClosureIdList = zonestorage.ClosureIdList.Trim();
					ClosureCount = zonestorage.ClosureCount;
					ClosureType = zonestorage.ClosureType;
					RelayType = zonestorage.RelayType;
					
					//if (ClosureOpenCloseRelay.RelayType == ClosureOpenCloseRelay.EnumRelayType.NormalOpen)
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

					UtilityType = zonestorage.UtilityType;
					PulseMainUnitsCount = zonestorage.PulseMainUnitsCount;
					PulseMainUnitsStartDate = zonestorage.PulseMainUnitsStartDate;
					PulseSampleMinutesFrequency = zonestorage.PulseSampleMinutesFrequency;
					PulseSubUnits = zonestorage.PulseSubUnits;
					PulseMainUnitType = zonestorage.PulseMainUnitType;
					PulseCountInTimeSample = zonestorage.PulseCountInTimeSample;
					LastCounterCount = zonestorage.LastCounterCount;
					LastPulseSamplingStart = zonestorage.LastPulseSamplingStart;

					LocationName = zonestorage.LocationName;

					LoadPicturesFromDisk();
				}
				else
					MLog.Log(this, "Ini state NOT FOUND for zone=" + ZoneName);
			}
			catch (Exception ex) {
				MLog.Log(ex, "Unable to load zone id="+ ZoneId +" message "+ ex.Message +" STACK=" + ex.StackTrace);
				throw new Exception("ZoneLoad Exception zoneid="+ZoneId, ex);
			}
		}


		public override void SaveToIni() {
			MLog.Log(this, "Saving state to ini zone=" + ZoneName);
			//remove fields that generate serialisation problems
			this.Meta = null;

			fastJSON.JSONParameters param = new fastJSON.JSONParameters(); 
			param.UseExtensions = false;
			String json = JSON.Instance.ToJSON(this, param);
			IniFile.IniWriteValuetoFinal(IniFile.INI_SECTION_ZONESTATE, ZoneId.ToString(), json);
			
			//update dependencies if fields have changed - not a good ideea as triggers activity change
			//MZPState.Instance.WDIO.SetPinTypes(this);
		}

		private void LoadPicturesFromDisk() {
			String dir = IniFile.CurrentPath() + IniFile.WEB_PICTURES_SUBFOLDER;
			PictureSnapshot pict;
			String[] files =Directory.GetFiles(dir, ZoneId+"*.jpg");
			//MLog.Log(this, "Loading " + files.Length + " pictures for zone " + ZoneName + " from dir=" + dir);
			foreach (string fileName in files) {
				pict = new PictureSnapshot();
				pict.FileName = Path.GetFileName(fileName);
				pict.ThumbFileName = Path.GetFileNameWithoutExtension(fileName) + ".png";
				pict.DateTimeTaken = File.GetCreationTime(fileName);
				
				if (fileName.Contains(EventSource.Alarm.ToString()))
					pict.EventSource = EventSource.Alarm;
				else if (fileName.Contains(EventSource.Cam.ToString()))
					pict.EventSource = EventSource.Cam;
				else if (fileName.Contains(EventSource.Closure.ToString()))
					pict.EventSource = EventSource.Closure;
				else pict.EventSource = EventSource.Undefined;
				PictureList.Add(pict);
			}
		}

		public bool HasOutputDeviceAvailable() {
			return GetOutputDeviceNameAutocompleted(OutputDeviceUserSelected, OutputKeywords) != "";
		}

		public String OutputDeviceAutoCompleted() {
			return GetOutputDeviceNameAutocompleted(OutputDeviceUserSelected, OutputKeywords);
		}

		public static String GetOutputDeviceNameAutocompleted(String p_outputDevice, String p_outputKeywords) {
			List<String> systemOutputDevices = DShowUtility.SystemDeviceNameList;
			String matchValue;
			String result = "";//"NONE-outdev=" + p_outputDevice + "|keys=" + p_outputKeywords + "|";
			if (p_outputKeywords != null) {
				String[] keys = p_outputKeywords.Split(',');

				if (p_outputDevice.Equals(IniFile.DEFAULT_AUTO_DEV_NAME)) {
					foreach (String device in systemOutputDevices) {
						matchValue = "";
						foreach (String key in keys) {
							if (device.ToLower().Contains(key.ToLower()))
								matchValue = device;
							else {
								matchValue = "";
								break;
							}
						}
						if (matchValue != "") {
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

		private static int GetWaveOutDeviceIndex(String p_outputKeywords) {
			List<DShowUtility.WAVEOUTCAPS> systemWaveOutputDevices;
			systemWaveOutputDevices = DShowUtility.GetDevCapsPlayback();
			int result = -1;
			if (p_outputKeywords != null) {
				String[] keys = p_outputKeywords.Split(new String[] { "," }, StringSplitOptions.RemoveEmptyEntries);

				for (int i = 0; i < systemWaveOutputDevices.Count; i++) {
					result = -1;
					foreach (String key in keys) {
						if (!key.Equals("directsound")) {
							if (systemWaveOutputDevices[i].szPname.ToLower().Contains(key.ToLower()))
								result = i;
							else {
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

		private static String GetVLCAudioWaveDeviceName(int wavedeviceindex) {
			String result;
			DShowUtility.WAVEOUTCAPS wave;
			if (wavedeviceindex != -1) {
				wave = DShowUtility.GetDevCapsPlayback()[wavedeviceindex];
				result = wave.szPname + " ($" + String.Format("{0:x}", wave.wMid) + ",$" + String.Format("{0:x}", wave.wPid) + ")";
			}
			else
				result = "no wave device";
			return result;
		}

		public void Close() {
			ZoneClose();
			SaveToIni();
		}

		public int GetDefaultVolume() {
			return GetDefaultVolume(DefaultVolumePercent);
		}
		public int GetDefaultAlarmVolume() {
			return GetDefaultVolume(DefaultVolumePercent / 2);
		}
		public int GetDefaultNotifyUserVolume() {
			int defvol = GetDefaultVolume(DefaultVolumePercent);
			defvol = defvol - defvol * Convert.ToInt16(IniFile.PARAM_NOTIFY_VOLUME_INCREASE[1]) / 100;
			return defvol;
		}

		private static int GetDefaultVolume(int percent) {
			String dt = DateTime.Now.ToString(IniFile.DATETIME_DAYHR_FORMAT);
			String[] interval = IniFile.PARAM_SILENCE_SAFE_HOUR_INTERVAL[1].Split('-');
			//reduce volume during silence interval - night
			if (dt.CompareTo(interval[0]) >= 0 || dt.CompareTo(interval[1]) <= 0)
				percent = Convert.ToInt16(IniFile.PARAM_SILENCE_SAFE_VOLUME[1]);
			double val = ((100 - Convert.ToDouble(percent)) / 100);
			return Convert.ToInt16(VolumeLevels.VolumeSilence * val);
		}

		public void ZoneStop() {
			Album = null;
			Author = null;
			Year = null;
			Title = null;
			Genre = null;
			SourceURL = null;
			//RequirePower = false;
			ZoneState = ZoneState.NotStarted;
			//ActivityType = GlobalCommands.nul;
		}

		public void ZoneClose() {
			ZoneStop();
			//IsActive = false;
			ActivityType = GlobalCommands.nul;
			ZoneState = ZoneState.NotInitialised;

		}

		public Boolean IsNearbyZone(int zoneId) {
			return NearbyZonesIdList.Contains(zoneId.ToString() + ";");
		}

		public List<PictureSnapshot> PictureList {
			get { return m_pictureList; }
		}

		public void SetVoltage(int voltageIndex, double value) {
			double lastVal;
			lastVal = m_voltage[voltageIndex];
			if (lastVal != value) {
				Utilities.AppendToCsvFile(IniFile.CSV_VOLTAGE, ",", ZoneName,
						Constants.CAPABILITY_VOLTAGE,
						DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT), 
						value.ToString(), ZoneId.ToString(), voltageIndex.ToString());
				m_voltage[voltageIndex] = value;
			}
			
		}
		#region statics
		/*public static List<ZoneDetails> ValueList {
			get { return ZoneDetails.ValueList.Select(x=>(ZoneDetails)x).ToList(); }
		}*/

		public static List<ZoneDetails> ZoneDetailsList {
			get { return m_valueList.Select(x => (ZoneDetails)x).ToList(); }
		}
		public static ZoneDetails GetZoneById(int zoneId) {
			return (ZoneDetails)m_valueList.Find(item => ((ZoneDetails)item).ZoneId == zoneId);
		}

		public static TimeSpan LastMovementAge_Min {
			get {
				return ZoneDetailsList.Min(x => x.LastMovementAge);
			}
		}
		public static Boolean HasImmediateMove_Once {
			get {
				return ZoneDetailsList.Find(x => x.HasImmediateMove) != null;
			}
		}
		public static Boolean HasRecentMove_Once {
			get {
				return ZoneDetailsList.Find(x => x.HasRecentMove) != null;
			}
		}
		public static List<ZoneDetails> HasImmediateMove_All {
			get {
				return ZoneDetailsList.FindAll(x => x.HasImmediateMove);
			}
		}
		public static List<ZoneDetails> HasRecentMove_All {
			get {
				return ZoneDetailsList.FindAll(x => x.HasRecentMove);
			}
		}

		
		public static List<ZoneDetails> ZoneWithPotentialUserPresence_All {
			get {
				return ZoneDetailsList.FindAll(x => x.HasSpeakers && (x.IsActive || x.HasImmediateMove
					|| x.HasRecentMove || x.LastLocalCommandAgeInSeconds < 600))
					.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.HasImmediateMove)
					.ThenBy(x => x.LastLocalCommandAgeInSeconds).ToList();
			}
		}

		public static Boolean ZoneThatRequireHeat_All {
			get {
				return ZoneDetailsList.Find(x=>x.RequireHeat) != null;
			}
		}
		#endregion
	}


	public enum ZoneState {
		NotInitialised,
		NotStarted,
		Running,
		Paused,
		Closing,
		Undefined,
		Error
	}


	public enum ZoneType {
		Undefined,
		Space,
		Component,
		Heat,
		Light,
		Alarm,
		Closure
	}

	public enum EnumRelayState {
		Undefined = -1,
		ContactOpen = 0,
		ContactClosed = 1
	}
	public enum EnumClosureType {
		Undefined = -1,
		Contact = 0,
		Pulse = 1,
		Counter = 2,
		PresenceContact = 3
	}
	public enum EnumUtilityType {
		Undefined = -1,
		Electricity = 0,
		Water = 1,
		Gas = 2
	}
	public enum EnumRelayType {
		Undefined,
		NormalOpen,
		NormalClosed,
		Button
	}

	
	/*
	public class ClosureOpenCloseRelay {
		//public string Key;
		public DateTime LastChange;
		private EnumRelayState m_relayState = EnumRelayState.Undefined;
		private EnumRelayState m_relayStateLast = EnumRelayState.Undefined;
		public EnumRelayType RelayType = EnumRelayType.Undefined;
		private bool m_contactMade = false;

		public ClosureOpenCloseRelay() {
		}

		public ClosureOpenCloseRelay(bool isRelayContactMade) {
			//Key = key;
			RelayContactMade = isRelayContactMade;
		}

		public String LastChangeAgeAsTimeSpan {
			get { return Utilities.DurationAsTimeSpan(DateTime.Now.Subtract(LastChange)); }
		}
		public EnumRelayState RelayState {
			get { return m_relayState; }
			//set { m_relayState = value; }
		}

		public bool RelayContactMade {
			get { return m_contactMade; }
			set {
				m_contactMade = value;
				m_relayState = GetRelayState(m_contactMade);
				LastChange = DateTime.Now;

				if (m_relayState != m_relayStateLast) {
					Rules.ExecuteRule(this, "contactmade=" + m_relayState);
					m_relayStateLast = m_relayState;
				}
			}
		}

		public EnumRelayState GetRelayState(bool isRelayContactMade) {
			switch (RelayType) {
				case EnumRelayType.NormalOpen:
				case EnumRelayType.Button:
					return isRelayContactMade ? EnumRelayState.ContactClosed : EnumRelayState.ContactOpen;
				case EnumRelayType.NormalClosed:
					return isRelayContactMade ? EnumRelayState.ContactOpen : EnumRelayState.ContactClosed;
				default:
					//MLog.Log(this, "Error, undefined relay type");
					return EnumRelayState.Undefined;
			}
		}

		public void ResetState() {
			m_relayState = EnumRelayState.Undefined;
			m_relayStateLast = EnumRelayState.Undefined;
		}

		
	}
	*/
}

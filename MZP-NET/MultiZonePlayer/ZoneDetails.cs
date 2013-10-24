using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.ComponentModel;
using fastJSON;

namespace MultiZonePlayer {

	public class ZoneDetails {
		public int ZoneId = 0;
		[Description("Edit")]
		public String Description;
		public Boolean IsActive = false;
		//public int MinutesUntilSleep = -1;
		[Description("Edit")]
		public String SleepHourMin = "";//format HH:MM
		public String ZoneName;
		private ZoneState m_zoneState, m_zoneStateLast = MultiZonePlayer.ZoneState.Undefined;
		public int ParentZoneId = -1;
		public int PowerIndex = -1;
		public String PowerType;
		public int PowerOnDelay;
		public String WakeTime = "";
		public String WakeWeekDay = "";
		public String CameraId = "";
		public Boolean HasSpeakers = false;
		[Description("Edit")]
		public int DefaultVolumePercent;
		public String OutputKeywords;
		public String OutputDeviceUserSelected;
		public ZoneType Type = ZoneType.Undefined;
		public String OutputDeviceNameWaveVLC;
		public int WavedeviceIndex;
		public Boolean HasCamera = false;
		public Boolean IsArmed = false;
		public int AlarmZoneId = -1;
		public int AlarmAreaId = -1;
		public Boolean HasMotionSensor = false;
		public Boolean HasMicrophone = false;
		public Boolean HasDisplay = false;
		public Boolean HasVideoPlayer = false;
		public String DisplayConnection = "";
		public String DisplayType = "";
		public Boolean RequirePower = false;
		public Boolean IsClosureArmed = false;
		[Description("Edit")]
		public String NearbyZonesIdList = "";//zone id list separated by ;
		[Description("Edit")]
		public string ClosureIdList = "";//separated by ; iopin=2 / for gpio
		//public EnumRelayType ClosureRelayType = EnumRelayType.Undefined;
		public ClosureOpenCloseRelay ClosureOpenCloseRelay;
		public ulong ClosureCounts = 0;
		[Description("Edit")]
		public String TemperatureDeviceId;

		[Description("Edit")]
		public double TemperatureMaxAlarm = 1000;
		[Description("Edit")]
		public double TemperatureMinAlarm = -1000;
		[Description("Edit")]
		public double TemperatureTarget = -1000;
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
		private Boolean m_movementAlert = false, m_movementAlertLast = false;

		public DateTime LastAlarmMovementDateTime = DateTime.MinValue;
		public DateTime LastCamAlertDateTime = DateTime.Now;
		public DateTime LastLocalCommandDateTime = DateTime.MinValue;
		public DateTime LastClosureEventDateTime = DateTime.MinValue;
		public ZoneNotifyState NotifyZoneEventTriggered = ZoneNotifyState.Closed;
		public DateTime LastNotifyZoneEventTriggered;
		protected const double DEFAULT_TEMP_HUM = -1000;

		protected double m_temperature = DEFAULT_TEMP_HUM, m_humidity = DEFAULT_TEMP_HUM;
		protected double m_temperatureLast = DEFAULT_TEMP_HUM, m_humidityLast = DEFAULT_TEMP_HUM;
		protected DateTime m_lastTempSet = DateTime.MinValue, m_lastHumSet = DateTime.MinValue;
		// not serializable, hidden from json
		protected static int m_intervalImmediate, m_intervalRecent, m_intervalPast;


		public ZoneDetails() {
			ClosureOpenCloseRelay = new ClosureOpenCloseRelay(false);
		}

		public ZoneDetails(int p_zoneId, String p_zoneName) {
			ZoneId = p_zoneId;
			ZoneName = p_zoneName;
			ZoneState = MultiZonePlayer.ZoneState.NotInitialised;
			ActivityType = GlobalCommands.nul;
			LoadStateFromIni();
		}

		public override string ToString() {
			return "ID=" + ZoneId + ";Name=" + ZoneName;
		}
		#region getters

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
							 + (ClosureOpenCloseRelay.RelayType != ClosureOpenCloseRelay.EnumRelayType.Undefined ? " " + ClosureState + "@ " + LastClosureEventDateTime : " ")
							 + Title
							 + (Temperature != DEFAULT_TEMP_HUM ? " " + Temperature + "C" : "")
							 + (Humidity != DEFAULT_TEMP_HUM ? " " + Humidity + "%" : "")
							 + (IsPowerOn ? " PowerIsOn" : "");
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
				return (span <= m_intervalImmediate);
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
				return (HasPastMove || (span > m_intervalRecent && span <= m_intervalPast));
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
			get { return ClosureIdList != ""; }
		}
		public Boolean HasTemperatureSensor {
			get {
				RFXDeviceDefinition.RFXDevice device = RFXDeviceDefinition.GetDevice(ZoneId);
				return (TemperatureDeviceId != "" ||
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
				return MZPState.Instance.ZoneDetails.Find(x => x.ParentZoneId == ZoneId && x.HasDisplay) != null
					   || HasDisplay;
			}
		}

		public String ClosureState {
			get {
				return ClosureOpenCloseRelay.RelayState.ToString();
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
				List<ZoneDetails> childs = MZPState.Instance.ZoneDetails.FindAll(x => x.ParentZoneId == ZoneId);
				return childs != null ? childs.Count : 0;
			}
		}

		public ZoneDetails ChildZone(int index) {
			List<ZoneDetails> childs = MZPState.Instance.ZoneDetails.FindAll(x => x.ParentZoneId == ZoneId);
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

		public Boolean IsClosureActivated {
			get {
				return ClosureOpenCloseRelay.RelayContactMade;
			}
		}

		public Boolean HasNotifyMove {
			get { return HasImmediateMove || HasRecentMove; }
		}

		public double Temperature {
			get { return m_temperature; }//return Math.Round(m_temperature, 2).ToString(); }
			set {
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

				if (m_temperature != m_temperatureLast) {
					Utilities.AppendToCsvFile(IniFile.CSV_TEMPERATURE_HUMIDITY, ",", ZoneName,
						Constants.CAPABILITY_TEMP, DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT), Temperature.ToString(), ZoneId.ToString());
					Rules.ExecuteRule(this, "temp=" + m_temperature);
					m_temperatureLast = m_temperature;
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
				m_humidity = value;
				m_lastHumSet = DateTime.Now;

				if (m_humidity != m_humidityLast) {
					Utilities.AppendToCsvFile(IniFile.CSV_TEMPERATURE_HUMIDITY, ",", ZoneName,
						Constants.CAPABILITY_HUM, DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT), Humidity.ToString(), ZoneId.ToString());
					Rules.ExecuteRule(this, "humid=" + m_humidity);
					m_humidityLast = m_humidity;
				}
			}
		}

		public String HumidityAgeInMinutes {
			get { return Math.Round(DateTime.Now.Subtract(m_lastHumSet).TotalMinutes).ToString(); }
		}


		#endregion
		public static void LoadFromIni(ref List<ZoneDetails> zones) {
			Hashtable zoneValues = IniFile.LoadAllIniEntriesByIntKey(IniFile.INI_SECTION_ZONES);

			ZoneDetails zone;

			m_intervalImmediate = Convert.ToInt16(IniFile.PARAM_GENERIC_INTERVAL_SPLIT[1].Split('-')[0]);
			m_intervalRecent = Convert.ToInt16(IniFile.PARAM_GENERIC_INTERVAL_SPLIT[1].Split('-')[1]);
			m_intervalPast = Convert.ToInt16(IniFile.PARAM_GENERIC_INTERVAL_SPLIT[1].Split('-')[2]);

			foreach (int id in zoneValues.Keys) {
				zone = new ZoneDetails(id, zoneValues[id].ToString());

				zones.Add(zone);
			}

			zones.Sort(delegate(ZoneDetails a1, ZoneDetails a2) {
				return a1.ZoneId.CompareTo(a2.ZoneId);
			});
		}

		public void LoadStateFromIni() {
			String json = IniFile.LoadIniEntryByKey(IniFile.INI_SECTION_ZONESTATE, ZoneId.ToString());
			try {
				if (json != "") {
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

					if (zonestorage.ClosureOpenCloseRelay != null) {
						ClosureOpenCloseRelay = zonestorage.ClosureOpenCloseRelay;
						ClosureOpenCloseRelay.RelayType = zonestorage.ClosureOpenCloseRelay.RelayType;
					}
					else {
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
			catch (Exception ex) {
				MLog.Log(ex, "Unable to load zone");
				throw new Exception("ZoneLoad Exception", ex);
			}

		}


		public void SaveStateToIni() {
			MLog.Log(this, "Saving state to ini zone=" + ZoneName);
			//remove fields that generate serialisation problems
			this.Meta = null;

			fastJSON.JSONParameters param = new fastJSON.JSONParameters(); param.UseExtensions = false;
			String json = JSON.Instance.ToJSON(this, param);
			IniFile.IniWriteValuetoFinal(IniFile.INI_SECTION_ZONESTATE, ZoneId.ToString(), json);
			
			//update dependencies if fields have changed
			MZPState.Instance.WDIO.SetPinTypes(this);
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
			SaveStateToIni();
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
			RequirePower = false;
			ZoneState = ZoneState.NotStarted;
		}

		public void ZoneClose() {
			ZoneStop();
			IsActive = false;
			ActivityType = GlobalCommands.nul;
			ZoneState = ZoneState.NotInitialised;

		}

		public Boolean IsNearbyZone(int zoneId) {
			return NearbyZonesIdList.Contains(zoneId.ToString() + ";");
		}

		public static TimeSpan LastMovementAge_Min {
			get {
				return MZPState.Instance.ZoneDetails.Min(x => x.LastMovementAge);
			}
		}
		public static Boolean HasImmediateMove_Once {
			get {
				return MZPState.Instance.ZoneDetails.Find(x => x.HasImmediateMove) != null;
			}
		}
		public static Boolean HasRecentMove_Once {
			get {
				return MZPState.Instance.ZoneDetails.Find(x => x.HasRecentMove) != null;
			}
		}
		public static List<ZoneDetails> HasImmediateMove_All {
			get {
				return MZPState.Instance.ZoneDetails.FindAll(x => x.HasImmediateMove);
			}
		}
		public static List<ZoneDetails> HasRecentMove_All {
			get {
				return MZPState.Instance.ZoneDetails.FindAll(x => x.HasRecentMove);
			}
		}
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

	public class ClosureOpenCloseRelay {
		public enum EnumState {
			Undefined = -1,
			ContactOpen = 0,
			ContactClosed = 1
		}

		public enum EnumRelayType {
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

		public ClosureOpenCloseRelay() {
		}

		public ClosureOpenCloseRelay(bool isRelayContactMade) {
			//Key = key;
			RelayContactMade = isRelayContactMade;
		}

		public String LastChangeAgeAsTimeSpan {
			get { return Utilities.DurationAsTimeSpan(DateTime.Now.Subtract(LastChange)); }
		}
		public EnumState RelayState {
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

		public EnumState GetRelayState(bool isRelayContactMade) {
			switch (RelayType) {
				case EnumRelayType.NormalOpen:
				case EnumRelayType.Button:
					return isRelayContactMade ? ClosureOpenCloseRelay.EnumState.ContactClosed : ClosureOpenCloseRelay.EnumState.ContactOpen;
				case EnumRelayType.NormalClosed:
					return isRelayContactMade ? ClosureOpenCloseRelay.EnumState.ContactOpen : ClosureOpenCloseRelay.EnumState.ContactClosed;
				default:
					//MLog.Log(this, "Error, undefined relay type");
					return EnumState.Undefined;
			}
		}

		public void ResetState() {
			m_relayState = EnumState.Undefined;
			m_relayStateLast = EnumState.Undefined;
		}

		
	}

}

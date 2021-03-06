﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.ComponentModel;
using fastJSON;
using System.IO;

namespace MultiZonePlayer {

	public class ZoneDetails : Singleton, IUPSNotify {
		//private static List<ZoneDetails> m_valueList = new List<ZoneDetails>();
		public int ZoneId = -1;
		[Category("Edit")]
		public String Description="";
		//public Boolean IsActive = false;
		//public int MinutesUntilSleep = -1;
		[Category("Edit")]
		public String SleepHourMin = "";//format HH:MM
		[Category("Edit")]
		public String ZoneName;
		private ZoneState m_zoneState, m_zoneStateLast = MultiZonePlayer.ZoneState.Undefined;
		[Category("Edit")]
		public int ParentZoneId = Constants.NOT_SET;
		[Category("Edit")]
		public String LocationName = "Default Location";
		[Category("Edit")]
		public int PowerIndex = Constants.NOT_SET;
        [Category("Edit")]
		public PowerType PowerType = PowerType.None;
		[Category("Edit")]
		public int PowerOnDelay;
		private DateTime m_lastPowerOn = DateTime.MinValue;
		public String WakeTime = "";
		public String WakeWeekDay = "";
		[Category("Edit"), Description("ID of camera object from iSpy (oid)")]
		public String CameraId = "";
		[Category("Edit"), Description("format widthXheight")]
		public String CameraResolution = "";
		public Boolean HasSpeakers = false;
        public Boolean HasSpeakersCapability = false;
		[Category("Edit")]
		public int DefaultVolumePercent;
		[Category("Edit")]
		public String OutputKeywords="";
		[Category("Edit")]
		public String OutputDeviceUserSelected;
        [Category("Edit"), Description("True if the audio device is a bluetooth device or False. When True no BT scans will be performed when playing")]
        public Boolean IsBluetoothOutputDevice = false;
		[Category("Edit")]
		public ZoneType Type = ZoneType.Undefined;
		public String OutputDeviceNameWaveVLC;
		public int WavedeviceIndex;
		[Category("Edit"), Description("the raw input device name as provided by Windows, eg: \\\\?\\HID#VID_0A81&PID_0101&MI_00#7&37e1499b&0&0000#{884b96c3-56ef-11d1-bc8c-00a0c91405dd}")]
		public String ControlDeviceName = "";
		//public Boolean HasCamera = false;
		public Boolean IsArmed = false;
		[Category("Edit"), Description("ID from winload/paradox system OR PinIndex from custom GPIO beagle/PI software")]
		public int AlarmZoneId = Constants.NOT_SET;
		[Category("Edit"), Description("ID used to arm an entire area")]
		public int AlarmAreaId = Constants.NOT_SET;
		public Boolean HasMotionSensor = false;
		public Boolean HasMicrophone = false;
		public Boolean HasDisplay = false;
		public Boolean HasVideoPlayer = false;
        [Category("Edit"), Description("COMport for the display, e.g. COM1 for an old TV connected to serial")]
		public String DisplayConnection = "";
        [Category("Edit"), Description("Display type, for example LGTV")]//TODO must be refactored
		public String DisplayType = "";
		public Boolean RequirePowerForced = false;
		[Category("Edit")]
		public String NearbyZonesIdList = "";//zone id list separated by ;
		[Category("Edit"), Description("list of io ports. for wdio use " + WDIO.WDIO_ATTRIB_BUTTON +"x;"+ WDIO.WDIO_ATTRIB_OUTPUT + "x;"+WDIO.WDIO_ATTRIB_SWITCH+"x;")]
		public string ClosureIdList = "";//separated by ; iopin=2 / for gpio
		//public ClosureOpenCloseRelay ClosureOpenCloseRelay;
		[Category("Edit")]
		public ulong ClosureCount = 0;
		
		public Boolean IsClosureArmed = false;
		[Category("Edit")]
		public EnumClosureType ClosureType = EnumClosureType.Undefined;
		private Boolean m_isClosureContactMade = false, m_isClosureContactMadeLast = false;
		public EnumRelayState RelayState = EnumRelayState.Undefined;
		[Category("Edit")]
		public EnumRelayType RelayType = EnumRelayType.Undefined;

		[Category("Edit"), Description("Set type if zone represents an utility")]
		public EnumUtilityType UtilityType = EnumUtilityType.Undefined;
		public ulong PulseCountInTimeSample = 0;
		//public ulong CounterCountInSample = 0;
		[Category("Edit"), Description("How often pulses or counters are read / sampling frequency")]
		public int PulseSampleMinutesFrequency = 1;//in minutes
		[Category("Edit")]
		public double PulseSubUnits = 1;//how many subunits in a main unit 100 e.g. flashes in a kwh or 10 pulses per liter
		[Category("Edit")]
		public double PulseMainUnitsCount = 0;//main units, e.g. 3 kwh
		//[Category("Edit")]
		//public double CounterMainUnitsCount = 0;//main units, e.g. 4 kwh
		[Category("Edit")]
		public string CounterPageNameToInclude = "1";//counter page name to consider, ignore rest (e.g. 1)
		[Category("Edit"), Description("IO IDs to include when checking for contact state (so not OUTPUT), comma separated. DS2406 has 2 ports, so value can be 0,1. Rest are ignored")]
		public string ClosureLevelNameToInclude = "0,1";//counter page name/ID to consider, ignore rest (e.g. 0,1)
		public string ClosuresCmdContactOnList = "";//list of commands to be executed with closure IDs that must be kept open, comma separated, e.g. for ds2406.
		private string m_closuresContactStateOnList = "";//list of closure state IDs that are read from device as open, comma separated, e.g. for ds2406.
		[Category("Edit")]
		public DateTime PulseMainUnitsStartDate = DateTime.Now;
		public double PulseLastMainUnitsCount = 0;
		//public double CounterLastMainUnitsCount = 0;
		public ulong LastCounterCount = 0;
		public DateTime LastPulseSamplingStart = DateTime.Now;
        private DateTime m_lastCounterWriteToDB = DateTime.MinValue;
        private Boolean m_counterZeroUnitNeedsRecord = true;
		//private DateTime m_lastCounterSamplingStart = DateTime.Now;
		[Category("Edit")]
		public String PulseMainUnitType = "";
		[Category("Edit"), Description("Max number of utility units that can be consumed / minute. Safety check for malfunctioning 1-wire. -1 if limit disabled.")]
		public int MaxUtilityUnitsPerMinute = Constants.NOT_SET;

		[Category("Edit"), Description("OneWire device id for temperature sensors. Use UPPERCASE. Format for multiple entries is <ADDRESS1XXXXX>[sensorname1-optional];<ADDRESS2XXXXX>[sensorname2-optional];etc.")]
		public String TemperatureDeviceId="";
        [Category("Edit"), Description("OneWire device descriptions for temperature sensors. Format for multiple entries is sensorname1;sensorname2;etc.")]
        public String TemperatureDeviceIdDescriptions = "";
		[Category("Edit"), Description("OneWire device id for devices that supports IO operations. E.g. DS2406, DS2408")]
		public String OneWireIODeviceId="";

        [Category("Edit"), Description("Chose UPS type, leave undefined if n/a")]
        public UPSType UPSType = UPSType.Undefined;
        [Category("Edit"), Description("COM port for UPS if this zone is an UPS. E.g. COM1")]
        public String UPSSerialPortAddress="";

        public double InputVoltage;
        
		public int WDIORelayOutputPinIndex = Constants.NOT_SET;
		public int WDIOButtonPinIndex = Constants.NOT_SET;
		public int WDIOSwitchPinIndex = Constants.NOT_SET;

		//[Category("Edit")]
		//public String OtherOneWireDeviceIdList;//separated by ;
		private List<TemperatureEntry> m_temperatureList = new List<TemperatureEntry>();

		[Category("Edit")]
		public double TemperatureMaxAlarm = 1000;
		[Category("Edit")]
		public double TemperatureMinAlarm = -1000;
		[Category("Edit")]
		public double TemperatureTarget = -1000;
		[Category("Edit"), Description("Number of digits for temp reading. Set -1 for default device settings.")]
		public int TemperatureResolutionDigits = Constants.NOT_SET;
		[Category("Edit"), Description("Max number of temp units variation / minute. Safety check for malfunctioning 1-wire. -1 if limit disabled.")]
		public int MaxTempUnitsVariationBetweenReads = Constants.NOT_SET;
        private bool m_maxtempVariationDetected = false;

		[Category("Edit"), Description("Index of voltage to be tracked, usually=2 in a DS2438 1 wire index")]
		public int VoltageSensorIndex = 2;
		[Category("Edit"), Description("Min expected voltage value, anything else is ignored. usefull to prevent 1 wire errors. default = -1")]
		public double MinAllowedVoltageValue = Constants.NOT_SET;
		[Category("Edit"), Description("Max expected voltage value, anything else is ignored. usefull to prevent 1 wire errors. default = -1")]
		public double MaxAllowedVoltageValue = Constants.NOT_SET;

		public Boolean ScheduledHeatActive = false;
		[Description("Default cron schedule, if no other conditional schedules are defined")]
        public String CronSchedule = "";
		[Category("Edit")]
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
		public MediaItemBase MediaItem;

		public Boolean CameraAlertActive = true;
		//private List<PictureSnapshot> m_pictureList = new List<PictureSnapshot>();
		private Boolean m_movementAlert = false, m_movementAlertLast = false;

		public DateTime LastAlarmMovementDateTime = DateTime.MinValue;
		public DateTime LastCamAlertDateTime = DateTime.Now;
		public DateTime LastLocalCommandDateTime = DateTime.MinValue;
		public DateTime LastClosureEventDateTime = DateTime.MinValue;
		private ZoneNotifyState m_notifyZoneEventTriggered = ZoneNotifyState.Closed;
		private int m_closureCountSinceArmed = 0, m_alarmsensorCountSinceArmed = 0, m_cameraCountSinceArmed = 0;

		
		public DateTime LastNotifyZoneEventTriggered = DateTime.MinValue;

		private double[] m_voltage = new double[5] { Constants.NOT_SET, Constants.NOT_SET, Constants.NOT_SET, Constants.NOT_SET, Constants.NOT_SET};
		protected const double DEFAULT_TEMP_HUM = -1000;

		protected double m_humidity = DEFAULT_TEMP_HUM; //m_temperature = DEFAULT_TEMP_HUM, 
		protected Boolean m_lowTempReached = true;
		//protected double m_temperatureLast = DEFAULT_TEMP_HUM, m_humidityLast = DEFAULT_TEMP_HUM;
		protected DateTime m_lastTempSet = DateTime.MinValue, m_lastHumSet = DateTime.MinValue;
		protected DateTime m_lastRunningDateTime = DateTime.MinValue;
		// not serializable, hidden from json
		protected static int m_intervalImmediate, m_intervalRecent, m_intervalPast;
		protected static List<Singleton> m_valueList = new List<Singleton>();
		protected Boolean m_hasOneWireTempSensor = false,m_hasOneWireIODevice=false,m_hasOneWireVoltageSensor=false,m_hasOneWireCounterDevice=false;
        protected Boolean m_hasUPSTemperatureSensor = false, m_hasUPSVoltageSensor;
		protected int m_OneWireIOPortCount = 0;
		protected ZoneGeneric m_zoneGeneric;
		private List<SensorDevice> m_sensorList = new List<SensorDevice>();
		private bool m_waitForControlDeviceSetup;

		
		

		public ZoneDetails() {
			//ClosureOpenCloseRelay = new ClosureOpenCloseRelay(false);
			if (m_zoneGeneric == null)
				m_zoneGeneric = new ZoneGeneric(this);
			ZoneId = base.Id;
		}

		public ZoneDetails(int p_zoneId, String p_zoneName) {
			ZoneId = p_zoneId;
			ZoneName = p_zoneName;
			ZoneState = MultiZonePlayer.ZoneState.NotInitialised;
			ActivityType = GlobalCommands.nul;
			if (m_zoneGeneric == null)
				m_zoneGeneric = new ZoneGeneric(this);
			LoadStateFromIni();

		}
		public String Name {//for web edit, match singleton Name field requirements
			get {return ZoneName;}
		}
		/*public new int Id {//for web edit, match singleton Name field requirements
			get { return ZoneId; }
		}*/
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

		public ZoneGeneric ZoneGeneric {
			get { return m_zoneGeneric; }
		}
		#region getters

		public ZoneNotifyState NotifyZoneEventTriggered {
			get { return m_notifyZoneEventTriggered; }
			set { m_notifyZoneEventTriggered = value; }
		}

		public List<SensorDevice> SensorList {
			get { return m_sensorList; }
			//set { m_sensorList = value; }
		}

		public void AddSensor(SensorDevice sensor) {
			if (m_sensorList.Find(x => x == sensor) == null)
				m_sensorList.Add(sensor);
		}

		public void RemoveSensor(SensorDevice sensor) {
			m_sensorList.Remove(sensor);
		}
		public bool WaitForControlDeviceSetup {
			get { return m_waitForControlDeviceSetup; }
			set { m_waitForControlDeviceSetup = value; }
		}
		public Boolean IsRelayInNormalState {
			get {
				return GetClosureIsRelayInNormalState(m_isClosureContactMade);
			}
		}
		public Boolean IsClosureContactMade {
			get { return m_isClosureContactMade; }
			set {
				m_isClosureContactMade = value;
				ClosureCount++;
				if (GetClosureIsRelayInNormalState(value))
					RelayState = EnumRelayState.ContactClosed;
				else
					RelayState = EnumRelayState.ContactOpen;
				
				MLog.Log(this, "Contact event=" + value + " on zone="+ ZoneName +" relaystate=" + RelayState + " relaytype="+RelayType);

				LastClosureEventDateTime = DateTime.Now;
				if (m_isClosureContactMade != m_isClosureContactMadeLast) {
					ScriptingRule.ExecuteRule(this, "isclosurecontact=" + m_isClosureContactMade);
					m_isClosureContactMadeLast= m_isClosureContactMade;
				}
			}
		}

		public Boolean IsClosureCmdIOOn(int id) {
			return ClosuresCmdContactOnList.Contains(id + ",");
		}


		public void SetClosureCmdIO(int id, Boolean setOn){
			if (setOn && !IsClosureCmdIOOn(id)) {
				ClosuresCmdContactOnList += id + ",";
			}
			else
				if (!setOn && IsClosureCmdIOOn(id))
					ClosuresCmdContactOnList = ClosuresCmdContactOnList.Replace(id + ",", "");
		}

		public Boolean IsClosureStateIOOn(int id) {
			return m_closuresContactStateOnList.Contains(id + ",");
		}


		public void SetClosureStateIO(int id, Boolean stateOn) {
			if (stateOn && !IsClosureStateIOOn(id)) {
				m_closuresContactStateOnList += id + ",";
			}
			else
				if (!stateOn && IsClosureStateIOOn(id))
					m_closuresContactStateOnList = m_closuresContactStateOnList.Replace(id + ",", "");
		}

		private Boolean GetClosureIsRelayInNormalState(Boolean contactMade) {
			if (contactMade && RelayType == EnumRelayType.NormalOpen) {
				return false;
			}
			else
				return true;
		}
		public Boolean IsClosureRelayInNormalState(int id) {
			SensorDevice dev = m_sensorList.Find(x => x.HasClosure && x.Zone.ZoneId==ZoneId);
			if (dev != null) {
				bool level = dev.Level[id];
				return GetClosureIsRelayInNormalState(level);
			}
			else
				return false;
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
					SaveEntryToIni();
				}
				else {
					PulseCountInTimeSample++;
				}
			}
			else {
				MLog.Log(this, "Ignoring closure level " + level);
			}//ignoring other closure levels
		}
		public void RecordCounter(string id, ulong counter, DateTime datetime) {
			if (id.Contains(CounterPageNameToInclude)) {
                HasOneWireCounterDevice = true;
				double lapsedMinutes = datetime.Subtract(LastPulseSamplingStart).TotalMinutes;
				if ( lapsedMinutes >= PulseSampleMinutesFrequency) {
                    if (PulseCountInTimeSample == 0) {//when program is loaded or no previous counter record happened
                        //MLog.Log("debug this");
                        PulseCountInTimeSample = counter - LastCounterCount;
                        LastCounterCount = counter;
                    }
					PulseLastMainUnitsCount = (double)PulseCountInTimeSample / PulseSubUnits;
					/*if (PulseLastMainUnitsCount > 1) {
						Alert.CreateAlert("WARNING: Large count, counter=" + counter + " deltaPulses=" + PulseCountInTimeSample,
							this, false, Alert.NotificationFlags.NotifyUserAfterXMinutes, 1);
					}*/
					if (lapsedMinutes > 2 * PulseSampleMinutesFrequency){
						Alert.CreateAlert("Long pulse counter period detected, minutes=" + lapsedMinutes
							+ " counterdelta=" + PulseCountInTimeSample + " in zone " + ZoneName + " lastpulsesampling="+LastPulseSamplingStart, true);
					}

                    if ((PulseCountInTimeSample != 0 || m_counterZeroUnitNeedsRecord) || datetime.Subtract(m_lastCounterWriteToDB).TotalHours >= 1) {//do not write if no pulses were recorded, but write one record per hour just in case
                        //record first 0 consumption then stop recording zeros for a while
                        m_counterZeroUnitNeedsRecord = (PulseCountInTimeSample != 0);
                    
                        UtilityCost utilCost = UtilityCost.UtilityCostList.Find(x => x.Name.Equals(UtilityType));
                        // TODO: split consumption evenly when PC down for long
                        //PulseMainUnitsCount += PulseLastMainUnitsCount;
                        double unitCost, cost, watts, pulseUnitCountPerMissedFrame, pulseUnitsPerMinute;
                        int missedFrames = (int)Math.Round(lapsedMinutes / PulseSampleMinutesFrequency, 0);
                        double totalLoggedPulses = 0, pulseIncrement = 0;
                        pulseUnitCountPerMissedFrame = PulseLastMainUnitsCount / missedFrames;
                        pulseUnitsPerMinute = pulseUnitCountPerMissedFrame / PulseSampleMinutesFrequency;
                        if (MaxUtilityUnitsPerMinute != -1 && pulseUnitsPerMinute > MaxUtilityUnitsPerMinute) {
                            Alert.CreateAlert("Large utility units consumption registered, possible error, skipping, for zone=" + ZoneName
                                + " units per minute=" + pulseUnitsPerMinute + ", limit is=" + MaxUtilityUnitsPerMinute, true);
                        }
                        else {
                            int passes = 0;
                            do {
                                pulseIncrement = Math.Min(PulseLastMainUnitsCount - totalLoggedPulses, pulseUnitCountPerMissedFrame);
                                totalLoggedPulses += pulseIncrement;
                                PulseMainUnitsCount += pulseIncrement;
                                unitCost = 0;
                                cost = 0;
                                watts = -1;
                                if (utilCost != null) {
                                    unitCost = utilCost.UnitCost;
                                    switch (UtilityType) {
                                        case EnumUtilityType.Electricity:
                                            //TODO: problem here, high watt value recorded after split of power loss delta
                                            watts = 1000 * pulseUnitCountPerMissedFrame / (PulseSampleMinutesFrequency / 60d);
                                            break;
                                        case EnumUtilityType.Gas:
                                        case EnumUtilityType.Water:
                                            break;
                                        default:
                                            MLog.Log(this, "WARNING unprocessed utility type " + UtilityType);
                                            break;
                                    }
                                    cost = pulseUnitCountPerMissedFrame * unitCost;
                                }
                                LastPulseSamplingStart = LastPulseSamplingStart.AddMinutes(PulseSampleMinutesFrequency);
                                DB.WriteRecord(DB.TABLE_COUNTER, DB.COL_COUNTER_DATETIME, LastPulseSamplingStart.ToString(Constants.DATETIME_DB_FORMAT),
                                    DB.COL_COUNTER_ZONEID, ZoneId,
                                    DB.COL_COUNTER_MAINUNIT, pulseUnitCountPerMissedFrame, DB.COL_COUNTER_SECONDARYUNIT, watts,
                                    DB.COL_COUNTER_UTILITYTYPE, UtilityType.ToString(), DB.COL_COUNTER_TOTALMAINUNIT, PulseLastMainUnitsCount,
                                    DB.COL_COUNTER_TOTALCOUNTER, counter, DB.COL_COUNTER_COST, cost);
                                //Utilities.AppendToCsvFile(IniFile.CSV_UTILITIES, ",", ZoneName, LastPulseSamplingStart.ToString(IniFile.DATETIME_FULL_FORMAT),
                                  //  PulseLastMainUnitsCount.ToString(), ZoneId.ToString(), UtilityType.ToString(), pulseUnitCountPerMissedFrame.ToString(),
                                    //cost.ToString(), unitCost.ToString(), watts.ToString(), counter.ToString(), PulseMainUnitsCount.ToString());
                                passes++;
                                m_lastCounterWriteToDB = datetime;
                            }
                            while (totalLoggedPulses < PulseLastMainUnitsCount && passes < 500);//fail safe for very lpng shutdowns
                            if (passes > 1) {
                                Alert.CreateAlert("Splited large counter consumption in zone " + ZoneName + " passes=" + passes, true);
                            }
                        }
                    }
                    LastPulseSamplingStart = datetime;
                    PulseCountInTimeSample = 0;
					SaveEntryToIni();//save in case of power outage
				}
				else {
					ulong units = counter - LastCounterCount;
					if (units < 0) {
						Alert.CreateAlert("WARNING, counter reset at LastCount="+LastCounterCount, true);
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
			get { return PulseMainUnitsCount * UtilityCost.UtilityCostList.Find(x => x.Name.Equals(UtilityType)).UnitCost; }
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

		public Boolean HadRecentRunState {
			get {
				return DateTime.Now.Subtract(m_lastRunningDateTime).TotalMinutes 
					< Convert.ToInt16(IniFile.PARAM_RECENT_RUN_INTERVAL_MINUTES[1]); 
			}
		}

		public ZoneState ZoneState {
			get { return m_zoneState; }
			set {
				m_zoneState = value;
				if (value == MultiZonePlayer.ZoneState.Running)
					m_lastRunningDateTime = DateTime.Now;
				if (m_zoneState != m_zoneStateLast) {
					ScriptingRule.ExecuteRule(this, "zonestate=" + m_zoneState);
					m_zoneStateLast = m_zoneState;
				}
			}
		}
		public Boolean HasCamera {
			get { return CameraId != ""; }
		}
		public int CameraCountSinceArmed {
			get { return m_cameraCountSinceArmed; }
		}

		public int AlarmsensorCountSinceArmed {
			get { return m_alarmsensorCountSinceArmed; }
		}

		public int ClosureCountSinceArmed {
			get { return m_closureCountSinceArmed; }
		}
		public void IncreaseCameraCountIfArmed() {
			if (MZPState.Instance.SystemAlarm.IsArmed)
				m_cameraCountSinceArmed++;
		}
		public void IncreaseAlarmsensorCountIfArmed() {
			if (MZPState.Instance.SystemAlarm.IsArmed)
				m_alarmsensorCountSinceArmed++;
		}
		public void IncreaseClosureCountIfArmed() {
			if (MZPState.Instance.SystemAlarm.IsArmed)
				m_closureCountSinceArmed++;
		}
		public void AreaArmedActions() {
			m_closureCountSinceArmed = 0;
			m_cameraCountSinceArmed = 0;
			m_alarmsensorCountSinceArmed = 0;
		}
		public Boolean MovementAlert {
			get { return m_movementAlert; }
			set {
				m_movementAlert = value;
				if (m_movementAlert != m_movementAlertLast) {
					ScriptingRule.ExecuteRule(this, "movement=" + m_movementAlert);
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
				//if presence contact is active means user is at home (e.g. on bed)
				bool isPresenceContactMade = (ClosureType == EnumClosureType.PresenceContact) && IsClosureContactMade;
				return (span <= m_intervalImmediate) || isPresenceContactMade;
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
				return (Temperature > TemperatureMaxAlarm) || (Temperature < TemperatureMinAlarm);
			}
		}

		public Boolean IsPowerOn {
			get { return MZPState.Instance.PowerControlIsOn(ZoneId); }
		}
		public String PowerOnAge {
			get {
				if (m_lastPowerOn != DateTime.MinValue)
					return Utilities.DurationAsTimeSpan(DateTime.Now.Subtract(m_lastPowerOn));
				else
					return "not on";
			}
		}
		public void PowerControlOn() {
			bool switchedOn = false;
			if (HasPowerCapabilities) {
                switch (this.PowerType){
                    case MultiZonePlayer.PowerType.Denkovi:
                        if (!MZPState.Instance.PowerControlDenkovi.IsPowerOn(ZoneId)) {
						    switchedOn = true;
					    }
					    MZPState.Instance.PowerControlDenkovi.PowerOn(ZoneId);
					    m_lastPowerOn = DateTime.Now;
                        break;
                    case MultiZonePlayer.PowerType.Numato:
                        if (!MZPState.Instance.PowerControlNumato.IsPowerOn(ZoneId)) {
						    switchedOn = true;
					    }
						MZPState.Instance.PowerControlNumato.PowerOn(ZoneId);
						m_lastPowerOn = DateTime.Now;
                        break;
                    case MultiZonePlayer.PowerType.Relay:
                        CommandResult res = new CommandResult();
                        ZoneGeneric.ProcessAction(GlobalCommands.closureclose, new ValueList(GlobalParams.id, WDIORelayOutputPinIndex.ToString(),CommandSources.system), ref res);
                        break;
                    case MultiZonePlayer.PowerType.RemoteRelayPI:
                        if (!MZPState.Instance.PowerControlRemoteRelayPI.IsPowerPinOn(PowerIndex)) {
                            switchedOn = true;
                            MZPState.Instance.PowerControlRemoteRelayPI.PowerPinOn(PowerIndex);
                        }
                        break;
                }
				if (switchedOn) {
					Utilities.AppendToCsvFile(IniFile.CSV_CLOSURES, ",", ZoneName, this.PowerType.ToString(),
						DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT), "PowerOn", ZoneId.ToString(),
						Constants.EVENT_TYPE_POWER);
				}
			}
			else {
				MLog.Log(this, "Error, Power ON command sent to zone without power cap: " + ZoneName);
			}
		}
		public void PowerControlOff() {
            try {
                bool switchedOff = false;
                RequirePowerForced = false;
                if (HasPowerCapabilities) {
                    switch (this.PowerType) {
                        case (MultiZonePlayer.PowerType.Denkovi):
                            if (MZPState.Instance.PowerControlDenkovi.IsPowerOn(ZoneId)) {
                                switchedOff = true;
                            }
                            MZPState.Instance.PowerControlDenkovi.PowerOff(ZoneId);
                            m_lastPowerOn = DateTime.MinValue;
                            break;
                        case MultiZonePlayer.PowerType.Numato:
                            if (MZPState.Instance.PowerControlNumato.IsPowerOn(ZoneId)) {
                                switchedOff = true;
                            }
                            MZPState.Instance.PowerControlNumato.PowerOff(ZoneId);
                            m_lastPowerOn = DateTime.MinValue;
                            break;
                        case MultiZonePlayer.PowerType.Relay:
                            if (ZoneGeneric != null) {
                                CommandResult res = new CommandResult();
                                ZoneGeneric.ProcessAction(GlobalCommands.closureopen, new ValueList(GlobalParams.id, WDIORelayOutputPinIndex.ToString(), CommandSources.system), ref res);
                            }
                            break;
                        case MultiZonePlayer.PowerType.RemoteRelayPI:
                            MZPState.Instance.PowerControlRemoteRelayPI.PowerPinOff(PowerIndex);
                            break;
                    }

                    if (switchedOff) {
                        Utilities.AppendToCsvFile(IniFile.CSV_CLOSURES, ",", ZoneName, this.PowerType.ToString(),
                            DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT), "PowerOff", ZoneId.ToString(),
                            Constants.EVENT_TYPE_POWER);
                    }
                }
                else {
                    MLog.Log(this, "Error, Power OFF command sent to zone without power cap: " + ZoneName);
                }
            }
            catch (Exception ex) { 
                MLog.Log(ex, this, "Err powerOff zone "+ZoneName); 
            }
		}
		public Boolean HasPowerCapabilities {
			get {
				return //PowerIndex != -1 && 
                    (PowerType != PowerType.None) && (PowerType!= PowerType.Undefined);
			}
		}
		public Boolean HasClosures {
			get { return ClosureIdList != "" || HasOneWireIODevice; }
		}
		public Boolean HasRelay {
			get {
				return HasWDIORelay || HasOneWireIODevice;
			}
		}
		public Boolean HasWDIORelay {
			get {
				return WDIORelayOutputPinIndex != Constants.NOT_SET;
			}
		}
		public Boolean HasOneWireTemperatureSensor {
			get { return m_hasOneWireTempSensor; }
			set { m_hasOneWireTempSensor = value; }
		}

		public Boolean HasOneWireIODevice {
			get { return m_hasOneWireIODevice; }
			set { m_hasOneWireIODevice = value; }
		}
		public int OneWireIOPortCount {
			get { return m_OneWireIOPortCount; }
			set { m_OneWireIOPortCount = value; }
		}

		public Boolean HasOneWireCounterDevice {
			get { return m_hasOneWireCounterDevice; }
			set { m_hasOneWireCounterDevice = value; }
		}

		public Boolean HasOneWireVoltageSensor {
			get { return m_hasOneWireVoltageSensor; }
			set { m_hasOneWireVoltageSensor = value; }
		}

		public Boolean HasTemperatureSensor {
			get {
				double tempAge = DateTime.Now.Subtract(m_lastTempSet).TotalHours;
				RFXDeviceDefinition.RFXDevice device = RFXDeviceDefinition.GetDevice(ZoneId);
                Boolean result = (tempAge <= 1 && (HasOneWireTemperatureSensor || m_hasUPSTemperatureSensor ||
                        (device != null && device.DeviceType == RFXDeviceDefinition.DeviceTypeEnum.temp_hum)));
				return result;
			}
		}
		public Boolean HasHumiditySensor {
			get {
				RFXDeviceDefinition.RFXDevice device = RFXDeviceDefinition.GetDevice(ZoneId);
				return (device != null && device.DeviceType == RFXDeviceDefinition.DeviceTypeEnum.temp_hum);
			}
		}

        public Boolean HasVoltageSensor {
            get {
                Boolean result = (HasOneWireVoltageSensor || m_hasUPSVoltageSensor);
                return result;
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
				if (Temperature < TemperatureTarget) {
					m_lowTempReached = true;
					return ScheduledHeatActive && HasTemperatureSensor;
				}

				if (Temperature < TemperatureTargetMaxTreshhold) {
					return ScheduledHeatActive && HasTemperatureSensor && m_lowTempReached;
				}
				else {
					m_lowTempReached = false;
					return false;
				}
			}
		}
		public String RequirePowerDetails {
			get{
				int closeperiod = Convert.ToInt16(IniFile.PARAM_POWER_CLOSE_AFTER_ACTIVITY_PERIOD[1]);
				String res = "powerforced=" + RequirePowerForced;
				res += " isactive="+IsActive;
				res +=" lastmoveagemin="+LastMovementAge.TotalMinutes;
				res +=" closeperiod="+closeperiod;
				res +=" lastcomandsec="+LastLocalCommandAgeInSeconds;
				res +=" zonestate="+ZoneState;
				res +=" hadrecentrun="+HadRecentRunState;
				res +=" activity="+ActivityType;
				res +=" type="+Type;

				bool powerforparentheat = false;
				if (Type == ZoneType.Heat) {//for zones with child heat component
					ZoneDetails parent = ZoneDetails.GetZoneById(ParentZoneId);
					if (parent!=null)
						powerforparentheat = parent.RequireHeat;
				}
				bool powerforchild = false;
				List<ZoneDetails> childs = ZoneDetails.ZoneDetailsList.FindAll(x => x.ParentZoneId == ZoneId && x.ZoneState == ZoneState.Running);
				if (childs != null && childs.Count>0)
					powerforchild = true;

				res +=" powerforparentheat="+powerforparentheat;
				res += " powerforchild=" + powerforchild;
				res += " haspowercapab=" + HasPowerCapabilities;
				return res;	
			}
		}
		public Boolean RequirePower {
			get {
				if (HasPowerCapabilities) {
                    int closeperiod = Convert.ToInt16(IniFile.PARAM_POWER_CLOSE_AFTER_ACTIVITY_PERIOD[1]);
                    if (ActivityType == GlobalCommands.music)
                        closeperiod = 2 * closeperiod;//do not close the music that fast
                        
					bool powerfortoolong = (RequirePowerForced || IsActive)
						&& (LastMovementAge.TotalMinutes > closeperiod)
						&& (LastLocalCommandAgeInSeconds > 60 * closeperiod);

					bool powerforparentheat = false;

					if (Type == ZoneType.Heat) {//for zones with child heat component
						ZoneDetails parent = ZoneDetails.GetZoneById(ParentZoneId);
						if (parent != null)
							powerforparentheat = parent.RequireHeat;
					}
					bool powerforchild = false;
					List<ZoneDetails> childs = ZoneDetails.ZoneDetailsList.FindAll(x => x.ParentZoneId == ZoneId && x.ZoneState == ZoneState.Running);
					if (childs != null && childs.Count > 0)
						powerforchild = true;

					bool regularstate = powerforparentheat
						|| (ZoneState == MultiZonePlayer.ZoneState.Running)
						|| HadRecentRunState
						|| RequirePowerForced
						|| powerforchild;

					bool keepOn = (ActivityType == GlobalCommands.tv) || (Type == ZoneType.Heat);

					return regularstate && ((!powerfortoolong) || keepOn);
				}
				else
					return false;
			}
		}

		public Boolean IsInactiveToBeStopped {
			get { 
				Boolean toBeStopped = false;
				int duration = 0;
				int musicInactivity = 60, videoInactivity = 60, tvInactivity = 60, inactiveZone = 1; //default values
				int.TryParse(IniFile.PARAM_CLOSE_ACTIVE_ZONE_MUSIC[1], out musicInactivity);
				int.TryParse(IniFile.PARAM_CLOSE_ACTIVE_ZONE_VIDEO[1], out videoInactivity);
				int.TryParse(IniFile.PARAM_CLOSE_ACTIVE_ZONE_TV[1], out tvInactivity);
				int.TryParse(IniFile.PARAM_CLOSE_INACTIVE_ZONE[1], out inactiveZone);


                bool isAlarm = false;
                if (ZoneGeneric!=null && ZoneGeneric.MainZoneActivity!= null){
                    if (ZoneGeneric.MainZoneActivity.GetType()==typeof(MusicActivity)){
                        isAlarm = ((MusicActivity)ZoneGeneric.MainZoneActivity).IsAlarm;
                    }
                }
                
				//close if no recent activity detected on an active zone, except tv & video
				if (ActivityType != GlobalCommands.nul) {
					if (IsActive) {
						switch (ActivityType) {
							case GlobalCommands.xbmc:
								duration = videoInactivity;
								break;
							case GlobalCommands.tv:
								duration = tvInactivity;
								break;
							case GlobalCommands.music:
							case GlobalCommands.streammp3:
							case GlobalCommands.radio:
								duration = musicInactivity;
								break;
							default:
								duration = 60;
								break;
						}
						if (DateTime.Now.Subtract(LastLocalCommandDateTime).TotalMinutes >= duration && (LastMovementAge.TotalMinutes > 5 && !isAlarm)) {
							toBeStopped = true;
							MLog.Log(this, "zone to be CLOSED is active but no move for 5 mins and for too long than duration mins="+duration);
						}
					}

					if (!HadRecentRunState && !IsActive) {
						if (DateTime.Now.Subtract(LastLocalCommandDateTime).TotalMinutes >= inactiveZone){
							toBeStopped = true;
							MLog.Log(this,"zone to be CLOSED is not recent and not active");
						}
					}
				}
				return toBeStopped;
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
			get {
				if (m_temperatureList.Count > 0)
					return m_temperatureList[0].Temperature;
				else
					return DEFAULT_TEMP_HUM;
			}//return Math.Round(m_temperature, 2).ToString(); }
		}

		public String DisplayTemperature {
			get {
				String display="";
				foreach (TemperatureEntry temp in m_temperatureList){
					display += "/"+temp.Temperature;
				}
				if (display.Length >= 1) 
					return display.Substring(1);
				else 
					return display;
			}
		}
		private int GetTemperatureDevicePosition(String deviceId) {
			string[] atoms = TemperatureDeviceId.ToLower().Split(Constants.MULTI_ENTRY_SEPARATOR);
			deviceId = deviceId.ToLower();
			int pos=0;
			for (int i = 0; i < atoms.Length; i++) {
				if (atoms[i].Contains(deviceId)) {
					pos = i;
					break;
				}
			}
			return pos;
		}
		//position in the 
		public string GetTemperatureDeviceName(String deviceId) {
			int position = GetTemperatureDevicePosition(deviceId);
			string[] atoms = TemperatureDeviceIdDescriptions.ToLower().Split(Constants.MULTI_ENTRY_SEPARATOR);
            if (position < atoms.Length)
                return atoms[position];
            else
                return "";
		}
		public void SetTemperature(double value, String deviceId, DateTime datetime) {
			int position = GetTemperatureDevicePosition(deviceId);
			string devicename = GetTemperatureDeviceName(deviceId);
			TemperatureEntry temp = m_temperatureList.Find(x => x.DeviceId == deviceId);
			if (temp == null) {
				temp = new TemperatureEntry(DEFAULT_TEMP_HUM, deviceId, devicename);
				m_temperatureList.Add(temp);
			}
			else {
				temp.DeviceName = devicename;
			}
			//device with position == 0 is representative for the current zone
			if (position == 0 && Temperature != DEFAULT_TEMP_HUM &&  MaxTempUnitsVariationBetweenReads!=Constants.NOT_SET) {
                if ((Math.Abs(Temperature - value) > MaxTempUnitsVariationBetweenReads)) {
                    if (m_maxtempVariationDetected == false) {//first read will be ignored
                        Alert.CreateAlert("Too big variance in temperature detected, last val=" + Temperature + " current=" + value
                            + " max variation set is=" + MaxTempUnitsVariationBetweenReads + " in zone " + ZoneName, true);
                        m_maxtempVariationDetected = true;
                        return;
                    }
                    else {//second read will be accepted
                        m_maxtempVariationDetected = false;
                        Alert.CreateAlert("Variance in temperature detected and stable after 1st read, so accepted, last val=" + Temperature + " current=" + value
                            + " max variation set is=" + MaxTempUnitsVariationBetweenReads + " in zone " + ZoneName, true);

                    }
                }
			}
			if (temp.Temperature != value) {
                //DB.Temperature.WriteRecord(DateTime.Now, ZoneId, value, position, deviceId);
                DB.WriteRecord(DB.TABLE_TEMPERATURE, DB.COL_TEMPERATURE_DATETIME, datetime.ToString(Constants.DATETIME_DB_FORMAT), 
                        DB.COL_TEMPERATURE_ZONEID, ZoneId, DB.COL_TEMPERATURE_VALUE, value, DB.COL_TEMPERATURE_SENSORPOSITION, position,
                        DB.COL_TEMPERATURE_SENSORID, deviceId, DB.COL_TEMPERATURE_SENSORNAME, devicename);
                Utilities.AppendToCsvFile(IniFile.CSV_TEMPERATURE_HUMIDITY, ",", ZoneName,
                    Constants.CAPABILITY_TEMP, datetime.ToString(Constants.DATETIME_DB_FORMAT), value.ToString(), ZoneId.ToString(), position.ToString(), devicename);
				if (position == 0) {
					ScriptingRule.ExecuteRule(this, "temp=" + value);
					//Temperature = value;
				}
				//m_temperatureLast = m_temperature;
			}
			temp.Temperature = value;
            m_lastTempSet = datetime;
			if (Temperature > TemperatureMaxAlarm) {
				Alert.CreateAlert("Max temperature [" + TemperatureMaxAlarm + "] exceeded on zone "
					+ ZoneName + ", temp is " + Temperature, this, false, null,
					Alert.NotificationFlags.NotifyUserAfterXHours, 1);
			}
			else
				if (Temperature < TemperatureMinAlarm) {
					Alert.CreateAlert("Min temperature [" + TemperatureMinAlarm + "] exceeded on zone "
						+ ZoneName + ", temp is " + Temperature, this, false, null,
						Alert.NotificationFlags.NotifyUserAfterXHours, 1);
				}
		}
		public double TemperatureTargetMaxTreshhold {
			get { return TemperatureTarget + Convert.ToDouble(IniFile.PARAM_TEMP_TRESHHOLD[1]); }
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
                    DB.WriteRecord(DB.TABLE_HUMIDITY, DB.COL_HUMIDITY_DATETIME, DateTime.Now.ToString(Constants.DATETIME_DB_FORMAT), DB.COL_HUMIDITY_ZONEID, ZoneId,
                        DB.COL_HUMIDITY_VALUE, value);
					Utilities.AppendToCsvFile(IniFile.CSV_TEMPERATURE_HUMIDITY, ",", ZoneName,
						Constants.CAPABILITY_HUM, DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT), value.ToString(), ZoneId.ToString());
					ScriptingRule.ExecuteRule(this, "humid=" + value);
				}
				m_humidity = value;
				m_lastHumSet = DateTime.Now;
			}
		}

		public String HumidityAgeInMinutes {
			get { return Math.Round(DateTime.Now.Subtract(m_lastHumSet).TotalMinutes).ToString(); }
		}

        public void CheckAndActivateSpeakers(){
            if (!OutputDeviceAutoCompleted().Equals("")) {
                HasSpeakers = true;
            }
        }

		#endregion
		public static void LoadFromIni() {
			//Hashtable zoneValues = IniFile.LoadAllIniEntriesByIntKey(IniFile.INI_SECTION_ZONES);
			ZoneDetails zone;
			m_intervalImmediate = Convert.ToInt16(IniFile.PARAM_GENERIC_INTERVAL_SPLIT[1].Split('-')[0]);
			m_intervalRecent = Convert.ToInt16(IniFile.PARAM_GENERIC_INTERVAL_SPLIT[1].Split('-')[1]);
			m_intervalPast = Convert.ToInt16(IniFile.PARAM_GENERIC_INTERVAL_SPLIT[1].Split('-')[2]);
			int i = 0;
			//foreach (int id in zoneValues.Keys) {
			do {
				try {
					zone = new ZoneDetails(i, "");
				}
				catch (Exception) { 
					break; 
				}
				m_valueList.Add(zone);
				i++;
			} while (true);

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
					ZoneDetails zonestorage = JSON.ToObject<ZoneDetails>(json);
					Id = zonestorage.Id;
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
                    if (OutputKeywords.Trim() != "") {
                        HasSpeakersCapability = true;
                        CheckAndActivateSpeakers();
                        //OutputDeviceAutoCompleted = GetOutputDeviceNameAutocompleted(OutputDeviceUserSelected, OutputKeywords);
                        
                    }
                    IsBluetoothOutputDevice = zonestorage.IsBluetoothOutputDevice;

					WavedeviceIndex = GetWaveOutDeviceIndex(OutputKeywords);
					OutputDeviceNameWaveVLC = GetVLCAudioWaveDeviceName(WavedeviceIndex);
					ControlDeviceName = zonestorage.ControlDeviceName;

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
					ClosuresCmdContactOnList = zonestorage.ClosuresCmdContactOnList;

					//if (ClosureOpenCloseRelay.RelayType == ClosureOpenCloseRelay.EnumRelayType.NormalOpen)
					IsClosureArmed = true;
					NearbyZonesIdList = zonestorage.NearbyZonesIdList;
					if (NearbyZonesIdList.Length > 0 && NearbyZonesIdList[NearbyZonesIdList.Length - 1] != ';')
						NearbyZonesIdList += ";";
					TemperatureDeviceId = zonestorage.TemperatureDeviceId;
					OneWireIODeviceId = zonestorage.OneWireIODeviceId;
					PowerType = zonestorage.PowerType;
					Type = zonestorage.Type;
					CronSchedule = zonestorage.CronSchedule;
					TemperatureTarget = zonestorage.TemperatureTarget;
					TemperatureMaxAlarm = zonestorage.TemperatureMaxAlarm;
					TemperatureMinAlarm = zonestorage.TemperatureMinAlarm;
					TemperatureResolutionDigits = zonestorage.TemperatureResolutionDigits;
					MaxTempUnitsVariationBetweenReads = zonestorage.MaxTempUnitsVariationBetweenReads;
					Color = zonestorage.Color;
					//m_temperature = "1";

					UtilityType = zonestorage.UtilityType;
					PulseMainUnitsCount = zonestorage.PulseMainUnitsCount;
					PulseMainUnitsStartDate = zonestorage.PulseMainUnitsStartDate;
					PulseSampleMinutesFrequency = zonestorage.PulseSampleMinutesFrequency;
					PulseSubUnits = zonestorage.PulseSubUnits;
					PulseMainUnitType = zonestorage.PulseMainUnitType;
					PulseCountInTimeSample = zonestorage.PulseCountInTimeSample;
					LastCounterCount = zonestorage.LastCounterCount;
					LastPulseSamplingStart = zonestorage.LastPulseSamplingStart;
					MaxUtilityUnitsPerMinute = zonestorage.MaxUtilityUnitsPerMinute;
					CounterPageNameToInclude = zonestorage.CounterPageNameToInclude;
					ClosureLevelNameToInclude = zonestorage.ClosureLevelNameToInclude;
					VoltageSensorIndex = zonestorage.VoltageSensorIndex;
					MinAllowedVoltageValue = zonestorage.MinAllowedVoltageValue;
					MaxAllowedVoltageValue = zonestorage.MaxAllowedVoltageValue;

					LocationName = zonestorage.LocationName;

                    UPSType = zonestorage.UPSType;
                    UPSSerialPortAddress = zonestorage.UPSSerialPortAddress;
                    if (UPSType != MultiZonePlayer.UPSType.Undefined) {
                        InitialiseUPS();
                    }

					LoadPicturesFromDisk();
				}
				else {
					throw new Exception("Ini state NOT FOUND for zone=" + ZoneName);
				}
			}
			catch (Exception ex) {
				if (ex.Message.Contains("NOT FOUND"))
                    throw new Exception("ZoneLoad ENDED at zone="+ZoneId, ex);
                else
                    Alert.CreateAlert("Unable to load zone id=" + ZoneId + " message " + ex.Message + " STACK=" + ex.StackTrace, true);
			}
		}


		public override void SaveEntryToIni() {
			//MLog.Log(this, "Saving state to ini zone=" + ZoneName);
			//remove fields that generate serialisation problems
			this.MediaItem = null;

			fastJSON.JSONParameters param = new fastJSON.JSONParameters(); 
			param.UseExtensions = false;
			String json = JSON.ToJSON(this, param);
			IniFile.IniWriteValue(IniFile.INI_SECTION_ZONESTATE, ZoneId.ToString(), json);
			
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
				//PictureList.Add(pict);
			}
		}

		public bool HasOutputDeviceAvailable() {
			return GetOutputDeviceNameAutocompleted(OutputDeviceUserSelected, OutputKeywords) != "";
		}

		public String OutputDeviceAutoCompleted() {
			return GetOutputDeviceNameAutocompleted(OutputDeviceUserSelected, OutputKeywords);
		}

        private void InitialiseUPS() {
            switch (UPSType) {
                case UPSType.NikyS:
                    MZPState.Instance.UpsList.Add(new NikysUPS(UPSSerialPortAddress, this));
                    break;
                default:
                    Alert.CreateAlert("Unknown UPS type " + UPSType, true);
                    break;
            }
        }
        /// <summary>
        /// Return the directshow device guid
        /// </summary>
        /// <param name="p_outputDevice">either the exact directshow device guid or AUTOxxx indicator</param>
        /// <param name="p_outputKeywords">keywords to look for using device friendly name search</param>
        /// <returns></returns>
		public static String GetOutputDeviceNameAutocompleted(String p_outputDevice, String p_outputKeywords) {
			List<String> systemOutputDevices = DShowUtility.SystemDeviceNameList;
            List<String> systemOutputDeviceFriendlyNames = DShowUtility.SystemDeviceFriendlyNameList;
			String matchValue;
			String result = "";//"NONE-outdev=" + p_outputDevice + "|keys=" + p_outputKeywords + "|";
			if (p_outputKeywords != null) {
				String[] keys = p_outputKeywords.Split(',');

                if (p_outputDevice.Equals(IniFile.DEFAULT_AUTO_DEV_NAME))
                {
					int count=systemOutputDeviceFriendlyNames.Count;
                    for (int i=0;i<count;i++) {
						matchValue = "";
						foreach (String key in keys) {
							if (systemOutputDeviceFriendlyNames[i].ToLower().Contains(key.ToLower()))
                                matchValue = systemOutputDevices[i];
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

			//MLog.Log(null, "SensorDevice user select="+p_outputDevice+" keywords="+p_outputKeywords+" res="+result);
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
            try {
                ZoneClose();
                SaveEntryToIni();
                if (PowerType == PowerType.Relay) {
                    if (WDIORelayOutputPinIndex != Constants.NOT_SET)
                        MZPState.Instance.WDIO.SetOutputLow(WDIORelayOutputPinIndex);
                }
            }
            catch (Exception ex) {
                MLog.Log(ex, this, "Err close zone " + ZoneName);
            }
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
            MediaItem = null;
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

		/*public List<PictureSnapshot> PictureList {
			get { return m_pictureList; }
		}*/

        public void SetInputVoltage(double value, DateTime datetime) {
            InputVoltage = value;
            DB.WriteRecord(DB.TABLE_VOLTAGE, DB.COL_VOLTAGE_DATETIME, datetime.ToString(Constants.DATETIME_DB_FORMAT),
                                DB.COL_VOLTAGE_ZONEID, ZoneId, DB.COL_VOLTAGE_VALUE, value.ToString());
        }

		public void Record1WireVoltage(int voltageIndex, double value, DateTime datetime) {
            HasOneWireVoltageSensor = true;
			if (voltageIndex == VoltageSensorIndex) {
				if (MaxAllowedVoltageValue != Constants.NOT_SET && value > MaxAllowedVoltageValue) {
					Alert.CreateAlertOnce("Recorded voltage " + value + " higher than max " + MaxAllowedVoltageValue + " in zone " + ZoneName, "MaxVoltageZone"+ZoneName);
					return;
				}
				if (MinAllowedVoltageValue != Constants.NOT_SET && value < MinAllowedVoltageValue) {
					Alert.CreateAlertOnce("Recorded voltage " + value + " lower than min " + MinAllowedVoltageValue + " in zone " + ZoneName, "MinVoltageZone" + ZoneName);
					return;
				}
			}
			double lastVal;
			LightSensor light = MZPState.Instance.LightSensorList.Find(x=>x.IsActive && x.ApplyForZoneId == ZoneId);
			if (light == null)
				Alert.CreateAlert("No light sensor (active) found for zone "+ ZoneName, false);

			lastVal = m_voltage[voltageIndex];
			if (lastVal != value) {
                SetInputVoltage(value, datetime);
				double lux=-1d;
				if (value>=0 && light != null && light.ApplyForVoltageIndex == voltageIndex && light.MaxVoltageValue != 0){
					lux = value * light.MaxLuxValue / light.MaxVoltageValue;
				}
				Utilities.AppendToCsvFile(IniFile.CSV_VOLTAGE, ",", ZoneName, Constants.CAPABILITY_VOLTAGE,
						datetime.ToString(IniFile.DATETIME_FULL_FORMAT), value.ToString(), ZoneId.ToString(), voltageIndex.ToString(),
						lux.ToString(), light!=null?light.Name:"[light sensor undefined]");
				m_voltage[voltageIndex] = value;
			}
		}

		private List<String> m_fieldChangedList = new List<string>();
		public void FieldChanged(String fieldname) {
			m_fieldChangedList.Add(fieldname);
		}

		private void CheckAndApplyFieldChanges() {
			if (m_fieldChangedList.Count > 0) {
				foreach (String fieldname in m_fieldChangedList) {
					switch (fieldname) {
						case "ClosureIdList":
							MLog.Log(this, "Applying closure field changes in zone "+ZoneName);
							MZPState.Instance.WDIO.SetPinTypes(this);
							break;
						default:
							break;
					}
				}
				m_fieldChangedList.Clear();
			}
		}

		public void SetControlDeviceName(String devicename) {
			foreach (ZoneDetails zone in ZoneDetailsList) {
				//remove control device from zones that have it already
                if (zone.ControlDeviceName == null)
                    zone.ControlDeviceName = "";
				if (zone.ControlDeviceName.Contains(devicename)) {
					zone.ControlDeviceName=zone.ControlDeviceName.Replace(devicename + Constants.MULTI_ENTRY_SEPARATOR, "");
					zone.ControlDeviceName=zone.ControlDeviceName.Replace(devicename, "");//failsafe for entries without separator
                    zone.SaveEntryToIni();
					Alert.CreateAlert("Removed existing control device from zone " + zone.ZoneName + " " + devicename, true);
				}
			}
            //allow only one control device per zone, possibl improv in the future
            ControlDeviceName = devicename;// +Constants.MULTI_ENTRY_SEPARATOR;
			WaitForControlDeviceSetup = false;
			SaveEntryToIni();
		}
		#region statics
		/*public static List<ZoneDetails> ValueList {
			get { return ZoneDetails.ValueList.Select(x=>(ZoneDetails)x).ToList(); }
		}*/
		public static void Add(ZoneDetails zone) {
			m_valueList.Add(zone);
		}
		public static List<ZoneDetails> ZoneDetailsList {
			get { return m_valueList.Select(x => (ZoneDetails)x).ToList(); }
		}
		public static ZoneDetails GetZoneById(int zoneId) {
			return (ZoneDetails)m_valueList.Find(item => ((ZoneDetails)item).ZoneId == zoneId);
		}
		public static ZoneDetails GetZoneByInternalId(int Id) {
			return (ZoneDetails)m_valueList.Find(item => ((ZoneDetails)item).Id == Id);
		}
		public static List<ZoneDetails> GetZonesWithOneWire(string deviceaddress) {
			string devaddlower = deviceaddress.ToLower();
			return m_valueList.FindAll(x => ((ZoneDetails)x).TemperatureDeviceId.ToLower().Contains(devaddlower) 
				|| ((ZoneDetails)x).OneWireIODeviceId.ToLower().Contains(devaddlower)).Select(x => (ZoneDetails)x).ToList(); 
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

		public static Boolean ZoneThatRequireHeat_AllExcept(int exceptedZoneId) {
			return ZoneDetailsList.Find(x => x.RequireHeat && x.ZoneId!=exceptedZoneId) != null;
		}

        public static String ZoneThatRequireHeatDetails_AllExcept(int exceptedZoneId) {
            List<ZoneDetails> zones = ZoneDetailsList.FindAll(x => x.RequireHeat && x.ZoneId!=exceptedZoneId).ToList();
            string details = "";
            if (zones==null || zones.Count==0)
                details = "No zones requires heat (zone "+ GetZoneById(exceptedZoneId).ZoneName + " is excepted)";
            else
                foreach (ZoneDetails zone in zones) {
                    details += zone.ZoneName + " current temp=" + zone.Temperature + " target=" + zone.TemperatureTarget + "; ";
                }
            return details;
        }

		public static List<ZoneDetails> ZoneWithLockContact_All {
			get {
				return ZoneDetailsList.FindAll(x => x.HasClosures && x.ClosureType==EnumClosureType.LockZoneContact)
					.OrderByDescending(x => x.LastClosureEventDateTime).ToList();
			}
		}

		public static List<ZoneDetails> ZoneWithCondition_All(String fieldName, String fieldValue) {
			return ZoneDetailsList.FindAll(x => Reflect.GetPropertyField(x,fieldName).ToString()==fieldValue)
					.OrderByDescending(x => x.LastClosureEventDateTime).ToList();
		}
		public static void ProcessAllZones(bool fastActions, bool slowActions) {
			foreach (ZoneDetails zone in ZoneDetailsList) {
                if (MZPState.Instance==null || MZPState.Instance.IsShuttingDown)
                    return;
				if (fastActions) {
					//turn on or off power
					if (zone.RequirePower && !zone.IsPowerOn) {
						MLog.Log("Powering on zone "+zone.ZoneName+", require power DETAILS: " + zone.RequirePowerDetails + zone.RequirePower);
						zone.PowerControlOn();
					}

					if (!zone.RequirePower && zone.IsPowerOn) {
                        MLog.Log("Powering off zone " + zone.ZoneName + ", require power DETAILS: " + zone.RequirePowerDetails + zone.RequirePower);
						zone.PowerControlOff();
					}

					zone.CheckAndApplyFieldChanges();
				}
				if (slowActions) {
					if (zone.ControlDeviceName != null && zone.ControlDeviceName != "") {
						ControlDevice dev = MZPState.Instance.SystemAvailableControlDevices.Find(x => zone.ControlDeviceName.Contains(x.DeviceName));
						if (dev == null) {
							Alert.CreateAlertOnce("Invalid control device in zone "+zone.ZoneName + " device="+zone.ControlDeviceName,"ControlDevice"+zone.ZoneName);
						}
					}
				}
			}
		}

		#endregion

		#region  zone generic
		public static List<ZoneGeneric> ActiveZones {
			get { return ZoneDetailsList.FindAll(x=>x.IsActive).Select(x=>x.ZoneGeneric).ToList(); }
		}
		#endregion

        public void ReceiveState(GenericUPS.UPSState state) {
            switch (UPSType) {
                case UPSType.NikyS:
                    m_hasUPSTemperatureSensor = true;
                    m_hasUPSVoltageSensor = true;
                    double temp = Convert.ToDouble(state.Temperature);
                    double inputvolt = Convert.ToDouble(state.InputVoltage);
                    double load = Convert.ToDouble(state.LoadPercent);
                    double outvolt = Convert.ToDouble(state.OutputVoltage);
                    double batvolt = Convert.ToDouble(state.BatteryVoltage);
                    SetTemperature(temp, state.UPSId, DateTime.Now);
                    SetInputVoltage(inputvolt, DateTime.Now);
                    SetUPSLoad(load, DateTime.Now);

                    SensorDevice dev = SensorDevice.UpdateGetDevice(ZoneName, state.UPSId, "UPS", this, SensorDevice.DeviceTypeEnum.UPS, "", DateTime.Now);
                    dev.Temperature = temp;
                    dev.Voltage[0] = inputvolt;
                    dev.Voltage[1] = outvolt;
                    dev.Voltage[2] = batvolt;
                    dev.OtherInfo = "load "+load+"%" + " remaining mins="+state.RemainingMinutes + " bitstat=" + state.OtherStatus;
                    dev.RecordSuccess();
                    break;
                }
        }

        private void SetUPSLoad(Double value, DateTime datetime) {
            //TODO
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
		PresenceContact = 3,
		LockZoneContact = 4
	}
	public enum EnumUtilityType {
		Undefined = -1,
		Electricity = 0,
		Water = 1,
		Gas = 2,
		Sunlight = 3
	}
	public enum EnumRelayType {
		Undefined,
		NormalOpen,
		NormalClosed,
		Button
	}
    public enum UPSType{
        Undefined,
        Mustek,
        APC,
        NikyS
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

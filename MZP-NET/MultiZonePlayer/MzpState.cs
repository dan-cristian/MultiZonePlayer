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
using System.ComponentModel;

namespace MultiZonePlayer {
	public class MZPState {
		[Description("Edit")] public Boolean SMSEnabled = false;
		[Description("Edit")] public Boolean LandLineEnabled;

		public Boolean IsMobilePhoneFunctional = false;
		public Boolean IsRFXFunctional = false;
		public Boolean IsModemFunctional = false;
		public Boolean IsChatFunctional = false;
		public Boolean IsDVRFunctional = false;

		private static MZPState m_sysState = null;
		private ArrayList m_systemInputDeviceList = null;
		private Hashtable m_systemInputDeviceNames = null;
		public String powerControlStatusCommand;
		private List<ControlDevice> m_iniControlList = new List<ControlDevice>();
		public List<Playlist> m_playlist = new List<Playlist>();
		public Hashtable zoneDefaultInputs;
		private static RemotePipi[] remotePipi;
		private static int maxRemotes = 10;
		private List<ControlDevice> m_systemAvailableControlDevices = new List<ControlDevice>();
		private static BasePowerControl m_powerControlDenkovi, m_powerControlNumato;
		private Hashtable m_zoneInputDeviceNames = null;
		private Hashtable m_playListOLD = null; //list with all songs from current playlist
		public Boolean IsShuttingDown = false;
		private List<MoodMusic> m_moodMusicList;
		private Cron m_cron;
		private MultiZonePlayer.Tail m_Tail;
		private ZoneEvents m_zoneEvents;
		private Alarm m_systemAlarm;
		private Boolean m_isFollowMeMusic = false;
		private Boolean m_isPowerFailure = false;
		private Boolean m_isWinloadLoading = false;
		public List<IMessenger> m_messengerList;
		private NotifyState m_notifyState;
		private List<MusicScheduleEntry> m_musicScheduleList;
		private DateTime m_initMZPStateDateTime = DateTime.Now;
		private DateTime m_lastBuzzDateTime = DateTime.Now;
		private List<Display> m_displayList = new List<Display>();
		private List<ZoneGeneric> m_activeZones = new List<ZoneGeneric>();
		private List<MacroEntry> m_macroList;
		private List<GenericUPS> m_upsList;
		private DateTime m_lastRulesFileModifiedDate = DateTime.MinValue;
		private DateTime m_lastScheduleFileModifiedDate = DateTime.MinValue, m_lastBTCheck = DateTime.MinValue;
		private WDIO m_wdio;
		public Boolean TestCond = false;
		private Bluetooth m_bt;
		private SysLog m_syslog;

		public WDIO WDIO {
			get { return m_wdio; }
		}

		private OneWire m_oneWire;

		public UserPresence UserPresence {
			get { return MultiZonePlayer.UserPresence.Instance; }
		}

		public User User {
			get { return (User) MultiZonePlayer.User.StaticInstance; }
		}

		public UtilityCost UtilityCost{
			get { return (UtilityCost)MultiZonePlayer.UtilityCost.StaticInstance; }
		}

		public OneWire OneWire {
			get { return m_oneWire; }
		}

		public Boolean IsWinloadLoading {
			get { return m_isWinloadLoading; }
			set { m_isWinloadLoading = value; }
		}

		public List<ZoneGeneric> ActiveZones {
			get { return m_activeZones; }
			set { m_activeZones = value; }
		}

		public List<Display> DisplayList {
			get { return m_displayList; }
			set { m_displayList = value; }
		}

		public List<MacroEntry> MacroList {
			get { return m_macroList; }
		}

		public NotifyState NotifyState {
			get { return m_notifyState; }
			set { m_notifyState = value; }
		}

		public Boolean IsPowerFailure {
			get { return m_isPowerFailure; }
			set { m_isPowerFailure = value; }
		}

		public WDIO Gpio {
			get { return m_wdio; }
		}

		public MZPState() {
			m_sysState = this;
			m_notifyState = new NotifyState();
			MLog.LoadFromIni();
			MLog.Log(this, "\r\n-----------------START--------------------");

			m_systemAlarm = new Alarm(Convert.ToInt16(IniFile.PARAM_ALARM_SECURE_AREA_ID[1]));
			InitRemotes();
			m_powerControlDenkovi = new DenkoviPowerControl("8 Relay Brd USB");
			m_powerControlNumato = new NumatoLPTControl(888);

			MLog.Log(this, "Retrieving system available audio output devices");

			String deviceName;
			RFXDeviceDefinition.LoadFromIni();
			MLog.Log(this, "Loading m_valueList from ini");
			MultiZonePlayer.ZoneDetails.LoadFromIni();
			User.LoadFromIni();
			m_moodMusicList = new List<MoodMusic>();
			MoodMusic.LoadFromIni(ref m_moodMusicList);

			LoadIniParams();
			LoadIniSections();
			LoadSystemAndUserControls();
			UtilityCost.LoadFromIni();

			MLog.Log(this, "Retrieving system available audio input devices");
			DShowUtility.GetDeviceOfCategory(DShowUtility.Clsid_AudioInput, out m_systemInputDeviceList);
			if (m_systemInputDeviceNames == null) {
				m_systemInputDeviceNames = new Hashtable();
			}
			else {
				m_systemInputDeviceNames.Clear();
			}

			m_musicScheduleList = new List<MusicScheduleEntry>();
			MusicScheduleEntry.LoadFromIni(ref m_musicScheduleList);

			MLog.Log(this, "Retrieving system audio input devices details");
			int index = 0;
			if (m_systemInputDeviceList != null) {
				foreach (Object m in MZPState.Instance.m_systemInputDeviceList) {
					deviceName = DShowUtility.GetDisplayName((IMoniker) m);
					m_systemInputDeviceNames.Add(index.ToString(), deviceName);
					index++;
				}
			}

			MLog.Log(this, "Loading other settings from ini");

			LoadIniInput();
			LoadPlaylist();
			m_zoneEvents = new ZoneEvents();
			WebServer.Initialise();
			Test.RunTest();

			m_Tail = new MultiZonePlayer.Tail(IniFile.PARAM_PARADOX_WINLOAD_DATA_FILE[1]);
			m_Tail.MoreData += new MultiZonePlayer.Tail.MoreDataHandler(m_zoneEvents.Tail_MoreData_PARADOX);

			Thread lazyLoad = new Thread(() => LoadSerials());
			lazyLoad.Name = "LoadSerials";
			lazyLoad.Start();

			//Thread th = new Thread(() => m_wdio.LoopForEvents());
			//th.Name = "WDIO Event Loop";
			//th.Start();

			LoadMacrosandRules();

			m_upsList = new List<GenericUPS>();
			m_upsList.Add(new MustekUPS(IniFile.PARAM_UPS_MUSTEK_STATUS_URL[1]));
			m_upsList.Add(new APCUPS("Application", IniFile.PARAM_UPS_APC_LOG_SOURCE[1]));
			MediaLibrary.InitialiseLibrary();

			m_cron = new Cron();
			Thread cron = new Thread(() => m_cron.start());
			cron.Name = "Cron";
			cron.Start();

			m_oneWire = new OneWire();
			Thread onew = new Thread(() => m_oneWire.LoopReadSlow());
			onew.Name = "Onewire";
			onew.Start();

			Thread bt = new Thread(() => Bluetooth.StartDiscovery());
			bt.Name = "Bluetooth";
			bt.Start();

			m_syslog = new SysLog();
			Thread sl = new Thread(() => m_syslog.Start());
			sl.Name = "Syslog";
			sl.Start();

			//adding displays
			foreach (ZoneDetails zone in ZoneDetails.ZoneDetailsList) {
				if (zone.DisplayType.Equals(Display.DisplayTypeEnum.LGTV.ToString())) {
					m_displayList.Add(new DisplayLGTV(zone.DisplayConnection, zone));
				}
			}

			LogEvent(EventSource.System, "System started", MZPEvent.EventType.Functionality, MZPEvent.EventImportance.Informative,
				null);
		}

		private void LoadSerials() {
			m_messengerList = new List<IMessenger>();
			m_wdio = new WDIO(); //new GPIO();
			m_bt = new Bluetooth();
			m_messengerList.Add(new GTalkMessengers(IniFile.PARAM_GTALK_USERNAME[1], IniFile.PARAM_GTALK_USERPASS[1]));
			if (SMSEnabled) {
				m_messengerList.Add(new SMS());
			}
			if (LandLineEnabled) {
				m_messengerList.Add(new Modem());
			}
			m_messengerList.Add(new RFXCom());
			m_messengerList.Add(m_wdio);
			m_messengerList.Add(m_oneWire);
		}

		public void LoadSystemAndUserControls() {
			PopulateSystemControlDevices();
			//loading user selected controls for each zone
			ControlDevice.LoadFromIni(m_iniControlList);
		}

		private void LoadMacrosandRules() {
			DateTime fileModified = System.IO.File.GetLastWriteTime(IniFile.CurrentPath() + IniFile.SCHEDULER_FILE);
			if (fileModified != m_lastScheduleFileModifiedDate) {
				m_macroList = MacroEntry.LoadFromIni();
				RFXDeviceDefinition.LoadFromIni();
				MLog.LoadFromIni();
				m_lastScheduleFileModifiedDate = fileModified;
			}

			fileModified = System.IO.File.GetLastWriteTime(IniFile.CurrentPath() + IniFile.RULES_FILE);
			if (fileModified != m_lastRulesFileModifiedDate) {
				Rules.LoadFromIni();
				m_lastRulesFileModifiedDate = fileModified;
			}
		}

		public List<MacroEntry> GetZoneMacros(int zoneId) {
			List<MacroEntry> macros = null;
			String zoneName = MultiZonePlayer.ZoneDetails.GetZoneById(zoneId).ZoneName;
			if (m_macroList != null) {
				macros = m_macroList.FindAll(x =>
					x.CommandList != null && x.CommandList.Find(y => y.ZoneName == zoneName) != null
					||
					x.CommandList != null &&
					x.CommandList.Find(z => z.ParameterValueList != null
					                        && (z.ParameterValueList.ToLower().Contains("zoneid=" + zoneId + ";")
					                            || z.ParameterValueList.ToLower().Contains("zoneid=" + zoneId + "\"")
					                            || z.ParameterValueList.ToLower().Contains("zonename=" + zoneName))) != null
					);
			}
			return macros;
		}

		public static void RestartGenericProc(string procName, string procPath,
			System.Diagnostics.ProcessWindowStyle startState, System.Diagnostics.ProcessPriorityClass priority) {
			MLog.Log(null, "Restarting " + procName);
			Utilities.CloseProcSync(procName);
			Utilities.RunProcessWait(procPath, System.Diagnostics.ProcessWindowStyle.Normal, priority);
		}

		public void Shutdown() {
			try {
				IsShuttingDown = true;
				foreach (string file in Directory.GetFiles(IniFile.CurrentPath(), "*" + IniFile.TEMP_EXTENSION)) {
					File.Delete(file);
				}
				PowerControlOff();
				WebServer.Shutdown();
				m_Tail.Stop();
				m_syslog.Stop();
				foreach (Display disp in m_displayList) {
					disp.Disconnect();
				}
				foreach (IMessenger msg in m_messengerList) {
					msg.Close();
				}
				m_cron.stop();
				foreach (ZoneDetails zone in ZoneDetails.ZoneDetailsList) {
					zone.SaveToIni();
				}
				foreach (IMessenger mes in m_messengerList) {
					mes.Close();
				}
				UtilityCost.SaveAllToIni();
				MediaLibrary.SaveLibraryToIni();
				m_sysState = null;
			}
			catch (Exception) {
			}
			finally {
				m_sysState = null;
			}
		}

		~MZPState() {
			Shutdown();
		}

		public static void RestartComputer(String reason) {
			MLog.Log(null, "RESTARTING COMPUTER for reason " + reason);
			Alert.CreateAlert("RESTARTING COMPUTER for reason " + reason, null, false,
				Alert.NotificationFlags.NeedsImmediateUserAck, 1);
			//MZPState.Instance.LogEvent(MZPEvent.EventSource.System, "RESTARTING COMPUTER for reason " + reason, MZPEvent.EventType.Security, MZPEvent.EventImportance.Critical,null);
			Thread.Sleep(3000);
			System.Diagnostics.Process.Start("shutdown.exe", "-r -f -t 0");
		}

		public static void Initialise() {
			if (m_sysState != null) {
				m_sysState.Shutdown();
			}
			m_sysState = new MZPState();
		}

		public static bool IsInitialised {
			get { return m_sysState != null; }
		}

		public static MZPState Instance {
			get { return m_sysState; }
		}

		public ZoneDetails ZoneDetails {
			get { return MultiZonePlayer.ZoneDetails.StaticInstance; } //.ValueList.Select(x => (ZoneDetails)x).ToList(); }
		}

		public List<ZoneDetails> ZoneDetailsList {
			get { return MultiZonePlayer.ZoneDetails.ZoneDetailsList; }
		}

		public static List<ZoneDetails> ZonesWithType(ZoneType type) {
			return ZoneDetails.ZoneDetailsList.FindAll(x => x.Type == type);
			//return ValueList.ValueList(type, "=");
		}

		/*
	public class ValueList {
		public static ValueList<ZoneDetails> ValueList(ZoneType value, String operation) {
			//ValueList<ZoneDetails> m_valueList;
			return ZoneDetails.ZoneDetailsList.FindAll(x => x.Type == value);
		}

	}*/

		public List<String> GetSingletonValueList(String className) {
			System.Runtime.Remoting.ObjectHandle handle = Activator.CreateInstance(null,
				System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "." + className);
			Object p = handle.Unwrap();
			Type t = p.GetType();

			Singleton sg = (Singleton) p;
			return sg.Instance.ValueList.Select(x => x.Id.ToString()).ToList();
			//TODO
		}

		public List<String> GetEditableFieldList(String className) {
			System.Runtime.Remoting.ObjectHandle handle = Activator.CreateInstance(null,
				System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "." + className);
			Object p = handle.Unwrap();
			Type t = p.GetType();
			if (t != null) {
				return t.GetFields().ToList().FindAll(x => (
					(x.GetCustomAttributes(typeof (DescriptionAttribute), false) != null) &&
					(x.GetCustomAttributes(typeof (DescriptionAttribute), false).Length > 0)
					&&
					(((DescriptionAttribute[]) x.GetCustomAttributes(typeof (DescriptionAttribute), false))[0].Description == "Edit"))
					).Select(y => y.Name).ToList();
			}
			else {
				return null;
			}
		}

		public List<String> GetEditableFieldTypeList(String className) {
			System.Runtime.Remoting.ObjectHandle handle = Activator.CreateInstance(null,
				System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "." + className);
			Object p = handle.Unwrap();
			Type t = p.GetType();
			List<String> list;
			if (t != null) {
				list = t.GetFields().ToList().FindAll(x => (
					(x.GetCustomAttributes(typeof(DescriptionAttribute), false) != null) &&
					(x.GetCustomAttributes(typeof(DescriptionAttribute), false).Length > 0)
					&&
					(((DescriptionAttribute[])x.GetCustomAttributes(typeof(DescriptionAttribute), false))[0].Description == "Edit"))
					).Select(y => (y.FieldType.Name + " " +Reflect.ListTypeValues(y.FieldType,","))).ToList();
			}
			else {
				list = null;
				
			}
			return list;
		}

		public List<String> GetFieldList(String className) {
			System.Runtime.Remoting.ObjectHandle handle = Activator.CreateInstance(null,
				System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "." + className);
			Object p = handle.Unwrap();
			Type t = p.GetType();
			if (t != null) {
				return t.GetFields().ToList().Select(y => y.Name).ToList();
			}
			else {
				return null;
			}
		}

		public ZoneDetails ZoneWithType(ZoneType type, int index) {
			List<ZoneDetails> zones;
			zones = ZoneDetails.ZoneDetailsList.FindAll(x => x.Type == type);
			if (index < zones.Count) {
				return zones[index];
			}
			else {
				return null;
			}
		}

		public List<String> ZoneTypesAvailable {
			get { return ZoneDetails.ZoneDetailsList.Select(x => x.Type.ToString()).Distinct().ToList(); }
		}

		public String ZoneTypesAvailableByIndex(int i) {
			return ZoneTypesAvailable[i];
		}

		public int ZonesCount {
			get { return ZoneDetails.ZoneDetailsList.Count; }
		}

		public ZoneEvents ZoneEvents {
			get { return m_zoneEvents; }
		}

		public List<User> UserList {
			get { return User.StaticInstance.ValueList.Select(x => (User) x).ToList(); }
		}

		public List<UserPresence> UserPresenceList {
			get { return MultiZonePlayer.UserPresence.PresenceList; }
		}

		public List<User> UserIsNearList {
			get { return MultiZonePlayer.UserPresence.UserIsNearList; }
		}

		public String UserIsNearSummary {
			get { return MultiZonePlayer.UserPresence.UserIsNearSummary; }
		}

		public List<UtilityCost> UtilityCostList{
			get { return UtilityCost.StaticInstance.ValueList.Select(x => (UtilityCost)x).ToList(); }
		}

		public List<IMZPDevice> DeviceList {
			get {
				List<IMZPDevice> obj = new List<IMZPDevice>();
				foreach (Object dev in m_messengerList) {
					if (dev is IMZPDevice)
						obj.Add(dev as IMZPDevice);
				}
				foreach (Object dev in m_upsList) {
					if (dev is IMZPDevice)
						obj.Add(dev as IMZPDevice);
				}
				obj.Add(m_powerControlDenkovi as IMZPDevice);
				obj.Add(m_powerControlNumato as IMZPDevice);
				obj.Add(m_bt);
				return obj;
			}
			
		}

		public Alarm SystemAlarm {
			get { return m_systemAlarm; }
		}

		public List<MoodMusic> MoodMusicList {
			get { return m_moodMusicList; }
		}

		public List<MusicScheduleEntry> MusicScheduleList {
			get { return m_musicScheduleList; }
		}

		public ZoneDetails GetZoneIdByContainsName(String zoneName) {
			if (zoneName != null) {
				return ZoneDetails.ZoneDetailsList.Find(item => item.ZoneName.Contains(zoneName));
			}
			else {
				return null;
			}
		}

		public List<ControlDevice> IniControlDevices {
			get { return m_iniControlList; }
		}

		public List<ControlDevice> SystemAvailableControlDevices {
			get { return m_systemAvailableControlDevices; }
		}

		public BasePowerControl PowerControl(int zoneid) {
			if (MultiZonePlayer.ZoneDetails.GetZoneById(zoneid).PowerType == PowerType.Denkovi.ToString()) {
				return m_powerControlDenkovi;
			}
			else {
				return m_powerControlNumato;
			}
		}

		public void PowerControlOn(int zoneid) {
			ZoneDetails zone = MultiZonePlayer.ZoneDetails.GetZoneById(zoneid);
			bool switchedOn = false;
			String powerdevice;
			if (zone.HasPowerCapabilities) {
				if (MultiZonePlayer.ZoneDetails.GetZoneById(zoneid).PowerType == PowerType.Denkovi.ToString()) {
					if (!m_powerControlDenkovi.IsPowerOn(zoneid)) {
						switchedOn = true;
					}
					m_powerControlDenkovi.PowerOn(zoneid);
					powerdevice = "Denkovi";
				}
				else {
					if (!m_powerControlNumato.IsPowerOn(zoneid)) {
						switchedOn = true;
					}
					if (MultiZonePlayer.ZoneDetails.GetZoneById(zoneid).PowerType == PowerType.Numato.ToString()) {
						m_powerControlNumato.PowerOn(zoneid);
					}
					powerdevice = "Numato";
				}
				if (switchedOn) {
					Utilities.AppendToCsvFile(IniFile.CSV_CLOSURES, ",", zone.ZoneName, powerdevice,
						DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT), "PowerOn", zoneid.ToString(),
						Constants.EVENT_TYPE_POWER);
				}
			}
			else {
				MLog.Log(this, "Error, Power ON command sent to zone without power cap: " + zone.ZoneName);
			}
		}

		public void PowerControlOff(int zoneid) {
			ZoneDetails zone = MultiZonePlayer.ZoneDetails.GetZoneById(zoneid);
			bool switchedOff = false;
			String powerdevice;

			if (zone.HasPowerCapabilities) {
				if (MultiZonePlayer.ZoneDetails.GetZoneById(zoneid).PowerType == PowerType.Denkovi.ToString()) {
					if (m_powerControlDenkovi.IsPowerOn(zoneid)) {
						switchedOff = true;
					}
					m_powerControlDenkovi.PowerOff(zoneid);
					powerdevice = "Denkovi";
				}
				else {
					if (m_powerControlNumato.IsPowerOn(zoneid)) {
						switchedOff = true;
					}
					if (MultiZonePlayer.ZoneDetails.GetZoneById(zoneid).PowerType == PowerType.Numato.ToString()) {
						m_powerControlNumato.PowerOff(zoneid);
					}
					powerdevice = "Numato";
				}
				if (switchedOff) {
					Utilities.AppendToCsvFile(IniFile.CSV_CLOSURES, ",", zone.ZoneName, powerdevice,
						DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT), "PowerOff", zoneid.ToString(),
						Constants.EVENT_TYPE_POWER);
				}
			}
			else {
				MLog.Log(this, "Error, Power OFF command sent to zone without power cap: " + zone.ZoneName);
			}
		}

		public void PowerControlOff() {
			//m_powerControlDenkovi.PowerOff();
			//m_powerControlNumato.PowerOff();
			foreach (ZoneDetails zone in ZoneDetails.ZoneDetailsList) {
				if ((zone.ActivityType != GlobalCommands.xbmc) || (!zone.IsActive)) {
					MLog.Log(this, "Powering off zone " + zone.ZoneName + " activity=" + zone.ActivityType + " active=" + zone.IsActive
					               + " reqpower=" + zone.RequirePower);
					this.PowerControlOff(zone.ZoneId);
				}
			}
		}

		public bool PowerControlIsOn(int zoneid) {
			if (MultiZonePlayer.ZoneDetails.GetZoneById(zoneid).PowerType == PowerType.Denkovi.ToString()) {
				return m_powerControlDenkovi.IsPowerOn(zoneid);
			}
			else if (MultiZonePlayer.ZoneDetails.GetZoneById(zoneid).PowerType == PowerType.Numato.ToString()) {
				return m_powerControlNumato.IsPowerOn(zoneid);
			}
			else {
				return false;
			}
		}

		public void ToogleFollowMeMusic() {
			m_isFollowMeMusic = !m_isFollowMeMusic;
		}

		public Boolean IsFollowMeMusic {
			get { return m_isFollowMeMusic; }
		}

		/*public ValueList<String> SystemOutputDeviceNames
    {
        get{return m_systemOutputDeviceNames;}
    }*/

		public Hashtable SystemInputDeviceNames {
			get { return m_systemInputDeviceNames; }
		}

		public Hashtable ZoneInputDeviceEntries {
			get { return m_zoneInputDeviceNames; }
		}

		public List<Playlist> Playlist {
			get { return m_playlist; }
		}

		public bool HasNotifyMove {
			get {
				bool move = false;
				move = ZoneDetails.ZoneDetailsList.Find(x => x.HasRecentMove || x.HasImmediateMove) != null;
				return move;
			}
		}

		public ZoneGeneric GetZoneIfActive(int zoneId) {
			ZoneGeneric zp = ActiveZones.Find(item => item.ZoneDetails.ZoneId == zoneId);
			if (zp != null && IsZoneActive(zoneId)) {
				return zp;
			}
			else {
				return null;
			}
		}

		public ZoneGeneric GetZone(int zoneId) {
			ZoneGeneric zp = ActiveZones.Find(item => item.ZoneDetails.ZoneId == zoneId);
			return zp;
		}

		public void OpenZone(int zoneId) {
			if (!IsZoneActive(zoneId)) {
				ActiveZones.Add(new ZoneGeneric(zoneId));
			}
		}

		public bool IsZoneActive(int zoneId) {
			ZoneDetails zone = ZoneDetails.ZoneDetailsList.Find(item => item.ZoneId == zoneId);
			if (zone != null) {
				return zone.IsActive;
			}
			else {
				return false;
			}
			;
		}

		public Hashtable GetAPlayList(int zoneId) {
			Hashtable table;
			if (m_playListOLD.Count > 0) {
				table = m_playListOLD[zoneId] as Hashtable;
				if (table == null) {
					table = m_playListOLD["0"] as Hashtable;
				}
			}
			else {
				table = new Hashtable();
				table.Add("0", "");
			}

			return table;
		}

		public int GetZoneByControlDevice(String deviceName, string key) {
			List<ControlDevice> devices = m_iniControlList.FindAll(x => x.DeviceName == deviceName);
			switch (devices.Count) {
				case 0:
					return -1;
				case 1:
					return devices[0].ZoneId;
				default:
					foreach (ControlDevice dev in devices) {
						if (MultiZonePlayer.ZoneDetails.GetZoneById(dev.ZoneId).ClosureIdList == key) {
							return dev.ZoneId;
						}
					}
					return -1;
			}
		}

		public String GetControlDeviceByZone(int zoneId) {
			foreach (ControlDevice ctrl in MZPState.Instance.m_iniControlList) {
				if (ctrl.ZoneId.Equals(zoneId)) {
					return ctrl.DeviceName;
				}
			}
			return null;
		}

		public List<Alert> ActiveAlertList {
			get { return Alert.ActiveAlertList; }
		}

		public List<Singleton> PerformanceList {
			get { return Performance.StaticInstance.ValueList; }
		}

		public void InitRemotes() {
			//this.multiPlayerDataSet.Remotes.Count;
			remotePipi = new RemotePipi[maxRemotes];
			remotePipi[0] = new RemotePipi();
			//remotePipi[0] = new RemotePipi(frmOptions.GetCommands());
		}

		private void PopulateSystemControlDevices() {
			RawInputDevice.EnumerateDevices();
			IDictionaryEnumerator enumerator = RawInputDevice.GetDeviceList().GetEnumerator();
			m_systemAvailableControlDevices.Clear();
			while (enumerator.MoveNext()) {
				m_systemAvailableControlDevices.Add(new ControlDevice(-1,
					((RawInputDevice.DeviceInfo) enumerator.Value).deviceName,
					((RawInputDevice.DeviceInfo) enumerator.Value).Name));
			}
		}

		private void LoadIniParams() {
			String pname;
			String pvalue;
			String[] param;

			for (int r = 0; r < IniFile.PARAMS.Length; r++) {
				param = IniFile.PARAMS[r] as String[];
				pname = param[0] as String;

				pvalue = IniFile.IniReadValue(IniFile.INI_SECTION_PARAMS, pname);
				if (pvalue != "") {
					param[1] = pvalue;
				}
			}
		}

		private void LoadIniSections() {
			zoneDefaultInputs = IniFile.LoadAllIniEntriesByKey(IniFile.INI_SECTION_INPUTS);
			powerControlStatusCommand = IniFile.LoadIniEntryByKey(IniFile.INI_SECTION_PARAMS,
				IniFile.PARAM_POWER_CONTROL_STATUS_APP_GEMBIRD[0]);
		}

		private void LoadPlaylist() {
			m_playListOLD = IniFile.LoadAllIniEntriesByKey(IniFile.INI_SECTION_PLAYLIST + "0");
			foreach (String key in m_playListOLD.Keys) {
				m_playlist.Add(new Playlist(m_playListOLD[key].ToString()));
			}
		}

		private void LoadIniInput() {
			String inputName, inputType, inputInZoneId, inputDeviceName;
			int r = 0;
			if (m_zoneInputDeviceNames == null) {
				m_zoneInputDeviceNames = new Hashtable();
			}
			else {
				m_zoneInputDeviceNames.Clear();
			}

			while (true) {
				inputName = IniFile.IniReadValue(IniFile.INI_SECTION_INPUTDEVICE + r.ToString(), "InputName");
				if (inputName == "") {
					break;
				}

				inputType = IniFile.IniReadValue(IniFile.INI_SECTION_INPUTDEVICE + r.ToString(), "InputTypeId_Inputs");
				inputInZoneId = IniFile.IniReadValue(IniFile.INI_SECTION_INPUTDEVICE + r.ToString(), "InZoneId");
				inputDeviceName = IniFile.IniReadValue(IniFile.INI_SECTION_INPUTDEVICE + r.ToString(), "InputDeviceName");

				if ((inputName != "") && (!DShowUtility.ValidateDevice(inputDeviceName))) {
					MLog.Log(null, "Invalid input device " + inputName + "in ini file " + inputDeviceName);
				}
				m_zoneInputDeviceNames.Add(r, new InputEntry(inputName, inputType, inputInZoneId, inputDeviceName));
				r++;
			}
		}

		public int GetZoneIdByAlarmZoneId(int alarmZoneId) {
			ZoneDetails zone = ZoneDetails.ZoneDetailsList.Find(x => x.AlarmZoneId.Equals(alarmZoneId));
			if (zone != null) {
				return zone.ZoneId;
			}
			else {
				return -1;
			}
		}

		public int GetZoneIdByCamZoneId(int camId) {
			ZoneDetails zone = ZoneDetails.ZoneDetailsList.Find(x => x.CameraId.Equals(camId.ToString()));
			if (zone != null) {
				return zone.ZoneId;
			}
			else {
				return -1;
			}
		}

		//return most recent zone that had an activity
		public int MostRecentZoneWithContext {
			get {
				ZoneDetails zone = ZoneDetails.ZoneDetailsList.OrderByDescending(x => x.LastLocalCommandDateTime).ToList().Find(x =>
					(x.ActivityType.Equals(GlobalCommands.music)
					 || x.ActivityType.Equals(GlobalCommands.streammp3)
						//|| x.ActivityType.Equals(GlobalCommands.tv) 
					 || x.ActivityType.Equals(GlobalCommands.xbmc))
					&& x.IsActive);
				if (zone != null) {
					MLog.Log(this, "Found most recent active zone id=" + zone.ZoneId + " name=" + zone.ZoneName);
					return zone.ZoneId;
				}
				else {
					zone =
						ZoneDetails.ZoneDetailsList.OrderByDescending(x => x.LastAlarmMovementDateTime).ToList().Find(x => x.HasSpeakers);
					if (zone != null) {
						MLog.Log(this, "Found most recent zone with speakers id=" + zone.ZoneId);
						return zone.ZoneId;
					}
					else {
						return -1;
					}
				}
			}
		}

		//Music zone to clone
		public ZoneGeneric GetFirstZoneMusic() {
			ZoneGeneric zone = null;
			ZoneDetails zonedetails = ZoneDetails.ZoneDetailsList.OrderBy(x => x.LastLocalCommandDateTime).ToList().Find(x =>
				(x.ActivityType.Equals(GlobalCommands.music) && x.IsActive));
			if (zonedetails != null) {
				zone = ActiveZones.Find(x => x.GetZoneId() == zonedetails.ZoneId);
			}
			return zone;
		}

		public int GetActiveChildZone(int parentZoneId) {
			ZoneDetails zone = ZoneDetails.ZoneDetailsList.OrderByDescending(x => x.LastLocalCommandDateTime).ToList()
				.Find(x => (x.ParentZoneId == parentZoneId) && x.IsActive);
			if (zone != null) {
				MLog.Log(this, "Found most recent active child zone id=" + zone.ZoneId + " name=" + zone.ZoneName);
				return zone.ZoneId;
			}
			else {
				return MostRecentZoneWithContext;
			}
		}

		public MoodMusic GetScheduledMood(int zoneId) {
			String time, weekday;
			time = DateTime.Now.ToString(IniFile.DATETIME_DAYHR_FORMAT);
			weekday = DateTime.Now.DayOfWeek.ToString().Substring(0, 2);

			foreach (MusicScheduleEntry entry in m_musicScheduleList) {
				if ((entry.ZoneId.Equals(zoneId) || entry.ZoneId.Equals(-1)) && (time.CompareTo(entry.StartTime) >= 0) &&
				    (time.CompareTo(entry.EndTime) <= 0)
				    && entry.WeekDays.Contains(weekday)) {
					return m_moodMusicList.Find(x => x.Name.Equals(entry.Mood));
				}
			}

			return null;
		}

		public List<MoodMusic> GetScheduledMoodList(int zoneId) {
			String time, weekday;
			time = DateTime.Now.ToString(IniFile.DATETIME_DAYHR_FORMAT);
			weekday = DateTime.Now.DayOfWeek.ToString().Substring(0, 2);
			List<MoodMusic> result = new List<MoodMusic>();

			foreach (MusicScheduleEntry entry in m_musicScheduleList) {
				if ((entry.ZoneId.Equals(zoneId) || entry.ZoneId.Equals(-1)) && (time.CompareTo(entry.StartTime) >= 0) &&
				    (time.CompareTo(entry.EndTime) <= 0)
				    && entry.WeekDays.Contains(weekday)) {
					MoodMusic mood = m_moodMusicList.Find(x => x.Name.Equals(entry.Mood));
					if (mood != null) {
						result.Add(mood);
					}
				}
			}

			return result;
		}

		public string GetZonesStatus() {
			String result = "Status at " + DateTime.Now.ToLongDateString()
			                + " " + DateTime.Now.ToLongTimeString();
			result += "\r\n" + MZPState.Instance.SystemAlarm.AreaState;
			result += "\r\nPower Failed=" + MZPState.Instance.IsPowerFailure;
			foreach (ZoneDetails zone in ZoneDetails.ZoneDetailsList) {
				result += "\r\n" + zone.SummaryStatus;
			}
			return result;
		}

		private void HealthCheckiSpy() {
			if (!Utilities.IsProcAlive(IniFile.PARAM_ISPY_PROCNAME[1])) {
				MLog.Log(this, "iSpy proc not running, restarting. Searched for proc:" + IniFile.PARAM_ISPY_PROCNAME[1]);
				Utilities.CloseProcSync(IniFile.PARAM_ISPY_OTHERPROC[1]);
				RestartGenericProc(IniFile.PARAM_ISPY_PROCNAME[1], IniFile.PARAM_ISPY_APP_PATH[1],
					System.Diagnostics.ProcessWindowStyle.Minimized, System.Diagnostics.ProcessPriorityClass.BelowNormal);
			}
		}

		private void HealthCheckWinload() {
			if (!Utilities.IsProcAlive(IniFile.PARAM_PARADOX_WINLOAD_PROCNAME[1])) {
				MLog.Log(this, "WINLOAD proc not running, restarting");
				m_isWinloadLoading = true;
				RestartGenericProc(IniFile.PARAM_PARADOX_WINLOAD_PROCNAME[1],
					IniFile.PARAM_PARADOX_WINLOAD_APP_PATH[1], System.Diagnostics.ProcessWindowStyle.Normal,
					System.Diagnostics.ProcessPriorityClass.BelowNormal);
			}
			else {
				if ((DateTime.Now.Subtract(SystemAlarm.LastAreaStateChange).Duration().TotalDays > 1)
				    && (DateTime.Now.Subtract(SystemAlarm.LastAlarmEventDateTime).Duration().TotalDays > 1)) {
					//MLog.Log(this, "WINLOAD suspected to be not connected");
				}

				DateTime lastCamEvent = m_initMZPStateDateTime;
				foreach (ZoneDetails zone in ZoneDetails.ZoneDetailsList) {
					if (zone.HasMotionSensor && zone.HasCamera) {
						if (zone.LastCamAlertDateTime.CompareTo(lastCamEvent) > 0) {
							lastCamEvent = zone.LastCamAlertDateTime;
						}
					}
				}
				DateTime alarmRefTime = DateTime.Compare(m_initMZPStateDateTime, m_systemAlarm.LastAlarmEventDateTime) > 0
					? m_initMZPStateDateTime
					: m_systemAlarm.LastAlarmEventDateTime;
				double diff = lastCamEvent.Subtract(alarmRefTime).TotalMinutes;
				if (diff > 60) {
					//MLog.Log(this, "WINLOAD potential error, not running, time dif in min=" + diff);
					//LogEvent(MZPEvent.EventSource.System, "Winload restarting, was not responsive", MZPEvent.EventType.Security, MZPEvent.EventImportance.Error);
					//m//_systemAlarm.LastAlarmEventDateTime = DateTime.Now;
					//RestartWinload();
				}
				//else MLog.Log(this, "WINLOAD move diff OK, min=" + diff);
			}
		}

		private void AutoArmCheck() {
			List<ZoneDetails> openZones;
			openZones = ZoneDetails.ZoneDetailsList.FindAll(x => x.HasMotionSensor && (x.HasRecentMove || x.HasImmediateMove));

			if (openZones.Count == 0) {
				//LogEvent(MZPEvent.EventSource.System, "AUTOARMING", MZPEvent.EventImportance.Security);
				/*
                Metadata.ValueList val = new Metadata.ValueList(Metadata.GlobalParams.command, Metadata.GlobalCommands.alarmarm.ToString(),
                                Metadata.CommandSources.Internal);
                Metadata.ValueList retval;
                API.DoCommandFromWeb(val, out retval);
                    */
			}
		}

		private void HealthCheckMessengers() {
			Boolean ok;
			foreach (IMessenger m in m_messengerList) {
				if (m != null && !m.IsFaulty()) {
					ok = m.TestConnection();
					if (!ok) {
						MLog.Log(this, "Error found on test messenger health, reinitialise: " + m.ToString());
						m.Reinitialise();
					}
				}
			}
		}

		public bool SendMessengerMessageToOne(String message) {
			EmailNotifier.SendEmail(message);
			foreach (IMessenger m in m_messengerList) {
				//if (m.IsConnected())
				{
					m.SendMessageToTarget(message);
					return true;
				}
			}

			return false;
		}

		public Boolean SendMessengerMessageToAll(String message) {
			Boolean result = false;
			result = EmailNotifier.SendEmail(message);
			foreach (IMessenger m in m_messengerList) {
				result = m.SendMessageToTarget(message) || result;
			}
			return result;
		}

		public void MessengerMakeBuzz() {
			double lastBuzz = DateTime.Now.Subtract(m_lastBuzzDateTime).TotalSeconds;

			if (lastBuzz >= 20) {
				foreach (IMessenger mess in MZPState.Instance.m_messengerList) {
					//if (mess.IsConnected())
					{
						mess.MakeBuzz();
						m_lastBuzzDateTime = DateTime.Now;
					}
				}
			}
		}

		public void NotifyEventToUsers(MZPEvent mzpevent, string cause, bool excludeSource, bool notifyMessenger) {
			MLog.Log(this, "NotifyEvent " + mzpevent.DisplayMessage() + " cause:" + cause);
			if (notifyMessenger) {
				SendMessengerMessageToOne(cause + "; " + mzpevent.DisplayMessage());
				MessengerMakeBuzz();
			}

			if (mzpevent != null) {
				List<ZoneDetails> zonesToNotify = MultiZonePlayer.ZoneDetails.ZoneWithPotentialUserPresence_All;
				if (excludeSource && mzpevent.ZoneDetails != null) {
					zonesToNotify.RemoveAll(x => x.ZoneId == mzpevent.ZoneDetails.ZoneId);
				}

				MLog.Log(this, "NotifyEvent to m_valueList count=" + zonesToNotify.Count);

				ValueList vals = new ValueList();
				vals.Add(GlobalParams.command, GlobalCommands.notifyuser.ToString());

				foreach (ZoneDetails zone in zonesToNotify) {
					if (!mzpevent.ZoneDetails.IsNearbyZone(zone.ZoneId) && !zone.IsNearbyZone(mzpevent.ZoneDetails.ZoneId)) {
						vals.Set(GlobalParams.zoneid, zone.ZoneId.ToString());
						vals.Set(GlobalParams.sourcezoneid, mzpevent.ZoneDetails.ZoneId.ToString());
						API.DoCommand(vals);
					}
				}
			}
		}

		public CommandResult ExecuteMacro(string macroName) {
			MacroEntry entry = m_macroList.Find(x => x.Name == macroName);
			return ExecuteMacro(entry.Id);
		}

		public CommandResult ExecuteMacro(int macroId) {
			CommandResult cmdresult = new CommandResult();
			cmdresult.Result = ResultEnum.ERR;
			MacroEntry entry = m_macroList.Find(x => x.Id == macroId);
			if (entry != null) {
				cmdresult.ErrorMessage = "macro found but empty command list";
				foreach (MacroEntryCommand cmd in entry.CommandList) {
					MLog.Log(this, "Executing macro event " + cmd.Command + " in zone=" + cmd.ZoneName);
					ValueList val = new ValueList(GlobalParams.command,
						cmd.Command.ToString(), CommandSources.system);
					ZoneDetails zone = GetZoneIdByContainsName(cmd.ZoneName);
					if (zone != null) {
						val.Add(GlobalParams.zoneid, zone.ZoneId.ToString());
					}
					//parameters
					MacroEntry.AddParams(cmd.ParameterValueList, ref val);
					cmdresult = API.DoCommand(val);
					if (cmd.DelayMiliSec != 0) {
						Thread.Sleep(cmd.DelayMiliSec);
					}
				}
			}
			else {
				cmdresult.ErrorMessage = "Macro not found id=" + macroId;
				MLog.Log(this, cmdresult.ErrorMessage);
			}
			return cmdresult;
		}

		public CommandResult ExecuteRFXCmd(string cmd) {
			CommandResult cmdresult = new CommandResult();
			cmdresult.Result = ResultEnum.ERR;
			if (cmd != null && cmd.Length > 0) {
				RFXCom rfx = (RFXCom) m_messengerList.Find(x => x.GetType() == typeof (RFXCom));
				if (rfx != null) {
					cmdresult.Command = GlobalCommands.rfxcmd.ToString() + ":" + cmd;
					cmdresult.OutputMessage = rfx.SendCommand(cmd);
					cmdresult.Result = ResultEnum.OK;
				}
				else {
					cmdresult.ErrorMessage = "RFXcomm instance not found";
				}
			}
			else {
				cmdresult.ErrorMessage = "RFXcomm empty cmd";
			}
			return cmdresult;
		}

		public void CheckForWakeTimers() {
			String dt = DateTime.Now.ToString(IniFile.DATETIME_DAYHR_FORMAT);
			String weekday = DateTime.Now.DayOfWeek.ToString().Substring(0, 2);
			foreach (ZoneDetails zone in ZoneDetails.ZoneDetailsList) {
				if ((zone.WakeTime.Length > 0) && (zone.WakeTime.CompareTo(dt) == 0) && zone.WakeWeekDay.Contains(weekday)) {
					ValueList val = new ValueList(GlobalParams.command,
						GlobalCommands.musicalarm.ToString(), CommandSources.system);
					val.Add(GlobalParams.zoneid, zone.ZoneId.ToString());
					API.DoCommand(val);
				}
			}
		}

		public void CheckForScheduleMacroEvents() {
			LoadMacrosandRules();
			String entrymonth, entryday;
			String hrmin = DateTime.Now.ToString(IniFile.DATETIME_DAYHR_FORMAT);
			String weekday = DateTime.Now.DayOfWeek.ToString().Substring(0, 2).ToUpper();
			string month = DateTime.Now.Month.ToString(IniFile.DATETIME_MONTH_FORMAT).ToUpper();
			foreach (MacroEntry entry in m_macroList) {
				entrymonth = entry.RepeatMonth != null ? entry.RepeatMonth.ToUpper() : "";
				entryday = entry.RepeatWeekDay != null ? entry.RepeatWeekDay.ToUpper() : "";
				if ((entrymonth == "ALL" || entrymonth == month)
				    && (entryday == "ALL" || entryday.Contains(weekday))
				    && (entry.RepeatTime == hrmin)
				    && (DateTime.Now.Subtract(entry.ExecutedDateTime).TotalSeconds > 60)) {
					ExecuteMacro(entry.Id);
					entry.ExecutedDateTime = DateTime.Now;
				}
			}
		}

		public void CheckForUpsStatus() {
			foreach (GenericUPS ups in m_upsList) {
				ups.GetStatus();
			}
		}

		public void ZoneSlowTickActivities() {
			//Check Cron
			if (m_cron != null) {
				String cronText = "";

				foreach (ZoneDetails zone in ZoneDetails.ZoneDetailsList) {
					if (zone.CronSchedule != "") {
						cronText += zone.CronSchedule + "\n";
					}
				}
				if (cronText != m_cron.CronTabText) {
//cron definition has changed, reload
					m_cron.readCrontabString(cronText);
				}
			}

			//generate images
			/*foreach (ZoneDetails zone in ZoneDetails)
		{
			if (zone.HasTemperatureSensor || zone.HasHumiditySensor)
			{
				SimpleGraph graph = new SimpleGraph();
				graph.ShowTempGraph(zone.ZoneId, 48);
				graph.Close();
			}
		}*/
		}

		public int GetMacroIdByShortcut(string shortcut, string deviceName) {
			if (shortcut != null) {
				shortcut = shortcut.ToLower();
				deviceName = deviceName.ToLower();
				MacroEntry entry = m_macroList.Find(x =>
					x.ShortcutList != null
					&& x.ShortcutList.Find(y => y.Shortcut == shortcut) != null
					&& (x.ShortcutList.Find(y => deviceName.Contains(y.DeviceName)) != null));
				if (entry != null) {
					return entry.Id;
				}
			}
			return -1;
		}

		public void CheckForAlarm() {
			MZPState.Instance.SystemAlarm.IsMonitoringActive = Utilities.IsProcAlive("WinLoad");
		}

		public void CheckForExternalZoneEvents() {
			foreach (Display display in m_displayList) {
				//refresh display properties via direct read
				bool disp = display.IsOn;
				Display.InputTypeEnum input = display.InputType;
			}

			foreach (ZoneDetails zone in ZoneDetails.ZoneDetailsList) {
				if (!zone.IsActive) {
					if (zone.DisplayType.Equals(Display.DisplayTypeEnum.LGTV.ToString())) {
						Display display = m_displayList.Find(x => x.Connection.Equals(zone.DisplayConnection));

						if (display.GetType().Equals(typeof (DisplayLGTV))) {
							if (((DisplayLGTV) display).IsOnCached) {
								ValueList val = new ValueList(GlobalParams.command,
									GlobalCommands.tv.ToString(), CommandSources.system);
								val.Add(GlobalParams.zoneid, zone.ZoneId.ToString());
								API.DoCommand(val);
							}
						}
					}

					if (zone.DisplayType.Equals(Display.DisplayTypeEnum.XBMC.ToString())) {
						if (Utilities.IsProcAlive(IniFile.PARAM_XBMC_PROCESS_NAME[1])) {
							ValueList val = new ValueList(GlobalParams.command,
								GlobalCommands.xbmc.ToString(), CommandSources.system);
							val.Add(GlobalParams.zoneid, zone.ZoneId.ToString());
							API.DoCommand(val);
						}
					}
				}
			}
		}

		private void LogEvent(MZPEvent mzpevent) {
			string cause;
			MLog.LogEvent(mzpevent);

			if (mzpevent.ZoneDetails != null) {
				if (mzpevent.ZoneDetails.IsArmed) {
					cause = "Event detected on armed zone " + mzpevent.ZoneDetails.ZoneName;
					NotifyEventToUsers(mzpevent, cause, true, true);
				}

				if ((mzpevent.ZoneDetails.IsClosureArmed)
				    && (mzpevent.Source == EventSource.Closure)
				    && (mzpevent.ZoneDetails.IsClosureContactMade)) {
					cause = "Closure event detected on closure armed zone" + mzpevent.ZoneDetails.ZoneName;
					NotifyEventToUsers(mzpevent, cause, false, false);
				}

				if (mzpevent.ZoneDetails.MovementAlert && (mzpevent.ZoneDetails.IsArmed ||
				                                           (MZPState.Instance.SystemAlarm.AreaState.Equals(Alarm.EnumAreaState.armed)
				                                            &&
				                                            (mzpevent.ZoneDetails.AlarmAreaId ==
				                                             MZPState.Instance.SystemAlarm.AreaId)))) {
					cause = "Event detected on armed area";
					NotifyEventToUsers(mzpevent, cause, false, true);
				}
				else {
					/*  MLog.Log(this, "Ignoring alarm event on " + mzpevent.ZoneDetails.ZoneName + " movementalert=" + mzpevent.ZoneDetails.MovementAlert
						+ " zonealarmareaid=" + mzpevent.ZoneDetails.AlarmAreaId + " systemareaid=" + MZPState.Instance.SystemAlarm.AreaId
						+ " areastate=" + MZPState.Instance.SystemAlarm.AreaState);
					*/
				}
			}
			else {
//area event
				if (SystemAlarm.IsArmed) {
					switch (m_systemAlarm.AreaState) {
						case Alarm.EnumAreaState.entrydelay:
							cause = "Area entry when area is armed";
							NotifyEventToUsers(mzpevent, cause, false, true);
							break;
					}
				}
			}

			if ((mzpevent.TypeEv.Equals(MZPEvent.EventType.Security) &&
			     mzpevent.Importance.Equals(MZPEvent.EventImportance.Critical))) {
				cause = "Security critical event";
				NotifyEventToUsers(mzpevent, cause, false, true);
			}

			if (NotifyState.GTalkEnabled) {
				MLog.Log(this, mzpevent.Source + " event optional GTalk notify required");
				m_messengerList.Find(x => x.GetType().Equals(typeof (GTalkMessengers)))
					.SendMessageToTarget(mzpevent.DisplayMessage());
			}

			if (NotifyState.SMSEnabled) {
				MLog.Log(this, mzpevent.Source + " event optional SMS notify required");
				m_messengerList.Find(x => x.GetType().Equals(typeof (SMS))).SendMessageToTarget(mzpevent.DisplayMessage());
			}
		}

		public void LogEvent(DateTime dateTime, EventSource source, String message, MZPEvent.EventType type,
			MZPEvent.EventImportance importance, ZoneDetails zonedetails) {
			LogEvent(new MZPEvent(dateTime, source, message, type, importance, zonedetails));
		}

		public void LogEvent(EventSource source, String message, MZPEvent.EventType type, MZPEvent.EventImportance importance,
			ZoneDetails zonedetails) {
			LogEvent(new MZPEvent(DateTime.Now, source, message, type, importance, zonedetails));
		}

		public void CheckAlerts() {
			if (Alert.ActiveAlertList.Count > 0) {
				List<Alert> alerts = Alert.GetAlertsToSend();
				foreach (Alert alert in alerts) {
					if (SendMessengerMessageToAll(alert.InitialCause)) {
						alert.LastSendOK = DateTime.Now;
						alert.SendOKCount++;
						MLog.Log(this, "User alert notified succesfully, alert " + alert.LastCause);
					}
					else {
						alert.LastSendAttempt = DateTime.Now;
						alert.SendAttemptCount++;
						MLog.Log(this, "User alert notification Failed, alert " + alert.LastCause);
					}
				}
			}
		}

		public void CheckPresence() {
			DateTime startDisc = DateTime.Now;

			UserPresence.CheckLocalWifi();
			Performance.Create("Check Local Wifi", false, "",
				Performance.PerformanceFlags.Speed, DateTime.Now.Subtract(startDisc).TotalMilliseconds);
			startDisc = DateTime.Now;
			UserPresence.CheckRemoteWifi();
			Performance.Create("Check Remote Wifi", false, "",
				Performance.PerformanceFlags.Speed, DateTime.Now.Subtract(startDisc).TotalMilliseconds);
			startDisc = DateTime.Now;
			UserPresence.CheckRemoteBluetooth();
			Performance.Create("Check Remote BT", false, "",
				Performance.PerformanceFlags.Speed, DateTime.Now.Subtract(startDisc).TotalMilliseconds);

			Utilities.InternetConnectionState istate = Utilities.InternetConnectionState.INTERNET_CONNECTION_OFFLINE;
			Utilities.InternetGetConnectedState(ref istate, 0);
			//MLog.Log(this, "Internet Connection state is " + istate);

			if (UserPresence.UserIsNearList.Count == 0
			    && !SystemAlarm.IsArmed
			    && MultiZonePlayer.ZoneDetails.HasImmediateMove_All.Find(x => x.AlarmAreaId == SystemAlarm.AreaId) == null
			    && MultiZonePlayer.ZoneDetails.HasRecentMove_All.Find(x => x.AlarmAreaId == SystemAlarm.AreaId) == null) {
				Alert.CreateAlert("Safe Area not armed when all family members are out", null, false,
					Alert.NotificationFlags.NeedsImmediateUserAck, 120);
			}
		}

		private void CleanTempFiles() {
			Utilities.DeleteFilesOlderThan(IniFile.CurrentPath() + IniFile.WEB_PICTURES_SUBFOLDER, "*.*", 3);
			Utilities.DeleteFilesOlderThan(IniFile.CurrentPath() + IniFile.WEB_TMP_IMG_SUBFOLDER, "*.*", 1);
		}

		public void TickFast() {
			CheckAlerts();
		}

		public void TickSlow() {
			if (DateTime.Now.Subtract(m_initMZPStateDateTime).Duration().TotalSeconds
			    > Convert.ToInt16(IniFile.PARAM_BOOT_TIME_SECONDS[1])) {
				//delayed check, after complete boot
				if (MZPState.Instance == null) {
					return;
				}
				HealthCheckiSpy();
				if (MZPState.Instance == null) {
					return;
				}
				HealthCheckWinload();
				if (MZPState.Instance == null) {
					return;
				}
				CheckPresence();
				if (MZPState.Instance == null) {
					return;
				}
				AutoArmCheck();
				if (MZPState.Instance == null) {
					return;
				}
				HealthCheckMessengers();
			}
			if (MZPState.Instance == null) {
				return;
			}
			CheckForExternalZoneEvents();
			CleanTempFiles();
			if (MZPState.Instance == null) {
				return;
			}
			CheckForWakeTimers();
			if (MZPState.Instance == null) {
				return;
			}
			CheckForAlarm();
			if (MZPState.Instance == null) {
				return;
			}
			CheckForScheduleMacroEvents();
			if (MZPState.Instance == null) {
				return;
			}
			CheckForUpsStatus();
			if (MZPState.Instance == null) {
				return;
			}
			ZoneSlowTickActivities();
			if (MZPState.Instance == null) {
				return;
			}
			m_powerControlDenkovi.timerPowerSaving_Tick();
			m_powerControlNumato.timerPowerSaving_Tick();
			if (MZPState.Instance == null) {
				return;
			}
			if (MediaLibrary.AllAudioFiles != null) {
				MediaLibrary.AllAudioFiles.SaveUpdatedItems();
			}
		}
	}
}
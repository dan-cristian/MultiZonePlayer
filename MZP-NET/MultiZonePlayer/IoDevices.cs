using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Net;
using System.Runtime.InteropServices;
using InTheHand.Net;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using com.dalsemi.onewire;
using com.dalsemi.onewire.adapter;
using com.dalsemi.onewire.container;
using com.dalsemi.onewire.application.monitor;

namespace MultiZonePlayer {

	public class SensorDevice {
		
		public const string ONEWIRE_CONTROLLER_NAME = "DS1990A";
		public const string ONEWIRE_TEMPDEV_NAME = "DS18B20";
		public const string ONEWIRE_IO_NAME = "DS2406";
		public const string ONEWIRE_IO8_NAME = "DS2408";
		public const string ONEWIRE_SMARTBATDEV_NAME = "DS2438";
		public const string ONEWIRE_COUNTER_NAME = "DS2423";
		public const string ONEWIRE_IO2_NAME = "DS2413";
		
		public enum DeviceTypeEnum { OneWire, RFX };

		public String Name;
		public String Address;
		public String Family;
		public ZoneDetails Zone;
		public DateTime LastRead = DateTime.MinValue;
		public double Temperature;
		public int TemperatureResolutionIndex = -1;//-1 for non initialised
		public double Humidity;
		public double[] Voltage = new double[3];
		public double[] Counter = new double[2];
		public Boolean[] Activity = new Boolean[8];
		public Boolean HasActivitySensing = false;
		public Boolean[] Level = new Boolean[8];
		public Boolean[] State = new Boolean[8];
		public Boolean IOHigh = false, IOAlarm = false;
		public int IONumberChannels = 2;
		public Boolean IOHasVCC = false;
		public Boolean TempResolutionCanBeSet = true;
		public DeviceTypeEnum DeviceType;
		public String OtherInfo;
		public int Signal = -1;
		public int Battery = -1;
		private int m_errorCount = 0, m_successCount = 0;
		private Boolean m_isCounter = false, m_hasTemp = false, m_hasHum=false, m_hasVoltage = false, m_hasClosure = false;
		private DateTime m_startProc = DateTime.Now;
		private TimeSpan m_lastProcessingDuration = TimeSpan.MinValue;
		private TimeSpan m_minProcessingDuration = TimeSpan.MaxValue;
		private TimeSpan m_maxProcessingDuration = TimeSpan.MinValue;
		private DateTime m_lastHealthResetDateTime = DateTime.Now;

		private static List<SensorDevice> m_deviceList = new List<SensorDevice>();

		public static List<SensorDevice> DeviceList {
			get { return SensorDevice.m_deviceList.FindAll(x=>x.SuccessCount>0); }
		}
		
		public SensorDevice(String name, String address, String family, ZoneDetails zone, DeviceTypeEnum devtype, String otherInfo ) {
			Name = name;
			Address = address;
			Family = family;
			Zone = zone;
			LastRead = DateTime.Now;
			DeviceType = devtype;
			OtherInfo = otherInfo;

			switch (devtype ) {
				case (DeviceTypeEnum.OneWire):
					switch (name) {
						case ONEWIRE_COUNTER_NAME:
							m_isCounter = true;
							break;
						case ONEWIRE_TEMPDEV_NAME:
							m_hasTemp = true;
							break;
						case ONEWIRE_SMARTBATDEV_NAME:
							m_hasVoltage = true;
							m_hasTemp = true;
							break;
						case ONEWIRE_IO_NAME:
						case ONEWIRE_IO2_NAME:
						case ONEWIRE_IO8_NAME:
							m_hasClosure = true;
							break;
						default:
							Alert.CreateAlert("Unknown one wire device name " + name, true);
							break;
					}
					break;
				case DeviceTypeEnum.RFX:
					if (family == RFXDeviceDefinition.DeviceTypeEnum.temp_hum.ToString()){
						m_hasTemp = true;
						m_hasHum = true;
					}
					else {
						Alert.CreateAlert("Unknown RFX device name " + name, true);
					}
					break;
			}
		}

		public static void Clear(DeviceTypeEnum devtype) {
			m_deviceList.RemoveAll(x => x.DeviceType == devtype);
		}

		public static SensorDevice UpdateGetDevice(String name, String address, String family, ZoneDetails zone, DeviceTypeEnum devtype, String otherInfo) {
			SensorDevice dev = m_deviceList.Find(x => x.Address == address&& x.DeviceType == devtype);
			if (dev == null) {
				dev = new SensorDevice(name, address, family, zone, devtype, otherInfo);
				m_deviceList.Add(dev);
			}
			else {
				dev.Name = name;
				dev.Zone = zone;
				dev.Family = family;
				dev.OtherInfo = otherInfo;
			}
			dev.LastRead = DateTime.Now;
			return dev;
		}
		
		
		private String GetZoneName {
			get { return Zone != null ? Zone.ZoneName : "zoneN/A"; }
		}
		private int GetZoneId {
			get { return Zone != null ? Zone.ZoneId : -1; }
		}
		public static SensorDevice GetDevice(String deviceAddress) {
			List<SensorDevice> dev = m_deviceList.FindAll(x => x.Address.ToLower() == deviceAddress.ToLower());
			if (dev == null || dev.Count==0)
				return null;
			else{
				if (dev.Count > 1)
					Alert.CreateAlert("Unexpected multiple sensors with same address", true);
				return dev[0];
			}
		}
		public String Summary {
			get {
				String zone = GetZoneName;
				String s = Name + "-" + Address + ", err=" + ErrorCount + ", " + zone;
				s += ", " + Utilities.DurationAsTimeSpan(DateTime.Now.Subtract(LastRead)) + " ago, speed=" + Math.Round(m_lastProcessingDuration.TotalSeconds, 2) + "s";
				//s += " min=" + m_minProcessingDuration.TotalSeconds + "s" + " max=" + m_maxProcessingDuration.TotalSeconds + "s"; 
				if (m_hasTemp)
					s += ", temp=" + Temperature +" hum=" + Humidity;
				if (m_isCounter)
					s += " cnt1=" + Counter[0] + " cnt2=" + Counter[1];
				if (m_hasClosure) {
					for (int i = 0; i < IONumberChannels; i++) {
						if (HasActivitySensing)
							s += " act" + i + ":" + (Activity[i] ? "A" : "a");
						s += " lev"+i+":"+(Level[i]?"L":"l") + " state"+i+":"+(State[i]?"S":"s");
					}
					s += " vcc=" + IOHasVCC + " high="+IOHigh;
				}
				if (m_hasVoltage)
					s += " V1=" + Voltage[0] + " V2=" + Voltage[1] + " V3=" + Voltage[2];
				s += " success="+SuccessCount + " errors="+ErrorCount;
				if (DeviceType == DeviceTypeEnum.RFX) {
					s += " battery=" + Battery + " signal=" + Signal;
				}
				return s;
			}
		}

		public String DisplayValue {
			get {
				String val = "";
				if (m_isCounter)
					val = Counter[0] + ":" + Counter[1];
				if (m_hasVoltage)
					val = Voltage[0] + "/" + Voltage[1] + "/" + Voltage[2];
				if (m_hasClosure) {
					for (int i = 0; i < IONumberChannels; i++) {
						if (HasActivitySensing)
							val += (Activity[i] ? "A" : "a");
						val += (Level[i] ? "L" : "l") + (State[i] ? "S" : "s") + "/";
					}
				}
				if (m_hasHum)
					val = Humidity.ToString()+"%";
				if (m_hasTemp) {
					if (val != "")
						val += " ";
					val += Temperature + "°";
				}
				if (val == "") val = "n/a";
				return val;
			}
		}
		public String LastReadAge {
			get { return Utilities.DurationAsTimeSpan(DateTime.Now.Subtract(LastRead)); }
		}

		public double LastReadAgeInMinutes {
			get { return DateTime.Now.Subtract(LastRead).TotalMinutes; }
		}

		public String Type {
			get { return (HasTemp ? "Temperature" : "") + (HasHumidity ? "Humidity" : "") + (IsCounter ? "Counter" : "") 
				+ (HasVoltage ? "Voltage" : "") + (HasClosure ? "Closure" : ""); }
		}

		public Boolean HasClosure {
			get { return m_hasClosure; }
		}

		public Boolean HasVoltage {
			get { return m_hasVoltage; }
		}

		public Boolean HasTemp {
			get { return m_hasTemp; }
		}
		public Boolean HasHumidity {
			get { return m_hasHum; }
		}

		public Boolean IsCounter {
			get { return m_isCounter; }
		}

		public int SuccessCount {
			get { return m_successCount; }
		}

		public int ErrorCount {
			get { return m_errorCount; }
		}

		public void RecordError(Exception ex) {
			m_errorCount++;
			Utilities.AppendToCsvFile(IniFile.CSV_DEVICEERRORS, ",", GetZoneId.ToString(), DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT), 
				Address, Name, GetZoneName, DeviceType.ToString(), ex.Message);
		}
		public void RecordSuccess() {
			m_successCount++;
		}

		public int HealthPercent {
			get {
				if (m_successCount + m_errorCount > 0)
					return 100 * m_successCount / (m_successCount + m_errorCount);
				else
					return 0;
			}
		}
		public void StartProcessing() {
			if (DateTime.Now.Subtract(m_lastHealthResetDateTime).TotalHours >= 24) {
				m_successCount = 0;
				m_errorCount = 0;
				m_lastHealthResetDateTime = DateTime.Now;
			}
			m_startProc = DateTime.Now;
		}

		public void EndProcessing() {
			m_lastProcessingDuration = DateTime.Now.Subtract(m_startProc);
			m_maxProcessingDuration = m_lastProcessingDuration > m_maxProcessingDuration ? m_lastProcessingDuration : m_maxProcessingDuration;
			m_minProcessingDuration = m_lastProcessingDuration < m_minProcessingDuration ? m_lastProcessingDuration : m_minProcessingDuration;
		}
	}

	public class GenericModem : SerialBase, IMessenger {
		public enum ModemCommandsEnum {
			[Description("ATD ")] MODEM_CALL,
			[Description("AT")] MODEM_CHECK
		};

		public class ModemCommand {
			public String Id;
			public String ATCommand;
			public String Response;

			public ModemCommand(String Id, String ATCommand, String Response) {
				this.Id = Id;
				this.ATCommand = ATCommand;
				this.Response = Response;
			}
		}

		private List<ModemCommand> m_commandList;
		protected int m_stdtimeout = 2000;
		protected int m_calltimeout = 60000;
		protected int m_atlinescount, m_atdlinescount;

		~GenericModem() {
			Close();
		}

		public void Close() {
			Disconnect();
		}

		public virtual void Reinitialise() {
		}

		public void Reinitialise(string baud, string parity, string stopBits, string dataBits, string comport,
			CommunicationManager.TransmissionType type,
			int atlinescount, int atdlinescount) {
			Initialise(baud, parity, stopBits, dataBits, comport, type);
			//comm = new CommunicationManager(baud, parity, stopBits, dataBits, comport, this.handler);
			m_atlinescount = atlinescount;
			m_atdlinescount = atdlinescount;
			//comm.OpenPort();
			//m_waitForResponse = false;
			//m_lastOperationWasOK = true;
			m_commandList = new List<ModemCommand>();
		}

		public override string SendCommand(Enum cmd, string value) {
			throw new NotImplementedException();
		}

		public override string GetCommandStatus(Enum cmd) {
			throw new NotImplementedException();
		}

		protected override void ReceiveSerialResponse(string response) {
			MLog.Log(this, "Received unexpected MODEM serial response: " + response);

			String message = response.ToLower().Replace("\r", "").Replace("\n", "").ToLower();

			ReceiveMessage(message, "");
		}

		public void ReceiveMessage(String message, String sender) {
			MLog.Log(this, "ReceiveMessage from " + sender + " = " + message);
		}

		public Boolean SendMessageToTarget(String message) {
			MLog.Log(this, "ERROR send message not implemented in modem");
			return false;
		}

		public override bool TestConnection() {
			//if (base.TestConnection() == false) return false;

			try {
				String res =
					WriteCommand(Utilities.GetEnumDescription((ModemCommandsEnum) ModemCommandsEnum.MODEM_CHECK), m_atlinescount,
						m_stdtimeout).ToLower();
				if (res.Contains("ok") || res.Contains("at")) {
					return true;
				}
				else {
					MLog.Log(this, "Error health check GenericModem res=" + res);
					return true;
				}
			}
			catch (Exception ex) {
				MLog.Log(ex, "Exception TestCond GenericModem");
				return false;
			}
		}

		public Boolean IsConnected() {
			return (comm != null && comm.IsPortOpen());
		}

		public Boolean IsTargetAvailable() {
			return (comm != null && comm.IsPortOpen());
		}

		public void MakeBuzz() {
			MLog.Log(this, "Calling target number " + IniFile.PARAM_MODEM_TARGETNUMBER[1]);
			String res = WriteCommand(Utilities.GetEnumDescription((ModemCommandsEnum) ModemCommandsEnum.MODEM_CALL)
			                          + IniFile.PARAM_MODEM_TARGETNUMBER[1] + ";", m_atdlinescount, m_calltimeout);
			MLog.Log(this, "Calling target number done, res=" + res);
			//comm.ClosePort();
			//Reinitialise();
			WriteCommand("ATH", 2, 3000);
			MLog.Log(this, "Call reset");
		}
	}

	internal class Modem : GenericModem, IMZPDevice {
		private string m_deviceName;

		public Modem() {
			Reinitialise();
		}

		public override void Reinitialise() {
			System.Management.ManagementObjectSearcher mos =
				new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_POTSModem");
			foreach (System.Management.ManagementObject mo in mos.Get()) {
				MLog.Log(this, "Found Modem " + mo["Caption"].ToString() + " at port " + mo["AttachedTo"].ToString());
				if (mo["Caption"].ToString().ToLower().Contains(IniFile.PARAM_MODEM_DEVICE_NAME[1].ToLower())) {
					IniFile.PARAM_MODEM_COMPORT[1] = mo["AttachedTo"].ToString();
					MLog.Log(this, "Selected modem port is " + IniFile.PARAM_MODEM_COMPORT[1]);
					m_deviceName = mo["Caption"].ToString();
					break;
				}
			}
			MLog.Log(this, "Reinitialising Modem on com " + IniFile.PARAM_MODEM_COMPORT[1]);
			Reinitialise("9600", "None", "One", "8", IniFile.PARAM_MODEM_COMPORT[1], CommunicationManager.TransmissionType.Text,
				Convert.ToInt16(IniFile.PARAM_MODEM_AT_LINES_COUNT[1]),
				Convert.ToInt16(IniFile.PARAM_MODEM_ATD_LINES_COUNT[1]));
		}

		public bool IsFunctional() {
			return IsConnected();
		}

		public bool IsEnabled() {
			throw new NotImplementedException();
		}

		public void Enable() {
			throw new NotImplementedException();
		}

		public void Disable() {
			throw new NotImplementedException();
		}

		public string Type() {
			return IniFile.DEVICE_TYPE_TEL;
		}

		public string Name() {
			return m_deviceName;
		}
	}

	internal class SMS : GenericModem, IMZPDevice {
		public enum SMSCommandsEnum {
			[Description("AT+CMGF=1")] SMS_ENABLE,
			[Description("AT+CMGS=")] SMS_SEND,
			[Description("AT+GMM")] SMS_DEVICEINFO,
		};

		public SMS() {
			Reinitialise();
		}

		public override void Reinitialise() {
			MLog.Log(this, "Reinitialising SMS on com " + IniFile.PARAM_SMS_COMPORT[1]);
			Reinitialise("9600", "None", "One", "8", IniFile.PARAM_SMS_COMPORT[1], CommunicationManager.TransmissionType.Text,
				Convert.ToInt16(IniFile.PARAM_SMS_AT_LINES_COUNT[1]),
				Convert.ToInt16(IniFile.PARAM_SMS_ATD_LINES_COUNT[1]));
		}

		public new void SendMessageToTarget(String message) {
			WriteCommand(Utilities.GetEnumDescription((SMSCommandsEnum) SMSCommandsEnum.SMS_ENABLE),
				m_atlinescount, m_stdtimeout);
			WriteCommand(Utilities.GetEnumDescription((SMSCommandsEnum) SMSCommandsEnum.SMS_SEND) + "\""
			             + IniFile.PARAM_SMS_TARGETNUMBER[1] + "\"", m_atlinescount, m_stdtimeout);
			WriteCommand(message + (char) 26, 6, m_stdtimeout*3);
		}

		public bool IsFunctional() {
			return IsConnected();
		}

		public bool IsEnabled() {
			throw new NotImplementedException();
		}

		public void Enable() {
			throw new NotImplementedException();
		}

		public void Disable() {
			throw new NotImplementedException();
		}

		public string Type() {
			return IniFile.DEVICE_TYPE_TEL;
		}

		public string Name() {
			return "Cell Phone";
		}
	}

	internal class RFXCom : SerialBase, IMessenger, IMZPDevice {
		private static string CMD_RESET = "0D 00 00 00 00 00 00 00 00 00 00 00 00 00";
		private static string CMD_GETSTATUS = "0D 00 00 01 02 00 00 00 00 00 00 00 00 00";
		private static string CMD_SETMODE = "0D 00 00 05 03 53 00 00 00 2E 00 00 00 00"; //oregon&arc&ac&h eu	
		//"0D 00 00 06 03 53 00 00 00 26 00 00 00 00";//oregon&arc&ac
		//"0D 00 00 02 03 53 00 00 00 22 00 00 00 00";//oregon & arc

		public RFXCom() {
			Reinitialise();
		}

		public void Reinitialise() {
			IniFile.PARAM_RFXCOM_PORT[1] = Utilities.FindFTDIComPortFromDesc(IniFile.PARAM_RFX_DEVICE_NAME[1].ToLower(), true);
			MLog.Log(this, "Initialise RFX on COM " + IniFile.PARAM_RFXCOM_PORT[1]);
			Initialise("38400", "None", "One", "8", IniFile.PARAM_RFXCOM_PORT[1], CommunicationManager.TransmissionType.Text);
			SensorDevice.Clear(SensorDevice.DeviceTypeEnum.RFX);
			//comm = new CommunicationManager("38400", "None", "One", "8", 
			//	IniFile.PARAM_RFXCOM_PORT[1], this.handler);
			//comm.OpenPort();
			comm.CurrentTransmissionType = CommunicationManager.TransmissionType.Hex;
			//comm.WriteData("FF FA 2C 01 00 00 96 00 FF F0 FF FA 2C 02 08 FF F0 FF FA 2C 03 01 FF F0 FF FA 2C 04 01 FF F0 FF FA 2C 05 01 FF F0");
			WriteCommand(CMD_RESET, 1, 1000);
			//comm.WriteData(CMD_RESET);
			Thread.Sleep(1000);
			comm.Flush();
			GetStatus();
			WriteCommand(CMD_SETMODE, 1, 1000);
		}

		private string GetStatus() {
			string status = WriteCommand(CMD_GETSTATUS, 1, 1000);
			//MLog.Log(this, "RFX status is " + status);
			return status;
		}

		public List<SensorDevice> DeviceList {
			get {
				if (SensorDevice.DeviceList.Count > 0)
					//return only devices that were read ok once
					return SensorDevice.DeviceList.FindAll(y=>y.DeviceType == SensorDevice.DeviceTypeEnum.RFX).OrderByDescending(x => x.Type).ToList();
				else
					return SensorDevice.DeviceList;
			}
		}

		protected override void ReceiveSerialResponse(string response) {
			string origResponse = response;
			//MLog.Log(this, "RFXCOMM: " + response);
			do {
				RFXDeviceDefinition.RFXDevice dev = RFXDeviceDefinition.GetDevice(ref response);
				if (dev != null) {
					MLog.Log(this, "RFX result in zoneid=" + dev.ZoneId
					               + " for response:[" + origResponse + "] is " + dev.DisplayValues());
					if (dev.ZoneId != -1) {
						ZoneDetails zone = ZoneDetails.GetZoneById(dev.ZoneId);
						if (zone == null) {
							Alert.CreateAlertOnce("Null zone " + dev.ZoneId + " in RFXrecvresp", "RFXReceiveSerialResponse");
							return;
						}
						SensorDevice devsensor = SensorDevice.UpdateGetDevice(dev.DeviceName, dev.DeviceId, dev.DeviceType.ToString(), zone, 
							SensorDevice.DeviceTypeEnum.RFX, dev.DisplayValues());
						
						switch (dev.DeviceType) {
							case RFXDeviceDefinition.DeviceTypeEnum.temp_hum:
								decimal temp, hum, lasttemp, lasthum, signal, battery;
								temp = Convert.ToDecimal(
										dev.FieldValues.Find(x => x.Name == RFXDeviceDefinition.DeviceAttributes.temperature.ToString()).Value)/10;
								hum = Convert.ToDecimal(
										dev.FieldValues.Find(x => x.Name == RFXDeviceDefinition.DeviceAttributes.humidity.ToString()).Value);
								signal = Convert.ToDecimal(
										dev.FieldValues.Find(x => x.Name == RFXDeviceDefinition.DeviceAttributes.signal.ToString()).Value);
								battery = Convert.ToDecimal(
										dev.FieldValues.Find(x => x.Name == RFXDeviceDefinition.DeviceAttributes.battery.ToString()).Value);

								lasttemp = Convert.ToDecimal(zone.Temperature);
								lasthum = Convert.ToDecimal(zone.Humidity);

								lasttemp = lasttemp == 0 ? 0.1m : lasttemp;
								lasthum = lasthum == 0 ? 0.1m : lasthum;
								zone.SetTemperature((double) temp, dev.DeviceId);
								zone.Humidity = (double) hum;
								devsensor.Temperature = (double) temp;
								devsensor.Humidity = (double)hum;
								devsensor.Signal = (int)signal;
								devsensor.Battery = (int)battery;
								devsensor.RecordSuccess();
								break;
							case RFXDeviceDefinition.DeviceTypeEnum.lighting1:

								break;
							default:
								MLog.Log(this, "Unknown RFX device type in response " + response);
								break;
						}
					}
					else {
						MLog.Log(this, "Unknown zone for RFX response " + response);
					}
				}
				else {
					MLog.Log(this, "No RFX device identified for response " + response);
				}
			} while (response.Length > 0 && MZPState.Instance != null);

			//String message = response.ToLower().Replace("\r", "").Replace("\n", "").ToLower();
		}

		public override string GetCommandStatus(Enum cmd) {
			throw new NotImplementedException();
		}

		public override string SendCommand(Enum cmd, string value) {
			throw new NotImplementedException();
		}

		public string SendCommand(string value) {
			return WriteCommand(value, 1, 1000);
		}

		public void Close() {
			Disconnect();
		}

		public Boolean IsConnected() {
			return true;
		}

		public void MakeBuzz() {
		}

		public void ReceiveMessage(String message, String sender) {
			//MLog.Log(this, "SMS from " +sender + " = " + message);
		}

		public Boolean SendMessageToTarget(String message) {
			MLog.Log(this, "RFX comm does not implement sendmessage");
			return false;
		}

		public override bool TestConnection() {
			if (GetStatus() != "") {
				return true;
			}
			else {
				return false;
			}
		}

		public bool IsFunctional() {
			return IsConnected();
		}

		public bool IsEnabled() {
			throw new NotImplementedException();
		}

		public void Enable() {
			throw new NotImplementedException();
		}

		public void Disable() {
			throw new NotImplementedException();
		}

		public string Type() {
			return IniFile.DEVICE_TYPE_RADIO;
		}

		public string Name() {
			return "RFXCom";
		}
	}

	public class WDIO : GenericModem, IMessenger, IMZPDevice {
		private String m_lastState, m_channel;
		public const string WDIO_ATTRIB_SWITCH = "iopin=", WDIO_ATTRIB_BUTTON = "iobutton=", WDIO_ATTRIB_OUTPUT="ioout=";
		private const int WDIO_PIN_COUNT = 14;
		private const char STATE_CONTACT_MADE = '0', STATE_CONTACT_NOTMADE = '1', STATE_UNDEFINED = '?';
		private int[] m_zoneIdMap = new int[WDIO_PIN_COUNT];
		private const char CMD_SWITCH = 'S', CMD_BUTTON = 'B', CMD_PARAM_LOW = 'L', CMD_PARAM_HIGH = 'H';

		public String State {
			get { return m_lastState; }
		}

		public WDIO() {
			Reinitialise();
		}

		public override void Reinitialise() {
			ManagementItem item =
				ManagementItem.GetManagementItemsInfo()
					.Find(x => x.Manufacturer.ToLower().Equals(IniFile.PARAM_WDIO_MANUFACTURER_NAME[1].ToLower()));
			if (item != null) {
				int start = item.Name.IndexOf("COM");
				string port = item.Name.Substring(start);
				port = port.Remove(port.Length - 1);
				IniFile.PARAM_WDIO_COMPORT[1] = port;
			}
			MLog.Log(this, "Reinitialising WDIO on com " + IniFile.PARAM_WDIO_COMPORT[1]);
			Reinitialise("9600", "None", "One", "8", IniFile.PARAM_WDIO_COMPORT[1],
				CommunicationManager.TransmissionType.TextR,
				Convert.ToInt16(IniFile.PARAM_SMS_AT_LINES_COUNT[1]),
				Convert.ToInt16(IniFile.PARAM_SMS_ATD_LINES_COUNT[1]));
			comm.VerboseDebug = true;
			m_channel = IniFile.PARAM_WDIO_CHANNEL[1];
			//ClearPins();
			m_lastState = "";
			for (int i = 0; i < m_zoneIdMap.Length; i++) {
				m_zoneIdMap[i] = Constants.NOT_SET;
				m_lastState += STATE_CONTACT_NOTMADE;
			}
			foreach (ZoneDetails zone in ZoneDetails.ZoneDetailsList) {
				SetPinTypes(zone);
			}
		}

		public void SetPinTypes(ZoneDetails zone) {
			string pin, result;
			char iocmd = '?';
			int startindex, end, pinindex, attriblength = 0;

			startindex = zone.ClosureIdList.IndexOf(WDIO_ATTRIB_SWITCH);
			if (startindex != -1) {
				iocmd = CMD_SWITCH;
				attriblength = WDIO_ATTRIB_SWITCH.Length;
			}
			else {
				startindex = zone.ClosureIdList.IndexOf(WDIO_ATTRIB_BUTTON);
				if (startindex != -1) {
					iocmd = CMD_BUTTON;
					attriblength = WDIO_ATTRIB_BUTTON.Length;
				}
				else {
					startindex = zone.ClosureIdList.IndexOf(WDIO_ATTRIB_OUTPUT);
					if (startindex != -1) {
						switch (zone.RelayType) {
							case EnumRelayType.NormalOpen:
								iocmd = CMD_PARAM_LOW;//set as output and turn on or off (depends)
								break;
							case EnumRelayType.NormalClosed:
								iocmd = CMD_PARAM_HIGH;
								break;
							default:
								Alert.CreateAlert("Relay type not set for WDIO at init", true);
								iocmd = CMD_PARAM_LOW;
								break;
						}
						attriblength = WDIO_ATTRIB_OUTPUT.Length;
					}
				}
			}
			if (startindex != -1) {
				end = zone.ClosureIdList.IndexOf(";", startindex);
				if (end == -1) {
					end = zone.ClosureIdList.Length - 1;
				}
				pin = zone.ClosureIdList.Substring(startindex + attriblength, end - startindex - attriblength + 1);
				if (int.TryParse(pin, out pinindex)) {
					pinindex = Convert.ToInt16(pin);
				}
				else {
					MLog.Log(this, "Error parsing pin=" + zone.ClosureIdList + " for zone " + zone.ZoneName);
				}
				switch (iocmd) {
					case CMD_PARAM_HIGH:
					case CMD_PARAM_LOW:
						zone.WDIORelayOutputPinIndex = pinindex;
						break;
					case CMD_BUTTON:
						zone.WDIOButtonPinIndex = pinindex;
						break;
					case CMD_SWITCH:
						zone.WDIOSwitchPinIndex = pinindex;
						break;
				}

				if (pinindex >= 0 && pinindex < m_zoneIdMap.Length) {
					m_zoneIdMap[pinindex] = zone.ZoneId;
					Thread.Sleep(100);//pause between wdio writes
					result = WriteCommand(m_channel + iocmd + Convert.ToChar('A' + pinindex), 1, 1000);
					MLog.Log(this, "Set IO pin index= " + pinindex + "on zone=" + zone.ZoneName + " cmd=" +iocmd 
						+ " result=" + result);
				}
				else {
					MLog.Log(this, "Error pin index in zone " + zone.ZoneName + " is out of range: " + pinindex);
				}
				//set all unused port to output not on
				for (int i = 0; i < m_zoneIdMap.Length; i++) {
					if (m_zoneIdMap[i] == Constants.NOT_SET) {
						Thread.Sleep(100);//pause between wdio writes
						MLog.Log(this, "Setting off unused wdio pin index" + i);
						WriteCommand(m_channel + CMD_PARAM_LOW + Convert.ToChar('A' + i), 1, 1000);
					}
				}
			}
		}
		/// <summary>
		/// Turn relay on
		/// </summary>
		/// <param name="pinindex">0 to n</param>
		public void SetOutputHigh(int pinindex) {
			String result = WriteCommand(m_channel + CMD_PARAM_HIGH + Convert.ToChar('A' + pinindex), 1, 1000);
			MLog.Log(this, "WDIO Set output High returned " + result);
			ReceiveSerialResponse(result);

		}
		/// <summary>
		/// Turn relay off
		/// </summary>
		/// <param name="pinindex">0 to n</param>
		public void SetOutputLow(int pinindex) {
			String result = WriteCommand(m_channel + CMD_PARAM_LOW + Convert.ToChar('A' + pinindex), 1, 1000);
			MLog.Log(this, "WDIO Set output Low returned " + result);
			ReceiveSerialResponse(result);
		}
		public override Boolean TestConnection() {
			if (comm == null || !comm.IsPortOpen()) {
				return false;
			}

			//String result = WriteCommand("\r\n", 1, 100);
			return true;
		}

		protected override void ReceiveSerialResponse(string response) {
			char channel, pin, state;
			int pinindex, zoneid;
			//base.ReceiveSerialResponse(response);
			if (response.Length >= 3) {
				channel = response[0]; //filter later, now all is A
				pin = response[1];
				//check for output response
				switch (response[1]) {
					case CMD_PARAM_LOW:
						state = STATE_CONTACT_NOTMADE;
						pin = response[2];
						break;
					case CMD_PARAM_HIGH:
						state = STATE_CONTACT_MADE;
						pin = response[2];
						break;
					default:
						if (response[2] == CMD_PARAM_LOW) {
							state = STATE_CONTACT_NOTMADE;
						}
						else if (response[2] == CMD_PARAM_HIGH) {
							state = STATE_CONTACT_MADE;
						}
						else
							state = STATE_UNDEFINED;
						break;
				}
				if (state != STATE_UNDEFINED) {
					if (state == STATE_CONTACT_MADE) {
						MLog.Log(this, "CONTACT MADE on pin=" + pin);
					}
					pinindex = Convert.ToInt16(pin) - Convert.ToInt16('A');
					zoneid = m_zoneIdMap[pinindex];
					//if (m_lastState[pinindex] != state)
					{
						ValueList val = new ValueList(GlobalParams.zoneid,
							zoneid.ToString(), CommandSources.rawinput);
						val.Add(GlobalParams.cmdsource, CommandSources.rawinput.ToString());
						val.Add(GlobalParams.command, GlobalCommands.closure.ToString());
						val.Add(GlobalParams.id, pinindex.ToString());
						val.Add(GlobalParams.iscontactmade, (state == STATE_CONTACT_MADE).ToString());
						API.DoCommand(val);
					}
					//else
					//	MLog.Log(this, "WDIO nochage event on pin=" + pinindex + " state=" + state);
					m_lastState = m_lastState.Substring(0, pinindex) + state
					              + m_lastState.Substring(pinindex + 1);
					m_lastState = m_lastState.Substring(0, WDIO_PIN_COUNT);
				}
				else {
					MLog.Log(this, "Undefined state on response=" + response);
				}
			}
		}

		public bool IsFunctional() {
			return IsConnected();
		}

		public bool IsEnabled() {
			throw new NotImplementedException();
		}

		public void Enable() {
			throw new NotImplementedException();
		}

		public void Disable() {
			throw new NotImplementedException();
		}

		public string Type() {
			return IniFile.DEVICE_TYPE_IO;
		}

		public string Name() {
			return "WDIO";
		}
	}

	public class GPIO : GenericModem, IMessenger, IMZPDevice {
		private String m_state, m_lastState;
		private const string STR_ENDLINE = ">", GPIO_ATTRIB_INI_NAME = "iopin=";
		private const int GPIO_PIN_COUNT = 32;

		private const char STATE_CONTACT_MADE = '0',
			STATE_CONTACT_NOTMADE = '1',
			STR_TIMEOUT_CHAR = '?',
			STR_EXCEPTION_CHAR = '!';

		private int[] m_zoneIdMap = new int[GPIO_PIN_COUNT];

		public String State {
			get { return m_lastState; }
		}

		public GPIO() {
			Reinitialise();
		}

		public override void Reinitialise() {
			ManagementItem item =
				ManagementItem.GetManagementItemsInfo()
					.Find(x => x.Manufacturer.ToLower().Equals(IniFile.PARAM_GPIO_CDC_MANUFACTURER_NAME[1].ToLower()));
			if (item != null) {
				int start = item.Name.IndexOf("COM");
				string port = item.Name.Substring(start);
				port = port.Remove(port.Length - 1);
				IniFile.PARAM_GPIO_CDC_COMPORT[1] = port;
			}
			MLog.Log(this, "Reinitialising GPIO on com " + IniFile.PARAM_GPIO_CDC_COMPORT[1]);
			Reinitialise("38400", "None", "One", "8", IniFile.PARAM_GPIO_CDC_COMPORT[1],
				CommunicationManager.TransmissionType.TextCR,
				Convert.ToInt16(IniFile.PARAM_SMS_AT_LINES_COUNT[1]),
				Convert.ToInt16(IniFile.PARAM_SMS_ATD_LINES_COUNT[1]));
			comm.VerboseDebug = false;
			ClearPins();

			for (int i = 0; i < m_zoneIdMap.Length; i++) {
				m_zoneIdMap[i] = -1;
				m_lastState += STATE_CONTACT_NOTMADE;
			}
			string pin;
			int startindex, end, pinindex;
			foreach (ZoneDetails zone in ZoneDetails.ZoneDetailsList) {
				startindex = zone.ClosureIdList.IndexOf(GPIO_ATTRIB_INI_NAME);
				if (startindex != -1) {
					end = zone.ClosureIdList.IndexOf(";", startindex);
					if (end == -1) {
						end = zone.ClosureIdList.Length - 1;
					}
					pin = zone.ClosureIdList.Substring(startindex + GPIO_ATTRIB_INI_NAME.Length,
						end - startindex - GPIO_ATTRIB_INI_NAME.Length + 1);
					if (int.TryParse(pin, out pinindex)) {
						pinindex = Convert.ToInt16(pin);
					}
					else {
						MLog.Log(this, "Error parsing pin=" + zone.ClosureIdList + " for zone " + zone.ZoneName);
					}
					if (pinindex >= 0 && pinindex < m_zoneIdMap.Length) {
						m_zoneIdMap[pinindex] = zone.ZoneId;
					}
					else {
						MLog.Log(this, "Error pin index in zone " + zone.ZoneName + " is out of range: " + pinindex);
					}
				}
			}
		}

		public override Boolean TestConnection() {
			if (comm == null || !comm.IsPortOpen()) {
				return false;
			}

			//String result = WriteCommand("\r\n", 1, 100);
			return true;
		}

		private void ClearPins() {
			WriteCommand("", 1, 100);
			Thread.Sleep(100);
			String cmd, result, cmdprefix = "gpio clear ";

			for (int i = 0; i < GPIO_PIN_COUNT; i++) {
				cmd = cmdprefix + i;
				result = WriteCommand(cmd, 1, 300, STR_ENDLINE);
			}

			MLog.Log(this, "GPIO Pins cleared");
		}

		public void LoopForEvents() {
			String pinstate, cmd;
			int zoneid, j;
			String cmdprefix = "gpio read ";
			//int start;

			do {
				m_state = "";
				try {
					//MLog.Log(this, "Read GPIO " + m_state);
					if (!IsFaulty()) {
						for (int i = 0; i < GPIO_PIN_COUNT; i++) {
							cmd = cmdprefix + i;
							pinstate = WriteCommand(cmd, 1, 300, STR_ENDLINE);
							pinstate = pinstate.Replace(STR_TIMEOUT, STR_TIMEOUT_CHAR.ToString())
								.Replace(STR_EXCEPTION, STR_EXCEPTION_CHAR.ToString());
							/*start = pinstate.IndexOf(cmdprefix);
							if (start != -1)
							{
								pin = pinstate.Substring(start+cmdprefix.Length, 2).Replace("\n", "").Replace("\r", "");
								pinstate = pinstate.Replace(cmdprefix + pin, "").Replace("\n", "").Replace("\r", "").Replace(">", "");
								m_state += pinstate;
								//Thread.Sleep(10);
							}
							 */

							//else						MLog.Log(this, "GPIO response unexpected " + pinstate);
							pinstate = pinstate.Replace(cmd, "").Replace("\n", "").Replace("\r", "").Replace(">", "");
							m_state += pinstate;
						}

						if (m_state != m_lastState) {
							for (int i = 0; i < Math.Min(m_state.Length, m_lastState.Length); i++) {
								if (m_state[i] != m_lastState[i]) {
									MLog.Log(this, "Pin " + i + " changed to " + m_state[i]);
									if (m_state[i] != STR_TIMEOUT_CHAR && m_state[i] != STR_EXCEPTION_CHAR) //ignore timeouts on pin reads
									{
										zoneid = m_zoneIdMap[i];
										if (zoneid != -1) {
											ValueList val = new ValueList(GlobalParams.zoneid,
												zoneid.ToString(), CommandSources.rawinput);
											val.Add(GlobalParams.cmdsource, CommandSources.rawinput.ToString());
											val.Add(GlobalParams.command, GlobalCommands.closure.ToString());
											val.Add(GlobalParams.id, GPIO_ATTRIB_INI_NAME + i);
											val.Add(GlobalParams.iscontactmade, (m_state[i] == STATE_CONTACT_MADE).ToString());
											API.DoCommand(val);
										}
									}
								}
							}
						}

						m_lastState = m_state;
					}
				}
				catch (Exception ex) {
					MLog.Log(ex, this, "Error in loopforevents");
				}

				if (IsFaulty()) {
					j = 0;
					MLog.Log(this, "Serial connection faulty, sleeping for a while");
					do {
						j++;
						Thread.Sleep(1000);
					} while (MZPState.Instance != null && j < 60*15);
				}
			} while (MZPState.Instance != null);
		}

		public bool IsFunctional() {
			return IsConnected();
		}

		public bool IsEnabled() {
			throw new NotImplementedException();
		}

		public void Enable() {
			throw new NotImplementedException();
		}

		public void Disable() {
			throw new NotImplementedException();
		}

		public string Type() {
			return IniFile.DEVICE_TYPE_RELAY;
		}

		public string Name() {
			return "GPIO";
		}
	}

	public class OneWire : IMessenger, DeviceMonitorEventListener, IMZPDevice {
		private List<DSPortAdapter> m_adapterList = new List<DSPortAdapter>();
		//private DeviceMonitor dMonitor;
		//private Thread m_searchThread;
		
		
		
		private DateTime m_lastOKRead = DateTime.Now;
		private Dictionary<string, string> m_deviceAttributes = new Dictionary<string, string>();
		private int m_initErrors = 0;

		public List<SensorDevice> DeviceList {
			get {
				if (SensorDevice.DeviceList.Count > 0)
					//return only devices that were read ok once
					return SensorDevice.DeviceList.FindAll(y=>y.DeviceType==SensorDevice.DeviceTypeEnum.OneWire).OrderByDescending(x => x.Type).ToList();
				else
					return SensorDevice.DeviceList;
			}
		}


		public OneWire() {
			Reinitialise();
		}

		public Boolean SendMessageToTarget(string message) {
			MLog.Log(this, "Error not implemented");
			return false;
		}

		public void ReceiveMessage(string message, string sender) {
			throw new NotImplementedException();
		}

		public bool TestConnection() {
			return true; //dMonitor.isRunning;
		}

		private const int ONEWIRE_ERR_COUNT_NOSILENT = 5;
		public void Reinitialise() {
			try {
				SensorDevice.Clear(SensorDevice.DeviceTypeEnum.OneWire);
				foreach (DSPortAdapter adapter in m_adapterList) {
					try { adapter.freePort(); }
					catch (Exception) { }
				}
				m_adapterList.Clear();
				DSPortAdapter adapterVar;
				if (m_initErrors < ONEWIRE_ERR_COUNT_NOSILENT)
					MLog.Log(this, "Initialising OneWire");
				try {
					adapterVar = null;
					adapterVar = OneWireAccessProvider.getDefaultAdapter();
					m_lastOKRead = DateTime.Now;
					m_initErrors = 0;
					m_adapterList.Add(adapterVar);
					MLog.Log("Found onewire Default adapter " + adapterVar.getAdapterName());
				}
				catch (Exception) {
					if (m_initErrors < 5) {
						MLog.Log(this, "Unable to init onewire default adapter");
					}
				}

				String[] adapterNames = IniFile.PARAM_ONEWIRE_ADAPTER_NAME[1].Split(';');
				String[] adapterPorts = IniFile.PARAM_ONEWIRE_ADAPTER_PORTNAME[1].Split(';');
				if (adapterNames.Length == adapterPorts.Length) {
					for (int i = 0; i < adapterNames.Length; i++) {
						if (m_initErrors < ONEWIRE_ERR_COUNT_NOSILENT) {
							MLog.Log(this, "Trying to initialize 1wire adapter " + adapterNames[i] + adapterPorts[i]);
						}
						try {
							adapterVar = OneWireAccessProvider.getAdapter(adapterNames[i], adapterPorts[i]);
							adapterVar.beginExclusive(true);
							adapterVar.setSearchAllDevices();
							adapterVar.targetAllFamilies();
							java.util.Enumeration containers = adapterVar.getAllDeviceContainers();

							m_lastOKRead = DateTime.Now;
							m_initErrors = 0;
							if (m_adapterList.Find(x => x.getAddressAsString() == adapterVar.getAddressAsString()) == null) {
								m_adapterList.Add(adapterVar);
								MLog.Log("Found onewire port based adapter " + adapterVar.getAdapterVersion()
									+ " " + adapterVar.getAdapterName() + adapterVar.getAddressAsString());
							}
							else
								MLog.Log(this, "Adapter already initialised previously, skipping");
							adapterVar.endExclusive();
						}
						catch (Exception) { }
					}
				}
				else
					Alert.CreateAlert("1wire adapter names and ports in ini file do not have same length, namesLen="
						+ adapterNames.Length + " ports=" + adapterPorts.Length, true);

				if (m_adapterList.Count == 0) {
					m_initErrors++;
					if (m_initErrors > 100)
						m_initErrors = 0;
					if (m_initErrors < ONEWIRE_ERR_COUNT_NOSILENT) {
						MLog.Log(this, "Unable to init onewire adapter name=" + IniFile.PARAM_ONEWIRE_ADAPTER_NAME[1]
							+ " port =" + IniFile.PARAM_ONEWIRE_ADAPTER_PORTNAME[1]);
						return;
					}
					//MLog.Log(this, "OneWire adapter=" + adapter.getAdapterName() + " port=" + adapter.getPortName());

					foreach (DSPortAdapter adapter in m_adapterList) {
						// get exclusive use of adapter
						adapter.beginExclusive(true);
						// clear any previous search restrictions
						adapter.setSearchAllDevices();
						adapter.targetAllFamilies();
						adapter.setSpeed(DSPortAdapter.SPEED_REGULAR);
						java.util.Enumeration containers = adapter.getAllDeviceContainers();
						OneWireContainer element; //TemperatureContainer temp;
						//sbyte[] state;
						while (containers.hasMoreElements()) {
							element = (OneWireContainer)containers.nextElement();
							List<ZoneDetails> zoneList = ZoneDetails.GetZonesWithOneWire(element.getAddressAsString());
							String zonename = zoneList.Count > 0 ? zoneList[0].ZoneName + ", zonecount=" + zoneList.Count : "ZONE NOT ASSOCIATED";
							MLog.Log(this, "OneWire device found adapter=" + adapter.getAdapterName() + " zone=" + zonename + ", addr=" + element.getAddressAsString()
										   + " name=" + element.getName() + " altname=" + element.getAlternateNames()
										   + " speed=" + element.getMaxSpeed()
										   + " desc=" + element.getDescription());
						}
						adapter.endExclusive();

						/*// Monitor of the network
						dMonitor = new DeviceMonitor(adapter);
						dMonitor.setDoAlarmSearch(false);
						// setup event listener (should point to this form)
						dMonitor.addDeviceMonitorEventListener(this);
						m_searchThread = new Thread(() => dMonitor.run());
						m_searchThread.Name = "OneWire Search";
						m_searchThread.Start();
						*/
					}
				}
			}
			catch (Exception ex) {
				MLog.Log(ex, this, ex.Message);
			}
		
		}

		public void SetResolution(OneWireContainer tempelement, ZoneDetails zone, SensorDevice dev, double readTemperature) {
			try {
				if (zone.TemperatureResolutionDigits == -1 || !dev.TempResolutionCanBeSet)
					return;//default setting is wanted, or temp resolution cannot be set, just exit

				int fractionalPart=0;
				if (readTemperature != Math.Round(readTemperature,0))
					fractionalPart= new System.Version(readTemperature.ToString()).Minor;
				//set once if resolutionList not yet set properly. use > comparison as digits are not 0 right filled
				if (dev.TemperatureResolutionIndex == -1 || fractionalPart.ToString().Length > zone.TemperatureResolutionDigits) {
					TemperatureContainer tc = (TemperatureContainer)tempelement;
					//Boolean selectable = tc.hasSelectableTemperatureResolution();
					double[] resolutionList = null;
					sbyte[] state = tc.readDevice();
					//if (selectable) {
					resolutionList = tc.getTemperatureResolutions();
					//}
					int resolIndex = -1;
					String resolStringList = "";
					for (int i = 0; i < resolutionList.Length;i++ ) {
						resolStringList += resolutionList[i] + ";";
						if (new System.Version((resolutionList[i]).ToString()).Minor.ToString().Length == zone.TemperatureResolutionDigits) {
							resolIndex = i;
							break;
						}
					}
					if (resolIndex != -1) {
						MLog.Log(this, "Setting temperature resolutionList to " + resolutionList[resolIndex] + " in " + zone.ZoneName);
						tc.setTemperatureResolution(resolutionList[resolIndex], state);
						tc.writeDevice(state);
						dev.TemperatureResolutionIndex = resolIndex;
					}
					else {
						Alert.CreateAlert("Unable to set resolution digits to " + zone.TemperatureResolutionDigits + " for zone " + zone.ZoneName
							+ " as we only have " + resolutionList.Length + " resolutions, " + resolStringList, false);
						dev.TempResolutionCanBeSet = false;
					}
				}
			}
			catch (Exception ex)
			{
				MLog.Log(this, "Error, could not set resolutionList on tempsensor: " + ex.Message);
			}
		}

		// The following 3 methods implement the J# interface "DeviceMonitorEventListener" from the 1-Wire API (J#).  
		// These methods are: deviceArrival, deviceDeparture, and networkException.
		//

		// Departure method for the 1-Wire API's DeviceMonitorEventListener interface
		public void deviceDeparture(DeviceMonitorEvent devt) {
			int i = 0;
			for (i = 0; i < devt.getDeviceCount(); i++) {
				MLog.Log(this, "OneWire Departing: " + devt.getAddressAsStringAt(i));
				//m_deviceList.Remove(devt.getAddressAsStringAt(i));
			}
		}

		// Arrival method for the 1-Wire API's DeviceMonitorEventListener interface
		public void deviceArrival(DeviceMonitorEvent devt) {
			int i = 0;
			for (i = 0; i < devt.getDeviceCount(); i++) {
				MLog.Log(this, "OneWire Arriving: " + devt.getAddressAsStringAt(i));
				//m_deviceList.Add(devt.getAddressAsStringAt(i));
			}
		}

		// exception for DeviceMonitor
		public void networkException(DeviceMonitorException dexc) {
			MLog.Log(this, "1-Wire network exception occurred: " + dexc.exception);
		}

		//Family codes http://owfs.sourceforge.net/family.html
		private void ProcessFamily(String familyName, DSPortAdapter adapter, Boolean verboseLog, Boolean tryOverdrive, params sbyte[] family) {
			if (adapter != null) {
				lock (adapter)
				try {
					// get exclusive use of adapter
					adapter.beginExclusive(true);
					// clear any previous search restrictions
					adapter.setSearchAllDevices();
					//adapter.targetAllFamilies();
					adapter.targetFamily(family);
					Boolean speedSet = false;
					try {
						if (tryOverdrive) {
							try {
								//adapter.setSpeed(DSPortAdapter.SPEED_OVERDRIVE);
								//speedSet = true;
								//MLog.Log(this, "Overdrive speed on family=" + family);
							}
							catch (Exception)
							{}
						}
						if (!speedSet)
							adapter.setSpeed(DSPortAdapter.SPEED_REGULAR);
					}
					catch (Exception) {
						MLog.Log(this, "Error setting regular speed on family=" + family + ", forcing reinit.");
						//adapter = null; //force reinit
						//adapter.setSpeed(DSPortAdapter.SPEED_FLEX);
					}
					java.util.Enumeration containers = adapter.getAllDeviceContainers();
					OneWireContainer element;
					List<ZoneDetails> zoneList;
					string familyString="";
					ZoneDetails zone;
					foreach (sbyte sfam in family) {
						familyString += sfam+";";
					}
					//m_deviceList.RemoveAll(x => x.Family == familyString);
					DateTime start = DateTime.Now;
					int elementCount = 0, errCount = 0;
					String address, sensorName;
					SensorDevice dev;
					while (MZPState.Instance != null && containers.hasMoreElements()) {
						element = (OneWireContainer) containers.nextElement();
						address = element.getAddressAsString();
						zoneList = ZoneDetails.GetZonesWithOneWire(address);
						if (zoneList.Count == 0)
							Alert.CreateAlertOnce("Onewire device not associate, adress=" + address + " name=" + element.getName(), "OneWire"+address);
						zone = zoneList.Count > 0 ? zoneList[0] : null;
						sensorName = zone.GetTemperatureDeviceName(address);
						dev = SensorDevice.UpdateGetDevice(element.getName(), address, familyString, zone, SensorDevice.DeviceTypeEnum.OneWire, sensorName);
						dev.StartProcessing();
						if (!ProcessElement(zoneList, element, dev)) {
							errCount++;
						}
						dev.EndProcessing();
						elementCount++;
					}
					Performance.Create("OneWire lookup family=" + familyName + " for elements count=" + elementCount + " took "
					                   + DateTime.Now.Subtract(start).TotalSeconds + " seconds and had errcount=" + errCount,
						verboseLog, familyName,
						Performance.PerformanceFlags.IsError, errCount,
						Performance.PerformanceFlags.Speed, DateTime.Now.Subtract(start).TotalMilliseconds);
					adapter.endExclusive();
				}
				catch (Exception ex) {
					MLog.Log(this, "Error process family=" + family + " err=" + ex.Message);
				}
			}
		}

		public void ReadLoop() {
			List<Thread> threadList = new List<Thread>();
			String adapterId;
			while (MZPState.Instance != null) {
				foreach (DSPortAdapter adapter in m_adapterList) {
					adapterId = (adapter.getAdapterVersion() + adapter.getPortName());
					if (threadList.Find(x => x.Name == adapterId) == null) {
						MLog.Log(this, "Adding new 1wire thread for adapter " + adapterId + " total threads="+threadList.Count);
						Thread th= new Thread(() => this.ReadAdapterInLoop(adapter));
						th.Name = adapterId;
						th.Start();
						threadList.Add(th);
					}
				}
				if (m_adapterList == null || m_adapterList.Count==0 || DateTime.Now.Subtract(m_lastOKRead).TotalMinutes > 10) {
					Alert.CreateAlert("Reinitialising OneWire as no components were found during last 10 minutes", false);
					foreach (Thread th in threadList) {
						th.Abort();
					}
					threadList.Clear();
					Reinitialise();
				}
				Thread.Sleep(1000);
			}
			MLog.Log(this, "OneWire ReadLoop exit");
		}

		/// <summary>
		/// Reads one wire components. DS2423 which is time critical is read mixed with others
		/// </summary>
		private void ReadAdapterInLoop(DSPortAdapter adapter) {
			int i = 0;
			
			while (MZPState.Instance != null) {
				try {
					if (i > Convert.ToInt16(IniFile.PARAM_ONEWIRE_SLOW_READ_DELAY[1])) {
						//slow tick
						i = 0;
						ProcessFamily(SensorDevice.ONEWIRE_TEMPDEV_NAME, adapter, false, false, 0x28); //DS18B20 = temp
						ProcessFamily(SensorDevice.ONEWIRE_SMARTBATDEV_NAME, adapter, false, false, 0x26); //DS2438 = Smart Battery Monitor
					}
					ProcessFamily(SensorDevice.ONEWIRE_IO_NAME, adapter, false, true, 0x12);//DS2406/DS2407
					ProcessFamily(SensorDevice.ONEWIRE_COUNTER_NAME, adapter, false, true, 0x1D); //DS2423
					ProcessFamily(SensorDevice.ONEWIRE_IO8_NAME, adapter, false, false, 0x29); //DS2408
					ProcessFamily(SensorDevice.ONEWIRE_IO2_NAME, adapter, false, false, 0x3A); //DS2413
					i++;
					Thread.Sleep(Convert.ToInt16(IniFile.PARAM_ONEWIRE_FAST_READ_DELAY[1]));
					
				}
				catch (Exception ex) {
					MLog.Log(ex, this, "Error on 1wire loop adapter " + adapter.getAddressAsString());
				}
			}
			MLog.Log(this, "OneWire ReadAdapterInLoop exit adapter " + adapter.getAddressAsString());
		}

		private Boolean ProcessElement(List<ZoneDetails> zoneList, OneWireContainer element, SensorDevice dev) {
			sbyte[] state, initialState;
			Boolean result = true;
			TemperatureContainer temp;
			double tempVal;
			String deviceId = element.getAddressAsString();
			m_lastOKRead = DateTime.Now;
			if (zoneList != null) {
				String zoneName = zoneList.Count > 0 ? zoneList[0].ZoneName + ", totalzonecount=" + zoneList.Count : "no zone exist";
				
				try {
					switch (element.getName()) {
						case SensorDevice.ONEWIRE_TEMPDEV_NAME:
							temp = (TemperatureContainer) element;
							state = temp.readDevice();
							temp.doTemperatureConvert(state);
							tempVal = temp.getTemperature(state);
							if (tempVal != Constants.TEMP_DEFAULT) {
								if (dev.TemperatureResolutionIndex != -1)
									dev.Temperature = tempVal;
								else
									dev.Temperature = Math.Round(tempVal, 2);
							}
							foreach (ZoneDetails zone in zoneList) {
								zone.HasOneWireTemperatureSensor = true;
								SetResolution(element, zone, dev, tempVal);
								if (tempVal != Constants.TEMP_DEFAULT) {
									if (zone.TemperatureResolutionDigits >= 0)
										zone.SetTemperature(Math.Round(tempVal, zone.TemperatureResolutionDigits), deviceId);
									else
										zone.SetTemperature(tempVal, deviceId);
								}
								else {
									MLog.Log(this, "Reading DEFAULT temp in zone " + zone.ZoneName);
								}
							}
							m_deviceAttributes[element.getAddressAsString() + "Temp"] = tempVal.ToString();
							break;
						case SensorDevice.ONEWIRE_IO8_NAME:
							OneWireContainer29 io = (OneWireContainer29)element;
							state = io.readDevice();
							initialState = (sbyte[])state.Clone();
							sbyte[] register = io.readRegister();
							int channels8 = io.getNumberChannels(state);
							bool activity8, level8, latch8, isOn8, lastlevel8, activityWasDetected8=false;
							bool vcc = io.getVCC(register);
							bool isHigh8 = io.isHighSideSwitch();
							dev.IOHigh = isHigh8;
							dev.IONumberChannels = 8;
							dev.IOHasVCC = vcc;
							dev.HasActivitySensing = io.hasActivitySensing();
							for (int i = 0; i < channels8; i++) {
								activity8 = io.getSensedActivity(i, state);
								level8 = io.getLevel(i, state);//TODO: check level
								latch8 = io.getLatchState(i, state);
								dev.Activity[i] = activity8;
								dev.Level[i] = level8;
								dev.State[i] = latch8;
								isOn8 = latch8;

								if (m_deviceAttributes.Keys.Contains(element.getAddressAsString() + "Level" + i)) {
									lastlevel8 = Convert.ToBoolean(m_deviceAttributes[element.getAddressAsString() + "Level" + i]);
								}
								else {
									lastlevel8 = !level8;//force an initial state update
								}
								m_deviceAttributes[element.getAddressAsString() + "Level" + i] = level8.ToString();
								if (activity8 || level8) {
									activityWasDetected8 = true;
									
								}

								foreach (ZoneDetails zone in zoneList) {
									zone.OneWireIOPortCount = channels8;
									zone.HasOneWireIODevice = true;
									zone.SetClosureStateIO(i, latch8);
									if (zone.IsClosureCmdIOOn(i)) {
										if (!isOn8) {
											MLog.Log(this, "OPENING IO closure that is off, id=" + i + " in zone=" + zone.ZoneName);
											io.setLatchState(i, true, false, state);
										}
									}
									else {
										if (isOn8) {
											MLog.Log(this, "CLOSING IO closure that is on, id=" + i + " in zone=" + zone.ZoneName);
											io.setLatchState(i, false, false, state);
										}
									}
								}
								
								if (lastlevel8 != level8 || activity8) {
									foreach (ZoneDetails zone in zoneList) {
										MLog.Log(this, "Event closure8 change on id=" + i + " zone=" + zone.ZoneName
													   + " count=" + zone.ClosureCount + " level=" + level8 + " lastlevel=" + lastlevel8 + " activity=" + activity8);
										//if (!activityB)
										//	Alert.CreateAlert("No Activity A on level change");
										ValueList val = new ValueList(GlobalParams.zoneid, zone.ZoneId.ToString(), CommandSources.rawinput);
										//val.Add(GlobalParams.cmdsource, CommandSources.rawinput.ToString());
										val.Add(GlobalParams.command, GlobalCommands.closure.ToString());
										val.Add(GlobalParams.id, i.ToString());
										val.Add(GlobalParams.iscontactmade, (level8).ToString());
										API.DoCommand(val);
									}
								}	

							}
							
							if (!state.ArraysEqual(initialState)) {
								io.writeDevice(state);
							}
							if (activityWasDetected8) {
								io.clearActivity();
								io.readDevice();
							}
							break;
						case SensorDevice.ONEWIRE_IO_NAME:
							// state is False for contact set to off, True for contact set to on
							// level is True for no contact, False for contact
							// activity set to true when contact state and level has changed

							SwitchContainer swd = (SwitchContainer) element;
							
							/*MemoryBank mem;
							string s="";
							int imem = 0;
							sbyte[] buf;
							java.util.Enumeration mems = element.getMemoryBanks();
							while (mems.hasMoreElements()) {
								mem = (MemoryBank)mems.nextElement();
								s += mem.getBankDescription() + imem;
								if (mem.isReadWrite()){
									buf = new sbyte[mem.getSize()];
									mem.read(mem.getStartPhysicalAddress(), true, buf, 0, buf.Length);
									buf[0] = (sbyte)'d';
									buf[1] = (sbyte)'a';
									mem.write(mem.getStartPhysicalAddress(), buf, 0, buf.Length);
								}
								s += "\n";
								imem++;
							}
							*/
							state = swd.readDevice();
							initialState = (sbyte[])state.Clone();
							int channels2 = swd.getNumberChannels(state);
							bool activity2, level2, state2, isHigh2, isOn2, lastlevel2, activityWasDetected2=false;
							dev.IONumberChannels = channels2;
							isHigh2 = swd.isHighSideSwitch();
							dev.IOHigh = isHigh2;
							dev.HasActivitySensing = swd.hasActivitySensing();
							for (int i = 0; i < channels2; i++) {
								activity2 = swd.getSensedActivity(i, state);
								level2 = swd.getLevel(i, state);
								level2 = !level2;//TODO: why level is diff than 8 IO device?
								state2 = swd.getLatchState(i, state);
								dev.Activity[i] = activity2;
								dev.Level[i] = level2;
								dev.State[i] = state2;
								isOn2 = state2;
								
								if (m_deviceAttributes.Keys.Contains(element.getAddressAsString() + "Level" + i)) {
									lastlevel2 = Convert.ToBoolean(m_deviceAttributes[element.getAddressAsString() + "Level" + i]);
								}
								else {
									lastlevel2 = !level2;//force an initial state update
								}
								m_deviceAttributes[element.getAddressAsString() + "Level" + i] = level2.ToString();
								if (activity2 || level2) {
									activityWasDetected2 = true;
								}

								foreach (ZoneDetails zone in zoneList){
									zone.OneWireIOPortCount = channels2;
									zone.HasOneWireIODevice = true;
									zone.SetClosureStateIO(i, state2);
									if (zone.IsClosureCmdIOOn(i)){
										if (!isOn2) {
											MLog.Log(this, "OPENING IO closure that is off, id="+i+" in zone="+ zone.ZoneName);
											swd.setLatchState(i, true, false, state);
										}
									}
									else {
										if (isOn2){//only detects state when state is True
											//MLog.Log(this, "CLOSING IO closure that is on, id="+i+" in zone="+ zone.ZoneName);
											//swd.setLatchState(i, false, false, state);
										}
									}
								}

								if (lastlevel2 != level2 || activity2) {
									foreach (ZoneDetails zone in zoneList) {
										zone.HasOneWireIODevice = true;
										MLog.Log(this, "Event closure2 change on id="+i+" zone=" + zone.ZoneName
													   + " count=" + zone.ClosureCount + " level=" + level2 + " lastlevel=" + lastlevel2 + " activity=" + activity2);
										//if (!activityB)
										//	Alert.CreateAlert("No Activity A on level change");
										ValueList val = new ValueList(GlobalParams.zoneid, zone.ZoneId.ToString(), CommandSources.rawinput);
										//val.Add(GlobalParams.cmdsource, CommandSources.rawinput.ToString());
										val.Add(GlobalParams.command, GlobalCommands.closure.ToString());
										val.Add(GlobalParams.id, i.ToString());
										val.Add(GlobalParams.iscontactmade, (level2).ToString());
										API.DoCommand(val);
									}
								}
							}
							if (!state.ArraysEqual(initialState)) {
								swd.writeDevice(state);
								//swd.readDevice();
							}
							if (activityWasDetected2) {
								swd.clearActivity();
								swd.readDevice();
							}
							
							break;
						case SensorDevice.ONEWIRE_IO2_NAME:
							SwitchContainer io2 = (OneWireContainer3A)element;
							state = io2.readDevice();
							initialState = (sbyte[])state.Clone();
							int channels2a = io2.getNumberChannels(state);
							bool activity2a, level2a, latch2a, isHigh2a, isOn2a, lastlevel2a, activityWasDetected2a=false;
							dev.IONumberChannels = channels2a;
							isHigh2a = io2.isHighSideSwitch();
							dev.IOHigh = isHigh2a;
							dev.HasActivitySensing = io2.hasActivitySensing();
							for (int i = 0; i < channels2a; i++) {
								activity2a = io2.getSensedActivity(i, state);
								level2a = io2.getLevel(i, state);
								level2a = !level2a;//TODO: why level is diff than 8 IO device?
								latch2a = io2.getLatchState(i, state);
								dev.Activity[i] = activity2a;
								dev.Level[i] = level2a;
								dev.State[i] = latch2a;
								isOn2a = latch2a;
								
								if (m_deviceAttributes.Keys.Contains(element.getAddressAsString() + "Level" + i)) {
									lastlevel2a = Convert.ToBoolean(m_deviceAttributes[element.getAddressAsString() + "Level" + i]);
								}
								else {
									lastlevel2a = !level2a;//force an initial state update
								}
								m_deviceAttributes[element.getAddressAsString() + "Level" + i] = level2a.ToString();
								if (activity2a || level2a) {
									activityWasDetected2a = true;
								}

								foreach (ZoneDetails zone in zoneList){
									zone.OneWireIOPortCount = channels2a;
									zone.HasOneWireIODevice = true;
									zone.SetClosureStateIO(i, latch2a);
									if (zone.IsClosureCmdIOOn(i)){
										if (!isOn2a) {
											MLog.Log(this, "OPENING IO closure that is off, id="+i+" in zone="+ zone.ZoneName);
											io2.setLatchState(i, true, false, state);
										}
									}
									else {
										if (isOn2a){
											MLog.Log(this, "CLOSING IO closure that is on, id="+i+" in zone="+ zone.ZoneName);
											io2.setLatchState(i, false, false, state);
										}
									}
								}

								if (lastlevel2a != level2a || activity2a) {
									foreach (ZoneDetails zone in zoneList) {
										zone.HasOneWireIODevice = true;
										MLog.Log(this, "Event closure2a change on id="+i+" zone=" + zone.ZoneName
													   + " count=" + zone.ClosureCount + " level=" + level2a + " lastlevel=" + lastlevel2a + " activity=" + activity2a);
										//if (!activityB)
										//	Alert.CreateAlert("No Activity A on level change");
										ValueList val = new ValueList(GlobalParams.zoneid, zone.ZoneId.ToString(), CommandSources.rawinput);
										//val.Add(GlobalParams.cmdsource, CommandSources.rawinput.ToString());
										val.Add(GlobalParams.command, GlobalCommands.closure.ToString());
										val.Add(GlobalParams.id, i.ToString());
										val.Add(GlobalParams.iscontactmade, (level2a).ToString());
										API.DoCommand(val);
									}
								}
							}
							if (!state.ArraysEqual(initialState)) {
								io2.writeDevice(state);
								//swd.readDevice();
							}
							if (activityWasDetected2a) {
								io2.clearActivity();
								io2.readDevice();
							}
							break;
						case SensorDevice.ONEWIRE_SMARTBATDEV_NAME:
							ADContainer adc = (ADContainer) element;
							int maxNumChan = adc.getNumberADChannels();
							// array to determine whether a specific channel has been selected
							Boolean[] channel = new Boolean[maxNumChan];
							state = adc.readDevice();
							for (int i = 0; i < maxNumChan; i++) {
								// clear all channel selection
								channel[i] = true;

								//if (adc.hasADAlarms()) {
								//	MLog.Log(this, "Has ALARM capability");
									/*/ disable alarms
									adc.setADAlarmEnable(i, ADContainer.ALARM_LOW, false,
															state);
									adc.setADAlarmEnable(i, ADContainer.ALARM_HIGH,
															false, state);
									 */
								//}
							}
							double[] voltage;
							getVoltage(adc, channel, out voltage);
							
							for (int i = 0; i < voltage.Length; i++) {
								foreach (ZoneDetails zone in zoneList) {
									zone.HasOneWireVoltageSensor = true;
									zone.SetVoltage(i, voltage[i]);
								}
								dev.Voltage[i] = Math.Round(voltage[i], 4);//more than 4 is not relevant for display
							}

							//reading temp
							temp = (TemperatureContainer) element;
							state = temp.readDevice();
							temp.doTemperatureConvert(state);
							tempVal = temp.getTemperature(state);
							if (tempVal != Constants.TEMP_DEFAULT) {
								dev.Temperature = Math.Round(tempVal, 2);//too many digits, not needed
							}
							foreach (ZoneDetails zone in zoneList) {
								zone.HasOneWireTemperatureSensor = true;
								SetResolution(element, zone, dev, tempVal);
								if (tempVal != Constants.TEMP_DEFAULT) {
									if (zone.TemperatureResolutionDigits >= 0)
										zone.SetTemperature(Math.Round(tempVal, zone.TemperatureResolutionDigits), deviceId);
									else
										zone.SetTemperature(tempVal, deviceId);
								}
								else {
									MLog.Log(this, "Reading DEFAULT temp via ds2438 in zone " + zone.ZoneName);
								}
							}
							m_deviceAttributes[element.getAddressAsString() + "Temp"] = tempVal.ToString();
							break;
						case SensorDevice.ONEWIRE_COUNTER_NAME:
							OneWireContainer1D counter = (OneWireContainer1D) element;
							long c1, c2;
							c1 = counter.readCounter(14);
							c2= counter.readCounter(15);
							dev.Counter[0] = c1;
							dev.Counter[1] = c2;
							foreach (ZoneDetails zone in zoneList) {
								zone.HasOneWireCounterDevice = true;

								ValueList val = new ValueList(GlobalParams.zoneid, zone.ZoneId.ToString(), CommandSources.events);
								val.Add(GlobalParams.command, GlobalCommands.counter.ToString());
								val.Add(GlobalParams.id, "1");
								val.Add(GlobalParams.count, c1.ToString()); //normal close
								API.DoCommand(val);

								val = new ValueList(GlobalParams.zoneid, zone.ZoneId.ToString(), CommandSources.events);
								val.Add(GlobalParams.command, GlobalCommands.counter.ToString());
								val.Add(GlobalParams.id, "2");
								val.Add(GlobalParams.count, c2.ToString()); //normal close
								API.DoCommand(val);
							}
							break;
						default:
							MLog.Log(this, "Unknown onewire device " + element.getName());
							break;
					}
					foreach (ZoneDetails zone in zoneList) {
						zone.AddSensor(dev);
					}
					dev.RecordSuccess();
				}
				catch (Exception ex) {
					String err = "Err reading OneWire zone=" + zoneName+ " err=" + ex.Message;
					Performance.Create(err, false, zoneName, Performance.PerformanceFlags.IsError, 1);
					//MLog.Log(this, err);
					dev.RecordError(ex);
					result = false;
				}
			}
			else {
				if (element.getName() != SensorDevice.ONEWIRE_CONTROLLER_NAME) {
					String err = "UNNALOCATED OneWire device addr=" + element.getAddressAsString() + " name=" + element.getName() +
					             " desc=" + element.getDescription();
					Performance.Create(err, true, "");
					MLog.Log(this, err);
				}
			}
			return result;
		}

		// read A/D from device
		private static void getVoltage(ADContainer adc, Boolean[] channel, out double[] curVoltage) {
			sbyte[] state;
			curVoltage = new double[channel.Length];
			state = adc.readDevice();
			if (adc.canADMultiChannelRead()) {
				// do all channels together
				adc.doADConvert(channel, state);
				curVoltage = adc.getADVoltage(state);
			}
			else {
				// do one channel at a time;
				for (int i = 0; i < channel.Length; i++) {
					if (channel[i]) {
						adc.doADConvert(i, state);
						curVoltage[i] = adc.getADVoltage(i, state);
					}
				}
			}

			for (int i = 0; i < channel.Length; i++) {
				//if (channel [i])   // show value up to 2 decimal places
				//MLog.Log(null, " Channel " + i + " = "
				//				+ (( int ) (curVoltage [i] * 10000)) / 10000.0
				//				+ "V");
			}
		}

		public void Close() {
/*
			// end the 1-Wire network SensorDevice Monitor
			if (dMonitor.isMonitorRunning())
			{
				//SetText("Killing deviceMonitor" + Environment.NewLine);
				dMonitor.killMonitor();
			}
			// cleanup after ourselves by destroying the thread
			if (m_searchThread.IsAlive)
			{
				m_searchThread.Abort();
			}
		  */
		}

		public bool IsConnected() {
			return false;
		}

		public void MakeBuzz() {
			//throw new NotImplementedException();
		}

		public bool IsFaulty() {
			return (m_adapterList.Count == 0);
		}

		public bool IsFunctional() {
			return !IsFaulty();
		}

		public bool IsEnabled() {
			throw new NotImplementedException();
		}

		public void Enable() {
			throw new NotImplementedException();
		}

		public void Disable() {
			throw new NotImplementedException();
		}

		public string Type() {
			return IniFile.DEVICE_TYPE_IO;
		}

		public string Name() {
			if (IsFunctional()) {
				String name="";
				foreach (DSPortAdapter adapter in m_adapterList)
					name += adapter.getAdapterName() + " ";
				return name;
			}
			else {
				return "OneWire";
			}
		}
	}

	public abstract class GenericUPS {
		public class UPSState {
			public Boolean LastPowerFailDateTime;
			public string IPVoltage, IPFaultVoltage, OPVoltage, OPCurrent, IPFrequency, BatteryVoltage, Temperature, UPSStatus;
			public Boolean PowerFail, BatteryLow, AVR, UPSFailed, StandbyUPS, TestInProgress, ShutdownActive, BeeperOn;
		}

		protected UPSState m_lastStatus;
		public abstract void Initialise();
		public abstract void GetStatus();
	}

	public class APCUPS : GenericUPS, IMZPDevice {
		private WinEventLogReader m_winEventLogReader;
		private String m_eventLog, m_eventSource;

		public APCUPS(String eventLog, String eventSource) {
			m_eventLog = eventLog;
			m_eventSource = eventSource;
			Initialise();
		}

		public override void Initialise() {
			m_winEventLogReader = new WinEventLogReader(m_eventLog);
			m_winEventLogReader.AddSource(m_eventSource);
		}

		public override void GetStatus() {
		}

		public bool IsFunctional() {
			return m_winEventLogReader != null && DateTime.Now.Subtract(m_winEventLogReader.LastLogEntryDate).TotalDays <= 1;
		}

		public bool IsEnabled() {
			throw new NotImplementedException();
		}

		public void Enable() {
			throw new NotImplementedException();
		}

		public void Disable() {
			throw new NotImplementedException();
		}

		public string Type() {
			return IniFile.DEVICE_TYPE_UPS;
		}

		public string Name() {
			return m_eventSource;
		}
	}

	public class MustekUPS : GenericUPS, IMZPDevice {
		private string m_statusurl, m_lastHtml = "";
		private WebClient m_client;

		public MustekUPS(String statusurl) {
			m_statusurl = statusurl;
			Initialise();
		}

		public override void Initialise() {
			m_lastStatus = new UPSState();
			m_client = new WebClient();
			GetStatus();
		}

		public override void GetStatus() {
			try {
				m_lastHtml = m_client.DownloadString(m_statusurl);
				string[] atoms, pairs;
				Boolean failure = m_lastStatus.PowerFail;

				atoms = m_lastHtml.Split(new string[] {"</br>\r\n"}, StringSplitOptions.RemoveEmptyEntries);
				foreach (string atom in atoms) {
					pairs = atom.Replace(" ", "").Split('=');
					if (pairs.Length > 1) {
						switch (pairs[0]) {
							case "I/PVoltage":
								m_lastStatus.IPVoltage = pairs[1];
								break;
							case "I/PFaultVoltage":
								m_lastStatus.IPFaultVoltage = pairs[1];
								break;
							case "O/PVoltage":
								m_lastStatus.OPVoltage = pairs[1];
								break;
							case "O/PCurrent":
								m_lastStatus.OPCurrent = pairs[1];
								break;
							case "I/PFrequency":
								m_lastStatus.IPFrequency = pairs[1];
								break;
							case "BatteryVoltage":
								m_lastStatus.BatteryVoltage = pairs[1];
								break;
							case "m_temperature ":
								m_lastStatus.Temperature = pairs[1];
								break;
							case "UPSStatus":
								m_lastStatus.UPSStatus = pairs[1];

								if (m_lastStatus.UPSStatus.Length >= 8) {
									m_lastStatus.PowerFail = m_lastStatus.UPSStatus[0] == '1';
									m_lastStatus.BatteryLow = m_lastStatus.UPSStatus[1] == '1';
									m_lastStatus.AVR = m_lastStatus.UPSStatus[2] == '1';
									m_lastStatus.UPSFailed = m_lastStatus.UPSStatus[3] == '1';
									m_lastStatus.StandbyUPS = m_lastStatus.UPSStatus[4] == '1';
									m_lastStatus.TestInProgress = m_lastStatus.UPSStatus[5] == '1';
									m_lastStatus.ShutdownActive = m_lastStatus.UPSStatus[6] == '1';
									m_lastStatus.BeeperOn = m_lastStatus.UPSStatus[7] == '1';

									failure = m_lastStatus.PowerFail;
								}
								break;
						}
					}
				}

				if (failure != MZPState.Instance.IsPowerFailure) {
					MLog.Log(this, "MUSTEK power event failure=" + m_lastStatus.PowerFail);
					ValueList val = new ValueList(GlobalParams.command, GlobalCommands.powerevent.ToString(), CommandSources.system);
					val.Add(GlobalParams.action, failure.ToString());
					val.Add(GlobalParams.datetime, DateTime.Now.ToString());
					CommandResult retcmd = API.DoCommandFromWeb(val); //, out retval);
					//Metadata.CommandResult retcmd = fastJSON.JSON.Instance.ToObject(json) as Metadata.CommandResult;
				}
			}
			catch (Exception ex) {
				MLog.Log(this, "Unable to read UPS status" + ex.Message);
			}
		}

		public bool IsFunctional() {
			return m_lastHtml != "";
		}

		public bool IsEnabled() {
			throw new NotImplementedException();
		}

		public void Enable() {
			throw new NotImplementedException();
		}

		public void Disable() {
			throw new NotImplementedException();
		}

		public string Type() {
			return IniFile.DEVICE_TYPE_UPS;
		}

		public string Name() {
			return "Mustek";
		}
	}

	public class Monitor {
		[StructLayout(LayoutKind.Sequential)]
		public struct DISPLAY_DEVICE {
			public int cb;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string DeviceName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceString;
			public int StateFlags;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceID;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceKey;

			public DISPLAY_DEVICE(int flags) {
				cb = 0;
				StateFlags = flags;
				DeviceName = new string((char) 32, 32);
				DeviceString = new string((char) 32, 128);
				DeviceID = new string((char) 32, 128);
				DeviceKey = new string((char) 32, 128);
				cb = Marshal.SizeOf(this);
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct DEVMODE {
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string dmDeviceName;
			public short dmSpecVersion;
			public short dmDriverVersion;
			public short dmSize;
			public short dmDriverExtra;
			public int dmFields;
			public short dmOrientation;
			public short dmPaperSize;
			public short dmPaperLength;
			public short dmPaperWidth;
			public short dmScale;
			public short dmCopies;
			public short dmDefaultSource;
			public short dmPrintQuality;
			public short dmColor;
			public short dmDuplex;
			public short dmYResolution;
			public short dmTTOption;
			public short dmCollate;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string dmFormName;
			public short dmUnusedPadding;
			public short dmBitsPerPel;
			public int dmPelsWidth;
			public int dmPelsHeight;
			public int dmDisplayFlags;
			public int dmDisplayFrequency;
		}

		private static List<String> listDevices = new List<string>();
		private static List<String> listSettings = new List<string>();
		private static int listSettingsIndex, listDevicesIndex=0;

		public static void RefreshFrequencySecondary() {
			MLog.Log(null, "Refreshing screen");
			EnumDevices();
			for (int i = 0; i < System.Windows.Forms.Screen.AllScreens.Length; i++) {
				//if (!MainDevice(i))
				{
					DEVMODE current = EnumModes(i);
					if (current.dmPelsHeight == 1080) {
						if (current.dmDisplayFrequency == 59) {
							current.dmDisplayFrequency = 60;
						}
						else {
							current.dmDisplayFrequency = 59;
						}
					}
					ChangeDisplaySettings(ref current, 0);
					MLog.Log(null, "Screen refreshed, device=" + current.dmDeviceName
					               + " freq=" + current.dmDisplayFrequency
					               + " width=" + current.dmPelsWidth + " height=" + current.dmPelsHeight);
				}
			}
		}

		private void listDevices_SelectedIndexChanged(object sender, EventArgs e) {
			int devNum = listDevicesIndex;
			bool isMain = MainDevice(devNum);
			//btnSet.Enabled = isMain; // enable only for the main device
			EnumModes(devNum);
		}

		private void btnSet_Click(object sender, EventArgs e) {
			//set selected display mode
			int devNum = listDevicesIndex;
			int modeNum = listSettingsIndex;
			DEVMODE d = GetDevmode(devNum, modeNum);
			if (d.dmBitsPerPel != 0 & d.dmPelsWidth != 0
			    & d.dmPelsHeight != 0) {
				ChangeDisplaySettings(ref d, 0);
			}
		}

		private static DEVMODE EnumModes(int devNum) {
			listSettings.Clear();

			string devName = GetDeviceName(devNum);
			DEVMODE devMode = new DEVMODE();
			int modeNum = 0;
			bool result = true;
			do {
				result = EnumDisplaySettings(devName,
					modeNum, ref devMode);

				if (result) {
					string item = DevmodeToString(devMode);
					listSettings.Add(item);
				}
				modeNum++;
			} while (result);

			if (listSettings.Count > 0) {
				DEVMODE current = GetDevmode(devNum, -1);
				int selected = listSettings.IndexOf(DevmodeToString(current));
				if (selected >= 0) {
					listSettingsIndex = selected;
				}
				return current;
			}
			else {
				return new DEVMODE();
			}
		}

		private static DEVMODE GetDevmode(int devNum, int modeNum) {
			//populates DEVMODE for the specified device and mode
			DEVMODE devMode = new DEVMODE();
			string devName = GetDeviceName(devNum);
			EnumDisplaySettings(devName, modeNum, ref devMode);
			return devMode;
		}

		private static string DevmodeToString(DEVMODE devMode) {
			return devMode.dmPelsWidth.ToString() +
			       " x " + devMode.dmPelsHeight.ToString() +
			       ", " + devMode.dmBitsPerPel.ToString() +
			       " bits, " +
			       devMode.dmDisplayFrequency.ToString() + " Hz";
		}

		private static void EnumDevices() {
			//populates Display Devices list
			listDevices.Clear();
			DISPLAY_DEVICE d = new DISPLAY_DEVICE(0);

			int devNum = 0;
			bool result;
			do {
				result = EnumDisplayDevices(IntPtr.Zero,
					devNum, ref d, 0);

				if (result) {
					string item = devNum.ToString() +
					              ". " + d.DeviceString.Trim();
					if ((d.StateFlags & 4) != 0) {
						item += " - main";
					}
					listDevices.Add(item);
				}
				devNum++;
			} while (result);
		}

		private static string GetDeviceName(int devNum) {
			DISPLAY_DEVICE d = new DISPLAY_DEVICE(0);
			bool result = EnumDisplayDevices(IntPtr.Zero,
				devNum, ref d, 0);
			return (result ? d.DeviceName.Trim() : "#error#");
		}

		private static bool MainDevice(int devNum) {
			//whether the specified device is the main device
			DISPLAY_DEVICE d = new DISPLAY_DEVICE(0);
			if (EnumDisplayDevices(IntPtr.Zero, devNum, ref d, 0)) {
				return ((d.StateFlags & 4) != 0);
			}
			return false;
		}

		[DllImport("User32.dll")]
		private static extern bool EnumDisplayDevices(
			IntPtr lpDevice, int iDevNum,
			ref DISPLAY_DEVICE lpDisplayDevice, int dwFlags);

		[DllImport("User32.dll")]
		private static extern bool EnumDisplaySettings(
			string devName, int modeNum, ref DEVMODE devMode);

		[DllImport("user32.dll")]
		public static extern int ChangeDisplaySettings(
			ref DEVMODE devMode, int flags);
	}

	public class LPT {
		[DllImport("inpout32.dll")]
		private static extern UInt32 IsInpOutDriverOpen();

		[DllImport("inpout32.dll")]
		private static extern void Out32(short PortAddress, short Data);

		[DllImport("inpout32.dll")]
		private static extern char Inp32(short PortAddress);

		[DllImport("inpout32.dll")]
		private static extern void DlPortWritePortUshort(short PortAddress, ushort Data);

		[DllImport("inpout32.dll")]
		private static extern ushort DlPortReadPortUshort(short PortAddress);

		[DllImport("inpout32.dll")]
		private static extern void DlPortWritePortUlong(int PortAddress, uint Data);

		[DllImport("inpout32.dll")]
		private static extern uint DlPortReadPortUlong(int PortAddress);

		[DllImport("inpoutx64.dll")]
		private static extern bool GetPhysLong(ref int PortAddress, ref uint Data);

		[DllImport("inpoutx64.dll")]
		private static extern bool SetPhysLong(ref int PortAddress, ref uint Data);

		[DllImport("inpoutx64.dll", EntryPoint = "IsInpOutDriverOpen")]
		private static extern UInt32 IsInpOutDriverOpen_x64();

		[DllImport("inpoutx64.dll", EntryPoint = "Out32")]
		private static extern void Out32_x64(short PortAddress, short Data);

		[DllImport("inpoutx64.dll", EntryPoint = "Inp32")]
		private static extern char Inp32_x64(short PortAddress);

		[DllImport("inpoutx64.dll", EntryPoint = "DlPortWritePortUshort")]
		private static extern void DlPortWritePortUshort_x64(short PortAddress, ushort Data);

		[DllImport("inpoutx64.dll", EntryPoint = "DlPortReadPortUshort")]
		private static extern ushort DlPortReadPortUshort_x64(short PortAddress);

		[DllImport("inpoutx64.dll", EntryPoint = "DlPortWritePortUlong")]
		private static extern void DlPortWritePortUlong_x64(int PortAddress, uint Data);

		[DllImport("inpoutx64.dll", EntryPoint = "DlPortReadPortUlong")]
		private static extern uint DlPortReadPortUlong_x64(int PortAddress);

		[DllImport("inpoutx64.dll", EntryPoint = "GetPhysLong")]
		private static extern bool GetPhysLong_x64(ref int PortAddress, ref uint Data);

		[DllImport("inpoutx64.dll", EntryPoint = "SetPhysLong")]
		private static extern bool SetPhysLong_x64(ref int PortAddress, ref uint Data);

		private bool m_bX64 = false;

		private short m_dataPortValue = 0;
		private bool m_isInit = false;
		private short m_portAddress;
		private Object m_locker = new Object();

		public LPT(short portAddress) {
			m_portAddress = portAddress;
			Initialise();
		}

		public void Initialise() {
			try {
				m_isInit = false;
				uint nResult = 0;
				try {
					nResult = IsInpOutDriverOpen();
					m_isInit = true;
					MLog.Log(this, "LPT init on 32 bit OK");
				}
				catch (BadImageFormatException) {
					MLog.Log(this, "LPT init on 32 bit Failed");
					nResult = IsInpOutDriverOpen_x64();
					if (nResult != 0) {
						m_bX64 = true;
						m_isInit = true;
						MLog.Log(this, "LPT init on 64 bit OK");
					}
					else {
						MLog.Log(this, "LPT init on 64 bit Failed");
					}
				}

				if (nResult == 0) {
					MLog.Log(this, "Unable to open InpOut32 driver");
				}
			}
			catch (DllNotFoundException ex) {
				MLog.Log(ex, this, "Unable to find InpOut32.dll");
			}
		}

		private bool ReadPort() {
			lock (m_locker) {
				try {
					char c;
					if (m_bX64) {
						c = Inp32_x64(m_portAddress);
					}
					else {
						c = Inp32(m_portAddress);
					}

					if (c != m_dataPortValue) {
//todo
					}
					//MLog.Log(this, "ReadLPT port int=" + (short)c+ " char="+c);
					m_dataPortValue = (short) c;
					return true;
				}
				catch (Exception ex) {
					MLog.Log(ex, this, "An error occured on ReadPort");
					return false;
				}
			}
		}

		public void WritePort(short value) {
			lock (m_locker) {
				try {
					if (m_bX64) {
						Out32_x64(m_portAddress, value);
					}
					else {
						Out32(m_portAddress, value);
					}
				}
				catch (Exception ex) {
					MLog.Log(ex, this, "An error occured at write");
				}
			}
		}

		public void WritePort(short pinIndex, bool isOn) {
			string state = PortState;
			if (pinIndex < state.Length) {
				state = state.ReplaceAt(state.Length - 1 - pinIndex, isOn ? '1' : '0');
				short newValue = Convert.ToInt16(state, 2);
				WritePort(newValue);
			}
			else {
				MLog.Log(this, "Pin index=" + pinIndex + " out of range on state=" + state);
			}
		}

		public string PortState {
			get {
				ReadPort();
				return Convert.ToString(m_dataPortValue, 2).PadLeft(8, '0');
			}
		}

		public bool IsPowerOn(short pinIndex) {
			string state = PortState;
			return state[state.Length - 1 - pinIndex] == '1';
		}

		public Boolean IsInitialised() {
			return m_isInit;
		}
	}

	public class Bluetooth : IMZPDevice {
		private static BluetoothClient m_btc = null;

		public class Device {
			public string DeviceName { get; set; }
			public bool Authenticated { get; set; }
			public bool Connected { get; set; }
			public BluetoothAddress Address { get; set; }
			public ushort Nap { get; set; }
			public uint Sap { get; set; }
			public DateTime LastSeen { get; set; }
			public DateTime LastUsed { get; set; }
			public bool Remembered { get; set; }
			public BluetoothDeviceInfo DevInfo { get; set; }

			public Device(BluetoothDeviceInfo device_info) {
				this.Authenticated = device_info.Authenticated;
				this.Connected = device_info.Connected;
				this.DeviceName = device_info.DeviceName;
				this.LastSeen = device_info.LastSeen;
				this.LastUsed = device_info.LastUsed;
				this.Address = device_info.DeviceAddress;
				this.Nap = device_info.DeviceAddress.Nap;
				this.Sap = device_info.DeviceAddress.Sap;
				this.Remembered = device_info.Remembered;
				this.DevInfo = device_info;
			}

			public override string ToString() {
				return DeviceName + " Seen:" + LastSeen + " Used:" + LastUsed + " Addr:" + Address
				       + " Conn:" + Connected + " Rememb:" + Remembered + " Auth:" + Authenticated;
			}

			public override int GetHashCode() {
				return this.Address.GetHashCode();
			}

			public override bool Equals(object obj) {
				if (!(obj is Bluetooth.Device)) {
					throw new ArgumentException("obj is not an Bluetooth.SensorDevice");
				}
				var dev = obj as Bluetooth.Device;
				if (dev == null) {
					return false;
				}
				return this.Address.Equals(dev.Address);
			}
		}

		public Bluetooth() {
			;
		}

		//separate thread as discovery takes long time
		public static void StartDiscovery() {
			int tick = 500;
			while (MZPState.Instance != null) {
				UserPresence.CheckLocalBluetooth();
				for (int i = 0; i < IniFile.ZONE_TICK_SLOW_SLEEP/tick; i++) {
					Thread.Sleep(tick);
					if (MZPState.Instance == null) {
						break;
					}
				}
			}
			MLog.Log(null, "BT StartDiscovery exit");
		}

		public static List<Device> DiscoverDevices() {
			BluetoothClient bc = new BluetoothClient();
			m_btc = bc;
			List<Device> devices = new List<Device>();
			DateTime startDisc;
			Boolean canConnect;
			BluetoothDeviceInfo[] array = bc.DiscoverDevices(15, true, true, true); //bc.DiscoverDevices();
			int count = array.Length;
			startDisc = DateTime.Now;
			for (int i = 0; i < count; i++) {
				Device device = new Device(array[i]);
				if (MZPState.Instance == null) {
					MLog.Log(null, "BT discovery interrupted");
					return devices;
				}

				canConnect = CanConnect(device);
				if (canConnect) {
					//MLog.Log(null, "Active BT device detected " + device.ToString());
					devices.Add(device);
				}
				//MLog.Log(null, "Discovery result="+canConnect+" on " + device.DeviceName + " " + device.Address+ " took " 
				//	+ Utilities.DurationAsTimeSpan(DateTime.Now.Subtract(startDisc)));
			}
			Performance.Create("Bluetooth local discovery", false, "",
				Performance.PerformanceFlags.Speed, DateTime.Now.Subtract(startDisc).TotalMilliseconds);
			return devices;
		}

		public static Boolean CanConnect(Device device) {
			//BluetoothClient bc = new BluetoothClient();
			//BluetoothEndPoint be;
			//Guid service = BluetoothService.SdpProtocol;

			bool inRange;
			Guid fakeUuid = new Guid("{F13F471D-47CB-41d6-9609-BAD0690BF891}"); // A specially created value, so no matches.
			try {
				ServiceRecord[] records = device.DevInfo.GetServiceRecords(fakeUuid);
				//Debug.Assert(records.Length == 0, "Why are we getting any records?? len: " + records.Length);
				inRange = true;
			}
			catch (Exception) {
				inRange = false;
			}
			return inRange;
			/*
			try
			{
				be = new BluetoothEndPoint(device.Address, service);
				bc.Close();
				bc.Connect(be);
				bc.Close();
				return true;
			}
			catch (Exception ex)
			{
				MLog.Log(null, "Could not connect to BT device " + device.ToString() + " ERROR="+ex.Message);
				return false;
			}*/
		}

		public bool IsFunctional() {
			return m_btc != null;
		}

		public bool IsEnabled() {
			throw new NotImplementedException();
		}

		public void Enable() {
			throw new NotImplementedException();
		}

		public void Disable() {
			throw new NotImplementedException();
		}

		public string Type() {
			return IniFile.DEVICE_TYPE_RADIO;
		}

		public string Name() {
			return "Bluetooth";
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;

using com.dalsemi.onewire;
using com.dalsemi.onewire.adapter;
using com.dalsemi.onewire.container;
using com.dalsemi.onewire.application.monitor;

namespace MultiZonePlayer
{
	public class GenericModem : SerialBase, IMessenger
	{
		public enum ModemCommandsEnum
		{
			[Description("ATD ")]
			MODEM_CALL,
			[Description("AT")]
			MODEM_CHECK

		};

		public class ModemCommand
		{
			public String Id;
			public String ATCommand;
			public String Response;
			public ModemCommand(String Id, String ATCommand, String Response)
			{
				this.Id = Id;
				this.ATCommand = ATCommand;
				this.Response = Response;
			}
		}
		private List<ModemCommand> m_commandList;
		protected int m_stdtimeout = 2000;
		protected int m_calltimeout = 60000;
		protected int m_atlinescount, m_atdlinescount;

		~GenericModem()
		{
			Close();
		}

		public void Close()
		{
			Disconnect();
		}

		public virtual void Reinitialise()
		{
		}

		public void Reinitialise(string baud, string parity, string stopBits, string dataBits, string comport, CommunicationManager.TransmissionType type,
			int atlinescount, int atdlinescount)
		{
			Initialise(baud, parity, stopBits, dataBits, comport, type);
			//comm = new CommunicationManager(baud, parity, stopBits, dataBits, comport, this.handler);
			m_atlinescount = atlinescount;
			m_atdlinescount = atdlinescount;
			//comm.OpenPort();
			//m_waitForResponse = false;
			//m_lastOperationWasOK = true;
			m_commandList = new List<ModemCommand>();
		}

		public override string SendCommand(Enum cmd, string value)
		{
			throw new NotImplementedException();
		}
		public override string GetCommandStatus(Enum cmd)
		{
			throw new NotImplementedException();
		}

		protected override void ReceiveSerialResponse(string response)
		{
			MLog.Log(this, "Received unexpected MODEM serial response: " + response);

			String message = response.ToLower().Replace("\r", "").Replace("\n", "").ToLower();

			ReceiveMessage(message, "");
		}

		public void ReceiveMessage(String message, String sender)
		{
			MLog.Log(this, "ReceiveMessage from " + sender + " = " + message);
		}

		public void SendMessageToTarget(String message)
		{
			MLog.Log(this, "ERROR send message not implemented in modem");
		}

		public override bool TestConnection()
		{
			//if (base.TestConnection() == false) return false;

			try
			{
				String res = WriteCommand(Utilities.GetEnumDescription((ModemCommandsEnum)ModemCommandsEnum.MODEM_CHECK), m_atlinescount, m_stdtimeout).ToLower();
				if (res.Contains("ok") || res.Contains("at"))
					return true;
				else
				{
					MLog.Log(this, "Error health check GenericModem res=" + res);
					return true;
				}
			}
			catch (Exception ex)
			{
				MLog.Log(ex, "Exception Test GenericModem");
				return false;
			}
		}

		public Boolean IsTargetAvailable()
		{
			return (comm != null && comm.IsPortOpen());
		}
		public void MakeBuzz()
		{
			MLog.Log(this, "Calling target number " + IniFile.PARAM_MODEM_TARGETNUMBER[1]);
			String res = WriteCommand(Utilities.GetEnumDescription((ModemCommandsEnum)ModemCommandsEnum.MODEM_CALL)
				+ IniFile.PARAM_MODEM_TARGETNUMBER[1] + ";", m_atdlinescount, m_calltimeout);
			MLog.Log(this, "Calling target number done, res=" + res);
			//comm.ClosePort();
			//Reinitialise();
			WriteCommand("ATH", 2, 3000);
			MLog.Log(this, "Call reset");
		}
	}

	class Modem : GenericModem
	{
		public Modem()
		{
			Reinitialise();
		}

		public override void Reinitialise()
		{

			System.Management.ManagementObjectSearcher mos = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_POTSModem");
			foreach (System.Management.ManagementObject mo in mos.Get())
			{
				MLog.Log(this, "Found Modem " + mo["Caption"].ToString() + " at port " + mo["AttachedTo"].ToString());
				if (mo["Caption"].ToString().ToLower().Contains(IniFile.PARAM_MODEM_DEVICE_NAME[1].ToLower()))
				{
					IniFile.PARAM_MODEM_COMPORT[1] = mo["AttachedTo"].ToString();
					MLog.Log(this, "Selected modem port is " + IniFile.PARAM_MODEM_COMPORT[1]);
					break;
				}
			}
			MLog.Log(this, "Reinitialising Modem on com " + IniFile.PARAM_MODEM_COMPORT[1]);
			Reinitialise("9600", "None", "One", "8", IniFile.PARAM_MODEM_COMPORT[1], CommunicationManager.TransmissionType.Text,
				Convert.ToInt16(IniFile.PARAM_MODEM_AT_LINES_COUNT[1]),
				Convert.ToInt16(IniFile.PARAM_MODEM_ATD_LINES_COUNT[1]));
		}
	}

	class SMS : GenericModem
	{
		public enum SMSCommandsEnum
		{
			[Description("AT+CMGF=1")]
			SMS_ENABLE,
			[Description("AT+CMGS=")]
			SMS_SEND,
			[Description("AT+GMM")]
			SMS_DEVICEINFO,
		};

		public SMS()
		{
			Reinitialise();
		}

		public override void Reinitialise()
		{
			MLog.Log(this, "Reinitialising SMS on com " + IniFile.PARAM_SMS_COMPORT[1]);
			Reinitialise("9600", "None", "One", "8", IniFile.PARAM_SMS_COMPORT[1], CommunicationManager.TransmissionType.Text,
				Convert.ToInt16(IniFile.PARAM_SMS_AT_LINES_COUNT[1]),
				Convert.ToInt16(IniFile.PARAM_SMS_ATD_LINES_COUNT[1]));
		}

		public new void SendMessageToTarget(String message)
		{
			WriteCommand(Utilities.GetEnumDescription((SMSCommandsEnum)SMSCommandsEnum.SMS_ENABLE),
				m_atlinescount, m_stdtimeout);
			WriteCommand(Utilities.GetEnumDescription((SMSCommandsEnum)SMSCommandsEnum.SMS_SEND) + "\""
				+ IniFile.PARAM_SMS_TARGETNUMBER[1] + "\"", m_atlinescount, m_stdtimeout);
			WriteCommand(message + (char)26, 6, m_stdtimeout * 3);
		}
	}
	
	class RFXCom : SerialBase, IMessenger
	{
		private static string CMD_RESET = "0D 00 00 00 00 00 00 00 00 00 00 00 00 00";
		private static string CMD_GETSTATUS = "0D 00 00 01 02 00 00 00 00 00 00 00 00 00";
		private static string CMD_SETMODE = "0D 00 00 05 03 53 00 00 00 2E 00 00 00 00";//oregon&arc&ac&h eu	
		//"0D 00 00 06 03 53 00 00 00 26 00 00 00 00";//oregon&arc&ac
		//"0D 00 00 02 03 53 00 00 00 22 00 00 00 00";//oregon & arc


		public RFXCom()
		{
			Reinitialise();
		}
		public void Reinitialise()
		{
			MLog.Log(this, "Initialise RFX on COM " + IniFile.PARAM_RFXCOM_PORT[1]);
			Initialise("38400", "None", "One", "8", IniFile.PARAM_RFXCOM_PORT[1], CommunicationManager.TransmissionType.Text);
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

			//m_waitForResponse = false;
			//m_lastOperationWasOK = true;
		}

		private string GetStatus()
		{
			string status = WriteCommand(CMD_GETSTATUS, 1, 1000);
			MLog.Log(this, "RFX status is " + status);
			return status;
		}

		protected override void ReceiveSerialResponse(string response)
		{
			string origResponse = response;
			//MLog.Log(this, "RFXCOMM: " + response);
			do
			{
				RFXDeviceDefinition.RFXDevice dev = RFXDeviceDefinition.GetDevice(ref response);
				if (dev != null)
				{
					MLog.Log(this, "RFX result for response:[" + origResponse + "] is " + dev.DisplayValues());
					if (dev.ZoneId != -1)
					{
						Metadata.ZoneDetails zone = MZPState.Instance.GetZoneById(dev.ZoneId);
						switch (dev.DeviceType)
						{
							case RFXDeviceDefinition.DeviceTypeEnum.temp_hum:
								decimal temp, hum, lasttemp, lasthum;
								temp = Convert.ToDecimal(dev.FieldValues.Find(x => x.Name == RFXDeviceDefinition.DeviceAttributes.temperature.ToString()).Value) / 10;
								hum = Convert.ToDecimal(dev.FieldValues.Find(x => x.Name == RFXDeviceDefinition.DeviceAttributes.humidity.ToString()).Value);
								lasttemp = Convert.ToDecimal(zone.Temperature);
								lasthum = Convert.ToDecimal(zone.Humidity);

								lasttemp = lasttemp == 0 ? 0.1m : lasttemp;
								lasthum = lasthum == 0 ? 0.1m : lasthum;
								zone.Temperature = temp.ToString();
								zone.Humidity = hum.ToString();
								break;
							case RFXDeviceDefinition.DeviceTypeEnum.lighting1:

								break;
							default:
								MLog.Log(this, "Unknown RFX device type in response " + response);
								break;
						}
					}
					else
						MLog.Log(this, "Unknown zone for RFX response " + response);
				}
				else MLog.Log(this, "No RFX device identified for response " + response);
			} while (response.Length > 0);


			//String message = response.ToLower().Replace("\r", "").Replace("\n", "").ToLower();
		}
		public override string GetCommandStatus(Enum cmd)
		{
			throw new NotImplementedException();
		}

		public override string SendCommand(Enum cmd, string value)
		{
			throw new NotImplementedException();
		}

		public string SendCommand(string value)
		{
			return WriteCommand(value, 1, 1000);
		}

		public void Close()
		{
			Disconnect();
		}

		public Boolean IsTargetAvailable()
		{
			return true;
		}

		public void MakeBuzz()
		{
		}
		public void ReceiveMessage(String message, String sender)
		{
			//MLog.Log(this, "SMS from " +sender + " = " + message);
		}

		public void SendMessageToTarget(String message)
		{
			MLog.Log(this, "RFX comm does not implement sendmessage");
		}

		public override bool TestConnection()
		{
			if (GetStatus() != "")
				return true;
			else
				return false;
		}
	}

	public class WDIO : GenericModem, IMessenger
	{
		private String m_lastState, m_channel;
		private const string WDIO_ATTRIB_SWITCH = "iopin=", WDIO_ATTRIB_BUTTON = "iobutton=";
		private const int WDIO_PIN_COUNT = 14;
		private const char STATE_CONTACT_MADE = '0', STATE_CONTACT_NOTMADE = '1', STATE_UNDEFINED = '?';
		private int[] m_zoneIdMap = new int[WDIO_PIN_COUNT];
		private const char CMD_SWITCH = 'S', CMD_BUTTON = 'B', CMD_PARAM_LOW = 'L', CMD_PARAM_HIGH = 'H';

		public String State
		{
			get { return m_lastState; }
		}

		public WDIO()
		{
			Reinitialise();
		}

		public override void Reinitialise()
		{
			ManagementItem item = ManagementItem.GetManagementItemsInfo().Find(x => x.Manufacturer.ToLower().Equals(IniFile.PARAM_WDIO_MANUFACTURER_NAME[1].ToLower()));
			if (item != null)
			{
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
			for (int i = 0; i < m_zoneIdMap.Length; i++)
			{
				m_zoneIdMap[i] = -1;
				m_lastState += STATE_CONTACT_NOTMADE;
			}
			string pin; char iocmd = '?';
			int startindex, end, pinindex, attriblength = 0;
			foreach (Metadata.ZoneDetails zone in MZPState.Instance.ZoneDetails)
			{
				startindex = zone.ClosureIdList.IndexOf(WDIO_ATTRIB_SWITCH);
				if (startindex != -1)
				{
					iocmd = CMD_SWITCH;
					attriblength = WDIO_ATTRIB_SWITCH.Length;
				}
				else
				{
					startindex = zone.ClosureIdList.IndexOf(WDIO_ATTRIB_BUTTON);
					if (startindex != -1)
					{
						iocmd = CMD_BUTTON;
						attriblength = WDIO_ATTRIB_BUTTON.Length;
					}

				}
				if (startindex != -1)
				{
					end = zone.ClosureIdList.IndexOf(";", startindex);
					if (end == -1) end = zone.ClosureIdList.Length - 1;
					pin = zone.ClosureIdList.Substring(startindex + attriblength, end - startindex - attriblength + 1);
					if (int.TryParse(pin, out pinindex))
						pinindex = Convert.ToInt16(pin);
					else
						MLog.Log(this, "Error parsing pin=" + zone.ClosureIdList + " for zone " + zone.ZoneName);
					if (pinindex >= 0 && pinindex < m_zoneIdMap.Length)
					{
						m_zoneIdMap[pinindex] = zone.ZoneId;
						Thread.Sleep(100);
						MLog.Log(this, "Set IO pin " + pinindex + " zone=" + zone.ZoneName
							+ " result=" + WriteCommand(m_channel + iocmd + Convert.ToChar('A' + pinindex), 1, 100));
					}
					else
						MLog.Log(this, "Error pin index in zone " + zone.ZoneName + " is out of range: " + pinindex);
				}
			}
		}

		public override Boolean TestConnection()
		{
			if (comm == null || !comm.IsPortOpen())
				return false;

			//String result = WriteCommand("\r\n", 1, 100);
			return true;
		}

		protected override void ReceiveSerialResponse(string response)
		{
			char channel, pin, state;
			int pinindex, zoneid;
			//base.ReceiveSerialResponse(response);
			if (response.Length >= 3)
			{
				channel = response[0];//filter later, now all is A
				pin = response[1];
				if (response[2] == CMD_PARAM_LOW)
					state = STATE_CONTACT_MADE;
				else
					if (response[2] == CMD_PARAM_HIGH)
						state = STATE_CONTACT_NOTMADE;
					else
						state = STATE_UNDEFINED;
				if (state != STATE_UNDEFINED)
				{
					pinindex = Convert.ToInt16(pin) - Convert.ToInt16('A');
					zoneid = m_zoneIdMap[pinindex];
					//if (m_lastState[pinindex] != state)
					{
						Metadata.ValueList val = new Metadata.ValueList(Metadata.GlobalParams.zoneid,
							zoneid.ToString(), Metadata.CommandSources.rawinput);
						val.Add(Metadata.GlobalParams.cmdsource, Metadata.CommandSources.rawinput.ToString());
						val.Add(Metadata.GlobalParams.command, Metadata.GlobalCommands.closure.ToString());
						val.Add(Metadata.GlobalParams.id, pinindex.ToString());
						val.Add(Metadata.GlobalParams.iscontactmade, (state == STATE_CONTACT_MADE).ToString());
						API.DoCommand(val);
					}
					//else
					//	MLog.Log(this, "WDIO nochage event on pin=" + pinindex + " state=" + state);
					m_lastState = m_lastState.Substring(0, pinindex) + state 
						+ m_lastState.Substring(pinindex + 1);
					m_lastState = m_lastState.Substring(0, WDIO_PIN_COUNT);
				}
				else
					MLog.Log(this, "Undefined state on response=" + response);
			}
		}


	}

	public class GPIO : GenericModem, IMessenger
	{
		private String m_state, m_lastState;
		private const string STR_ENDLINE = ">", GPIO_ATTRIB_INI_NAME = "iopin=";
		private const int GPIO_PIN_COUNT = 32;
		private const char STATE_CONTACT_MADE = '0', STATE_CONTACT_NOTMADE = '1',
			STR_TIMEOUT_CHAR = '?', STR_EXCEPTION_CHAR = '!';

		private int[] m_zoneIdMap = new int[GPIO_PIN_COUNT];

		public String State
		{
			get { return m_lastState; }
		}

		public GPIO()
		{
			Reinitialise();
		}

		public override void Reinitialise()
		{
			ManagementItem item = ManagementItem.GetManagementItemsInfo().Find(x => x.Manufacturer.ToLower().Equals(IniFile.PARAM_GPIO_CDC_MANUFACTURER_NAME[1].ToLower()));
			if (item != null)
			{
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

			for (int i = 0; i < m_zoneIdMap.Length; i++)
			{
				m_zoneIdMap[i] = -1;
				m_lastState += STATE_CONTACT_NOTMADE;
			}
			string pin;
			int startindex, end, pinindex;
			foreach (Metadata.ZoneDetails zone in MZPState.Instance.ZoneDetails)
			{
				startindex = zone.ClosureIdList.IndexOf(GPIO_ATTRIB_INI_NAME);
				if (startindex != -1)
				{
					end = zone.ClosureIdList.IndexOf(";", startindex);
					if (end == -1) end = zone.ClosureIdList.Length - 1;
					pin = zone.ClosureIdList.Substring(startindex + GPIO_ATTRIB_INI_NAME.Length,
						end - startindex - GPIO_ATTRIB_INI_NAME.Length + 1);
					if (int.TryParse(pin, out pinindex))
						pinindex = Convert.ToInt16(pin);
					else
						MLog.Log(this, "Error parsing pin=" + zone.ClosureIdList + " for zone " + zone.ZoneName);
					if (pinindex >= 0 && pinindex < m_zoneIdMap.Length)
						m_zoneIdMap[pinindex] = zone.ZoneId;
					else
						MLog.Log(this, "Error pin index in zone " + zone.ZoneName + " is out of range: " + pinindex);
				}

			}
		}

		public override Boolean TestConnection()
		{
			if (comm == null || !comm.IsPortOpen())
				return false;

			//String result = WriteCommand("\r\n", 1, 100);
			return true;
		}

		private void ClearPins()
		{
			WriteCommand("", 1, 100);
			Thread.Sleep(100);
			String cmd, result, cmdprefix = "gpio clear ";

			for (int i = 0; i < GPIO_PIN_COUNT; i++)
			{
				cmd = cmdprefix + i;
				result = WriteCommand(cmd, 1, 300, STR_ENDLINE);
			}

			MLog.Log(this, "GPIO Pins cleared");
		}

		public void LoopForEvents()
		{
			String pinstate, cmd;
			int zoneid, j;
			String cmdprefix = "gpio read ";
			//int start;

			do
			{
				m_state = "";
				try
				{
					//MLog.Log(this, "Read GPIO " + m_state);
					if (!IsFaulty())
					{
						for (int i = 0; i < GPIO_PIN_COUNT; i++)
						{
							cmd = cmdprefix + i;
							pinstate = WriteCommand(cmd, 1, 300, STR_ENDLINE);
							pinstate = pinstate.Replace(STR_TIMEOUT, STR_TIMEOUT_CHAR.ToString()).Replace(STR_EXCEPTION, STR_EXCEPTION_CHAR.ToString());
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

						if (m_state != m_lastState)
						{
							for (int i = 0; i < Math.Min(m_state.Length, m_lastState.Length); i++)
							{
								if (m_state[i] != m_lastState[i])
								{
									MLog.Log(this, "Pin " + i + " changed to " + m_state[i]);
									if (m_state[i] != STR_TIMEOUT_CHAR && m_state[i] != STR_EXCEPTION_CHAR)//ignore timeouts on pin reads
									{
										zoneid = m_zoneIdMap[i];
										if (zoneid != -1)
										{
											Metadata.ValueList val = new Metadata.ValueList(Metadata.GlobalParams.zoneid,
												zoneid.ToString(), Metadata.CommandSources.rawinput);
											val.Add(Metadata.GlobalParams.cmdsource, Metadata.CommandSources.rawinput.ToString());
											val.Add(Metadata.GlobalParams.command, Metadata.GlobalCommands.closure.ToString());
											val.Add(Metadata.GlobalParams.id, GPIO_ATTRIB_INI_NAME + i);
											val.Add(Metadata.GlobalParams.iscontactmade, (m_state[i] == STATE_CONTACT_MADE).ToString());
											API.DoCommand(val);
										}
									}
								}
							}
						}

						m_lastState = m_state;
					}
				}
				catch (Exception ex)
				{
					MLog.Log(ex, this, "Error in loopforevents");
				}

				if (IsFaulty())
				{
					j = 0;
					MLog.Log(this, "Serial connection faulty, sleeping for a while");
					do
					{
						j++;
						Thread.Sleep(1000);
					} while (MZPState.Instance != null && j < 60 * 15);
				}
			}
			while (MZPState.Instance != null);

		}


	}

	public class OneWire : IMessenger, DeviceMonitorEventListener
	{
		DSPortAdapter adapter;
		DeviceMonitor dMonitor;
		Thread m_searchThread;
		List<String> m_devices;
		const string ONEWIRE_CONTROLLER_NAME = "DS1990A";

		public OneWire()
		{
			Reinitialise();
		}

		
		public void SendMessageToTarget(string message)
		{
			throw new NotImplementedException();
		}

		public void ReceiveMessage(string message, string sender)
		{
			throw new NotImplementedException();
		}

		public bool TestConnection()
		{
			return true;//dMonitor.isRunning;
		}

		public void Reinitialise()
		{
			try
			{
				MLog.Log(this, "Initialising OneWire");
				try
				{
					adapter = null;
					adapter = OneWireAccessProvider.getDefaultAdapter();
				}
				catch (Exception ex)
				{
					MLog.Log(ex, this, "ERROR init onewire adapter");
				}
				if (adapter == null)
				{
					MLog.Log(this, "Error, OneWire adapter not found");
					return;
				}
				MLog.Log(this, "OneWire adapter=" + adapter.getAdapterName() + " port=" + adapter.getPortName());
				m_devices = new List<String>();
				
				// get exclusive use of adapter
				adapter.beginExclusive(true);

				// clear any previous search restrictions
				adapter.setSearchAllDevices();
				adapter.targetAllFamilies();
				adapter.setSpeed(DSPortAdapter.SPEED_REGULAR);
				java.util.Enumeration containers = adapter.getAllDeviceContainers();
				OneWireContainer element;
				while (containers.hasMoreElements())
				{
					element = (OneWireContainer)containers.nextElement();
					MLog.Log(this, "OneWire device found addr=" + element.getAddressAsString() 
						+" name=" + element.getName() +" desc=" + element.getDescription());
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
			catch (Exception ex)
			{
				MLog.Log(ex,this,"General 1wire exception occurred");
			}

		}

		// The following 3 methods implement the J# interface "DeviceMonitorEventListener" from the 1-Wire API (J#).  
		// These methods are: deviceArrival, deviceDeparture, and networkException.
		//

		// Departure method for the 1-Wire API's DeviceMonitorEventListener interface
		public void deviceDeparture(DeviceMonitorEvent devt)
		{
			int i = 0;
			for (i = 0; i < devt.getDeviceCount(); i++)
			{
				MLog.Log(this, "OneWire Departing: " + devt.getAddressAsStringAt(i));
				m_devices.Remove(devt.getAddressAsStringAt(i));
			}
		}

		// Arrival method for the 1-Wire API's DeviceMonitorEventListener interface
		public void deviceArrival(DeviceMonitorEvent devt)
		{
			int i = 0;
			for (i = 0; i < devt.getDeviceCount(); i++)
			{
				MLog.Log(this,"OneWire Arriving: " + devt.getAddressAsStringAt(i));
				m_devices.Add(devt.getAddressAsStringAt(i));
			}
		}

		// exception for DeviceMonitor
		public void networkException(DeviceMonitorException dexc)
		{
			MLog.Log(this, "1-Wire network exception occurred: " + dexc.exception);
		}

		public void Tick()
		{
			if (adapter != null)
			{
				TemperatureContainer temp;
				// get exclusive use of adapter
				adapter.beginExclusive(true);

				// clear any previous search restrictions
				adapter.setSearchAllDevices();
				adapter.targetAllFamilies();
				adapter.setSpeed(DSPortAdapter.SPEED_REGULAR);
				java.util.Enumeration containers = adapter.getAllDeviceContainers();
				OneWireContainer element;
				sbyte[] state;
				Metadata.ZoneDetails zone;
				while (containers.hasMoreElements())
				{
					element = (OneWireContainer)containers.nextElement();
					//MLog.Log(this, "OneWire device:" + element.getAddressAsString() + element.getName() + element.getDescription());
					zone = MZPState.Instance.ZoneDetails.Find(x => x.TemperatureDeviceId.ToLower() == element.getAddressAsString().ToLower());

					if (zone != null)
					{
						temp = (TemperatureContainer)element;
						state = temp.readDevice();
						temp.doTemperatureConvert(state);
						zone.Temperature = temp.getTemperature(state).ToString();
					}
					else
					{
						if (element.getName() != ONEWIRE_CONTROLLER_NAME)
							MLog.Log(this, "UNNALOCATED OneWire device addr=" + element.getAddressAsString() + " name="+element.getName() + " desc="+element.getDescription());
					}
				}
				adapter.endExclusive();
			}
		}

		public void Close()
		{/*
			// end the 1-Wire network Device Monitor
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

		public bool IsTargetAvailable()
		{
			return false;
		}

		public void MakeBuzz()
		{
			//throw new NotImplementedException();
		}

		public bool IsFaulty()
		{
			return (adapter==null);
		}
	}
}

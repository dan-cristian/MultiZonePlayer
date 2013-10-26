using System;
using System.Text;
using System.Drawing;
using System.IO.Ports;
using System.Threading;
using System.Runtime.InteropServices;
using System.Management;
using System.Collections.Generic;

//using System.Windows.Forms;
//*****************************************************************************************
//                           LICENSE INFORMATION
//*****************************************************************************************
//   PCCom.SerialCommunication Version 1.0.0.0
//   Class file for managing serial port communication
//
//   Copyright (C) 2007  
//   Richard L. McCutchen 
//   Email: richard@psychocoder.net
//   Created: 20OCT07
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//
//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <http://www.gnu.org/licenses/>.
//*****************************************************************************************
namespace MultiZonePlayer
{
    public abstract class SerialBase
    {
		
        public String Connection;
        protected CommunicationManager comm;
        protected Boolean m_waitForResponse = false;
        protected Boolean m_lastOperationWasOK = true;
        protected String m_lastMessageResponse;

        public abstract String SendCommand(Enum cmd, String value);
        public abstract String GetCommandStatus(Enum cmd);
        protected abstract void ReceiveSerialResponse(String response);

        private System.Object m_lockThisSend = new System.Object();
        
        private AutoResetEvent m_autoEventSend = new AutoResetEvent(false);
        private AutoResetEvent m_autoEventReceive = new AutoResetEvent(false);
        private int m_responseTimeoutsCount = 0;

		public const string STR_TIMEOUT = "[timeout]";
		public const string STR_EXCEPTION = "[exception]";

        public int ResponseTimeoutsCount
        {
            get { return m_responseTimeoutsCount; }
        }

        public void Initialise(String baud, String parity, String stopbits, String databits, String port, CommunicationManager.TransmissionType type)
        {
            comm = new CommunicationManager(baud, parity, stopbits, databits, port, type, this.handler);
			comm.OpenPort();
            m_waitForResponse = false;
            m_lastOperationWasOK = true;
            Connection = port;
        }

		public Boolean IsFaulty()
		{
			return comm.IsFaulty();
		}

		public virtual Boolean TestConnection()
		{
			if (comm == null || !comm.IsPortOpen())
				return false;
			else return true;
		}

        public void Disconnect()
        {
			if (comm != null)
				comm.ClosePort();
            
			m_autoEventReceive.Set();
            m_autoEventSend.Set();
        }

		protected String WriteCommand(String cmd, int responseLinesCountExpected, int timeoutms)
		{
			return WriteCommand(cmd, responseLinesCountExpected, timeoutms, null);
		}

        protected String WriteCommand(String cmd, int responseLinesCountExpected, int timeoutms, string expectedResponse)
        {
            String responseMessage="";
			try
			{
				lock (m_lockThisSend)
				{
					if (m_waitForResponse == true)
					{
						MLog.Log(this, "Error, trying to write " + this.ToString() + " command: " + cmd + " while waiting for response");
						return "[thread conflict]";
					}

					m_autoEventSend.Reset();
					m_lastMessageResponse = "";
					m_waitForResponse = true;

					comm.WriteData(cmd);

					//WAIT FOR RESPONSE - ----------------------
					bool signalReceived;

					int responseCount = 0;
					do
					{
						signalReceived = m_autoEventSend.WaitOne(timeoutms);
						if (expectedResponse != null)
						{
							//MLog.Log(this, "Received GPIO response:"+m_lastMessageResponse);
						}
						if (!signalReceived)
						{
							int decim;
							m_responseTimeoutsCount++;
							Math.DivRem(m_responseTimeoutsCount, 400, out decim);
							if (m_responseTimeoutsCount < 5 || decim == 0)
								MLog.Log(this, "No serial response received for cmd=" + cmd + " at count=" + responseCount + " resp=" + m_lastMessageResponse + " number of timeouts=" + m_responseTimeoutsCount);
							if (!m_lastMessageResponse.Contains(STR_TIMEOUT))
								m_lastMessageResponse += STR_TIMEOUT;
						}
						else
						{
							m_responseTimeoutsCount = 0;
							responseCount++;
							if (responseCount < responseLinesCountExpected)
								m_waitForResponse = true;
							m_lastMessageResponse = m_lastMessageResponse.Replace(STR_TIMEOUT, "");
							responseMessage = m_lastMessageResponse;
						}
						m_autoEventReceive.Set();
						
						if (MZPState.Instance == null)
							break;
					}
					while ((responseCount < responseLinesCountExpected && signalReceived && expectedResponse==null)
						|| (responseCount < 10 && m_responseTimeoutsCount <10 && expectedResponse != null && !m_lastMessageResponse.Contains(expectedResponse)));
				}
			}
			catch (Exception ex)
			{
				MLog.Log(ex, this, "Error writeserialcommand");
				if (comm != null) comm.ClosePort();
				m_lastMessageResponse += STR_EXCEPTION;
			}
			finally
			{
				if (responseMessage == "")
					responseMessage = m_lastMessageResponse;
				m_lastMessageResponse = "";
				m_waitForResponse = false;
			}
			return responseMessage;
        }


        protected int handler(String message)
        {
            bool signalReceived;
            if (m_waitForResponse)
            {
                m_lastMessageResponse += message;
                m_autoEventSend.Set();

                signalReceived = m_autoEventReceive.WaitOne(500);
                if (!signalReceived)
                    MLog.Log(this, "Unexpected timeout at serial handler after response=" + message);
            }
            else
                ReceiveSerialResponse(message);
            
            return 0;
            
        }

        public Boolean IsBusy
        {
            get { return m_waitForResponse; }
        }


    }

	public class ExSerialPort : SerialPort
	{
		protected override void Dispose(bool disposing)
		{
			// our variant for
			// 
			// http://social.msdn.microsoft.com/Forums/en-US/netfxnetcom/thread/8b02d5d0-b84e-447a-b028-f853d6c6c690
			// http://connect.microsoft.com/VisualStudio/feedback/details/140018/serialport-crashes-after-disconnect-of-usb-com-port

			var stream = (System.IO.Stream)typeof(SerialPort).GetField("internalSerialStream", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(this);

			if (stream != null)
			{
				try { stream.Dispose(); }
				catch (Exception e) 
				{ MLog.Log(e, "Error exsserial dispose"); }
			}

			base.Dispose(disposing);
		}
	}

    public class CommunicationManager
    {
        #region Manager Enums
        /// <summary>
        /// enumeration to hold our transmission types
        /// </summary>
        public enum TransmissionType { Text, Hex, TextCR, TextR }

        /// <summary>
        /// enumeration to hold our message types
        /// </summary>
        public enum MessageType { Incoming, Outgoing, Normal, Warning, Error };
        #endregion

        #region Manager Variables
        //property variables
        private string _baudRate = string.Empty;
        private string _parity = string.Empty;
        private string _stopBits = string.Empty;
        private string _dataBits = string.Empty;
        private string _portName = string.Empty;
        private TransmissionType _transType;
        private Func<String, int> _callback;
        //private RichTextBox _displayWindow;
        //global manager variables
		private SerialPort comPort = new ExSerialPort();
        private System.Object m_lockThisReceive = new System.Object();
		public Boolean VerboseDebug = true;
		protected DateTime m_lastReinitTryDate = DateTime.MinValue;
		protected int m_reinitTries = 0;
		#endregion

        #region Manager Properties
        /// <summary>
        /// Property to hold the BaudRate
        /// of our manager class
        /// </summary>
        public string BaudRate
        {
            get { return _baudRate; }
            set { _baudRate = value; }
        }

        /// <summary>
        /// property to hold the Parity
        /// of our manager class
        /// </summary>
        public string Parity
        {
            get { return _parity; }
            set { _parity = value; }
        }

        /// <summary>
        /// property to hold the StopBits
        /// of our manager class
        /// </summary>
        public string StopBits
        {
            get { return _stopBits; }
            set { _stopBits = value; }
        }

        /// <summary>
        /// property to hold the DataBits
        /// of our manager class
        /// </summary>
        public string DataBits
        {
            get { return _dataBits; }
            set { _dataBits = value; }
        }

        /// <summary>
        /// property to hold the PortName
        /// of our manager class
        /// </summary>
        public string PortName
        {
            get { return _portName; }
            set { _portName = value; }
        }

        /// <summary>
        /// property to hold our TransmissionType
        /// of our manager class
        /// </summary>
        public TransmissionType CurrentTransmissionType
        {
            get { return _transType; }
            set { _transType = value; }
        }

        
        #endregion

        #region Manager Constructors
        /// <summary>
        /// Constructor to set the properties of our Manager Class
        /// </summary>
        /// <param name="baud">Desired BaudRate</param>
        /// <param name="par">Desired Parity</param>
        /// <param name="sBits">Desired StopBits</param>
        /// <param name="dBits">Desired DataBits</param>
        /// <param name="name">Desired PortName</param>
        public CommunicationManager(string baud, string par, string sBits, string dBits, string name, TransmissionType type, Func<String, int> callback)
        {
            _baudRate = baud;
            _parity = par;
            _stopBits = sBits;
            _dataBits = dBits;
            _portName = name;
			_transType = type;// TransmissionType.Text;
            _callback = callback;
            //now add an event handler
            comPort.DataReceived += new SerialDataReceivedEventHandler(comPort_DataReceived);
            comPort.ErrorReceived += new SerialErrorReceivedEventHandler(comPort_ErrorReceived);
        }

        
        #endregion
		public void Flush()
		{
			if (comPort != null && comPort.IsOpen)
			{
				comPort.DiscardInBuffer();
				comPort.DiscardOutBuffer();
			}
		}

        #region WriteData
        public void WriteData(string msg)
        {
            if (!comPort.IsOpen)
            {
                MLog.Log(this, "Error, COM port " + comPort.PortName+ " was not open at WriteData, opening, cmd="+msg);
				try
				{
					OpenPort();
					if (!comPort.IsOpen)
						return;
				}
				catch (Exception ex)
				{
					MLog.Log(ex, this, "Error, COM port " + comPort.PortName + " cannot be opened at WriteData, cmd=" + msg);
					return;
				}
            }
            
			if (VerboseDebug)
				MLog.LogModem(String.Format("{0} {1} WRITE [{2}] \r\n", DateTime.Now.ToString(), comPort.PortName, msg));

            switch (CurrentTransmissionType)
            {
                case TransmissionType.Text:
                    //send the message to the port
                    comPort.Write(msg+"\r\n");
                    //display the message
                    //DisplayData(MessageType.Outgoing, msg + "\n");
                    break;
				case TransmissionType.TextCR:
				case TransmissionType.TextR:
					//send the message to the port
					comPort.Write(msg + "\r");
					//display the message
					//DisplayData(MessageType.Outgoing, msg + "\n");
					break;
                case TransmissionType.Hex:
                        //convert the message to byte array
                        byte[] newMsg = Utilities.HexToByte(msg);
                        //send the message to the port
                        comPort.Write(newMsg, 0, newMsg.Length);
                        //convert back to hex and display
                        //DisplayData(MessageType.Outgoing, ByteToHex(newMsg) + "\n");
                    
                    break;
                default:
                    //send the message to the port
                    comPort.Write(msg + "\r");
                    
                    //display the message
                    //DisplayData(MessageType.Outgoing, msg + "\n");
                    break;
            }
        }
        #endregion

        

        public bool IsPortOpen()
        {
            return comPort.IsOpen;
        }

        #region OpenPort
        public bool OpenPort()
        {
            try
            {
				Boolean result;
                //first check if the port is already open
                //if its open then close it
				if (comPort.IsOpen == true)
				{
					MLog.Log(this, "Comport already opened com=" + comPort.PortName);
					return true;
					//comPort.Close();
				}
				m_reinitTries++;
				m_lastReinitTryDate = DateTime.Now;

                //set the properties of our SerialPort Object
                comPort.BaudRate = int.Parse(_baudRate);    //BaudRate
                comPort.DataBits = int.Parse(_dataBits);    //DataBits
                comPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), _stopBits);    //StopBits
                comPort.Parity = (Parity)Enum.Parse(typeof(Parity), _parity);    //Parity
                comPort.PortName = _portName;   //PortName
                comPort.Handshake = Handshake.None;
                comPort.RtsEnable = true;
                comPort.DtrEnable = true;
                comPort.WriteTimeout = 500;
                comPort.ReadTimeout = 500;
                //now open the port
                comPort.Open();
				result = comPort.IsOpen;
				if (result)
				{
					m_reinitTries = 0;
					//display message
					MLog.Log(this, "Port " + comPort.PortName + " opened with baud=" + comPort.BaudRate);
					MLog.LogModem(comPort.PortName + " Opened with baud=" + comPort.BaudRate + "\r\n");
				}
				else
					MLog.Log(this, "Port " + comPort.PortName + " failed to be opened");
                return result;
            }
            catch (Exception ex)
            {
				if (comPort.PortName == "COM1")
				{
					MLog.Log(this, "DEBUG TARGET");
				}
                MLog.Log(this, "Error open port " + comPort.PortName + " " + ex.Message);
                return false;
            }
        }
        #endregion

		public Boolean IsFaulty()
		{
			return m_reinitTries > 5 && DateTime.Now.Subtract(m_lastReinitTryDate).TotalMinutes < 30;
		}

        #region ClosePort
        public bool ClosePort()
        {
            try
            {
                //first check if the port is already open
                //if its open then close it
				if (comPort.IsOpen == true)
				{
					comPort.Close();
					//display message
					MLog.Log(this, "Port closed");
					MLog.LogModem(comPort.PortName + " Closed\r\n");
				}
				//return true if port is closed
                if (comPort.IsOpen == false) 
                    return true;
                MLog.Log(this, "Issue closing port");
                return false;
            }
            catch (Exception ex)
            {
                MLog.Log(ex, "err close port");
                return false;
            }
        }
        #endregion


        /*
        #region SetParityValues
        
        public void SetParityValues(object obj)
        {
            foreach (string str in Enum.GetNames(typeof(Parity)))
            {
                ((ComboBox)obj).Items.Add(str);
            }
        }
        #endregion

        #region SetStopBitValues
        public void SetStopBitValues(object obj)
        {
            foreach (string str in Enum.GetNames(typeof(StopBits)))
            {
                ((ComboBox)obj).Items.Add(str);
            }
        }
        #endregion

        #region SetPortNameValues
        public void SetPortNameValues(object obj)
        {

            foreach (string str in SerialPort.GetPortNames())
            {
                ((ComboBox)obj).Items.Add(str);
            }
        }
        #endregion
         */

        #region comPort_DataReceived
        /// <summary>
        /// method that will be called when theres data waiting in the buffer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void comPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
			string msg, msgDisplay="";
            try
            {
				lock (m_lockThisReceive)    
                {
					if (Thread.CurrentThread != null && 
						(Thread.CurrentThread.Name == null ||
						!Thread.CurrentThread.Name.StartsWith("Serial Data")))
						Thread.CurrentThread.Name = "Serial Received " + comPort.PortName;
					Thread th;
					
					//determine the mode the user selected (binary/string)
                    switch (CurrentTransmissionType)
                    {
                        //user chose string
                        case TransmissionType.Text:
                            while (comPort != null && comPort.BytesToRead > 0)
                            {
                                //read data waiting in the buffer
                                try
                                {
                                    msg = comPort.ReadLine();//comPort.ReadExisting();
                                }
                                catch (TimeoutException)
                                {
                                    //MLog.Log(this, "Timeout comport " + comPort.PortName);
                                    msg = comPort.ReadExisting();
                                }
								msgDisplay = msg.Replace("\r", "{R}").Replace("\n", "{N}");
								if (VerboseDebug)
									MLog.LogModem(String.Format("{0} {1}   READ [{2}] len={3}\r\n", DateTime.Now.ToString(), comPort.PortName, msgDisplay, msg.Length));
								/*if (msg.Length == 1)
                                {
                                    MLog.LogModem(String.Format("{0} {1} READ 1 CHAR [{2}]\r\n",DateTime.Now.ToString(), comPort.PortName, + Convert.ToByte(msg[0])));
                                    //comPort.Close();
                                }*/
								th = new Thread(() => _callback(msg));
								th.Name = "Serial Received " + comPort.PortName + " Exec";
								th.Start();
                                //_callback(msg);
                            }
                            //string msg = comPort.ReadLine();

                            //display the data to the user
                            //DisplayData(MessageType.Incoming, msg + "\n");
                            break;
						case TransmissionType.TextCR:
							while (comPort != null && comPort.BytesToRead > 0)
							{
								//read data waiting in the buffer
								try
								{
									msg = comPort.ReadExisting();
								}
								catch (TimeoutException)
								{
									msg = comPort.ReadExisting();
								}
								msgDisplay = msg.Replace("\r", "{R}").Replace("\n", "{N}");
								if (VerboseDebug)
									MLog.LogModem(String.Format("{0} {1}   READ [{2}] len={3}\r\n", DateTime.Now.ToString(), comPort.PortName, msgDisplay, msg.Length));

								th = new Thread(() => _callback(msg));
								th.Name = "Serial Received " + comPort.PortName + " Exec";
								th.Start();
								//_callback(msg);
							}
							break;
						case TransmissionType.TextR:
							string[] msgarray;
							while (comPort != null && comPort.BytesToRead > 0)
							{
								//read data waiting in the buffer
								try
								{
									msg = comPort.ReadTo("\r");
									msgDisplay = msg.Replace("\r", "{R}").Replace("\n", "{N}");
									if (VerboseDebug)
										MLog.LogModem(String.Format("{0} {1}   READ [{2}] len={3}\r\n", DateTime.Now.ToString(),
											comPort.PortName, msgDisplay, msg.Length));
									msgarray = msg.Split(new char[]{'\r'}, StringSplitOptions.RemoveEmptyEntries);
									foreach (string atom in msgarray)
									{
										msgDisplay = atom.Replace("\r", "{R}").Replace("\n", "{N}");
										if (VerboseDebug)
											MLog.LogModem(String.Format("{0} {1}        read split [{2}] len={3}\r\n", DateTime.Now.ToString(), 
												comPort.PortName, msgDisplay, atom.Length));

										th = new Thread(() => _callback(atom));
										th.Name = "Serial Received " + comPort.PortName + " Exec";
										th.Start();
										//_callback(atom);
									}
								}
								catch (TimeoutException)
								{
									//msg = comPort.ReadExisting();
								}
							}
							break;
                        //user chose binary
                        case TransmissionType.Hex:
							byte[] comBuffer;
							for (int i = 0; i <= 1; i++)
							{
								//retrieve number of bytes in the buffer
								int bytes = comPort.BytesToRead;
								//create a byte array to hold the awaiting data
								comBuffer = new byte[bytes];
								//read the data and store it
								comPort.Read(comBuffer, 0, bytes);
								msgDisplay += Utilities.ByteToHex(comBuffer);
								Thread.Sleep(100);
							}
							if (VerboseDebug)
								MLog.LogModem(String.Format("{0} {1}   READ [{2}] len={3}\r\n", DateTime.Now.ToString(), comPort.PortName, msgDisplay, msgDisplay.Length));
                            //display the data to the user
                            //DisplayData(MessageType.Incoming, ByteToHex(comBuffer) + "\n");
							th = new Thread(() => _callback(msgDisplay));
							th.Name = "Serial Received " + comPort.PortName + " Exec";
							th.Start();
							//_callback(msgDisplay);
							break;
                        
						 default:
                            //read data waiting in the buffer
                            string str = comPort.ReadExisting();
                            //display the data to the user
                            //DisplayData(MessageType.Incoming, str + "\n");
                            break;
                    }

                }
			}
            catch (Exception ex)
            {
                MLog.Log(ex, "Error com port data received com=" + comPort!=null?comPort.PortName:" null port");
            }
            
        }
        void comPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
			try
			{
				if (Thread.CurrentThread.Name == null || !Thread.CurrentThread.Name.StartsWith("Serial Error"))
					Thread.CurrentThread.Name = "Serial Error Received " + comPort.PortName;
				MLog.Log(this, "ERROR received sender=" + sender.ToString() + " evtype=" + e.EventType);
			}
			catch (Exception ex)
			{
				MLog.Log(ex, "Error com port error received com=" + comPort != null ? comPort.PortName : " null port");
			}
        }
        #endregion
    }

	public class SetupDiWrap
	{
		static public string ComPortNameFromFriendlyNamePrefix(string friendlyNamePrefix)
		{
			const string className = "Ports";
			Guid[] guids = GetClassGUIDs(className);

			System.Text.RegularExpressions.Regex friendlyNameToComPort =
				new System.Text.RegularExpressions.Regex(@".? \((COM\d+)\)$");  // "..... (COMxxx)" -> COMxxxx

			foreach (Guid guid in guids)
			{
				// We start at the "root" of the device tree and look for all
				// devices that match the interface GUID of a disk
				Guid guidClone = guid;
				IntPtr h = SetupDiGetClassDevs(ref guidClone, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_PROFILE);
				if (h.ToInt32() != INVALID_HANDLE_VALUE)
				{
					int nDevice = 0;
					while (true)
					{
						SP_DEVINFO_DATA da = new SP_DEVINFO_DATA();
						da.cbSize = (uint)Marshal.SizeOf(da);

						if (0 == SetupDiEnumDeviceInfo(h, nDevice++, ref da))
							break;

						uint RegType;
						byte[] ptrBuf = new byte[BUFFER_SIZE];
						uint RequiredSize;
						if (SetupDiGetDeviceRegistryProperty(h, ref da,
							(uint)SPDRP.FRIENDLYNAME, out RegType, ptrBuf,
							BUFFER_SIZE, out RequiredSize))
						{
							const int utf16terminatorSize_bytes = 2;
							string friendlyName = System.Text.UnicodeEncoding.Unicode.GetString(ptrBuf, 0, (int)RequiredSize - utf16terminatorSize_bytes);

							MLog.Log(null, "com friendly name: " + friendlyName);
							if (!friendlyName.StartsWith(friendlyNamePrefix))
								continue;

							if (!friendlyNameToComPort.IsMatch(friendlyName))
								continue;

							return friendlyNameToComPort.Match(friendlyName).Groups[1].Value;
						}
					} // devices
					SetupDiDestroyDeviceInfoList(h);
				}
			} // class guids

			return null;
		}

		/// <summary>
		/// The SP_DEVINFO_DATA structure defines a device instance that is a member of a device information set.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		private struct SP_DEVINFO_DATA
		{
			/// <summary>Size of the structure, in bytes.</summary>
			public uint cbSize;
			/// <summary>GUID of the device interface class.</summary>
			public Guid ClassGuid;
			/// <summary>Handle to this device instance.</summary>
			public uint DevInst;
			/// <summary>Reserved; do not use.</summary>
			public uint Reserved;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct SP_DEVICE_INTERFACE_DATA
		{
			public Int32 cbSize;
			public Guid interfaceClassGuid;
			public Int32 flags;
			private UIntPtr reserved;
		}

		const int BUFFER_SIZE = 1024;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		private struct SP_DEVICE_INTERFACE_DETAIL_DATA
		{
			public int cbSize;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = BUFFER_SIZE)]
			public string DevicePath;
		}

		private enum SPDRP
		{
			DEVICEDESC = 0x00000000,
			HARDWAREID = 0x00000001,
			COMPATIBLEIDS = 0x00000002,
			NTDEVICEPATHS = 0x00000003,
			SERVICE = 0x00000004,
			CONFIGURATION = 0x00000005,
			CONFIGURATIONVECTOR = 0x00000006,
			CLASS = 0x00000007,
			CLASSGUID = 0x00000008,
			DRIVER = 0x00000009,
			CONFIGFLAGS = 0x0000000A,
			MFG = 0x0000000B,
			FRIENDLYNAME = 0x0000000C,
			LOCATION_INFORMATION = 0x0000000D,
			PHYSICAL_DEVICE_OBJECT_NAME = 0x0000000E,
			CAPABILITIES = 0x0000000F,
			UI_NUMBER = 0x00000010,
			UPPERFILTERS = 0x00000011,
			LOWERFILTERS = 0x00000012,
			MAXIMUM_PROPERTY = 0x00000013,
		}

		[DllImport("setupapi.dll", SetLastError = true)]
		static extern bool SetupDiClassGuidsFromName(string ClassName,
			ref Guid ClassGuidArray1stItem, UInt32 ClassGuidArraySize,
			out UInt32 RequiredSize);

		[DllImport("setupapi.dll")]
		internal static extern IntPtr SetupDiGetClassDevsEx(IntPtr ClassGuid,
			[MarshalAs(UnmanagedType.LPStr)]String enumerator,
			IntPtr hwndParent, Int32 Flags, IntPtr DeviceInfoSet,
			[MarshalAs(UnmanagedType.LPStr)]String MachineName, IntPtr Reserved);

		[DllImport("setupapi.dll")]
		internal static extern Int32 SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

		[DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern Boolean SetupDiEnumDeviceInterfaces(
		   IntPtr hDevInfo,
		   IntPtr optionalCrap, //ref SP_DEVINFO_DATA devInfo,
		   ref Guid interfaceClassGuid,
		   UInt32 memberIndex,
		   ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData
		);

		[DllImport("setupapi.dll")]
		private static extern Int32 SetupDiEnumDeviceInfo(IntPtr DeviceInfoSet,
			Int32 MemberIndex, ref SP_DEVINFO_DATA DeviceInterfaceData);

		[DllImport("setupapi.dll")]
		private static extern Int32 SetupDiClassNameFromGuid(ref Guid ClassGuid,
			StringBuilder className, Int32 ClassNameSize, ref Int32 RequiredSize);

		[DllImport("setupapi.dll")]
		private static extern Int32 SetupDiGetClassDescription(ref Guid ClassGuid,
			StringBuilder classDescription, Int32 ClassDescriptionSize, ref Int32 RequiredSize);

		[DllImport("setupapi.dll")]
		private static extern Int32 SetupDiGetDeviceInstanceId(IntPtr DeviceInfoSet,
			ref SP_DEVINFO_DATA DeviceInfoData,
			StringBuilder DeviceInstanceId, Int32 DeviceInstanceIdSize, ref Int32 RequiredSize);

		[DllImport("setupapi.dll", CharSet = CharSet.Auto)]
		static extern IntPtr SetupDiGetClassDevs(           // 1st form using a ClassGUID only, with null Enumerator
		   ref Guid ClassGuid,
		   IntPtr Enumerator,
		   IntPtr hwndParent,
		   int Flags
		);

		[DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern Boolean SetupDiGetDeviceInterfaceDetail(
		   IntPtr hDevInfo,
		   ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
		   ref SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData,
		   UInt32 deviceInterfaceDetailDataSize,
		   out UInt32 requiredSize,
		   ref SP_DEVINFO_DATA deviceInfoData
		);

		/// <summary>
		/// The SetupDiGetDeviceRegistryProperty function retrieves the specified device property.
		/// This handle is typically returned by the SetupDiGetClassDevs or SetupDiGetClassDevsEx function.
		/// </summary>
		/// <param Name="DeviceInfoSet">Handle to the device information set that contains the interface and its underlying device.</param>
		/// <param Name="DeviceInfoData">Pointer to an SP_DEVINFO_DATA structure that defines the device instance.</param>
		/// <param Name="Property">Device property to be retrieved. SEE MSDN</param>
		/// <param Name="PropertyRegDataType">Pointer to a variable that receives the registry data Type. This parameter can be NULL.</param>
		/// <param Name="PropertyBuffer">Pointer to a buffer that receives the requested device property.</param>
		/// <param Name="PropertyBufferSize">Size of the buffer, in bytes.</param>
		/// <param Name="RequiredSize">Pointer to a variable that receives the required buffer size, in bytes. This parameter can be NULL.</param>
		/// <returns>If the function succeeds, the return value is nonzero.</returns>
		[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern bool SetupDiGetDeviceRegistryProperty(
			IntPtr DeviceInfoSet,
			ref SP_DEVINFO_DATA DeviceInfoData,
			uint Property,
			out UInt32 PropertyRegDataType,
			byte[] PropertyBuffer,
			uint PropertyBufferSize,
			out UInt32 RequiredSize);


		const int DIGCF_DEFAULT = 0x1;
		const int DIGCF_PRESENT = 0x2;
		const int DIGCF_ALLCLASSES = 0x4;
		const int DIGCF_PROFILE = 0x8;
		const int DIGCF_DEVICEINTERFACE = 0x10;
		const int INVALID_HANDLE_VALUE = -1;

		private static Guid[] GetClassGUIDs(string className)
		{
			UInt32 requiredSize = 0;
			Guid[] guidArray = new Guid[1];

			bool status = SetupDiClassGuidsFromName(className, ref guidArray[0], 1, out requiredSize);
			if (true == status)
			{
				if (1 < requiredSize)
				{
					guidArray = new Guid[requiredSize];
					SetupDiClassGuidsFromName(className, ref guidArray[0], requiredSize, out requiredSize);
				}
			}
			else
				throw new System.ComponentModel.Win32Exception();

			return guidArray;
		}


	}

	internal class ProcessConnection
	{

		public static ConnectionOptions ProcessConnectionOptions()
		{
			ConnectionOptions options = new ConnectionOptions();
			options.Impersonation = ImpersonationLevel.Impersonate;
			options.Authentication = AuthenticationLevel.Default;
			options.EnablePrivileges = true;
			return options;
		}

		public static ManagementScope ConnectionScope(string machineName, ConnectionOptions options, string path)
		{
			ManagementScope connectScope = new ManagementScope();
			connectScope.Path = new ManagementPath(@"\\" + machineName + path);
			connectScope.Options = options;
			connectScope.Connect();
			return connectScope;
		}
	}

	public class ManagementItem
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string Manufacturer { get; set; }
		public ManagementObject Object { get; set; }

		public ManagementItem() { }

		public static List<ManagementItem> GetManagementItemsInfo()
		{
			List<ManagementItem> itemInfoList = new List<ManagementItem>();

			ConnectionOptions options = ProcessConnection.ProcessConnectionOptions();
			ManagementScope connectionScope = ProcessConnection.ConnectionScope(Environment.MachineName, options, @"\root\CIMV2");

			ObjectQuery objectQuery = new ObjectQuery("SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode = 0");
			ManagementObjectSearcher itemSearcher = new ManagementObjectSearcher(connectionScope, objectQuery);

			using (itemSearcher)
			{
				foreach (ManagementObject obj in itemSearcher.Get())
				{
					if (obj != null)
					{
						ManagementItem item = new ManagementItem();
						item.Name = obj["Caption"]!=null?obj["Caption"].ToString():"";
						item.Description = obj["Description"] != null ? obj["Description"].ToString() : "";
						item.Manufacturer = obj["Manufacturer"] != null ? obj["Manufacturer"].ToString() : "";
						item.Object = obj;
						itemInfoList.Add(item);
						/*
						object captionObj = obj["Caption"];
						if (captionObj != null)
						{
							caption = captionObj.ToString();
							MLog.Log(null, "WINMANAG raw: " + caption + ", man="+obj["Manufacturer"] +", desc="+obj["Description"]+
								", devid=" + obj["DeviceID"] + ", name=" + obj["Name"] + ", pnpid=" + obj["PNPDeviceID"]);
							if (caption.Contains("(COM"))
							{
								ManagementItem comPortInfo = new ManagementItem();
								comPortInfo.Name = caption.Substring(caption.LastIndexOf("(COM")).Replace("(", string.Empty).Replace(")",
																	 string.Empty);
								comPortInfo.Description = caption;
								comPortInfoList.Add(comPortInfo);
							}
						}*/
					}
				}
			}
			return itemInfoList;
		}
	}
}

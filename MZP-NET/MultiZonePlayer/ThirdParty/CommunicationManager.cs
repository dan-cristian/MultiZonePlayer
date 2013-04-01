using System;
using System.Text;
using System.Drawing;
using System.IO.Ports;
using System.Threading;

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

        public int ResponseTimeoutsCount
        {
            get { return m_responseTimeoutsCount; }
        }

        public void Initialise(String baud, String parity, String stopbits, String databits, String port)
        {
            comm = new CommunicationManager(baud, parity, stopbits, databits, port, this.handler);
            comm.OpenPort();
            m_waitForResponse = false;
            m_lastOperationWasOK = true;
            Connection = port;
        }

        public void Disconnect()
        {
            comm.ClosePort();
            m_autoEventReceive.Set();
            m_autoEventSend.Set();
        }

        protected String WriteCommand(String cmd, int responseLinesCountExpected, int timeoutms)
        {
            String responseMessage="";
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

                    if (!signalReceived)
                    {
                        int decim;
                        m_responseTimeoutsCount++;
                        Math.DivRem(m_responseTimeoutsCount, 400, out decim);
                        if (m_responseTimeoutsCount < 5 || decim == 0 )
                            MLog.Log(this, "No serial response received for cmd=" + cmd + " at count=" + responseCount + " resp="+m_lastMessageResponse + " number of timeouts="+m_responseTimeoutsCount);
                        m_lastMessageResponse += "[timeout]";
                    }
                    else
                    {
                        m_responseTimeoutsCount = 0;
                        responseCount++;
                        if (responseCount < responseLinesCountExpected)
                            m_waitForResponse = true;
                        responseMessage = m_lastMessageResponse;
                    }
                    m_autoEventReceive.Set();
                }
                while (responseCount<responseLinesCountExpected && signalReceived);
                if (responseMessage == "")
                    responseMessage = m_lastMessageResponse;
                m_lastMessageResponse = "";
                m_waitForResponse = false;
                return responseMessage;
            }
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

    public class CommunicationManager
    {
        #region Manager Enums
        /// <summary>
        /// enumeration to hold our transmission types
        /// </summary>
        public enum TransmissionType { Text, Hex }

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
        private SerialPort comPort = new SerialPort();
        private System.Object m_lockThisReceive = new System.Object();
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
        public CommunicationManager(string baud, string par, string sBits, string dBits, string name, Func<String, int> callback)
        {
            _baudRate = baud;
            _parity = par;
            _stopBits = sBits;
            _dataBits = dBits;
            _portName = name;
            _transType = TransmissionType.Text;
            _callback = callback;
            //now add an event handler
            comPort.DataReceived += new SerialDataReceivedEventHandler(comPort_DataReceived);
            comPort.ErrorReceived += new SerialErrorReceivedEventHandler(comPort_ErrorReceived);
        }

        
        #endregion

        #region WriteData
        public void WriteData(string msg)
        {
            if (!(comPort.IsOpen == true))
            {
                MLog.Log(this, "Error, COM port " + comPort.PortName+ " was not open at WriteData, opening, cmd="+msg);
				try
				{
					comPort.Open();
				}
				catch (Exception ex)
				{
					MLog.Log(ex, "Error, COM port " + comPort.PortName + " cannot be opened at WriteData, cmd=" + msg);
					return;
				}
            }
            
            MLog.LogModem(String.Format("{0} {1} WRITE [{2}] \r\n", DateTime.Now.ToString(), comPort.PortName, msg));

            switch (CurrentTransmissionType)
            {
                case TransmissionType.Text:
                    //send the message to the port
                    comPort.Write(msg+"\r\n");
                    //display the message
                    //DisplayData(MessageType.Outgoing, msg + "\n");
                    break;
                case TransmissionType.Hex:
                    
                        //convert the message to byte array
                        byte[] newMsg = HexToByte(msg);
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

        #region HexToByte
        /// <summary>
        /// method to convert hex string into a byte array
        /// </summary>
        /// <param name="msg">string to convert</param>
        /// <returns>a byte array</returns>
        private byte[] HexToByte(string msg)
        {
            //remove any spaces from the string
            msg = msg.Replace(" ", "");
            //create a byte array the length of the
            //divided by 2 (Hex is 2 characters in length)
            byte[] comBuffer = new byte[msg.Length / 2];
            //loop through the length of the provided string
            for (int i = 0; i < msg.Length; i += 2)
                //convert each set of 2 characters to a byte
                //and add to the array
                comBuffer[i / 2] = (byte)Convert.ToByte(msg.Substring(i, 2), 16);
            //return the array
            return comBuffer;
        }
        #endregion

        #region ByteToHex
        /// <summary>
        /// method to convert a byte array into a hex string
        /// </summary>
        /// <param name="comByte">byte array to convert</param>
        /// <returns>a hex string</returns>
        private string ByteToHex(byte[] comByte)
        {
            //create a new StringBuilder object
            StringBuilder builder = new StringBuilder(comByte.Length * 3);
            //loop through each byte in the array
            foreach (byte data in comByte)
                //convert the byte to a string and add to the stringbuilder
                builder.Append(Convert.ToString(data, 16).PadLeft(2, '0').PadRight(3, ' '));
            //return the converted value
            return builder.ToString().ToUpper();
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
                //first check if the port is already open
                //if its open then close it
                if (comPort.IsOpen == true) comPort.Close();

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
                //display message
                MLog.Log(this,"Port "+comPort.PortName +" opened with baud=" + comPort.BaudRate);
                MLog.LogModem(comPort.PortName + " Opened with baud=" + comPort.BaudRate +"\r\n");
                //return true
                return true;
            }
            catch (Exception ex)
            {
                MLog.Log(this, "Error open port " + comPort.PortName + " " + ex.Message);
                return false;
            }
        }
        #endregion

        #region ClosePort
        public bool ClosePort()
        {
            try
            {
                //first check if the port is already open
                //if its open then close it
                if (comPort.IsOpen == true) comPort.Close();
                //display message
                MLog.Log(this, "Port closed");
                MLog.LogModem(comPort.PortName + " Closed\r\n");
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
            lock (m_lockThisReceive)
            {
                try
                {
					if (Thread.CurrentThread != null && 
						(Thread.CurrentThread.Name == null ||
						!Thread.CurrentThread.Name.StartsWith("Serial Data")))
						Thread.CurrentThread.Name = "Serial Data Received " + comPort.PortName;
                    //determine the mode the user selected (binary/string)
                    switch (CurrentTransmissionType)
                    {
                        //user chose string
                        case TransmissionType.Text:
                            while (comPort != null && comPort.BytesToRead > 0)
                            {
                                string msg, msgDisplay;
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
                                MLog.LogModem(String.Format("{0} {1}   READ [{2}] len={3}\r\n", DateTime.Now.ToString(), comPort.PortName, msgDisplay, msg.Length));
                                /*if (msg.Length == 1)
                                {
                                    MLog.LogModem(String.Format("{0} {1} READ 1 CHAR [{2}]\r\n",DateTime.Now.ToString(), comPort.PortName, + Convert.ToByte(msg[0])));
                                    //comPort.Close();
                                }*/
                                _callback(msg);
                            }
                            //string msg = comPort.ReadLine();

                            //display the data to the user
                            //DisplayData(MessageType.Incoming, msg + "\n");
                            break;
                        //user chose binary
                        case TransmissionType.Hex:
                            //retrieve number of bytes in the buffer
                            int bytes = comPort.BytesToRead;
                            //create a byte array to hold the awaiting data
                            byte[] comBuffer = new byte[bytes];
                            //read the data and store it
                            comPort.Read(comBuffer, 0, bytes);
                            //display the data to the user
                            //DisplayData(MessageType.Incoming, ByteToHex(comBuffer) + "\n");
                            break;
                        default:
                            //read data waiting in the buffer
                            string str = comPort.ReadExisting();
                            //display the data to the user
                            //DisplayData(MessageType.Incoming, str + "\n");
                            break;
                    }

                }
                catch (Exception ex)
                {
                    MLog.Log(ex, "Error com port data received com=" + comPort.PortName);
                }
            }
        }
        void comPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
			try
			{
				if (Thread.CurrentThread.Name != null && !Thread.CurrentThread.Name.StartsWith("Serial Error"))
					Thread.CurrentThread.Name = "Serial Error Received " + comPort.PortName;
				MLog.Log(this, "ERROR received sender=" + sender.ToString() + " errtype=" + e.ToString());
			}
			catch (Exception ex)
			{
				MLog.Log(ex, "Error com port error received com=" + comPort.PortName);
			}
        }
        #endregion
    }
}

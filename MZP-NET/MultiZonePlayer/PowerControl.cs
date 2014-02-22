using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using HE853;

namespace MultiZonePlayer
{
    public abstract class BasePowerControl
    {
        protected static int powerSavingCycleCount = 0;
        protected static bool isPowerSavingMode = false;
        protected String m_deviceName;
        protected String m_socketsStatus="";

		public BasePowerControl()
		{
		}

        public BasePowerControl(String deviceName)
        {
            isPowerSavingMode = true;
            m_deviceName = deviceName;
        }
        
        public void timerPowerSaving_Tick()
        {
            //currentZone = null;
            if (ControlCenter.GetActiveZonesCount() == 0)
            {
                powerSavingCycleCount++;
            }

            if (powerSavingCycleCount == IniFile.POWERSAVING_CYCLE_WARNING)
            {
                //play warning message
            }

            if ((!isPowerSavingMode) && (powerSavingCycleCount > IniFile.POWERSAVING_MAX_CYCLES))
            {
                ExecutePowerSavingsActions();
            }
        }

        public void ExecutePowerSavingsActions()
        {
            if (!ControlCenter.IsPowerControlEnabled)
                return;
			/*
            MLog.Log(null,"Power saving");
            //tell windows it can go into idle
            Utilities.SetThreadExecutionState(Utilities.EXECUTION_STATE.ES_CONTINUOUS);
            //run external power save actions
            MLog.Log(null,"Wait for power saving action");
            Utilities.RunProcessWait(IniFile.CurrentPath() + IniFile.PARAM_POWER_SAVING_APP[1], 
				System.Diagnostics.ProcessWindowStyle.Minimized);
            MLog.Log(null,"Wait completed for power saving action");

            isPowerSavingMode = true;
			 */
        }

        public void ResumeFromPowerSaving()
        {
            if (!ControlCenter.IsPowerControlEnabled)
                return;

            if (!isPowerSavingMode)
                return;
            MLog.Log(null,"Power resume");
			/*
            isPowerSavingMode = false;
            //tell windows it cannot go into idle
            Utilities.SetThreadExecutionState(Utilities.EXECUTION_STATE.ES_SYSTEM_REQUIRED);

            Utilities.RunProcessWait(IniFile.CurrentPath() + IniFile.PARAM_POWER_RESUME_APP[1],
				System.Diagnostics.ProcessWindowStyle.Minimized);

            //resume from power saving
            powerSavingCycleCount = 0;
			 */
        }

        public int GetInactivityCycles()
        {
            return powerSavingCycleCount;
        }

        public bool IsPowerSavingMode()
        {
            return isPowerSavingMode;
        }

        public bool SocketsOn(int socketIndex)
        {
            if (m_socketsStatus.Length > socketIndex)
                return m_socketsStatus[socketIndex] == 1;
            else
                return false;
        }

        public String SocketsStatus
        {
            get { return m_socketsStatus; }
        }

        protected abstract void Open();
        protected abstract void Close();
        public abstract bool IsPowerControlOn();
		public virtual String GetPowerControlName()
		{
			return m_deviceName;
		}
        public abstract void PowerOn(int zoneId);
        public abstract void PowerOff(int zoneId);
        public abstract void PowerOff();
        public abstract bool IsPowerOn(int zoneId);
    }

    class DenkoviPowerControl : BasePowerControl
    {
        private FTD2XX_NET.FTDI m_usb8Relay;
        private String m_deviceSerial="";
        private const String STATE_ALL_ON  = "11111111";
        private const String STATE_ALL_OFF = "00000000";
        private String m_relayState = STATE_ALL_OFF;
        //private Hashtable m_zoneIndexes = null;
        private DateTime m_lastOpenDateTime;
        private static List<Thread> m_threadList;

        public DenkoviPowerControl(String deviceName):base(deviceName)
        {
			Reinitialise();
			PowerOff();
        }

		public void Reinitialise()
		{
			MLog.Log(this, "Reinitialise Denkovi, trying to close");
			try
			{
				Close();
				MLog.Log(this, "Reinitialising, closed existing relay instance");
			}
			catch (Exception ex)
			{
				MLog.Log(ex, this, "Unable to close usb relay");
			}
			MLog.Log(this, "Reinitialising, opening relay");
			try
			{
				m_deviceSerial = Utilities.FindFTDIComPortFromDesc(IniFile.PARAM_RELAY_DEVICE_NAME[1].ToLower(), false);
				m_usb8Relay = new FTD2XX_NET.FTDI();
				m_usb8Relay.SetLatency(32);//!!! check this
				m_lastOpenDateTime = DateTime.MinValue;
				m_threadList = new List<Thread>();
				
			}
			catch (Exception ex)
			{
				MLog.Log(ex, this, "Unable to reinitialise usb relay");
			}
		}

        ~DenkoviPowerControl()
        {
            PowerOff();
        }

        protected override void Open()
        {
			if (!m_usb8Relay.IsOpen)
			{
				FTD2XX_NET.FTDI.FT_STATUS status;
				try
				{
					status = m_usb8Relay.OpenBySerialNumber(m_deviceSerial);
					if (status == FTD2XX_NET.FTDI.FT_STATUS.FT_OK)
					{
						m_lastOpenDateTime = DateTime.Now;
						status = m_usb8Relay.SetBitMode(255, 1);
						status = m_usb8Relay.SetTimeouts(1000, 1000);
						if (status != FTD2XX_NET.FTDI.FT_STATUS.FT_OK)
							MLog.Log(this, "Setbit failed on Open, status = " + status);
					}
					else
					{
						Alert.CreateAlert("Denkovi Open failed, status = " + status);
						//Reinitialise();
					}
					LogDebugInfo();
				}
				catch (Exception ex)
				{
					MLog.Log(ex,this, "Unable to open device " + m_deviceName + " id=" + m_deviceSerial + " err=" + ex.Message);
					//Close();
				}
			}
			else MLog.Log(this, "Relay already opened on open, skip");
        }

        protected override void Close()
        {
			if (m_usb8Relay!=null && m_usb8Relay.IsOpen)
			{
				FTD2XX_NET.FTDI.FT_STATUS status;

				try
				{
					Thread.Sleep(100);
					MLog.Log(this,"Closing relay, reading all data");
					ReadAllData();
					//MLog.Log(this,"Closing relay, purge rx");
					m_usb8Relay.Purge(FTD2XX_NET.FTDI.FT_PURGE.FT_PURGE_RX);
					//MLog.Log(this,"Closing relay, purge tx");
					m_usb8Relay.Purge(FTD2XX_NET.FTDI.FT_PURGE.FT_PURGE_TX);
					Thread.Sleep(100);
					//MLog.Log(this, "Now resetting");
					m_usb8Relay.ResetDevice();
					Thread.Sleep(100);
					//MLog.Log(this,"Now closing");
					status = m_usb8Relay.Close();
					MLog.Log(this,"Now closed");
				}
				catch (Exception ex)
				{
					MLog.Log(ex,this, "Unable to close device " + m_deviceName + " id=" + m_deviceSerial + " err=" + ex.Message);
				}
			}
			else MLog.Log(this, "Relay already closed on close, skip");
        }

        public override bool IsPowerControlOn()
        {
            return m_usb8Relay.IsOpen;
        }

        public override bool IsPowerOn(int zoneId)
        {
            int[] index = GetSocketIndexForZone(zoneId);
			bool res;
			if (index.Length > 0)
			{
				res = m_socketsStatus[index[0] - 1] == '1';
			}
			else res = false;
			return res;
        }

        private void ReadAllData()
        {
            FTD2XX_NET.FTDI.FT_STATUS status;
            uint uvalue = 0;
            //MLog.Log(this,"Reading bytes avail");
            status = m_usb8Relay.GetRxBytesAvailable(ref uvalue);
			//MLog.Log(this, "Reading bytes avail="+uvalue);
            if (status == FTD2XX_NET.FTDI.FT_STATUS.FT_OK)
            {
                if ((uvalue > 0))
                {
                    //MLog.Log(null,"RX buffer has data, bytecount=" + uvalue);
                    byte[] dataBuffer = new byte[uvalue];
                    uint numRead = 0;
					Thread.Sleep(100);
                    status = m_usb8Relay.Read(dataBuffer, uvalue, ref numRead);
                    if (status != FTD2XX_NET.FTDI.FT_STATUS.FT_OK)
                    //    MLog.Log(this,"Read data numread=" + numRead + " status=" + status);
                    //else
                        MLog.Log(this,"Read failed, status = " + status);
                }
            }
            else
                MLog.Log(this,"GetRXBytes failed, status = " + status);
        }

        public override String GetPowerControlName()
        {
            return m_deviceSerial;
        }

        private void UpdateSocketsStatus()
        {
			byte bvalue = 0;
			MLog.Log(this, "Updating socket status");
			m_usb8Relay.GetPinStates(ref bvalue);
			//MLog.Log(this, "Updated socket status OK");
			m_socketsStatus = Convert.ToString(bvalue, 2);
			m_socketsStatus = m_socketsStatus.PadLeft(8, '0');
			m_socketsStatus = Utilities.ReverseString(m_socketsStatus);
        }

        public override void PowerOn(int zoneId)
        {
            if (!IsPowerOn(zoneId))
            {
				AutoResetEvent ev = new AutoResetEvent(false);
                Thread th = new Thread(() => PowerOnSync(zoneId, ev));
                th.Name = "PowerOn zoneid=" + zoneId;
                th.Start();

				if (!ev.WaitOne(3000))
				{
					Alert.CreateAlert("Denkovi Running async power on ERROR");
					th.Abort();
					//Reinitialise();
				}
            }
        }

        private bool PowerOnSync(int zoneId, AutoResetEvent ev)
        {
			lock (m_usb8Relay)
			{
				MLog.Log(this, "Power ON started zoneid = " + zoneId + " Index count=" + GetSocketIndexForZone(zoneId).Length);
				Open();
				String status = SendPowerCommand(zoneId, "1").ToString();
				LogDebugInfo();
				Close();
				MLog.Log(this, "Power ON completed status = " + status + " state=" + m_relayState);
				ev.Set();
			}
            return true;
        }

        public override void PowerOff()
        {
            AutoResetEvent ev = new AutoResetEvent(false);
			Thread th = new Thread(() => PowerOffSync(ev));
            th.Name = "PowerOff zoneid=all";
            
            th.Start();
            if (!ev.WaitOne(3000))
			{
				th.Abort();
				Alert.CreateAlert("Denkovi Running async power off ALL ERROR", null, false);
				//Reinitialise();
			};
        }

        public override void PowerOff(int zoneId)
        {
            //making an async call
            if (IsPowerOn(zoneId))
            {
				AutoResetEvent ev = new AutoResetEvent(false);
                Thread th = new Thread(() => PowerOffSync(zoneId, ev));
                th.Name = "PowerOff zoneid="+zoneId;
                //m_threadList.Add(th);
                th.Start();
				if (!ev.WaitOne(3000))
				{
					Alert.CreateAlert("Running async power off ERROR");
					th.Abort();
					//Reinitialise();
				}
            }
        }

        private bool PowerOffSync(int zoneId, AutoResetEvent ev)
        {
			lock (m_usb8Relay)
			{
				MLog.Log(this, "Power OFF started zoneid = " + zoneId + " Index=" + GetSocketIndexForZone(zoneId));
				Open();
				String status = SendPowerCommand(zoneId, "0").ToString();
				LogDebugInfo();
				Close();
				MLog.Log(this, "Power OFF completed status = " + status + " state=" + m_relayState);
				ev.Set();
			}
            return true;
        }

        private bool PowerOffSync(AutoResetEvent ev)
        {

			lock (m_usb8Relay)
			{
				MLog.Log(this, "Power OFF All started");
				Open();
				String status = SendPowerCommand(STATE_ALL_OFF).ToString();
				LogDebugInfo();
				Close();
				MLog.Log(this, "Power OFF ALL completed status = " + status + " state=" + m_relayState);
				ev.Set();
			}
            return true;
        }

        private FTD2XX_NET.FTDI.FT_STATUS SendPowerCommand(int zoneId, String state)
        {
            int zoneIndex;
            String initialState;
            String stateToSend;
            FTD2XX_NET.FTDI.FT_STATUS status = FTD2XX_NET.FTDI.FT_STATUS.FT_OTHER_ERROR;

            int[] index = GetSocketIndexForZone(zoneId);

			
            for (int j = 0; j < index.Length; j++)
            {
                stateToSend = "";
                initialState = m_relayState;
                zoneIndex = index[j];
                // checking if this zone is still active, if active and bool param is false will ignore
                //TODO

                MLog.Log(this,"Sending power command "+state+" to zoneindex " + zoneIndex);
                initialState = initialState.Substring(0, zoneIndex - 1) + state + initialState.Substring(zoneIndex);
                m_relayState = initialState;

                for (int i = m_relayState.Length; i > 0; i--)
                    stateToSend += initialState[i - 1];
                status = SendPowerCommand(stateToSend);
                if (status != FTD2XX_NET.FTDI.FT_STATUS.FT_OK)
                {
                    MLog.Log(this,"Error send command " +state+ ", status=" + status);
                    if (status == FTD2XX_NET.FTDI.FT_STATUS.FT_IO_ERROR)
                    {
                        //MLog.Log(null,"Reseting device started");
                        //status = m_usb8Relay.ResetDevice();
                        //MLog.Log(null,"Reseting device completed, status=" + status);
                    }
                }
            }
            return status;
        }

        private void LogDebugInfo()
        {
            //String value = "";
            //byte bvalue = 0;
            //uint uvalue = 0;
            //String debug = "Open=" + m_usb8Relay.IsOpen;
            /*
            m_usb8Relay.GetCOMPort(out value);
            debug += "|ComPort=" + value;
            m_usb8Relay.GetDescription(out value);
            debug += "|Description=" + value;
            m_usb8Relay.GetLineStatus(ref bvalue);
            debug += "|LineStatus=" + bvalue;
            m_usb8Relay.GetPinStates(ref bvalue);
            debug += "|PinStates=" + bvalue;
            m_usb8Relay.GetRxBytesAvailable(ref uvalue);
            debug += "|RXBytesAvail=" + uvalue;
            m_usb8Relay.GetTxBytesWaiting(ref uvalue);
            debug += "|TXBytesAvail=" + uvalue;
             * */
            //MLog.Log(null,debug);
            //Application.DoEvents();
        }

        private FTD2XX_NET.FTDI.FT_STATUS SendPowerCommand(String stateToSend)
        {
            byte value;
            uint numBytesWritten = 0;
            byte[] buffer = new byte[1];
            FTD2XX_NET.FTDI.FT_STATUS status = FTD2XX_NET.FTDI.FT_STATUS.FT_OTHER_ERROR;

			Thread.Sleep(100);
			try
			{
				value = Convert.ToByte(stateToSend, 2);
				buffer[0] = value;
				status = m_usb8Relay.Write(buffer, 1, ref numBytesWritten);
				if (status != FTD2XX_NET.FTDI.FT_STATUS.FT_OK)
					MLog.Log(this, "SendPowercmd failed status=" + status);
				Thread.Sleep(100);
				UpdateSocketsStatus();
			}
			catch (Exception ex)
			{
				MLog.Log(ex, this, "Unable to change power state " + stateToSend + " ex=" + ex.Message);
			}
			
            return status;
        }


        private int[] GetSocketIndexForZone(int zoneId)
        {
            //int[] result;
            List<int> res=new List<int>();
            
            foreach (ZoneDetails zone in ZoneDetails.ZoneDetailsList)
            {
                if (zone.ZoneId == zoneId && zone.PowerIndex>0)
                    res.Add(zone.PowerIndex);
            }
            return res.ToArray();
        }
    }

	public class NumatoLPTControl : BasePowerControl
	{
		private LPT m_lptDevice;

		public NumatoLPTControl(short portAddress):base("NumatoLPT")
		{
			m_lptDevice = new LPT(portAddress);
		}

		~NumatoLPTControl()
        {
            PowerOff();
        }

		protected override void Open()
		{
			//throw new NotImplementedException();
		}

		protected override void Close()
		{
			//throw new NotImplementedException();
		}

		public override bool IsPowerControlOn()
		{
			return m_lptDevice != null;
		}

		public override void PowerOn(int zoneId)
		{
			m_lptDevice.WritePort((short)ZoneDetails.GetZoneById(zoneId).PowerIndex, true);
		}

		public override void PowerOff(int zoneId)
		{
			m_lptDevice.WritePort((short)ZoneDetails.GetZoneById(zoneId).PowerIndex, false);
		}

		public override void PowerOff()
		{
			m_lptDevice.WritePort(0);
		}

		public override bool IsPowerOn(int zoneId)
		{
			return m_lptDevice.IsPowerOn((short)ZoneDetails.GetZoneById(zoneId).PowerIndex);
		}
	}

    class  GembirdPowerControl: BasePowerControl
    {
        public GembirdPowerControl(String deviceName)
            : base(deviceName)
        {
        }

        public override void PowerOn(int zoneId)
        {
            if (!ControlCenter.IsPowerControlEnabled)
                return;

            int zoneIndex = Convert.ToInt16(zoneId) - 1;

            for (int i = 0; i < 2; i++)
            {
                MLog.Log(null,"Power resume action initiated zone " + zoneId);
                String powerOnCommand = IniFile.LoadIniEntryByKey(IniFile.INI_SECTION_ZONEPOWERONCTRL_GEMBIRD, zoneId.ToString());
				Utilities.RunProcessWait(powerOnCommand, System.Diagnostics.ProcessWindowStyle.Minimized, System.Diagnostics.ProcessPriorityClass.BelowNormal);
                MLog.Log(null,"Power resume action completed zone " + zoneId);

                //wait for socket to go on
                System.Threading.Thread.Sleep(2000);

                if (IsSocketOn(/*IniFile.PARAM_POWER_CONTROL_DEVICE_NAME[1], */zoneIndex.ToString()))
                {
                    MLog.Log(this,"Power resume action OK, socket is on zone " + zoneId);
                    break;
                }
                //something went wrong
                MLog.Log(this,"Power resume action did not worked, retrying for zone " + zoneId);
                Close();
                Open();
            }
        }

        public override void PowerOff()
        {
            Close();
        }

        public override void PowerOff(int zoneId)
        {
            if (!ControlCenter.IsPowerControlEnabled)
                return;

            int zoneIndex = Convert.ToInt16(zoneId) - 1;

            for (int i = 0; i < 2; i++)
            {
                MLog.Log(null,"Power save action initiated zone " + zoneId);
                String powerOffCommand = IniFile.LoadIniEntryByKey(IniFile.INI_SECTION_ZONEPOWEROFFCTRL_GEMBIRD, zoneId.ToString());
				Utilities.RunProcessWait(powerOffCommand, System.Diagnostics.ProcessWindowStyle.Minimized, System.Diagnostics.ProcessPriorityClass.BelowNormal);
                MLog.Log(null,"Power save action completed zone " + zoneId);

                //wait for socket to go off
                System.Threading.Thread.Sleep(2000);

                if (IsSocketOff(IniFile.PARAM_POWER_CONTROL_DEVICE_NAME_GEMBIRD[1], zoneIndex.ToString()))
                {
                    MLog.Log(null,"Power save action OK, socket off on zone " + zoneId);
                    break;
                }
                //something went wrong
                MLog.Log(null,"Power save action did not worked, retrying for zone " + zoneId);
                Close();
                Open();
            }
        }

        protected bool IsSocketOn(String socketIndex)
        {
            return (GetSocketStatus(socketIndex) == "TRUE" ? true : false);
        }

        private  bool IsSocketOff(String powerDeviceName, String socketIndex)
        {
            return (GetSocketStatus(socketIndex) == "FALSE" ? true : false);
        }

        public override bool IsPowerOn(int zoneId)
        {
            throw new NotImplementedException();
        }

        protected String GetSocketStatus(String socketIndex)
        {
            if (!IsPowerControlOn())
                Open();

            Utilities.MoveFile(IniFile.PARAM_POWER_CONTROL_STATUS_FILE_GEMBIRD[1], IniFile.PARAM_POWER_CONTROL_STATUS_FILE_GEMBIRD[1] + ".old", true);
			Utilities.RunProcessWait(MZPState.Instance.powerControlStatusCommand, System.Diagnostics.ProcessWindowStyle.Minimized, System.Diagnostics.ProcessPriorityClass.BelowNormal);

            int i = 0;
            while (true)
            {
                if (File.Exists(IniFile.PARAM_POWER_CONTROL_STATUS_FILE_GEMBIRD[1]))
                {
                    if (new FileInfo(IniFile.PARAM_POWER_CONTROL_STATUS_FILE_GEMBIRD[1]).Length > 0)
                        break;
                }
                System.Threading.Thread.Sleep(500);
                i++;
                if (i<=10) break;
            }

            for (i = 0; i < 10; i++)
            {
                String deviceNameFile = Utilities.IniReadValue("Device #" + i, "DeviceName",IniFile.PARAM_POWER_CONTROL_STATUS_FILE_GEMBIRD[1]);
                if (m_deviceName.Equals(deviceNameFile)) break;
            }

            String status = Utilities.IniReadValue("Device #" + i, "Socket" + socketIndex + "SwitchState", 
                    IniFile.PARAM_POWER_CONTROL_STATUS_FILE_GEMBIRD[1]);
            MLog.Log(null,"Status for devicename=" +m_deviceName+" deviceindex=" + i + " socketindex=" + socketIndex + " status=" + status);
            return status;
        }

        protected override void Close()
        {
            if (Utilities.IsProcAlive(IniFile.PARAM_POWER_CONTROL_APP_PROCESSNAME[1]))
                Utilities.CloseProcSync(IniFile.PARAM_POWER_CONTROL_APP_PROCESSNAME[1]);
        }

        protected override void Open()
        {
			Utilities.RunProcessWait(IniFile.PARAM_POWER_CONTROL_APP_PATH_GEMBIRD[1], System.Diagnostics.ProcessWindowStyle.Minimized, System.Diagnostics.ProcessPriorityClass.BelowNormal);
        }

        public override bool IsPowerControlOn()
        {
            return Utilities.IsProcAlive(IniFile.PARAM_POWER_CONTROL_APP_PROCESSNAME[1]);
        }

        public override String GetPowerControlName()
        {
            return IniFile.PARAM_POWER_CONTROL_APP_PROCESSNAME[1];
        }
    }

	public class RemotePowerControl
	{
		private static IDevice device;
		public static string SwitchOff(int remoteid)
		{
			string result = "SwitchOff OK";
			lock (device)
			{
				device = new Device();
				try
				{
					device.Open();
					device.SwitchOff(remoteid, CommandStyle.Comprehensive);
				}
				catch (Exception ex)
				{
					result = "Eror SwitchOff " + ex.Message;
					MLog.Log(ex, result);
					device.Close();
				}
			}
			return result;
		}

		public static string SwitchOn(int remoteid)
		{
			string result = "SwitchOn OK";
			lock (device)
			{
				device = new Device();
				try
				{
					device.Open();
					device.SwitchOn(remoteid, CommandStyle.Comprehensive);
				}
				catch (Exception ex)
				{
					result = "Error SwitchOn " + ex.Message;
					MLog.Log(ex, result);
					device.Close();
				}
			}
			return result;
		}

		public static string AdjustDim(int remoteid, int dimamount)
		{
			string result = "AdjustDim OK";
			lock (device)
			{
				device = new Device();
				try
				{
					device.Open();
					device.AdjustDim(remoteid, CommandStyle.Comprehensive, dimamount);
				}
				catch (Exception ex)
				{
					result = "Eror AdjustDim " + ex.Message;
					MLog.Log(ex, result);
					device.Close();
				}
			}
			return result;
		}
	}
}

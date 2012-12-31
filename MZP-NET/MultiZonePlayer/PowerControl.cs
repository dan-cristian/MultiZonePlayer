using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace MultiZonePlayer
{
    public abstract class BasePowerControl:IStorageOnIni
    {
        protected static int powerSavingCycleCount = 0;
        protected static bool isPowerSavingMode = false;
        protected String m_deviceName;
        protected String m_socketsStatus="";

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

            MLog.Log(null,"Power saving");
            //tell windows it can go into idle
            Utilities.SetThreadExecutionState(Utilities.EXECUTION_STATE.ES_CONTINUOUS);
            //run external power save actions
            MLog.Log(null,"Wait for power saving action");
            Utilities.RunProcessWaitExit(IniFile.CurrentPath() + IniFile.PARAM_POWER_SAVING_APP[1]);
            MLog.Log(null,"Wait completed for power saving action");

            isPowerSavingMode = true;
        }

        public void ResumeFromPowerSaving()
        {
            if (!ControlCenter.IsPowerControlEnabled)
                return;

            if (!isPowerSavingMode)
                return;
            MLog.Log(null,"Power resume");

            isPowerSavingMode = false;
            //tell windows it cannot go into idle
            Utilities.SetThreadExecutionState(Utilities.EXECUTION_STATE.ES_SYSTEM_REQUIRED);

            Utilities.RunProcessWaitExit(IniFile.CurrentPath() + IniFile.PARAM_POWER_RESUME_APP[1]);

            //resume from power saving
            powerSavingCycleCount = 0;
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
        public abstract String GetPowerControlName();
        public abstract void PowerOn(int zoneId);
        public abstract void PowerOff(int zoneId);
        public abstract void PowerOff();
        public abstract bool IsPowerOn(int zoneId);
        //protected abstract String GetSocketStatus(String zoneId);
        public abstract void LoadFromIni();
        public abstract void SaveToIni();
    }

    class DenkoviPowerControl : BasePowerControl
    {
        private FTD2XX_NET.FTDI m_usb8Relay;
        private static String USB8_IDENTIFIER = "FT245R USB FIFO";
        private String m_deviceSerial="";
        private const String STATE_ALL_ON  = "11111111";
        private const String STATE_ALL_OFF = "00000000";
        private String m_relayState = STATE_ALL_OFF;
        //private Hashtable m_zoneIndexes = null;
        private DateTime m_lastOpenDateTime;
        private static List<Thread> m_threadList;

        public DenkoviPowerControl(String deviceName):base(deviceName)
        {
            LoadFromIni();
            m_usb8Relay = new FTD2XX_NET.FTDI();
            ScanForDevices();
            m_lastOpenDateTime = DateTime.MinValue;
            m_threadList = new List<Thread>();
            PowerOff();
        }

        ~DenkoviPowerControl()
        {
            PowerOff();
        }

        private void ScanForDevices()
        {
            uint count=0;
            m_usb8Relay.GetNumberOfDevices(ref count);
            FTD2XX_NET.FTDI.FT_DEVICE_INFO_NODE[] deviceNode = new FTD2XX_NET.FTDI.FT_DEVICE_INFO_NODE[count];
            m_usb8Relay.GetDeviceList(deviceNode);

            for (int i=0; i<count; i++)
            {
                MLog.Log(null,
                    "desc="+deviceNode[i].Description
                    +" id="+deviceNode[i].ID
                    +" serial="+deviceNode[i].SerialNumber
                    +" type="+deviceNode[i].Type
                    +" locid="+deviceNode[i].LocId);
                if (deviceNode[i].Description.Equals(USB8_IDENTIFIER))
                {
                    m_deviceSerial = deviceNode[i].SerialNumber;
                }
            }

            if (m_deviceSerial == "")
                MLog.Log(null,"No " + USB8_IDENTIFIER + " found in " + count + " devices");
        }

        private void ResetBoard()
        {
            MZPState.Instance.LogEvent(MZPEvent.EventSource.System, "Relay Action freezed, recovery needed", MZPEvent.EventType.Functionality, MZPEvent.EventImportance.Error, null);
            MZPState.RestartComputer("power control freezed");
            /*
            MLog.Log(this,"Reseting board");
            //MLog.Log(this, "Reseting port");
            m_usb8Relay.ResetPort();
            //MLog.Log(this, "Reseting device");
            m_usb8Relay.ResetDevice();
            //MLog.Log(this, "Cycling port");
            m_usb8Relay.CyclePort();
            MLog.Log(this,"Reseting board done");
             */
        }

        protected override void Open()
        {
            FTD2XX_NET.FTDI.FT_STATUS status;
            try{
                status = m_usb8Relay.OpenBySerialNumber(m_deviceSerial);
                if (status == FTD2XX_NET.FTDI.FT_STATUS.FT_OK)
                {
                    m_lastOpenDateTime = DateTime.Now;
                    status = m_usb8Relay.SetBitMode(255, 1);
                    status = m_usb8Relay.SetTimeouts(1000, 1000);
                    if (status != FTD2XX_NET.FTDI.FT_STATUS.FT_OK)
                        MLog.Log(null, "Setbit failed on Open, status = " + status);
                }
                else
                    MLog.Log(null,"Open failed, status = " + status);
                LogDebugInfo();
            }
            catch (Exception ex)
            {
                MLog.Log(null,"Unable to open device " + m_deviceName + " id="+ m_deviceSerial + " err=" + ex.Message);
                Close();
            }
        }

        protected override void Close()
        {
            FTD2XX_NET.FTDI.FT_STATUS status;

            try
            {
                //MLog.Log(null,"Closing relay, reading all data");
                ReadAllData();
                //MLog.Log(null,"Closing relay, purge rx");
                m_usb8Relay.Purge(FTD2XX_NET.FTDI.FT_PURGE.FT_PURGE_RX);
                //MLog.Log(null,"Closing relay, purge tx");
                m_usb8Relay.Purge(FTD2XX_NET.FTDI.FT_PURGE.FT_PURGE_TX);
                //MLog.Log(null,"Now closing");
                status = m_usb8Relay.Close();
                //MLog.Log(null,"Now closed");
                if (status == FTD2XX_NET.FTDI.FT_STATUS.FT_OK)
                {
                    SaveToIni();
                    //MLog.Log(null,"Close OK device serial= " + m_deviceSerial);
                }
                else
                    MLog.Log(null,"Close failed, status = " + status);
            }
                catch (Exception ex)
                {
                    MLog.Log(null,"Unable to close device " + m_deviceName + " id="+ m_deviceSerial + " err=" + ex.Message);
                }
        }

        public override bool IsPowerControlOn()
        {
            return m_usb8Relay.IsOpen;
        }

        public override bool IsPowerOn(int zoneId)
        {
            int[] index = GetSocketIndexForZone(zoneId);
            //MLog.Log(this, "index len="+index.Length);
            
            for (int r = 0; r < index.Length; r++)
            {
                //MLog.Log(this, "check r=" + r + "=" + m_socketsStatus[index[r] - 1]);
                if (m_socketsStatus[index[r]-1] == '0')
                    return false;
            }
            return true;
        }

        private void ReadAllData()
        {
            FTD2XX_NET.FTDI.FT_STATUS status;
            uint uvalue = 0;
            //MLog.Log(null,"Reading bytes avail");
            status = m_usb8Relay.GetRxBytesAvailable(ref uvalue);
            if (status == FTD2XX_NET.FTDI.FT_STATUS.FT_OK)
            {
                if ((uvalue > 0))
                {
                    //MLog.Log(null,"RX buffer has data, bytecount=" + uvalue);
                    byte[] dataBuffer = new byte[uvalue];
                    uint numRead = 0;
                    status = m_usb8Relay.Read(dataBuffer, uvalue, ref numRead);
                    if (status == FTD2XX_NET.FTDI.FT_STATUS.FT_OK)
                        MLog.Log(null,"Read data numread=" + numRead + " status=" + status);
                    else
                        MLog.Log(null,"Read failed, status = " + status);
                }
            }
            else
                MLog.Log(null,"GetRXBytes failed, status = " + status);
        }

        public override String GetPowerControlName()
        {
            return m_deviceSerial;
        }

        private void UpdateSocketsStatus()
        {
            byte bvalue =0;
            m_usb8Relay.GetPinStates(ref bvalue);
            m_socketsStatus = Convert.ToString(bvalue, 2);
            m_socketsStatus = m_socketsStatus.PadLeft(8, '0');
            m_socketsStatus = Utilities.ReverseString(m_socketsStatus);

            //MLog.Log(this, "SocketsOnStatus=" + m_socketsStatus);
        }

        public override void PowerOn(int zoneId)
        {
            //MLog.Log(null,"Running async power on, active thread count=" + m_threadList.Count);
            //making an async call
            if (!IsPowerOn(zoneId))
            {
                Thread th = new Thread(() => PowerOnSync(zoneId));
                th.Name = "PowerOn zoneid=" + zoneId;
                m_threadList.Add(th);
                th.Start();
                WaitForThreadEnd(th);
                MLog.Log(null, "Running async power on completed, active thread count=" + m_threadList.Count);
            }
            else
                MLog.Log(this, "Power already on, power on ignored zoneid=" + zoneId 
                    + " status=" + m_socketsStatus + " socketindex=" + GetSocketIndexForZone(zoneId));
        }

        private bool PowerOnSync(int zoneId)
        {
            MLog.Log(null,"Power ON started zoneid = " + zoneId +" Index count="+ GetSocketIndexForZone(zoneId).Length);
            Open();
            String status = SendPowerCommand(zoneId, "1").ToString();
            LogDebugInfo();
            Close();
            //MLog.Log(null,"Power ON completed status = " + status + " state=" + m_relayState);
            return true;
        }

        public override void PowerOff()
        {
            //MLog.Log(null,"Running async power off ALL, active thread count=" + m_threadList.Count);
            //making an async call
            Thread th = new Thread(() => PowerOffSync());
            th.Name = "PowerOff zoneid=all";
            m_threadList.Add(th);
            th.Start();
            WaitForThreadEnd(th);
            MLog.Log(null,"Running async power off ALL completed, active thread count=" + m_threadList.Count);
        }

        public override void PowerOff(int zoneId)
        {
            //MLog.Log(null,"Running async power off, active thread count=" + m_threadList.Count);
            //making an async call
            if (IsPowerOn(zoneId))
            {
                Thread th = new Thread(() => PowerOffSync(zoneId));
                th.Name = "PowerOff zoneid="+zoneId;
                m_threadList.Add(th);
                th.Start();
                WaitForThreadEnd(th);
                MLog.Log(null, "Running async power off completed, active thread count=" + m_threadList.Count);
            }
            //else MLog.Log(this, "Power already off, off not needed zone " + zoneId);
        }

        private bool PowerOffSync(int zoneId)
        {

            MLog.Log(null,"Power OFF started zoneid = " + zoneId + " Index=" + GetSocketIndexForZone(zoneId));
            Open();
            String status = SendPowerCommand(zoneId, "0").ToString();
            LogDebugInfo();
            Close();
            //MLog.Log(null,"Power OFF completed status = " + status + " state=" + m_relayState);
            return true;
        }

        private bool PowerOffSync()
        {

            MLog.Log(this,"Power OFF All started");
            //if (!m_relayState.Equals(STATE_ALL_OFF))
            //{
                Open();
                String status = SendPowerCommand(STATE_ALL_OFF).ToString();
                LogDebugInfo();
                Close();
                MLog.Log(this, "Power OFF ALL completed status = " + status + " state=" + m_relayState);
            //}
            //else
            //    MLog.Log(this, "Power OFF not needed as status is = " + m_relayState);
            return true;
        }

        private bool WaitForThreadEnd(Thread th)
        {
            bool done=false;
            int i = 0;
            for (i = 0; i < 5000; i++)
            {
                Application.DoEvents();//must have otherwise will not work
                Thread.Sleep(1);
                if (!th.IsAlive)
                {
                    //MLog.Log(null,"Thread " + th.Name + " state=" + th.ThreadState);
                    
                    done = true;
                    m_threadList.Remove(th);
                    //MLog.Log(null,th.Name + " thread is not alive");
                    break;
                }
            }
            if (!done)
            {
                MLog.Log(null,th.Name + " Action freezed, recovery needed, pas="+i);
                ResetBoard();
            }
            return done;
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

                MLog.Log(null,"Sending power command "+state+" to zoneindex " + zoneIndex);
                initialState = initialState.Substring(0, zoneIndex - 1) + state + initialState.Substring(zoneIndex);
                m_relayState = initialState;

                for (int i = m_relayState.Length; i > 0; i--)
                    stateToSend += initialState[i - 1];
                status = SendPowerCommand(stateToSend);
                if (status != FTD2XX_NET.FTDI.FT_STATUS.FT_OK)
                {
                    MLog.Log(null,"Error send command " +state+ ", status=" + status);
                    if (status == FTD2XX_NET.FTDI.FT_STATUS.FT_IO_ERROR)
                    {
                        //MLog.Log(null,"Reseting device started");
                        status = m_usb8Relay.ResetDevice();
                        //MLog.Log(null,"Reseting device completed, status=" + status);
                    }
                }
                Application.DoEvents();
            }

            UpdateSocketsStatus();
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
            
            try
            {
                //MLog.Log(null,"Sending power command " + stateToSend);
                value = Convert.ToByte(stateToSend, 2);
                buffer[0] = value;
                status = m_usb8Relay.Write(buffer, 1, ref numBytesWritten);
                if (status != FTD2XX_NET.FTDI.FT_STATUS.FT_OK)
                    MLog.Log(null,"SendPowercmd failed status="+status);
                UpdateSocketsStatus();
            }
            catch (Exception ex)
            {
                MLog.Log(null,"Unable to change power state " + stateToSend + " ex=" + ex.Message);
            }
            return status;
        }


        private int[] GetSocketIndexForZone(int zoneId)
        {
            //int[] result;
            List<int> res=new List<int>();
            /*
            if (m_zoneIndexes.ContainsKey(zoneId))
            {
                List<Object> value = Utilities.ParseStringForValues(m_zoneIndexes[zoneId].ToString(), ',', typeof(int));
                result = new int[value.Count];

                for (int i = 0; i < value.Count; i++)
                {
                    result[i] = (int)value[i];
                }
            }
            else
                result = new int[0];
            */
            foreach (Metadata.ZoneDetails zone in MZPState.Instance.ZoneDetails)
            {
                if (zone.ZoneId == zoneId && zone.PowerIndex>0)
                    res.Add(zone.PowerIndex);
            }
            return res.ToArray();
        }

        

        public override void LoadFromIni()
        {
            //m_zoneIndexes = IniFile.LoadAllIniEntriesByIntKey(IniFile.INI_SECTION_ZONEPOWERCTRLINDEX_DK);
        }

        public override void SaveToIni()
        {
            //not needed
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
                Utilities.RunProcessWaitExit(powerOnCommand);
                MLog.Log(null,"Power resume action completed zone " + zoneId);

                //wait for socket to go on
                System.Threading.Thread.Sleep(2000);

                if (IsSocketOn(/*IniFile.PARAM_POWER_CONTROL_DEVICE_NAME[1], */zoneIndex.ToString()))
                {
                    MLog.Log(null,"Power resume action OK, socket is on zone " + zoneId);
                    break;
                }
                //something went wrong
                MLog.Log(null,"Power resume action did not worked, retrying for zone " + zoneId);
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
                Utilities.RunProcessWaitExit(powerOffCommand);
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
            Utilities.RunProcessWaitExit(MZPState.Instance.powerControlStatusCommand);

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
            Utilities.RunProcessWait(IniFile.PARAM_POWER_CONTROL_APP_PATH_GEMBIRD[1]);
        }

        public override bool IsPowerControlOn()
        {
            return Utilities.IsProcAlive(IniFile.PARAM_POWER_CONTROL_APP_PROCESSNAME[1]);
        }

        public override String GetPowerControlName()
        {
            return IniFile.PARAM_POWER_CONTROL_APP_PROCESSNAME[1];
        }

        public override void LoadFromIni()
        {
            //not needed
        }

        public override void SaveToIni()
        {
            //not needed
        }
    }
}

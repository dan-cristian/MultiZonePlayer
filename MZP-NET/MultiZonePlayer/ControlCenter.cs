using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;


namespace MultiZonePlayer
{
    
    public  partial class ControlCenter : Form
    {
        public class ZonesFormPair
        {
            public int ZoneId;
            public ZonesForm ZonesForm;

            public ZonesFormPair(int zoneId, ZonesForm zone)
            {
                ZoneId = zoneId;
                ZonesForm = zone;
            }
        }

        static MainScreen parentForm = null;
        public static bool IsPowerControlEnabled = false;

        private RawInputDevice m_rawDeviceId;

        private static List<ZonesFormPair> m_zoneFormsList;
        private static Hashtable hsDelegatedZoneForms;
        private static ZonesForm m_currentZone = null;
        private static DriveDetector driveDetector = null;
        private static RichTextBox TXTLog,TXTLog2,TXTLog3;
        private static ControlCenter m_ctrlCenter;
        private DateTime m_lastSlowTickDateTime = DateTime.Now;
        
        public ControlCenter(MainScreen p_parentForm)
        {
            InitializeComponent();
            parentForm = p_parentForm;

            m_zoneFormsList = new List<ZonesFormPair>();
            hsDelegatedZoneForms = new Hashtable();
            
            foreach (Metadata.ZoneDetails zone in MZPState.Instance.ZoneDetails)
            {
                m_zoneFormsList.Add(new ZonesFormPair(zone.ZoneId, null));
            }
            
        }

        private void ControlCenter_Load(object sender, EventArgs e)
        {
            TXTLog = txtError;
            TXTLog2 = txtLog2;
            TXTLog3 = txtLog3;
            m_ctrlCenter = this;

            
            timerStatus.Interval = IniFile.ZONE_INACTIVITY_CYCLE_DURATION;
            timerStatus.Start();

            //RegisterDriveDetector(); UNUSED
            RegisterRawInput();
            //hsHookKeys = new Hashtable();
            
            MLog.Log(null,"Using Ini path " + IniFile.CurrentPath());
            Application.DoEvents();
            
        }

        public static ControlCenter Instance
        {
            get{return m_ctrlCenter;}
        }

        private void m_KeyPressed(object sender, ref RawInputDevice.KeyControlEventArgs e)
        {
            KeyDetail kd = new KeyDetail(e.Keyboard.vKey, e.Keyboard.deviceName);
            if (parentForm.m_formOptions != null && parentForm.m_formOptions.Visible)
            {
                parentForm.m_formOptions.m_KeyPressed(kd);
                return;
            }

            API.DoCommandFromRawInput(kd);
        }

        // required by RawInput, The WndProc is overridden to allow InputDevice to intercept
        // messages to the window and thus catch WM_INPUT messages
        protected override void WndProc(ref Message message)
        {
            if (m_rawDeviceId != null)
            {
                m_rawDeviceId.ProcessMessage(message);
            }
            base.WndProc(ref message);
        }


        private void RegisterRawInput()
        {
            // Create a new InputDevice object, get the number of
            // keyboards, and register the method which will handle the 
            // InputDevice KeyPressed event
            m_rawDeviceId = new RawInputDevice(Handle);
            m_rawDeviceId.KeyPressed += new RawInputDevice.DeviceEventHandler(m_KeyPressed);
        }


        public void RefreshState()
        {
            if (CurrentZone != null)
            {       
                cmbCurrentZone.Text = CurrentZone.GetZoneId().ToString();
            }
            else 
            {
                cmbCurrentZone.Text = "no zone";
            }
            if (MZPState.Instance != null)
            {
                txtInactivityCycles.Text = MZPState.Instance.PowerControl.GetInactivityCycles().ToString() + "/" + IniFile.POWERSAVING_MAX_CYCLES;
                txAlarm.Text = MZPState.Instance.SystemAlarm.IsMonitoringActive + ":" + MZPState.Instance.SystemAlarm.AreaState.ToString();
            }
            txtZoneCount.Text = m_zoneFormsList.Count.ToString();


            if (MediaLibrary.IsInitialised)
            {
                txAudioCount.Text = MediaLibrary.AllAudioFiles.PlaylistFiles.Count.ToString();
                txVideoCount.Text = MediaLibrary.AllVideoFiles.PlaylistFiles.Count.ToString();
                txAudioSave.Text = MediaLibrary.AllAudioFiles.PendingSaveItemsCount().ToString();
            }
            else
                txAudioCount.Text = "loading " + MediaLibrary.AllAudioFiles.PlaylistFiles.Count;
            ShowPowerControlStatus();
        }

        public void CloseAllZones()
        {
            try
            {
                foreach (ZonesFormPair zone in m_zoneFormsList)
                {
                    if (zone.ZonesForm!=null)
                        zone.ZonesForm.Close();
                }
            }
            catch (Exception ex)
            {
                MLog.Log(ex, "Error close all zones");
            }
        }

        public void ZoneClosing(int zoneId)
        {
            //MLog.Log(null,"Closing zone " + zoneId);
            try
            {
                m_currentZone = null;
                ZonesFormPair zone = m_zoneFormsList.Find(item => item.ZoneId == zoneId);
                zone.ZonesForm = null;
                Metadata.ZoneDetails zonedet = MZPState.Instance.ZoneDetails.Find(item => item.ZoneId == zoneId);
                zonedet.Close();
            }
            catch (Exception ex)
            {
                MLog.Log(ex, "Error closing zone " + zoneId);
            }
        }

        public ZonesForm CurrentZone
        {
            get
            {
                ZonesForm zone = null; ;
                if (m_currentZone != null)
                    zone = m_currentZone;
                else
                {
                    //set first zone
                    if ((m_zoneFormsList != null) && (m_zoneFormsList.Count > 0))
                    {
                        zone = m_zoneFormsList[0].ZonesForm;
                    }
                }
                return zone;
            }
        }

        public int CurrentZoneId
        {
            get
            {
                if (CurrentZone != null)
                    return CurrentZone.GetZoneId();
                else
                    if (m_zoneFormsList.Count > 0)
                        return m_zoneFormsList[0].ZoneId;
                    else
                        return -1;
            }
            set
            {
                m_currentZone = m_zoneFormsList.Find(item => item.ZoneId==value).ZonesForm;
            }
        }

        public int CurrentZoneMusicId
        {
            get
            {
                Metadata.ZoneDetails zone = MZPState.Instance.ZoneDetails.OrderByDescending(x=>x.LastLocalCommandDateTime).ToList().Find(x => 
                    x.ActivityType.Equals(Metadata.GlobalCommands.music) && x.IsActive);
                if (zone != null) 
                    return zone.ZoneId;
                else
                    return -1;
            }
        }

        public int CurrentZoneStreamId
        {
            get
            {
                Metadata.ZoneDetails zone = MZPState.Instance.ZoneDetails.OrderByDescending(x => x.LastLocalCommandDateTime).ToList().Find(x =>
                    x.ActivityType.Equals(Metadata.GlobalCommands.streammp3) && x.IsActive);
                if (zone != null)
                    return zone.ZoneId;
                else
                    return -1;
            }
        }

        /*private void SelectZone(int zoneId)
        {
            int sourceZoneId=zoneId;
            MLog.Log(this, "Selecting zone " + zoneId);
            //check for delegated control
            if (hsDelegatedZoneForms.ContainsKey(sourceZoneId))
            {
                zoneId = (int)hsDelegatedZoneForms[sourceZoneId];
                MLog.Log(null,"Delegated zone selected for source zone " + sourceZoneId + " delegateid=" + zoneId);
            }

            if (!IsZoneActive(zoneId))
            {
                MLog.Log(null,"Zone " + zoneId + " not active, opening");
                OpenZone(zoneId, null);
            }
            else
            {
                m_currentZone = GetZone(zoneId);
                //MLog.Log(null,"Zone  " + zoneId + " is active");
            }

            if (m_currentZone != null)
                m_currentZone.SetSourceZone(sourceZoneId);
            else
                MLog.Log(null,"Current Zone  is null, problem");
        }*/

        public void OpenZone(int zoneId, String cmd)
        {
            //MLog.Log(this, "Opening zone " + zoneId + " for cmd="+cmd);
            
            Metadata.ZoneDetails zDetails = MZPState.Instance.ZoneDetails.Find(item => item.ZoneId == zoneId);

            if (zDetails != null)
            {
                ZonesForm newZoneForm = new ZonesForm(zoneId, this, zDetails);
                if (cmd != null)
                    newZoneForm.SetActivity(cmd);

                m_zoneFormsList.Find(item => item.ZoneId == zoneId).ZonesForm = newZoneForm;
                //parentForm.LayoutMdi(MdiLayout.Cascade);
                m_currentZone = GetZone(zoneId);
                zDetails.IsActive = true;
                newZoneForm.MdiParent = parentForm;
                newZoneForm.WindowState = FormWindowState.Minimized;
                newZoneForm.Text = "Zone " + zoneId;
                newZoneForm.Show();
            }
            else
                MLog.Log(this, "Unable to open zone " + zoneId);
        }

        public static int GetActiveZonesCount()
        {
            return m_zoneFormsList.Count;
        }

        public ZonesForm GetZone(int zoneId)
        {
            ZonesFormPair zp = m_zoneFormsList.Find(item => item.ZoneId == zoneId);
            if (zp != null)
                return zp.ZonesForm;
            else
                return null;
        }

        public  bool IsZoneActive(int zoneId)
        {
            Metadata.ZoneDetails zone = MZPState.Instance.ZoneDetails.Find(item => item.ZoneId==zoneId);
            if (zone != null) 
                return zone.IsActive;
            else return false;;
        }

        public static ZonesForm GetFirstZoneMusic()
        {
            foreach (ZonesFormPair zone in m_zoneFormsList)
            {
                if (zone.ZonesForm != null)
                {
                    if ((zone.ZonesForm.GetCurrentActivity() != null) && (zone.ZonesForm.GetCurrentActivity().GetType() == typeof(ZoneMusic)))
                        return zone.ZonesForm;
                }
            }
            return null;
        }

        

        
        #region USBEvents
        private void RegisterDriveDetector()
        {
            driveDetector = new DriveDetector();
            driveDetector.DeviceArrived += new DriveDetectorEventHandler(OnDriveArrived);
            driveDetector.DeviceRemoved += new DriveDetectorEventHandler(OnDriveRemoved);
            driveDetector.QueryRemove += new DriveDetectorEventHandler(OnQueryRemove);
        }

        // Called by DriveDetector when removable device in inserted 
        private void OnDriveArrived(object sender, DriveDetectorEventArgs e)
        {
            try
            {
                // Report the event in the listbox.
                // e.Drive is the drive letter for the device which just arrived, e.g. "E:\\"
                MLog.Log(this,"Drive arrived " + e.Drive);

                //DirectoryPlaylist dp = new DirectoryPlaylist(e.Drive, "*.mp3", System.IO.SearchOption.AllDirectories);
                // If you want to be notified when drive is being removed (and be able to cancel it), 
                // set HookQueryRemove to true 
                //if (checkBoxAskMe.Checked)
                e.HookQueryRemove = true;
            }
            catch (Exception ex)
            {
                MLog.Log(ex,"Drive arrived exception ");
            }
        }

        // Called by DriveDetector after removable device has been unpluged 
        private void OnDriveRemoved(object sender, DriveDetectorEventArgs e)
        {
            // TODO: do clean up here, etc. Letter of the removed drive is in e.Drive;

            // Just add report to the listbox
            string s = "Drive removed " + e.Drive;
            MLog.Log(this,s);
        }

        // Called by DriveDetector when removable drive is about to be removed
        private void OnQueryRemove(object sender, DriveDetectorEventArgs e)
        {
            // Should we allow the drive to be unplugged?
            //if (checkBoxAskMe.Checked)
            {
                if (MessageBox.Show("Allow remove?", "Query remove",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    e.Cancel = false;       // Allow removal
                else
                    e.Cancel = true;        // Cancel the removal of the device  
            }
        }
        #endregion

        #region Misc
        

        
        private void ErrorInvalidCommand(RemotePipiCommand cmd, KeyEventArgs e)
        {
            //PlayErrorMessage("This is an Invalid command: " + cmd + " code=" + RemotePipi.GetCodeFriendlyName(e));
        }

        public static void PlayErrorMessage(String errMsg, ZonesForm zone)
        {
            MLog.Log(null,"PlayErrMessage: " + errMsg);
            //Text2Speech.PlayError(errMsg, zone);
        }

        public static void PlayInfoMessage(String infoMsg, ZonesForm zone)
        {
            MLog.Log(null,"PlayInfoMessage: " + infoMsg);
            //Text2Speech.PlayMessage(infoMsg, zone);
        }

        public static void PlayInfoMessageClean(String infoMsg, ZonesForm zone)
        {
            MLog.Log(null,"PlayInfoMessageClean: " + infoMsg);
            //Text2Speech.PlayMessageClean(infoMsg, zone);
        }
        /*
        public static void PlayBeepOK()
        {
            if (m_currentZone != null)
            {
                m_currentZone.PlayBeepOK();
            }
            else
            {
                MLog.Log(null,"No current zone available to play beep OK on key pressed");
            }
        }
        public static void PlayBeepError()
        {
            if (m_currentZone != null)
            {
                m_currentZone.PlayBeepError();
            }
            else
            {
                MLog.Log(null,"No current zone available to play beep Error on key pressed");
            }
        }
         * */
        #endregion

        #region FormEvents
        private void ControlCenter_Shown(object sender, EventArgs e)
        {
            txtCommand.Focus();
        }

        private void timerStatus_Tick(object sender, EventArgs e)
        {
            try
            {
                
                    Thread.Sleep(200);
                    Application.DoEvents();
                
                

                // FAST TICK
                foreach (ZonesFormPair zone in m_zoneFormsList)
                {
                    if (zone.ZonesForm != null)
                        zone.ZonesForm.Tick(sender, e);
                }

                // SLOW TICK
                if (DateTime.Now.Subtract(m_lastSlowTickDateTime).Duration().TotalSeconds > 30)
                {
                    m_lastSlowTickDateTime = DateTime.Now;
                    RefreshState();
                    MZPState.Instance.Tick();
                }
            }
            catch (Exception ex)
            {
                MLog.Log(ex, this, "Error timer");
            }
        }

    
        private void txtCommand_Leave(object sender, EventArgs e)
        {
            //if ((parentForm.m_formOptions==null) || (!parentForm.m_formOptions.Visible))
            //    txtCommand.Focus();
        }

        

        

        private void ShowPowerControlStatus()
        {
            
            bool alive = MZPState.Instance.PowerControl.IsPowerControlOn();
            txtPwrControl.Text = alive.ToString();
            txPwrName.Text = MZPState.Instance.PowerControl.GetPowerControlName();
            txtPowerSaving.Text = MZPState.Instance.PowerControl.IsPowerSavingMode().ToString();
            for (int i = 0; i < 10; i++)
            {
                if ((Button)tbButtons.Controls["bt" + i] != null)
                    ((Button)tbButtons.Controls["bt" + i]).FlatStyle = (MZPState.Instance.PowerControl.SocketsOn(i) ? FlatStyle.Flat : FlatStyle.Standard);
            }
            txSocketsStatus.Text = MZPState.Instance.PowerControl.SocketsStatus;
            
        }

        #endregion

        private void txtLog2_DoubleClick(object sender, EventArgs e)
        {
            txtLog2.Clear();
        }

        private void txtLog2_Click(object sender, EventArgs e)
        {
            txtLog2.AppendText("\n");
        }

        private void bt1_Click(object sender, EventArgs e)
        {
            String result = API.DoCommandFromGUIInput(Metadata.GlobalCommands.selectzone.ToString(), ((Button)sender).Text);
        }

        private void btCommand_Click(object sender, EventArgs e)
        {
            if (m_currentZone != null)
                API.DoCommandFromGUIInput(((Button)sender).Tag.ToString(), m_currentZone.GetZoneId().ToString());
            else
                MLog.Log(null,"No zone selected for API Command click");
        }

        private void chkPowerControl_CheckedChanged(object sender, EventArgs e)
        {
            IsPowerControlEnabled = chkPowerControl.Checked;
        }

        private void ControlCenter_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                foreach (ZonesFormPair zone in m_zoneFormsList)
                {
                    if (zone.ZonesForm!=null)
                        zone.ZonesForm.Close();
                }
                MZPState.Instance.Shutdown();
                
            }
            catch (Exception )
            {
            }
        }
    }
}

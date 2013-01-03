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
using System.Text.RegularExpressions;

namespace MultiZonePlayer
{
    
    public  partial class ControlCenter : Form
    {
        static MainScreen parentForm = null;
        public static bool IsPowerControlEnabled = false;

        private RawInputDevice m_rawDeviceId;

        private static List<ZonesForm> m_zoneFormsList;
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

            m_zoneFormsList = new List<ZonesForm>();
            hsDelegatedZoneForms = new Hashtable();
            
            
        }

        private void ControlCenter_Load(object sender, EventArgs e)
        {
            TXTLog = txtError;
            TXTLog2 = txtLog2;
            TXTLog3 = txtLog3;
            m_ctrlCenter = this;

            Thread th = new Thread(() => TickLoop());
            th.Name = "ControlCenter TickLoop";
            th.Start();

            //RegisterDriveDetector(); UNUSED
            RegisterRawInput();
            Program.kh = new KeyboardHook(KeyboardHook.Parameters.PassAllKeysToNextApp);
            Program.kh.KeyIntercepted += new KeyboardHook.KeyboardHookEventHandler(kh_KeyIntercepted);
            
            MLog.Log(null,"Using Ini path " + IniFile.CurrentPath());
        }

        public static ControlCenter Instance
        {
            get{return m_ctrlCenter;}
        }

        private void m_KeyPressed(object sender, ref RawInputDevice.KeyControlEventArgs e)
        {
            MLog.LogKey(String.Format("Raw: {0} down={1}", e.Keyboard.vKey, e.Keyboard.isKeyDownWinMessage));
            KeyDetail kd = new KeyDetail(e.Keyboard.vKey, e.Keyboard.deviceName, e.Keyboard.isKeyDownWinMessage, e.Keyboard.isKeyUpWinMessage);
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

        void kh_KeyIntercepted(KeyboardHook.KeyboardHookEventArgs e)
        {
            //Check if this key event is being passed to
            //other applications and disable TopMost in 
            //case they need to come to the front
            if (e.PassThrough)
            {
                this.TopMost = false;
            }

            MLog.LogKey(String.Format("KeyHook: {0} code={1} pass={2} name={3} up={4}",
                e.keySet, e.KeyCode, e.PassThrough, e.KeyName, e.keyUp));
            //ds.Draw(e.KeyName);
        }

        private void TickLoop()
        {
            Thread.Sleep(Convert.ToInt16(IniFile.PARAM_BOOT_TIME_SECONDS[1]) * 1000);
            do
            {
                try
                {
                    Thread.Sleep(IniFile.ZONE_INACTIVITY_CYCLE_DURATION);
                    // FAST TICK
                    RefreshState();
                    foreach (ZoneGeneric zone in MZPState.Instance.ActiveZones)
                    {
                        zone.Tick();
                    }
                    // SLOW TICK
                    if (DateTime.Now.Subtract(m_lastSlowTickDateTime).Duration().TotalSeconds > 30)
                    {
                        m_lastSlowTickDateTime = DateTime.Now;
                        MZPState.Instance.Tick();
                    }
                }
                catch (Exception ex)
                {
                    MLog.Log(ex, this, "Error tickloop");
                }
            }
            while (MZPState.Instance != null);
            MLog.Log(this, "ControlCenter TickLoop EXIT");
        }

        delegate void RefreshStateDelegate();
        public void RefreshState()
        {
            if (this.InvokeRequired)
            {
                RefreshStateDelegate dlg = new RefreshStateDelegate(RefreshState);
                this.BeginInvoke(dlg);
                return;
            }
            
            if (MZPState.Instance != null)
            {
                txtInactivityCycles.Text = MZPState.Instance.PowerControl.GetInactivityCycles().ToString() + "/" + IniFile.POWERSAVING_MAX_CYCLES;
                txAlarm.Text = MZPState.Instance.SystemAlarm.IsMonitoringActive + ":" + MZPState.Instance.SystemAlarm.AreaState.ToString();

                foreach (Metadata.ZoneDetails zone in MZPState.Instance.ZoneDetails)
                {
                    if (zone.IsActive)
                    {
                        ZonesForm form = m_zoneFormsList.Find(x => x.ZoneDetails.ZoneId.Equals(zone.ZoneId));
                        if (form == null)
                        {
                            MLog.Log(this, "Initialising ZoneForm " + zone.ZoneName);
                            OpenZoneForm(zone.ZoneId);
                        }
                    }
                }
                ShowPowerControlStatus();
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
        }

        public void CloseAllZones()
        {
            try
            {
                foreach (ZonesForm zone in m_zoneFormsList)
                {
                     zone.Close();
                }
            }
            catch (Exception ex)
            {
                MLog.Log(ex, "Error close all zones");
            }
        }

        public void CloseZone(int zoneId)
        {
            //MLog.Log(null,"Closing zone " + zoneId);
            try
            {
                m_currentZone = null;
                ZonesForm zone = m_zoneFormsList.Find(item => item.ZoneDetails.ZoneId == zoneId);
                m_zoneFormsList.Remove(zone);
            }
            catch (Exception ex)
            {
                MLog.Log(ex, "Error closing zone " + zoneId);
            }
        }
        
        public void OpenZone(int zoneId)
        {
            if (!ControlCenter.Instance.IsZoneActive(zoneId))
            {
                MZPState.Instance.ActiveZones.Add(new ZoneGeneric(zoneId));
            }
        }

        private void OpenZoneForm(int zoneId)
        {
            if (ControlCenter.Instance.IsZoneActive(zoneId))
            {
                ZoneGeneric zone = MZPState.Instance.ActiveZones.Find(item => item.ZoneDetails.ZoneId == zoneId);

                if (zone != null)
                {
                    ZonesForm newZoneForm = new ZonesForm(zone, this);
                    m_zoneFormsList.Add(newZoneForm);
                    zone.ZoneForm = newZoneForm;
                    //parentForm.LayoutMdi(MdiLayout.TileHorizontal);m_currentZone = GetZone(zoneId);
                    newZoneForm.MdiParent = parentForm;
                    newZoneForm.WindowState = FormWindowState.Minimized;
                    newZoneForm.Text = "Zone " + zoneId;
                    newZoneForm.Show();
                }
                else
                    MLog.Log(this, "Unable to open zone form" + zoneId);
            }
        }

        public static int GetActiveZonesCount()
        {
            return m_zoneFormsList.Count;
        }

        public ZoneGeneric GetZoneIfActive(int zoneId)
        {
            ZoneGeneric zp = MZPState.Instance.ActiveZones.Find(item => item.ZoneDetails.ZoneId == zoneId);
            if (zp != null && IsZoneActive(zoneId))
                return zp;
            else
                return null;
        }

        public ZoneGeneric GetZone(int zoneId)
        {
            ZoneGeneric zp = MZPState.Instance.ActiveZones.Find(item => item.ZoneDetails.ZoneId == zoneId);
            return zp;
        }

        public  bool IsZoneActive(int zoneId)
        {
            Metadata.ZoneDetails zone = MZPState.Instance.ZoneDetails.Find(item => item.ZoneId==zoneId);
            if (zone != null) 
                return zone.IsActive;
            else return false;;
        }

        public static ZoneGeneric GetFirstZoneMusic()
        {
            foreach (ZoneGeneric zone in MZPState.Instance.ActiveZones)
            {
                    if ((zone.GetCurrentActivity() != null) && (zone.GetCurrentActivity().GetType() == typeof(ZoneMusic)))
                        return zone;
                
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
        
        #endregion

        #region FormEvents
        private void ControlCenter_Shown(object sender, EventArgs e)
        {
            txtCommand.Focus();
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
                foreach (ZonesForm zone in m_zoneFormsList)
                {
                    zone.Close();
                }
                MZPState.Instance.Shutdown();
            }
            catch (Exception )
            {
            }
        }
    }
}

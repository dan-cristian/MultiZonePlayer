﻿using System;
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
        
        public static bool IsPowerControlEnabled = false;
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
            MLog.Log(null,"Using Ini path " + IniFile.CurrentPath());
        }


        public static ControlCenter Instance
        {
            get{return m_ctrlCenter;}
        }


        private void TickLoop()
        {
            Thread.Sleep(Convert.ToInt16(IniFile.PARAM_BOOT_TIME_SECONDS[1]) * 1000);
            Array activeZoneClone;
			String debug="";
			do {
                try  {
					//Application.DoEvents();
                    Thread.Sleep(IniFile.ZONE_TICK_FAST_SLEEP);
                    // FAST TICK
                    RefreshState();
					ZoneDetails.ProcessAllZones(true, false);
					if (MZPState.Instance != null)	{
						debug = "";
						activeZoneClone = ZoneDetails.ActiveZones.ToArray();
						foreach (ZoneGeneric zone in activeZoneClone)	{
							debug += zone.ZoneDetails.ZoneName + "-";
							if (MZPState.Instance == null) break;
							if (DateTime.Now.Subtract(m_lastSlowTickDateTime).Duration().TotalSeconds > 30) {
								zone.TickSlow();
								debug += "slow;";
							}
							zone.TickFast();
							debug += "fast;";
						}
						//MLog.Log(this, "TICK: " + debug);
						MZPState.Instance.TickFast();
					}
                    // SLOW TICK
                    if (DateTime.Now.Subtract(m_lastSlowTickDateTime).Duration().TotalSeconds > 10)
                    {
                        m_lastSlowTickDateTime = DateTime.Now;
						ZoneDetails.ProcessAllZones(false, true);
						MZPState.Instance.TickSlow();
                        ZoneDetails zonewithmusic = ZoneDetails.ZoneDetailsList.Find(x => x.IsActive && x.ActivityType == GlobalCommands.music);
                        //increase priority when playing music
                        if (zonewithmusic!=null)
                            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.AboveNormal;
                        else
                            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;
						/*foreach (ZoneDetails details in ZoneDetails.ZoneDetailsList){
							if (details.HasPastActivity) ZoneGeneric.ZoneInactiveActions(details);
							List<ZoneDetails> zonesWithPower = ZoneDetails.ZoneDetailsList.FindAll(x =>
								(x.IsIdle ||x.RequirePower) &&
								(x.PowerType == details.PowerType && x.PowerIndex == details.PowerIndex) &&
								(x.ZoneId != details.ZoneId));
							if (zonesWithPower.Count==0 && !details.IsActive && !details.RequirePower)
							{
								if (MZPState.Instance.PowerControlIsOn(details.ZoneId))
								{
									MLog.Log(this, "No power required on index " + details.PowerIndex
										+ ", powering off inactive zone=" + details.ZoneName);
									MZPState.Instance.PowerControlOff(details.ZoneId);
								}
							}
						}*/
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
                //txtInactivityCycles.Text = MZPState.Instance.PowerControl.GetInactivityCycles().ToString() + "/" + IniFile.POWERSAVING_MAX_CYCLES;
                txAlarm.Text = MZPState.Instance.SystemAlarm.IsMonitoringActive + ":" + MZPState.Instance.SystemAlarm.AreaState.ToString();
				//txtGPIO.Text = MZPState.Instance.Gpio.State;
                /*foreach (ZoneDetails zone in ZoneDetails.ZoneDetailsList)
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
                }*/
	            txtThreadCount.Text = WebServer.WebThreadCount.ToString();
                ShowPowerControlStatus();
            }
            txtZoneCount.Text = m_zoneFormsList.Count.ToString();

            /*
			if (MediaLibrary.IsInitialised)
			{
				//txAudioCount.Text = MediaLibrary.AllAudioFiles.PlaylistFiles.Count.ToString() + "[" +
				//	MediaLibrary.AllAudioFiles.ArtistMetaList.Count.ToString() + "]";
				//txVideoCount.Text = MediaLibrary.AllVideoFiles.PlaylistFiles.Count.ToString();
				//txAudioSave.Text = MediaLibrary.AllAudioFiles.PendingSaveItemsCount().ToString();
				//txPictureCount.Text = MediaLibrary.AllPictureFiles.PlaylistFiles.Count.ToString();
			}
			else
			{
				if (MediaLibrary.AllAudioFiles != null && MediaLibrary.AllAudioFiles.PlaylistFiles != null)
					txAudioCount.Text = "loading " + MediaLibrary.AllAudioFiles.PlaylistFiles.Count;
				if (MediaLibrary.AllPictureFiles != null && MediaLibrary.AllPictureFiles.PlaylistFiles != null)
					txPictureCount.Text = "loading " + MediaLibrary.AllPictureFiles.PlaylistFiles.Count;
				if (MediaLibrary.AllVideoFiles != null && MediaLibrary.AllVideoFiles.PlaylistFiles != null)
					txVideoCount.Text = "loading " + MediaLibrary.AllVideoFiles.PlaylistFiles.Count;
			}
            */
			
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
                MLog.Log(ex, "Error close all m_valueList");
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
        
		/*
        private void OpenZoneForm(int zoneId)
        {
            if (MZPState.Instance.IsZoneActive(zoneId))
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
                    MLog.Log(this, "Unable to open zone form, zone not in active zoneform list " + zoneId);
            }
        }*/

        public static int GetActiveZonesCount()
        {
            return m_zoneFormsList.Count;
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
                /*if (MultiZonePlayer.MLog.Log(this, "Allow remove?", "Query remove",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    e.Cancel = false;       // Allow removal
                else
                    e.Cancel = true;        // Cancel the removal of the device  
				 */
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
            
            /*bool alive = MZPState.Instance.PowerControl.IsPowerControlOn();
            txtPwrControl.Text = alive.ToString();
            txPwrName.Text = MZPState.Instance.PowerControl.GetPowerControlName();
            txtPowerSaving.Text = MZPState.Instance.PowerControl.IsPowerSavingMode().ToString();
            for (int i = 0; i < 10; i++)
            {
                if ((Button)tbButtons.Controls["bt" + i] != null)
                    ((Button)tbButtons.Controls["bt" + i]).FlatStyle = (MZPState.Instance.PowerControl.SocketsOn(i) ? FlatStyle.Flat : FlatStyle.Standard);
            }
            txSocketsStatus.Text = MZPState.Instance.PowerControl.SocketsStatus;
			 * */
            
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
            API.DoCommandFromGUIInput(GlobalCommands.selectzone.ToString(), ((Button)sender).Text);
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
                if (MZPState.Instance != null)
					MZPState.Instance.Shutdown();
            }
            catch (Exception ex)
            {
				MLog.Log(ex, "Err closing m_valueList");
            }
        }

		private void txtError_TextChanged(object sender, EventArgs e)
		{

		}

		private void button2_Click(object sender, EventArgs e)
		{
			txtError.Clear();
			txtError.AppendText(Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds / 100 + "");
			foreach (ProcessThreadCollection th in Process.GetCurrentProcess().Threads)
			{
				
				foreach (ProcessThread t in th)
				{
					txtError.AppendText(t.Id + "  state=" + t.ThreadState + "  start=" + t.StartTime + "\n");
				}
				
			}
		}
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;


namespace MultiZonePlayer
{
    public interface IPlayEvent
    {
        void EventNextAuto();
        int GetZoneId();
    }

    public partial class ZonesForm : Form, IPlayEvent
    {
        private ControlCenter controlCenter;

        private ZoneGeneric m_zone;
        private Metadata.ZoneDetails m_zoneDetails;

        public ZonesForm(ZoneGeneric zone, ControlCenter controlCenter)
        {
            InitializeComponent();
            this.controlCenter = controlCenter;
            m_zone = zone;
            m_zoneDetails = zone.ZoneDetails;
        }

        private void ZonesForm_Load(object sender, EventArgs e)
        {
            
            //timerZone.Start();
            this.Text = "Zone " + m_zone.ZoneName;
        }

  
        public IZoneActivity GetCurrentActivity()
        {
            return m_zone.MainZoneActivity;
        }

        public int GetZoneId()
        {
            return m_zoneDetails.ZoneId;
        }

        public Metadata.ZoneDetails ZoneDetails
        {
            get
            {
                return m_zoneDetails;
             }
        }

        public String ZoneName
        {
            get
            {
                return m_zoneDetails.ZoneName;
            }
        }

        

        public int GetVolumeLevel()
        {
            if (m_zone.MainZoneActivity != null)
                return m_zone.MainZoneActivity.GetVolumeLevel();
            else
                return m_zoneDetails.GetDefaultVolume();
        }

        /*
        public String GetDelegatedZoneName()
        {
            return m_targetZoneName;
        }
        */
        public void EventNextAuto()
        {
            m_zone.MainZoneActivity.Next();
            RefreshState();
        }

        public void NextPlaylist()
        {
            ((ZoneMusic)m_zone.MainZoneActivity).NextPlaylist();
            ShowPlayList();
        }

        delegate void RefreshStateDelegate();
        public void RefreshState()
        {
            try
            {
                if (this.InvokeRequired)
                {
                    RefreshStateDelegate dlg = new RefreshStateDelegate(RefreshState);
                    this.BeginInvoke(dlg);
                    return;
                }

                txtOutputDevice.Text = m_zoneDetails.OutputDeviceAutoCompleted;
                txtUser.Text = m_zone.ZoneUser.Name;
                txtInactiveCycles.Text = m_zone.InactiveCyclesCount.ToString()+"/" + IniFile.ZONE_INACTIVITY_MAX_CYCLES;
                txtControlDevice.Text = m_zone.ControlDevice;
                txtCurrentCmd.Text = m_zone.CurrentCmd;
                txtZoneName.Text = m_zoneDetails.ZoneId + ":"+m_zoneDetails.ZoneName;
                txSleepTimer.Text = m_zoneDetails.SleepHourMin;
                txWakeTime.Text = m_zoneDetails.WakeTime;
                txAlarm.Text = m_zoneDetails.MovementAlert + ":" + m_zoneDetails.LastAlarmMovementDateTime;
                txtActive.Text = m_zoneDetails.IsActive.ToString();

                if (m_zone.MainZoneActivity != null)
                {
                    txGenPosition.Text = m_zone.MainZoneActivity.Position.ToString();
                    txPositionPercent.Text = m_zone.MainZoneActivity.PositionPercent.ToString();
                    txtVolumeLevel.Text = m_zone.MainZoneActivity.GetVolumeLevel().ToString();
                    txtMusicStatus.Text = m_zone.MainZoneActivity.GetState().ToString();

                    if (m_zone.MainZoneActivity.GetType() == typeof(ZoneMusic))
                    {
                        ZoneMusic zMusic = (ZoneMusic)m_zone.MainZoneActivity;

                        tabZone.SelectedTab = tabZone.TabPages[0];
                        txtMusicFile.Text = zMusic.GetMusicFileDescription();
                        txtPlaylist.Text = m_zoneDetails.Playlist;
                        txtPlayMode.Text = zMusic.GetPlayMode().ToString();
                        
                        txCount.Text = zMusic.GetSongList().Count.ToString();
						if (zMusic.CurrentItem != null)
						{
							//show playlist index
							String currentSong = zMusic.CurrentItem.SourceURL;
							//int playIndex;
							txRating.Text = zMusic.CurrentItem.Rating.ToString();
						}
                        List<MediaItem> defaultSongList = zMusic.GetSongList();
                        //playIndex = zMusic.GetHashKey(defaultSongList, currentSong);
                        //dgvPlayList.CurrentCell = dgvPlayList.Rows[playIndex].Cells[0];
                    }

                    if (m_zone.MainZoneActivity.GetType() == typeof(ZoneRadio))
                    {
                        ZoneRadio radio = (ZoneRadio)m_zone.MainZoneActivity;
                        tabZone.SelectedTab = tabZone.TabPages[1];
                        txtInputDevice.Text = radio.GetInputDeviceName();
                        txtExtProcess.Text = radio.GetExtProcessStatus();
                        txtAppHandle.Text = radio.GetAppHandle().ToString() + " active = " + radio.IsActive();
                    }

                    if (m_zone.MainZoneActivity.GetType() == typeof(ZoneStreamVLC))
                    {
                        ZoneStreamVLC stream= (ZoneStreamVLC)m_zone.MainZoneActivity;
                        tabZone.SelectedTab = tabZone.TabPages[5];
                        textBoxStreamingUrl.Text = stream.GetCurrentURL();
                        lbVolume.Text = stream.GetVolumeLevelNative().ToString();
                        lbRadio.Text = stream.ZoneDetails.Author;
                        lbstreamtitle.Text = stream.ZoneDetails.Title;
                    }

                    if ((m_zone.MainZoneActivity.GetType() == typeof(ZoneVideoMPC)) || (m_zone.MainZoneActivity.GetType() == typeof(ZoneVideoVlc)))
                    {
                        ZoneVideoBase video = (ZoneVideoBase)m_zone.MainZoneActivity;
                        tabZone.SelectedTab = tabZone.TabPages[2];
                    }

                    if (m_zone.MainZoneActivity.GetType() == typeof(ZoneDisplayLG))
                    {
                        ZoneDisplayLG tv= (ZoneDisplayLG)m_zone.MainZoneActivity;
                        tabZone.SelectedTab = tabZone.TabPages[6];
                        txtInput.Text = tv.InputType.ToString();
                    }

                    if (m_zone.MainZoneActivity.GetType() == typeof(ZonePlayerXBMC))
                    {
                        ZonePlayerXBMC zXBMC = (ZonePlayerXBMC)m_zone.MainZoneActivity;
                        tabZone.SelectedTab = tabZone.TabPages[0];
                        txtMusicFile.Text = zXBMC.ZoneDetails.Title;
                    }
                }
            }
            catch (Exception ex)
            {
                MLog.Log(ex,this, "Error zone form refreshstate zone="+m_zoneDetails.ZoneName );
            }
        }

        delegate void ShowPlayListDelegate();
        public void ShowPlayList()
        {
            if (this.InvokeRequired)
            {
                ShowPlayListDelegate dlg = new ShowPlayListDelegate(ShowPlayList);
                this.BeginInvoke(dlg);
                return;
            }
            dgvPlayList.Rows.Clear();
            List<MediaItem> defaultSongList;

            defaultSongList = ((ZoneMusic)m_zone.MainZoneActivity).GetSongList();
            if (defaultSongList != null)
            {
                dgvPlayList.Visible = false;

                for (int i = 0; i < defaultSongList.Count; i++)
                {
                    //songName = defaultSongList[i].SourceURL.Substring(Math.Max(0, defaultSongList[i].SourceURL.Length - 40)).Replace("\\", "/");
                    dgvPlayList.Rows.Add(defaultSongList[i].Index,
                        defaultSongList[i].Rating, 
                        defaultSongList[i].PlayCount, 
                        defaultSongList[i].Author,
                        //songName, 
                        defaultSongList[i].Title,
                        defaultSongList[i].Genre,
                        defaultSongList[i].RandomId);
                }
                dgvPlayList.Visible = true;
               
            }

        }

        delegate void DelegateCloseFormSafe();
        public void CloseFormSafe()
        {
            if (this.InvokeRequired)
            {
                DelegateCloseFormSafe dlg = new DelegateCloseFormSafe(CloseFormSafe);
                this.BeginInvoke(dlg);
                return;
            }

            Close();
        }

        private void ZonesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_zone.CloseZone();
            controlCenter.CloseZone(m_zoneDetails.ZoneId);
        }

        private void CheckForSleep()
        {
            if (m_zoneDetails.SleepHourMin.Equals(DateTime.Now.ToString(IniFile.DATETIME_FORMAT)))
            {
                MLog.Log(this, "Zone sleeping");
                this.Close();
                //m_zoneDetails.SleepHourMin = "";
            }
        }

        

        

        private void button1_Click(object sender, EventArgs e)
        {
            if (m_zone.MainZoneActivity.GetType() == typeof(ZoneRadio))
            {
                ZoneRadio radio = (ZoneRadio)m_zone.MainZoneActivity;
                radio.Next();
            }
        }

       

        private void btRepeat_Click(object sender, EventArgs e)
        {
            API.DoCommandFromGUIInput(((Button)sender).Tag.ToString().ToLower(), GetZoneId().ToString());
        }

        private void dgvPlayList_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            ZoneMusic music = (ZoneMusic) m_zone.MainZoneActivity;
            int ind = Convert.ToInt16(dgvPlayList.Rows[e.RowIndex].Cells[Music_Index.Name].Value);//todo change index

            if (ind >=0)
                music.Play(ind);
        }

        private void txWakeTime_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Metadata.ValueList vals = new Metadata.ValueList(Metadata.GlobalParams.command, Metadata.GlobalCommands.setwaketimer.ToString(), Metadata.CommandSources.gui);
            vals.Add(Metadata.GlobalParams.datetime, txWakeTime.Text);
            vals.Add(Metadata.GlobalParams.weekday, "WK");
            m_zone.ProcessAction(Metadata.GlobalCommands.setwaketimer, vals);
        }

        private void txSleepTimer_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            m_zoneDetails.SleepHourMin = txSleepTimer.Text;
        }

        private void cmbMoods_SelectedIndexChanged(object sender, EventArgs e)
        {
            Metadata.ValueList vals = new Metadata.ValueList(Metadata.GlobalParams.zoneid, m_zoneDetails.ZoneId.ToString(), Metadata.CommandSources.gui);
            vals.Add(Metadata.GlobalParams.command, Metadata.GlobalCommands.setmoodmusic.ToString());
            vals.Add(Metadata.GlobalParams.selectedindex, MZPState.Instance.MoodMusicList.Find(x=>x.Name.Equals(cmbMoods.Text)).Index.ToString());
            API.DoCommandFromGUIInput(vals);
        }

        private void btClearTags_Click(object sender, EventArgs e)
        {
            ZoneMusic music = (ZoneMusic)m_zone.MainZoneActivity;
            music.ClearUsageTags();
            ShowPlayList();
            RefreshState();
        }

        private void label25_Click(object sender, EventArgs e)
        {

        }
    }

    
}

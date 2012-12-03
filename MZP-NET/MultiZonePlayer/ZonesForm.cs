using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


namespace MultiZonePlayer
{

    public partial class ZonesForm : Form, IPlayEvent
    {
        private ControlCenter controlCenter;

        private Users m_zoneUser = null;
        private String m_currentCmd = null;
        private int m_sourceZoneId;
        private int m_inactiveCyclesCount = 0;
        private IZoneActivity m_mainZoneActivity;
        private String m_controlDevice = null;
        private Metadata.ZoneDetails m_zoneDetails;
        private List<IZoneActivity> m_clonedZones;
        private static int m_recIndex = 0;

        public ZonesForm(int zoneId, ControlCenter controlCenter, Metadata.ZoneDetails zoneDetails)
        {
            InitializeComponent();
            this.controlCenter = controlCenter;
            m_zoneDetails = zoneDetails;
            this.m_sourceZoneId = zoneId;
            m_clonedZones = new List<IZoneActivity>();
            
            //default user - all
            m_zoneUser = new Users("0","all","000");//SystemState.iniUserList["000"] as Users;
            LoadZoneIni();
        }

        private void ZonesForm_Load(object sender, EventArgs e)
        {
            
            //timerZone.Start();
            this.Text = "Zone " + m_zoneDetails.ZoneName;
        }

        /*public void SetActivity(String cmd)
        {
            Metadata.GlobalCommands enumcmd = (Metadata.GlobalCommands)Enum.Parse(typeof(Metadata.GlobalCommands), cmd);
            switch (enumcmd)
            {
                case Metadata.GlobalCommands.music:
                    InitZoneMusic();
                    break;
                case Metadata.GlobalCommands.musicclone:
                    InitZoneMusicClone();
                    break;
                case Metadata.GlobalCommands.video:
                    InitZoneVideoApp();
                    break;
                case Metadata.GlobalCommands.streammp3:
                    InitZoneStreamVLC();
                    break;
                case Metadata.GlobalCommands.tv:
                    InitZoneDisplayTV();
                    break;
                case Metadata.GlobalCommands.alarmevent:
                    break;
                case Metadata.GlobalCommands.cameraevent:
                    break;
                default:
                    MLog.Log(this, "Invalid activity specified " + cmd);
                    break;
            }
            ShowPlayList();
        }*/

        private void CloseZone()
        {
            try
            {
                //MLog.Log(null, "Closing zone " + m_zoneDetails.ZoneId + ", activity=" + m_mainZoneActivity);
                if (m_mainZoneActivity != null)
                {
                    m_mainZoneActivity.Close();
                }

                controlCenter.ZoneClosing(m_zoneDetails.ZoneId);

                Metadata.ZoneDetails childZone = MZPState.Instance.ZoneDetails.Find(x => x.ParentZoneId == m_zoneDetails.ZoneId);
                
                //power off only if no child zones are using power socket
                if (childZone == null || !childZone.IsActive)
                    MZPState.Instance.PowerControl.PowerOff(m_zoneDetails.ZoneId);
                else
                    MLog.Log(this, "Not powering off zone " + m_zoneDetails.ZoneName+", there are active childs using it, firstchild=" + childZone.ZoneName);

                Metadata.ZoneDetails parentZone = MZPState.Instance.GetZoneById(m_zoneDetails.ParentZoneId);
                if (parentZone != null && !parentZone.IsActive)
                {
                    MLog.Log(this, "Closing child zone " +m_zoneDetails.ZoneName+" , power off parent zone " + parentZone.ZoneName+ " as well");
                    MZPState.Instance.PowerControl.PowerOff(m_zoneDetails.ParentZoneId);
                }

                m_zoneDetails.ResetValues();
            }
            catch (Exception ex)
            {
                MLog.Log(null, "Error closing form " + ex.Message);
            }
        }

        private void StopRemoveClonedZones()
        {
            foreach (IZoneActivity izone in m_clonedZones)
            {
                izone.Stop();
            }
            m_clonedZones.Clear();
        }

        private void InitZoneMusic()
        {
            MLog.Log(this,"Initialising music");
            if ((m_mainZoneActivity == null) || (m_mainZoneActivity.GetType() != typeof(ZoneMusic)))
            {
                //ControlCenter.Instance.OpenZone(m_zoneDetails.ZoneId);
                StopRemoveClonedZones();

                m_mainZoneActivity = new ZoneMusic(this);
                m_zoneDetails.ActivityType = Metadata.GlobalCommands.music;
                m_zoneDetails.IsActive = true;
                
                AddClonedZone(m_mainZoneActivity);
                
                cmbMoods.Items.Clear();
                foreach (MoodMusic mood in MZPState.Instance.MoodMusicList)
                {
                    cmbMoods.Items.Add(mood.Name);
                }
            }
            else MLog.Log(this, "Initialising music not needed, already init");

        }

        private void InitZoneMusicClone()
        {
            MLog.Log(null, "Initialising music clone");
            ///ControlCenter.Instance.OpenZone(m_zoneDetails.ZoneId);
            StopRemoveClonedZones();

            m_mainZoneActivity = new ZoneMusicClone(this, ControlCenter.GetFirstZoneMusic());
            m_zoneDetails.ActivityType = Metadata.GlobalCommands.musicclone;
            m_zoneDetails.IsActive = true;
        }

        private void InitZoneRadio()
        {
            MLog.Log(null,"Initialising radio");
            // input zone 0 is Radio
            //ControlCenter.Instance.OpenZone(m_zoneDetails.ZoneId);
            String inDevice = (String)MZPState.Instance.zoneDefaultInputs["0"];
            m_mainZoneActivity = new ZoneRadio(this, inDevice, m_zoneDetails.OutputDeviceAutoCompleted);
            m_zoneDetails.ActivityType = Metadata.GlobalCommands.radio;
            m_zoneDetails.IsActive = true;
            m_mainZoneActivity.Play();
        }

        private void InitZoneStreamVLC()
        {
            MLog.Log(null, "Initialising mp3 VLC stream");
            //ControlCenter.Instance.OpenZone(m_zoneDetails.ZoneId);
            m_mainZoneActivity = new ZoneStreamVLC(m_zoneDetails);
            m_zoneDetails.ActivityType = Metadata.GlobalCommands.streammp3;
            m_zoneDetails.IsActive = true;
            //m_mainZoneActivity.Play();
        }

        private void InitZoneMic()
        {
            MLog.Log(null,"Initialising microphone");
            //ControlCenter.Instance.OpenZone(m_zoneDetails.ZoneId);
            String inDevice = (String)MZPState.Instance.zoneDefaultInputs[m_zoneDetails.ZoneId];
            m_mainZoneActivity = new ZoneMic(this, inDevice, m_zoneDetails.OutputDeviceAutoCompleted);
            m_zoneDetails.ActivityType = Metadata.GlobalCommands.microphone;
            m_zoneDetails.IsActive = true;
        }

        private void InitZoneVideoApp()
        {
            MLog.Log(null,"Initialising video");
            //ControlCenter.Instance.OpenZone(m_zoneDetails.ZoneId);
            //m_mainZoneActivity = new ZoneVideoVlc(this);
            m_mainZoneActivity = new ZoneVideoMPC(this);
            m_zoneDetails.ActivityType = Metadata.GlobalCommands.video;
            m_zoneDetails.IsActive = true;
        }

        private void InitZoneDisplayTV()
        {
            MLog.Log(null, "Initialising TV");
            //ControlCenter.Instance.OpenZone(m_zoneDetails.ZoneId);
            //m_mainZoneActivity = new ZoneVideoVlc(this);
            m_mainZoneActivity = new ZoneDisplayLG(this.ZoneDetails);
            m_zoneDetails.ActivityType = Metadata.GlobalCommands.tv;
            m_zoneDetails.IsActive = true;
        }
        
        public Metadata.ValueList ProcessAction(Metadata.GlobalCommands cmdRemote, Metadata.ValueList vals)
        {
            Metadata.ValueList result = new Metadata.ValueList();
            //reset inactivity counter
            m_inactiveCyclesCount = -1;

            m_zoneDetails.LastLocalCommandDateTime = DateTime.Now;

            String date,weekday,action;

            #region commands without activity
            switch (cmdRemote)
            {
                case Metadata.GlobalCommands.cameraevent://video camera event
                    MZPState.Instance.ZoneEvents.AddCamAlert(vals);
                    if (m_zoneDetails.IsArmed || MZPState.Instance.SystemAlarm.AreaState.Equals(Alarm.EnumAreaState.armed))
                    {
                        String msg =  "CAM event on " + m_zoneDetails.ZoneName + " when zone armed";
                        MZPState.Instance.LogEvent(MZPEvent.EventSource.Cam, msg, MZPEvent.EventType.Security, MZPEvent.EventImportance.Critical);
                    }
                    //TODO
                    break;
                case Metadata.GlobalCommands.dismisscameraalert:
                    MZPState.Instance.ZoneEvents.DismissAlert(vals);
                    break;
                case Metadata.GlobalCommands.togglecameraalert:
                    MZPState.Instance.ZoneEvents.ToggleAlertStatus(vals);
                    break;
                case Metadata.GlobalCommands.alarmevent://alarm sensor event
                    String zonestate = vals.GetValue(Metadata.GlobalParams.status);
                    m_zoneDetails.MovementAlert = zonestate.Equals(Alarm.EnumZoneState.opened.ToString());
                    if (m_zoneDetails.MovementAlert && (m_zoneDetails.IsArmed  || MZPState.Instance.SystemAlarm.AreaState.Equals(Alarm.EnumAreaState.armed)))
                    {
                        String msg = "ALARM event on " + m_zoneDetails.ZoneName + " when zone armed";
                        MZPState.Instance.LogEvent(MZPEvent.EventSource.Alarm, msg, MZPEvent.EventType.Security, MZPEvent.EventImportance.Critical);
                    }
                    if (MZPState.Instance.IsFollowMeMusic & m_zoneDetails.HasSpeakers)
                    {
                        cmdRemote = Metadata.GlobalCommands.musicclone;
                    }
                    m_zoneDetails.LastAlarmMovementDateTime = Convert.ToDateTime(vals.GetValue(Metadata.GlobalParams.datetime));
                    break;
                case Metadata.GlobalCommands.setwaketimer:
                    date = vals.GetValue(Metadata.GlobalParams.datetime);
                    weekday = vals.GetValue(Metadata.GlobalParams.weekday);
                    m_zoneDetails.WakeTime = date == null ? "" : date;
                    m_zoneDetails.WakeWeekDay = weekday == null ? "" : weekday;
                    break;
                case Metadata.GlobalCommands.sleeptimer:
                    date = vals.GetValue(Metadata.GlobalParams.datetime);
                    if (date.Equals(""))
                        m_zoneDetails.SleepHourMin = "";
                    else
                        m_zoneDetails.SleepHourMin = Convert.ToDateTime(date).ToString(IniFile.DATETIME_FORMAT);
                    /*if (minutes == -1)
                        m_zoneDetails.SleepHourMin = "";
                    else
                    {
                        DateTime dt = DateTime.Now.AddMinutes(minutes);
                        m_zoneDetails.SleepHourMin = dt.Hour + ":" + dt.Minute;
                    }*/
                    break;
                case Metadata.GlobalCommands.micrecord:
                    m_recIndex++;
                    Utilities.WriteBinaryFile("\\webroot\\direct\\zone" + m_zoneDetails.ZoneId + "_" + m_recIndex + ".wav", vals.BinaryData);
                    break;
                case Metadata.GlobalCommands.micplay:
                    InitZoneMusic();
                    ZoneMusic music = (ZoneMusic)m_mainZoneActivity;
                    music.Play(IniFile.CurrentPath() + "\\webroot\\direct\\zone" + m_zoneDetails.ZoneId + "_" + m_recIndex + ".wav");
                    break;
                case Metadata.GlobalCommands.zonearm:
                case Metadata.GlobalCommands.zonedisarm:
                    m_zoneDetails.IsArmed = (cmdRemote == Metadata.GlobalCommands.zonearm);
                    result.Add(Metadata.GlobalParams.msg, "Zone " + m_zoneDetails.ZoneName + " armed status=" + m_zoneDetails.IsArmed);
                    break;
            }
            #endregion

            action = vals.GetValue(Metadata.GlobalParams.action);

            #region Zone init commands
            switch (cmdRemote)
            {
                case Metadata.GlobalCommands.music:
                case Metadata.GlobalCommands.musicalarm:
                    //if cmd is music, if not on Music already stop current activity and init Music
                    if ((m_mainZoneActivity != null) && (m_mainZoneActivity.GetType() != typeof(ZoneMusic)))
                    {
                        m_mainZoneActivity.Close();
                        m_mainZoneActivity = null;
                    }

                    ZoneMusic zMusic;

                    if (m_mainZoneActivity == null)
                    {
                        InitZoneMusic();
                        zMusic = (ZoneMusic)m_mainZoneActivity;
                        //MoodMusic mood = MZPState.Instance.MoodMusicList.Find(x => x.Name.Equals("default"));
                        //if (mood != null)
                        //    zMusic.SetMood(mood);
                        ShowPlayList();
                    }
                    zMusic = (ZoneMusic)m_mainZoneActivity;

                    if (m_mainZoneActivity != null)
                    {
                        switch (cmdRemote)
                        {
                            case Metadata.GlobalCommands.musicalarm:
                                if (!zMusic.IsAlarm)//(!m_mainZoneActivity.GetState().Equals(Metadata.ZoneState.Running))
                                {
                                    zMusic.IsAlarm = true;
                                    //set musicalarm mood if exists
                                    MoodMusic mood = MZPState.Instance.MoodMusicList.Find(x => x.Name.Equals(Metadata.GlobalCommands.musicalarm.ToString()));
                                    if (mood != null)
                                        zMusic.SetMood(mood);
                                    m_mainZoneActivity.Play();
                                }
                                break;
                            case Metadata.GlobalCommands.music://play once opened unless cmd is from web or explicit play
                                if (!vals.CommandSource.Equals(Metadata.CommandSources.Web) || (action != null && action.Equals(Metadata.GlobalCommands.play.ToString())))
                                    zMusic.Play();
                                break;
                        }
                    }
                    break;


                case Metadata.GlobalCommands.streammp3:
                case Metadata.GlobalCommands.radio:
                    //check if is stream cmd
                    if ((m_mainZoneActivity != null) && (m_mainZoneActivity.GetType() != typeof(ZoneStreamVLC)))
                    {
                        m_mainZoneActivity.Close();
                        m_mainZoneActivity = null;
                    }
                    if (m_mainZoneActivity == null)
                        InitZoneStreamVLC();

                    if (!vals.CommandSource.Equals(Metadata.CommandSources.Web) || (action != null && action.Equals(Metadata.GlobalCommands.play.ToString())))
                        m_mainZoneActivity.Play();
                    break;
                case Metadata.GlobalCommands.dvd:
                    //check if Microphone

                    if ((m_mainZoneActivity != null) && (m_mainZoneActivity.GetType() != typeof(ZoneMic)))
                    {
                        m_mainZoneActivity.Close();
                        m_mainZoneActivity = null;
                    }
                    if (m_mainZoneActivity == null)
                        InitZoneMic();
                    m_mainZoneActivity.Play();
                    break;

                //check if video
                case Metadata.GlobalCommands.video:
                    if ((m_mainZoneActivity != null) && (m_mainZoneActivity.GetType() != typeof(ZoneVideoMPC)))
                    {
                        m_mainZoneActivity.Close();
                        m_mainZoneActivity = null;
                    }

                    if (m_mainZoneActivity == null)
                        InitZoneVideoApp();
                    break;

                //check if photo (clone zone)
                case Metadata.GlobalCommands.photo:
                case Metadata.GlobalCommands.musicclone:
                    if (!this.Equals(ControlCenter.GetFirstZoneMusic()))//cannot close myself
                    {
                        if ((m_mainZoneActivity != null) && (m_mainZoneActivity.GetType() != typeof(ZoneMusicClone)))
                        {
                            m_mainZoneActivity.Close();
                            m_mainZoneActivity = null;
                        }

                        if (m_mainZoneActivity == null)
                            InitZoneMusicClone();
                    }
                    break;


                //check if TV
                case Metadata.GlobalCommands.tv:
                    if ((m_mainZoneActivity != null) && (m_mainZoneActivity.GetType() != typeof(ZoneDisplayLG)))
                    {
                        m_mainZoneActivity.Close();
                        m_mainZoneActivity = null;
                    }

                    if (m_mainZoneActivity == null)
                        InitZoneDisplayTV();

                    if (!vals.CommandSource.Equals(Metadata.CommandSources.Web) || (action != null && action.Equals(Metadata.GlobalCommands.play.ToString())))
                        m_mainZoneActivity.Play();
                    break;
            }

            #endregion

            if (m_mainZoneActivity == null)
            {
                MLog.Log(this, "Null activity for cmd=" + cmdRemote + " zone=" + m_zoneDetails.ZoneName);
                return result;
            }
            #region Generic commands
            switch (cmdRemote)
            {
                case Metadata.GlobalCommands.play:
                    m_mainZoneActivity.Play();
                    break;
                case Metadata.GlobalCommands.stop:
                    m_mainZoneActivity.Stop();
                    //foreach (m_
                    break;
                case Metadata.GlobalCommands.pause:
                    m_mainZoneActivity.Pause();
                    break;
                case Metadata.GlobalCommands.next:
                    m_mainZoneActivity.Next();
                    break;
                case Metadata.GlobalCommands.previous:
                    m_mainZoneActivity.Previous();
                    break;
                case Metadata.GlobalCommands.right:
                    m_mainZoneActivity.NextPlaylist();
                    break;
                case Metadata.GlobalCommands.left:
                    m_mainZoneActivity.PreviousPlaylist();
                    break;
                case Metadata.GlobalCommands.mute:
                    m_mainZoneActivity.Mute();
                    break;
                case Metadata.GlobalCommands.chup:
                case Metadata.GlobalCommands.volumeup:
                    m_mainZoneActivity.VolumeUp();
                    break;
                case Metadata.GlobalCommands.chdown:
                case Metadata.GlobalCommands.volumedown:
                    m_mainZoneActivity.VolumeDown();
                    break;
                case Metadata.GlobalCommands.volumeset:
                    m_mainZoneActivity.SetVolumeLevel(Convert.ToInt16(vals.GetValue(Metadata.GlobalParams.volumelevel)));
                    break;
                case Metadata.GlobalCommands.guide:
                    m_mainZoneActivity.Guide();
                    break;
                
                //specific commands
                default:
                    //specific commands only to Music
                    if (m_mainZoneActivity.GetType() == typeof(ZoneMusic))
                    {
                        ZoneMusic zMusic = ((ZoneMusic)m_mainZoneActivity);

                        int rating;
                        switch (cmdRemote)
                        {
                            case Metadata.GlobalCommands.enter://for numpads
                                zMusic.Next();
                                break;
                            case Metadata.GlobalCommands.ffwd:
                                zMusic.Ffwd();
                                break;
                            case Metadata.GlobalCommands.rewind:
                                zMusic.Rewind();
                                break;
                            case Metadata.GlobalCommands.record:
                                if (m_zoneUser.Id != "0")
                                {
                                    //ControlCenter.PlayInfoMessage("Deleting current file", this);
                                    zMusic.DeleteCurrentFile();
                                }
                                break;
                            case Metadata.GlobalCommands.right:
                                NextPlaylist();
                                break;
                            case Metadata.GlobalCommands.repeat:
                                zMusic.SwitchPlayMode();
                                ShowPlayList();
                                break;
                            case Metadata.GlobalCommands.left:
                                zMusic.PreviousPlaylist();
                                break;
                            case Metadata.GlobalCommands.up:
                                rating = zMusic.RatingUp();
                                //ControlCenter.PlayInfoMessage("Rating Up " + rating, this);
                                break;
                            case Metadata.GlobalCommands.down:
                                rating = zMusic.RatingDown();
                                //ControlCenter.PlayInfoMessage("Rating Down " + rating, this);
                                break;
                            case Metadata.GlobalCommands.ratingset:
                                zMusic.SetRating(Convert.ToInt16(vals.GetValue(Metadata.GlobalParams.ratingvalue)));
                                break;
                            case Metadata.GlobalCommands.setgenrelist:
                                zMusic.SetGenreList(vals);
                                ShowPlayList();
                                break;
                            case Metadata.GlobalCommands.setartistlist:
                                zMusic.SetArtistList(vals);
                                ShowPlayList();
                                break;
                            case Metadata.GlobalCommands.medialist:
                                result = zMusic.GetSongValueList();
                                break;
                            case Metadata.GlobalCommands.setmediaitem:
                                zMusic.Play(Convert.ToInt16(vals.IndexValueList[0]));
                                break;
                            case Metadata.GlobalCommands.getmoodmusiclist:
                                result = zMusic.GetMoodValueList();
                                break;
                            case Metadata.GlobalCommands.setmoodmusic:
                                zMusic.SetMood(MZPState.Instance.MoodMusicList.Find(x => x.Index.ToString().Equals(vals.IndexValueList[0])));
                                zMusic.Play();
                                ShowPlayList();
                                break;
                            case Metadata.GlobalCommands.searchmediaitem:
                                zMusic.Search(vals.GetValue(Metadata.GlobalParams.searchvalue));
                                ShowPlayList();
                                break;
                            case Metadata.GlobalCommands.followmemusic:
                                MZPState.Instance.ToogleFollowMeMusic();
                                break;
                            case Metadata.GlobalCommands.back:
                            case Metadata.GlobalCommands.holdcriteria:
                                if (vals.GetValue(Metadata.GlobalParams.action) != null)
                                    zMusic.SetHoldCriteria(vals.GetValue(Metadata.GlobalParams.action).ToLower());
                                else
                                    zMusic.HoldCriteriaToggle();
                                ShowPlayList();
                                break;
                        }
                    }

                    //specific commands only to internettream
                    if (m_mainZoneActivity.GetType() == typeof(ZoneStreamVLC))
                    {
                        ZoneStreamVLC zStream = (ZoneStreamVLC)m_mainZoneActivity;

                        switch (cmdRemote)
                        {
                            case Metadata.GlobalCommands.enter://for numpads
                                zStream.Next();
                                break;
                        }
                    }

                    //specific commands only to TV
                    if (m_mainZoneActivity.GetType() == typeof(ZoneDisplayLG))
                    {
                        ((ZoneDisplayLG)m_mainZoneActivity).ProcessAction(cmdRemote, vals);
                    }

                    //specific commands only to Video
                    if (m_mainZoneActivity.GetType() == typeof(ZoneVideoMPC))
                    {
                        ZoneVideoMPC zVideo = (ZoneVideoMPC)m_mainZoneActivity;
                        switch (cmdRemote)
                        {
                            case Metadata.GlobalCommands.fullscreen:
                                zVideo.GoToFullScreen();
                                break;
                            case Metadata.GlobalCommands.down:
                                zVideo.VideoSelectionDown();
                                break;
                            case Metadata.GlobalCommands.up:
                                zVideo.VideoSelectionUp();
                                break;
                            case Metadata.GlobalCommands.right:
                                zVideo.NextSearchCriteria();
                                break;
                            case Metadata.GlobalCommands.left:
                                zVideo.PreviousSearchCriteria();
                                break;
                            case Metadata.GlobalCommands.enter:
                                zVideo.ToggleSearchCriteria();
                                break;
                            case Metadata.GlobalCommands.ffwd:
                                zVideo.Ffwd();
                                break;
                            case Metadata.GlobalCommands.rewind:
                                zVideo.Rewind();
                                break;
                        }
                    }
                    break;
            }

            #endregion

            this.m_currentCmd = cmdRemote.ToString();
            m_mainZoneActivity.Tick();//update zone details after command
            RefreshState();
            return result;
        }

        public IZoneActivity GetCurrentActivity()
        {
            return m_mainZoneActivity;
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

        public List<IZoneActivity> GetClonedZones()
        {
            return m_clonedZones;
            //return m_zoneDetails.OutputDevice;
        }

        public void AddClonedZone(IZoneActivity zone)
        {
            m_clonedZones.Add(zone);
        }

        public void RemoveClonedZone(IZoneActivity zone)
        {
            m_clonedZones.Remove(zone);
        }

        public Users GetUser()
        {
            return m_zoneUser;
        }

        public void SetSourceZone(int zoneId)
        {
            m_sourceZoneId = zoneId;
        }

        public int GetVolumeLevel()
        {
            if (m_mainZoneActivity != null)
                return m_mainZoneActivity.GetVolumeLevel();
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
            m_mainZoneActivity.Next();
            RefreshState();
        }

        public void NextPlaylist()
        {
            ((ZoneMusic)m_mainZoneActivity).NextPlaylist();
            ShowPlayList();
        }

        private void RefreshState()
        {
            try
            {
                txtOutputDevice.Text = m_zoneDetails.OutputDeviceAutoCompleted;
                txtUser.Text = m_zoneUser.Name;
                txtInactiveCycles.Text = m_inactiveCyclesCount.ToString()+"/" + IniFile.ZONE_INACTIVITY_MAX_CYCLES;
                txtControlDevice.Text = m_controlDevice;
                txtCurrentCmd.Text = m_currentCmd;
                txtZoneName.Text = m_zoneDetails.ZoneId + ":"+m_zoneDetails.ZoneName;
                txSleepTimer.Text = m_zoneDetails.SleepHourMin;
                txWakeTime.Text = m_zoneDetails.WakeTime;
                txAlarm.Text = m_zoneDetails.MovementAlert + ":" + m_zoneDetails.LastAlarmMovementDateTime;

                if (m_mainZoneActivity != null)
                {
                    txGenPosition.Text = m_mainZoneActivity.Position.ToString();
                    txPositionPercent.Text = m_mainZoneActivity.PositionPercent.ToString();
                    txtVolumeLevel.Text = m_mainZoneActivity.GetVolumeLevel().ToString();
                    txtMusicStatus.Text = m_mainZoneActivity.GetState().ToString();

                    m_zoneDetails.Position = m_mainZoneActivity.Position;
                    m_zoneDetails.PositionPercent = m_mainZoneActivity.PositionPercent;
                    m_zoneDetails.VolumeLevel = m_mainZoneActivity.GetVolumeLevel();
                    m_zoneDetails.ZoneState = m_mainZoneActivity.GetState();

                    if (m_mainZoneActivity.GetType() == typeof(ZoneMusic))
                    {
                        ZoneMusic zMusic = (ZoneMusic)m_mainZoneActivity;

                        tabZone.SelectedTab = tabZone.TabPages[0];
                        txtMusicFile.Text = zMusic.GetMusicFileDescription();
                        txtPlaylist.Text = m_zoneDetails.Playlist;
                        txtPlayMode.Text = zMusic.GetPlayMode().ToString();
                        txRating.Text = zMusic.CurrentItem.Rating.ToString();
                        txCount.Text = zMusic.GetSongList().Count.ToString();

                        //show playlist index
                        String currentSong = zMusic.CurrentItem.SourceURL;
                        int playIndex;
                        List<MediaItem> defaultSongList = zMusic.GetSongList();
                        playIndex = zMusic.GetHashKey(defaultSongList, currentSong);
                        dgvPlayList.CurrentCell = dgvPlayList.Rows[playIndex].Cells[0];
                    }

                    if (m_mainZoneActivity.GetType() == typeof(ZoneRadio))
                    {
                        ZoneRadio radio = (ZoneRadio)m_mainZoneActivity;
                        tabZone.SelectedTab = tabZone.TabPages[1];
                        txtInputDevice.Text = radio.GetInputDeviceName();
                        txtExtProcess.Text = radio.GetExtProcessStatus();
                        txtAppHandle.Text = radio.GetAppHandle().ToString() + " active = " + radio.IsActive();
                    }

                    if (m_mainZoneActivity.GetType() == typeof(ZoneStreamVLC))
                    {
                        ZoneStreamVLC stream= (ZoneStreamVLC)m_mainZoneActivity;
                        tabZone.SelectedTab = tabZone.TabPages[5];
                        textBoxStreamingUrl.Text = stream.GetCurrentURL();
                        lbVolume.Text = stream.GetVolumeLevelNative().ToString();
                        lbRadio.Text = stream.ZoneDetails.Author;
                        lbstreamtitle.Text = stream.ZoneDetails.Title;
                    }

                    if ((m_mainZoneActivity.GetType() == typeof(ZoneVideoMPC)) || (m_mainZoneActivity.GetType() == typeof(ZoneVideoVlc)))
                    {
                        ZoneVideoBase video = (ZoneVideoBase)m_mainZoneActivity;
                        tabZone.SelectedTab = tabZone.TabPages[2];
                    }

                    if (m_mainZoneActivity.GetType() == typeof(ZoneDisplayLG))
                    {
                        ZoneDisplayLG tv= (ZoneDisplayLG)m_mainZoneActivity;
                        tabZone.SelectedTab = tabZone.TabPages[6];
                        txtInput.Text = tv.InputType;
                    }
                }
            }
            catch (Exception)
            {
                //MLog.Log(null,"Error zone form refreshstate " + ex.Message + ex.StackTrace);
            }
        }

        private void ShowPlayList()
        {
            dgvPlayList.Rows.Clear();
            List<MediaItem> defaultSongList;

            defaultSongList = ((ZoneMusic)m_mainZoneActivity).GetSongList();
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

        private void LoadZoneIni()
        {
            m_controlDevice = MZPState.Instance.GetControlDeviceByZone(m_zoneDetails.ZoneId);
        }

        private void ZonesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseZone();
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

        public void Tick(object sender, EventArgs e)
        {
            try
            {
                if (m_mainZoneActivity != null)
                {
                    if (!m_mainZoneActivity.IsActive())
                    {
                        m_inactiveCyclesCount++;
                    }
                    m_mainZoneActivity.Tick();
                }
                else
                    m_inactiveCyclesCount++;

                RefreshState();
                CheckForSleep();

                if (m_inactiveCyclesCount == IniFile.ZONE_INACTIVITY_CYCLE_WARNING)
                {
                    //play warning message
                }

                if (m_inactiveCyclesCount > IniFile.ZONE_INACTIVITY_MAX_CYCLES)
                {
                    this.Close();
                }

                //close if no recent activity detected
                if (m_zoneDetails.HasPastActivity && m_zoneDetails.IsActive && !m_zoneDetails.ActivityType.Equals(Metadata.GlobalCommands.nul))//HasPastMove)
                {
                    MLog.Log(this, "Zone " + m_zoneDetails.ZoneName + " closed due to inactivity, activity="+m_zoneDetails.ActivityType.ToString());
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MLog.Log(ex, this, "Error zone timer");
            }
            
        }

        /*public void PlayBeepOK()
        {
            int currentVolume = Metadata.VolumeLevels.VolumeSilence / 2;

            if ((m_mainZoneActivity!=null) && (m_mainZoneActivity.GetType() == typeof(ZoneMusic)))
            {
                currentVolume = Convert.ToInt16((Metadata.VolumeLevels.VolumeSilence - (Metadata.VolumeLevels.VolumeSilence - m_mainZoneActivity.GetVolumeLevel()) / 1.2));
            }
            ZoneMusic zMusic = new ZoneMusic(this, m_outputDevice, currentVolume, IniFile.CurrentPath()+IniFile.BEEP_OK_FILE_NAME);
        }

        public void PlayBeepError()
        {
            int currentVolume = Metadata.VolumeLevels.VolumeSilence / 2;

            if ((m_mainZoneActivity!=null)&&(m_mainZoneActivity.GetType() == typeof(ZoneMusic)))
            {
                currentVolume = Convert.ToInt16((Metadata.VolumeLevels.VolumeSilence - (Metadata.VolumeLevels.VolumeSilence - m_mainZoneActivity.GetVolumeLevel()) / 1.2));
            }
            ZoneMusic zMusic = new ZoneMusic(this, m_outputDevice, currentVolume, IniFile.CurrentPath() + IniFile.BEEP_ERROR_FILE_NAME);
        }*/

        

        private void button1_Click(object sender, EventArgs e)
        {
            if (m_mainZoneActivity.GetType() == typeof(ZoneRadio))
            {
                ZoneRadio radio = (ZoneRadio)m_mainZoneActivity;
                radio.Next();
            }
        }

       

        private void btRepeat_Click(object sender, EventArgs e)
        {
            API.DoCommandFromGUIInput(((Button)sender).Tag.ToString().ToLower(), GetZoneId().ToString());
        }

        private void dgvPlayList_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            ZoneMusic music = (ZoneMusic) m_mainZoneActivity;
            int ind = Convert.ToInt16(dgvPlayList.Rows[e.RowIndex].Cells[Music_Index.Name].Value);//todo change index

            if (ind >=0)
                music.Play(ind);
        }

        private void txWakeTime_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Metadata.ValueList vals = new Metadata.ValueList(Metadata.GlobalParams.command, Metadata.GlobalCommands.setwaketimer.ToString(), Metadata.CommandSources.GUI);
            vals.Add(Metadata.GlobalParams.datetime, txWakeTime.Text);
            vals.Add(Metadata.GlobalParams.weekday, "WK");
            ProcessAction(Metadata.GlobalCommands.setwaketimer, vals);
        }

        private void txSleepTimer_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            m_zoneDetails.SleepHourMin = txSleepTimer.Text;
        }

        private void cmbMoods_SelectedIndexChanged(object sender, EventArgs e)
        {
            Metadata.ValueList vals = new Metadata.ValueList(Metadata.GlobalParams.zoneid, m_zoneDetails.ZoneId.ToString(), Metadata.CommandSources.GUI);
            vals.Add(Metadata.GlobalParams.command, Metadata.GlobalCommands.setmoodmusic.ToString());
            vals.Add(Metadata.GlobalParams.selectedindex, MZPState.Instance.MoodMusicList.Find(x=>x.Name.Equals(cmbMoods.Text)).Index.ToString());
            API.DoCommandFromGUIInput(vals);
        }

        private void btClearTags_Click(object sender, EventArgs e)
        {
            ZoneMusic music = (ZoneMusic)m_mainZoneActivity;
            music.ClearUsageTags();
            ShowPlayList();
            RefreshState();
        }

        private void label25_Click(object sender, EventArgs e)
        {

        }
    }

    public interface IPlayEvent
    {
        void EventNextAuto();
        int GetZoneId();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;


namespace MultiZonePlayer
{
    public class ZoneGeneric : IPlayEvent
    {

        private Users m_zoneUser = null;

        public Users ZoneUser
        {
            get { return m_zoneUser; }
            set { m_zoneUser = value; }
        }
        private String m_currentCmd = null;

        public String CurrentCmd
        {
            get { return m_currentCmd; }
            set { m_currentCmd = value; }
        }
        private int m_sourceZoneId;
        private int m_inactiveCyclesCount = 0;

        public int InactiveCyclesCount
        {
            get { return m_inactiveCyclesCount; }
            set { m_inactiveCyclesCount = value; }
        }
        private IZoneActivity m_mainZoneActivity;

        public IZoneActivity MainZoneActivity
        {
            get { return m_mainZoneActivity; }
            set { m_mainZoneActivity = value; }
        }
        private String m_controlDevice = null;

        public String ControlDevice
        {
            get { return m_controlDevice; }
            set { m_controlDevice = value; }
        }
        private Metadata.ZoneDetails m_zoneDetails;
        private List<IZoneActivity> m_clonedZones;
        private static int m_recIndex = 0;
        private ZonesForm m_zoneForm;

        public Metadata.ZoneDetails ZoneDetails
        {
            get { return m_zoneDetails; }
        }

        public ZonesForm ZoneForm
        {
            get { return m_zoneForm; }
            set { m_zoneForm = value; }
        }

        public ZoneGeneric(int zoneId)
        {
            m_zoneDetails = MZPState.Instance.GetZoneById(zoneId);
            this.m_sourceZoneId = zoneId;

            m_clonedZones = new List<IZoneActivity>();
            //default user - all
            m_zoneUser = new Users("0","all","000");//SystemState.iniUserList["000"] as Users;
            LoadZoneIni();
        }

        

        public void CloseZone()
        {
            try
            {
                //MLog.Log(null, "Closing zone " + m_zoneDetails.ZoneId + ", activity=" + m_mainZoneActivity);
                if (m_mainZoneActivity != null)
                {
                    m_mainZoneActivity.Close();
                }

                //controlCenter.ZoneClosing(m_zoneDetails.ZoneId);

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

                m_zoneDetails.Close();
                MZPState.Instance.ActiveZones.Remove(this);
            }
            catch (Exception ex)
            {
                MLog.Log(ex, "Error closing generic zone " + m_zoneDetails.ZoneName);
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

        private void InitZonePlayerXBMC()
        {
            MLog.Log(null, "Initialising XBMC");
            //ControlCenter.Instance.OpenZone(m_zoneDetails.ZoneId);
            //m_mainZoneActivity = new ZoneVideoVlc(this);
            m_mainZoneActivity = new ZonePlayerXBMC(this.ZoneDetails);
            m_zoneDetails.ActivityType = Metadata.GlobalCommands.xbmc;
            m_zoneDetails.IsActive = true;
        }
        
        public Metadata.ValueList ProcessAction(Metadata.GlobalCommands cmdRemote, Metadata.ValueList vals)
        {
            Metadata.ValueList result = new Metadata.ValueList();
            String cmdSource = vals.GetValue(Metadata.GlobalParams.cmdsource);
            Metadata.CommandSources cmdSourceEnum;
            if (cmdSource == null)
            {
                cmdSourceEnum = vals.CommandSource;
            }
            else
            {
                cmdSourceEnum = (Metadata.CommandSources)Enum.Parse(typeof(Metadata.CommandSources), cmdSource);
            }

            //reset inactivity counter
            m_inactiveCyclesCount = -1;

            m_zoneDetails.LastLocalCommandDateTime = DateTime.Now;

            String date,weekday,action;

            #region commands without activity
            switch (cmdRemote)
            {
                case Metadata.GlobalCommands.cameraevent://video camera event
                    MZPState.Instance.ZoneEvents.AddCamAlert(vals);
                    if (m_zoneDetails.IsArmed || 
                        (MZPState.Instance.SystemAlarm.AreaState.Equals(Alarm.EnumAreaState.armed)&&(m_zoneDetails.AlarmAreaId==MZPState.Instance.SystemAlarm.AreaId)))
                    {
                        String msg =  "CAM event on " + m_zoneDetails.ZoneName + " when zone armed";
                        MZPState.Instance.LogEvent(MZPEvent.EventSource.Cam, msg, MZPEvent.EventType.Security, MZPEvent.EventImportance.Critical);
                    }
                    else
                        MLog.Log(this, "Ignoring cam event on " + m_zoneDetails.ZoneName + " movementalert=" + m_zoneDetails.MovementAlert + " zonealarmareaid=" + m_zoneDetails.AlarmAreaId
                                + " systemareaid=" + MZPState.Instance.SystemAlarm.AreaId + " areastate=" + MZPState.Instance.SystemAlarm.AreaState);

                    if (MZPState.Instance.IsFollowMeMusic & m_zoneDetails.HasSpeakers)
                    {
                        cmdRemote = Metadata.GlobalCommands.musicclone;
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
                    if (m_zoneDetails.MovementAlert && (m_zoneDetails.IsArmed ||
                        (MZPState.Instance.SystemAlarm.AreaState.Equals(Alarm.EnumAreaState.armed) && (m_zoneDetails.AlarmAreaId == MZPState.Instance.SystemAlarm.AreaId))))
                    {
                        String msg = "ALARM event on " + m_zoneDetails.ZoneName + " when zone armed";
                        MZPState.Instance.LogEvent(MZPEvent.EventSource.Alarm, msg, MZPEvent.EventType.Security, MZPEvent.EventImportance.Critical);
                    }
                    else
                        MLog.Log(this, "Ignoring alarm event on " + m_zoneDetails.ZoneName + " movementalert=" + m_zoneDetails.MovementAlert + " zonealarmareaid=" + m_zoneDetails.AlarmAreaId
                                + " systemareaid=" + MZPState.Instance.SystemAlarm.AreaId + " areastate=" + MZPState.Instance.SystemAlarm.AreaState);
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
                case Metadata.GlobalCommands.powercycle:
                    MZPState.Instance.PowerControl.PowerOn(m_zoneDetails.ZoneId);
                    Thread.Sleep(1000);
                    MZPState.Instance.PowerControl.PowerOff(m_zoneDetails.ZoneId);
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
                                    if (mood != null) zMusic.SetMood(mood);
                                    m_mainZoneActivity.Play();
                                }
                                break;
                            case Metadata.GlobalCommands.music://play once opened unless cmd is from web or explicit play
                                if (!cmdSourceEnum.Equals(Metadata.CommandSources.web) || (action != null && action.Equals(Metadata.GlobalCommands.play.ToString())))
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

                    if (!vals.CommandSource.Equals(Metadata.CommandSources.web) || (action != null && action.Equals(Metadata.GlobalCommands.play.ToString())))
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
                    if (!this.Equals(ControlCenter.GetFirstZoneMusic()))//cannot clone myself
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

                    if (action != null && action.Equals(Metadata.GlobalCommands.play.ToString()))
                        m_mainZoneActivity.Play();
                    break;

                //check if XBMC
                case Metadata.GlobalCommands.xbmc:
                    if ((m_mainZoneActivity != null) && (m_mainZoneActivity.GetType() != typeof(ZonePlayerXBMC)))
                    {
                        m_mainZoneActivity.Close();
                        m_mainZoneActivity = null;
                    }

                    if (m_mainZoneActivity == null)
                        InitZonePlayerXBMC();

                    if (action != null && action.Equals(Metadata.GlobalCommands.play.ToString()))
                        m_mainZoneActivity.Play();
                    break;
            }

            #endregion

            if (m_mainZoneActivity == null)
            {
                //MLog.Log(this, "Null activity for cmd=" + cmdRemote + " zone=" + m_zoneDetails.ZoneName);
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
                    if (cmdSourceEnum.Equals(Metadata.CommandSources.mobileslow))
                    {
                        m_mainZoneActivity.VolumeUp();
                    }
                    break;
                case Metadata.GlobalCommands.chdown:
                case Metadata.GlobalCommands.volumedown:
                    m_mainZoneActivity.VolumeDown();
                    if (cmdSourceEnum.Equals(Metadata.CommandSources.mobileslow))
                    {
                        m_mainZoneActivity.VolumeDown();
                    }
                    break;
                case Metadata.GlobalCommands.volumeset:
                    m_mainZoneActivity.SetVolumeLevel(Convert.ToInt16(vals.GetValue(Metadata.GlobalParams.volumelevel)));
                    break;
                case Metadata.GlobalCommands.guide:
                    m_mainZoneActivity.Guide();
                    break;
                
                //specific commands
                default:
                    //specific commands only to Music / Clone
                    if (m_mainZoneActivity.GetType() == typeof(ZoneMusic))
                    {
                        ZoneMusic zMusic = ((ZoneMusic)m_mainZoneActivity);

                        int rating;
                        switch (cmdRemote)
                        {
                            case Metadata.GlobalCommands.enter://for numpads
                                m_mainZoneActivity.Next();
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
                                break;
                            case Metadata.GlobalCommands.left:
                                zMusic.PreviousPlaylist();
                                break;
                            case Metadata.GlobalCommands.up:
                                rating = zMusic.RatingUp();
                                break;
                            case Metadata.GlobalCommands.down:
                                rating = zMusic.RatingDown();
                                break;
                            case Metadata.GlobalCommands.ratingset:
                                zMusic.SetRating(Convert.ToInt16(vals.GetValue(Metadata.GlobalParams.ratingvalue)));
                                break;
                            case Metadata.GlobalCommands.setgenrelist:
                                zMusic.SetGenreList(vals);
                                break;
                            case Metadata.GlobalCommands.setartistlist:
                                zMusic.SetArtistList(vals);
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
                                break;
                            case Metadata.GlobalCommands.searchmediaitem:
                                zMusic.Search(vals.GetValue(Metadata.GlobalParams.searchvalue));
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
                                break;
                        }
                    }

                    //music clone commands
                    if (m_mainZoneActivity.GetType() == typeof(ZoneMusicClone))
                    {
                        ZoneMusicClone zMusic = ((ZoneMusicClone)m_mainZoneActivity);

                        int rating;
                        switch (cmdRemote)
                        {
                            case Metadata.GlobalCommands.enter://for numpads
                                m_mainZoneActivity.Next();
                                break;
                            case Metadata.GlobalCommands.right:
                                NextPlaylist();
                                break;
                            case Metadata.GlobalCommands.left:
                                zMusic.PreviousPlaylist();
                                break;
                            case Metadata.GlobalCommands.up:
                                rating = zMusic.RatingUp();
                                break;
                            case Metadata.GlobalCommands.down:
                                rating = zMusic.RatingDown();
                                break;
                            case Metadata.GlobalCommands.followmemusic:
                                MZPState.Instance.ToogleFollowMeMusic();
                                break;
                            case Metadata.GlobalCommands.back:
                            case Metadata.GlobalCommands.holdcriteria:
                                zMusic.HoldCriteriaToggle();
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
            //RefreshState();
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
        }

        public void NextPlaylist()
        {
            ((ZoneMusic)m_mainZoneActivity).NextPlaylist();
        }

        private void LoadZoneIni()
        {
            m_controlDevice = MZPState.Instance.GetControlDeviceByZone(m_zoneDetails.ZoneId);
        }

        private void CheckForSleep()
        {
            if (m_zoneDetails.SleepHourMin.Equals(DateTime.Now.ToString(IniFile.DATETIME_FORMAT)))
            {
                MLog.Log(this, "Zone sleeping");
                if (m_zoneForm != null) m_zoneForm.Close();
                //m_zoneDetails.SleepHourMin = "";
            }
        }

        public void Tick()
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
                    m_zoneDetails.Position = MainZoneActivity.Position;
                    m_zoneDetails.PositionPercent = MainZoneActivity.PositionPercent;
                    m_zoneDetails.VolumeLevel = MainZoneActivity.GetVolumeLevel();
                    m_zoneDetails.ZoneState = MainZoneActivity.GetState();
                }
                else
                    m_inactiveCyclesCount++;

                CheckForSleep();

                if (m_inactiveCyclesCount == IniFile.ZONE_INACTIVITY_CYCLE_WARNING)
                {
                    //play warning message
                }

                if (m_inactiveCyclesCount > IniFile.ZONE_INACTIVITY_MAX_CYCLES)
                {
                    if (m_zoneForm != null) m_zoneForm.CloseFormSafe();
                }

                //close if no recent activity detected
                if (m_zoneDetails.HasPastActivity && m_zoneDetails.IsActive && !m_zoneDetails.ActivityType.Equals(Metadata.GlobalCommands.nul))//HasPastMove)
                {
                    MLog.Log(this, "Zone " + m_zoneDetails.ZoneName + " closed due to inactivity, activity="+m_zoneDetails.ActivityType.ToString());
                    if (m_zoneForm != null) m_zoneForm.CloseFormSafe();
                }

               
                if (m_zoneForm != null) m_zoneForm.RefreshState();
            }
            catch (Exception ex)
            {
                MLog.Log(ex, this, "Error zone timer");
            }
            
        }
    }


}


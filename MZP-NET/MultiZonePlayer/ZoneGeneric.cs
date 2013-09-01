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
        private List<Metadata.ZoneDetails> m_clonedZones;
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

			m_clonedZones = new List<Metadata.ZoneDetails>();

            //default user - all
            m_zoneUser = new Users("0","all","000");//SystemState.iniUserList["000"] as Users;
            LoadZoneIni();
        }

        public void CloseZone()
        {
            try
			{
				MLog.Log(null, "Closing zone " + m_zoneDetails.ZoneId + ", activity=" + m_mainZoneActivity);
				if (m_mainZoneActivity != null)
				{
					m_mainZoneActivity.Close();
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
			ZoneGeneric zone;
            foreach (Metadata.ZoneDetails izone in m_clonedZones)
            {
				zone = MZPState.Instance.ActiveZones.Find(x => x.ZoneDetails.ZoneId == izone.ZoneId);
				if (zone!=null && zone.MainZoneActivity != null) 
					zone.MainZoneActivity.Stop();
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
                
                AddClonedZone(m_mainZoneActivity.ZoneDetails);
                
            }
            else MLog.Log(this, "Initialising music not needed, already init");

        }

        private void InitZoneMusicClone()
        {
            MLog.Log(null, "Initialising music clone");
            ///ControlCenter.Instance.OpenZone(m_zoneDetails.ZoneId);
            StopRemoveClonedZones();

			ZoneGeneric clonesource = MZPState.Instance.ActiveZones.Find
				(x => x.ZoneDetails.ActivityType == Metadata.GlobalCommands.musicclone);
            if (clonesource != null)
				m_mainZoneActivity = new ZoneMusicClone(this, ((ZoneMusicClone)clonesource.MainZoneActivity).CloneSourceZone);
            else
				m_mainZoneActivity = new ZoneMusicClone(this, MZPState.Instance.GetFirstZoneMusic());
            m_zoneDetails.ActivityType = Metadata.GlobalCommands.musicclone;
            m_zoneDetails.IsActive = true;
        }

        private void InitZoneRadio()
        {
            MLog.Log(null,"Initialising radio");
            // input zone 0 is Radio
            //ControlCenter.Instance.OpenZone(m_zoneDetails.ZoneId);
            String inDevice = (String)MZPState.Instance.zoneDefaultInputs["0"];
            m_mainZoneActivity = new ZoneRadio(this, inDevice, m_zoneDetails.OutputDeviceAutoCompleted());
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
            m_mainZoneActivity = new ZoneMic(this, inDevice, m_zoneDetails.OutputDeviceAutoCompleted());
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
            MLog.Log(null, "Initialising XBMC zone");
            //ControlCenter.Instance.OpenZone(m_zoneDetails.ZoneId);
            //m_mainZoneActivity = new ZoneVideoVlc(this);
			if (m_zoneDetails.DisplayConnection != "")
			{
				m_mainZoneActivity = new ZonePlayerXBMC(this.ZoneDetails);
				m_zoneDetails.ActivityType = Metadata.GlobalCommands.xbmc;
				m_zoneDetails.IsActive = true;
			}
			else
				MLog.Log(this, "no display connection details, zone not initialised " + m_zoneDetails.ZoneName);
        }
        
        public void ProcessAction(Metadata.GlobalCommands cmdRemote, Metadata.ValueList vals, ref Metadata.CommandResult cmdresult)
        {
            //Metadata.ValueList result = new Metadata.ValueList();
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

            
            switch (cmdRemote)
            {
                #region commands without activity
                case Metadata.GlobalCommands.cameraevent://video camera event
                    MZPState.Instance.ZoneEvents.AddCamAlert(vals);
                    String camId = vals.GetValue(Metadata.GlobalParams.oid);
                    string message = "Cam alert from camid=" + camId + " zone is " + m_zoneDetails.ZoneName;
                    m_zoneDetails.MovementAlert = true;
                    MZPState.Instance.LogEvent(MZPEvent.EventSource.Cam, message, 
						MZPEvent.EventType.Security, MZPEvent.EventImportance.Informative, m_zoneDetails);
					ZoneOpenActions();
					
					m_zoneDetails.MovementAlert = false;
                   
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
                    DateTime eventDateTime = Convert.ToDateTime(vals.GetValue(Metadata.GlobalParams.datetime));
                    m_zoneDetails.LastAlarmMovementDateTime = eventDateTime;

                    MZPState.Instance.LogEvent(eventDateTime, MZPEvent.EventSource.Alarm,
                            vals.GetValue(Metadata.GlobalParams.action) + " ZoneEvent " + m_zoneDetails.ZoneName + " is " + vals.GetValue(Metadata.GlobalParams.status),
                            MZPEvent.EventType.Security, MZPEvent.EventImportance.Informative, m_zoneDetails);
					ZoneOpenActions();
					
                    
                    if (MZPState.Instance.IsFollowMeMusic & m_zoneDetails.HasSpeakers)
                    {
                        cmdRemote = Metadata.GlobalCommands.musicclone;
                    }
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
                        m_zoneDetails.SleepHourMin = Convert.ToDateTime(date).ToString(IniFile.DATETIME_DAYHR_FORMAT);
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
					cmdresult.OutputMessage += "Zone " + m_zoneDetails.ZoneName + " armed status=" + m_zoneDetails.IsArmed;
                    break;
                case Metadata.GlobalCommands.powercycle:
                    MZPState.Instance.PowerControl.PowerOn(m_zoneDetails.ZoneId);
                    Thread.Sleep(Convert.ToInt16(vals.GetValue(Metadata.GlobalParams.interval)));
                    MZPState.Instance.PowerControl.PowerOff(m_zoneDetails.ZoneId);
                    break;
				case Metadata.GlobalCommands.closure:
					IoEvent(m_zoneDetails, vals.GetValue(Metadata.GlobalParams.id),
						vals.GetValue(Metadata.GlobalParams.iscontactmade).ToLower()=="true");
					break;
				case Metadata.GlobalCommands.closurearm:
				case Metadata.GlobalCommands.closuredisarm:	
					m_zoneDetails.IsClosureArmed = (cmdRemote == Metadata.GlobalCommands.closurearm);
					cmdresult.OutputMessage += "Zone " + m_zoneDetails.ZoneName + " closure armed status=" + m_zoneDetails.IsClosureArmed;
					break;
                case Metadata.GlobalCommands.notifyuser:
					bool needsPower = m_zoneDetails.RequirePower;
					m_zoneDetails.RequirePower = true;
					if (!MZPState.Instance.PowerControl.IsPowerOn(m_zoneDetails.ZoneId))
					{
						MZPState.Instance.PowerControl.PowerOn(m_zoneDetails.ZoneId);
						System.Threading.Thread.Sleep(m_zoneDetails.PowerOnDelay);//ensure we can hear this
					}
					string sourcezoneid = vals.GetValue(Metadata.GlobalParams.sourcezoneid);
					string file = IniFile.CurrentPath()+ IniFile.PARAM_NOTIFYUSER_SOUND_FILE[1].Replace("x",sourcezoneid);
					if (!System.IO.File.Exists(file))
						file = IniFile.CurrentPath()+IniFile.PARAM_NOTIFYUSER_SOUND_FILE[1];
					DCPlayer tempPlay = new DCPlayer(this, file, m_zoneDetails.GetDefaultNotifyUserVolume());
					//System.Threading.Thread.Sleep(4000);//ensure we can hear this
					m_zoneDetails.RequirePower = needsPower;
					break;
				#endregion
                default:
                    
                    action = vals.GetValue(Metadata.GlobalParams.action);
                    ZoneMusic zMusic;
                    switch (cmdRemote)
                    {
                        #region Zone init commands
                        case Metadata.GlobalCommands.music:
                        case Metadata.GlobalCommands.musicalarm:
                            //if cmd is music, if not on Music already stop current activity and init Music
                            if ((m_mainZoneActivity != null) && (m_mainZoneActivity.GetType() != typeof(ZoneMusic)))
                            {
                                m_mainZoneActivity.Close();
                                m_mainZoneActivity = null;
                            }

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
                                    case Metadata.GlobalCommands.music://play once opened unless is playing, cmd is from web or explicit play
                                        if (zMusic.GetState() != Metadata.ZoneState.Running
                                            && (!cmdSourceEnum.Equals(Metadata.CommandSources.web) || (action != null && action.Equals(Metadata.GlobalCommands.play.ToString()))))
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
                        /*
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
                        break;*/

                        //check if video
                        /*
                    case Metadata.GlobalCommands.video:
                        if ((m_mainZoneActivity != null) && (m_mainZoneActivity.GetType() != typeof(ZoneVideoMPC)))
                        {
                            m_mainZoneActivity.Close();
                            m_mainZoneActivity = null;
                        }

                        if (m_mainZoneActivity == null)
                            InitZoneVideoApp();
                        break;
                        */
                        //check if photo (clone zone)
                        case Metadata.GlobalCommands.photo:
						case Metadata.GlobalCommands.back:
                        case Metadata.GlobalCommands.musicclone:
							if (MZPState.Instance.GetFirstZoneMusic()!=null && !this.Equals(MZPState.Instance.GetFirstZoneMusic()))//cannot clone myself
							{
								if ((m_mainZoneActivity != null) && (m_mainZoneActivity.GetType() != typeof(ZoneMusicClone)))
								{
									m_mainZoneActivity.Close();
									m_mainZoneActivity = null;
								}

								if (m_mainZoneActivity == null)
									InitZoneMusicClone();
							}
							else MLog.Log(this, "Cannot clone myself or no existing zone to clone");
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
                        case Metadata.GlobalCommands.video:
                            if ((m_mainZoneActivity != null) && (m_mainZoneActivity.GetType() != typeof(ZonePlayerXBMC)))
                            {
                                m_mainZoneActivity.Close();
                                m_mainZoneActivity = null;
                            }

                            if (m_mainZoneActivity == null || !m_mainZoneActivity.ZoneDetails.IsActive)
                                InitZonePlayerXBMC();

                            if (action != null && action.Equals(Metadata.GlobalCommands.play.ToString()))
                                m_mainZoneActivity.Play();
                            break;
                        default:
                            if (m_mainZoneActivity == null)
                            {
								cmdresult.OutputMessage+= "No suitable zoneactivity for cmd=" + cmdRemote + " zone=" + m_zoneDetails.ZoneName;
								MLog.Log(this, cmdresult.OutputMessage);
								return;
                            }

                        #endregion 

                        switch (cmdRemote)
                        {
                            #region Generic commands
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
                            #endregion

                            default:
                                #region specific commands
                                //specific commands only to Music / Clone
                                if (m_mainZoneActivity.GetType() == typeof(ZoneMusic))
                                {
                                    zMusic = ((ZoneMusic)m_mainZoneActivity);
                                    zMusic.ProcessAction(cmdRemote, vals, ref cmdresult);
                                }

                                //music clone commands
                                if (m_mainZoneActivity.GetType() == typeof(ZoneMusicClone))
                                {
                                    ZoneMusicClone zMusicClone = ((ZoneMusicClone)m_mainZoneActivity);
                                    zMusicClone.ProcessAction(cmdRemote, vals, ref cmdresult);
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
                                        case Metadata.GlobalCommands.right:
                                            m_mainZoneActivity.NextPlaylist();
                                            break;
                                        case Metadata.GlobalCommands.left:
                                            m_mainZoneActivity.PreviousPlaylist();
                                            break;
                                    }
                                }

                                //specific commands only to TV
                                if (m_mainZoneActivity.GetType() == typeof(ZoneDisplayLG))
                                {
                                    ((ZoneDisplayLG)m_mainZoneActivity).ProcessAction(cmdRemote, vals, ref cmdresult);
                                }

                                //specific commands only to XBMC
                                if (m_mainZoneActivity.GetType() == typeof(ZonePlayerXBMC))
                                {
                                    ((ZonePlayerXBMC)m_mainZoneActivity).ProcessAction(cmdRemote, vals, ref cmdresult);
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
                                #endregion specific commands
                        }
                        break;
                    }
                    break;
            }
            
            this.m_currentCmd = cmdRemote.ToString();
			if (m_mainZoneActivity != null)
				m_mainZoneActivity.Tick();//update zone details after command
				//RefreshState();
            return;
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

		public List<Metadata.ZoneDetails> GetClonedZones()
        {
            return m_clonedZones;
            //return m_zoneDetails.OutputDevice;
        }

        public void AddClonedZone(Metadata.ZoneDetails zone)
        {
            m_clonedZones.Add(zone);
        }

        public void RemoveClonedZone(Metadata.ZoneDetails zone)
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

       
        public void EventNextAuto()
        {
            m_mainZoneActivity.Next();
        }

        private void LoadZoneIni()
        {
            m_controlDevice = MZPState.Instance.GetControlDeviceByZone(m_zoneDetails.ZoneId);
        }

        private void CheckForSleep()
        {
            if (m_zoneDetails.SleepHourMin.Equals(DateTime.Now.ToString(IniFile.DATETIME_DAYHR_FORMAT)))
            {
                MLog.Log(this, "Zone sleeping");
                if (m_zoneForm != null) m_zoneForm.Close();
                //m_zoneDetails.SleepHourMin = "";
            }
        }

		private void ZoneOpenActions()
		{
			if ((m_zoneDetails.NotifyZoneEventTriggered == Metadata.ZoneNotifyState.Closed)
				|| (DateTime.Now.Subtract(m_zoneDetails.LastNotifyZoneEventTriggered).TotalHours>1))
			{
				Utilities.RunProcessWait(IniFile.CurrentPath() + "\\zone-open-" + m_zoneDetails.ZoneId + ".bat",
					System.Diagnostics.ProcessWindowStyle.Hidden);
				m_zoneDetails.NotifyZoneEventTriggered = Metadata.ZoneNotifyState.Open;
				m_zoneDetails.LastNotifyZoneEventTriggered = DateTime.Now;
			}
		}

		public static void ZoneInactiveActions(Metadata.ZoneDetails p_zoneDetails)
		{
			if (p_zoneDetails.NotifyZoneEventTriggered == Metadata.ZoneNotifyState.Open)
			{
				Utilities.RunProcessWait(IniFile.CurrentPath() + "\\zone-close-" + p_zoneDetails.ZoneId + ".bat",
					System.Diagnostics.ProcessWindowStyle.Hidden);
				p_zoneDetails.NotifyZoneEventTriggered = Metadata.ZoneNotifyState.Closed;
				p_zoneDetails.LastNotifyZoneEventTriggered = DateTime.Now;
			}
		}

		public void IoEvent(Metadata.ZoneDetails zone, string key, Boolean isKeyDown)// KeyDetail kd)
		{
			//if (zone.ZoneId == 15)
			//	MLog.Log(null, "debug");
			Metadata.ClosureOpenCloseRelay lastState = zone.ClosureOpenCloseRelay;
			if (lastState == null)
			{
				lastState = new Metadata.ClosureOpenCloseRelay(isKeyDown);
				zone.ClosureOpenCloseRelay = lastState;
			}
			else
			{
				if (zone.ClosureOpenCloseRelay.RelayType != Metadata.ClosureOpenCloseRelay.EnumRelayType.Button)
					if (lastState.RelayState == zone.ClosureOpenCloseRelay.GetRelayState(isKeyDown))
						return;//return if state does not change, for closures, not buttons
				lastState.RelayContactMade = isKeyDown;
			}
			zone.ClosureCounts++;
			zone.MovementAlert = true;
			zone.LastClosureEventDateTime = DateTime.Now;
			string message = "Closure state " + key + " is " + lastState.RelayState + " on zone " + zone.ZoneName;
			MLog.Log(null, message);
			Utilities.AppendToCsvFile(IniFile.CSV_CLOSURES, ",", zone.ZoneName, key, DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT), lastState.RelayState.ToString());
			if (lastState.RelayState == Metadata.ClosureOpenCloseRelay.EnumState.ContactClosed)
			{
				MZPState.Instance.LogEvent(MZPEvent.EventSource.Closure, message, MZPEvent.EventType.Security,
					MZPEvent.EventImportance.Informative, zone);
			}
			zone.MovementAlert = false;
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

                if (m_inactiveCyclesCount > IniFile.ZONE_INACTIVITY_MAX_CYCLES)
                {
                    if (m_zoneForm != null) 
						m_zoneForm.CloseFormSafe();
                }

                //close if no recent activity detected on an active zone
                if (m_zoneDetails.HasPastActivity && m_zoneDetails.IsActive 
					&& !m_zoneDetails.ActivityType.Equals(Metadata.GlobalCommands.nul)
					&& !m_zoneDetails.ActivityType.Equals(Metadata.GlobalCommands.xbmc))
                {
                    MLog.Log(this, "Zone " + m_zoneDetails.ZoneName + 
						" closed due to inactivity, activity="+m_zoneDetails.ActivityType.ToString());
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


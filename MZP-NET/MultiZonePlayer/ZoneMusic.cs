﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace MultiZonePlayer
{
    public enum PlayMode
    {
        Default1, 
        Random2/*,
          AZOrder3*/
    };

    public enum HoldCriteria
    {
        nothing,
        artist,
        folder,
        album,
        genre,
        rating
    }

    public class ZoneMusic : ZoneBase, IZoneActivity
    {
        private DCPlayer m_dcPlay = null;
        private ZoneGeneric m_zoneForm = null;
        private String m_currentPlaylistName;
        private int m_currentSongKey = 0;
        private int m_indexPlaylist = 0;
        private int m_indexMood = 0;
        private List<AudioItem> m_songList = null;
        private List<IZoneActivity> m_cloneMusicActivityList = new List<IZoneActivity>();
        private HoldCriteria m_holdCriteria = HoldCriteria.nothing;
           
        private PlayMode m_playMode = PlayMode.Default1;
        private Boolean m_isAlarm = false;
        private String m_alarmStartMinute = "";
        private bool m_isGuideMode = false;
        private string m_numericCmd = "";

            public ZoneMusic(ZoneGeneric zoneForm)
            {
                this.m_zoneForm = zoneForm;
                m_zoneDetails = zoneForm.ZoneDetails;

                m_dcPlay = new DCPlayer();
                SetVolumeLevel(zoneForm.ZoneDetails.GetDefaultVolume());
                LoadPlaylist(m_currentPlaylistName);//, currentSongPath);

                MZPState.Instance.LogEvent(MZPEvent.EventSource.GUI, "Music Started in zone " + m_zoneDetails.ZoneName, 
                    MZPEvent.EventType.Functionality, MZPEvent.EventImportance.Informative, m_zoneDetails);
            }

            // used to play beeps and other notification signals
            public ZoneMusic(ZoneGeneric p_zoneForm, String p_fileName)
            {
                m_zoneForm = p_zoneForm;
                m_zoneDetails = p_zoneForm.ZoneDetails;
				Play(p_fileName);
                
            }

            private void LoadPlaylist(String playListName)//, String fileNameToResume)
            {
                MLog.Log(this,"Loading playlist name=" + playListName);// + " file=" + fileNameToResume);

                LoadDefaultPlaylist();

                //search resume position in playlist
                if ((playListName != null) && (playListName.Length!=0))
                {
                    for (int i = 0; i < MZPState.Instance.GetAPlayList(m_zoneForm.GetZoneId()).Count; i++)
                    {
                        if (MZPState.Instance.GetAPlayList(m_zoneForm.GetZoneId())[i.ToString()].ToString() == playListName)
                        {
                            m_indexPlaylist = i;
                            break;
                        }
                    }
                }

                if (m_songList == null)
                {
                    //ControlCenter.PlayErrorMessage("Unable to find a playlist", m_zoneForm);
                    m_currentPlaylistName = "No files in playlist";
                }
                else
                {
                    if (playListName != null)
                        this.m_currentPlaylistName = playListName;
                    
                    m_currentSongKey = 0;
                }

                m_zoneForm.ZoneDetails.Playlist = m_currentPlaylistName;
            }

            private void LoadDefaultPlaylist()
            {
                MoodMusic mood = MZPState.Instance.GetScheduledMood(m_zoneDetails.ZoneId);
                if (mood != null)
                {
                    MLog.Log(this, "Setting scheduled mood " + mood.Name);
                    SetMood(mood);
                }
                else
                {
                    mood = MZPState.Instance.MoodMusicList.Find(x => x.Name.Equals(IniFile.PLAYLIST_DEFAULT));
                    if (mood != null)
                    {
                        SetMood(mood);
                    }
                    else
                    {
                        m_songList = MediaLibrary.AllAudioFiles.PlaylistItems;
                        m_currentPlaylistName = "AllAudioFiles - default not exists";
                    }
                }

                if (m_songList != null && m_songList.Count == 0)
                {
                    m_songList = MediaLibrary.AllAudioFiles.PlaylistItems;
                    m_currentPlaylistName = "AllAudioFiles - empty playlist";
                }

                m_zoneDetails.PlaylistCount = m_songList.Count;
            }

        private void LoadPlaylistByCode(string code)
        {
            MoodMusic mood = MZPState.Instance.MoodMusicList.Find(x => x.NumericCode==code);
            if (mood != null)
            {
                MLog.Log(this, "Set playlist by numeric code=" + code + " found match=" + mood.Name);
                SetMood(mood);
                Play();
            }
            else
                MLog.Log(this, "Set playlist by numeric code=" + code + " found no match");
        }
        private void SetPlayMode(PlayMode playMode)
        {
            this.m_playMode = playMode;
            switch (playMode)
            {
                case PlayMode.Default1:
                    m_songList = m_songList.OrderBy(x => x.PlayCount).ToList();
                    break;
                case PlayMode.Random2:
                    m_songList = m_songList.OrderBy(x => x.PlayCount).ThenBy(x => x.RandomId).ToList();
                    break;
            }
        }

            #region Commands

            public void SwitchPlayMode()
            {
                String currentSong = CurrentItem.SourceURL;
                switch (m_playMode)
                {
                    case PlayMode.Default1:
                        SetPlayMode(PlayMode.Random2);
                        //currentSongKey = GetHashKey(randomisedSongList, currentSong);
                        break;

                    case PlayMode.Random2:
                        SetPlayMode(PlayMode.Default1);
                        //currentSongKey = GetHashKey(defaultArrangementSongList, currentSong);
                        break;
                }
                //locate new index
            }

			public override void Play()
            {
                String musicFile;

                if (m_dcPlay.GetState() == Metadata.ZoneState.Running)
                    Stop();
                if (CurrentItem != null)
                {
                    musicFile = CurrentItem.SourceURL;

                    if (File.Exists(musicFile))
                    {
                        //m_currentPlayList.MediaItemValueChange(musicFile, WindowsMediaItem.keyPlayCount, 1, 0, int.MaxValue);
						MLog.Log(this, "Playing file" + CurrentItem.SourceURL + " playcount=" + CurrentItem.PlayCount + " vol="+m_dcPlay.GetVolumeLevel());
                        //init amp power if needed
						m_zoneDetails.RequirePower = true;
						m_zoneDetails.IsActive = true;
                        MZPState.Instance.PowerControlOn(m_zoneDetails.ZoneId);
						int loop = 0;
						while (!m_zoneDetails.HasOutputDeviceAvailable() && loop <50)
						{
							MLog.Log(this, "Waiting, device not yet available for zone " + m_zoneDetails.ZoneName);
							System.Threading.Thread.Sleep(500);
							loop++;
						}
						if (loop >= 50)
						{
							MLog.Log(this, "Error, NO device available for zone " + m_zoneDetails.ZoneName);
							m_zoneDetails.IsActive = false;
						}
						else
						{
							m_dcPlay.OpenClip(musicFile, this.m_zoneForm);
							if (m_dcPlay.GetState() == Metadata.ZoneState.Running)
							{
								m_zoneDetails.ZoneState = Metadata.ZoneState.Running;
							}
							m_songList[m_currentSongKey].IncreasePlayCount();
						}
                    }
                    else
                    {
                        MLog.Log(this, "At Play, File does not exist, name=" + musicFile);
                        Next();
                    }
                }
                else
                    MLog.Log(this, "Error no current item to play");
                Tick();//update details
            }

            //media index, not the zero based index
            public void Play(int mediaIndex)
            {
                m_currentSongKey = m_songList.FindIndex(x => x.Index.Equals(mediaIndex));//index;
				if (m_currentSongKey != -1)
				{
					m_dcPlay.StopClip();
					Play();
				}
				else MLog.Log(this, "Invalid index on Play, i=" + mediaIndex);
            }

            public void Play(String filePathName)
            {
				Stop();
                int volume = m_zoneForm.GetVolumeLevel();
				m_zoneDetails.RequirePower = true;
                MZPState.Instance.PowerControlOn(m_zoneForm.ZoneDetails.ZoneId);
				System.Threading.Thread.Sleep(4000);//ensure we can hear this
                DCPlayer tempPlay = new DCPlayer(m_zoneForm, IniFile.CurrentPath()+ filePathName, volume);
            }

            public void UpdateOutputDevices()
            {
                if (GetState().Equals(Metadata.ZoneState.Running))
                    m_dcPlay.UpdateOutputDevices();
            }

			public override void Stop()
            {
                SaveStateIni();
                //dcPlay.StopClip();
                m_dcPlay.CloseClip();
				m_zoneDetails.ZoneStop();
                m_isGuideMode = false;
            }

            delegate void DelegateClose();
			public override void Close()
            {
                Stop();
				if (m_dcPlay.InvokeRequired)
				{
					DelegateClose dlg = new DelegateClose(Close);
					m_dcPlay.BeginInvoke(dlg);
				}
				else
				{
					m_dcPlay.Close();
					m_zoneDetails.ZoneClose();
				}
            }

            public void DeleteCurrentFile()
            {
                if (GetState() == Metadata.ZoneState.Paused)
                {
                    String currentFile = CurrentItem.SourceURL;
                    Next();
                    Utilities.MoveFile(currentFile, IniFile.PARAM_FOLDER_DELETED_ITEMS[1] + System.IO.Path.GetFileName(currentFile), true);
                }
            }

            public void ClearUsageTags()
            {
                foreach (AudioItem me in m_songList)
                {
                    me.SetPlayCount(0);
                    me.SetRating(0);
                }
            }

            public override void Pause()
            {
				base.Pause();
                m_dcPlay.PauseClip();
            }

			public override void Mute()
            {
                m_dcPlay.ToggleMute();
            }

			public override void Guide()
            {
                m_isGuideMode = !m_isGuideMode;
                m_numericCmd = "";
            }

            public void Ffwd()
            {
                m_dcPlay.ModifyRate(+0.25);
            }

            public void Rewind()
            {
                m_dcPlay.ModifyRate(-0.25);
            }

			public override void Next()
            {
                AudioItem currentItem = m_songList[m_currentSongKey];
                Stop();
                if (m_songList != null)
                {
                    m_currentSongKey++;
                    if (m_currentSongKey == m_songList.Count)
                        m_currentSongKey = 0;
                    if (m_songList.Count > 0)
                        Play();
                    else
                        MLog.Log(this, "No songs in songlist, not going next");
                }
            }

			public override void NextMood()
            {
                List<MoodMusic> moodList = MZPState.Instance.GetScheduledMoodList(m_zoneDetails.ZoneId);
                MLog.Log(this, "At next mood cmd moodcount=" + moodList.Count + " current index="+m_indexMood);
                if (moodList.Count > m_indexMood + 1)
                    m_indexMood++;
                else
                    m_indexMood = 0;
                SetMood(moodList[m_indexMood]);
                Play();
            }

			public override void PreviousMood()
            {
                List<MoodMusic> moodList = MZPState.Instance.GetScheduledMoodList(m_zoneDetails.ZoneId);
                MLog.Log(this, "At previous mood cmd moodcount=" + moodList.Count + " current index=" + m_indexMood);
                if (m_indexMood > 0)
                    m_indexMood--;
                else
                    m_indexMood = moodList.Count - 1;
                SetMood(moodList[m_indexMood]);
                Play();
            }

			public override void NextPlaylist()
            {
                /*
                Stop();
                m_indexPlaylist++;

                if (m_indexPlaylist >= MZPState.Instance.GetAPlayList(m_zoneForm.GetZoneId()).Count)
                    m_indexPlaylist = 0;

                m_currentSongKey = 0;
                LoadPlaylist(MZPState.Instance.GetAPlayList(m_zoneForm.GetZoneId())[m_indexPlaylist.ToString()].ToString());
                Play();
                 * */
                NextMood();
            }

			public override void PreviousPlaylist()
            {
                /*Stop();
                m_indexPlaylist--;

                if (m_indexPlaylist < 0)
                    m_indexPlaylist = MZPState.Instance.GetAPlayList(m_zoneForm.GetZoneId()).Count - 1;

                m_currentSongKey = 0;
                LoadPlaylist(MZPState.Instance.GetAPlayList(m_zoneForm.GetZoneId())[m_indexPlaylist.ToString()].ToString());
                Play();*/
                PreviousMood();
            }

			public override void Previous()
            {
                AudioItem currentItem = m_songList[m_currentSongKey];
                Stop();
                m_currentSongKey--;
                if (m_currentSongKey < 0)
                    m_currentSongKey = m_songList.Count-1;
                if (m_songList.Count > 0)
                    Play();
                else
                    MLog.Log(this, "No songs in songlist, not going previous");
                MediaLibrary.AllAudioFiles.SaveUpdatedItems();
            }

			public override void VolumeUp()
            {
                m_dcPlay.ChangeVolume(IniFile.VOLUME_STEP);
            }

			public override void VolumeDown()
            {
                m_dcPlay.ChangeVolume(-IniFile.VOLUME_STEP);
            }

            public int RatingUp()
            {
                return CurrentItem.UpdateRating(1, 0, 5);
            }

            public int RatingDown()
            {
                return CurrentItem.UpdateRating(-1, 0, 5);
            }

            public void SetRating(int ratingLevel)
            {
                CurrentItem.SetRating(ratingLevel);
            }

			public override void SaveStateIni()
            {
                //IniFile.IniWriteValue(IniFile.INI_SECTION_ZONERESUME + m_zoneForm.GetZoneId() + "USR" + m_zoneForm.GetUser().Id, "PLAYLIST", GetPlayList());
                //IniFile.IniWriteValue(IniFile.INI_SECTION_ZONERESUME + zoneForm.GetZoneId() + "USR" +zoneForm.GetUser().Id, "PLAYFILE", GetCurrentMusicFileFullPath());
                //IniFile.IniWriteValue(IniFile.INI_SECTION_ZONERESUME + zoneForm.GetZoneId() + "USR" +zoneForm.GetUser().Id, "VOLUME", GetVolumeLevel().ToString());
            }

			public override long Position
            {
                get
                {
                    return m_dcPlay.Position;
                }
            }
			public override int PositionPercent
            {
                get
                {
                    return m_dcPlay.PositionPercent;
                }
            }

            #endregion

            #region GetSet
            public bool IsAlarm
            {
                get {
                    if (m_isAlarm)
                    {
                        if (!DateTime.Now.ToString("mm").Equals(m_alarmStartMinute))
                        {
                            m_isAlarm = false;
                        }
                    }
                    return m_isAlarm; 
                    }
                set { 
                    m_isAlarm = value;
                    m_alarmStartMinute = DateTime.Now.ToString("mm");
                    if (value)
                        SetVolumeLevel(m_zoneDetails.GetDefaultAlarmVolume());
                }
            }
           

            public AudioItem CurrentItem
            {
                get {
                    if (m_songList != null && m_songList.Count > 0)
                        return m_songList[m_currentSongKey];
                    else
                        return null;
                }
            }

            public void SetGenreList(Metadata.ValueList vals, string genre)
            {
                Stop();
				if (genre == null)
					m_songList = MediaLibrary.AudioFilesByGenre(vals);
				else
					m_songList = MediaLibrary.AudioFilesByGenre(genre);
                SetPlayMode(m_playMode);
                m_currentSongKey = 0;
                Play();
            }
			

            public void SetArtistList(Metadata.ValueList vals, string artist)
            {
                Stop();
				if (artist == null)
					m_songList = MediaLibrary.AudioFilesByArtist(vals);
				else
					m_songList = MediaLibrary.AudioFilesByArtist(artist);
                SetPlayMode(m_playMode);
                m_currentSongKey = 0;
                Play();
            }
            
            public void HoldCriteriaToggle()
            {
                Stop();
                bool match = false;
                foreach (HoldCriteria crit in Enum.GetValues(typeof(HoldCriteria)))
                {
                    if (m_holdCriteria == crit)
                        match = true;
                    else
                        if (match)
                        {
                            m_holdCriteria = crit;
                            match = false;
                            break;
                        }
                }
                if (match == true) m_holdCriteria = HoldCriteria.nothing;
                SetHoldCriteria(m_holdCriteria);
            }

        public void SetHoldCriteria(String criteria)
        {
            if (Enum.IsDefined(typeof(HoldCriteria), criteria))
                SetHoldCriteria((HoldCriteria)Enum.Parse(typeof(HoldCriteria), criteria));
            else
                MLog.Log(this, "error set hold criteria, crit not defined =" + criteria);
        }

        public void SetHoldCriteria(HoldCriteria holdCriteria)
        {
            m_holdCriteria = holdCriteria;
            switch(m_holdCriteria)
            {
                case HoldCriteria.nothing:
                    //m_songList = MediaLibrary.AllAudioFiles.PlaylistItems;
                    LoadDefaultPlaylist();
                    break;
                case HoldCriteria.artist:
                    if (!CurrentItem.Author.Trim().Equals(""))
                        m_songList = MediaLibrary.AudioFilesByArtist(CurrentItem.Author);
                    break;
                case HoldCriteria.album:
                    if (!CurrentItem.Album.Trim().Equals(""))
                        m_songList = MediaLibrary.AudioFilesByAlbum(CurrentItem.Album);
                    break;
                case HoldCriteria.genre:
                    if (!CurrentItem.Genre.Trim().Equals(""))
                        m_songList = MediaLibrary.AudioFilesByGenre(CurrentItem.Genre);
                    break;
                case HoldCriteria.folder:
                    m_songList = MediaLibrary.AudioFilesByFolder(CurrentItem.SourceContainerURL);
                    break;
                case HoldCriteria.rating:
                    m_songList = MediaLibrary.AudioFilesByRating(CurrentItem.Rating);
                    break;
            }
            MLog.Log(this, "Hold criteria is now " + m_holdCriteria.ToString());
            m_currentSongKey = 0;
            SetPlayMode(m_playMode);
            //Play();
        }

            public void SetMood(MoodMusic mood)
            {
                Stop();
                m_songList = MediaLibrary.GetMoodPlaylist(mood);
                if (m_songList == null || m_songList.Count == 0)
                {
                    mood = MZPState.Instance.MoodMusicList.Find(x => x.Name.Equals(IniFile.PLAYLIST_DEFAULT));
                    m_songList = MediaLibrary.GetMoodPlaylist(mood);
                    MLog.Log(this, "No songs in mood found, swithcing to default");
                }
                m_currentSongKey = 0;
                m_currentPlaylistName = mood.Name;
                SetPlayMode(mood.IsRandom ? PlayMode.Random2 : PlayMode.Default1);
            }

            public void Search(String key)
            {
                var res = MediaLibrary.AllAudioFiles.PlaylistItems.Where(i => i.Title.ToLower().Contains(key.ToLower())).ToList();
                res = res.Union(MediaLibrary.AllAudioFiles.PlaylistItems.Where(i => i.Genre.ToLower().Contains(key.ToLower()))).ToList();
                res = res.Union(MediaLibrary.AllAudioFiles.PlaylistItems.Where(i => i.Album.ToLower().Contains(key.ToLower()))).ToList();
                res = res.Union(MediaLibrary.AllAudioFiles.PlaylistItems.Where(i => i.Author.ToLower().Contains(key.ToLower()))).Distinct().OrderBy(x=>x.PlayCount).ThenBy(x=>x.RandomId).ToList();
                m_songList = res;
                SetPlayMode(m_playMode);
                m_currentSongKey = 0;
                Play();
            }

            public override int GetVolumeLevel()
            {
                return m_dcPlay.GetVolumeLevel();
            }

			public override void SetVolumeLevel(int volume)
            {
                m_dcPlay.SetVolume(volume);
            }

            public String GetMusicFileDescription()
            {
                String musicFile = m_dcPlay.GetFileName();
                
                if (musicFile == null)
                    musicFile = m_currentSongKey + ": No file loaded";
                return m_currentSongKey +": "+ musicFile;
            }

            public PlayMode GetPlayMode()
            {
                return m_playMode;
            }

            public String GetNextMusicFileDescription()
            {
                int nextIndex;

                if (m_currentSongKey == m_songList.Count-1)
                    nextIndex = 0;
                else 
                    nextIndex= m_currentSongKey+1;

                return nextIndex + ":"+ m_songList[nextIndex].SourceURL;
            }

            public String GetPreviousMusicFileDescription()
            {
                int prevIndex;
                if (m_currentSongKey == 0)
                    prevIndex = m_songList.Count-1;
                else
                    prevIndex = m_currentSongKey-1;

                return prevIndex + ":"+ m_songList[prevIndex].SourceURL;
            }

			public override Metadata.ZoneState GetState()
            {
                return m_dcPlay.GetState();
            }

			public override bool IsActive()
            {
                return (m_dcPlay.GetState() == Metadata.ZoneState.Running);
            }

            private void SetPlayList(String playlist)
            {
                this.m_currentPlaylistName = playlist;
            }

            /*public String GetPlayList()
            {
                return currentPlaylistName;
            }*/

            public List<AudioItem> GetSongList()
            {
                return m_songList;
            }

            public Metadata.ValueList GetSongValueList()
            {
                Metadata.ValueList val = new Metadata.ValueList(Metadata.CommandSources.system);
                var titles = m_songList.Select(i => i.Author+" | "+i.Title).ToList();
                //val.Values = titles.ToList();
                var indexes = m_songList.Select(i => i.Index.ToString()).ToList();
                //val.Keys = indexes.ToList();
                val.SetBulk(indexes.ToList(), titles.ToList());
                return val;
            }

            public Metadata.ValueList GetMoodValueList()
            {
                Metadata.ValueList val = new Metadata.ValueList(Metadata.CommandSources.system);
                var names = MZPState.Instance.MoodMusicList.Select(m => m.Name);
                var indexes = MZPState.Instance.MoodMusicList.Select(m => m.Index.ToString());
                val.SetBulk(indexes.ToList(), names.ToList());
                return val;
            }


            public int GetHashKey(List<AudioItem> list, String currentSong)
            {
                int index;

                for (index = 0; index < list.Count; index++)
                {
                    if (list[index].SourceURL.Equals(currentSong))
                    {
                        break;
                    }
                }
                return index;
            }

            public List<AudioItem> GetRandomisedSongList()
            {
                return m_songList;
            }

            public int GetPlaylistPosition()
            {
                return m_currentSongKey;
            }
            #endregion
            
        private void NumericCmdReceived(string key)
        {
            if (m_isGuideMode)
            {
                m_numericCmd += key;
                if (m_numericCmd.Length == 2)
                {
                    m_isGuideMode = false;
                    LoadPlaylistByCode(m_numericCmd);
                    m_numericCmd = "";
                }
            }
        }

            public Metadata.ValueList ProcessAction(Metadata.GlobalCommands cmdRemote, Metadata.ValueList vals, ref Metadata.CommandResult cmdresult)
            {
                Metadata.ValueList result = new Metadata.ValueList();
                String action = action = vals.GetValue(Metadata.GlobalParams.action);
                int rating;               
                switch (cmdRemote)
                {
                    case Metadata.GlobalCommands.k0:
                    case Metadata.GlobalCommands.k1:
                    case Metadata.GlobalCommands.k2:
                    case Metadata.GlobalCommands.k3:
                    case Metadata.GlobalCommands.k4:
                    case Metadata.GlobalCommands.k5:
                    case Metadata.GlobalCommands.k6:
                    case Metadata.GlobalCommands.k7:
                    case Metadata.GlobalCommands.k8:
                    case Metadata.GlobalCommands.k9:
                        NumericCmdReceived(cmdRemote.ToString().Substring(cmdRemote.ToString().Length - 1));
                        break;
                    case Metadata.GlobalCommands.right:
                        NextPlaylist();
                        break;
                    case Metadata.GlobalCommands.left:
                        PreviousPlaylist();
                        break;
                    case Metadata.GlobalCommands.enter://for numpads
                        Next();
                        break;
                    case Metadata.GlobalCommands.ffwd:
                        Ffwd();
                        break;
                    case Metadata.GlobalCommands.rewind:
                        Rewind();
                        break;
                    case Metadata.GlobalCommands.record:
                        /*if (m_zoneUser.Id != "0")
                        {
                            //ControlCenter.PlayInfoMessage("Deleting current file", this);
                            DeleteCurrentFile();
                        }*/
                        break;
                    case Metadata.GlobalCommands.repeat:
                        SwitchPlayMode();
                        break;
                    case Metadata.GlobalCommands.up:
                        rating = RatingUp();
                        break;
                    case Metadata.GlobalCommands.down:
                        rating = RatingDown();
                        break;
                    case Metadata.GlobalCommands.ratingset:
                        SetRating(Convert.ToInt16(vals.GetValue(Metadata.GlobalParams.ratingvalue)));
                        break;
                    case Metadata.GlobalCommands.setgenrelist:
						SetGenreList(vals, vals.GetValue(Metadata.GlobalParams.singleparamvalue));
                        break;
                    case Metadata.GlobalCommands.setartistlist:
						SetArtistList(vals, vals.GetValue(Metadata.GlobalParams.singleparamvalue));
                        break;
                    case Metadata.GlobalCommands.medialist:
                        result = GetSongValueList();
                        break;
                    case Metadata.GlobalCommands.setmediaitem:
						string keyword = vals.GetValue(Metadata.GlobalParams.singleparamvalue);
						if (keyword != null)
						{
							keyword = keyword.ToLower();
							int songIndex = m_songList.FindIndex(x=>x.Title.ToLower().Contains(keyword)
								|| x.SourceURL.ToLower().Contains(keyword)
								|| x.Album.ToLower().Contains(keyword));
							if (songIndex != -1)
							{
								m_currentSongKey = songIndex;
								Play();
								cmdresult.OutputMessage += " Media found:" + CurrentItem.SourceURL;
							}
							else 
								cmdresult.ErrorMessage += " No media found for keyword:" + keyword;
						}
						else
							Play(Convert.ToInt16(vals.IndexValueList[0]));
                        break;
                    case Metadata.GlobalCommands.getmoodmusiclist:
                        result = GetMoodValueList();
                        break;
                    case Metadata.GlobalCommands.setmoodmusic:
						if (vals.GetValue(Metadata.GlobalParams.moodindex) != null)
							SetMood(MZPState.Instance.MoodMusicList.Find(x => x.Index.ToString().Equals(vals.GetValue(Metadata.GlobalParams.moodindex))));
						else
						{
							if (vals.ContainsKey(Metadata.GlobalParams.singleparamvalue))
								vals.Add(Metadata.GlobalParams.moodname, vals.GetValue(Metadata.GlobalParams.singleparamvalue));
							if (vals.GetValue(Metadata.GlobalParams.moodname) != null)
								SetMood(MZPState.Instance.MoodMusicList.Find(x => x.Name.Equals(vals.GetValue(Metadata.GlobalParams.moodname))));
						}
                        Play();
                        break;
                    case Metadata.GlobalCommands.searchmediaitem:
						if (vals.ContainsKey(Metadata.GlobalParams.searchvalue))
							Search(vals.GetValue(Metadata.GlobalParams.searchvalue));
						else
							Search(vals.GetValue(Metadata.GlobalParams.singleparamvalue));
                        break;
                    case Metadata.GlobalCommands.followmemusic:
                        MZPState.Instance.ToogleFollowMeMusic();
                        break;
                    //case Metadata.GlobalCommands.back:
                    case Metadata.GlobalCommands.holdcriteria:
                        if (vals.GetValue(Metadata.GlobalParams.action) != null)
                            SetHoldCriteria(vals.GetValue(Metadata.GlobalParams.action).ToLower());
                        else
                            HoldCriteriaToggle();
                        break;
					//case Metadata.GlobalCommands.notifyuser:
					//	Play(IniFile.PARAM_NOTIFYUSER_SOUND_FILE[1]);
					//	break;
                    default:
                        MLog.Log(this, "WARNING, unprocessed zone command " + cmdRemote);
                        break;
                }
                
                return result;
            }
			public override void Tick()
            {
                if (IsAlarm && GetVolumeLevel() < m_zoneForm.ZoneDetails.GetDefaultVolume())
                {
                    SetVolumeLevel(GetVolumeLevel() + 100);
					MLog.Log(this, "Setting alarm volume on " + m_zoneDetails.ZoneName + " vol="+GetVolumeLevel());
                }

                if (CurrentItem != null)
                {
                    m_zoneDetails.Title = CurrentItem.Title;
                    m_zoneDetails.Rating = CurrentItem.Rating;
                    m_zoneDetails.Playcount = CurrentItem.PlayCount;
                    m_zoneDetails.Author = CurrentItem.Author;
                    m_zoneDetails.Genre = CurrentItem.Genre;
                    m_zoneDetails.Year = CurrentItem.Year;
                    m_zoneDetails.Album = CurrentItem.Album;
                    m_zoneDetails.SourceURL = CurrentItem.SourceURL;
                    m_zoneDetails.Playlist = m_currentPlaylistName;
                    m_zoneDetails.PlaylistCount = m_songList.Count;
                }
                if (m_dcPlay!=null) m_dcPlay.Tick();
            }
    }

    public class ZoneMusicClone : ZoneBase
    {
        private ZoneGeneric m_parentZoneForm;
		private ZoneGeneric m_cloneSourceZone;

		public ZoneGeneric CloneSourceZone
		{
			get { return m_cloneSourceZone; }
			
		}
        private ZoneMusic m_clonedZoneMusic;
        private Metadata.ZoneState m_zoneState;

        public ZoneMusicClone(ZoneGeneric p_zoneForm, ZoneGeneric p_cloneSource)
        {
            m_cloneSourceZone = p_cloneSource;
            m_parentZoneForm = p_zoneForm;
            m_zoneState = Metadata.ZoneState.NotInitialised;
            m_zoneDetails = p_zoneForm.ZoneDetails;

            if (m_cloneSourceZone != null)
            {
				m_cloneSourceZone.MainZoneActivity.SetVolumeLevel(m_cloneSourceZone.ZoneDetails.GetDefaultVolume());
				m_zoneDetails.RequirePower = true;
                MZPState.Instance.PowerControlOn(m_parentZoneForm.ZoneDetails.ZoneId);
                m_cloneSourceZone.AddClonedZone(this.ZoneDetails);
                m_clonedZoneMusic = (ZoneMusic)m_cloneSourceZone.GetCurrentActivity();
                m_clonedZoneMusic.UpdateOutputDevices();
                m_zoneState = Metadata.ZoneState.Running;
            }
        }

        public override void Stop()
        {
            if ((m_clonedZoneMusic != null) && (m_parentZoneForm != null))
            {
                m_cloneSourceZone.RemoveClonedZone(this.ZoneDetails);
                m_clonedZoneMusic.UpdateOutputDevices();
            }
            m_zoneState = Metadata.ZoneState.NotStarted;
			m_zoneDetails.RequirePower = false;
        }
		public override void Close()
        {
            Stop();
        }
		public override void Next()
        {
            if ((m_cloneSourceZone != null) && (m_cloneSourceZone.GetCurrentActivity() != null))
                m_cloneSourceZone.GetCurrentActivity().Next();
        }

		public override void NextPlaylist()
        {
            //Next();
            NextMood();
        }

		public override void Previous()
        {
            if ((m_cloneSourceZone != null) && (m_cloneSourceZone.GetCurrentActivity() != null))
                m_cloneSourceZone.GetCurrentActivity().Previous();
        }

		public override void PreviousPlaylist()
        {
            //Previous();
            PreviousMood();
        }


		public override void Play()
        {
            if ((m_cloneSourceZone != null) && (m_cloneSourceZone.GetCurrentActivity() != null))
            {
                MZPState.Instance.PowerControlOn(m_parentZoneForm.ZoneDetails.ZoneId);
                m_cloneSourceZone.GetCurrentActivity().Play();
                m_zoneState = Metadata.ZoneState.Running;
            }
        }
		public override void Pause()
        {
            if ((m_cloneSourceZone!=null) && (m_cloneSourceZone.GetCurrentActivity()!= null))
                m_cloneSourceZone.GetCurrentActivity().Pause();
        }
		public override void Mute()
        {
            if ((m_cloneSourceZone != null) && (m_cloneSourceZone.GetCurrentActivity() != null))
                m_cloneSourceZone.GetCurrentActivity().Mute();
        }
		public override void VolumeUp()
        {
            if ((m_cloneSourceZone != null) && (m_cloneSourceZone.GetCurrentActivity() != null))
                m_cloneSourceZone.GetCurrentActivity().VolumeUp();
        }
		public override void VolumeDown()
        {
            if ((m_cloneSourceZone != null) && (m_cloneSourceZone.GetCurrentActivity() != null))
                m_cloneSourceZone.GetCurrentActivity().VolumeDown();
        }
		public override void SaveStateIni()
        {
        }
		public override void Guide()
        {
            if ((m_cloneSourceZone != null) && (m_cloneSourceZone.GetCurrentActivity() != null))
                m_cloneSourceZone.GetCurrentActivity().Guide();
        }

		public override Metadata.ZoneState GetState()
        {
            if ((m_cloneSourceZone != null) && (m_cloneSourceZone.GetCurrentActivity() != null))
            {
                if (m_zoneState == Metadata.ZoneState.Running)
                    return m_cloneSourceZone.GetCurrentActivity().GetState();
                else
                    return m_zoneState;
            }
            else
                return Metadata.ZoneState.NotInitialised;
        }

		public override Metadata.ZoneDetails ZoneDetails
        {
            get
            {
                return m_parentZoneForm.ZoneDetails;
            }
        }

		public override bool IsActive()
        {
            if ((m_cloneSourceZone != null) && (m_cloneSourceZone.GetCurrentActivity() != null))
            {
                if (m_zoneState == Metadata.ZoneState.Running)
                    return m_cloneSourceZone.GetCurrentActivity().IsActive();
                else
                    return false;
            }
            else
                return false;
        }

		public override void SetVolumeLevel(int volume)
        {
            if ((m_cloneSourceZone != null) && (m_cloneSourceZone.GetCurrentActivity() != null))
                m_cloneSourceZone.GetCurrentActivity().SetVolumeLevel(volume);

        }

		public override int GetVolumeLevel()
        {
            if ((m_cloneSourceZone != null) && (m_cloneSourceZone.GetCurrentActivity() != null))
                return m_cloneSourceZone.GetCurrentActivity().GetVolumeLevel();
            else
                return Metadata.VolumeLevels.VolumeSilence;
        }

        public int RatingUp()
        {
            if ((m_cloneSourceZone != null) && (m_cloneSourceZone.GetCurrentActivity() != null))
                return m_clonedZoneMusic.RatingUp();
            else
                return -1;
        }

        public int RatingDown()
        {
            if ((m_cloneSourceZone != null) && (m_cloneSourceZone.GetCurrentActivity() != null))
                return m_clonedZoneMusic.RatingDown();
            else
                return -1;
        }

		public override long Position
        {
            get
            {
                if ((m_cloneSourceZone != null) && (m_cloneSourceZone.GetCurrentActivity() != null))
                    return m_cloneSourceZone.GetCurrentActivity().Position;
                else
                    return -1;
            }
        }

		public override int PositionPercent
        {
            get
            {
                if ((m_cloneSourceZone != null) && (m_cloneSourceZone.GetCurrentActivity() != null))
                    return m_cloneSourceZone.GetCurrentActivity().PositionPercent;
                else
                    return -1;
            }
        }

        public void HoldCriteriaToggle()
        {
            if ((m_cloneSourceZone != null) && (m_cloneSourceZone.GetCurrentActivity() != null))
                m_clonedZoneMusic.HoldCriteriaToggle();
        }

        private void NumericCmdReceived(string key)
        {
        }

		public Metadata.ValueList ProcessAction(Metadata.GlobalCommands cmdRemote, Metadata.ValueList vals, ref Metadata.CommandResult cmdresult)
        {
            Metadata.ValueList result = new Metadata.ValueList();
            String action = action = vals.GetValue(Metadata.GlobalParams.action);
            int rating;
            switch (cmdRemote)
            {
                case Metadata.GlobalCommands.k0:
                case Metadata.GlobalCommands.k1:
                case Metadata.GlobalCommands.k2:
                case Metadata.GlobalCommands.k3:
                case Metadata.GlobalCommands.k4:
                case Metadata.GlobalCommands.k5:
                case Metadata.GlobalCommands.k6:
                case Metadata.GlobalCommands.k7:
                case Metadata.GlobalCommands.k8:
                case Metadata.GlobalCommands.k9:
                    NumericCmdReceived(cmdRemote.ToString().Substring(cmdRemote.ToString().Length-1));
                    break;
                case Metadata.GlobalCommands.enter://for numpads
                    Next();
                    break;
                case Metadata.GlobalCommands.right:
                    NextPlaylist();
                    break;
                case Metadata.GlobalCommands.left:
                    PreviousPlaylist();
                    break;
                case Metadata.GlobalCommands.up:
                    rating = RatingUp();
                    break;
                case Metadata.GlobalCommands.down:
                    rating = RatingDown();
                    break;
                case Metadata.GlobalCommands.followmemusic:
                    MZPState.Instance.ToogleFollowMeMusic();
                    break;
                case Metadata.GlobalCommands.back:
                case Metadata.GlobalCommands.holdcriteria:
                    HoldCriteriaToggle();
                    break;
                default:
                    MLog.Log(this, "WARNING, unprocessed zone command " + cmdRemote);
                    break;
            }
            return result;
        }

		public override void Tick()
        {
            //not implemented
            if (m_clonedZoneMusic != null && m_clonedZoneMusic.CurrentItem != null)
            {
                m_zoneDetails.Title = m_clonedZoneMusic.CurrentItem.Title;
                m_zoneDetails.Rating = m_clonedZoneMusic.CurrentItem.Rating;
                m_zoneDetails.Playcount = m_clonedZoneMusic.CurrentItem.PlayCount;
                m_zoneDetails.Author = m_clonedZoneMusic.CurrentItem.Author;
                m_zoneDetails.Genre = m_clonedZoneMusic.CurrentItem.Genre;
                m_zoneDetails.Year = m_clonedZoneMusic.CurrentItem.Year;
                m_zoneDetails.Album = m_clonedZoneMusic.CurrentItem.Album;
                m_zoneDetails.SourceURL = m_clonedZoneMusic.CurrentItem.SourceURL;
                m_zoneDetails.Playlist = "cloned zone";
            }

            
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;

using Vlc.DotNet.Core;
using Vlc.DotNet.Core.Medias;
using Vlc.DotNet.Forms;

namespace MultiZonePlayer
{
    class ZoneStreamVLC:BaseActivity
    {
        private VlcControl m_vlcControl;

        public const int VOLUME_NATIVE_MAX = 60;
        public const int VOLUME_NATIVE_MIN = 0;
        private String m_url;
        private int m_playlistIndex = 0;
        private int m_tickCount = 0;

        public ZoneStreamVLC(ZoneDetails p_zoneDetails)
        {
            m_zoneDetails = p_zoneDetails;

            if (VlcContext.IsInitialized)
            {
                //VlcContext.CloseAll();
                VlcContext.StartupOptions.ClearAllOptions();
            }

			VlcContext.LibVlcDllsPath = IniFile.PARAM_VIDEO_APP_VLC_DIR_PATH[1];
            //VlcContext.LibVlcDllsPath = CommonStrings.LIBVLC_DLLS_PATH_DEFAULT_VALUE_X86;
            VlcContext.LibVlcPluginsPath = IniFile.PARAM_VIDEO_APP_VLC_DIR_PATH[1]+"plugins\\";

			/*
			if (IntPtr.Size == 8)
			{
				VlcContext.LibVlcDllsPath = CommonStrings.LIBVLC_DLLS_PATH_DEFAULT_VALUE_X86;
				VlcContext.LibVlcPluginsPath = CommonStrings.PLUGINS_PATH_DEFAULT_VALUE_X86;
			}*/

            MLog.Log(this, "Initializing VLC DLL path=" + VlcContext.LibVlcDllsPath + " plugpath="+VlcContext.LibVlcPluginsPath);
            
            //Set the startup options
            VlcContext.StartupOptions.IgnoreConfig = true;
            VlcContext.StartupOptions.LogOptions.LogInFile = true;
            VlcContext.StartupOptions.LogOptions.ShowLoggerConsole = false;
            VlcContext.StartupOptions.LogOptions.Verbosity = VlcLogVerbosities.None;
            VlcContext.StartupOptions.LogOptions.LogInFilePath = IniFile.CurrentPath() + "vlc.log";

            if (IniFile.PARAM_VLC_AUDIOOUT[1].Equals("directx"))
            {
                VlcContext.StartupOptions.AddOption("--aout=directx");
                VlcContext.StartupOptions.AddOption("--directx-audio-device=" + ZoneDetails.ZoneDetailsList.Find(x => x.ZoneId.Equals(m_zoneDetails.ZoneId)).OutputDeviceDirectXName);
            }
            else
            {
                VlcContext.StartupOptions.AddOption("--aout=waveout");
                VlcContext.StartupOptions.AddOption("--waveout-audio-device=" + ZoneDetails.ZoneDetailsList.Find(x => x.ZoneId.Equals(m_zoneDetails.ZoneId)).OutputDeviceNameWaveVLC);
            }
            
            VlcContext.Initialize();

            m_vlcControl = new VlcControl();
            SetVolumeLevel(m_zoneDetails.GetDefaultVolume());
            
            //this.m_vlcControl.PositionChanged += this.VlcControlOnPositionChanged;
            //m_vlcControl.Playing += this.VlcControlPlaying;
            //m_vlcControl.Stopped += this.VlcControlStopped;

            //m_vlcControl.Play(new PathMedia(IniFile.CurrentPath() + "\\BeepOK.wav"));
        }

        /// <summary>
        /// Event handler for the <see cref="VlcControl.PositionChanged"/> event. 
        /// Updates the label containing the playback position. 
        /// </summary>
        /// <param name="sender">Event sending <see cref="VlcControl"/>. </param>
        /// <param name="e">Event arguments, containing the current position. </param>
        private void VlcControlOnPositionChanged(VlcControl sender, VlcEventArgs<float> e)
        {
            
            //label1.Text = (e.Data * 100).ToString("000") + " %";
        }

        private void VlcControlPlaying(VlcControl sender, EventArgs e)
        {
            MLog.Log(this, "Playing " + e.ToString());
            m_tickCount = 0;
            //Thread.Sleep(5000);
            
        }

        private void VlcControlStopped(VlcControl sender, EventArgs e)
        {
            MLog.Log(this, "Stopped " + e.ToString());
            //SetVolumeLevel(m_zoneDetails.GetDefaultVolume());
            //Thread.Sleep(5000);
            //SetVolumeLevel(m_zoneDetails.GetDefaultVolume());
        }

        public override void Stop()
        {
            m_vlcControl.Stop();
            m_tickCount = 0;
			UpdateZoneState();
            //m_zoneDetails.RequirePower = false;
			//m_zoneDetails.IsActive = false;
        }
		public override void Close()
        {
            Stop();
			m_zoneDetails.ZoneClose();
        }

		public override void Next()
        {
            //not impl
            Stop();
            m_playlistIndex++;
            Play();
        }

		public override void NextPlaylist()
        {
            Next();
        }
		public override void Previous()
        {
            Stop();
            m_playlistIndex--;
            Play();
        }
		public override void PreviousPlaylist()
        {
            Previous();
        }

        private String GetNextPlaylist()
        {
            String media="";

            for (int i = m_playlistIndex; i < MZPState.Instance.Playlist.Count; i++ )
            {
                media = MZPState.Instance.Playlist[i].MediaURL;
                if (media.StartsWith("http"))
                {
                    m_playlistIndex = i;
                    break;
                }
            }

            if (media.Equals(""))
            {
                media = MZPState.Instance.Playlist[m_playlistIndex].MediaURL;
                if (!media.StartsWith("http"))
                    media = "";
            }

            m_zoneDetails.Playlist = media;
            return media;
        }

        public override void Play()
        {
            m_url = GetNextPlaylist();

            if (!m_url.Equals(""))
            {
                //MZPState.Instance.PowerControlOn(m_zoneDetails.ZoneId);
                var media = new PathMedia(m_url);
                m_vlcControl.Media = media;
				UpdateZoneState();
				//m_zoneDetails.RequirePower = true;
				//m_zoneDetails.IsActive = true;
            }
            else
                MLog.Log(this, "error empty stream url");
        }

		public override void Pause()
        {
            m_vlcControl.Pause();
			UpdateZoneState();
        }

		public override void Mute()
        {
            //not impl
        }

		public override void VolumeUp()
        {
            if (m_vlcControl.AudioProperties.Volume < VOLUME_NATIVE_MAX)
                SetVolumeLevelNative(m_vlcControl.AudioProperties.Volume + 4);
        }

		public override void VolumeDown()
        {
            if (m_vlcControl.AudioProperties.Volume > VOLUME_NATIVE_MIN)
                SetVolumeLevelNative(m_vlcControl.AudioProperties.Volume - 4);
        }

        public override void Guide()
        {
            //
        }
		public override ZoneState GetState()
        {
            switch (m_vlcControl.State)
            {
                case Vlc.DotNet.Core.Interops.Signatures.LibVlc.Media.States.Stopped:
                    return ZoneState.NotStarted;
                case Vlc.DotNet.Core.Interops.Signatures.LibVlc.Media.States.Paused:
                    return ZoneState.Paused;
				case Vlc.DotNet.Core.Interops.Signatures.LibVlc.Media.States.Opening:
				case Vlc.DotNet.Core.Interops.Signatures.LibVlc.Media.States.Buffering:
                case Vlc.DotNet.Core.Interops.Signatures.LibVlc.Media.States.Playing:
                    return ZoneState.Running;
                case Vlc.DotNet.Core.Interops.Signatures.LibVlc.Media.States.Error:
                    return ZoneState.Error;
                default:
					MLog.Log(this, "UNKOWN VLC state=" + m_vlcControl.State);
                    return ZoneState.NotInitialised;
            }
        }

        
        public override bool IsActive()
        {
            //
            return GetState().Equals(ZoneState.Running);
        }

		public override void SetVolumeLevel(int volume)
        {
            int calcvol = (VOLUME_NATIVE_MAX - VOLUME_NATIVE_MIN) * (VolumeLevels.VolumeSilence - volume) / VolumeLevels.VolumeSilence;
            MLog.Log(this, "set volume val="+volume);
            SetVolumeLevelNative(calcvol);
        }

		public override int GetVolumeLevel()
        {
            return (VOLUME_NATIVE_MAX - m_vlcControl.AudioProperties.Volume) * VolumeLevels.VolumeSilence / (VOLUME_NATIVE_MAX - VOLUME_NATIVE_MIN);
        }

        public float GetVolumeLevelNative()
        {
            return m_vlcControl.AudioProperties.Volume;
        }

        private void SetVolumeLevelNative(float volume)
        {
            MLog.Log(this, "set volume native val=" + volume);
            m_vlcControl.AudioProperties.Volume = (int)volume;
        }

        public String GetCurrentURL()
        {
            return m_url;
        }

		private void UpdateZoneState() {
			if (m_vlcControl != null && m_vlcControl.Media != null) {
				m_zoneDetails.ActivityType = GlobalCommands.radio;
				m_zoneDetails.Title = m_vlcControl.Media.Metadatas.NowPlaying;
				m_zoneDetails.Genre = m_vlcControl.Media.Metadatas.Genre;
				m_zoneDetails.Author = m_vlcControl.Media.Metadatas.Title;
				m_zoneDetails.SourceURL = m_vlcControl.Media.MRL;
				m_zoneDetails.ZoneState = GetState();
				m_tickCount++;
			}
		}
		public override void Tick()
        {
			UpdateZoneState();
        }
    }
}

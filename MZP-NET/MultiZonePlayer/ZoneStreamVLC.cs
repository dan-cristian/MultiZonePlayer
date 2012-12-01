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
    class ZoneStreamVLC:IZoneActivity
    {
        private VlcControl m_vlcControl;

        public const int VOLUME_NATIVE_MAX = 60;
        public const int VOLUME_NATIVE_MIN = 0;
        private String m_url;
        private int m_playlistIndex = 0;
        private Metadata.ZoneDetails m_zoneDetails;
        private int m_tickCount = 0;

        public ZoneStreamVLC(Metadata.ZoneDetails p_zoneDetails)
        {
            m_zoneDetails = p_zoneDetails;

            if (VlcContext.IsInitialized)
            {
                //VlcContext.CloseAll();
                VlcContext.StartupOptions.ClearAllOptions();
            }

            //VlcContext.LibVlcDllsPath = CommonStrings.LIBVLC_DLLS_PATH_DEFAULT_VALUE_X86;
            //VlcContext.LibVlcPluginsPath = CommonStrings.PLUGINS_PATH_DEFAULT_VALUE_X86;
            MLog.Log(this, "Initializing VLC DLL path=" + VlcContext.LibVlcDllsPath);
            
            //Set the startup options
            VlcContext.StartupOptions.IgnoreConfig = true;
            VlcContext.StartupOptions.LogOptions.LogInFile = true;
            VlcContext.StartupOptions.LogOptions.ShowLoggerConsole = false;
            VlcContext.StartupOptions.LogOptions.Verbosity = VlcLogVerbosities.None;
            VlcContext.StartupOptions.LogOptions.LogInFilePath = IniFile.CurrentPath() + "vlc.log";

            if (IniFile.PARAM_VLC_AUDIOOUT[1].Equals("directx"))
            {
                VlcContext.StartupOptions.AddOption("--aout=directx");
                VlcContext.StartupOptions.AddOption("--directx-audio-device=" + MZPState.Instance.ZoneDetails.Find(x => x.ZoneId.Equals(m_zoneDetails.ZoneId)).OutputDeviceDirectXName);
            }
            else
            {
                VlcContext.StartupOptions.AddOption("--aout=waveout");
                VlcContext.StartupOptions.AddOption("--waveout-audio-device=" + MZPState.Instance.ZoneDetails.Find(x => x.ZoneId.Equals(m_zoneDetails.ZoneId)).OutputDeviceNameWaveVLC);
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

        public void Stop()
        {
            m_vlcControl.Stop();
            m_tickCount = 0;
        }
        public void Close()
        {
            Stop();
        }

        public void Next()
        {
            //not impl
            Stop();
            m_playlistIndex++;
            Play();
        }

        public void NextPlaylist()
        {
            Next();
        }
        public void Previous()
        {
            Stop();
            m_playlistIndex--;
            Play();
        }
        public void PreviousPlaylist()
        {
            Previous();
        }

        public  void NextMood()
        {
        }

        public  void PreviousMood()
        {
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

        public void Play()
        {
            m_url = GetNextPlaylist();

            if (!m_url.Equals(""))
            {
                MZPState.Instance.PowerControl.PowerOn(m_zoneDetails.ZoneId);
                var media = new PathMedia(m_url);
                m_vlcControl.Media = media;
            }
            else
                MLog.Log(this, "error empty stream url");
        }

        public void Pause()
        {
                m_vlcControl.Pause();
        }

        public void Mute()
        {
            //not impl
        }

        public void VolumeUp()
        {
            if (m_vlcControl.AudioProperties.Volume < VOLUME_NATIVE_MAX)
                SetVolumeLevelNative(m_vlcControl.AudioProperties.Volume + 4);
        }

        public void VolumeDown()
        {
            if (m_vlcControl.AudioProperties.Volume > VOLUME_NATIVE_MIN)
                SetVolumeLevelNative(m_vlcControl.AudioProperties.Volume - 4);
        }

        public void SaveStateIni()
        {
            //
        }

        public void Guide()
        {
            //
        }
        public Metadata.ZoneState GetState()
        {
            switch (m_vlcControl.State)
            {
                case Vlc.DotNet.Core.Interops.Signatures.LibVlc.Media.States.Stopped:
                case Vlc.DotNet.Core.Interops.Signatures.LibVlc.Media.States.Buffering:
                    return Metadata.ZoneState.NotStarted;
                case Vlc.DotNet.Core.Interops.Signatures.LibVlc.Media.States.Paused:
                    return Metadata.ZoneState.Paused;
                case Vlc.DotNet.Core.Interops.Signatures.LibVlc.Media.States.Playing:
                    return Metadata.ZoneState.Running;
                case Vlc.DotNet.Core.Interops.Signatures.LibVlc.Media.States.Error:
                    return Metadata.ZoneState.Error;
                default:
                    return Metadata.ZoneState.NotInitialised;
            }
        }

        public Metadata.ZoneDetails ZoneDetails
        {
            get
            {
                return m_zoneDetails;
            }
        }
        public long Position
        {
            get { return 0; }
        }
        public int PositionPercent
        {
            get { return 0; }
        }
        public bool IsActive()
        {
            //
            return GetState().Equals(Metadata.ZoneState.Running);
        }

        public void SetVolumeLevel(int volume)
        {
            int calcvol = (VOLUME_NATIVE_MAX - VOLUME_NATIVE_MIN) * (Metadata.VolumeLevels.VolumeSilence - volume) / Metadata.VolumeLevels.VolumeSilence;
            MLog.Log(this, "set volume val="+volume);
            SetVolumeLevelNative(calcvol);
        }

        public int GetVolumeLevel()
        {
            return (VOLUME_NATIVE_MAX - m_vlcControl.AudioProperties.Volume) * Metadata.VolumeLevels.VolumeSilence / (VOLUME_NATIVE_MAX - VOLUME_NATIVE_MIN);
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
        

        public void Tick()
        {
            if (m_vlcControl != null && m_vlcControl.Media != null)
            {
                m_zoneDetails.Title = m_vlcControl.Media.Metadatas.NowPlaying;
                m_zoneDetails.Genre = m_vlcControl.Media.Metadatas.Genre;
                m_zoneDetails.Author = m_vlcControl.Media.Metadatas.Title;
                m_zoneDetails.SourceURL = m_vlcControl.Media.MRL;
                
                //if (m_tickCount <1)
                    //SetVolumeLevelNative(VOLUME_NATIVE_MAX);
                //if (m_tickCount == 1)
                //    SetVolumeLevel(m_zoneDetails.GetDefaultVolume());
                //if (m_tickCount==2)
                //    SetVolumeLevelNative(GetVolumeLevelNative());
                m_tickCount++;
                 
            }
            
        }

    }
}

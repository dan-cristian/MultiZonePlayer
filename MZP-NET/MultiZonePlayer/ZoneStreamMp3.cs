using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;
using NAudioDemo;
using System.Net;
using System.IO;
using System.Threading;

namespace MultiZonePlayer
{
    class ZoneStreamMp3:IZoneActivity
    {
        enum StreamingPlaybackState
        {
            Stopped,
            Playing,
            Buffering,
            Paused
        }

        public const float VOLUME_MAX = 0.100F;
        public const float VOLUME_MIN = 0;
        private BufferedWaveProvider bufferedWaveProvider;
        private IWavePlayer waveOut;
        private volatile StreamingPlaybackState playbackState;
        private volatile bool fullyDownloaded;
        private HttpWebRequest webRequest;
        private VolumeWaveProvider16 volumeProvider;
        private double m_bufferSeconds;
        private String m_url;
        private int m_deviceIndex;
        private int m_playlistIndex = 0;
        private Metadata.ZoneDetails m_zoneDetails;
        private bool m_isStarted = false;

        public ZoneStreamMp3(Metadata.ZoneDetails p_zoneDetails)
        {
            m_zoneDetails = p_zoneDetails;
            m_deviceIndex = m_zoneDetails.WavedeviceIndex;
            
        }

        public void Stop()
        {
            m_isStarted = false;
            StopPlayback();
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

        public void NextMood()
        {
        }

        public void PreviousMood()
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
                m_isStarted = true;
            }
            else
                MLog.Log(this, "error empty stream url");
        }

        public void Pause()
        {
            if (playbackState == StreamingPlaybackState.Playing || playbackState == StreamingPlaybackState.Buffering)
            {
                waveOut.Pause();
                MLog.Log(this, String.Format("User requested Pause, waveOut.PlaybackState={0}", waveOut.PlaybackState));
                playbackState = StreamingPlaybackState.Paused;
            }
        }

        public void Mute()
        {
            //not impl
        }

        public void VolumeUp()
        {
            if (this.volumeProvider.Volume < VOLUME_MAX)
                SetVolumeLevelNative(this.volumeProvider.Volume - ((float)IniFile.VOLUME_STEP / Metadata.VolumeLevels.VolumeSilence));
        }

        public void VolumeDown()
        {
            if (this.volumeProvider.Volume > VOLUME_MIN)
                SetVolumeLevelNative(this.volumeProvider.Volume + ((float)IniFile.VOLUME_STEP / Metadata.VolumeLevels.VolumeSilence));
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
            switch (playbackState)
            {
                case StreamingPlaybackState.Buffering:
                    return Metadata.ZoneState.NotStarted;
                case StreamingPlaybackState.Paused:
                    return Metadata.ZoneState.Paused;
                case StreamingPlaybackState.Playing:
                    return Metadata.ZoneState.Running;
                case StreamingPlaybackState.Stopped:
                    return Metadata.ZoneState.NotStarted;
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
            SetVolumeLevelNative(VOLUME_MAX - (VOLUME_MAX-VOLUME_MIN)*((float)volume / Metadata.VolumeLevels.VolumeSilence));
        }

        public int GetVolumeLevel()
        {
            return (int)(Metadata.VolumeLevels.VolumeSilence - this.volumeProvider.Volume * Metadata.VolumeLevels.VolumeSilence);
        }

        public float GetVolumeLevelNative()
        {
            return this.volumeProvider.Volume;
        }

        private void SetVolumeLevelNative(float volume)
        {
            this.volumeProvider.Volume = volume;
        }

        public int GetBufferSeconds()
        {
            return (int)m_bufferSeconds;
        }

        public int GetMaxBufferSize()
        {
            return (int)bufferedWaveProvider.BufferDuration.TotalMilliseconds;
        }

        public String GetCurrentURL()
        {
            return m_url;
        }

        private void StreamMP3(object state)
        {
            this.fullyDownloaded = false;
            string url = (string)state;
            webRequest = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse resp = null;
            try
            {
                resp = (HttpWebResponse)webRequest.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Status != WebExceptionStatus.RequestCanceled)
                {
                    MLog.Log(e, "open url stream error");
                }
                return;
            }
            byte[] buffer = new byte[16384 * 4]; // needs to be big enough to hold a decompressed frame

            IMp3FrameDecompressor decompressor = null;
            try
            {
                using (var responseStream = resp.GetResponseStream())
                {
                    var readFullyStream = new ReadFullyStream(responseStream);
                    do
                    {
                        if (bufferedWaveProvider != null && bufferedWaveProvider.BufferLength - bufferedWaveProvider.BufferedBytes < bufferedWaveProvider.WaveFormat.AverageBytesPerSecond / 4)
                        {
                            MLog.Log(this, "Buffer getting full, taking a break");
                            Thread.Sleep(500);
                        }
                        else
                        {
                            Mp3Frame frame = null;
                            try
                            {
                                frame = Mp3Frame.LoadFromStream(readFullyStream);
                                
                            }
                            catch (EndOfStreamException)
                            {
                                this.fullyDownloaded = true;
                                // reached the end of the MP3 file / stream
                                break;
                            }
                            catch (WebException)
                            {
                                // probably we have aborted download from the GUI thread
                                break;
                            }

                            if (frame == null)
                            {
                                MLog.Log(this, "unsuported mp3 file");
                                break;
                            }
                            if (decompressor == null)
                            {
                                // don't think these details matter too much - just help ACM select the right codec
                                // however, the buffered provider doesn't know what sample rate it is working at
                                // until we have a frame
                                WaveFormat waveFormat = new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2, frame.FrameLength, frame.BitRate);
                                decompressor = new AcmMp3FrameDecompressor(waveFormat);
                                this.bufferedWaveProvider = new BufferedWaveProvider(decompressor.OutputFormat);
                                this.bufferedWaveProvider.BufferDuration = TimeSpan.FromSeconds(20); // allow us to get well ahead of ourselves
                                //this.bufferedWaveProvider.BufferedDuration = 250;
                          
                            }
                            try
                            {
                                int decompressed = decompressor.DecompressFrame(frame, buffer, 0);
                                //MLog.Log(String.Format("Decompressed a frame {0}", decompressed));
                                bufferedWaveProvider.AddSamples(buffer, 0, decompressed);
                            }
                            catch (NAudio.MmException mex)
                            {
                                MLog.Log(mex, "got mm ex");
                                Stop();
                            }
                            catch (Exception ex)
                            {
                                MLog.Log(ex, "err playing stream");
                                Stop();
                            }
                        }

                    } while (playbackState != StreamingPlaybackState.Stopped);
                    MLog.Log(this, "Exiting");
                    // was doing this in a finally block, but for some reason
                    // we are hanging on response stream .Dispose so never get there
                    decompressor.Dispose();
                }
            }
            finally
            {
                if (decompressor != null)
                {
                    decompressor.Dispose();
                }
            }
        }

        private void StreamADTS(object state)
        {
            
        }

        private IWavePlayer CreateWaveOut(int deviceNumber)
        {
            return new WaveOut(deviceNumber);
            //return new DirectSoundOut();
        }

        public void Tick()
        {
            
            if (playbackState == StreamingPlaybackState.Stopped && m_isStarted)
            {
                playbackState = StreamingPlaybackState.Buffering;
                this.bufferedWaveProvider = null;
                //ThreadPool.QueueUserWorkItem(new WaitCallback(StreamMP3), m_url);
                ThreadPool.QueueUserWorkItem(new WaitCallback(StreamADTS), m_url);
                //timer1.Enabled = true;
                MZPState.Instance.PowerControl.PowerOn(m_zoneDetails.ZoneId);
            }
            else if (playbackState == StreamingPlaybackState.Paused)
            {
                playbackState = StreamingPlaybackState.Buffering;
            }
            

            if (playbackState != StreamingPlaybackState.Stopped)
            {
                if (this.waveOut == null && this.bufferedWaveProvider != null)
                {
                    MLog.Log(this, "Creating WaveOut Device");
                    this.waveOut = CreateWaveOut(m_deviceIndex);
                    waveOut.PlaybackStopped += new EventHandler(waveOut_PlaybackStopped);
                    this.volumeProvider = new VolumeWaveProvider16(bufferedWaveProvider);
                    SetVolumeLevel(m_zoneDetails.GetDefaultVolume());
                    waveOut.Init(volumeProvider);
                    //progressBarBuffer.Maximum = (int)bufferedWaveProvider.BufferDuration.TotalMilliseconds;
                }
                else if (bufferedWaveProvider != null)
                {
                    var bufferedSeconds = bufferedWaveProvider.BufferedDuration.TotalSeconds;
                    m_bufferSeconds = bufferedSeconds;
                    //ShowBufferState(bufferedSeconds);
                    // make it stutter less if we buffer up a decent amount before playing
                    if (bufferedSeconds < 0.5 && this.playbackState == StreamingPlaybackState.Playing && !this.fullyDownloaded)
                    {
                        this.playbackState = StreamingPlaybackState.Buffering;
                        waveOut.Pause();
                        MLog.Log(this, String.Format("Paused to buffer, waveOut.PlaybackState={0}", waveOut.PlaybackState));
                    }
                    else if (bufferedSeconds > 4 && this.playbackState == StreamingPlaybackState.Buffering)
                    {
                        waveOut.Play();
                        MLog.Log(this, String.Format("Started playing, waveOut.PlaybackState={0}", waveOut.PlaybackState));
                        this.playbackState = StreamingPlaybackState.Playing;
                    }
                    else if (this.fullyDownloaded && bufferedSeconds == 0)
                    {
                        MLog.Log(this, "Reached end of stream");
                        StopPlayback();
                    }
                }

            }
        }

        private void StopPlayback()
        {
            if (playbackState != StreamingPlaybackState.Stopped)
            {
                if (!fullyDownloaded)
                {
                    webRequest.Abort();
                }
                this.playbackState = StreamingPlaybackState.Stopped;
                if (waveOut != null)
                {
                    waveOut.Stop();
                    waveOut.Dispose();
                    waveOut = null;
                }
                //timer1.Enabled = false;
                // n.b. streaming thread may not yet have exited
                Thread.Sleep(500);
                m_bufferSeconds = 0;
                //ShowBufferState(0);
            }
        }

        private void waveOut_PlaybackStopped(object sender, EventArgs e)
        {
            MLog.Log(this, "Playback Stopped");
            Stop();
        }
    }
}

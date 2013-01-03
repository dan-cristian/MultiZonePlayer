using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Management;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;


namespace MultiZonePlayer
{
    enum VlcCommand
    {
        Add,
        Enqueue,
        Play,
        F,
        Is_Playing,
        Get_Time,
        seek,
        Pause,
        FastForward,
        Rewind,
        stop,
        next,
        prev,
        faster,
        slower,
        volume,
        volup,
        voldown,
        quit
    }
    enum MPCCommand
    {
        //OpenFile_800,
        //Pause_888,
        Exit_816,
        Play_887,
        PlayPause_889,
        VolumeMute_909,
        VolumeUp_907,
        VolumeDown_908,
        JumpForwardSmall_900,
        JumpBackwardSmall_899,
        JumpForwardMedium_902,
        JumpBackwardMedium_901,
        Fullscreen_830,
        NextSubtitleOGM_959
    }

    public abstract class ZoneVideoBase: IZoneActivity
    {
        protected ZoneGeneric m_zoneForm;
        protected VideoPlayList m_playlistForm;
        protected VideoPlayMenu m_playlistMenu;
        protected int m_videoAppWinHandle = -1;
        protected WrapperBase m_wrapper = null;
        protected String m_VideoAppTitleKey;
        protected String m_videoFilePath;

        public ZoneVideoBase(ZoneGeneric zone)
        {
            m_zoneForm = zone;
            m_playlistMenu = new VideoPlayMenu(this);
            ShowPlayMenuForm();
        }

         ~ZoneVideoBase()
        {
            Close();
        }

        private void ShowPlayMenuForm()
        {
            if (m_playlistMenu != null)
            {
                //try showing this on 2nd monitor
                Screen[] sc;
                sc = Screen.AllScreens;
                if ((sc.Length > 1) && ((m_playlistMenu.Left < sc[1].Bounds.X) || (m_playlistMenu.Top < sc[1].Bounds.Y)))
                {
                    MLog.Log(null, "Multiple screens detected, count=" + sc.Length);
                    m_playlistMenu.DesktopLocation = sc[1].Bounds.Location;
                    m_playlistMenu.StartPosition = FormStartPosition.Manual;
                }
                m_playlistMenu.Show();
            }
        }

        public void InitSetup()
        {
            m_playlistForm = new VideoPlayList();
            m_playlistForm.Show();
            m_playlistForm.BringToFront();
        }

        public virtual void Close()
        {
            Stop();
            if (m_playlistMenu != null)
            {
                m_playlistMenu.Close();
                m_playlistMenu = null;
            }
            
        }

        public void Play(String videoPath)
        {
            if ((m_wrapper == null) || !m_wrapper.IsVideoAppProcessAlive || !m_wrapper.IsTCPConnected)
            {
                MLog.Log(null, "Playing video " + videoPath);
                m_videoFilePath = videoPath;
                ResetVideoApp();

                if (InitVideoApp(videoPath))
                {
                    GetVideoAppHandle();
                    if (m_videoAppWinHandle == -1)
                        MLog.Log(null, "No videoapp handle found, control disabled");
                    SendVideoAppToSecondaryDisplay();
                    GoToFullScreen();
                    VideoItem info = m_playlistMenu.VideoCollection.GetVideoInfo(videoPath);
                    info.Seen = DateTime.Now.Year + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day;
                    info.SaveItem();
                }
                else
                    MLog.Log(null, "Video cannot be initialised");
            }
            else
            {
                MLog.Log(null, "Play command received while playing, ignored, video=" + videoPath);
            }
        }

        public void Play()
        {
            //m_vlc.Play();
            Play(m_playlistMenu.GetCurrentMovie());
        }


        public void VideoSelectionDown()
        {
            m_playlistMenu.VideoSelectionDown();
        }

        public void VideoSelectionUp()
        {
            m_playlistMenu.VideoSelectionUp();
        }

        public void NextSearchCriteria()
        {
            m_playlistMenu.NextCriteriaSelection();
        }

        public void PreviousSearchCriteria()
        {
            m_playlistMenu.PreviousCriteriaSelection();
        }

        public void ToggleSearchCriteria()
        {
            m_playlistMenu.ToggleSearchCriteria();
        }

        protected abstract bool InitVideoApp(String videoFullPath);

        protected void ResetVideoApp()
        {
            if (m_wrapper != null)
            {
                m_wrapper.Quit();
                m_wrapper.Close();
                m_wrapper = null;
            }
        }

        public void Tick()
        {
            try
            {
                Utilities.RECT winRect;

                if ((Screen.AllScreens.Length > 1) && (m_videoAppWinHandle != -1))
                {
                    if (Utilities.GetWindowRect(new IntPtr(m_videoAppWinHandle), out winRect))
                    {
                        if ((winRect.Left < Screen.AllScreens[1].Bounds.Location.X) || (winRect.Top < Screen.AllScreens[1].Bounds.Location.Y))
                        {
                            MLog.Log(this, "Need to send to secondary display, screen count=" + Screen.AllScreens.Length
                                + " VideoApp Rect=" + winRect.Left + "," + winRect.Top + "," + winRect.Bottom + "," + winRect.Right);
                            SendVideoAppToSecondaryDisplay();
                            GoToFullScreen();
                        }
                    }
                    ShowPlayMenuForm();
                }

                //read status periodically
                RetrieveStatus();
                //if (Screen.AllScreens.Length >= 1)
            }
            catch (Exception ex)
            {
                MLog.Log(ex, this, "Exception on timerclick, screencount="+Screen.AllScreens+" apphandle="+m_videoAppWinHandle);
            }
        }

        protected void SendVideoAppToSecondaryDisplay()
        {
            if (Screen.AllScreens.Length > 1)
            {
                if (m_videoAppWinHandle != -1)
                {
                    Utilities.MoveWindow(new IntPtr(m_videoAppWinHandle), Screen.AllScreens[1].Bounds.Location.X, Screen.AllScreens[1].Bounds.Location.Y,
                        Screen.AllScreens[1].WorkingArea.Width, Screen.AllScreens[1].WorkingArea.Height, true);
                    
                }
                else
                {
                    MLog.Log(null, "No Window found to send on 2nd display");
                }
            }
            else
            {
                MLog.Log(null, "No dual screen detected");
            }
        }

        protected abstract int GetVideoAppHandle();
        protected void RetrieveStatus()
        {
            if (m_wrapper != null)
            {
                if (!m_wrapper.RetrieveStatus())
                    Stop();
            }
        }

        public void GoToFullScreen()
        {
            if (m_wrapper != null)
            {
                m_wrapper.GoToFullScreen();
            }
            else
                m_playlistMenu.WindowState = m_playlistMenu.WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
        }

        protected bool IsVideoRunning
        {
            get
            {
                if (m_wrapper == null)
                    return false;

                bool stateOK = m_wrapper.IsVideoAppProcessAlive;
                if (!stateOK)
                {
                    //MLog.Log(null,"VLC State not OK, attempting close");
                    //Close();
                }
                return stateOK;
            }
        }

        public long Position
        {
            get
            {
                if (m_wrapper != null)
                    return m_wrapper.Position;
                else
                    return -1;
            }
        }

        public int PositionPercent
        {
            get
            {
                if (m_wrapper != null)
                    return m_wrapper.PositionPercent;
                else
                    return -1;
            }
        }

        public virtual void Stop()
        {
            if (m_wrapper != null)
            {
                m_wrapper.Close();
                m_wrapper = null;
            }
        }

        public virtual void Next()
        {
            Play();
        }

        public virtual void NextPlaylist()
        {
            Next();
        }

        public virtual void NextMood()
        {
        }

        public virtual void PreviousMood()
        {
        }

        public virtual void Previous()
        {
        }

        public virtual void PreviousPlaylist()
        {
            Previous();
        }

        public virtual void Pause()
        {
            if (m_wrapper != null) m_wrapper.Pause();
        }

        public virtual void Mute()
        {
            if (m_wrapper != null) m_wrapper.Mute();

        }
        public virtual void VolumeUp()
        {
            if (m_wrapper != null) m_wrapper.VolumeUp();
        }
        public virtual void VolumeDown()
        {
            if (m_wrapper != null) m_wrapper.VolumeDown();
        }
        public virtual void SetVolumeLevel(int volume)
        {
        }

        public virtual int GetVolumeLevel()
        {
            if (m_wrapper != null)
                return m_wrapper.Volume;
            else return -1;
        }

        public virtual void SaveStateIni()
        {
        }

        public virtual void Guide()
        {
        }

        public virtual Metadata.ZoneState GetState()
        {
            if (m_wrapper != null)
                return m_wrapper.State;
            else
                return Metadata.ZoneState.NotInitialised;
        }

        public Metadata.ZoneDetails ZoneDetails
        {
            get
            {
                return m_zoneForm.ZoneDetails;
            }
        }

        public virtual bool IsActive()
        {
            return GetState().Equals(Metadata.ZoneState.Running);
        }

        
    }

    public class ZoneVideoMPC : ZoneVideoBase
    {
         public ZoneVideoMPC(ZoneGeneric zone)
            : base(zone)
        {
            m_VideoAppTitleKey = IniFile.PARAM_VIDEO_APP_MPC_WINDOWTITLE_KEY[1];
        }

        public void Ffwd()
        {
            if (IsVideoRunning)
            {
                m_wrapper.Ffwd();
            }
            else
            {
                m_playlistMenu.NextCriteriaContainer();
            }
        }

        public void Rewind()
        {
            if (IsVideoRunning)
            {
                m_wrapper.Rewind();
            }
            else
            {
                m_playlistMenu.PreviousCriteriaContainer();
            }
        }

        protected override bool InitVideoApp(String videoFullPath)
        {
            bool result;
            try
            {
                m_wrapper = new WrapperMPCRemote(videoFullPath);
                //SendToSecondaryDisplay("vlc.exe");
                result = m_wrapper.Initialize();
            }
            catch (Exception ex)
            {
                MLog.Log(this, "Error InitVideoApp " + videoFullPath + ex.Message + ex.StackTrace);
                result = false;
            }

            if (!result)
                ResetVideoApp();

            return result;
        }

        protected override int GetVideoAppHandle()
        {
            for (int i = 1; i < 60; i++)
            {
                MLog.Log(null, "Searching for VideoApp handle " + " pas " + i);
                m_videoAppWinHandle = WindowsEnumerator.GetWindowHandleByTitle(m_videoFilePath);
                if (m_videoAppWinHandle != -1)
                {
                    MLog.Log(this, "Handle found for title " + m_videoFilePath);
                    break;
                }
                System.Threading.Thread.Sleep(250);
                Application.DoEvents();
            }
            return m_videoAppWinHandle;
        }


    }

    public class ZoneVideoVlc:ZoneVideoBase
    {

        public ZoneVideoVlc(ZoneGeneric zone)
            : base(zone)
        {
            m_VideoAppTitleKey = IniFile.PARAM_VIDEO_APP_VLC_WINDOWTITLE_KEY[1];
        }
        
        public void NotifyClose()
        {
            m_playlistMenu = null;
        }

        public override void Stop()
        {
            ResetVideoApp();
        }


        public override void Next()
        {
           // m_vlc.Next();
           m_wrapper.Ffwd();
        }

        public override void Previous()
        {
            // m_vlc.Next();
            m_wrapper.Rewind();
        }

        public override void Pause()
        {
            if (IsVideoRunning)
            {
                m_wrapper.Pause();
            }
            else
                Play();
        }

        public override void Mute()
        {
            if (IsVideoRunning)
            {
                m_wrapper.Mute();
            }
        }

        public override void VolumeUp()
        {
            if (IsVideoRunning)
            {
                m_wrapper.VolumeUp();
            }
        }
        public override void VolumeDown()
        {
            if (IsVideoRunning)
            {
                m_wrapper.VolumeDown();
            }
        }



        public override void SaveStateIni()
        {
            //save video
            //save position
        }

        public override void Guide()
        {
        }

        public override Metadata.ZoneState GetState()
        {
            if (m_wrapper == null)
                return Metadata.ZoneState.NotInitialised;
            else
                return Metadata.ZoneState.Undefined;
        }

        public override bool IsActive()
        {
            if (m_wrapper == null)
                return false;
            else
                return m_wrapper.IsVideoAppProcessAlive && m_wrapper.IsTCPConnected;
        }

        public override void SetVolumeLevel(int volume)
        {
            if (IsVideoRunning)
            {
                // m_vlc.SetVolume(volume);
            }
        }

        public new long Position
        {
            get
            {
                if (IsVideoRunning)
                    return m_wrapper.Position;
                else
                    return -1;
            }
        }

        public override int GetVolumeLevel()
        {
            if (IsVideoRunning)
                return m_wrapper.Volume;
            else
                return -1;
        }

        protected override bool InitVideoApp(String videoFullPath)
        {
            bool result;
            try
            {
                m_wrapper = new WrapperVlcRemote(videoFullPath);
                //SendToSecondaryDisplay("vlc.exe");
                result = m_wrapper.Initialize();
            }
            catch (Exception ex)
            {
                MLog.Log(null,"Error InitVideoApp " + videoFullPath + ex.Message + ex.StackTrace);
                result = false;
            }

            if (!result)
                ResetVideoApp();

            return result;
        }

       protected override int GetVideoAppHandle()
        {
            for (int i = 1; i < 60; i++)
            {
                MLog.Log(null, "Searching for VideoApp handle " + " pas " + i);
                m_videoAppWinHandle = WindowsEnumerator.GetWindowHandleByTitle(m_VideoAppTitleKey);
                if (m_videoAppWinHandle != -1)
                    {
                        MLog.Log(this, "Handle found for title " + m_VideoAppTitleKey);
                        break;
                    }
                System.Threading.Thread.Sleep(250);
                Application.DoEvents();
            }
            return m_videoAppWinHandle;
        } 
        
        
    }

    public abstract class WrapperBase
    {
        // maximum 5 second wait on results. 
        protected const int WaitTimeout = 5000;
        protected static ASCIIEncoding ASCII_Encoding = new ASCIIEncoding();

        protected TcpClient m_TcpClient;
        protected String m_videoAppprocessName;
        protected Process m_videoAppProcess;
        protected String m_videoFullPath;
        protected String m_videoAppPath;
        protected int m_port;

        protected long m_position;
        protected long m_totalposition;
        protected int m_volume;
        protected Metadata.ZoneState m_zoneState;

        public abstract bool Initialize();
        public abstract bool RetrieveStatus();
        
        public virtual void Close()
        {
            try
            {
                Quit();
                TCPDisconnect();
                m_zoneState = Metadata.ZoneState.Closing;
                m_videoAppProcess.CloseMainWindow();
                m_videoAppProcess.Kill();
            }
            catch (Exception ex)
            {
                MLog.Log(ex, this, "Error closing zonevideoapp");
            }
        }

       
        public abstract void Quit();

        public bool IsTCPConnected
        {
            get
            {
                try
                {
                    return (m_TcpClient != null) && (m_TcpClient.Connected);
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        protected static int GetParentProcessId(int Id)
        {
            int parentPid = 0;
            using (ManagementObject mo = new ManagementObject("win32_process.handle='" + Id.ToString() + "'"))
            {
                mo.Get();
                parentPid = Convert.ToInt32(mo["ParentProcessId"]);
            }
            return parentPid;
        }

        protected Process VideoAppProcess
        {
            get
            {
                var currentProcessId = Process.GetCurrentProcess().Id;
                Process videoAppProcess = null;
                foreach (var process in Process.GetProcessesByName(m_videoAppprocessName))
                {
                    if (GetParentProcessId(process.Id) == currentProcessId)
                    {
                        videoAppProcess = process;
                        break;
                    }
                }
                return videoAppProcess;
            }
        }

        protected bool StartVideoAppProcess(String vlcPath, String vlcParams)
        {
            try
            {
                var info = new ProcessStartInfo(vlcPath, vlcParams);
                //info.WindowStyle = ProcessWindowStyle.Minimized;
                m_videoAppProcess = Process.Start(info);
                m_videoAppProcess.PriorityClass = ProcessPriorityClass.AboveNormal;
            }
            catch (Exception ex)
            {
                MLog.Log(ex, this, "Unable to init videoappprocess");
                return false;
            }

            //trying to connect
            if (!TCPConnect(true))
                return false;
            
            //ok, disconnecting
            TCPDisconnect();
            return true;
        }

        protected bool TCPConnect(bool enableRetry)
        {
            for (int i = 0; i < 20; i++)
            {
                if (m_zoneState != Metadata.ZoneState.Closing)
                {
                    try
                    {
                        m_TcpClient = new TcpClient("localhost", m_port);
                        break;
                    }
                    catch (Exception )
                    {
                        //MLog.Log(this, "Trying to connect to videoapp, step " + i);
                        if (!enableRetry)
                            break;
                    }
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(200);
                }
            }

            if ((m_TcpClient != null) && m_TcpClient.Connected)
            {
                //MLog.Log(this, "TCP Connected");
                return true;
            }
            else
            {
                MLog.Log(this, "Unable to init wrapper TCP client");
                return false;
            }
        }

        protected void TCPDisconnect()
        {
            if ((m_TcpClient != null)&&(m_TcpClient.Connected))
                m_TcpClient.Close();
        }

        public bool IsVideoAppProcessAlive
        {
            get
            {
                return VideoAppProcess != null;
            }
        }

        protected string ReadTillEnd()
        {
            StringBuilder sb = new StringBuilder();
                while (m_TcpClient.Connected && m_TcpClient.GetStream().DataAvailable)
                {
                    int b = m_TcpClient.GetStream().ReadByte();
                    if (b >= 0)
                    {
                        sb.Append((char)b);
                    }
                    else
                    {
                        break;
                    }
                }
            return sb.ToString();
        }

        protected string WaitForResult()
        {
            string result = "";
            DateTime start = DateTime.Now;
            while ((DateTime.Now - start).TotalMilliseconds < WaitTimeout)
            {
                result = ReadTillEnd();
                if (!string.IsNullOrEmpty(result))
                {
                    break;
                }
            }
            return result;
        }

        public abstract void GoToFullScreen();

        public abstract void Pause();
        public abstract void Mute();
        public abstract void Play();
        public abstract void VolumeUp();
        public abstract void VolumeDown();
        public abstract void Ffwd();
        public abstract void Rewind();

        public long Position
        {
            get
            {
                return m_position;
            }
        }

        public int PositionPercent
        {
            get
            {
                return Convert.ToInt16(m_position/m_totalposition);
            }
        }

        public int Volume
        {
            get
            {
                return m_volume;
            }
        }

        public Metadata.ZoneState State
        {
            get
            {
                return m_zoneState;
            }
        }
    }

    public class WrapperVlcRemote:WrapperBase
    {
        
        const string VOLUME_DELIM_START = "volume: ";
        const string VOLUME_DELIM_END = " )";
        
        
        public WrapperVlcRemote(String videoFullPath)
        {
            this.m_videoFullPath = videoFullPath;
            /* m_vlcPath = null;
             var vlcKey = Registry.LocalMachine.OpenSubKey(@"Software\VideoLan\VLC");
             if (vlcKey == null)
             {
                 vlcKey = Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\VideoLan\VLC");
             }
             if (vlcKey != null)
             {
                 m_vlcPath = vlcKey.GetValue(null) as string;
             }

             if (m_vlcPath == null)
             {
                 throw new ApplicationException("Can not find the VLC executable!");
             }
             {*/
            //m_videoAppPath = IniFile.PARAM_VIDEO_APP_VLC_PATH[1];

            

        }

        public override bool Initialize()
        {
            m_videoAppprocessName = IniFile.PARAM_VIDEO_APP_VLC_PROCESSNAME[1];
            m_port = 9876;
            String vlcParams = "-I rc --rc-host=localhost:9876 --high-priority --sub-autodetect-file --osd --sub-file=XX YY";
            int pointIndex;
            
            pointIndex = m_videoFullPath.LastIndexOf(".");
            String videoPathNoExtension = m_videoFullPath.Substring(0,pointIndex);
            
            vlcParams = vlcParams.Replace("XX", "\""+videoPathNoExtension+".srt\"");
            vlcParams = vlcParams.Replace("YY", "\""+m_videoFullPath+"\"");

            return StartVideoAppProcess(m_videoAppPath, vlcParams);
            
        }

        public override void Close()
        {
            MLog.Log(null,"Closing vlc");
            try
            {
                m_TcpClient.Close();
            }
            catch (Exception ex)
            {
                MLog.Log(null,"Unable to close tcp conn. " + ex.Message);
            }
            
            try
            {
                m_videoAppProcess.CloseMainWindow();
                m_videoAppProcess.Kill();
                m_videoAppProcess.Close();
            }
            catch (Exception ex)
            {
                MLog.Log(null,"Unable to close process, " + ex.Message);
            }
        }

        

        public void Add(string filename)
        {
            SendCommand(VlcCommand.Add, filename);
        }

        public void Enqueue(string filename)
        {
            SendCommand(VlcCommand.Enqueue, filename);
        }

        public override void Play()
        {
            SendCommand(VlcCommand.Play);
        }

        public void Stop()
        {
            SendCommand(VlcCommand.stop);
        }

        public override void Quit()
        {
            SendCommand(VlcCommand.quit);
        }

        public override void Pause()
        {
            SendCommand(VlcCommand.Pause);
        }

        public void Next()
        {
            SendCommand(VlcCommand.next);
        }

        public void Previous()
        {
            SendCommand(VlcCommand.prev);
        }

        public void Faster()
        {
            SendCommand(VlcCommand.faster);
        }

        public void Slower()
        {
            SendCommand(VlcCommand.slower);
        }

        public void Seek(int seconds)
        {
            SendCommand(VlcCommand.seek, seconds.ToString());
        }

        public override void Ffwd()
        {
        }

        public override void GoToFullScreen()
        {
            SendCommand(VlcCommand.F, "on");
        }

        public override void VolumeUp()
        {
            SendCommand(VlcCommand.volup, "1");
        }

        public override void VolumeDown()
        {
            SendCommand(VlcCommand.voldown, "1");
        }

        public void SetVolume(int volume)
        {
            SendCommand(VlcCommand.volume, volume.ToString());
        }

        public override void Mute()
        {
            SendCommand(VlcCommand.volume, "0");
        }

        public bool IsPlaying
        {
            get
            {
                SendCommand(VlcCommand.Is_Playing);
                string result = WaitForResult().Trim();
                return result == "1";
            }
        }

        public override bool RetrieveStatus()
        {
            m_position = Position;
            m_volume = Volume;
            return true;
        }

        private new int Position
        {
            get
            {
                if (m_TcpClient.Connected)
                {
                    SendCommand(VlcCommand.Get_Time);
                    var result = WaitForResult().Trim();
                    try
                    {
                        return Convert.ToInt32(result);
                    }
                    catch (Exception)
                    { return -1; };
                }
                else
                    return -1;
            }
            set
            {
                SendCommand(VlcCommand.seek, value.ToString());
            }
        }

        private new int Volume
        {
            get
            {
                if (m_TcpClient.Connected)
                {
                    SendCommand(VlcCommand.volume);
                    var result = WaitForResult();
                    int start = result.IndexOf(VOLUME_DELIM_START);
                    int end = result.IndexOf(VOLUME_DELIM_END);
                    try
                    {
                        result = result.Substring(start + VOLUME_DELIM_START.Length, end - start - VOLUME_DELIM_START.Length).Trim();
                        return Convert.ToInt32(result);
                    }
                    catch (Exception )
                    { return -1; };
                }
                else
                    return -1;
            }
            set
            {
                SendCommand(VlcCommand.volume, value.ToString());
            }
        }

        public void FastForward()
        {
            SendCommand(VlcCommand.FastForward);
        }

        public override void Rewind()
        {
            SendCommand(VlcCommand.Rewind);
        }

        

        bool SendCommand(VlcCommand command)
        {
            return SendCommand(command, null);
        }

        bool SendCommand(VlcCommand command, string param)
        {
            try
            {
                // flush old stuff
                ReadTillEnd();
                if (m_TcpClient.Connected)
                {

                    string packet = Enum.GetName(typeof(VlcCommand), command).ToLower();
                    if (param != null)
                    {
                        packet += " " + param;
                    }
                    packet += Environment.NewLine;

                    var buffer = ASCII_Encoding.GetBytes(packet);
                    m_TcpClient.GetStream().Write(buffer, 0, buffer.Length);
                    m_TcpClient.GetStream().Flush();

                    Trace.Write(packet);
                    return true;
                }
                else
                {
                    MLog.Log(null,"VLC TCP not connected for command " + command);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MLog.Log(null,"exception send command " + command + ex.Message);
                return false;
            }
        }

    }

    public class WrapperMPCRemote:WrapperBase
    {

        public WrapperMPCRemote(String videoFullPath)
        {
            this.m_videoFullPath = videoFullPath;
            m_videoAppPath = IniFile.PARAM_VIDEO_APP_MPC_PATH[1];
        }

        public override void Quit()
        {
            SendCommand(MPCCommand.Exit_816, false);
        }

        public override bool Initialize()
        {
            m_videoAppprocessName = IniFile.PARAM_VIDEO_APP_MPC_PROCESSNAME[1];
            m_port = Convert.ToInt16(IniFile.PARAM_VIDEO_APP_MPC_WEBPORT[1]);
            String param = "YY";
            int pointIndex;
            
            pointIndex = m_videoFullPath.LastIndexOf(".");
            String videoPathNoExtension = m_videoFullPath.Substring(0,pointIndex);
            
            //param = param.Replace("XX", "");
            param = param.Replace("YY", "\"" + m_videoFullPath + "\"");

            return StartVideoAppProcess(m_videoAppPath, param);
            
        }

        private bool SendCommand(MPCCommand command)
        {
            return SendCommand(command, true);
        }

        private bool SendCommand(MPCCommand command, bool enableRetry)
        {
            String cmdData;
            String packet="";

            if (!TCPConnect(enableRetry))
            {
                return false;
            }
            cmdData = command.ToString();
            cmdData = cmdData.Substring(cmdData.IndexOf("_")+1);
            cmdData = "wm_command="+cmdData;
            try
            {
                // flush old stuff
                ReadTillEnd();
                if (m_TcpClient.Connected)
                {
                    /*
                    SendLine("POST /command.html HTTP/1.0\r\n");
                    SendLine("HOST: localhost\r\n");
                    SendLine("Content-Type: application/x-www-form-urlencoded\r\n");
                    SendLine("Content-length: " + cmdData.Length + "\r\n");
                    SendLine("\r\n");
                    SendLine(cmdData + "\r\n");
                    SendLine("\r\n");
                    //Trace.Write(packet);
                     */
                    packet += "POST /command.html HTTP/1.0\r\n";
                    packet += "HOST: localhost\r\n";
                    packet += "Content-Type: application/x-www-form-urlencoded\r\n";
                    packet += "Content-length: " + cmdData.Length + "\r\n";
                    packet += "\r\n";
                    packet += cmdData + "\r\n";
                    packet += "\r\n";
                    SendLine(packet);
                    TCPDisconnect();
                    return true;
                }
                else
                {
                    MLog.Log(this,"TCP not connected for command " + command);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MLog.Log(ex,this,"exception send command " + command + ex.Message);
                return false;
            }
        }

        private void SendLine(String line)
        {
            var buffer = ASCII_Encoding.GetBytes(line);
            m_TcpClient.GetStream().Write(buffer, 0, buffer.Length);
            m_TcpClient.GetStream().Flush();
        }

        public override bool RetrieveStatus()
        {
            try
            {
                WebClient client = new WebClient();
                String html = client.DownloadString("http://localhost:" + m_port + "/status.html");
                String[] atoms = Regex.Split(html.Replace("'", ""), ", ");

                m_position = Convert.ToInt32(atoms[2]);
                //// TODO TODO 
                m_totalposition = m_position;

                m_volume = Convert.ToInt32(atoms[7]);
                switch (atoms[1])
                {
                    case "Playing":
                        m_zoneState = Metadata.ZoneState.Running;
                        break;
                    default:
                        m_zoneState = Metadata.ZoneState.Undefined;
                        break;
                }
                return true;
            }
            catch (Exception ex)
            {
                MLog.Log(ex, "unable to retrieve status");
                return false;
            }
        }

        public override void GoToFullScreen()
        {
            SendCommand(MPCCommand.Fullscreen_830);
        }

        public override void Pause()
        {
            SendCommand(MPCCommand.PlayPause_889);
        }

        public override void Play()
        {
        }

        public override void Mute()
        {
            SendCommand(MPCCommand.VolumeMute_909);
        }

        public override void VolumeUp()
        {
            SendCommand(MPCCommand.VolumeUp_907);
        }
        public override void VolumeDown()
        {
            SendCommand(MPCCommand.VolumeDown_908);
        }

        public override void Rewind()
        {
            SendCommand(MPCCommand.JumpBackwardMedium_901);
        }

        public override void Ffwd()
        {
            SendCommand(MPCCommand.JumpForwardMedium_902);
        }

    }
}

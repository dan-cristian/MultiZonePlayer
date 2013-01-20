using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using iSpyApplication.Audio.streams;
using iSpyApplication.Audio.talk;
using iSpyApplication.Controls;
using iSpyApplication.Properties;
using iSpyApplication.Video;
using Microsoft.Win32;
using NAudio.Wave;
using System.Reflection;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using PictureBox = AForge.Controls.PictureBox;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using Timer = System.Timers.Timer;

namespace iSpyApplication
{
    /// <summary>
    /// Summary description for MainForm
    /// </summary>
    public partial class MainForm : Form
    {
        public static bool NeedsSync;
        public static bool LoopBack;
        public static bool StopRecordingFlag;
        public static string NL = Environment.NewLine;
        public static Font Drawfont = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular, GraphicsUnit.Pixel);
        public static Font Iconfont = new Font(FontFamily.GenericSansSerif, 15, FontStyle.Bold, GraphicsUnit.Pixel);
        public static Brush IconBrush = new SolidBrush(Color.White);
        public static Brush IconBrushOff = new SolidBrush(Color.FromArgb(64, 255, 255, 255));
        public static Brush IconBrushActive = new SolidBrush(Color.Red);
        public static Brush OverlayBrush = new SolidBrush(Color.White);

        public static string NextLog = "";
        public static string Identifier;
        public static DataTable IPTABLE;
        public static string IPADDR = "";
        public static string IPCHANNEL = "0";
        public static string IPMODEL = "";
        public static string IPUN = "";
        public static string IPPORTS = "80";
        public static int IPPORT = 80;
        public static string IPPASS = "";
        public static string IPTYPE = "";
        public static string EmailAddress = "", MobileNumber = "";
        public static int ThrottleFramerate = 40;
        public static int CpuUsage, CpuTotal;
        public static int RecordingThreads;
        public object ContextTarget;
        public bool SilentStartup;
        public static List<String> Plugins = new List<String>();
        public static bool NeedsRedraw;
        public McRemoteControlManager.RemoteControlDevice RemoteManager;
        public static List<FilePreview> MasterFileList = new List<FilePreview>();
        public static EncoderParameters EncoderParams;
        public bool reallyclose = false;

        public static string Website = "http://www.ispyconnect.com";
        public static string Webserver = "http://www.ispyconnect.com";
        public static string WebserverSecure = "https://www.ispyconnect.com";

        public static int ButtonWidth
        {
            get { return (Convert.ToInt32(Iconfont.Size)+1); }
        }

        internal static LocalServer MWS;
        internal Player _player;

        private static string PurchaseLink = "http://www.ispyconnect.com/astore.aspx";
        private static int _storageCounter;
        private static Timer RescanIPTimer;
        private string _lastPath = Program.AppDataPath;
        private static bool _logging;
        private static string _counters = "";
        private static readonly Random Random = new Random();
        private static ViewController _vc;
        private static int _pingCounter;
        private static ImageCodecInfo _encoder;
        private static readonly StringBuilder LogFile = new StringBuilder(100000);
        private PTZTool _ptzTool;
        private static readonly string LogTemplate =
            "<html><head><title>iSpy v<!--VERSION--> Log File</title><style type=\"text/css\">body,td,th,div {font-family:Verdana;font-size:10px}</style></head><body><h1>Log Start (v<!--VERSION-->): " +
            DateTime.Now.ToLongDateString() +
            "</h1><p><table cellpadding=\"2px\"><!--CONTENT--></table></p></body></html>";
        private static string _lastlog = "";
        private static List<objectsMicrophone> _microphones;
        private static string _browser = String.Empty;
        private static List<objectsFloorplan> _floorplans;
        private static List<objectsCommand> _remotecommands;
        private static List<objectsCamera> _cameras;
        private static List<PTZSettings2Camera> _ptzs;
        private static List<ManufacturersManufacturer> _sources;
        private PerformanceCounter _cpuCounter, _cputotalCounter, _pcMem;
        private static bool _pcMemAvailable;
        private static IPAddress[] _ipv4Addresses, _ipv6Addresses;
        private Timer _houseKeepingTimer;
        private bool _shuttingDown;
        private string _startCommand = "";
        private Timer _updateTimer;
        private bool _closing;
        private FileSystemWatcher _fsw;
        private MenuItem _aboutHelpItem;
        private ToolStripMenuItem _activateToolStripMenuItem;
        private ToolStripMenuItem _addCameraToolStripMenuItem;
        private ToolStripMenuItem _addFloorPlanToolStripMenuItem;
        private ToolStripMenuItem _addMicrophoneToolStripMenuItem;
        private ToolStripMenuItem _applyScheduleToolStripMenuItem;
        private ToolStripMenuItem _applyScheduleToolStripMenuItem1;
        private IContainer components;
        private ContextMenuStrip ctxtMainForm;
        private ContextMenuStrip ctxtMnu;
        private ContextMenuStrip ctxtTaskbar;
        private ToolStripMenuItem _deleteToolStripMenuItem;
        private ToolStripMenuItem _editToolStripMenuItem;
        private MenuItem _exitFileItem;
        private ToolStripMenuItem _exitToolStripMenuItem;
        private MenuItem _fileItem;
        private ToolStripMenuItem _floorPlanToolStripMenuItem;
        private MenuItem _helpItem;
        private ToolStripMenuItem _helpToolstripMenuItem;
        private ToolStripMenuItem _iPCameraToolStripMenuItem;
        private ToolStripMenuItem _listenToolStripMenuItem;
        private ToolStripMenuItem _localCameraToolStripMenuItem;
        private PersistWindowState _mWindowState;
        private MainMenu mainMenu;
        private MenuItem _menuItem1;
        private MenuItem _menuItem10;
        private MenuItem _menuItem11;
        private MenuItem _menuItem12;
        private MenuItem _menuItem13;
        private MenuItem _menuItem14;
        private MenuItem _menuItem15;
        private MenuItem _menuItem16;
        private MenuItem _menuItem17;
        private MenuItem _menuItem18;
        private MenuItem _menuItem19;
        private MenuItem _menuItem2;
        private MenuItem _menuItem20;
        private MenuItem _menuItem21;
        private MenuItem _menuItem22;
        private MenuItem _menuItem23;
        private MenuItem _menuItem24;
        private MenuItem _menuItem25;
        private MenuItem _menuItem26;
        private MenuItem _menuItem27;
        private MenuItem _menuItem28;
        private MenuItem _menuItem29;
        private MenuItem _menuItem3;
        private MenuItem _menuItem30;
        private MenuItem _menuItem31;
        private MenuItem _menuItem32;
        private MenuItem _menuItem33;
        private MenuItem _menuItem34;
        private MenuItem _menuItem35;
        private MenuItem _menuItem36;
        private MenuItem _menuItem37;
        private MenuItem _menuItem38;
        private MenuItem _menuItem39;
        private MenuItem _menuItem4;
        private MenuItem _menuItem5;
        private MenuItem _menuItem6;
        private MenuItem _menuItem7;
        private MenuItem _menuItem8;
        private MenuItem _menuItem9;
        private MenuItem _miApplySchedule;
        private MenuItem _miOffAll;
        private MenuItem _miOffSched;
        private MenuItem _miOnAll;
        private MenuItem _miOnSched;
        private ToolStripMenuItem _microphoneToolStripMenuItem;
        private NotifyIcon notifyIcon1;
        private ToolStripMenuItem _onMobileDevicesToolStripMenuItem;
        private ToolStripMenuItem _positionToolStripMenuItem;
        private ToolStripMenuItem _recordNowToolStripMenuItem;
        private ToolStripMenuItem _remoteCommandsToolStripMenuItem;
        private ToolStripMenuItem _resetRecordingCounterToolStripMenuItem;
        private ToolStripMenuItem _resetSizeToolStripMenuItem;
        private ToolStripMenuItem _setInactiveToolStripMenuItem;
        private ToolStripMenuItem _settingsToolStripMenuItem;
        private ToolStripMenuItem _showFilesToolStripMenuItem;
        private ToolStripMenuItem _showISpy100PercentOpacityToolStripMenuItem;
        private ToolStripMenuItem _showISpy10PercentOpacityToolStripMenuItem;
        private ToolStripMenuItem _showISpy30OpacityToolStripMenuItem;
        private ToolStripMenuItem _showToolstripMenuItem;
        private StatusStrip statusStrip1;
        private ToolStripMenuItem _switchAllOffToolStripMenuItem;
        private ToolStripMenuItem _switchAllOnToolStripMenuItem;
        private ToolStripMenuItem _takePhotoToolStripMenuItem;
        private ToolStripMenuItem _thruWebsiteToolStripMenuItem;
        private ToolStrip toolStripMenu;
        private ToolStripButton _toolStripButton1;
        private ToolStripButton _toolStripButton4;
        private ToolStripButton _toolStripButton8;
        private ToolStripDropDownButton _toolStripDropDownButton1;
        private ToolStripDropDownButton _toolStripDropDownButton2;
        private ToolStripMenuItem _toolStripMenuItem1;
        private ToolStripStatusLabel _tsslStats;
        private ToolStripMenuItem _uSbCamerasAndMicrophonesOnOtherToolStripMenuItem;
        private ToolStripMenuItem _unlockToolstripMenuItem;
        private ToolStripMenuItem _viewMediaOnAMobileDeviceToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem fullScreenToolStripMenuItem;
        private MenuItem mnuSaveLayout;
        private MenuItem mnuResetLayout;
        private ToolStripMenuItem _websiteToolstripMenuItem;
        private ToolStripMenuItem pTZToolStripMenuItem;
        private ToolStripStatusLabel tsslMonitor;
        private ToolStripStatusLabel tsslPerformance;
        private ToolStripMenuItem inExplorerToolStripMenuItem;
        private MenuItem menuItem1;
        private LayoutPanel _pnlCameras;
        private Panel _pnlContent;
        private SplitContainer splitContainer1;
        private FlowLayoutPanel flowPreview;
        private LinkLabel llblDelete;
        private LinkLabel llblSelectAll;
        private ToolStripMenuItem opacityToolStripMenuItem;
        private ToolStripMenuItem opacityToolStripMenuItem1;
        private ToolStripMenuItem opacityToolStripMenuItem2;
        private ToolStripMenuItem opacityToolStripMenuItem3;
        private ToolStripMenuItem layoutToolStripMenuItem;
        private ToolStripMenuItem autoLayoutToolStripMenuItem;
        private ToolStripMenuItem saveLayoutToolStripMenuItem1;
        private ToolStripMenuItem resetLayoutToolStripMenuItem1;
        private ToolStripMenuItem displayToolStripMenuItem;
        private ToolStripMenuItem fullScreenToolStripMenuItem1;
        private ToolStripMenuItem statusBarToolStripMenuItem;
        private ToolStripMenuItem fileMenuToolStripMenuItem;
        private ToolStripMenuItem toolStripToolStripMenuItem;
        private ToolStripMenuItem alwaysOnTopToolStripMenuItem1;
        private ToolStripMenuItem mediaPaneToolStripMenuItem;
        private FlowLayoutPanel flCommands;
        private SplitContainer splitContainer2;
        private ToolTip toolTip1;
        private FlowLayoutPanel flowLayoutPanel1;
        private Panel panel1;
        private ToolStripMenuItem iPCameraWithWizardToolStripMenuItem;
        private MenuItem menuItem2;
        private MenuItem menuItem9;
        private MenuItem menuItem10;
        private MenuItem menuItem11;
        private MenuItem menuItem12;
        private MenuItem menuItem3;
        private MenuItem menuItem4;
        private MenuItem menuItem5;
        private MenuItem menuItem6;
        private MenuItem menuItem7;
        private MenuItem menuItem8;
        private MenuItem menuItem13;
        private Panel panel2;
        private ToolStripButton tsbPlugins;
        private Thread StorageThread;
        private MenuItem menuItem14;
        private MenuItem menuItem15;
        private MenuItem menuItem16;
        private MenuItem menuItem17;
        private MenuItem menuItem19;
        private MenuItem menuItem20;
        private MenuItem menuItem18;
        private ToolStripMenuItem pTZControllerToolStripMenuItem;
        private ToolStripMenuItem viewControllerToolStripMenuItem;
        private MenuItem menuItem21;
        private MenuItem menuItem22;
        private MenuItem menuItem23;
        private MenuItem menuItem24;
        private ContextMenuStrip ctxtPlayer;
        private ToolStripMenuItem iSpyToolStripMenuItem;
        private ToolStripMenuItem defaultPlayerToolStripMenuItem;
        private ToolStripMenuItem websiteToolStripMenuItem;
        private ToolStripMenuItem deleteToolStripMenuItem;
        private ToolStripMenuItem pTZControllerToolStripMenuItem1;
        private LinkLabel llblRefresh;
        private ToolStripMenuItem showInFolderToolStripMenuItem;
        private ToolStripMenuItem otherVideoSourceToolStripMenuItem;
        private ToolStripMenuItem videoFileToolStripMenuItem;
        private ToolStripMenuItem uploadToYouTubeToolStripMenuItem;
        private ToolStripMenuItem uploadToYouTubePublicToolStripMenuItem;
        private ToolStripMenuItem saveToToolStripMenuItem;
        private static List<LayoutItem> SavedLayout = new List<LayoutItem>();
        private MenuItem menuItem25;
        private MenuItem menuItem26;
        private MenuItem menuItem27;
        private MenuItem menuItem28;
        private MenuItem menuItem29;
        private MenuItem menuItem30;
        private MenuItem menuItem31;
        private FolderBrowserDialog fbdSaveTo = new FolderBrowserDialog()
        {
            ShowNewFolderButton = true,
            Description = "Select a folder to copy the file to"
        };

        public MainForm(bool silent, string command)
        {
            SilentStartup = silent;
            if (Conf.Enable_Password_Protect)
                SilentStartup = true;

            if (!SilentStartup)
            {
                _mWindowState = new PersistWindowState {Parent = this, RegistryPath = @"Software\ispy\startup"};
            }
            InitializeComponent();

            RenderResources();

            _startCommand = command;


            var r = Antiufo.Controls.Windows7Renderer.Instance;
            toolStripMenu.Renderer = r;
            statusStrip1.Renderer = r;

            _pnlCameras.BackColor = Conf.MainColor.ToColor();
            
            if (SilentStartup)
            {
                ShowInTaskbar = false;
                ShowIcon = false;
                WindowState = FormWindowState.Minimized;
            }

            
            RemoteManager = new McRemoteControlManager.RemoteControlDevice();
            RemoteManager.ButtonPressed += RemoteManager_ButtonPressed;
            
            SetPriority();
            Arrange(false);

        }

        private bool IsOnScreen(Form form)
        {
            Screen[] screens = Screen.AllScreens;
            foreach (Screen screen in screens)
            {
                Point formTopLeft = new Point(form.Left, form.Top);

                if (screen.WorkingArea.Contains(formTopLeft))
                {
                    return true;
                }
            }

            return false;
        }

        protected override void WndProc(ref Message message)
        {
            RemoteManager.ProcessMessage(message);
            base.WndProc(ref message);
        }


        private void RemoteManager_ButtonPressed(object sender, McRemoteControlManager.RemoteControlEventArgs e)
        {
            ProcessKey(e.Button.ToString().ToLower());
        }

        public static void SetPriority()
        {
            switch (Conf.Priority)
            {
                case 1:
                    Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;
                    break;
                case 2:
                    Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;
                    break;
                case 3:
                    Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
                    break;
                case 4:
                    Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
                    break;
            }

        }

       

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                notifyIcon1.Visible = false;
                notifyIcon1.Dispose();

                if (components != null)
                {
                    components.Dispose();
                }
                if (_mWindowState != null)
                    _mWindowState.Dispose();

                Drawfont.Dispose();
                if (_updateTimer != null)
                    _updateTimer.Dispose();
                if (_houseKeepingTimer != null)
                    _houseKeepingTimer.Dispose();
                if (_fsw != null)
                    _fsw.Dispose();
                if (fbdSaveTo!=null)
                    fbdSaveTo.Dispose();
            }
            base.Dispose(disposing);
        }

        // Close the main form
        private void ExitFileItemClick(object sender, EventArgs e)
        {
            reallyclose = true;
            Close();
        }

        // On "Help->About"
        private void AboutHelpItemClick(object sender, EventArgs e)
        {
            var form = new AboutForm();
            form.ShowDialog(this);
            form.Dispose();
        }

        private void VolumeControlDoubleClick(object sender, EventArgs e)
        {
            Maximise(sender);
        }

        private void FloorPlanDoubleClick(object sender, EventArgs e)
        {
            Maximise(sender);
        }

        private static string Zeropad(int i)
        {
            if (i > 9)
                return i.ToString();
            return "0" + i;
        }

        private void AddPlugin(FileInfo dll)
        {
            try
            {
                Assembly plugin = Assembly.LoadFrom(dll.FullName);
                object ins = plugin.CreateInstance("Plugins.Main", true);
                if (ins != null)
                {
                    LogMessageToFile("Added: " + dll.FullName);
                    Plugins.Add(dll.FullName);
                }

            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
            }
        }

        

        public void Play(string filename, int objectId)
        {
            if (_player == null)
            {
                _player = new Player();
                _player.Show(this);
                _player.Closed += PlayerClosed;
            }
            _player.Owner = this;
            _player.Activate();
            _player.BringToFront();
            _player.ObjectID = objectId;
            _player.Play(filename);


        }

        private void PlayerClosed(object sender, EventArgs e)
        {
            _player = null;
        }

        private void InitLogging()
        {
            DateTime logdate = DateTime.Now;

            FileInfo fi;
            foreach (string s in Directory.GetFiles(Program.AppDataPath, "log_*", SearchOption.TopDirectoryOnly))
            {
                fi = new FileInfo(s);
                if (fi.CreationTime < DateTime.Now.AddDays(-5))
                    FileOperations.Delete(s);
            }
            NextLog = Zeropad(logdate.Day) + Zeropad(logdate.Month) + logdate.Year;
            int i = 1;
            if (File.Exists(Program.AppDataPath + "log_" + NextLog + ".htm"))
            {
                while (File.Exists(Program.AppDataPath + "log_" + NextLog + "_" + i + ".htm"))
                    i++;
                NextLog += "_" + i;
            }
            try
            {
                File.WriteAllText(Program.AppDataPath + "log_" + NextLog + ".htm", DateTime.Now + Environment.NewLine);
                _logging = true;
            }
            catch (Exception ex)
            {
                if (
                    MessageBox.Show(LocRm.GetString("LogStartError").Replace("[MESSAGE]", ex.Message),
                                    LocRm.GetString("Warning"), MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    reallyclose = true;
                    Close();
                    return;
                }
            }
        }

        private void MainFormLoad(object sender, EventArgs e)
        {
            UISync.Init(this);

            try
            {
                File.WriteAllText(Program.AppDataPath + "exit.txt", "RUNNING");
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
            }

            InitLogging();
            
            EncoderParams = new EncoderParameters(1);
            EncoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, Conf.JPEGQuality);

            //this initializes the port mapping collection
            NATUPNPLib.IStaticPortMappingCollection map = NATControl.Mappings;
            if (Conf.MediaDirectory == null || Conf.MediaDirectory == "NotSet")
            {
                Conf.MediaDirectory = Program.AppDataPath + @"WebServerRoot\Media\";
            }
            if (!Directory.Exists(Conf.MediaDirectory))
            {
                string notfound = Conf.MediaDirectory;
                //Conf.MediaDirectory = Program.AppDataPath + @"WebServerRoot\Media\";
                LogErrorToFile("Media directory could not be found (" + notfound + ") - reset it to " +
                                 Program.AppDataPath + @"WebServerRoot\Media\" + " in settings if it doesn't attach.");
            }

            if (!VlcHelper.VlcInstalled)
            {
                LogWarningToFile("VLC not installed - install VLC (x86) for extra connectivity and inbuilt video playback.");
            }
            else
            {
                var v = VlcHelper.VlcVersion;
                if (v.CompareTo(VlcHelper.VMin) < 0)
                {
                    LogWarningToFile("Old VLC installed - update VLC (x86) for extra connectivity and inbuilt video playback.");
                }
                else
                {
                    if (v.CompareTo(new Version(2,0,2))==0)
                    {
                        LogWarningToFile("VLC v2.0.2 detected - there are known issues with this version of VLC (HTTP streaming is broken for a lot of cameras) - if you are having problems with VLC connectivity we recommend you install v2.0.1 ( http://download.videolan.org/pub/videolan/vlc/2.0.1/ ) or the latest (if available).");
                    }
                }
            }

            
            _fsw = new FileSystemWatcher
                       {
                           Path = Program.AppDataPath,
                           IncludeSubdirectories = false,
                           Filter = "external_command.txt",
                           NotifyFilter = NotifyFilters.LastWrite
                       };
            _fsw.Changed += FswChanged;
            _fsw.EnableRaisingEvents = true;
            GC.KeepAlive(_fsw);

           
            Menu = mainMenu;
            notifyIcon1.ContextMenuStrip = ctxtTaskbar;
            Identifier = Guid.NewGuid().ToString();
            MWS = new LocalServer(this)
                      {
                          ServerRoot = Program.AppDataPath + @"WebServerRoot\",
                      };

            if (Conf.Monitor)
            {
                var w = Process.GetProcessesByName("ispymonitor");
                if (w.Length == 0)
                {
                    try
                    {
                        var si = new ProcessStartInfo(Program.AppPath + "/ispymonitor.exe", "ispy");
                        Process.Start(si);
                    }
                    catch
                    {
                    }
                }
            }

            GC.KeepAlive(Program.Mutex);
            GC.KeepAlive(MWS);

            SetBackground();

            toolStripMenu.Visible = Conf.ShowToolbar;
            statusStrip1.Visible = Conf.ShowStatus;
            Menu = !Conf.ShowFileMenu ? null : mainMenu;


            if (Conf.Fullscreen && !SilentStartup)
            {
                WindowState = FormWindowState.Maximized;
                FormBorderStyle = FormBorderStyle.None;
                WinApi.SetWinFullScreen(Handle);
            }

            statusBarToolStripMenuItem.Checked = menuItem4.Checked = Conf.ShowStatus;
            toolStripToolStripMenuItem.Checked = menuItem6.Checked = Conf.ShowToolbar;
            fileMenuToolStripMenuItem.Checked = menuItem5.Checked = Conf.ShowFileMenu;
            fullScreenToolStripMenuItem1.Checked = menuItem3.Checked = Conf.Fullscreen;
            alwaysOnTopToolStripMenuItem1.Checked = menuItem8.Checked = Conf.AlwaysOnTop;
            mediaPaneToolStripMenuItem.Checked = menuItem7.Checked = Conf.ShowMediaPanel;
            menuItem22.Checked = Conf.LockLayout;
            TopMost = Conf.AlwaysOnTop;

            Iconfont = new Font(FontFamily.GenericSansSerif, Conf.BigButtons ? 22 : 15, FontStyle.Bold, GraphicsUnit.Pixel);

            double dOpacity;
            Double.TryParse(Conf.Opacity.ToString(), out dOpacity);
            Opacity = dOpacity/100.0;



            if (Conf.ServerName == "NotSet")
            {
                Conf.ServerName = SystemInformation.ComputerName;
            }

            notifyIcon1.Text = Conf.TrayIconText;
            notifyIcon1.BalloonTipClicked += NotifyIcon1BalloonTipClicked;
            autoLayoutToolStripMenuItem.Checked = Conf.AutoLayout;

            _updateTimer = new Timer(500);
            _updateTimer.Elapsed += UpdateTimerElapsed;
            _updateTimer.AutoReset = true;
            _updateTimer.SynchronizingObject = this;
            GC.KeepAlive(_updateTimer);

            _houseKeepingTimer = new Timer(1000);
            _houseKeepingTimer.Elapsed += HouseKeepingTimerElapsed;
            _houseKeepingTimer.AutoReset = true;
            _houseKeepingTimer.SynchronizingObject = this;
            GC.KeepAlive(_houseKeepingTimer);

            //load plugins
            var plugindir = new DirectoryInfo(Program.AppPath + "Plugins");
            LogMessageToFile("Checking Plugins...");
            foreach (var dll in plugindir.GetFiles("*.dll"))
            {

                AddPlugin(dll);
            }
            foreach (DirectoryInfo d in plugindir.GetDirectories())
            {
                LogMessageToFile(d.Name);
                foreach (var dll in d.GetFiles("*.dll"))
                {
                    AddPlugin(dll);
                }
            }

            resetLayoutToolStripMenuItem1.Enabled = mnuResetLayout.Enabled = false; //reset layout

            NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;
            mediaPaneToolStripMenuItem.Checked = Conf.ShowMediaPanel;
            ShowHideMediaPane();
            if (!String.IsNullOrEmpty(Conf.MediaPanelSize))
            {
                string[] dd = Conf.MediaPanelSize.Split('x');
                int d1 = Convert.ToInt32(dd[0]);
                int d2 = Convert.ToInt32(dd[1]);
                try
                {
                    splitContainer1.SplitterDistance = d1;
                    splitContainer2.SplitterDistance = d2;
                }
                catch
                {

                }
            }
            //load in object list

            if (_startCommand.Trim().StartsWith("open"))
            {
                ParseCommand(_startCommand);
                _startCommand = "";
            }
            else
            {
                if (!File.Exists(Program.AppDataPath + @"XML\objects.xml"))
                {
                    File.Copy(Program.AppPath + @"XML\objects.xml", Program.AppDataPath + @"XML\objects.xml");
                }
                ParseCommand("open " + Program.AppDataPath + @"XML\objects.xml");
            }
            if (_startCommand != "")
            {
                ParseCommand(_startCommand);
            }

            StopAndStartServer();
            var t = new Thread(ConnectServices) {IsBackground = false};
            t.Start();

            if (SilentStartup)
            {
                _mWindowState = new PersistWindowState {Parent = this, RegistryPath = @"Software\ispy\startup"};
            }

            _updateTimer.Start();
            _houseKeepingTimer.Start();

            if (Conf.RunTimes == 0)
                ShowGettingStarted();


            if (!String.IsNullOrEmpty(Conf.Vendor))
            {
                var pb = new PictureBox();
                switch (Conf.Vendor.ToLower())
                {
                    case "ensidio":
                        pb.Image = Resources.ensidio;
                        PurchaseLink = "http://www.ensidio.com/";
                        break;
                    case "tenvis":
                        pb.Image = Resources.TENVIS;
                        PurchaseLink = "http://www.tenvis.com/";
                        break;
                    case "smartisp":
                        pb.Image = Resources.smartisp;
                        break;
                    case "addplus":
                        pb.Image = Resources.Addplus;
                        break;
                    case "foscam":
                        pb.Image = Resources.foscam;
                        PurchaseLink = "http://www.foscam.com/";
                        break;
                    case "phyxius":
                        pb.Image = Resources.phyxius;
                        break;
                    case "bigdipper":
                        pb.Image = Resources.bigdipper;
                        break;
                    case "allnet gmbh":
                        pb.Image = Resources.ALLNET;
                        PurchaseLink = "http://www.allnet.de/";
                        break;
                    case "eos":
                        pb.Image = Resources.EOSLogo;
                        PurchaseLink = "http://nowyoucansee.com/";
                        break;
                }
                pb.Width = pb.Image.Width;
                pb.Height = pb.Image.Height;

                pb.Left = _pnlCameras.Width/2 - pb.Width/2;
                pb.Top = _pnlCameras.Height/2 - pb.Height/2;

                _pnlCameras.Controls.Add(pb);
                _pnlCameras.BrandedImage = pb;
                
            }

            LoadCommands();
            if (!SilentStartup && Conf.ViewController)
            {
                ShowViewController();
                viewControllerToolStripMenuItem.Checked = menuItem14.Checked = true;
            }

            pTZControllerToolStripMenuItem.Checked = menuItem18.Checked = pTZControllerToolStripMenuItem1.Checked = Conf.ShowPTZController;

            if (Conf.ShowPTZController)
                ShowHidePTZTool();
            
            Conf.RunTimes++;

            try
            {
                _cputotalCounter = new PerformanceCounter("Processor", "% Processor Time", "_total", true);
                _cpuCounter = new PerformanceCounter("Process", "% Processor Time",
                                                     Process.GetCurrentProcess().ProcessName, true);
                try
                {
                    _pcMem = new PerformanceCounter("Process", "Working Set - Private",
                                                    Process.GetCurrentProcess().ProcessName, true);
                }
                catch
                {
                    //no working set - only total available on windows xp
                    try
                    {
                        _pcMem = new PerformanceCounter("Memory", "Available MBytes");
                        _pcMemAvailable = true;
                    }
                    catch (Exception ex2)
                    {
                        LogExceptionToFile(ex2);
                        _pcMem = null;
                    }
                }

            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
                _cputotalCounter = null;
            }

        }       

        private static void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            //schedule update check for a few seconds as a network change involves 2 calls to this event - removing and adding.
            if (RescanIPTimer == null)
            {
                RescanIPTimer = new Timer(5000);
                RescanIPTimer.Elapsed += RescanIPTimer_Elapsed;
                RescanIPTimer.Start();
            }

        }

        private static void RescanIPTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            RescanIPTimer.Stop();
            RescanIPTimer = null;
            if (Conf.IPMode == "IPv4")
            {
                _ipv4Addresses = null;
                bool iplisted = false;
                foreach (IPAddress ip in AddressListIPv4)
                {
                    if (Conf.IPv4Address == ip.ToString())
                        iplisted = true;
                }
                if (!iplisted)
                {

                    _ipv4Address = "";
                    Conf.IPv4Address = AddressIPv4;
                }
                if (iplisted)
                    return;
            }
            if (!String.IsNullOrEmpty(Conf.WSUsername) && !String.IsNullOrEmpty(Conf.WSPassword))
            {
                switch (Conf.IPMode)
                {
                    case "IPv4":
                        LogErrorToFile(
                        "Your IP address has changed. Please set a static IP address for your local computer to ensure uninterrupted connectivity.");
                        if (Conf.DHCPReroute && Conf.IPMode == "IPv4")
                        {
                            //check if IP address has changed
                            if (Conf.UseUPNP)
                            {
                                //change router ports
                                if (NATControl.SetPorts(Conf.ServerPort, Conf.LANPort))
                                    LogMessageToFile("Router port forwarding has been updated. (" +
                                                     Conf.IPv4Address + ")");
                            }
                            else
                            {
                                LogMessageToFile("Please check Use UPNP in web settings to handle this automatically");
                            }
                        }
                        else
                        {
                            LogMessageToFile("Enable DHCP Reroute in Web Settings to handle this automatically");
                        }
                        WsWrapper.ForceSync();
                        break;
                    case "IPv6":
                         _ipv6Addresses = null;
                        bool iplisted = false;
                        foreach (IPAddress ip in AddressListIPv6)
                        {
                            if (Conf.IPv6Address == ip.ToString())
                                iplisted = true;
                        }
                        if (!iplisted)
                        {
                            LogErrorToFile(
                                "Your IP address has changed. Please set a static IP address for your local computer to ensure uninterrupted connectivity.");
                            _ipv6Address = "";
                            Conf.IPv6Address = AddressIPv6;
                        }
                        break;
                }
            }
        }


        private void RenderResources()
        {
            Text = "iSpy v" + Application.ProductVersion;
            if (!String.IsNullOrEmpty(Conf.Vendor))
            {
                Text += " with " + Conf.Vendor;
            }
            _aboutHelpItem.Text = LocRm.GetString("About");
            _activateToolStripMenuItem.Text = LocRm.GetString("Switchon");
            _addCameraToolStripMenuItem.Text = LocRm.GetString("AddCamera");
            _addFloorPlanToolStripMenuItem.Text = LocRm.GetString("AddFloorplan");
            _addMicrophoneToolStripMenuItem.Text = LocRm.GetString("Addmicrophone");
            autoLayoutToolStripMenuItem.Text = LocRm.GetString("AutoLayout");

            _deleteToolStripMenuItem.Text = LocRm.GetString("remove");
            _editToolStripMenuItem.Text = LocRm.GetString("Edit");
            _exitFileItem.Text = LocRm.GetString("Exit");
            _exitToolStripMenuItem.Text = LocRm.GetString("Exit");
            _fileItem.Text = LocRm.GetString("file");
            fileMenuToolStripMenuItem.Text = LocRm.GetString("Filemenu");
            menuItem5.Text = LocRm.GetString("Filemenu");
            _floorPlanToolStripMenuItem.Text = LocRm.GetString("FloorPlan");
            fullScreenToolStripMenuItem.Text = LocRm.GetString("fullScreen");
            fullScreenToolStripMenuItem1.Text = LocRm.GetString("fullScreen");
            _helpItem.Text = LocRm.GetString("help");
            _helpToolstripMenuItem.Text = LocRm.GetString("help");
            _iPCameraToolStripMenuItem.Text = LocRm.GetString("IpCamera");
            _menuItem24.Text = LocRm.GetString("ShowGettingStarted");
            _listenToolStripMenuItem.Text = LocRm.GetString("Listen");
            _localCameraToolStripMenuItem.Text = LocRm.GetString("LocalCamera");
            _menuItem1.Text = LocRm.GetString("chars_2949165");
            _menuItem10.Text = LocRm.GetString("checkForUpdates");
            _menuItem11.Text = LocRm.GetString("reportBugFeedback");
            _menuItem13.Text = LocRm.GetString("chars_2949165");
            _menuItem15.Text = LocRm.GetString("ResetAllRecordingCounters");
            _menuItem16.Text = LocRm.GetString("View");
            _menuItem17.Text = inExplorerToolStripMenuItem.Text = LocRm.GetString("files");
            _menuItem18.Text = LocRm.GetString("clearCaptureDirectories");
            _menuItem19.Text = LocRm.GetString("saveObjectList");
            _menuItem2.Text = LocRm.GetString("help");
            _menuItem20.Text = LocRm.GetString("Logfile");
            _menuItem21.Text = LocRm.GetString("openObjectList");
            _menuItem22.Text = LocRm.GetString("LogFiles");
            _menuItem23.Text = LocRm.GetString("audiofiles");
            _menuItem25.Text = LocRm.GetString("MediaOnAMobiledeviceiphon");
            _menuItem26.Text = LocRm.GetString("supportIspyWithADonation");
            _menuItem27.Text = LocRm.GetString("chars_2949165");
            _menuItem29.Text = LocRm.GetString("Current");
            _menuItem3.Text = LocRm.GetString("MediaoverTheWeb");
            _menuItem30.Text = LocRm.GetString("chars_2949165");
            _menuItem31.Text = LocRm.GetString("removeAllObjects");
            _menuItem32.Text = LocRm.GetString("chars_2949165");
            _menuItem33.Text = LocRm.GetString("switchOff");
            _menuItem34.Text = LocRm.GetString("Switchon");
            _miOnAll.Text = LocRm.GetString("All");
            _miOffAll.Text = LocRm.GetString("All");
            _miOnSched.Text = LocRm.GetString("Scheduled");
            _miOffSched.Text = LocRm.GetString("Scheduled");
            _miApplySchedule.Text = _applyScheduleToolStripMenuItem1.Text = LocRm.GetString("ApplySchedule");
            _applyScheduleToolStripMenuItem.Text = LocRm.GetString("ApplySchedule");
            _menuItem35.Text = LocRm.GetString("ConfigureremoteCommands");
            _menuItem36.Text = LocRm.GetString("Edit");
            _menuItem37.Text = LocRm.GetString("CamerasAndMicrophones");
            _menuItem38.Text = LocRm.GetString("ViewUpdateInformation");
            _menuItem39.Text = LocRm.GetString("AutoLayoutObjects");
            _menuItem4.Text = LocRm.GetString("ConfigureremoteAccess");
            _menuItem5.Text = LocRm.GetString("GoTowebsite");
            _menuItem6.Text = LocRm.GetString("chars_2949165");
            _menuItem7.Text = LocRm.GetString("videofiles");
            _menuItem8.Text = LocRm.GetString("settings");
            _menuItem9.Text = LocRm.GetString("options");
            _microphoneToolStripMenuItem.Text = LocRm.GetString("Microphone");
            notifyIcon1.Text = LocRm.GetString("Ispy");
            _onMobileDevicesToolStripMenuItem.Text = LocRm.GetString("MobileDevices");

            opacityToolStripMenuItem.Text = LocRm.GetString("Opacity");
            opacityToolStripMenuItem1.Text = LocRm.GetString("Opacity10");
            opacityToolStripMenuItem2.Text = LocRm.GetString("Opacity30");
            opacityToolStripMenuItem3.Text = LocRm.GetString("Opacity100");

            menuItem9.Text = LocRm.GetString("Opacity");
            menuItem10.Text = LocRm.GetString("Opacity10");
            menuItem11.Text = LocRm.GetString("Opacity30");
            menuItem12.Text = LocRm.GetString("Opacity100");


            _positionToolStripMenuItem.Text = LocRm.GetString("Position");
            _recordNowToolStripMenuItem.Text = LocRm.GetString("RecordNow");
            _remoteCommandsToolStripMenuItem.Text = LocRm.GetString("RemoteCommands");
            _resetRecordingCounterToolStripMenuItem.Text = LocRm.GetString("ResetRecordingCounter");
            _resetSizeToolStripMenuItem.Text = LocRm.GetString("ResetSize");
            _setInactiveToolStripMenuItem.Text = LocRm.GetString("switchOff");
            _settingsToolStripMenuItem.Text = LocRm.GetString("settings");
            _showFilesToolStripMenuItem.Text = LocRm.GetString("ShowFiles");
            _showISpy100PercentOpacityToolStripMenuItem.Text = LocRm.GetString("ShowIspy100Opacity");
            _showISpy10PercentOpacityToolStripMenuItem.Text = LocRm.GetString("ShowIspy10Opacity");
            _showISpy30OpacityToolStripMenuItem.Text = LocRm.GetString("ShowIspy30Opacity");
            _showToolstripMenuItem.Text = LocRm.GetString("showIspy");
            statusBarToolStripMenuItem.Text = LocRm.GetString("Statusbar");
            menuItem4.Text = LocRm.GetString("Statusbar");
            _switchAllOffToolStripMenuItem.Text = LocRm.GetString("SwitchAllOff");
            _switchAllOnToolStripMenuItem.Text = LocRm.GetString("SwitchAllOn");
            _takePhotoToolStripMenuItem.Text = LocRm.GetString("TakePhoto");
            _thruWebsiteToolStripMenuItem.Text = LocRm.GetString("Online");
            _toolStripButton1.Text = LocRm.GetString("WebSettings");
            _toolStripButton4.Text = LocRm.GetString("settings");
            _toolStripButton8.Text = LocRm.GetString("Commands");
            _toolStripDropDownButton1.Text = LocRm.GetString("AccessMedia");
            _toolStripDropDownButton2.Text = LocRm.GetString("Add");
            _toolStripMenuItem1.Text = LocRm.GetString("Viewmedia");
            toolStripToolStripMenuItem.Text = LocRm.GetString("toolStrip");
            menuItem6.Text = LocRm.GetString("toolStrip");
            _tsslStats.Text = LocRm.GetString("Loading");
            _unlockToolstripMenuItem.Text = LocRm.GetString("unlock");
            _viewMediaOnAMobileDeviceToolStripMenuItem.Text = LocRm.GetString("ViewMediaOnAMobiledevice");
            _websiteToolstripMenuItem.Text = LocRm.GetString("website");
            _uSbCamerasAndMicrophonesOnOtherToolStripMenuItem.Text =
                LocRm.GetString("CamerasAndMicrophonesOnOtherComputers");
            fullScreenToolStripMenuItem.Text = LocRm.GetString("Fullscreen");
            menuItem3.Text = LocRm.GetString("Fullscreen");
            alwaysOnTopToolStripMenuItem1.Text = LocRm.GetString("AlwaysOnTop");
            menuItem8.Text = LocRm.GetString("AlwaysOnTop");
            llblSelectAll.Text = LocRm.GetString("SelectAll");
            llblDelete.Text = LocRm.GetString("Delete");
            menuItem13.Text = LocRm.GetString("PurchaseMoreCameras");
            _exitToolStripMenuItem.Text = LocRm.GetString("Exit");

            layoutToolStripMenuItem.Text = LocRm.GetString("Layout");
            displayToolStripMenuItem.Text = LocRm.GetString("Display");

            mnuSaveLayout.Text = saveLayoutToolStripMenuItem1.Text = LocRm.GetString("SaveLayout");
            mnuResetLayout.Text = resetLayoutToolStripMenuItem1.Text = LocRm.GetString("ResetLayout");
            mediaPaneToolStripMenuItem.Text = LocRm.GetString("ShowMediaPanel");
            menuItem7.Text = LocRm.GetString("ShowMediaPanel");
            iPCameraWithWizardToolStripMenuItem.Text = LocRm.GetString("IPCameraWithWizard");
            tsbPlugins.Text = LocRm.GetString("Plugins");

            menuItem14.Text = viewControllerToolStripMenuItem.Text = LocRm.GetString("ViewController");

            llblRefresh.Text = LocRm.GetString("Reload");

        }


        private void HouseKeepingTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _houseKeepingTimer.Stop();
            if (NeedsRedraw)
            {
                _pnlCameras.Invalidate();
                NeedsRedraw = false;
            }
            if (_cputotalCounter != null)
            {
                try
                {
                    CpuUsage = Convert.ToInt32(_cpuCounter.NextValue())/Environment.ProcessorCount;
                    CpuTotal = Convert.ToInt32(_cputotalCounter.NextValue());
                    _counters = "CPU: " + CpuUsage + "%";

                    if (_pcMem != null)
                    {
                        if (_pcMemAvailable)
                            _counters += " RAM Available: " + Convert.ToInt32(_pcMem.NextValue()) + "Mb";
                        else
                            _counters += " RAM Usage: " + Convert.ToInt32(_pcMem.RawValue/1048576) + "Mb";
                    }
                    tsslMonitor.Text = _counters;
                }
                catch (Exception ex)
                {
                    _cputotalCounter = null;
                    LogExceptionToFile(ex);
                }
                if (CpuTotal > _conf.CPUMax)
                {
                    if (ThrottleFramerate > 1)
                        ThrottleFramerate--;
                }
                else
                {
                    if (ThrottleFramerate < 40)
                        ThrottleFramerate++;
                }
            }
            else
            {
                _counters = "Stats Unavailable - See Log File";
            }

            _pingCounter++;

            if (_pingCounter == 301)
            {
                _pingCounter = 0;
                //auto save
                try
                {
                    SaveObjects("");
                }
                catch (Exception ex)
                {
                    LogExceptionToFile(ex);
                }
                try
                {
                    SaveConfig();
                }
                catch (Exception ex)
                {
                    LogExceptionToFile(ex);
                }
            }
            try
            {
                if (!MWS.Running)
                {
                    _tsslStats.Text = "Server Error - see log file";
                    if (MWS.NumErr >= 5)
                    {
                        LogMessageToFile("Server not running - restarting");
                        StopAndStartServer();
                    }
                }
                else
                {
                    if (WsWrapper.WebsiteLive && Conf.ServicesEnabled)
                    {
                        _tsslStats.Text = LocRm.GetString("Online");
                        if (LoopBack && Conf.Subscribed)
                            _tsslStats.Text += " (" + LocRm.GetString("loopback") + ")";
                        else
                        {
                            if (!Conf.Subscribed)
                                _tsslStats.Text += " (" + LocRm.GetString("LANonlynotsubscribed") + ")";
                            else
                                _tsslStats.Text += " (" + LocRm.GetString("LANonlyNoLoopback") + ")";
                        }
                    }
                    else
                        _tsslStats.Text = LocRm.GetString("Offline");
                }
                
                

               

                if (Conf.ServicesEnabled)
                {
                    try
                    {
                        if (NeedsSync)
                        {
                            WsWrapper.ForceSync();
                            NeedsSync = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Housekeeping Error: " + ex.Message);
                    }


                    if (_pingCounter == 180)
                    {
                        WsWrapper.PingServer();
                    }
                }
                if (Conf.Enable_Storage_Management)
                {
                    _storageCounter++;
                    if (_storageCounter == 3600) // every hour
                    {
                        RunStorageManagement();
                        _storageCounter = 0;
                    }
                }

                if (_pingCounter == 80)
                {
                    var t = new Thread(SaveFileData) {IsBackground = true, Name = "Saving File Data"};
                    t.Start();
                }
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
            }
            WriteLog();
            if (!_shuttingDown)
                _houseKeepingTimer.Start();
        }

        private delegate void RunStorageManagementDelegate();
        public void RunStorageManagement()
        {
            if (InvokeRequired)
            {
                Invoke(new RunStorageManagementDelegate(RunStorageManagement));
                return;
            }

            
            if (StorageThread == null || !StorageThread.IsAlive)
            {
                LogMessageToFile("Running Storage Management");
                StorageThread = new Thread(DeleteOldFiles) { IsBackground = true };
                StorageThread.Start();
            }
            else
                LogMessageToFile("Storage Management is already running");
        }

        private void UpdateTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _updateTimer.Stop();

            foreach (Control c in _pnlCameras.Controls)
            {
                try
                {
                    if (c is CameraWindow)
                    {
                        ((CameraWindow) c).Tick();
                    }
                    if (c is VolumeLevel)
                    {
                        ((VolumeLevel) c).Tick();
                    }
                    if (c is FloorPlanControl)
                    {
                        var fpc = ((FloorPlanControl) c);
                        if (fpc.Fpobject.needsupdate)
                        {
                            fpc.NeedsRefresh = true;
                            fpc.Fpobject.needsupdate = false;
                        }
                        fpc.Tick();
                    }
                }
                catch (Exception ex)
                {
                    LogExceptionToFile(ex);
                }
            }
            if (!_shuttingDown)
                _updateTimer.Start();
        }

        private void FswChanged(object sender, FileSystemEventArgs e)
        {
            _fsw.EnableRaisingEvents = false;
            bool err = true;
            int i = 0;
            try
            {
                string txt = "";
                while (err && i < 5)
                {
                    try
                    {
                        using (var fs = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            using (var sr = new StreamReader(fs))
                            {
                                while (sr.EndOfStream == false)
                                {
                                    txt = sr.ReadLine();
                                    err = false;
                                }
                                sr.Close();
                            }
                            fs.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        LogExceptionToFile(ex);
                        i++;
                        Thread.Sleep(500);
                    }
                }
                if (txt != null)
                    if (txt.Trim() != "")
                        ParseCommand(txt);
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
            }
            _fsw.EnableRaisingEvents = true;
        }

        private void ParseCommand(string command)
        {
            if (command == null) throw new ArgumentNullException("command");
            try
            {
                command = Uri.UnescapeDataString(command);

                LogMessageToFile("Running External Command: " + command);

                if (command.ToLower().StartsWith("open "))
                {
                    if (InvokeRequired)
                        Invoke(new ExternalCommandDelegate(LoadObjectList), command.Substring(5).Trim('"'));
                    else
                        LoadObjectList(command.Substring(5).Trim('"'));
                }
                int i = command.ToLower().IndexOf("commands ");
                if (i!=-1)
                {
                    string cmd = command.Substring(i+9).Trim('"');
                    string[] commands = cmd.Split('|');
                    foreach (string command2 in commands)
                    {
                        if (!String.IsNullOrEmpty(command2))
                        {
                            if (InvokeRequired)
                                Invoke(new ExternalCommandDelegate(ProcessCommandInternal), command2.Trim('"'));
                            else
                                ProcessCommandInternal(command2.Trim('"'));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
                MessageBox.Show(LocRm.GetString("LoadFailed").Replace("[MESSAGE]", ex.Message));
            }
        }

        internal static void ProcessCommandInternal(string command)
        {
            //parse command into new format
            string[] cfg = command.Split(',');
            string newcommand;
            if (cfg.Length == 1)
                newcommand = cfg[0];
            else
            {
                newcommand = cfg[0] + "?ot=" + cfg[1] + "&oid=" + cfg[2];
            }
            MWS.ProcessCommandInternal(newcommand);
        }

        public void SetBackground()
        {
            _pnlCameras.BackColor = Conf.MainColor.ToColor();
            _pnlContent.BackColor = SystemColors.AppWorkspace;
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                notifyIcon1.Visible = false;

                notifyIcon1.Icon.Dispose();
                notifyIcon1.Dispose();
            }
            catch
            {
            }
            base.OnClosed(e);
        }

        private void MenuItem2Click(object sender, EventArgs e)
        {
            StartBrowser(MainForm.Website + "/userguide.aspx");
        }

        internal static string StopAndStartServer()
        {
            string message = "";
            try
            {
                MWS.StopServer();
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
            }

            Application.DoEvents();
            try
            {
                message = MWS.StartServer();
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
            }
            return message;
        }

        private void MenuItem4Click(object sender, EventArgs e)
        {
            WebConnect();
        }

        private void MenuItem5Click(object sender, EventArgs e)
        {
            StartBrowser(Website + "/");
        }

        private void MenuItem10Click(object sender, EventArgs e)
        {
            CheckForUpdates(false);
        }

        private void CheckForUpdates(bool suppressMessages)
        {
            string version = "";
            try
            {
                version = WsWrapper.ProductLatestVersion(11);
                if (version == LocRm.GetString("iSpyDown"))
                {
                    throw new Exception("down");
                }
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
                if (!suppressMessages)
                    MessageBox.Show(LocRm.GetString("CheckUpdateError"), LocRm.GetString("Error"));
            }
            if (version != "" && version != LocRm.GetString("iSpyDown"))
            {
                var verThis = new Version(Application.ProductVersion);
                var verLatest = new Version(version);
                if (verThis < verLatest)
                {
                    var nv = new NewVersion();
                    nv.ShowDialog(this);
                    nv.Dispose();
                }
                else
                {
                    if (!suppressMessages)
                        MessageBox.Show(LocRm.GetString("HaveLatest"), LocRm.GetString("Note"), MessageBoxButtons.OK);
                }
            }
        }

        private void MenuItem8Click(object sender, EventArgs e)
        {
            ShowSettings(0);
        }

        public void ShowSettings(int tabindex)
        {
            var settings = new Settings {Owner = this, InitialTab = tabindex};
            if (settings.ShowDialog(this) == DialogResult.OK)
            {
                _pnlCameras.BackColor = Conf.MainColor.ToColor();
                notifyIcon1.Text = Conf.TrayIconText;
            }

            if (settings.ReloadResources)
            {
                RenderResources();
                LoadCommands();
                Refresh();
            }
            AddressIPv4 = ""; //forces reload
            AddressIPv6 = "";
            settings.Dispose();
            SaveConfig();
        }

        private void MenuItem11Click(object sender, EventArgs e)
        {
            using (var fb = new Feedback())
            {
                fb.ShowDialog(this);
            }
        }

        private void MainFormResize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                if (Conf.BalloonTips)
                {
                    if (Conf.BalloonTips)
                    {
                        notifyIcon1.BalloonTipText = LocRm.GetString("RunningInTaskBar");
                        notifyIcon1.ShowBalloonTip(1500);
                    }
                }
            }
            else
            {
                if (Conf.AutoLayout)
                    LayoutObjects(0, 0);
                if (!IsOnScreen(this))
                {
                    this.Location = new Point(0,0);
                }
            }
        }

        private void NotifyIcon1DoubleClick(object sender, EventArgs e)
        {
            ShowIfUnlocked();
        }

        public void ShowIfUnlocked()
        {
            if (Visible == false || WindowState == FormWindowState.Minimized)
            {
                if (Conf.Enable_Password_Protect)
                {
                    using (var cp = new CheckPassword())
                    {
                        cp.ShowDialog(this);
                        if (cp.DialogResult == DialogResult.OK)
                        {
                            ShowForm(-1);
                        }
                    }
                }
                else
                {
                    ShowForm(-1);
                }
            }
            else
            {
                ShowForm(-1);
            }
        }

        private void MainFormFormClosing1(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.WindowsShutDown)
            {
                if (Conf.MinimiseOnClose && !reallyclose)
                {
                    e.Cancel = true;
                    this.WindowState = FormWindowState.Minimized;
                    return;
                }
            }
            Exit();
        }

        private void Exit()
        {
            if (_houseKeepingTimer != null)
                _houseKeepingTimer.Stop();
            if (_updateTimer != null)
                _updateTimer.Stop();
            _shuttingDown = true;

            if (Conf.ShowMediaPanel)
                Conf.MediaPanelSize = splitContainer1.SplitterDistance+"x"+splitContainer2.SplitterDistance;

            if (Conf.BalloonTips)
            {
                if (Conf.BalloonTips)
                {
                    notifyIcon1.BalloonTipText = LocRm.GetString("ShuttingDown");
                    notifyIcon1.ShowBalloonTip(1500);
                }
            }
            _closing = true;

            try
            {
                SaveObjects("");
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
            } 
            
            try
            {
                SaveConfig();
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
            }

            try
            {
                if (_talkSource != null)
                {
                    _talkSource.Stop();
                    _talkSource = null;
                }
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
            }
            try
            {
                if (_talkTarget != null)
                {
                    _talkTarget.Stop();
                    _talkTarget = null;
                }
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
            }
            try
            {
                RemoveObjects();
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
            }
            try
            {
                MWS.StopServer();
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
            }
            try
            {
                Application.DoEvents();
                if (Conf.ServicesEnabled)
                {
                    if (WsWrapper.WebsiteLive)
                    {
                        try
                        {
                            WsWrapper.Disconnect();
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
            }

            try
            {
                File.WriteAllText(Program.AppDataPath + "exit.txt", "OK");
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
            }

            WriteLog();
        }

        

        private void ControlNotification(object sender, NotificationType e)
        {
            if (Conf.BalloonTips)
            {
                if (String.IsNullOrEmpty(e.OverrideMessage))
                    notifyIcon1.BalloonTipText = LocRm.GetString(e.Type) + ":" + NL + e.Text;
                else
                {
                    notifyIcon1.BalloonTipText = e.OverrideMessage+ ":" + NL + e.Text;
                }
                notifyIcon1.ShowBalloonTip(1500);
            }
        }

        

        private void NotifyIcon1BalloonTipClicked(object sender, EventArgs e)
        {
            ShowIfUnlocked();
        }

        

        public static string RandomString(int length)
        {
            var b = "";

            for (int i = 0; i < length; i++)
            {
                char ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * Random.NextDouble() + 65)));
                b+=ch;
            }
            return b;
        }

        private void SetNewStartPosition()
        {
            if (Conf.AutoLayout)
                LayoutObjects(0, 0);
        }

        private void VolumeControlRemoteCommand(object sender, VolumeLevel.ThreadSafeCommand e)
        {
            InvokeMethod i = DoInvoke;
            Invoke(i, new object[] {e.Command});
        }

        private void ConnectServices()
        {
            if (Conf.ServicesEnabled)
            {
                if (Conf.UseUPNP)
                {
                    NATControl.SetPorts(Conf.ServerPort, Conf.LANPort);
                }

                string[] result =
                    WsWrapper.TestConnection(Conf.WSUsername, Conf.WSPassword, Conf.Loopback);

                if (result.Length>0 && result[0] == "OK")
                {
                    WsWrapper.Connect();
                    NeedsSync = true;
                    EmailAddress = result[2];
                    MobileNumber = result[4];
                    Conf.Reseller = result[5];

                    Conf.ServicesEnabled = true;
                    Conf.Subscribed = (Convert.ToBoolean(result[1]));

                    UISync.Execute(() => Text = "iSpy v" + Application.ProductVersion);
                    if (Conf.WSUsername != "")
                    {
                        UISync.Execute(() => Text += " (" + Conf.WSUsername + ")");
                    }
                    if (Conf.Reseller != "")
                    {
                        UISync.Execute(() => Text += " Powered by " + Conf.Reseller.Split('|')[0]);
                    }

                    if (result[3] == "")
                    {
                        LoopBack = Conf.Loopback;
                        WsWrapper.Connect(Conf.Loopback);
                    }
                    else
                    {
                        LoopBack = false;
                    }
                }
            }
            if (Conf.Enable_Update_Check && !SilentStartup)
            {
                UISync.Execute(() => CheckForUpdates(true));
            }
            SilentStartup = false;
        }

        

        private void SetInactiveToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (ContextTarget.GetType() == typeof (CameraWindow))
            {
                var cameraControl = ((CameraWindow) ContextTarget);
                cameraControl.Disable();
            }
            else
            {
                if (ContextTarget.GetType() == typeof (VolumeLevel))
                {
                    var vf = ((VolumeLevel) ContextTarget);
                    vf.Disable();
                }
            }
        }

        private void EditToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (ContextTarget.GetType() == typeof (CameraWindow))
            {
                EditCamera(((CameraWindow) ContextTarget).Camobject);
            }
            if (ContextTarget.GetType() == typeof (VolumeLevel))
            {
                EditMicrophone(((VolumeLevel) ContextTarget).Micobject);
            }
            if (ContextTarget.GetType() == typeof (FloorPlanControl))
            {
                EditFloorplan(((FloorPlanControl) ContextTarget).Fpobject);
            }
        }

        private void DeleteToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (ContextTarget.GetType() == typeof (CameraWindow))
            {
                RemoveCamera((CameraWindow) ContextTarget, true);
            }
            if (ContextTarget.GetType() == typeof (VolumeLevel))
            {
                RemoveMicrophone((VolumeLevel) ContextTarget, true);
            }
            if (ContextTarget.GetType() == typeof (FloorPlanControl))
            {
                RemoveFloorplan((FloorPlanControl) ContextTarget, true);
            }
        }


        private void ToolStripButton4Click(object sender, EventArgs e)
        {
            ShowSettings(0);
        }

        public static void GoSubscribe()
        {
            OpenUrl(Website + "/subscribe.aspx");
        }

        public static void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch (Exception)
            {
                try
                {
                    var p = new Process {StartInfo = {FileName = DefaultBrowser, Arguments = url}};
                    p.Start();
                }
                catch (Exception ex2)
                {
                    LogExceptionToFile(ex2);
                }
            }
        }

        private static string DefaultBrowser
        {
            get
            {
                if (!String.IsNullOrEmpty(_browser))
                    return _browser;

                _browser = string.Empty;
                RegistryKey key = null;
                try
                {
                    key = Registry.ClassesRoot.OpenSubKey(@"HTTP\shell\open\command", false);

                    //trim off quotes
                    if (key != null) _browser = key.GetValue(null).ToString().ToLower().Replace("\"", "");
                    if (!_browser.EndsWith(".exe"))
                    {
                        _browser = _browser.Substring(0, _browser.LastIndexOf(".exe") + 4);
                    }
                }
                finally
                {
                    if (key != null) key.Close();
                }
                return _browser;
            }
        }

        private void ActivateToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (ContextTarget.GetType() == typeof (CameraWindow))
            {
                var cameraControl = ((CameraWindow) ContextTarget);
                cameraControl.Enable();
            }
            else
            {
                if (ContextTarget.GetType() == typeof (VolumeLevel))
                {
                    var vf = ((VolumeLevel) ContextTarget);
                    vf.Enable();
                }
            }
        }

        private void WebsiteToolstripMenuItemClick(object sender, EventArgs e)
        {
            StartBrowser(Website + "/");
        }

        private void HelpToolstripMenuItemClick(object sender, EventArgs e)
        {
            StartBrowser(Website + "/userguide.aspx");
        }

        private void ShowToolstripMenuItemClick(object sender, EventArgs e)
        {
            ShowForm(-1);
        }

        public void ShowForm(double opacity)
        {
            Activate();
            Visible = true;
            if (WindowState == FormWindowState.Minimized)
            {
                Show();
                WindowState = FormWindowState.Normal;
            }
            if (opacity > -1)
                Opacity = opacity;
            TopMost = true;
            TopMost = false; //need to force a switch to move above other forms
            TopMost = Conf.AlwaysOnTop;
            BringToFront();
            Focus();
        }

        private void UnlockToolstripMenuItemClick(object sender, EventArgs e)
        {
            ShowUnlock();
        }

        private void ShowUnlock()
        {
            var cp = new CheckPassword();
            cp.ShowDialog(this);
            if (cp.DialogResult == DialogResult.OK)
            {
                Activate();
                Visible = true;
                if (WindowState == FormWindowState.Minimized)
                {
                    Show();
                    WindowState = FormWindowState.Normal;
                }
                Focus();
            }
            cp.Dispose();
        }

        private void NotifyIcon1Click(object sender, EventArgs e)
        {
        }

        private void AddCameraToolStripMenuItemClick(object sender, EventArgs e)
        {
            AddCamera(3);
        }

        private void AddMicrophoneToolStripMenuItemClick(object sender, EventArgs e)
        {
            AddMicrophone(0);
        }

        private void CtxtMainFormOpening(object sender, CancelEventArgs e)
        {
            if (ctxtMnu.Visible || ctxtPlayer.Visible)
                e.Cancel = true;
        }


        public static void StartBrowser(string url)
        {
            if (url != "")
                OpenUrl(url);
        }

        private void ExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            reallyclose = true;
            Close();
        }

        private void MenuItem3Click(object sender, EventArgs e)
        {
            Connect(false);
        }

        private void MenuItem18Click(object sender, EventArgs e)
        {
            if (
                MessageBox.Show(LocRm.GetString("AreYouSure"), LocRm.GetString("Confirm"), MessageBoxButtons.OKCancel,
                                MessageBoxIcon.Warning) == DialogResult.Cancel)
                return;
            string loc = Conf.MediaDirectory + "audio\\";

            if (Directory.Exists(loc))
            {

                string[] files = Directory.GetFiles(loc, "*.*", SearchOption.AllDirectories);
                foreach (string t in files)
                {
                    try
                    {

                        FileOperations.Delete(t);
                    }
                    catch
                    {
                    }
                }
            }
            loc = Conf.MediaDirectory + "video\\";
            if (Directory.Exists(loc))
            {
                string[] files = Directory.GetFiles(loc, "*.*", SearchOption.AllDirectories);
                foreach (string t in files)
                {
                    try
                    {
                        FileOperations.Delete(t);
                    }
                    catch
                    {
                    }
                }
            }
            foreach (objectsCamera oc in Cameras)
            {
                CameraWindow occ = GetCameraWindow(oc.id);
                if (occ != null)
                {
                    occ.FileList.Clear();
                }
            }
            foreach (objectsMicrophone om in Microphones)
            {
                VolumeLevel omc = GetMicrophone(om.id);
                if (omc != null)
                {
                    omc.FileList.Clear();
                }
            }
            LoadPreviews();
            MessageBox.Show(LocRm.GetString("FilesDeleted"), LocRm.GetString("Note"));
        }

        private void MenuItem20Click(object sender, EventArgs e)
        {
            Process.Start(Program.AppDataPath + "log_" + NextLog + ".htm");
        }

        private void ResetSizeToolStripMenuItemClick(object sender, EventArgs e)
        {
            Minimize(ContextTarget, true);
        }

        private void Minimize(object obj, bool tocontents)
        {
            if (obj == null || Conf.LockLayout)
                return;
            if (obj.GetType() == typeof (CameraWindow))
            {
                var cw = (CameraWindow) obj;
                var r = cw.RestoreRect;
                if (r != Rectangle.Empty && !tocontents)
                {
                    cw.Location = r.Location;
                    cw.Height = r.Height;
                    cw.Width = r.Width;
                }
                else
                {
                    if (cw.Camera != null && !cw.Camera.LastFrameNull)
                    {
                        cw.Width = cw.Camera.LastFrameUnmanaged.Width + 2;
                        cw.Height = cw.Camera.LastFrameUnmanaged.Height + 26;
                    }
                    else
                    {
                        cw.Width = 322;
                        cw.Height = 266;
                    }
                }
                cw.Invalidate();
            }

            if (obj.GetType() == typeof(VolumeLevel))
            {
                var cw = (VolumeLevel)obj;
                var r = cw.RestoreRect;
                if (r != Rectangle.Empty && !tocontents)
                {
                    cw.Location = r.Location;
                    cw.Height = r.Height;
                    cw.Width = r.Width;
                }
                else
                {
                    cw.Width = 160;
                    cw.Height = 40;
                }
                cw.Invalidate();
            }

            if (obj.GetType() == typeof (FloorPlanControl))
            {
                var fp = (FloorPlanControl) obj;
                var r = fp.RestoreRect;
                if (r != Rectangle.Empty && !tocontents)
                {
                    fp.Location = r.Location;
                    fp.Height = r.Height;
                    fp.Width = r.Width;
                    fp.Invalidate();
                }
                else
                {
                    if (fp.ImgPlan != null)
                    {
                        fp.Width = fp.ImgPlan.Width + 2;
                        fp.Height = fp.ImgPlan.Height + 26;
                    }
                    else
                    {
                        fp.Width = 322;
                        fp.Height = 266;
                    }
                }
            }
        }

        private void SettingsToolStripMenuItemClick(object sender, EventArgs e)
        {
            ShowSettings(0);
        }


        private void MenuItem19Click(object sender, EventArgs e)
        {
            if (Cameras.Count == 0 && Microphones.Count == 0)
            {
                MessageBox.Show(LocRm.GetString("NothingToExport"), LocRm.GetString("Error"));
                return;
            }

            var saveFileDialog = new SaveFileDialog
                                     {
                                         InitialDirectory = _lastPath,
                                         Filter = "iSpy Files (*.ispy)|*.ispy|XML Files (*.xml)|*.xml"
                                     };

            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string fileName = saveFileDialog.FileName;

                if (fileName.Trim() != "")
                {
                    SaveObjects(fileName);
                    try
                    {
                        var fi = new FileInfo(fileName);
                        _lastPath = fi.DirectoryName;
                    }
                    catch
                    {
                    }
                }
            }
            saveFileDialog.Dispose();
        }


        private void MenuItem21Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.InitialDirectory = _lastPath;
                ofd.Filter = "iSpy Files (*.ispy)|*.ispy|XML Files (*.xml)|*.xml";
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    string fileName = ofd.FileName;
                    try
                    {
                        var fi = new FileInfo(fileName);
                        _lastPath = fi.DirectoryName;
                    }
                    catch
                    {
                    }


                    if (fileName.Trim() != "")
                    {
                        LoadObjectList(fileName.Trim());
                    }
                }
            }
        }

        private void ToolStripMenuItem1Click(object sender, EventArgs e)
        {
            string id;
            if (ContextTarget.GetType() == typeof (CameraWindow))
            {
                //id = ((CameraWindow) ContextTarget).Camobject.id.ToString();
                string url = Webserver + "/watch_new.aspx";
                if (WsWrapper.WebsiteLive && Conf.ServicesEnabled)
                {
                    OpenUrl(url);
                }
                else
                    Connect(url, false);
            }
            else
            {
                if (ContextTarget.GetType() == typeof (VolumeLevel))
                {
                    //id = ((VolumeLevel) ContextTarget).Micobject.id.ToString();
                    string url = Webserver + "/watch_new.aspx";
                    if (WsWrapper.WebsiteLive && Conf.ServicesEnabled)
                    {
                        OpenUrl(url);
                    }
                    else
                        Connect(url, false);
                }
                else
                {
                    if (ContextTarget.GetType() == typeof (FloorPlanControl))
                    {
                        string url = Webserver + "/watch_new";
                        if (WsWrapper.WebsiteLive && Conf.ServicesEnabled)
                        {
                            OpenUrl(url);
                        }
                        else
                            Connect(url, false);
                    }
                }
            }
        }

        public void Connect(bool silent)
        {
            Connect(Webserver + "/watch_new.aspx", silent);
        }

        public void Connect(string successUrl, bool silent)
        {
            if (!MWS.Running)
            {
                string message = StopAndStartServer();
                if (message != "")
                {
                    if (!silent)
                        MessageBox.Show(this, message);
                    return;
                }
            }
            if (WsWrapper.WebsiteLive)
            {
                if (Conf.WSUsername != null && Conf.WSUsername.Trim() != "")
                {
                    if (Conf.UseUPNP)
                    {
                        NATControl.SetPorts(Conf.ServerPort, Conf.LANPort);
                    }
                    WsWrapper.Connect();
                    WsWrapper.ForceSync();
                    if (WsWrapper.WebsiteLive)
                    {
                        if (successUrl != "")
                            StartBrowser(successUrl);
                        return;
                    }
                    if (!silent && !_shuttingDown)
                        LogMessageToFile(LocRm.GetString("WebsiteDown"));
                    return;
                }
                var ws = new Webservices();
                ws.ShowDialog(this);
                if (ws.EmailAddress != "")
                    EmailAddress = ws.EmailAddress;
                if (ws.DialogResult == DialogResult.Yes || ws.DialogResult == DialogResult.No)
                {
                    ws.Dispose();
                    Connect(successUrl, silent);
                    return;
                }
                ws.Dispose();
            }
            else
            {
                LogMessageToFile(LocRm.GetString("WebsiteDown"));
            }
        }

        private void MenuItem7Click(object sender, EventArgs e)
        {
            string foldername = Conf.MediaDirectory + "video\\";
            if (!foldername.EndsWith(@"\"))
                foldername += @"\";
            Process.Start(foldername);
        }

        private void MenuItem23Click(object sender, EventArgs e)
        {
            string foldername = Conf.MediaDirectory + "audio\\";
            if (!foldername.EndsWith(@"\"))
                foldername += @"\";
            Process.Start(foldername);
        }

        private void MenuItem25Click(object sender, EventArgs e)
        {
            ViewMobile();
        }


        private void MainFormHelpButtonClicked(object sender, CancelEventArgs e)
        {
            OpenUrl(Website + "/userguide.aspx");
        }

        private void menuItem21_Click(object sender, EventArgs e)
        {
            LayoutOptimised();
        }

        private void ShowISpy10PercentOpacityToolStripMenuItemClick(object sender, EventArgs e)
        {
            ShowForm(.1);
        }

        private void ShowISpy30OpacityToolStripMenuItemClick(object sender, EventArgs e)
        {
            ShowForm(.3);
        }

        private void ShowISpy100PercentOpacityToolStripMenuItemClick(object sender, EventArgs e)
        {
            ShowForm(1);
        }

        private void CtxtTaskbarOpening(object sender, CancelEventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                if (Conf.Enable_Password_Protect)
                {
                    _unlockToolstripMenuItem.Visible = true;
                    _showToolstripMenuItem.Visible =
                        _showISpy10PercentOpacityToolStripMenuItem.Visible =
                        _showISpy30OpacityToolStripMenuItem.Visible =
                        _showISpy100PercentOpacityToolStripMenuItem.Visible = false;
                    _exitToolStripMenuItem.Visible = false;
                    _websiteToolstripMenuItem.Visible = false;
                    _helpToolstripMenuItem.Visible = false;
                    _switchAllOffToolStripMenuItem.Visible = false;
                    _switchAllOnToolStripMenuItem.Visible = false;
                }
                else
                {
                    _unlockToolstripMenuItem.Visible = false;
                    _showToolstripMenuItem.Visible =
                        _showISpy10PercentOpacityToolStripMenuItem.Visible =
                        _showISpy30OpacityToolStripMenuItem.Visible =
                        _showISpy100PercentOpacityToolStripMenuItem.Visible = true;
                    _exitToolStripMenuItem.Visible = true;
                    _websiteToolstripMenuItem.Visible = true;
                    _helpToolstripMenuItem.Visible = true;
                    _switchAllOffToolStripMenuItem.Visible = true;
                    _switchAllOnToolStripMenuItem.Visible = true;
                }
            }
            else
            {
                _showToolstripMenuItem.Visible = false;
                _showISpy10PercentOpacityToolStripMenuItem.Visible =
                    _showISpy30OpacityToolStripMenuItem.Visible =
                    _showISpy100PercentOpacityToolStripMenuItem.Visible = true;
                _unlockToolstripMenuItem.Visible = false;
                _exitToolStripMenuItem.Visible = true;
                _websiteToolstripMenuItem.Visible = true;
                _helpToolstripMenuItem.Visible = true;
                _switchAllOffToolStripMenuItem.Visible = true;
                _switchAllOnToolStripMenuItem.Visible = true;
            }
        }

       

        private void MenuItem26Click(object sender, EventArgs e)
        {
            OpenUrl(Website + "/donate.aspx");
        }

        private void RecordNowToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (ContextTarget.GetType() == typeof (CameraWindow))
            {
                var cameraControl = ((CameraWindow) ContextTarget);
                cameraControl.RecordSwitch(!cameraControl.Recording);
            }
            else
            {
                if (ContextTarget.GetType() == typeof (VolumeLevel))
                {
                    var volumeControl = ((VolumeLevel) ContextTarget);
                    volumeControl.RecordSwitch(!volumeControl.Recording);
                }
            }
        }

        private void ShowFilesToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (ContextTarget.GetType() == typeof (CameraWindow))
            {
                var cw = ((CameraWindow) ContextTarget);
                string foldername = Conf.MediaDirectory + "video\\" + cw.Camobject.directory +
                                    "\\";
                if (!foldername.EndsWith(@"\"))
                    foldername += @"\";
                Process.Start(foldername);
                cw.Camobject.newrecordingcount = 0;
            }
            else
            {
                if (ContextTarget.GetType() == typeof (VolumeLevel))
                {
                    var vl = ((VolumeLevel) ContextTarget);
                    string foldername = Conf.MediaDirectory + "audio\\" + vl.Micobject.directory +
                                        "\\";
                    if (!foldername.EndsWith(@"\"))
                        foldername += @"\";
                    Process.Start(foldername);
                    vl.Micobject.newrecordingcount = 0;
                }
                else
                {
                    string foldername = Conf.MediaDirectory;
                    Process.Start(foldername);
                }
            }
        }

        private void ViewMediaOnAMobileDeviceToolStripMenuItemClick(object sender, EventArgs e)
        {
            ViewMobile();
        }

        private void ViewMobile()
        {
            if (WsWrapper.WebsiteLive && Conf.ServicesEnabled)
            {
                OpenUrl(Webserver + "/mobile/");
            }
            else
                WebConnect();
        }

        private void AddFloorPlanToolStripMenuItemClick(object sender, EventArgs e)
        {
            AddFloorPlan();
        }

        private void ListenToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (ContextTarget.GetType() == typeof (VolumeLevel))
            {
                var vf = ((VolumeLevel) ContextTarget);
                vf.Listening = !vf.Listening;
            }
        }

        private void MenuItem31Click(object sender, EventArgs e)
        {
            if (
                MessageBox.Show(LocRm.GetString("AreYouSure"), LocRm.GetString("Confirm"), MessageBoxButtons.OKCancel,
                                MessageBoxIcon.Warning) == DialogResult.Cancel)
                return;
            RemoveObjects();
        }

        private void MenuItem34Click(object sender, EventArgs e)
        {
        }

        

        private void MenuItem33Click(object sender, EventArgs e)
        {
        }

        private void ToolStripButton8Click1(object sender, EventArgs e)
        {
            ShowRemoteCommands();
        }

        private void MenuItem35Click(object sender, EventArgs e)
        {
            ShowRemoteCommands();
        }

        private void ToolStrip1ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
        }

        private void RemoteCommandsToolStripMenuItemClick(object sender, EventArgs e)
        {
            ShowRemoteCommands();
        }       

        private void MenuItem37Click(object sender, EventArgs e)
        {
            MessageBox.Show(LocRm.GetString("EditInstruct"), LocRm.GetString("Note"));
        }

        private void PositionToolStripMenuItemClick(object sender, EventArgs e)
        {
            var p = (PictureBox) ContextTarget;
            int w = p.Width;
            int h = p.Height;
            int x = p.Location.X;
            int y = p.Location.Y;

            var le = new LayoutEditor {X = x, Y = y, W = w, H = h};


            if (le.ShowDialog(this) == DialogResult.OK)
            {
                PositionPanel(p, new Point(le.X, le.Y), le.W, le.H);
            }
            le.Dispose();
        }

        private static void PositionPanel(PictureBox p, Point xy, int w, int h)
        {
            p.Width = w;
            p.Height = h;
            p.Location = new Point(xy.X, xy.Y);
        }

        private void MenuItem38Click(object sender, EventArgs e)
        {
            StartBrowser(Website + "/producthistory.aspx?productid=11");
        }

        private void MenuItem39Click(object sender, EventArgs e)
        {
        }

        private void TakePhotoToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (ContextTarget.GetType() == typeof (CameraWindow))
            {
                var cameraControl = ((CameraWindow) ContextTarget);
                string fn = cameraControl.SaveFrame();
                if (fn != "")
                    OpenUrl(fn);
                //OpenUrl("http://" + IPAddress + ":" + Conf.LANPort + "/livefeed?oid=" + cameraControl.Camobject.id + "&r=" + Random.NextDouble() + "&full=1&auth=" + Identifier);
            }
        }

        private void ToolStripDropDownButton1Click(object sender, EventArgs e)
        {
        }

        private void ThruWebsiteToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (WsWrapper.WebsiteLive && Conf.ServicesEnabled)
            {
                OpenUrl(Webserver + "/watch_new.aspx");
            }
            else
                WebConnect();
        }

        private void OnMobileDevicesToolStripMenuItemClick(object sender, EventArgs e)
        {
            ViewMobile();
        }

        private void LocalCameraToolStripMenuItemClick(object sender, EventArgs e)
        {
            AddCamera(3);
        }

        private void IpCameraToolStripMenuItemClick(object sender, EventArgs e)
        {
            AddCamera(1);
        }

        private void MicrophoneToolStripMenuItemClick(object sender, EventArgs e)
        {
            AddMicrophone(0);
        }

        private void FloorPlanToolStripMenuItemClick(object sender, EventArgs e)
        {
            AddFloorPlan();
        }

        private void MenuItem12Click(object sender, EventArgs e)
        {
            //+26 height for control bar
            LayoutObjects(164, 146);
        }

        private void MenuItem14Click(object sender, EventArgs e)
        {
            LayoutObjects(324, 266);
        }

        private void MenuItem29Click1(object sender, EventArgs e)
        {
            LayoutObjects(0, 0);
        }

        private void ToolStripButton1Click1(object sender, EventArgs e)
        {
            WebConnect();
        }

        private void WebConnect()
        {
            var ws = new Webservices();
            ws.ShowDialog(this);
            if (ws.EmailAddress != "")
            {
                EmailAddress = ws.EmailAddress;
                MobileNumber = ws.MobileNumber;
            }
            if (ws.DialogResult == DialogResult.Yes)
            {
                Connect(false);
            }
            ws.Dispose();
            Text = "iSpy v" + Application.ProductVersion;
            if (Conf.WSUsername != "")
            {
                Text += " (" + Conf.WSUsername + ")";
            }
        }

        private void MenuItem17Click(object sender, EventArgs e)
        {
        }

        private void ResetRecordingCounterToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (ContextTarget.GetType() == typeof (CameraWindow))
            {
                var cw = ((CameraWindow) ContextTarget);
                cw.Camobject.newrecordingcount = 0;
                if (cw.VolumeControl != null)
                {
                    cw.VolumeControl.Micobject.newrecordingcount = 0;
                    cw.VolumeControl.Invalidate();
                }
                cw.Invalidate();
            }
            if (ContextTarget.GetType() == typeof (VolumeLevel))
            {
                var vw = ((VolumeLevel) ContextTarget);
                vw.Micobject.newrecordingcount = 0;
                if (vw.Paired)
                {
                    objectsCamera oc = Cameras.SingleOrDefault(p => p.settings.micpair == vw.Micobject.id);
                    if (oc != null)
                    {
                        CameraWindow cw = GetCameraWindow(oc.id);
                        cw.Camobject.newrecordingcount = 0;
                        cw.Invalidate();
                    }
                }
                vw.Invalidate();
            }
        }

        private void MenuItem15Click(object sender, EventArgs e)
        {
            foreach (Control c in _pnlCameras.Controls)
            {
                if (c is CameraWindow)
                {
                    var cameraControl = (CameraWindow) c;
                    cameraControl.Camobject.newrecordingcount = 0;
                    cameraControl.Invalidate();
                }
                if (c is VolumeLevel)
                {
                    var volumeControl = (VolumeLevel) c;
                    volumeControl.Micobject.newrecordingcount = 0;
                    volumeControl.Invalidate();
                }
            }
        }

        private void SwitchAllOnToolStripMenuItemClick(object sender, EventArgs e)
        {
            SwitchObjects(false, true);
        }

        private void SwitchAllOffToolStripMenuItemClick(object sender, EventArgs e)
        {
            SwitchObjects(false, false);
        }

        private void MenuItem22Click1(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
                          {
                              InitialDirectory = Program.AppDataPath,
                              Filter = "iSpy Log Files (*.htm)|*.htm"
                          };

            if (ofd.ShowDialog(this) != DialogResult.OK) return;
            string fileName = ofd.FileName;

            if (fileName.Trim() != "")
            {
                Process.Start(ofd.FileName);
            }
        }

        private void USbCamerasAndMicrophonesOnOtherToolStripMenuItemClick(object sender, EventArgs e)
        {
            OpenUrl(Website + "/download_ispyserver.aspx");
        }

        private void MenuItem24Click(object sender, EventArgs e)
        {
            SwitchObjects(false, true);
        }

        private void MenuItem40Click(object sender, EventArgs e)
        {
            SwitchObjects(false, false);
        }

        private void MenuItem41Click(object sender, EventArgs e)
        {
            SwitchObjects(true, false);
        }

        private void MenuItem28Click1(object sender, EventArgs e)
        {
            SwitchObjects(true, true);
        }

        private void MenuItem24Click1(object sender, EventArgs e)
        {
            ApplySchedule();
        }

        public void ApplySchedule()
        {
            foreach (objectsCamera cam in _cameras)
            {
                if (cam.schedule.active)
                {
                    CameraWindow cw = GetCamera(cam.id);
                    cw.ApplySchedule();
                }
            }


            foreach (objectsMicrophone mic in _microphones)
            {
                if (mic.schedule.active)
                {
                    VolumeLevel vl = GetMicrophone(mic.id);
                    vl.ApplySchedule();
                }
            }
        }

        private void ApplyScheduleToolStripMenuItemClick1(object sender, EventArgs e)
        {
            ApplySchedule();
        }

        private void ApplyScheduleToolStripMenuItem1Click(object sender, EventArgs e)
        {
            if (ContextTarget.GetType() == typeof (CameraWindow))
            {
                var cameraControl = ((CameraWindow) ContextTarget);
                cameraControl.ApplySchedule();
            }
            else
            {
                if (ContextTarget.GetType() == typeof (VolumeLevel))
                {
                    var vf = ((VolumeLevel) ContextTarget);
                    vf.ApplySchedule();
                }
            }
        }

        private void MenuItem24Click2(object sender, EventArgs e)
        {
            ShowGettingStarted();
        }

        private void ShowGettingStarted()
        {
            var gs = new GettingStarted();
            gs.Closed += _gs_Closed;
            gs.Show(this);
            gs.Activate();
        }

        private void _gs_Closed(object sender, EventArgs e)
        {
            if (((GettingStarted) sender).LangChanged)
            {
                RenderResources();
                LoadCommands();
                Refresh();
            }
        }

        private void MenuItem28Click2(object sender, EventArgs e)
        {
            LayoutObjects(644, 506);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            reallyclose = true;
            Close();
        }


        private delegate void CloseDelegate();

        public void ExternalClose()
        {
            if (InvokeRequired)
            {
                Invoke(new CloseDelegate(ExternalClose));
                return;
            }
            reallyclose = true;
            Close();
        }

        private void fullScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Maximise(ContextTarget);
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            OpenUrl(Website + "/userguide.aspx#4");
        }

        private void inExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string foldername = Conf.MediaDirectory;
            if (!foldername.EndsWith(@"\"))
                foldername += @"\";
            Process.Start(foldername);
        }

        private void menuItem1_Click_1(object sender, EventArgs e)
        {
            LayoutObjects(-1, -1);
        }

        private class UISync
        {
            private static ISynchronizeInvoke _sync;

            public static void Init(ISynchronizeInvoke sync)
            {
                _sync = sync;
            }

            public static void Execute(Action action)
            {
                try
                {
                    _sync.BeginInvoke(action, null);
                }
                catch
                {
                }
            }
        }

        private bool _selectedall;

        private void llblSelectAll_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _selectedall = !_selectedall;
            lock (flowPreview.Controls)
            {
                foreach (PreviewBox pb in flowPreview.Controls)
                    pb.Selected = _selectedall;
                flowPreview.Invalidate(true);
            }
        }


       

        private void llblDelete_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            DeleteSelectedMedia();
        }


        
        private void opacityToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ShowForm(.1);
        }

        private void opacityToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            ShowForm(.3);
        }

        private void opacityToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            ShowForm(1);
        }

        private void autoLayoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            autoLayoutToolStripMenuItem.Checked = !autoLayoutToolStripMenuItem.Checked;
            Conf.AutoLayout = autoLayoutToolStripMenuItem.Checked;
            if (Conf.AutoLayout)
                LayoutObjects(0, 0);
        }

        private void saveLayoutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SaveLayout();
            

        }

        private void resetLayoutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ResetLayout();
        }

        private void fullScreenToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MaxMin();
        }

        private void statusBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statusBarToolStripMenuItem.Checked = menuItem4.Checked = !statusBarToolStripMenuItem.Checked;
            statusStrip1.Visible = statusBarToolStripMenuItem.Checked;

            Conf.ShowStatus = statusBarToolStripMenuItem.Checked;
        }

        private void fileMenuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileMenuToolStripMenuItem.Checked = menuItem5.Checked = !fileMenuToolStripMenuItem.Checked;
            Menu = !fileMenuToolStripMenuItem.Checked ? null : mainMenu;

            Conf.ShowFileMenu = fileMenuToolStripMenuItem.Checked;
        }

        private void toolStripToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripToolStripMenuItem.Checked = menuItem6.Checked = !toolStripToolStripMenuItem.Checked;
            toolStripMenu.Visible = toolStripToolStripMenuItem.Checked;
            Conf.ShowToolbar = toolStripToolStripMenuItem.Checked;
        }

        private void alwaysOnTopToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            alwaysOnTopToolStripMenuItem1.Checked = menuItem8.Checked = !alwaysOnTopToolStripMenuItem1.Checked;
            Conf.AlwaysOnTop = alwaysOnTopToolStripMenuItem1.Checked;
            TopMost = Conf.AlwaysOnTop;
        }

        private void mediaPaneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mediaPaneToolStripMenuItem.Checked = menuItem7.Checked = !mediaPaneToolStripMenuItem.Checked;
            Conf.ShowMediaPanel = mediaPaneToolStripMenuItem.Checked;
            ShowHideMediaPane();
        }

        private void ShowHideMediaPane()
        {
            if (Conf.ShowMediaPanel)
            {
                splitContainer1.Panel2Collapsed = false;
                splitContainer1.Panel2.Show();
            }
            else
            {
                splitContainer1.Panel2Collapsed = true;
                splitContainer1.Panel2.Hide();
            }
        }

        private void iPCameraWithWizardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddCamera(1, true);
        }

        private void menuItem13_Click(object sender, EventArgs e)
        {
            OpenUrl(PurchaseLink);
        }

        private void tsbPlugins_Click(object sender, EventArgs e)
        {
            OpenUrl("http://www.ispyconnect.com/plugins.aspx");
        }

        private void flowPreview_MouseEnter(object sender, EventArgs e)
        {
            flowPreview.Focus();
        }

        private void flowPreview_Click(object sender, EventArgs e)
        {
        }

        private void flCommands_MouseEnter(object sender, EventArgs e)
        {
            flCommands.Focus();
        }

        public void PTZToolUpdate(CameraWindow cw)
        {
            if (_ptzTool!=null)
            {
                _ptzTool.CameraControl = cw;
            }
        }

        

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.PageUp)
            {
                ProcessKey("previous_control");
            }

            if (e.KeyCode == Keys.PageDown)
            {
                ProcessKey("next_control");
            }

            if (e.KeyCode == Keys.P && (e.Control))
            {
                ProcessKey("play");
            }

            if (e.KeyCode == Keys.S && e.Control)
            {
                ProcessKey("stop");
            }

            if (e.KeyCode == Keys.R && e.Control)
            {
                ProcessKey("record");
            }

            if (e.KeyCode == Keys.Z && e.Control)
            {
                ProcessKey("zoom");
            }
            if (e.KeyCode==Keys.F4 && e.Alt)
            {
                ProcessKey("power");
            }
            if (e.KeyCode.ToString() == "D0")
            {
                MaximiseControl(10);
            }
            if (e.KeyCode.ToString() == "D1")
            {
                MaximiseControl(0);
            }
            if (e.KeyCode.ToString() == "D2")
            {
                MaximiseControl(1);
            }
            if (e.KeyCode.ToString() == "D3")
            {
                MaximiseControl(2);
            }
            if (e.KeyCode.ToString() == "D4")
            {
                MaximiseControl(3);
            }
            if (e.KeyCode.ToString() == "D5")
            {
                MaximiseControl(4);
            }
            if (e.KeyCode.ToString() == "D6")
            {
                MaximiseControl(5);
            }
            if (e.KeyCode.ToString() == "D7")
            {
                MaximiseControl(6);
            }
            if (e.KeyCode.ToString() == "D8")
            {
                MaximiseControl(7);
            }
            if (e.KeyCode.ToString() == "D9")
            {
                MaximiseControl(8);
            }
        }

        private void MaximiseControl(int index)
        {
            int i = 0;
            foreach(Control c in _pnlCameras.Controls)
            {
                if (c.Tag is int)
                {
                    if ((int)c.Tag == index)
                    {
                        Maximise(c, true);
                        c.Focus();
                        break;
                    }
                    i++;
                }
            }
        }

        private void menuItem14_Click(object sender, EventArgs e)
        {
            if (_vc != null)
            {
                _vc.Close();
                _vc = null;
            }
            else
                ShowViewController();
        }

        private void ShowViewController()
        {
            if (_vc == null)
            {
                _vc = new ViewController(_pnlCameras);
                if (_pnlCameras.Height > 0)
                {
                    double ar = Convert.ToDouble(_pnlCameras.Height)/Convert.ToDouble(_pnlCameras.Width);
                    _vc.Width = 180;
                    _vc.Height = Convert.ToInt32(ar*_vc.Width);
                }
                _vc.TopMost = true;

                _vc.Show();
                _vc.Closed += _vc_Closed;
                viewControllerToolStripMenuItem.Checked = menuItem14.Checked = Conf.ViewController = true;
            }
            else
            {
                _vc.Show();
            }
        }

        private void _vc_Closed(object sender, EventArgs e)
        {
            _vc = null;
            viewControllerToolStripMenuItem.Checked = menuItem14.Checked = Conf.ViewController = false;
        }

        private void _pnlCameras_Scroll(object sender, ScrollEventArgs e)
        {
            if (_vc != null)
                _vc.Redraw();
        }

        private void _toolStripDropDownButton2_Click(object sender, EventArgs e)
        {

        }

        private enum LayoutModes
        {
            bottom,
            left,
            right
        };

        private void menuItem16_Click(object sender, EventArgs e)
        {
            Conf.LayoutMode = (int)LayoutModes.bottom;
            Arrange(true);
        }

        private void menuItem17_Click(object sender, EventArgs e)
        {
            Conf.LayoutMode = (int)LayoutModes.left;
            Arrange(true);
        }

        private void menuItem19_Click(object sender, EventArgs e)
        {
            Conf.LayoutMode = (int)LayoutModes.right;
            Arrange(true);
        }

        private void Arrange(bool ShowIfHidden)
        {
            if (!Conf.ShowMediaPanel)
            {
                if (ShowIfHidden)
                {
                    mediaPaneToolStripMenuItem.Checked = menuItem7.Checked = true;
                    Conf.ShowMediaPanel = true;
                    ShowHideMediaPane();
                }
                else
                    return;
            }

            SuspendLayout();
            try {
                var lm = (LayoutModes) Conf.LayoutMode;

            
                switch (lm)
                {
                    case LayoutModes.bottom:
                        splitContainer1.Orientation = Orientation.Horizontal;
                        splitContainer1.RightToLeft = RightToLeft.No;

                        splitContainer2.Orientation = Orientation.Vertical;
                        splitContainer2.RightToLeft = RightToLeft.No;

                        splitContainer1.SplitterDistance = splitContainer1.Height-200;
                        splitContainer2.SplitterDistance = splitContainer2.Width - 200;
                        break;
                    case LayoutModes.left:
                        splitContainer1.Orientation = Orientation.Vertical;
                        splitContainer1.RightToLeft = RightToLeft.Yes;

                        splitContainer2.Orientation = Orientation.Horizontal;
                        splitContainer2.RightToLeft = RightToLeft.No;

                        splitContainer1.SplitterDistance = splitContainer1.Width - 200;
                        splitContainer2.SplitterDistance = splitContainer2.Height - 200;
                        break;
                    case LayoutModes.right:
                        splitContainer1.Orientation = Orientation.Vertical;
                        splitContainer1.RightToLeft = RightToLeft.No;

                        splitContainer2.Orientation = Orientation.Horizontal;
                        splitContainer2.RightToLeft = RightToLeft.No;

                        splitContainer1.SplitterDistance = splitContainer1.Width - 200;
                        splitContainer2.SplitterDistance = splitContainer2.Height - 200;

                        break;

                }
            }
            catch {}
            ResumeLayout(true);
        }

        private void flowPreview_ControlRemoved(object sender, ControlEventArgs e)
        {
            
        }

        private void menuItem18_Click(object sender, EventArgs e)
        {
            ShowHidePTZTool();
        }

        private void pTZControllerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowHidePTZTool();
        }

        private void ShowHidePTZTool()
        {
            bool bShow = true;
            if (_ptzTool != null)
            {
                _ptzTool.Close();
                _ptzTool.Dispose();
                _ptzTool = null;
                bShow = false;
            }
            else
            {
                _ptzTool = new PTZTool { Owner = this };
                _ptzTool.Show(this);
                _ptzTool.Closing += _ptzTool_Closing;
                _ptzTool.CameraControl = null;
                for (int i = 0; i < _pnlCameras.Controls.Count; i++)
                {
                    Control c = _pnlCameras.Controls[i];
                    if (c.Focused && c is CameraWindow)
                    {
                        _ptzTool.CameraControl = (CameraWindow)c;
                        break;
                    }
                }
            }
            pTZControllerToolStripMenuItem.Checked = menuItem18.Checked = pTZControllerToolStripMenuItem1.Checked = bShow;
            Conf.ShowPTZController = bShow;
        }

        void _ptzTool_Closing(object sender, CancelEventArgs e)
        {
            pTZControllerToolStripMenuItem.Checked = menuItem18.Checked = pTZControllerToolStripMenuItem1.Checked = false;
            Conf.ShowPTZController = false;
        }

        private IAudioSource _talkSource;
        private ITalkTarget _talkTarget;
        internal CameraWindow _talkCamera;

        public void TalkTo(CameraWindow cw, bool talk)
        {
            if (_talkSource != null)
            {
                _talkSource.Stop();
                _talkSource = null;
            }
            if (_talkTarget != null)
            {
                _talkTarget.Stop();
                _talkTarget = null;
            }

            if (!talk)
            {
                if (cw.VolumeControl != null)
                {
                    cw.VolumeControl.Listening = false;
                }
                return;
            }
            Application.DoEvents();
            _talkCamera = cw;
            _talkSource = new TalkDeviceStream(Conf.TalkMic) { RecordingFormat = new WaveFormat(8000, 16, 1) };
            _talkSource.AudioSourceError += _talkSource_AudioSourceError;

            if (!_talkSource.IsRunning)
                _talkSource.Start();           

            switch (cw.Camobject.settings.audiomodel)
            {
                default:
                    _talkTarget = new TalkFoscam(cw.Camobject.settings.audioip, cw.Camobject.settings.audioport, cw.Camobject.settings.audiousername, cw.Camobject.settings.audiopassword, _talkSource);
                    break;
                case "iSpyServer":
                    _talkTarget = new TalkiSpyServer(cw.Camobject.settings.audioip, cw.Camobject.settings.audioport, _talkSource);
                    break;
                case "NetworkKinect":
                    _talkTarget = new TalkNetworkKinect(cw.Camobject.settings.audioip, cw.Camobject.settings.audioport, _talkSource);
                    break;
                case "Axis":
                    _talkTarget = new TalkAxis(cw.Camobject.settings.audioip, cw.Camobject.settings.audioport, cw.Camobject.settings.audiousername, cw.Camobject.settings.audiopassword, _talkSource);
                    break;
            }
            
            _talkTarget.TalkStopped += TalkTargetTalkStopped;
            _talkTarget.Start();

            //auto listen
            if (cw.VolumeControl != null)
            {
                cw.VolumeControl.Listening = true;
            }
            
        }

        void _talkSource_AudioSourceError(object sender, Audio.AudioSourceErrorEventArgs eventArgs)
        {
            LogErrorToFile(eventArgs.Description);
        }

        void TalkTargetTalkStopped(object sender, EventArgs e)
        {
            if (_talkCamera!=null)
            {
                _talkCamera.Talking = false;
            }
        }

        private void pTZToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void viewControllerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_vc != null)
            {
                _vc.Close();
                _vc = null;
            }
            else
                ShowViewController();
        }


        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.mainMenu = new System.Windows.Forms.MainMenu(this.components);
            this._fileItem = new System.Windows.Forms.MenuItem();
            this._menuItem19 = new System.Windows.Forms.MenuItem();
            this._menuItem21 = new System.Windows.Forms.MenuItem();
            this._menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItem13 = new System.Windows.Forms.MenuItem();
            this._exitFileItem = new System.Windows.Forms.MenuItem();
            this._menuItem36 = new System.Windows.Forms.MenuItem();
            this._menuItem37 = new System.Windows.Forms.MenuItem();
            this._menuItem16 = new System.Windows.Forms.MenuItem();
            this._menuItem17 = new System.Windows.Forms.MenuItem();
            this._menuItem7 = new System.Windows.Forms.MenuItem();
            this._menuItem23 = new System.Windows.Forms.MenuItem();
            this._menuItem3 = new System.Windows.Forms.MenuItem();
            this._menuItem25 = new System.Windows.Forms.MenuItem();
            this.menuItem9 = new System.Windows.Forms.MenuItem();
            this.menuItem10 = new System.Windows.Forms.MenuItem();
            this.menuItem11 = new System.Windows.Forms.MenuItem();
            this.menuItem12 = new System.Windows.Forms.MenuItem();
            this._menuItem13 = new System.Windows.Forms.MenuItem();
            this._menuItem39 = new System.Windows.Forms.MenuItem();
            this._menuItem12 = new System.Windows.Forms.MenuItem();
            this._menuItem14 = new System.Windows.Forms.MenuItem();
            this._menuItem28 = new System.Windows.Forms.MenuItem();
            this.menuItem21 = new System.Windows.Forms.MenuItem();
            this._menuItem29 = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.mnuSaveLayout = new System.Windows.Forms.MenuItem();
            this.mnuResetLayout = new System.Windows.Forms.MenuItem();
            this.menuItem15 = new System.Windows.Forms.MenuItem();
            this.menuItem16 = new System.Windows.Forms.MenuItem();
            this.menuItem17 = new System.Windows.Forms.MenuItem();
            this.menuItem19 = new System.Windows.Forms.MenuItem();
            this.menuItem22 = new System.Windows.Forms.MenuItem();
            this.menuItem20 = new System.Windows.Forms.MenuItem();
            this._menuItem20 = new System.Windows.Forms.MenuItem();
            this._menuItem22 = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.menuItem4 = new System.Windows.Forms.MenuItem();
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.menuItem6 = new System.Windows.Forms.MenuItem();
            this.menuItem7 = new System.Windows.Forms.MenuItem();
            this.menuItem25 = new System.Windows.Forms.MenuItem();
            this.menuItem26 = new System.Windows.Forms.MenuItem();
            this.menuItem27 = new System.Windows.Forms.MenuItem();
            this.menuItem28 = new System.Windows.Forms.MenuItem();
            this.menuItem29 = new System.Windows.Forms.MenuItem();
            this.menuItem30 = new System.Windows.Forms.MenuItem();
            this.menuItem31 = new System.Windows.Forms.MenuItem();
            this.menuItem23 = new System.Windows.Forms.MenuItem();
            this.menuItem14 = new System.Windows.Forms.MenuItem();
            this.menuItem18 = new System.Windows.Forms.MenuItem();
            this.menuItem24 = new System.Windows.Forms.MenuItem();
            this.menuItem8 = new System.Windows.Forms.MenuItem();
            this._menuItem9 = new System.Windows.Forms.MenuItem();
            this._menuItem18 = new System.Windows.Forms.MenuItem();
            this._menuItem8 = new System.Windows.Forms.MenuItem();
            this._menuItem15 = new System.Windows.Forms.MenuItem();
            this._menuItem6 = new System.Windows.Forms.MenuItem();
            this._menuItem34 = new System.Windows.Forms.MenuItem();
            this._miOnAll = new System.Windows.Forms.MenuItem();
            this._miOnSched = new System.Windows.Forms.MenuItem();
            this._menuItem33 = new System.Windows.Forms.MenuItem();
            this._miOffAll = new System.Windows.Forms.MenuItem();
            this._miOffSched = new System.Windows.Forms.MenuItem();
            this._menuItem31 = new System.Windows.Forms.MenuItem();
            this._miApplySchedule = new System.Windows.Forms.MenuItem();
            this._menuItem32 = new System.Windows.Forms.MenuItem();
            this._menuItem4 = new System.Windows.Forms.MenuItem();
            this._menuItem35 = new System.Windows.Forms.MenuItem();
            this._helpItem = new System.Windows.Forms.MenuItem();
            this._aboutHelpItem = new System.Windows.Forms.MenuItem();
            this._menuItem30 = new System.Windows.Forms.MenuItem();
            this._menuItem2 = new System.Windows.Forms.MenuItem();
            this._menuItem24 = new System.Windows.Forms.MenuItem();
            this._menuItem10 = new System.Windows.Forms.MenuItem();
            this._menuItem38 = new System.Windows.Forms.MenuItem();
            this._menuItem11 = new System.Windows.Forms.MenuItem();
            this._menuItem5 = new System.Windows.Forms.MenuItem();
            this._menuItem27 = new System.Windows.Forms.MenuItem();
            this._menuItem26 = new System.Windows.Forms.MenuItem();
            this.ctxtMainForm = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._addCameraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._addMicrophoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._addFloorPlanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._remoteCommandsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._applyScheduleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.opacityToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.opacityToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.opacityToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.opacityToolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.layoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoLayoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveLayoutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.resetLayoutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.displayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fullScreenToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.statusBarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mediaPaneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pTZControllerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewControllerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.alwaysOnTopToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenu = new System.Windows.Forms.ToolStrip();
            this._toolStripDropDownButton2 = new System.Windows.Forms.ToolStripDropDownButton();
            this._localCameraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._iPCameraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.iPCameraWithWizardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.videoFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.otherVideoSourceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._microphoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._uSbCamerasAndMicrophonesOnOtherToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._floorPlanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this._thruWebsiteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._onMobileDevicesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.inExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolStripButton8 = new System.Windows.Forms.ToolStripButton();
            this._toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.tsbPlugins = new System.Windows.Forms.ToolStripButton();
            this._toolStripButton4 = new System.Windows.Forms.ToolStripButton();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.ctxtMnu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this._viewMediaOnAMobileDeviceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._activateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._setInactiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._recordNowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._takePhotoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pTZToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pTZControllerToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this._listenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._applyScheduleToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this._positionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fullScreenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._resetSizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._resetRecordingCounterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._showFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxtTaskbar = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._unlockToolstripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._switchAllOnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._switchAllOffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._showToolstripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._showISpy10PercentOpacityToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._showISpy30OpacityToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._showISpy100PercentOpacityToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._helpToolstripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._websiteToolstripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this._tsslStats = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslMonitor = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslPerformance = new System.Windows.Forms.ToolStripStatusLabel();
            this._pnlContent = new System.Windows.Forms.Panel();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.flowPreview = new System.Windows.Forms.FlowLayoutPanel();
            this.flCommands = new System.Windows.Forms.FlowLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.llblDelete = new System.Windows.Forms.LinkLabel();
            this.llblSelectAll = new System.Windows.Forms.LinkLabel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this._pnlCameras = new iSpyApplication.Controls.LayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.llblRefresh = new System.Windows.Forms.LinkLabel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.ctxtPlayer = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.iSpyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.defaultPlayerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.websiteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showInFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uploadToYouTubeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uploadToYouTubePublicToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxtMainForm.SuspendLayout();
            this.toolStripMenu.SuspendLayout();
            this.ctxtMnu.SuspendLayout();
            this.ctxtTaskbar.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this._pnlContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.flCommands.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.ctxtPlayer.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu
            // 
            this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._fileItem,
            this._menuItem36,
            this._menuItem16,
            this._menuItem9,
            this._helpItem});
            // 
            // _fileItem
            // 
            this._fileItem.Index = 0;
            this._fileItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._menuItem19,
            this._menuItem21,
            this._menuItem1,
            this.menuItem13,
            this._exitFileItem});
            this._fileItem.Text = "&File";
            // 
            // _menuItem19
            // 
            this._menuItem19.Index = 0;
            this._menuItem19.Text = "&Save Object List";
            this._menuItem19.Click += new System.EventHandler(this.MenuItem19Click);
            // 
            // _menuItem21
            // 
            this._menuItem21.Index = 1;
            this._menuItem21.Text = "&Open Object List";
            this._menuItem21.Click += new System.EventHandler(this.MenuItem21Click);
            // 
            // _menuItem1
            // 
            this._menuItem1.Index = 2;
            this._menuItem1.Text = "-";
            // 
            // menuItem13
            // 
            this.menuItem13.Index = 3;
            this.menuItem13.Text = "Purchase More Cameras";
            this.menuItem13.Click += new System.EventHandler(this.menuItem13_Click);
            // 
            // _exitFileItem
            // 
            this._exitFileItem.Index = 4;
            this._exitFileItem.Text = "E&xit";
            this._exitFileItem.Click += new System.EventHandler(this.ExitFileItemClick);
            // 
            // _menuItem36
            // 
            this._menuItem36.Index = 1;
            this._menuItem36.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._menuItem37});
            this._menuItem36.Text = "Edit";
            // 
            // _menuItem37
            // 
            this._menuItem37.Index = 0;
            this._menuItem37.Text = "Cameras and Microphones";
            this._menuItem37.Click += new System.EventHandler(this.MenuItem37Click);
            // 
            // _menuItem16
            // 
            this._menuItem16.Index = 2;
            this._menuItem16.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._menuItem17,
            this._menuItem3,
            this._menuItem25,
            this.menuItem9,
            this._menuItem13,
            this._menuItem39,
            this.mnuSaveLayout,
            this.mnuResetLayout,
            this.menuItem15,
            this.menuItem22,
            this.menuItem20,
            this._menuItem20,
            this._menuItem22,
            this.menuItem2,
            this.menuItem3,
            this.menuItem25,
            this.menuItem4,
            this.menuItem5,
            this.menuItem6,
            this.menuItem7,
            this.menuItem23,
            this.menuItem14,
            this.menuItem18,
            this.menuItem24,
            this.menuItem8});
            this._menuItem16.Text = "View";
            // 
            // _menuItem17
            // 
            this._menuItem17.Index = 0;
            this._menuItem17.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._menuItem7,
            this._menuItem23});
            this._menuItem17.Text = "&Files";
            this._menuItem17.Click += new System.EventHandler(this.MenuItem17Click);
            // 
            // _menuItem7
            // 
            this._menuItem7.Index = 0;
            this._menuItem7.Text = "&Video (files)";
            this._menuItem7.Click += new System.EventHandler(this.MenuItem7Click);
            // 
            // _menuItem23
            // 
            this._menuItem23.Index = 1;
            this._menuItem23.Text = "&Audio (files)";
            this._menuItem23.Click += new System.EventHandler(this.MenuItem23Click);
            // 
            // _menuItem3
            // 
            this._menuItem3.Index = 1;
            this._menuItem3.Text = "Media &Over the Web";
            this._menuItem3.Click += new System.EventHandler(this.MenuItem3Click);
            // 
            // _menuItem25
            // 
            this._menuItem25.Index = 2;
            this._menuItem25.Text = "Media on a Mobile &Device (iPhone/ Android/ Windows 7 etc)";
            this._menuItem25.Click += new System.EventHandler(this.MenuItem25Click);
            // 
            // menuItem9
            // 
            this.menuItem9.Index = 3;
            this.menuItem9.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem10,
            this.menuItem11,
            this.menuItem12});
            this.menuItem9.Text = "Opacity";
            // 
            // menuItem10
            // 
            this.menuItem10.Index = 0;
            this.menuItem10.Text = "10%";
            this.menuItem10.Click += new System.EventHandler(this.opacityToolStripMenuItem1_Click);
            // 
            // menuItem11
            // 
            this.menuItem11.Index = 1;
            this.menuItem11.Text = "30%";
            this.menuItem11.Click += new System.EventHandler(this.opacityToolStripMenuItem2_Click);
            // 
            // menuItem12
            // 
            this.menuItem12.Index = 2;
            this.menuItem12.Text = "100%";
            this.menuItem12.Click += new System.EventHandler(this.opacityToolStripMenuItem3_Click);
            // 
            // _menuItem13
            // 
            this._menuItem13.Index = 4;
            this._menuItem13.Text = "-";
            // 
            // _menuItem39
            // 
            this._menuItem39.Index = 5;
            this._menuItem39.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._menuItem12,
            this._menuItem14,
            this._menuItem28,
            this.menuItem21,
            this._menuItem29,
            this.menuItem1});
            this._menuItem39.Text = "Auto Layout Objects";
            this._menuItem39.Click += new System.EventHandler(this.MenuItem39Click);
            // 
            // _menuItem12
            // 
            this._menuItem12.Index = 0;
            this._menuItem12.Text = "160 x 120";
            this._menuItem12.Click += new System.EventHandler(this.MenuItem12Click);
            // 
            // _menuItem14
            // 
            this._menuItem14.Index = 1;
            this._menuItem14.Text = "320 x 240";
            this._menuItem14.Click += new System.EventHandler(this.MenuItem14Click);
            // 
            // _menuItem28
            // 
            this._menuItem28.Index = 2;
            this._menuItem28.Text = "640 x 480";
            this._menuItem28.Click += new System.EventHandler(this.MenuItem28Click2);
            // 
            // menuItem21
            // 
            this.menuItem21.Index = 3;
            this.menuItem21.Text = "Optimised";
            this.menuItem21.Click += new System.EventHandler(this.menuItem21_Click);
            // 
            // _menuItem29
            // 
            this._menuItem29.Index = 4;
            this._menuItem29.Text = "Current";
            this._menuItem29.Click += new System.EventHandler(this.MenuItem29Click1);
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 5;
            this.menuItem1.Text = "Native";
            this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click_1);
            // 
            // mnuSaveLayout
            // 
            this.mnuSaveLayout.Index = 6;
            this.mnuSaveLayout.Text = "&Save Layout";
            this.mnuSaveLayout.Click += new System.EventHandler(this.saveLayoutToolStripMenuItem1_Click);
            // 
            // mnuResetLayout
            // 
            this.mnuResetLayout.Index = 7;
            this.mnuResetLayout.Text = "&Reset Layout";
            this.mnuResetLayout.Click += new System.EventHandler(this.resetLayoutToolStripMenuItem1_Click);
            // 
            // menuItem15
            // 
            this.menuItem15.Index = 8;
            this.menuItem15.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem16,
            this.menuItem17,
            this.menuItem19});
            this.menuItem15.Text = "Arrange Media";
            // 
            // menuItem16
            // 
            this.menuItem16.Index = 0;
            this.menuItem16.Text = "Bottom";
            this.menuItem16.Click += new System.EventHandler(this.menuItem16_Click);
            // 
            // menuItem17
            // 
            this.menuItem17.Index = 1;
            this.menuItem17.Text = "Left";
            this.menuItem17.Click += new System.EventHandler(this.menuItem17_Click);
            // 
            // menuItem19
            // 
            this.menuItem19.Index = 2;
            this.menuItem19.Text = "Right";
            this.menuItem19.Click += new System.EventHandler(this.menuItem19_Click);
            // 
            // menuItem22
            // 
            this.menuItem22.Checked = true;
            this.menuItem22.Index = 9;
            this.menuItem22.Text = "Lock Layout";
            this.menuItem22.Click += new System.EventHandler(this.menuItem22_Click);
            // 
            // menuItem20
            // 
            this.menuItem20.Index = 10;
            this.menuItem20.Text = "-";
            // 
            // _menuItem20
            // 
            this._menuItem20.Index = 11;
            this._menuItem20.Text = "Log &File";
            this._menuItem20.Click += new System.EventHandler(this.MenuItem20Click);
            // 
            // _menuItem22
            // 
            this._menuItem22.Index = 12;
            this._menuItem22.Text = "Log F&iles";
            this._menuItem22.Click += new System.EventHandler(this.MenuItem22Click1);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 13;
            this.menuItem2.Text = "-";
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 14;
            this.menuItem3.Text = "Full Screen";
            this.menuItem3.Click += new System.EventHandler(this.fullScreenToolStripMenuItem1_Click);
            // 
            // menuItem4
            // 
            this.menuItem4.Index = 16;
            this.menuItem4.Text = "Status Bar";
            this.menuItem4.Click += new System.EventHandler(this.statusBarToolStripMenuItem_Click);
            // 
            // menuItem5
            // 
            this.menuItem5.Index = 17;
            this.menuItem5.Text = "File Menu";
            this.menuItem5.Click += new System.EventHandler(this.fileMenuToolStripMenuItem_Click);
            // 
            // menuItem6
            // 
            this.menuItem6.Index = 18;
            this.menuItem6.Text = "Tool Bar";
            this.menuItem6.Click += new System.EventHandler(this.toolStripToolStripMenuItem_Click);
            // 
            // menuItem7
            // 
            this.menuItem7.Index = 19;
            this.menuItem7.Text = "Media Pane";
            this.menuItem7.Click += new System.EventHandler(this.mediaPaneToolStripMenuItem_Click);
            // 
            // menuItem25
            // 
            this.menuItem25.Index = 15;
            this.menuItem25.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem26,
            this.menuItem27,
            this.menuItem28,
            this.menuItem29,
            this.menuItem30,
            this.menuItem31});
            this.menuItem25.Text = "New Grid View";
            this.menuItem25.Click += new System.EventHandler(this.menuItem25_Click);
            // 
            // menuItem26
            // 
            this.menuItem26.Index = 0;
            this.menuItem26.Text = "1x1";
            this.menuItem26.Click += new System.EventHandler(this.menuItem26_Click);
            // 
            // menuItem27
            // 
            this.menuItem27.Index = 1;
            this.menuItem27.Text = "2x2";
            this.menuItem27.Click += new System.EventHandler(this.menuItem27_Click);
            // 
            // menuItem28
            // 
            this.menuItem28.Index = 2;
            this.menuItem28.Text = "3x3";
            this.menuItem28.Click += new System.EventHandler(this.menuItem28_Click);
            // 
            // menuItem29
            // 
            this.menuItem29.Index = 3;
            this.menuItem29.Text = "4x4";
            this.menuItem29.Click += new System.EventHandler(this.menuItem29_Click);
            // 
            // menuItem30
            // 
            this.menuItem30.Index = 4;
            this.menuItem30.Text = "5x5";
            this.menuItem30.Click += new System.EventHandler(this.menuItem30_Click);
            // 
            // menuItem31
            // 
            this.menuItem31.Index = 5;
            this.menuItem31.Text = "Custom";
            this.menuItem31.Click += new System.EventHandler(this.menuItem31_Click);
            // 
            // menuItem23
            // 
            this.menuItem23.Index = 20;
            this.menuItem23.Text = "-";
            // 
            // menuItem14
            // 
            this.menuItem14.Index = 21;
            this.menuItem14.Text = "Layout Controller";
            this.menuItem14.Click += new System.EventHandler(this.menuItem14_Click);
            // 
            // menuItem18
            // 
            this.menuItem18.Index = 22;
            this.menuItem18.Text = "PTZ Controller";
            this.menuItem18.Click += new System.EventHandler(this.menuItem18_Click);
            // 
            // menuItem24
            // 
            this.menuItem24.Index = 23;
            this.menuItem24.Text = "-";
            // 
            // menuItem8
            // 
            this.menuItem8.Index = 24;
            this.menuItem8.Text = "Always on Top";
            this.menuItem8.Click += new System.EventHandler(this.alwaysOnTopToolStripMenuItem1_Click);
            // 
            // _menuItem9
            // 
            this._menuItem9.Index = 3;
            this._menuItem9.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._menuItem18,
            this._menuItem8,
            this._menuItem15,
            this._menuItem6,
            this._menuItem34,
            this._menuItem33,
            this._menuItem31,
            this._miApplySchedule,
            this._menuItem32,
            this._menuItem4,
            this._menuItem35});
            this._menuItem9.Text = "&Options";
            // 
            // _menuItem18
            // 
            this._menuItem18.Index = 0;
            this._menuItem18.Text = "&Clear Capture Directories";
            this._menuItem18.Click += new System.EventHandler(this.MenuItem18Click);
            // 
            // _menuItem8
            // 
            this._menuItem8.Index = 1;
            this._menuItem8.Text = "&Settings";
            this._menuItem8.Click += new System.EventHandler(this.MenuItem8Click);
            // 
            // _menuItem15
            // 
            this._menuItem15.Index = 2;
            this._menuItem15.Text = "Reset all Recording Counters";
            this._menuItem15.Click += new System.EventHandler(this.MenuItem15Click);
            // 
            // _menuItem6
            // 
            this._menuItem6.Index = 3;
            this._menuItem6.Text = "-";
            // 
            // _menuItem34
            // 
            this._menuItem34.Index = 4;
            this._menuItem34.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._miOnAll,
            this._miOnSched});
            this._menuItem34.Text = "Switch On";
            this._menuItem34.Click += new System.EventHandler(this.MenuItem34Click);
            // 
            // _miOnAll
            // 
            this._miOnAll.Index = 0;
            this._miOnAll.Text = "All";
            this._miOnAll.Click += new System.EventHandler(this.MenuItem24Click);
            // 
            // _miOnSched
            // 
            this._miOnSched.Index = 1;
            this._miOnSched.Text = "Scheduled";
            this._miOnSched.Click += new System.EventHandler(this.MenuItem28Click1);
            // 
            // _menuItem33
            // 
            this._menuItem33.Index = 5;
            this._menuItem33.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._miOffAll,
            this._miOffSched});
            this._menuItem33.Text = "Switch Off";
            this._menuItem33.Click += new System.EventHandler(this.MenuItem33Click);
            // 
            // _miOffAll
            // 
            this._miOffAll.Index = 0;
            this._miOffAll.Text = "All";
            this._miOffAll.Click += new System.EventHandler(this.MenuItem40Click);
            // 
            // _miOffSched
            // 
            this._miOffSched.Index = 1;
            this._miOffSched.Text = "Scheduled";
            this._miOffSched.Click += new System.EventHandler(this.MenuItem41Click);
            // 
            // _menuItem31
            // 
            this._menuItem31.Index = 6;
            this._menuItem31.Text = "&Remove All Objects";
            this._menuItem31.Click += new System.EventHandler(this.MenuItem31Click);
            // 
            // _miApplySchedule
            // 
            this._miApplySchedule.Index = 7;
            this._miApplySchedule.Text = "Apply Schedule";
            this._miApplySchedule.Click += new System.EventHandler(this.MenuItem24Click1);
            // 
            // _menuItem32
            // 
            this._menuItem32.Index = 8;
            this._menuItem32.Text = "-";
            // 
            // _menuItem4
            // 
            this._menuItem4.Index = 9;
            this._menuItem4.Text = "Configure &Remote Access";
            this._menuItem4.Click += new System.EventHandler(this.MenuItem4Click);
            // 
            // _menuItem35
            // 
            this._menuItem35.Index = 10;
            this._menuItem35.Text = "Configure &Remote Commands";
            this._menuItem35.Click += new System.EventHandler(this.MenuItem35Click);
            // 
            // _helpItem
            // 
            this._helpItem.Index = 4;
            this._helpItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._aboutHelpItem,
            this._menuItem30,
            this._menuItem2,
            this._menuItem24,
            this._menuItem10,
            this._menuItem38,
            this._menuItem11,
            this._menuItem5,
            this._menuItem27,
            this._menuItem26});
            this._helpItem.Text = "&Help";
            // 
            // _aboutHelpItem
            // 
            this._aboutHelpItem.Index = 0;
            this._aboutHelpItem.Text = "&About";
            this._aboutHelpItem.Click += new System.EventHandler(this.AboutHelpItemClick);
            // 
            // _menuItem30
            // 
            this._menuItem30.Index = 1;
            this._menuItem30.Text = "-";
            // 
            // _menuItem2
            // 
            this._menuItem2.Index = 2;
            this._menuItem2.Text = "&Help";
            this._menuItem2.Click += new System.EventHandler(this.MenuItem2Click);
            // 
            // _menuItem24
            // 
            this._menuItem24.Index = 3;
            this._menuItem24.Text = "Show &Getting Started";
            this._menuItem24.Click += new System.EventHandler(this.MenuItem24Click2);
            // 
            // _menuItem10
            // 
            this._menuItem10.Index = 4;
            this._menuItem10.Text = "&Check For Updates";
            this._menuItem10.Click += new System.EventHandler(this.MenuItem10Click);
            // 
            // _menuItem38
            // 
            this._menuItem38.Index = 5;
            this._menuItem38.Text = "View Update Information";
            this._menuItem38.Click += new System.EventHandler(this.MenuItem38Click);
            // 
            // _menuItem11
            // 
            this._menuItem11.Index = 6;
            this._menuItem11.Text = "&Report Bug/ Feedback";
            this._menuItem11.Click += new System.EventHandler(this.MenuItem11Click);
            // 
            // _menuItem5
            // 
            this._menuItem5.Index = 7;
            this._menuItem5.Text = "Go to &Website";
            this._menuItem5.Click += new System.EventHandler(this.MenuItem5Click);
            // 
            // _menuItem27
            // 
            this._menuItem27.Index = 8;
            this._menuItem27.Text = "-";
            // 
            // _menuItem26
            // 
            this._menuItem26.Index = 9;
            this._menuItem26.Text = "&Support iSpy With a Donation";
            this._menuItem26.Click += new System.EventHandler(this.MenuItem26Click);
            // 
            // ctxtMainForm
            // 
            this.ctxtMainForm.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._addCameraToolStripMenuItem,
            this._addMicrophoneToolStripMenuItem,
            this._addFloorPlanToolStripMenuItem,
            this._remoteCommandsToolStripMenuItem,
            this._settingsToolStripMenuItem,
            this._applyScheduleToolStripMenuItem,
            this.opacityToolStripMenuItem,
            this.layoutToolStripMenuItem,
            this.displayToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.ctxtMainForm.Name = "_ctxtMainForm";
            this.ctxtMainForm.Size = new System.Drawing.Size(181, 224);
            this.ctxtMainForm.Opening += new System.ComponentModel.CancelEventHandler(this.CtxtMainFormOpening);
            // 
            // _addCameraToolStripMenuItem
            // 
            this._addCameraToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_addCameraToolStripMenuItem.Image")));
            this._addCameraToolStripMenuItem.Name = "_addCameraToolStripMenuItem";
            this._addCameraToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this._addCameraToolStripMenuItem.Text = "Add &Camera";
            this._addCameraToolStripMenuItem.Click += new System.EventHandler(this.AddCameraToolStripMenuItemClick);
            // 
            // _addMicrophoneToolStripMenuItem
            // 
            this._addMicrophoneToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_addMicrophoneToolStripMenuItem.Image")));
            this._addMicrophoneToolStripMenuItem.Name = "_addMicrophoneToolStripMenuItem";
            this._addMicrophoneToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this._addMicrophoneToolStripMenuItem.Text = "Add &Microphone";
            this._addMicrophoneToolStripMenuItem.Click += new System.EventHandler(this.AddMicrophoneToolStripMenuItemClick);
            // 
            // _addFloorPlanToolStripMenuItem
            // 
            this._addFloorPlanToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_addFloorPlanToolStripMenuItem.Image")));
            this._addFloorPlanToolStripMenuItem.Name = "_addFloorPlanToolStripMenuItem";
            this._addFloorPlanToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this._addFloorPlanToolStripMenuItem.Text = "Add Floor &Plan";
            this._addFloorPlanToolStripMenuItem.Click += new System.EventHandler(this.AddFloorPlanToolStripMenuItemClick);
            // 
            // _remoteCommandsToolStripMenuItem
            // 
            this._remoteCommandsToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_remoteCommandsToolStripMenuItem.Image")));
            this._remoteCommandsToolStripMenuItem.Name = "_remoteCommandsToolStripMenuItem";
            this._remoteCommandsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this._remoteCommandsToolStripMenuItem.Text = "Remote Commands";
            this._remoteCommandsToolStripMenuItem.Click += new System.EventHandler(this.RemoteCommandsToolStripMenuItemClick);
            // 
            // _settingsToolStripMenuItem
            // 
            this._settingsToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_settingsToolStripMenuItem.Image")));
            this._settingsToolStripMenuItem.Name = "_settingsToolStripMenuItem";
            this._settingsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this._settingsToolStripMenuItem.Text = "&Settings";
            this._settingsToolStripMenuItem.Click += new System.EventHandler(this.SettingsToolStripMenuItemClick);
            // 
            // _applyScheduleToolStripMenuItem
            // 
            this._applyScheduleToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_applyScheduleToolStripMenuItem.Image")));
            this._applyScheduleToolStripMenuItem.Name = "_applyScheduleToolStripMenuItem";
            this._applyScheduleToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this._applyScheduleToolStripMenuItem.Text = "Apply Schedule";
            this._applyScheduleToolStripMenuItem.Click += new System.EventHandler(this.ApplyScheduleToolStripMenuItemClick1);
            // 
            // opacityToolStripMenuItem
            // 
            this.opacityToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.opacityToolStripMenuItem1,
            this.opacityToolStripMenuItem2,
            this.opacityToolStripMenuItem3});
            this.opacityToolStripMenuItem.Name = "opacityToolStripMenuItem";
            this.opacityToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.opacityToolStripMenuItem.Text = "Opacity";
            // 
            // opacityToolStripMenuItem1
            // 
            this.opacityToolStripMenuItem1.Name = "opacityToolStripMenuItem1";
            this.opacityToolStripMenuItem1.Size = new System.Drawing.Size(146, 22);
            this.opacityToolStripMenuItem1.Text = "10% Opacity";
            this.opacityToolStripMenuItem1.Click += new System.EventHandler(this.opacityToolStripMenuItem1_Click);
            // 
            // opacityToolStripMenuItem2
            // 
            this.opacityToolStripMenuItem2.Name = "opacityToolStripMenuItem2";
            this.opacityToolStripMenuItem2.Size = new System.Drawing.Size(146, 22);
            this.opacityToolStripMenuItem2.Text = "30% Opacity";
            this.opacityToolStripMenuItem2.Click += new System.EventHandler(this.opacityToolStripMenuItem2_Click);
            // 
            // opacityToolStripMenuItem3
            // 
            this.opacityToolStripMenuItem3.Name = "opacityToolStripMenuItem3";
            this.opacityToolStripMenuItem3.Size = new System.Drawing.Size(146, 22);
            this.opacityToolStripMenuItem3.Text = "100% Opacity";
            this.opacityToolStripMenuItem3.Click += new System.EventHandler(this.opacityToolStripMenuItem3_Click);
            // 
            // layoutToolStripMenuItem
            // 
            this.layoutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.autoLayoutToolStripMenuItem,
            this.saveLayoutToolStripMenuItem1,
            this.resetLayoutToolStripMenuItem1});
            this.layoutToolStripMenuItem.Name = "layoutToolStripMenuItem";
            this.layoutToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.layoutToolStripMenuItem.Text = "Layout";
            // 
            // autoLayoutToolStripMenuItem
            // 
            this.autoLayoutToolStripMenuItem.Name = "autoLayoutToolStripMenuItem";
            this.autoLayoutToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.autoLayoutToolStripMenuItem.Text = "Auto Layout";
            this.autoLayoutToolStripMenuItem.Click += new System.EventHandler(this.autoLayoutToolStripMenuItem_Click);
            // 
            // saveLayoutToolStripMenuItem1
            // 
            this.saveLayoutToolStripMenuItem1.Name = "saveLayoutToolStripMenuItem1";
            this.saveLayoutToolStripMenuItem1.Size = new System.Drawing.Size(141, 22);
            this.saveLayoutToolStripMenuItem1.Text = "Save Layout";
            this.saveLayoutToolStripMenuItem1.Click += new System.EventHandler(this.saveLayoutToolStripMenuItem1_Click);
            // 
            // resetLayoutToolStripMenuItem1
            // 
            this.resetLayoutToolStripMenuItem1.Name = "resetLayoutToolStripMenuItem1";
            this.resetLayoutToolStripMenuItem1.Size = new System.Drawing.Size(141, 22);
            this.resetLayoutToolStripMenuItem1.Text = "Reset Layout";
            this.resetLayoutToolStripMenuItem1.Click += new System.EventHandler(this.resetLayoutToolStripMenuItem1_Click);
            // 
            // displayToolStripMenuItem
            // 
            this.displayToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fullScreenToolStripMenuItem1,
            this.statusBarToolStripMenuItem,
            this.fileMenuToolStripMenuItem,
            this.toolStripToolStripMenuItem,
            this.mediaPaneToolStripMenuItem,
            this.pTZControllerToolStripMenuItem,
            this.viewControllerToolStripMenuItem,
            this.alwaysOnTopToolStripMenuItem1});
            this.displayToolStripMenuItem.Name = "displayToolStripMenuItem";
            this.displayToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.displayToolStripMenuItem.Text = "Display";
            // 
            // fullScreenToolStripMenuItem1
            // 
            this.fullScreenToolStripMenuItem1.Checked = true;
            this.fullScreenToolStripMenuItem1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.fullScreenToolStripMenuItem1.Name = "fullScreenToolStripMenuItem1";
            this.fullScreenToolStripMenuItem1.Size = new System.Drawing.Size(155, 22);
            this.fullScreenToolStripMenuItem1.Text = "Full Screen";
            this.fullScreenToolStripMenuItem1.Click += new System.EventHandler(this.fullScreenToolStripMenuItem1_Click);
            // 
            // statusBarToolStripMenuItem
            // 
            this.statusBarToolStripMenuItem.Checked = true;
            this.statusBarToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.statusBarToolStripMenuItem.Name = "statusBarToolStripMenuItem";
            this.statusBarToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.statusBarToolStripMenuItem.Text = "Status Bar";
            this.statusBarToolStripMenuItem.Click += new System.EventHandler(this.statusBarToolStripMenuItem_Click);
            // 
            // fileMenuToolStripMenuItem
            // 
            this.fileMenuToolStripMenuItem.Checked = true;
            this.fileMenuToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.fileMenuToolStripMenuItem.Name = "fileMenuToolStripMenuItem";
            this.fileMenuToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.fileMenuToolStripMenuItem.Text = "File Menu";
            this.fileMenuToolStripMenuItem.Click += new System.EventHandler(this.fileMenuToolStripMenuItem_Click);
            // 
            // toolStripToolStripMenuItem
            // 
            this.toolStripToolStripMenuItem.Checked = true;
            this.toolStripToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripToolStripMenuItem.Name = "toolStripToolStripMenuItem";
            this.toolStripToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.toolStripToolStripMenuItem.Text = "Tool Strip";
            this.toolStripToolStripMenuItem.Click += new System.EventHandler(this.toolStripToolStripMenuItem_Click);
            // 
            // mediaPaneToolStripMenuItem
            // 
            this.mediaPaneToolStripMenuItem.Checked = true;
            this.mediaPaneToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mediaPaneToolStripMenuItem.Name = "mediaPaneToolStripMenuItem";
            this.mediaPaneToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.mediaPaneToolStripMenuItem.Text = "Media Pane";
            this.mediaPaneToolStripMenuItem.Click += new System.EventHandler(this.mediaPaneToolStripMenuItem_Click);
            // 
            // pTZControllerToolStripMenuItem
            // 
            this.pTZControllerToolStripMenuItem.Checked = true;
            this.pTZControllerToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.pTZControllerToolStripMenuItem.Name = "pTZControllerToolStripMenuItem";
            this.pTZControllerToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.pTZControllerToolStripMenuItem.Text = "PTZ Controller";
            this.pTZControllerToolStripMenuItem.Click += new System.EventHandler(this.pTZControllerToolStripMenuItem_Click);
            // 
            // viewControllerToolStripMenuItem
            // 
            this.viewControllerToolStripMenuItem.Checked = true;
            this.viewControllerToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.viewControllerToolStripMenuItem.Name = "viewControllerToolStripMenuItem";
            this.viewControllerToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.viewControllerToolStripMenuItem.Text = "View Controller";
            this.viewControllerToolStripMenuItem.Click += new System.EventHandler(this.viewControllerToolStripMenuItem_Click);
            // 
            // alwaysOnTopToolStripMenuItem1
            // 
            this.alwaysOnTopToolStripMenuItem1.Checked = true;
            this.alwaysOnTopToolStripMenuItem1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.alwaysOnTopToolStripMenuItem1.Name = "alwaysOnTopToolStripMenuItem1";
            this.alwaysOnTopToolStripMenuItem1.Size = new System.Drawing.Size(155, 22);
            this.alwaysOnTopToolStripMenuItem1.Text = "Always on Top";
            this.alwaysOnTopToolStripMenuItem1.Click += new System.EventHandler(this.alwaysOnTopToolStripMenuItem1_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // toolStripMenu
            // 
            this.toolStripMenu.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._toolStripDropDownButton2,
            this._toolStripDropDownButton1,
            this._toolStripButton8,
            this._toolStripButton1,
            this.tsbPlugins,
            this._toolStripButton4});
            this.toolStripMenu.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStripMenu.Location = new System.Drawing.Point(0, 0);
            this.toolStripMenu.Name = "toolStripMenu";
            this.toolStripMenu.Size = new System.Drawing.Size(810, 39);
            this.toolStripMenu.TabIndex = 0;
            this.toolStripMenu.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.ToolStrip1ItemClicked);
            // 
            // _toolStripDropDownButton2
            // 
            this._toolStripDropDownButton2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._localCameraToolStripMenuItem,
            this._iPCameraToolStripMenuItem,
            this.iPCameraWithWizardToolStripMenuItem,
            this.videoFileToolStripMenuItem,
            this.otherVideoSourceToolStripMenuItem,
            this._microphoneToolStripMenuItem,
            this._uSbCamerasAndMicrophonesOnOtherToolStripMenuItem,
            this._floorPlanToolStripMenuItem});
            this._toolStripDropDownButton2.Image = global::iSpyApplication.Properties.Resources.DownloadProgram;
            this._toolStripDropDownButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolStripDropDownButton2.Name = "_toolStripDropDownButton2";
            this._toolStripDropDownButton2.ShowDropDownArrow = false;
            this._toolStripDropDownButton2.Size = new System.Drawing.Size(74, 36);
            this._toolStripDropDownButton2.Text = "Add...";
            this._toolStripDropDownButton2.Click += new System.EventHandler(this._toolStripDropDownButton2_Click);
            // 
            // _localCameraToolStripMenuItem
            // 
            this._localCameraToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.Music;
            this._localCameraToolStripMenuItem.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this._localCameraToolStripMenuItem.Name = "_localCameraToolStripMenuItem";
            this._localCameraToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0);
            this._localCameraToolStripMenuItem.Size = new System.Drawing.Size(331, 20);
            this._localCameraToolStripMenuItem.Text = "Local Camera";
            this._localCameraToolStripMenuItem.Click += new System.EventHandler(this.LocalCameraToolStripMenuItemClick);
            // 
            // _iPCameraToolStripMenuItem
            // 
            this._iPCameraToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.Computer;
            this._iPCameraToolStripMenuItem.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this._iPCameraToolStripMenuItem.Name = "_iPCameraToolStripMenuItem";
            this._iPCameraToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0);
            this._iPCameraToolStripMenuItem.Size = new System.Drawing.Size(331, 20);
            this._iPCameraToolStripMenuItem.Text = "IP Camera";
            this._iPCameraToolStripMenuItem.Click += new System.EventHandler(this.IpCameraToolStripMenuItemClick);
            // 
            // iPCameraWithWizardToolStripMenuItem
            // 
            this.iPCameraWithWizardToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.Computer;
            this.iPCameraWithWizardToolStripMenuItem.Name = "iPCameraWithWizardToolStripMenuItem";
            this.iPCameraWithWizardToolStripMenuItem.Size = new System.Drawing.Size(331, 22);
            this.iPCameraWithWizardToolStripMenuItem.Text = "IP Camera With Wizard";
            this.iPCameraWithWizardToolStripMenuItem.Click += new System.EventHandler(this.iPCameraWithWizardToolStripMenuItem_Click);
            // 
            // videoFileToolStripMenuItem
            // 
            this.videoFileToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.HardDrive;
            this.videoFileToolStripMenuItem.Name = "videoFileToolStripMenuItem";
            this.videoFileToolStripMenuItem.Size = new System.Drawing.Size(331, 22);
            this.videoFileToolStripMenuItem.Text = "Video File";
            this.videoFileToolStripMenuItem.Click += new System.EventHandler(this.videoFileToolStripMenuItem_Click);
            // 
            // otherVideoSourceToolStripMenuItem
            // 
            this.otherVideoSourceToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.Darkfix;
            this.otherVideoSourceToolStripMenuItem.Name = "otherVideoSourceToolStripMenuItem";
            this.otherVideoSourceToolStripMenuItem.Size = new System.Drawing.Size(331, 22);
            this.otherVideoSourceToolStripMenuItem.Text = "Other Video Source";
            this.otherVideoSourceToolStripMenuItem.Click += new System.EventHandler(this.otherVideoSourceToolStripMenuItem_Click);
            // 
            // _microphoneToolStripMenuItem
            // 
            this._microphoneToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.EMail;
            this._microphoneToolStripMenuItem.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this._microphoneToolStripMenuItem.Name = "_microphoneToolStripMenuItem";
            this._microphoneToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0);
            this._microphoneToolStripMenuItem.Size = new System.Drawing.Size(331, 20);
            this._microphoneToolStripMenuItem.Text = "Microphone";
            this._microphoneToolStripMenuItem.Click += new System.EventHandler(this.MicrophoneToolStripMenuItemClick);
            // 
            // _uSbCamerasAndMicrophonesOnOtherToolStripMenuItem
            // 
            this._uSbCamerasAndMicrophonesOnOtherToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.Picture;
            this._uSbCamerasAndMicrophonesOnOtherToolStripMenuItem.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this._uSbCamerasAndMicrophonesOnOtherToolStripMenuItem.Name = "_uSbCamerasAndMicrophonesOnOtherToolStripMenuItem";
            this._uSbCamerasAndMicrophonesOnOtherToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0);
            this._uSbCamerasAndMicrophonesOnOtherToolStripMenuItem.Size = new System.Drawing.Size(331, 20);
            this._uSbCamerasAndMicrophonesOnOtherToolStripMenuItem.Text = "Cameras and Microphones on Other Computers ";
            this._uSbCamerasAndMicrophonesOnOtherToolStripMenuItem.Click += new System.EventHandler(this.USbCamerasAndMicrophonesOnOtherToolStripMenuItemClick);
            // 
            // _floorPlanToolStripMenuItem
            // 
            this._floorPlanToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.Default;
            this._floorPlanToolStripMenuItem.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this._floorPlanToolStripMenuItem.Name = "_floorPlanToolStripMenuItem";
            this._floorPlanToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0);
            this._floorPlanToolStripMenuItem.Size = new System.Drawing.Size(331, 20);
            this._floorPlanToolStripMenuItem.Text = "Floor Plan";
            this._floorPlanToolStripMenuItem.Click += new System.EventHandler(this.FloorPlanToolStripMenuItemClick);
            // 
            // _toolStripDropDownButton1
            // 
            this._toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._thruWebsiteToolStripMenuItem,
            this._onMobileDevicesToolStripMenuItem,
            this.inExplorerToolStripMenuItem});
            this._toolStripDropDownButton1.Image = global::iSpyApplication.Properties.Resources.Video2;
            this._toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolStripDropDownButton1.Name = "_toolStripDropDownButton1";
            this._toolStripDropDownButton1.ShowDropDownArrow = false;
            this._toolStripDropDownButton1.Size = new System.Drawing.Size(115, 36);
            this._toolStripDropDownButton1.Text = "Access Media";
            this._toolStripDropDownButton1.Click += new System.EventHandler(this.ToolStripDropDownButton1Click);
            // 
            // _thruWebsiteToolStripMenuItem
            // 
            this._thruWebsiteToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.Firefox;
            this._thruWebsiteToolStripMenuItem.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this._thruWebsiteToolStripMenuItem.Name = "_thruWebsiteToolStripMenuItem";
            this._thruWebsiteToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0);
            this._thruWebsiteToolStripMenuItem.Size = new System.Drawing.Size(154, 20);
            this._thruWebsiteToolStripMenuItem.Text = "Online";
            this._thruWebsiteToolStripMenuItem.Click += new System.EventHandler(this.ThruWebsiteToolStripMenuItemClick);
            // 
            // _onMobileDevicesToolStripMenuItem
            // 
            this._onMobileDevicesToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_onMobileDevicesToolStripMenuItem.Image")));
            this._onMobileDevicesToolStripMenuItem.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this._onMobileDevicesToolStripMenuItem.Name = "_onMobileDevicesToolStripMenuItem";
            this._onMobileDevicesToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0);
            this._onMobileDevicesToolStripMenuItem.Size = new System.Drawing.Size(154, 20);
            this._onMobileDevicesToolStripMenuItem.Text = "Mobile Devices";
            this._onMobileDevicesToolStripMenuItem.Click += new System.EventHandler(this.OnMobileDevicesToolStripMenuItemClick);
            // 
            // inExplorerToolStripMenuItem
            // 
            this.inExplorerToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.HardDrive;
            this.inExplorerToolStripMenuItem.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.inExplorerToolStripMenuItem.Name = "inExplorerToolStripMenuItem";
            this.inExplorerToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.inExplorerToolStripMenuItem.Text = "Files";
            this.inExplorerToolStripMenuItem.Click += new System.EventHandler(this.inExplorerToolStripMenuItem_Click);
            // 
            // _toolStripButton8
            // 
            this._toolStripButton8.Image = global::iSpyApplication.Properties.Resources.Run;
            this._toolStripButton8.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolStripButton8.Name = "_toolStripButton8";
            this._toolStripButton8.Size = new System.Drawing.Size(105, 36);
            this._toolStripButton8.Text = "Commands";
            this._toolStripButton8.Click += new System.EventHandler(this.ToolStripButton8Click1);
            // 
            // _toolStripButton1
            // 
            this._toolStripButton1.Image = global::iSpyApplication.Properties.Resources.Network;
            this._toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolStripButton1.Name = "_toolStripButton1";
            this._toolStripButton1.Size = new System.Drawing.Size(112, 36);
            this._toolStripButton1.Text = "Web Settings";
            this._toolStripButton1.Click += new System.EventHandler(this.ToolStripButton1Click1);
            // 
            // tsbPlugins
            // 
            this.tsbPlugins.Image = global::iSpyApplication.Properties.Resources.Desktop;
            this.tsbPlugins.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbPlugins.Name = "tsbPlugins";
            this.tsbPlugins.Size = new System.Drawing.Size(82, 36);
            this.tsbPlugins.Text = "Plugins";
            this.tsbPlugins.Click += new System.EventHandler(this.tsbPlugins_Click);
            // 
            // _toolStripButton4
            // 
            this._toolStripButton4.Image = global::iSpyApplication.Properties.Resources.ControlPanel;
            this._toolStripButton4.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolStripButton4.Name = "_toolStripButton4";
            this._toolStripButton4.Size = new System.Drawing.Size(85, 36);
            this._toolStripButton4.Text = "Settings";
            this._toolStripButton4.Click += new System.EventHandler(this.ToolStripButton4Click);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "iSpy";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.Click += new System.EventHandler(this.NotifyIcon1Click);
            this.notifyIcon1.DoubleClick += new System.EventHandler(this.NotifyIcon1DoubleClick);
            // 
            // ctxtMnu
            // 
            this.ctxtMnu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._toolStripMenuItem1,
            this._viewMediaOnAMobileDeviceToolStripMenuItem,
            this._activateToolStripMenuItem,
            this._setInactiveToolStripMenuItem,
            this._recordNowToolStripMenuItem,
            this._takePhotoToolStripMenuItem,
            this.pTZToolStripMenuItem,
            this._listenToolStripMenuItem,
            this._editToolStripMenuItem,
            this._applyScheduleToolStripMenuItem1,
            this._positionToolStripMenuItem,
            this.fullScreenToolStripMenuItem,
            this._resetSizeToolStripMenuItem,
            this._resetRecordingCounterToolStripMenuItem,
            this._showFilesToolStripMenuItem,
            this._deleteToolStripMenuItem});
            this.ctxtMnu.Name = "_ctxtMnu";
            this.ctxtMnu.Size = new System.Drawing.Size(240, 356);
            // 
            // _toolStripMenuItem1
            // 
            this._toolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("_toolStripMenuItem1.Image")));
            this._toolStripMenuItem1.Name = "_toolStripMenuItem1";
            this._toolStripMenuItem1.Size = new System.Drawing.Size(239, 22);
            this._toolStripMenuItem1.Text = "View &Media ";
            this._toolStripMenuItem1.Click += new System.EventHandler(this.ToolStripMenuItem1Click);
            // 
            // _viewMediaOnAMobileDeviceToolStripMenuItem
            // 
            this._viewMediaOnAMobileDeviceToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_viewMediaOnAMobileDeviceToolStripMenuItem.Image")));
            this._viewMediaOnAMobileDeviceToolStripMenuItem.Name = "_viewMediaOnAMobileDeviceToolStripMenuItem";
            this._viewMediaOnAMobileDeviceToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this._viewMediaOnAMobileDeviceToolStripMenuItem.Text = "View Media on a Mobile &Device";
            this._viewMediaOnAMobileDeviceToolStripMenuItem.Click += new System.EventHandler(this.ViewMediaOnAMobileDeviceToolStripMenuItemClick);
            // 
            // _activateToolStripMenuItem
            // 
            this._activateToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_activateToolStripMenuItem.Image")));
            this._activateToolStripMenuItem.Name = "_activateToolStripMenuItem";
            this._activateToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this._activateToolStripMenuItem.Text = "Switch &On";
            this._activateToolStripMenuItem.Click += new System.EventHandler(this.ActivateToolStripMenuItemClick);
            // 
            // _setInactiveToolStripMenuItem
            // 
            this._setInactiveToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_setInactiveToolStripMenuItem.Image")));
            this._setInactiveToolStripMenuItem.Name = "_setInactiveToolStripMenuItem";
            this._setInactiveToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this._setInactiveToolStripMenuItem.Text = "&Switch Off";
            this._setInactiveToolStripMenuItem.Click += new System.EventHandler(this.SetInactiveToolStripMenuItemClick);
            // 
            // _recordNowToolStripMenuItem
            // 
            this._recordNowToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_recordNowToolStripMenuItem.Image")));
            this._recordNowToolStripMenuItem.Name = "_recordNowToolStripMenuItem";
            this._recordNowToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this._recordNowToolStripMenuItem.Text = "Record Now";
            this._recordNowToolStripMenuItem.Click += new System.EventHandler(this.RecordNowToolStripMenuItemClick);
            // 
            // _takePhotoToolStripMenuItem
            // 
            this._takePhotoToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_takePhotoToolStripMenuItem.Image")));
            this._takePhotoToolStripMenuItem.Name = "_takePhotoToolStripMenuItem";
            this._takePhotoToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this._takePhotoToolStripMenuItem.Text = "Take Photo";
            this._takePhotoToolStripMenuItem.Click += new System.EventHandler(this.TakePhotoToolStripMenuItemClick);
            // 
            // pTZToolStripMenuItem
            // 
            this.pTZToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pTZControllerToolStripMenuItem1});
            this.pTZToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("pTZToolStripMenuItem.Image")));
            this.pTZToolStripMenuItem.Name = "pTZToolStripMenuItem";
            this.pTZToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this.pTZToolStripMenuItem.Text = "PTZ";
            this.pTZToolStripMenuItem.Click += new System.EventHandler(this.pTZToolStripMenuItem_Click);
            // 
            // pTZControllerToolStripMenuItem1
            // 
            this.pTZControllerToolStripMenuItem1.Name = "pTZControllerToolStripMenuItem1";
            this.pTZControllerToolStripMenuItem1.Size = new System.Drawing.Size(151, 22);
            this.pTZControllerToolStripMenuItem1.Text = "PTZ Controller";
            this.pTZControllerToolStripMenuItem1.Click += new System.EventHandler(this.pTZControllerToolStripMenuItem1_Click);
            // 
            // _listenToolStripMenuItem
            // 
            this._listenToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_listenToolStripMenuItem.Image")));
            this._listenToolStripMenuItem.Name = "_listenToolStripMenuItem";
            this._listenToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this._listenToolStripMenuItem.Text = "Listen";
            this._listenToolStripMenuItem.Click += new System.EventHandler(this.ListenToolStripMenuItemClick);
            // 
            // _editToolStripMenuItem
            // 
            this._editToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_editToolStripMenuItem.Image")));
            this._editToolStripMenuItem.Name = "_editToolStripMenuItem";
            this._editToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this._editToolStripMenuItem.Text = "&Edit";
            this._editToolStripMenuItem.Click += new System.EventHandler(this.EditToolStripMenuItemClick);
            // 
            // _applyScheduleToolStripMenuItem1
            // 
            this._applyScheduleToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("_applyScheduleToolStripMenuItem1.Image")));
            this._applyScheduleToolStripMenuItem1.Name = "_applyScheduleToolStripMenuItem1";
            this._applyScheduleToolStripMenuItem1.Size = new System.Drawing.Size(239, 22);
            this._applyScheduleToolStripMenuItem1.Text = "Apply Schedule";
            this._applyScheduleToolStripMenuItem1.Click += new System.EventHandler(this.ApplyScheduleToolStripMenuItem1Click);
            // 
            // _positionToolStripMenuItem
            // 
            this._positionToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_positionToolStripMenuItem.Image")));
            this._positionToolStripMenuItem.Name = "_positionToolStripMenuItem";
            this._positionToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this._positionToolStripMenuItem.Text = "Position";
            this._positionToolStripMenuItem.Click += new System.EventHandler(this.PositionToolStripMenuItemClick);
            // 
            // fullScreenToolStripMenuItem
            // 
            this.fullScreenToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("fullScreenToolStripMenuItem.Image")));
            this.fullScreenToolStripMenuItem.Name = "fullScreenToolStripMenuItem";
            this.fullScreenToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this.fullScreenToolStripMenuItem.Text = "Full Screen";
            this.fullScreenToolStripMenuItem.Click += new System.EventHandler(this.fullScreenToolStripMenuItem_Click);
            // 
            // _resetSizeToolStripMenuItem
            // 
            this._resetSizeToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_resetSizeToolStripMenuItem.Image")));
            this._resetSizeToolStripMenuItem.Name = "_resetSizeToolStripMenuItem";
            this._resetSizeToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this._resetSizeToolStripMenuItem.Text = "Reset Si&ze";
            this._resetSizeToolStripMenuItem.Click += new System.EventHandler(this.ResetSizeToolStripMenuItemClick);
            // 
            // _resetRecordingCounterToolStripMenuItem
            // 
            this._resetRecordingCounterToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_resetRecordingCounterToolStripMenuItem.Image")));
            this._resetRecordingCounterToolStripMenuItem.Name = "_resetRecordingCounterToolStripMenuItem";
            this._resetRecordingCounterToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this._resetRecordingCounterToolStripMenuItem.Text = "Reset Recording Counter";
            this._resetRecordingCounterToolStripMenuItem.Click += new System.EventHandler(this.ResetRecordingCounterToolStripMenuItemClick);
            // 
            // _showFilesToolStripMenuItem
            // 
            this._showFilesToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_showFilesToolStripMenuItem.Image")));
            this._showFilesToolStripMenuItem.Name = "_showFilesToolStripMenuItem";
            this._showFilesToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this._showFilesToolStripMenuItem.Text = "Show Files";
            this._showFilesToolStripMenuItem.Click += new System.EventHandler(this.ShowFilesToolStripMenuItemClick);
            // 
            // _deleteToolStripMenuItem
            // 
            this._deleteToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_deleteToolStripMenuItem.Image")));
            this._deleteToolStripMenuItem.Name = "_deleteToolStripMenuItem";
            this._deleteToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this._deleteToolStripMenuItem.Text = "&Remove";
            this._deleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteToolStripMenuItemClick);
            // 
            // ctxtTaskbar
            // 
            this.ctxtTaskbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._unlockToolstripMenuItem,
            this._switchAllOnToolStripMenuItem,
            this._switchAllOffToolStripMenuItem,
            this._showToolstripMenuItem,
            this._showISpy10PercentOpacityToolStripMenuItem,
            this._showISpy30OpacityToolStripMenuItem,
            this._showISpy100PercentOpacityToolStripMenuItem,
            this._helpToolstripMenuItem,
            this._websiteToolstripMenuItem,
            this._exitToolStripMenuItem});
            this.ctxtTaskbar.Name = "_ctxtMnu";
            this.ctxtTaskbar.Size = new System.Drawing.Size(219, 224);
            this.ctxtTaskbar.Opening += new System.ComponentModel.CancelEventHandler(this.CtxtTaskbarOpening);
            // 
            // _unlockToolstripMenuItem
            // 
            this._unlockToolstripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_unlockToolstripMenuItem.Image")));
            this._unlockToolstripMenuItem.Name = "_unlockToolstripMenuItem";
            this._unlockToolstripMenuItem.Size = new System.Drawing.Size(218, 22);
            this._unlockToolstripMenuItem.Text = "&Unlock";
            this._unlockToolstripMenuItem.Click += new System.EventHandler(this.UnlockToolstripMenuItemClick);
            // 
            // _switchAllOnToolStripMenuItem
            // 
            this._switchAllOnToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_switchAllOnToolStripMenuItem.Image")));
            this._switchAllOnToolStripMenuItem.Name = "_switchAllOnToolStripMenuItem";
            this._switchAllOnToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this._switchAllOnToolStripMenuItem.Text = "Switch All On";
            this._switchAllOnToolStripMenuItem.Click += new System.EventHandler(this.SwitchAllOnToolStripMenuItemClick);
            // 
            // _switchAllOffToolStripMenuItem
            // 
            this._switchAllOffToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_switchAllOffToolStripMenuItem.Image")));
            this._switchAllOffToolStripMenuItem.Name = "_switchAllOffToolStripMenuItem";
            this._switchAllOffToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this._switchAllOffToolStripMenuItem.Text = "Switch All Off";
            this._switchAllOffToolStripMenuItem.Click += new System.EventHandler(this.SwitchAllOffToolStripMenuItemClick);
            // 
            // _showToolstripMenuItem
            // 
            this._showToolstripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_showToolstripMenuItem.Image")));
            this._showToolstripMenuItem.Name = "_showToolstripMenuItem";
            this._showToolstripMenuItem.Size = new System.Drawing.Size(218, 22);
            this._showToolstripMenuItem.Text = "&Show iSpy";
            this._showToolstripMenuItem.Click += new System.EventHandler(this.ShowToolstripMenuItemClick);
            // 
            // _showISpy10PercentOpacityToolStripMenuItem
            // 
            this._showISpy10PercentOpacityToolStripMenuItem.Name = "_showISpy10PercentOpacityToolStripMenuItem";
            this._showISpy10PercentOpacityToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this._showISpy10PercentOpacityToolStripMenuItem.Text = "Show iSpy @ 10% opacity";
            this._showISpy10PercentOpacityToolStripMenuItem.Click += new System.EventHandler(this.ShowISpy10PercentOpacityToolStripMenuItemClick);
            // 
            // _showISpy30OpacityToolStripMenuItem
            // 
            this._showISpy30OpacityToolStripMenuItem.Name = "_showISpy30OpacityToolStripMenuItem";
            this._showISpy30OpacityToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this._showISpy30OpacityToolStripMenuItem.Text = "Show iSpy @ 30% opacity";
            this._showISpy30OpacityToolStripMenuItem.Click += new System.EventHandler(this.ShowISpy30OpacityToolStripMenuItemClick);
            // 
            // _showISpy100PercentOpacityToolStripMenuItem
            // 
            this._showISpy100PercentOpacityToolStripMenuItem.Name = "_showISpy100PercentOpacityToolStripMenuItem";
            this._showISpy100PercentOpacityToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this._showISpy100PercentOpacityToolStripMenuItem.Text = "Show iSpy @ 100 % opacity";
            this._showISpy100PercentOpacityToolStripMenuItem.Click += new System.EventHandler(this.ShowISpy100PercentOpacityToolStripMenuItemClick);
            // 
            // _helpToolstripMenuItem
            // 
            this._helpToolstripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_helpToolstripMenuItem.Image")));
            this._helpToolstripMenuItem.Name = "_helpToolstripMenuItem";
            this._helpToolstripMenuItem.Size = new System.Drawing.Size(218, 22);
            this._helpToolstripMenuItem.Text = "&Help";
            this._helpToolstripMenuItem.Click += new System.EventHandler(this.HelpToolstripMenuItemClick);
            // 
            // _websiteToolstripMenuItem
            // 
            this._websiteToolstripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_websiteToolstripMenuItem.Image")));
            this._websiteToolstripMenuItem.Name = "_websiteToolstripMenuItem";
            this._websiteToolstripMenuItem.Size = new System.Drawing.Size(218, 22);
            this._websiteToolstripMenuItem.Text = "&Website";
            this._websiteToolstripMenuItem.Click += new System.EventHandler(this.WebsiteToolstripMenuItemClick);
            // 
            // _exitToolStripMenuItem
            // 
            this._exitToolStripMenuItem.Name = "_exitToolStripMenuItem";
            this._exitToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this._exitToolStripMenuItem.Text = "Exit";
            this._exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItemClick);
            // 
            // statusStrip1
            // 
            this.statusStrip1.AutoSize = false;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._tsslStats,
            this.tsslMonitor,
            this.tsslPerformance});
            this.statusStrip1.Location = new System.Drawing.Point(0, 424);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(810, 30);
            this.statusStrip1.TabIndex = 0;
            // 
            // _tsslStats
            // 
            this._tsslStats.Name = "_tsslStats";
            this._tsslStats.Size = new System.Drawing.Size(59, 25);
            this._tsslStats.Text = "Loading...";
            // 
            // tsslMonitor
            // 
            this.tsslMonitor.Name = "tsslMonitor";
            this.tsslMonitor.Size = new System.Drawing.Size(76, 25);
            this.tsslMonitor.Text = "Monitoring...";
            // 
            // tsslPerformance
            // 
            this.tsslPerformance.ForeColor = System.Drawing.Color.Blue;
            this.tsslPerformance.IsLink = true;
            this.tsslPerformance.Name = "tsslPerformance";
            this.tsslPerformance.Size = new System.Drawing.Size(56, 25);
            this.tsslPerformance.Text = "Perf. Tips";
            this.tsslPerformance.Click += new System.EventHandler(this.toolStripStatusLabel1_Click);
            // 
            // _pnlContent
            // 
            this._pnlContent.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this._pnlContent.Controls.Add(this.splitContainer2);
            this._pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pnlContent.Location = new System.Drawing.Point(0, 0);
            this._pnlContent.Margin = new System.Windows.Forms.Padding(0);
            this._pnlContent.Name = "_pnlContent";
            this._pnlContent.Size = new System.Drawing.Size(810, 103);
            this._pnlContent.TabIndex = 20;
            // 
            // splitContainer2
            // 
            this.splitContainer2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.flowPreview);
            this.splitContainer2.Panel1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.flCommands);
            this.splitContainer2.Panel2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.splitContainer2.Size = new System.Drawing.Size(810, 103);
            this.splitContainer2.SplitterDistance = 576;
            this.splitContainer2.TabIndex = 88;
            // 
            // flowPreview
            // 
            this.flowPreview.AutoScroll = true;
            this.flowPreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(79)))), ((int)(((byte)(79)))));
            this.flowPreview.ContextMenuStrip = this.ctxtMainForm;
            this.flowPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowPreview.Location = new System.Drawing.Point(0, 0);
            this.flowPreview.Margin = new System.Windows.Forms.Padding(0);
            this.flowPreview.Name = "flowPreview";
            this.flowPreview.Size = new System.Drawing.Size(576, 103);
            this.flowPreview.TabIndex = 87;
            this.flowPreview.Click += new System.EventHandler(this.flowPreview_Click);
            this.flowPreview.ControlRemoved += new System.Windows.Forms.ControlEventHandler(this.flowPreview_ControlRemoved);
            this.flowPreview.MouseEnter += new System.EventHandler(this.flowPreview_MouseEnter);
            // 
            // flCommands
            // 
            this.flCommands.AutoScroll = true;
            this.flCommands.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.flCommands.Controls.Add(this.panel2);
            this.flCommands.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flCommands.Location = new System.Drawing.Point(0, 0);
            this.flCommands.Name = "flCommands";
            this.flCommands.Size = new System.Drawing.Size(230, 103);
            this.flCommands.TabIndex = 0;
            this.flCommands.MouseEnter += new System.EventHandler(this.flCommands_MouseEnter);
            // 
            // panel2
            // 
            this.panel2.AutoSize = true;
            this.panel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(0, 0);
            this.panel2.TabIndex = 0;
            // 
            // llblDelete
            // 
            this.llblDelete.ActiveLinkColor = System.Drawing.Color.White;
            this.llblDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.llblDelete.AutoSize = true;
            this.llblDelete.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.llblDelete.LinkColor = System.Drawing.Color.White;
            this.llblDelete.Location = new System.Drawing.Point(81, 0);
            this.llblDelete.Name = "llblDelete";
            this.llblDelete.Size = new System.Drawing.Size(38, 13);
            this.llblDelete.TabIndex = 1;
            this.llblDelete.TabStop = true;
            this.llblDelete.Text = "Delete";
            this.llblDelete.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.llblDelete.VisitedLinkColor = System.Drawing.Color.White;
            this.llblDelete.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llblDelete_LinkClicked);
            // 
            // llblSelectAll
            // 
            this.llblSelectAll.ActiveLinkColor = System.Drawing.Color.White;
            this.llblSelectAll.AutoSize = true;
            this.llblSelectAll.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.llblSelectAll.LinkColor = System.Drawing.Color.White;
            this.llblSelectAll.Location = new System.Drawing.Point(3, 0);
            this.llblSelectAll.Name = "llblSelectAll";
            this.llblSelectAll.Size = new System.Drawing.Size(72, 13);
            this.llblSelectAll.TabIndex = 0;
            this.llblSelectAll.TabStop = true;
            this.llblSelectAll.Text = "Select / Clear";
            this.llblSelectAll.VisitedLinkColor = System.Drawing.Color.White;
            this.llblSelectAll.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llblSelectAll_LinkClicked);
            // 
            // splitContainer1
            // 
            this.splitContainer1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 39);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this._pnlCameras);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            this.splitContainer1.Panel2.Controls.Add(this._pnlContent);
            this.splitContainer1.Panel2MinSize = 20;
            this.splitContainer1.Size = new System.Drawing.Size(810, 385);
            this.splitContainer1.SplitterDistance = 278;
            this.splitContainer1.TabIndex = 21;
            // 
            // _pnlCameras
            // 
            this._pnlCameras.AutoScroll = true;
            this._pnlCameras.AutoSize = true;
            this._pnlCameras.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._pnlCameras.BackColor = System.Drawing.Color.DimGray;
            this._pnlCameras.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this._pnlCameras.ContextMenuStrip = this.ctxtMainForm;
            this._pnlCameras.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pnlCameras.Location = new System.Drawing.Point(0, 0);
            this._pnlCameras.Margin = new System.Windows.Forms.Padding(0);
            this._pnlCameras.Name = "_pnlCameras";
            this._pnlCameras.Size = new System.Drawing.Size(810, 278);
            this._pnlCameras.TabIndex = 19;
            this._pnlCameras.Scroll += new System.Windows.Forms.ScrollEventHandler(this._pnlCameras_Scroll);
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.panel1.Controls.Add(this.flowLayoutPanel1);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(3);
            this.panel1.Size = new System.Drawing.Size(178, 19);
            this.panel1.TabIndex = 20;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.llblSelectAll);
            this.flowLayoutPanel1.Controls.Add(this.llblDelete);
            this.flowLayoutPanel1.Controls.Add(this.llblRefresh);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(172, 13);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // llblRefresh
            // 
            this.llblRefresh.ActiveLinkColor = System.Drawing.Color.White;
            this.llblRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.llblRefresh.AutoSize = true;
            this.llblRefresh.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.llblRefresh.LinkColor = System.Drawing.Color.White;
            this.llblRefresh.Location = new System.Drawing.Point(125, 0);
            this.llblRefresh.Name = "llblRefresh";
            this.llblRefresh.Size = new System.Drawing.Size(44, 13);
            this.llblRefresh.TabIndex = 2;
            this.llblRefresh.TabStop = true;
            this.llblRefresh.Text = "Refresh";
            this.llblRefresh.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.llblRefresh.VisitedLinkColor = System.Drawing.Color.White;
            this.llblRefresh.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llblRefresh_LinkClicked);
            // 
            // ctxtPlayer
            // 
            this.ctxtPlayer.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.iSpyToolStripMenuItem,
            this.defaultPlayerToolStripMenuItem,
            this.websiteToolStripMenuItem,
            this.showInFolderToolStripMenuItem,
            this.uploadToYouTubeToolStripMenuItem,
            this.uploadToYouTubePublicToolStripMenuItem,
            this.saveToToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.ctxtPlayer.Name = "ctxPlayer";
            this.ctxtPlayer.Size = new System.Drawing.Size(225, 180);
            // 
            // iSpyToolStripMenuItem
            // 
            this.iSpyToolStripMenuItem.Name = "iSpyToolStripMenuItem";
            this.iSpyToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.iSpyToolStripMenuItem.Text = "Play in iSpy";
            this.iSpyToolStripMenuItem.Click += new System.EventHandler(this.iSpyToolStripMenuItem_Click);
            // 
            // defaultPlayerToolStripMenuItem
            // 
            this.defaultPlayerToolStripMenuItem.Name = "defaultPlayerToolStripMenuItem";
            this.defaultPlayerToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.defaultPlayerToolStripMenuItem.Text = "Play in Default Player";
            this.defaultPlayerToolStripMenuItem.Click += new System.EventHandler(this.defaultPlayerToolStripMenuItem_Click);
            // 
            // websiteToolStripMenuItem
            // 
            this.websiteToolStripMenuItem.Name = "websiteToolStripMenuItem";
            this.websiteToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.websiteToolStripMenuItem.Text = "Play on Website";
            this.websiteToolStripMenuItem.Click += new System.EventHandler(this.websiteToolStripMenuItem_Click);
            // 
            // showInFolderToolStripMenuItem
            // 
            this.showInFolderToolStripMenuItem.Name = "showInFolderToolStripMenuItem";
            this.showInFolderToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.showInFolderToolStripMenuItem.Text = "Show in Folder";
            this.showInFolderToolStripMenuItem.Click += new System.EventHandler(this.showInFolderToolStripMenuItem_Click);
            // 
            // uploadToYouTubeToolStripMenuItem
            // 
            this.uploadToYouTubeToolStripMenuItem.Name = "uploadToYouTubeToolStripMenuItem";
            this.uploadToYouTubeToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.uploadToYouTubeToolStripMenuItem.Text = "Upload to YouTube (Private)";
            this.uploadToYouTubeToolStripMenuItem.Click += new System.EventHandler(this.uploadToYouTubeToolStripMenuItem_Click);
            // 
            // uploadToYouTubePublicToolStripMenuItem
            // 
            this.uploadToYouTubePublicToolStripMenuItem.Name = "uploadToYouTubePublicToolStripMenuItem";
            this.uploadToYouTubePublicToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.uploadToYouTubePublicToolStripMenuItem.Text = "Upload to YouTube (Public)";
            this.uploadToYouTubePublicToolStripMenuItem.Click += new System.EventHandler(this.uploadToYouTubePublicToolStripMenuItem_Click);
            // 
            // saveToToolStripMenuItem
            // 
            this.saveToToolStripMenuItem.Name = "saveToToolStripMenuItem";
            this.saveToToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.saveToToolStripMenuItem.Text = "Save to...";
            this.saveToToolStripMenuItem.Click += new System.EventHandler(this.saveToToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(810, 454);
            this.ContextMenuStrip = this.ctxtTaskbar;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStripMenu);
            this.Controls.Add(this.statusStrip1);
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(50, 50);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "iSpy";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.MainFormHelpButtonClicked);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainFormFormClosing1);
            this.Load += new System.EventHandler(this.MainFormLoad);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.Resize += new System.EventHandler(this.MainFormResize);
            this.ctxtMainForm.ResumeLayout(false);
            this.toolStripMenu.ResumeLayout(false);
            this.toolStripMenu.PerformLayout();
            this.ctxtMnu.ResumeLayout(false);
            this.ctxtTaskbar.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this._pnlContent.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.flCommands.ResumeLayout(false);
            this.flCommands.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ctxtPlayer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private void menuItem22_Click(object sender, EventArgs e)
        {
            Conf.LockLayout = !Conf.LockLayout;
            menuItem22.Checked = Conf.LockLayout;
        }

        private void iSpyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((PreviewBox) ContextTarget).PlayMedia(1);
        }

        private void defaultPlayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((PreviewBox)ContextTarget).PlayMedia(2);
        }

        private void websiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((PreviewBox)ContextTarget).PlayMedia(0);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var pb = ((PreviewBox) ContextTarget);
            RemovePreviewBox(pb);
        }

        private void pTZControllerToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ShowHidePTZTool();
        }

        private void llblRefresh_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LoadPreviews();
        }

        private void showInFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var pb = ((PreviewBox)ContextTarget);

            string argument = @"/select, " + pb.FileName;
            Process.Start("explorer.exe", argument);
        }

        private void otherVideoSourceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddCamera(4);
        }

        private void videoFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddCamera(2);
        }

        private void uploadToYouTubeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((PreviewBox)ContextTarget).Upload(false);
        }

        private void uploadToYouTubePublicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((PreviewBox)ContextTarget).Upload(true);
            
        }

        
        private void saveToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var pb = ((PreviewBox)ContextTarget);
                var fi = new FileInfo(pb.FileName);

                if (fbdSaveTo.ShowDialog(this) == DialogResult.OK)
                {
                    File.Copy(pb.FileName, fbdSaveTo.SelectedPath + @"\" + fi.Name);                
                }
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
            }
        }

        private void menuItem25_Click(object sender, EventArgs e)
        {
            
        }

        private void menuItem26_Click(object sender, EventArgs e)
        {
            ShowGrid("1x1");
        }      

        private void ShowGrid(string cfg)
        {
            var gv = new GridView(this,cfg);
            gv.Show();
        }

        private void menuItem27_Click(object sender, EventArgs e)
        {
            ShowGrid("2x2");
        }

        private void menuItem28_Click(object sender, EventArgs e)
        {
            ShowGrid("3x3");
        }

        private void menuItem29_Click(object sender, EventArgs e)
        {
            ShowGrid("4x4");
        }

        private void menuItem30_Click(object sender, EventArgs e)
        {
            ShowGrid("5x5");
        }

        private void menuItem31_Click(object sender, EventArgs e)
        {
            var gvc = new GridViewCustom();
            gvc.ShowDialog(this);
            if (gvc.DialogResult== DialogResult.OK)
            {
                ShowGrid(gvc.Cols+"x"+gvc.Rows);
            }
            gvc.Dispose();
        }


    }

    
    
}
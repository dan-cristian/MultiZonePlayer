using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Net;

namespace MzpMonitor {
	public class SysTrayApp : Form {
		[STAThread]
		public static void Main() {
			Application.Run(new SysTrayApp());
		}

		private NotifyIcon trayIcon;
		private ContextMenu trayMenu;
		public bool IsShuttingDown = false;
		public string MZPProcName = "MultiZonePlayer";
		public const String LOG_GENERAL_FILE = "\\MultiZonePlayerMonitor.log";
		private MyWebClient m_webclient = new MyWebClient();

		public SysTrayApp() {
			// Create a simple tray menu with only one item.
			trayMenu = new ContextMenu();
			trayMenu.MenuItems.Add("Exit", OnExit);
			trayMenu.MenuItems.Add("Status", OnStatus);

			// Create a tray icon. In this example we use a
			// standard system icon for simplicity, but you
			// can of course use your own custom icon too.
			trayIcon = new NotifyIcon();
			trayIcon.Text = "MyTrayApp";
			trayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);

			// Add menu to tray icon and show it.
			trayIcon.ContextMenu = trayMenu;
			trayIcon.Visible = true;

			Thread th = new Thread(() => RunCheck());
			th.Start();
		}

		protected override void OnLoad(EventArgs e) {
			Visible = false; // Hide form window.
			ShowInTaskbar = false; // Remove from taskbar.

			base.OnLoad(e);
		}

		private void OnExit(object sender, EventArgs e) {
			IsShuttingDown = true;
			Application.Exit();
		}
		private void OnStatus(object sender, EventArgs e) {
			Form1 form = new Form1();
			form.Show();
		}

		protected override void Dispose(bool isDisposing) {
			if (isDisposing) {
				// Release the icon resource.
				trayIcon.Dispose();
			}

			base.Dispose(isDisposing);
		}

		protected void RunCheck() {
			AppendToGenericLogFile("MZP Monitor started");
			do {
				if (!IsProcAlive(MZPProcName)) {
					AppendToGenericLogFile("MZP Monitor detected MZP not running, restarting");
					RestartMZP();
				}
				else {
					if (!URLGetOK()) {
						AppendToGenericLogFile("MZP Monitor detected MZP not answering to HTTP status, restarting");
						RestartMZP();
					}
				}
				Thread.Sleep(120000);
			}
			while (!IsShuttingDown);
			AppendToGenericLogFile("MZP Monitor closed");
		}
		public bool URLGetOK() {
			String data;
			try {
				data = m_webclient.DownloadString("http://localhost/cmd?command=status");
				if (data != null && data.Contains("Status at"))
					return true;
				else
					return false;
			}
			catch (Exception) { return false; }
		}

		private void RestartMZP() {
			AppendToGenericLogFile("Killing MZP proc");
			Process[] procList = Process.GetProcessesByName(MZPProcName);
			if (procList!=null && procList.Length>0)
				procList[0].Kill();
			System.Diagnostics.Process proc = RunProcessWait(CurrentPath() + "\\" + MZPProcName + ".exe",
						System.Diagnostics.ProcessWindowStyle.Normal, System.Diagnostics.ProcessPriorityClass.Normal);
			AppendToGenericLogFile("MZP proc restarted");
		}
		public static bool IsProcAlive(String procName)
        {
            Process[] proc;
            proc = Process.GetProcessesByName(procName);
            
            return (proc.Length != 0);
        }

		public static Process RunProcessWait(String command, ProcessWindowStyle style, ProcessPriorityClass priority) {
			Process extProc = null;
			String fileName;
			String arguments = "";
			int delimStart, delimEnd;

			delimStart = command.IndexOf("'");
			delimEnd = command.LastIndexOf("'");
			if (delimStart == -1) {
				fileName = command;
			}
			else {
				fileName = command.Substring(delimStart + 1, delimEnd - 1);
				arguments = command.Substring(delimEnd + 1, command.Length - delimEnd - 1);
			}

			Log("running proc " + command);
			if (!File.Exists(fileName)) {
				Log("Process File does not exist");
				return null;
			}
			// run external power resume actions
			extProc = new Process();
			extProc.StartInfo.FileName = fileName;
			extProc.StartInfo.Arguments = arguments;
			extProc.EnableRaisingEvents = true;
			//extProc.StartInfo.UseShellExecute = false;
			//extProc.StartInfo.RedirectStandardInput = true;
			//extProc.StartInfo.ErrorDialog = false;
			extProc.StartInfo.WindowStyle = style;
			extProc.Start();
			if (!extProc.HasExited)
				extProc.PriorityClass = priority;
#if DEBUG
			System.Threading.Thread.Sleep(100);
#else
             System.Threading.Thread.Sleep(30000);
#endif

			Log("running proc completed " + command);
			return extProc;
		}
		public static void AppendToGenericLogFile(String text) {
			try {
				StreamWriter str;
				str = File.AppendText(CurrentPath() + LOG_GENERAL_FILE);
				text = System.DateTime.Now.ToString("dd-MM HH:mm:ss-ff ") + text + "\n";
				str.Write(text);
				str.Close();
			}
			catch (Exception) { }
		}

		public static void Log(String text) {
				AppendToGenericLogFile("==MONITOR== "+text + "\n\n");
			}

		public static String CurrentPath()
            {
                //return Directory.GetCurrentDirectory();
                return Directory.GetParent(Application.ExecutablePath).FullName;
            }
		
	}

	public class MyWebClient : WebClient {
		public int Timeout = 10000;
		protected override WebRequest GetWebRequest(Uri uri) {
			WebRequest w = base.GetWebRequest(uri);
			w.Timeout = Timeout;
			return w;
		}
	}
}
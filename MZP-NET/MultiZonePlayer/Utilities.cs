﻿//#define DEBUG

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Speech.Synthesis;
using System.Speech.AudioFormat;
using System.Net;
using SoftCircuits.Parsing;
using System.Windows.Forms;
using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using System.Threading;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Web;
using System.Runtime.InteropServices.ComTypes;
using System.Drawing;

namespace MultiZonePlayer
{
	public static class MyExtensions
	{
		public static string ReplaceAt(this string input, int index, char newChar)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			char[] chars = input.ToCharArray();
			chars[index] = newChar;
			return new string(chars);
		}

		public static string ReplaceFirst(this string text, string search, string replace)
		{
			int pos = text.IndexOf(search);
			if (pos < 0)
			{
				return text;
			}
			return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
		}

		public static string Substring(this string text, string startDelimiter, string endDelimiter)
		{
			int startPos = text.IndexOf(startDelimiter);
			int endPos = text.IndexOf(endDelimiter, Math.Max(0, startPos));
			
			if (startPos >= 0 && endPos >= 0)
				return text.Substring(startPos + startDelimiter.Length, 
					endPos - (startPos + startDelimiter.Length));
			else
				return null;
		}

        /// <summary>
        /// Returns the substring at the end of the string with length = endLength 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="endCount"></param>
        /// <returns></returns>
        public static string SubstringEnd(this string text, int endLength) {
            return text.Substring(text.Length - endLength, endLength);
        }

		public static String[] SplitTwo(this string text, String separator)
		{
			String[] result;
			int index = text.IndexOf(separator);
			if (index >= 0)
			{
				result = new String[2];
				result[0] = text.Substring(0, index);
				result[1] = text.Substring(index + separator.Length);
				return result;
			}
			result = new string[1];
			result[0] = text;
			return result;
		}

		public static byte[] GetBytes(this string str)
		{
			byte[] bytes = new byte[str.Length * sizeof(char)];
			System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
			return bytes;
		}

		public static String Find(this string text, String findtext, String endDelimiter) {
			int start = text.IndexOf(findtext) + findtext.Length;
			int end = text.IndexOf(endDelimiter, start);
			if (end >= start)
				return text.Substring(start, end - start);
			else
				return "";
		}

		public static bool ArraysEqual<T>(this T[] a1, T[] a2) {
			if (ReferenceEquals(a1, a2))
				return true;

			if (a1 == null || a2 == null)
				return false;

			if (a1.Length != a2.Length)
				return false;

			EqualityComparer<T> comparer = EqualityComparer<T>.Default;
			for (int i = 0; i < a1.Length; i++) {
				if (!comparer.Equals(a1[i], a2[i])) return false;
			}
			return true;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text">contains parameters like @var=value </param>
        /// <param name="paramPrefix">e.g. @</param>
        /// <param name="parameters">has a list with name,value, so should be odd</param>
        /// <returns></returns>
        public static String ReplaceParams(this string text, String paramPrefix, params String[] parameters) {
            if (parameters.Length == 2* (int)parameters.Length/2){
                string name, value;
                int start;
                for (int i = 0; i < parameters.Length; i=i+2) {
                    name = parameters[i];
                    value = parameters[i+1];
                    start = text.IndexOf(paramPrefix + name);
                    if (start >= 0) {
                        text = text.Replace(paramPrefix + name, value);
                    }
                }
                return text;
            }
            else {
                Alert.CreateAlert("Odd number of parameters expected for text=" + text, true);
                return text;
            }
        }
	}

	public class MyWebClient : WebClient {
		public int Timeout = 500;
		protected override WebRequest GetWebRequest(Uri uri) {
			WebRequest w = base.GetWebRequest(uri);
			w.Timeout = Timeout;
			return w;
		}
	}

    public class Utilities
    {
        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(
            IntPtr hWnd // handle to window
            );
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(
            string lpClassName, // class name 
            string lpWindowName // window name 
        );

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern IntPtr GetForegroundWindow();
		
		[DllImport("user32.dll")]
		static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(
            int hWnd, // handle to destination window 
            uint Msg, // message 
            int wParam, // first message parameter 
            int lParam // second message parameter 
        );

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
        internal static extern void MoveWindow(IntPtr hwnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

		/// <summary>
		/// SetWindowPos Flags
		/// </summary>
		public static class SWP
		{
			public static readonly int
			NOSIZE = 0x0001,
			NOMOVE = 0x0002,
			NOZORDER = 0x0004,
			NOREDRAW = 0x0008,
			NOACTIVATE = 0x0010,
			DRAWFRAME = 0x0020,
			FRAMECHANGED = 0x0020,
			SHOWWINDOW = 0x0040,
			HIDEWINDOW = 0x0080,
			NOCOPYBITS = 0x0100,
			NOOWNERZORDER = 0x0200,
			NOREPOSITION = 0x0200,
			NOSENDCHANGING = 0x0400,
			DEFERERASE = 0x2000,
			ASYNCWINDOWPOS = 0x4000;
		}

		[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
		public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("user32.dll")]
        static public extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.Dll", EntryPoint = "PostMessageA")]
        public static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

        [DllImport("User32.Dll", EntryPoint = "PostMessageA")]
        public static extern bool PostMessageInt(int hWnd, uint msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern byte VkKeyScan(char ch);
 

        [DllImport("kernel32")]
        public static extern long WritePrivateProfileString(string section,
            string key, string val, string filePath);
        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string section,
                 string key, string def, StringBuilder retVal,
            int size, string filePath);

        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_SYSTEM_REQUIRED = 0x00000001,
            ES_DISPLAY_REQUIRED = 0x00000002,
            // Legacy flag, should not be used.
            // ES_USER_PRESENT   = 0x00000004,
            ES_CONTINUOUS = 0x80000000,
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        [DllImport("kernel32.dll")]
        public static extern SafeWaitHandle CreateWaitableTimer(IntPtr lpTimerAttributes, bool bManualReset, string lpTimerName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWaitableTimer(SafeWaitHandle hTimer, [In] ref long pDueTime, int lPeriod, IntPtr pfnCompletionRoutine, IntPtr lpArgToCompletionRoutine, bool fResume);

		
        

        [DllImport("winmm.dll")]
        public static extern int waveOutGetVolume(IntPtr hwo, out uint dwVolume);

        [DllImport("winmm.dll")]
        public static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

        public const int WM_SETFOCUS = 0x0007;
        public const int WM_KEYDOWN = 0x100;
        public const int WM_KEYUP = 0x101;
        public const int WM_CHAR = 0x105;
        public const int WM_SYSKEYDOWN = 0x104;
        public const int WM_SYSKEYUP = 0x105;
        public const int WM_LBUTTONUP = 0x0202;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONDBLCLK = 0x0203;
        public const int VK_PRIOR = 0x21;
        public const int VK_F5 = 0x74;
        public const int VK_F6 = 0x75;
        public const int VK_RETURN = 0x0D;
        public const int VK_TAB = 0x09;

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern short GetKeyState(int keyCode);
		

        private static Process extProc = null;

        public static IntPtr MakeLParam(int LoWord, int HiWord)
        {
            return (IntPtr)((HiWord << 16) | (LoWord & 0xffff));
        }

	   

        public static bool CloseProcSync(String procName)
        {
            MLog.Log(null,"Closing proc " + procName);

            Process[] proc;
            proc = Process.GetProcessesByName(procName);

            try
            {
                for (int i = 0; i < proc.Length; i++)
                {
                    if (!proc[i].CloseMainWindow())//try a nice close
                        proc[i].Kill();//kill if nice close do not work

                    if (!proc[i].WaitForExit(10000))
                        proc[i].Kill();//force kill if not exited yet via nice close

                    MLog.Log(null,"Closed proc " + proc[i].ProcessName);
                }
            }
            catch (Exception ex)
            {
                MLog.Log(null,"Exception closing proc=" + procName + ex.Message);
            }
            return (proc.Length != 0);
        }

        public static bool IsProcAlive(String procName)
        {
            Process[] proc;
            proc = Process.GetProcessesByName(procName);
            
            return (proc.Length != 0);
        }

        public static Process GetProcess(String procName) {
            Process[] proc;
            proc = Process.GetProcessesByName(procName);
            if (proc.Length > 0)
                return proc[0];
            else
                return null;
        }

        public static bool SetProcessPriority(String procName, ProcessPriorityClass priority) {
            System.Diagnostics.Process[] procList;
            procList = System.Diagnostics.Process.GetProcessesByName(procName);
            if (procList.Length > 0) {
                procList[0].PriorityClass = priority;
                return true;
            }
            else
                MLog.Log("Cannot locate process for setting priority, proc=" + procName);
            return false;
        }
        public static Process RunProcessWait(String command, ProcessWindowStyle style, ProcessPriorityClass priority)
        {
            String fileName;
            String arguments="";
            int delimStart, delimEnd;

            delimStart = command.IndexOf("'");
            delimEnd = command.LastIndexOf("'");
            if (delimStart == -1)
            {
                fileName = command;
            }
            else
            {
                fileName = command.Substring(delimStart + 1, delimEnd - 1);
                arguments = command.Substring(delimEnd + 1, command.Length - delimEnd - 1);
            }

            MLog.Log(null,"running proc " + command);
            if (!File.Exists(fileName))
            {
                MLog.Log(null,"Process File does not exist");
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

            MLog.Log(null,"running proc completed " + command);
            return extProc;
        }

		[DllImport("WININET", CharSet = CharSet.Auto)]
		public static extern bool InternetGetConnectedState(ref InternetConnectionState lpdwFlags, int dwReserved);
		//InternetConnectionState flags = 0;
		public enum InternetConnectionState : int {
			INTERNET_CONNECTION_MODEM = 0x1,
			INTERNET_CONNECTION_LAN = 0x2,
			INTERNET_CONNECTION_PROXY = 0x4,
			INTERNET_RAS_INSTALLED = 0x10,
			INTERNET_CONNECTION_OFFLINE = 0x20,
			INTERNET_CONNECTION_CONFIGURED = 0x40
		}

        public static void SetWaitForWakeUpTime(DateTime date)
        {
            long duetime = date.ToFileTime();

            using (SafeWaitHandle handle = CreateWaitableTimer(IntPtr.Zero, true, "MyWaitabletimer"))
            {
                if (SetWaitableTimer(handle, ref duetime, 0, IntPtr.Zero, IntPtr.Zero, true))
                {
                    using (EventWaitHandle wh = new EventWaitHandle(false, EventResetMode.AutoReset))
                    {
                        wh.SafeWaitHandle = handle;
                        //wh.WaitOne();
                    }
                }
                else
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }

            // You could make it a recursive call here, setting it to 1 hours time or similar        
        }

        /// <summary>
        /// Read Data Value From the Ini File
        /// </summary>
        /// <PARAM name="Section"></PARAM>
        /// <PARAM name="Key"></PARAM>
        /// <PARAM name="Path"></PARAM>
        /// <returns></returns>
        public static string IniReadValue(string Section, string Key, String path)
        {
            if (path == null)
            {
                MLog.Log(null,"null ini");
            }

            StringBuilder temp = new StringBuilder(16384);
			int i = Utilities.GetPrivateProfileString(Section, Key, "", temp, 16384, path);
            return temp.ToString();
        }

        private static Object m_csvLock= new Object();
		public static void AppendToCsvFile(string filename, string separator, params string[] values)
		{
            lock (m_csvLock) {
                StreamWriter logStr = null;
                try {
                    logStr = File.AppendText(IniFile.CurrentPath() + filename);

                    foreach (string val in values) {
                        logStr.Write(val + separator);
                    }
                    logStr.Write("\r\n");
                    logStr.Close();
                }
                catch (Exception ex) {
                    if (logStr != null)
                        logStr.Close();
                    MLog.Log(ex, "error append to csv file " + filename);
                }
            }
		}

        public static void AppendToGenericLogFile(String text, EventSource logType){
			try {
				StreamWriter str;
				switch (logType) {
					case EventSource.System:
						str = File.AppendText(IniFile.CurrentPath() + IniFile.LOG_GENERAL_FILE);
						break;
					case EventSource.SystemDropped:
						str = File.AppendText(IniFile.CurrentPath() + "dropped." + IniFile.LOG_GENERAL_FILE);
						break;
					case EventSource.Modem:
						str = File.AppendText(IniFile.CurrentPath() + IniFile.LOG_MODEM_FILE);
						break;
					case EventSource.Web:
						str = File.AppendText(IniFile.CurrentPath() + IniFile.LOG_WEB_FILE);
						break;
					case EventSource.Keyboard:
						str = File.AppendText(IniFile.CurrentPath() + IniFile.LOG_KEY_FILE);
						break;
					case EventSource.RawInput:
						str = File.AppendText(IniFile.CurrentPath() + IniFile.LOG_RAWINPUT_FILE);
						break;
					default:
						str = File.AppendText(IniFile.CurrentPath() + IniFile.LOG_EVENTS_FILE);
						break;
				}
				str.Write(text);
				str.Close();
			}
			catch (Exception) { }
        }
        public static void AppendToInfoLogFile(String text) {
            try {
                StreamWriter str;
                str = File.AppendText(IniFile.CurrentPath() + IniFile.LOG_INFO_FILE);
                str.Write(text);
                str.Close();
            }
            catch (Exception) { }
        }
        public static void AppendToSqlLogFile(String text) {
            try {
                StreamWriter str;
                str = File.AppendText(IniFile.CurrentPath() + IniFile.LOG_SQL_FILE);
                str.Write(text);
                str.Close();
            }
            catch (Exception) { }
        }
		public static bool ExistFileRelativeToAppPath(String fileName) {
			return File.Exists(IniFile.CurrentPath() + fileName);
		}
        public static String ReadFileRelativeToAppPath(String fileName)
        {
            StreamReader str = File.OpenText(IniFile.CurrentPath() + fileName);
            String result = str.ReadToEnd();
            str.Close();
            return result;
        }
		
		public static void DeleteFilesOlderThan(String directory, String filePattern, int days){
			var files = new DirectoryInfo(directory).GetFiles(filePattern);
			foreach (var file in files)
			{
				if (DateTime.UtcNow - file.CreationTimeUtc > TimeSpan.FromDays(days))
				{
					File.Delete(file.FullName);
				}
			}
		}


		public static bool DownloadRemoteImageFile(string uri, string fileName) {
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
			HttpWebResponse response;
			try {
				response = (HttpWebResponse)request.GetResponse();
			}
			catch (Exception) {
				return false;
			}

			// Check that the remote file was found. The ContentType
			// check is performed since a request for a non-existent
			// image file might be redirected to a 404-page, which would
			// yield the StatusCode "OK", even though the image was not
			// found.
			if ((response.StatusCode == HttpStatusCode.OK ||
				response.StatusCode == HttpStatusCode.Moved ||
				response.StatusCode == HttpStatusCode.Redirect) &&
				response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase)) {

				// if the remote file was found, download it
				using (Stream inputStream = response.GetResponseStream())
				using (Stream outputStream = File.OpenWrite(fileName)) {
					byte[] buffer = new byte[4096];
					int bytesRead;
					do {
						bytesRead = inputStream.Read(buffer, 0, buffer.Length);
						outputStream.Write(buffer, 0, bytesRead);
					} while (bytesRead != 0);
				}
				return true;
			}
			else
				return false;
		}

		public static String GenerateThumb(string sourceFilePath, string thumbExtension, int maxWidth) {
			Image thumb, image = Image.FromFile(sourceFilePath);
			Bitmap srcBmp = new Bitmap(sourceFilePath);
			float ratio = 1f * srcBmp.Width / srcBmp.Height;
			thumb = image.GetThumbnailImage(maxWidth, Convert.ToInt16(maxWidth/ratio), () => false, IntPtr.Zero);
			string thumbPath = Path.ChangeExtension(sourceFilePath, thumbExtension);
			thumb.Save(thumbPath);
			return thumbPath;
		}

		public static String ReadFile(String filePath)
		{
			StreamReader str = File.OpenText(filePath);
			String result = str.ReadToEnd();
			str.Close();
			return result;
		}

		public static String ReadFileRelToAppPath(String fileName) {
			StreamReader str = File.OpenText(IniFile.CurrentPath() + fileName);
			String result = str.ReadToEnd();
			str.Close();
			return result;
		}

        public static byte[] ReadBinaryFileRelativeToAppPath(String fileName)
        {
            FileStream fs = File.OpenRead(IniFile.CurrentPath() + fileName);
            byte[] data = new byte[fs.Length];
            fs.Read(data, 0, data.Length);
            fs.Close();
            return data;
        }

		public static byte[] ReadBinaryFile(String filePath)
		{
			FileStream fs = File.OpenRead(filePath);
			byte[] data = new byte[fs.Length];
			fs.Read(data, 0, data.Length);
			fs.Close();
			return data;
		}

        public static void WriteBinaryFile(String fileName, byte[] data)
        {
            try
            {
                FileStream fs = File.Create(IniFile.CurrentPath() + fileName);
                fs.Write(data, 0, data.Length);
                fs.Close();
            }
            catch (Exception ex)
            {
                MLog.Log(ex, "Unable to write file=" + fileName);
            }
        }

		public static void WriteTextFileRelToAppPath(String fileName, String data)
		{
			try
			{
				File.WriteAllText(IniFile.CurrentPath() + fileName, data);
				/*FileStream fs = File.Create(IniFile.CurrentPath() + fileName);
				fs.Write(data.GetBytes(), 0, data.Length);
				fs.Close();*/
			}
			catch (Exception ex)
			{
				MLog.Log(ex, "Unable to write text file=" + fileName);
			}
		}

        public static void MoveFile(String sourceFullPath, String destinationFullPath, bool overrideDestination)
        {
            try
            {
                MLog.Log(null,"Moving file " + sourceFullPath + " to " + destinationFullPath);
                
                if (overrideDestination && File.Exists(destinationFullPath))
                {
                    File.Delete(destinationFullPath);
                }

                File.Move(sourceFullPath, destinationFullPath);
            }
            catch (Exception ex)
            {
                MLog.Log(ex,"Cannot move file " + sourceFullPath + " to " + destinationFullPath + " err=" + ex.Message);
            }
        }

        public static FileInfo GetFileInfo(string filepath)
        {
            FileInfo fi = new FileInfo(filepath);
            return fi;
        }

        public static bool CanConvertToInt(String value, out int result)
        {
            try
            {
                result = Convert.ToInt32(value);
                return true;
            }
            catch (Exception)
            {
                result = 0;
                return false;
            }
        }

		public static byte ReverseBitsWith4Operations(byte b)
		{
			return (byte)(((b * 0x80200802ul) & 0x0884422110ul) * 0x0101010101ul >> 32);
		}
		public static byte ReverseBitsWith3Operations(byte b)
		{
			return (byte)((b * 0x0202020202ul & 0x010884422010ul) % 1023);
		}
		public static byte ReverseBitsWith7Operations(byte b)
		{
			return (byte)(((b * 0x0802u & 0x22110u) | (b * 0x8020u & 0x88440u)) * 0x10101u >> 16);
		}

		public static string GetActiveWindowTitle()
		{
			const int nChars = 256;
			IntPtr handle = IntPtr.Zero;
			StringBuilder Buff = new StringBuilder(nChars);
			handle = GetForegroundWindow();

			if (GetWindowText(handle, Buff, nChars) > 0)
			{
				return Buff.ToString();
			}
			return null;
		}

        // C# to convert a string to a byte array.
        public static byte[] StrToByteArray(string str)
        {
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            return encoding.GetBytes(str);
        }

        public static String ToTitleCase(string title)
        {
            if (title != null)
            {
                CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
                TextInfo textInfo = cultureInfo.TextInfo;
                return textInfo.ToTitleCase(title.ToLower())??"";
            }
            else return null;
        }

        public static String SanitiseInternationalTrimUpper(String text)
        {
			if (text == null) return null;
			String charsource = "ăşîâĂÎŞȚéÖöëü&�Ã¡Å";
            String chardest =   "asiiAISTeOoeu*eAlR";

            StringBuilder sb = new StringBuilder(Utilities.ToTitleCase(text));

            if ((text != null) && (text != ""))
            {
                for (int i = 0; i < charsource.Length; i++)
                {
                    sb = sb.Replace(charsource[i], chardest[i]);
                }
            }
            
            return sb.ToString().Trim();
        }
		#region HexToByte
		/// <summary>
		/// method to convert hex string into a byte array
		/// </summary>
		/// <param name="msg">string to convert</param>
		/// <returns>a byte array</returns>
		public static byte[] HexToByte(string msg)
		{
			//remove any spaces from the string
			msg = msg.Replace(" ", "");
			//create a byte array the length of the
			//divided by 2 (Hex is 2 characters in length)
			byte[] comBuffer = new byte[msg.Length / 2];
			//loop through the length of the provided string
			for (int i = 0; i < msg.Length; i += 2)
				//convert each set of 2 characters to a byte
				//and add to the array
				comBuffer[i / 2] = (byte)Convert.ToByte(msg.Substring(i, 2), 16);
			//return the array
			return comBuffer;
		}
		#endregion

		#region ByteToHex
		/// <summary>
		/// method to convert a byte array into a hex string
		/// </summary>
		/// <param name="comByte">byte array to convert</param>
		/// <returns>a hex string</returns>
		public static string ByteToHex(byte[] comByte)
		{
			//create a new StringBuilder object
			StringBuilder builder = new StringBuilder(comByte.Length * 2);//3);
			//loop through each byte in the array
			foreach (byte data in comByte)
				//convert the byte to a string and add to the stringbuilder
				builder.Append(Convert.ToString(data, 16).PadLeft(2, '0'));//.PadRight(3, ' '));
			//return the converted value
			return builder.ToString().ToUpper();
		}
		#endregion

        public static List<Object> ParseStringForValues(String parseValue, char delimitator, Type convertToType)
        {
            int lastIndex;
            String value;
            List<Object> result = new List<Object>();
                
            lastIndex = 0;
            for (int i = 0; i < parseValue.Length; i++)
            {
                value ="";
                if ((parseValue[i] == delimitator))
                {
                    value = parseValue.Substring(lastIndex, i - lastIndex );
                }

                if (i == parseValue.Length - 1)
                {
                    value = parseValue.Substring(lastIndex, i - lastIndex +1);
                }

                if (value !="")
                {
                    if (convertToType == typeof(int))
                        result.Add(Convert.ToInt32(value));
                    if (convertToType == typeof(String))
                        result.Add(value);
                    lastIndex = i + 1;
                }
            }

            return result;
        }

        //key1=value1;key2=value2
        public static Hashtable ParseStringForKeyAndValue(String parseValue, char delimitator)
        {
            List<Object> values = ParseStringForValues(parseValue, delimitator, typeof(String));
            Hashtable result = new Hashtable();
            String pair;
            String key, value;
            int index;
            for (int i = 0; i < values.Count; i++)
            {
                pair=values[i] as String;
                index = pair.IndexOf("=");
                if (index > 0)
                {
                    key = pair.Substring(0, index).ToLower();
                    value = pair.Substring(index + 1).ToLower();
                    result.Add(key, value);
                }
            }

            return result;
        }

        /// <summary>
        /// Receives string and returns the string with its letters reversed.
        /// </summary>
        public static string ReverseString(string s)
        {
            char[] arr = s.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }

        /*
        ** Method 2 (using string modifiers)
        */
        public static string ExtractDomainNameFromURL(string Url)
        {
            if (Url.Contains(@"://"))
                Url = Url.Split(new string[] { "://" }, 2, StringSplitOptions.None)[1];

            return Url.Split('/')[0];
        }

        public static string ExtractServerNameFromURL(string Url)
        {
            if (Url.Contains(@"://"))
                Url = Url.Split(new string[] { "://" }, 2, StringSplitOptions.None)[1];

            Url= Url.Split('/')[0];
            return Url.Split(':')[0];
        }

		public static String DurationAsTimeSpan(TimeSpan span)
		{
			String result;
			if (DateTime.Now.Subtract(DateTime.MinValue).Days == span.Days)
				result = "N/A";
			else
			{
				result = span.ToString(IniFile.PARAM_TIMESPAN_FORMAT[1]);
				if (result.Substring(0, 2) == "0d")
					result = result.Replace("0d", "");
				if (result.Substring(0,2)=="0h")
					result = result.Replace("0h", "");
				if (result.Substring(0, 2) == "0m")
					result = result.Replace("0m", "");
			}
			return result;
		}


        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }


		public static string FindFTDIComPortFromDesc(string description, bool returnCOM) {
			FTD2XX_NET.FTDI ftdiDevice = new FTD2XX_NET.FTDI();
			uint count = 0;
			ftdiDevice.GetNumberOfDevices(ref count);
			FTD2XX_NET.FTDI.FT_DEVICE_INFO_NODE[] deviceNode = new FTD2XX_NET.FTDI.FT_DEVICE_INFO_NODE[count];
			ftdiDevice.GetDeviceList(deviceNode);
			MLog.Log(null, "Found FTDI devices count=" + count);

			FTD2XX_NET.FTDI.FT_STATUS status;
			string serial, com;
			//FTD2XX_NET.FTDI ftdiDevice = new FTD2XX_NET.FTDI();

			for (int i = 0; i < count; i++) {
				MLog.Log(null,
					"FTDI desc=" + deviceNode[i].Description
					+ " id=" + deviceNode[i].ID
					+ " serial=" + deviceNode[i].SerialNumber
					+ " type=" + deviceNode[i].Type
					+ " locid=" + deviceNode[i].LocId
					);
				serial = deviceNode[i].SerialNumber;

				if (returnCOM) {
					try {
						status = ftdiDevice.OpenBySerialNumber(serial);
						if (status == FTD2XX_NET.FTDI.FT_STATUS.FT_OK) {
							ftdiDevice.GetCOMPort(out com);
							MLog.Log(null, "FTDI SensorDevice opened ok, COM=" + com);

							if (deviceNode[i].Description.ToLower().Equals(description)) {
								MLog.Log(null, "Found FTDI device " + description + " at com port " + com);
								ftdiDevice.Close();
								return com;
							}
						}
						else
							MLog.Log(null, "Error, unable to open device " + serial);
					}
					catch (Exception ex) {
						MLog.Log(ex, "Error opening ftdi device " + serial);
					}
					finally {
						if (ftdiDevice != null)
							ftdiDevice.Close();
					}
				}

				if (deviceNode[i].Description.ToLower().Equals(description)) {
					MLog.Log(null, "Found FTDI device " + description + " at port " + serial);
					return serial;
				}
			}

			MLog.Log("No port for " + description + " found in " + count + " devices");
			return "";
		}
    }

    public class WebPostRequest
    {
        WebRequest theRequest;
        HttpWebResponse theResponse;
        ArrayList theQueryData;
		String ContentType;

        public WebPostRequest(string url, string contenttype)
        {
            MLog.LogWeb("WebPostRequest " + url);
            theRequest = WebRequest.Create(url);
			theRequest.Timeout = 15000;
            theRequest.Method = "POST";
            theQueryData = new ArrayList();
			ContentType = contenttype;
        }

        public void Add(string key, string value)
        {
            theQueryData.Add(String.Format("{0}={1}", key, HttpUtility.UrlEncode(value)));
        }

        public string GetResponse()
        {
            // Set the encoding type
			if (ContentType == "" || ContentType==null)
				ContentType = "application/x-www-form-urlencoded";
			
			theRequest.ContentType = ContentType;// "application/x-www-form-urlencoded";
            theRequest.Timeout = 10000;

            // Build a string containing all the parameters
            string Parameters = String.Join("&", (String[])theQueryData.ToArray(typeof(string)));
            theRequest.ContentLength = Parameters.Length;
            MLog.LogWeb(theRequest);
            MLog.LogWeb(Parameters);
            // We write the parameters into the request
            StreamWriter sw = new StreamWriter(theRequest.GetRequestStream());
            sw.Write(Parameters);
            sw.Close();

            // Execute the query
            theResponse = (HttpWebResponse)theRequest.GetResponse();
            MLog.LogWeb(theResponse);
            StreamReader sr = new StreamReader(theResponse.GetResponseStream());
            String response = sr.ReadToEnd();
            MLog.LogWeb(response);
            return response;
        }
    }

    public enum VoicePriority
    {
        Low,
        Normal,
        High
    };

    class Text2Speech
    {
        public static void PlayError(String text, ZoneGeneric zone)
        {
            PlayMessageClean(text, zone);
        }

        public static void PlayMessage2(String text, ZoneGeneric zone)
        {

            try
            {

                ZoneAux aux = null;
                if (zone != null)
                {
                    aux = new ZoneAux(zone, (String)MZPState.Instance.zoneDefaultInputs["0"], 
						zone.GetClonedZones()[0].OutputDeviceAutoCompleted());
                    aux.SetVolumeLevel(zone.GetVolumeLevel());
                    aux.Play();
                }
                SpeechSynthesizer synth = new SpeechSynthesizer();
                //SpeechAudioFormatInfo formatInfo = new SpeechAudioFormatInfo(8000, AudioBitsPerSample.Sixteen, AudioChannel.Mono);
                //synth.SetOutputToAudioStream(,formatInfo);
                synth.Speak(text);
                synth.Dispose();
                if (aux != null) aux.Close();
            }
            catch (Exception ex)
            {
                MLog.Log(null,"error t2s playerr: " + ex.Message + ex.StackTrace);
            }
        }

        public static void PlayMessage(String text, ZoneGeneric zone)
        {
            try
            {
                if (zone != null)
                {
                    FileStream stream = new FileStream(IniFile.CurrentPath() + "\\zone"+zone.GetZoneId()+DateTime.Now.Ticks+IniFile.TEMP_EXTENSION, FileMode.Create);
                    SpeechSynthesizer synth = new SpeechSynthesizer();
                    synth.SetOutputToWaveStream(stream);
                    synth.Speak(text);
                    synth.Dispose();
                    stream.Flush();
                    stream.Close();
                    MusicActivity music = new MusicActivity(zone, stream.Name);
                }
            }
            catch (Exception ex)
            {
                MLog.Log(null, "error t2s playerr: " + ex.Message + ex.StackTrace);
            }
        }

        public static void PlayMessageClean(String text, ZoneGeneric zone)
        {
            PlayMessage(CleanMessage(text), zone);
        }

        private static String CleanMessage(String message)
        {
            String remove = "_!@#$%^&*(){}_-=+1234567890\\|,./<>?`~;':|";
            for (int i = 0; i < remove.Length; i++)
            {
                message = message.Replace(remove[i].ToString(), "");
            }
            return message;
        }

        
        
    }

    public class Hyena
    {
        public class Log
        {
            public static void DebugFormat(String s, params Object[] param)
            {
                //MLog.Log(param, s);
            }
            public static void Warning(String s)
            {
                MLog.Log(null, s);
            }
            public static void WarningFormat(String s,params Object[] param)
            {
                MLog.Log(param, s);
            }
        }
    }

    public class MLog
    {
		private static HashSet<String> m_keywords = new HashSet<String>();

		public static void LoadFromIni()
		{
			m_keywords.Clear();
			string inientry;
			string[] keywords;
			int line = 0;
			do {
				inientry = Utilities.IniReadValue(IniFile.SCHEDULER_SECTION_LOG,
					line.ToString(), IniFile.CurrentPath() + IniFile.SCHEDULER_FILE);
				if (inientry != "") {
					keywords = inientry.Split(',');
					foreach (string key in keywords)
						m_keywords.Add(key.ToLower());
				}
				line++;
			}
			while (inientry != "");
			MLog.Log(null, "Loaded " + (line - 1) + " log definition lines");
		}

		public static void Log(String text) {
			Log(null, text);
		}

        public static void Log(Object o, CommandResult res)
        {
            Log(o, res.ErrorMessage);
        }

        public static void Log(Exception e, Object o, String text)
        {
			string callingMethod = "";
			try {
				var stack = new System.Diagnostics.StackTrace(1, false);
				if (stack != null) {
					for (int i = 0; i < Math.Min(7, stack.GetFrames().Length); i++)
						callingMethod += stack.GetFrame(i).GetMethod().Name + " | ";
					if (o == null)
						o = "null";
					if (e != null)
						Log(e, e.StackTrace + "|" + text + "method: " + callingMethod + " sender:" + o.ToString());
					else
						Log(e, text + " sender:" + o.ToString(), stack);
				}
			}
			catch (Exception ex) {
				Utilities.AppendToGenericLogFile(ex.Message + ex.StackTrace + ex.Source + "\n", EventSource.System);
			}
        }

        public static void Log(Object e, String text, params StackTrace[] stackList)
        {
            try
            {
				String callingMethod="";
				StackTrace stack;

				if (stackList != null && stackList.Length > 0) 
					stack = stackList[1];
				else
					stack = new System.Diagnostics.StackTrace(1, false);
				if (stack.GetFrames() != null) {
					for (int i = 0; i < Math.Min(7, stack.GetFrames().Length); i++)
						callingMethod += stack.GetFrame(i).GetMethod().Name + " << ";
					if (e != null && stack.GetFrames().Length>0) {
						if (e.GetType().ToString().ToLower().Contains("exception"))
							text += " err=" + ((Exception)e).Message + " method: "
								+ callingMethod
								+ " errline=" + stack.GetFrame(0).GetFileLineNumber()
								+ " errcol=" + stack.GetFrame(0).GetFileColumnNumber()
								+ " stack=" + ((Exception)e).StackTrace.Replace('\n',' ');
						else
							text += " Method (" + callingMethod + ") [from " + e.ToString()+"]";
					}
				}
            }
            catch (Exception ex) {
				Utilities.AppendToGenericLogFile("1-" + ex.Message + ex.StackTrace + ex.Source + "\n", EventSource.System); 
			}
            try
            {
				if (
					(m_keywords==null || m_keywords.Contains("all"))||
					(m_keywords!=null && e!= null && text != null && e.GetType() != null 
					&& Thread.CurrentThread != null && Thread.CurrentThread.Name != null &&
					((m_keywords.Contains(e.ToString().ToLower()))//sender
					|| m_keywords.Contains(Thread.CurrentThread.Name.ToLower())//thread
					|| (e.GetType().ToString().ToLower().Contains("exception")) 
					|| text.ToLower().Contains("error"))))//any error
				{
					Utilities.AppendToGenericLogFile(System.DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT_FOR_LOGS+ " [")
						+ Thread.CurrentThread.Name + "]:" + text + "\n", EventSource.System);
				}
				else
					Utilities.AppendToGenericLogFile("DROPPED: "+System.DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT_FOR_LOGS+" [")
						+ Thread.CurrentThread.Name + "]:" + text + "\n", EventSource.SystemDropped);
            }
            catch (Exception ex){
				Utilities.AppendToGenericLogFile("2-"+ex.Message + ex.StackTrace + ex.Source + "\n", EventSource.System);
			}
        }

        public static void LogEvent(MZPEvent mzpevent)
        {
            try
            {
                String message = mzpevent.DateTime + " | " + mzpevent.Source + " | " + mzpevent.Message + " | " + mzpevent.TypeEv + " | "+ mzpevent.Importance;
                Utilities.AppendToGenericLogFile(message + "\r\n", mzpevent.Source);
            }
            catch (Exception)
            { }
        }


        public static void LogModem(String text)
        {
            try
            {
            Utilities.AppendToGenericLogFile(text, EventSource.Modem);
            }
            catch (Exception)
            { }
        }

		//private Boolean m_userConfirm = false;
        public static void LogInfo(String text) {
            try {
                Utilities.AppendToInfoLogFile(System.DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT_FOR_LOGS)+" "+text+"\r\n");
            }
            catch (Exception) { }
        }
        public static void LogSql(String text) {
            try {
                Utilities.AppendToSqlLogFile(System.DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT_FOR_LOGS) + " " + text + "\r\n");
            }
            catch (Exception) { }
        }


        public static void LogWeb(HttpListenerRequest request){
            try{
                Utilities.AppendToGenericLogFile(String.Format("{0} {1} {2} {3} \r\n", DateTime.Now.ToString(),
                    request.RemoteEndPoint.Address, request.Url.AbsoluteUri, request.Headers.ToString().Replace('\n',' ').Replace('\r',' ')), 
                    EventSource.Web);
                Utilities.AppendToGenericLogFile("-------------------------------------------------------------------\r\n", EventSource.Web);
            }
            catch (Exception)
            { }
        }

        public static void LogWeb(WebRequest request)
        {
            try
            {
                
                Utilities.AppendToGenericLogFile(String.Format("{0} {1} {2} \r", DateTime.Now.ToString(),
					request.RequestUri.AbsoluteUri, request.Headers.ToString().Replace('\n', ' ').Replace('\r', ' ')),
                    EventSource.Web);
            }
            catch (Exception)
            { }
        }

        public static void LogWeb(HttpWebResponse response)
        {
            try
            {
                Utilities.AppendToGenericLogFile(String.Format("{0} {1} {2} \r", DateTime.Now.ToString(),
					response.ResponseUri.AbsoluteUri, response.Headers.ToString().Replace('\n', ' ').Replace('\r', ' ')),
                    EventSource.Web);
            }
            catch (Exception)
            { }
        }

        public static void LogWeb(String streamResponse)
        {
            try
            {
                Utilities.AppendToGenericLogFile(String.Format("{0} {1} \r", DateTime.Now.ToString(),
                    streamResponse), EventSource.Web);
            }
            catch (Exception)
            { }
        }

        public static void LogKey(String key)
        {
            try
            {
                Utilities.AppendToGenericLogFile(String.Format("{0} {1} \r\n", DateTime.Now.ToString("dd-MM hh:mm:ss-ff"), key), 
                    EventSource.Keyboard);
            }
            catch (Exception)
            { }
        }

        public static void LogRawInput(String key)
        {
            try
            {
                Utilities.AppendToGenericLogFile(key + "\r\n", EventSource.RawInput);
            }
            catch (Exception)
            { }
        }

		public static void Assert(Boolean condition, String message) {
			if (condition == false) {
				var callingMethod = new System.Diagnostics.StackTrace(1, false);
				Log(null, "Error, ASSERTION Failed: " + message, callingMethod);
			}
		}
    }

    public class IMDBParser
    {
        public static VideoItem GetVideoInfo(String imdbId, bool showPages, WebBrowser webBrowser)
        {
            String html;
            MLog.Log(null, "Loading page " + IniFile.VIDEO_INFO_IMDB_URL + imdbId);
            if (webBrowser == null) showPages = false;
            // Download page
            if (showPages)
            {
                int maxwait = 100;
                webBrowser.Navigate(IniFile.VIDEO_INFO_IMDB_URL + imdbId);

                do
                {
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(200);
                    maxwait--;
                    //Application.DoEvents();
                } while ((webBrowser.ReadyState != WebBrowserReadyState.Complete) && (maxwait > 0));

                if (maxwait > 0)
                    html = webBrowser.DocumentText;
                else
                {
                    html = "";
                    MLog.Log(null, "Unable to fully load page");
                }
            }
            else
            {
                WebClient client = new WebClient();
                html = client.DownloadString(IniFile.VIDEO_INFO_IMDB_URL + imdbId);
            }
            MLog.Log(null, "Loading done " + IniFile.VIDEO_INFO_IMDB_URL + imdbId);

            VideoItem videoInfo = new VideoItem();

            MLog.Log(null, "Parsing page " + IniFile.VIDEO_INFO_IMDB_URL + imdbId);
            videoInfo.ImdbId = imdbId;
            videoInfo.ImdbTitle = GetTagValue(html, "meta", "property", "og:title", "content", false);
            videoInfo.ImdbImageURL = GetTagValue(html, "meta", "property", "og:image", "content", false);
            videoInfo.ImdbDescription = GetTagValue(html, "meta", "name", "description", "content", false);
            videoInfo.ImdbYear = GetTagValue(html, "time", "itemprop", "datePublished", "datetime", false).Substring(0, 4);
            String genres = GetTagValue(html, "a", "itemprop", "genre", "href", true).Replace("/genre/", "");
            videoInfo.ImdbGenre = genres;
            
            String ratingIndentifier = "<div class=\"star-box-giga-star\">\n";
            int index = html.IndexOf(ratingIndentifier);
            String rating = html.Substring(index + ratingIndentifier.Length, 3);
            videoInfo.ImdbRating = rating;
            
            videoInfo.ImdbActors = GetImdbActorsFromDescription(videoInfo.ImdbDescription);
            videoInfo.ImdbDirector = GetImdbDirectorsFromDescription(videoInfo.ImdbDescription);
            MLog.Log(null, "Parsing done " + IniFile.VIDEO_INFO_IMDB_URL + imdbId);
            return videoInfo;
        }

        public static String GetImdbActorsFromDescription(String description)
        {
            String delimStartKey = ". With ";
            String delimEndKey = ".";
            int actorStartIndex = description.IndexOf(delimStartKey);
            int actorEndIndex = description.IndexOf(delimEndKey, actorStartIndex + delimStartKey.Length);
            String actorsFromDesc = description.Substring(actorStartIndex + delimStartKey.Length, actorEndIndex - actorStartIndex - delimStartKey.Length + delimEndKey.Length);
            actorsFromDesc = actorsFromDesc.Replace(", ", ",");
            return actorsFromDesc;
        }

        public static String GetImdbDirectorsFromDescription(String description)
        {
            String delimStartKey = "Directed by ";
            String delimEndKey = ".";
            int dirStartIndex = description.IndexOf(delimStartKey);
            int dirEndIndex = description.IndexOf(delimEndKey, dirStartIndex + delimStartKey.Length);
            String dirFromDesc = description.Substring(dirStartIndex + delimStartKey.Length, dirEndIndex - dirStartIndex - delimStartKey.Length + delimEndKey.Length);
            dirFromDesc = dirFromDesc.Replace(", ", ",");
            return dirFromDesc;
        }

        public static String GetImdbId(String html)
        {
            String id = GetTagValue(html, "div", "class", "rating rating-list", "id", false);
            if ((id != null) && (id.Length > 7))
                id = id.Substring(2, 7);
            else
                id = "";
            return id;
        }

        public static String CleanTitleforImdbSearch(String title)
        {
            String result;
            String[] removeStrings = new String[]{
                "720p","1080p","hd1080","480p","bluray","blu-ray","H.264",
                "x264","ac3", "hdtv", "sinners", "chd", "ajp", "pen", "dxva","eureka",
                "bdrip", "hddvd", "dts", "esir", "fxg", "mkv", "avi", "mp4", "hdrip",
                "wiki", "cmtc", "sample", "cinefile", "cbqb", "dvdrip", "jaybob",
                
                "[eng]", "[ger]","madeec","noir", "FLAWL3SS", "zmg","mxmg™",
                "brrip", "xvid", "vision", "wbz", "dvdscr", "xvidwbz", "timpe",
                "proper", "hidt", "devise","axxo","r5", "mic","imagine","h33t","[By KooKoo]",
                "www.torentz.3xforum.ro","MAXSPEED","P0nG","SCR","perfectionhd","dd5","fxm",
                "kirklestat","aac","[hdscene-release]","[sundance]","webscr","hdbrise",

                "1", "2", "3", "4", "5", "6", "7", "8","9","0", "[", "]", "~", "{", "}", "(", ")","-"
                
            };
            result = title.ToLower();

            for (int i = 0; i < removeStrings.Length; i++)
            {
                result = result.Replace(removeStrings[i].ToLower(), "");
            }

            result = result.Replace(".", " ");

            return result;
        }

        public static String GetTagValue(String html, String tagName, String searchAttributeName, String searchAttributeValue, String searchAttributeResult, bool enableMultipleResults)
        {
            String result = "";
            try
            {
                // Scan links on this page
                HtmlTag tag;
                HtmlParser parse = new HtmlParser(html);
                while (parse.ParseNext(tagName, out tag))
                {
                    // See if this anchor links to us
                    string value;
                    if (tag.Attributes.TryGetValue(searchAttributeName, out value))
                    {
                        if (value == searchAttributeValue)
                        {
                            if (tag.Attributes.TryGetValue(searchAttributeResult, out value))
                            {
                                if (enableMultipleResults)
                                    result += value + " ";
                                else
                                    return value;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MLog.Log(null, "error gettagvalue" + ex.Message);
            }
            return result;
        }
    }

	public class LastFMService
	{
		public static LastFmMeta GetArtistMeta(string artistName)
		{
			LastFmMeta meta = new LastFmMeta();
			meta.ArtistName = artistName;
			try
			{
				WebClient client = new WebClient();

				string xmlresponse = client.DownloadString(IniFile.PARAM_LASTFM_WS_URL[1]
					+ "method=artist.getInfo&api_key=" + IniFile.PARAM_LASTFM_API_KEY[1] + "&artist=" + artistName);

				XDocument doc = XDocument.Parse(xmlresponse);

				var items = doc.Descendants("tag");

				meta.GenreTags = new List<String>(items.ToList().Count);
				foreach (XElement el in items)
				{
					meta.GenreTags.Add(el.Element("name").Value);
				}
				if (meta.GenreTags.Count > 0)
					meta.MainGenre = meta.GenreTags[0];

				items = doc.Descendants("artist");
				meta.SimilarArtists = new List<String>(items.ToList().Count);
				foreach (XElement el in items)
				{
					meta.SimilarArtists.Add(el.Element("name").Value);
					if (meta.ArtistURL == null)
					{
						meta.ArtistURL = el.Element("url").Value;
						meta.ImageURL = (string) el.Elements("image").First(x => x.Attribute("size").Value == "medium");
							//el.Element("image").Value;
					}
				}
				items = doc.Descendants("bio");
				foreach (XElement el in items)
				{
					if (meta.ArtistOrigin==null)
					{
						XElement e = el.Element("placeformed");
						meta.ArtistOrigin = e!=null?e.Value:null;
						meta.ArtistSummary = el.Element("summary").Value;
						e = el.Element("yearformed");
						meta.YearFormed = e != null ? e.Value : null;
					}
				}
			}
			catch (Exception ex)
			{
				MLog.Log(ex, "Error loading lastfm data for " + artistName);
			}
			return meta;
		}
	}

	public class XMPService
	{
		public static void GetProperties(string xmpContent, ref MediaImageItem media)
		{
			XDocument doc = XDocument.Parse(xmpContent);
			var items = doc.Descendants("rdf_Description");

			String face;
			foreach (XElement el in items)
			{
				foreach (XAttribute atr in el.Attributes())
				{
					if (atr.Name == "mwg-rs_Name")
					{
						face = atr.Value;
						if (face != null)
							media.AddFace(face);
					}
				}

				foreach (XElement el2 in el.Descendants())
				{
					switch (el2.Name.ToString())
					{
						case "xmp_Rating":
							media.Rating = Convert.ToInt16(el2.Value);
							break;

						case "MicrosoftPhoto_LastKeywordXMP":
							foreach (XElement el3 in el2.Descendants())
							{
								if (el3.Name == "rdf_li")
									media.Tags.Add(el3.Value);
							}
							break;
						case "dc_description":
							foreach (XElement el3 in el2.Descendants())
							{
								if (el3.Name == "rdf_li")
									media.Subject=el3.Value;
							}
							break;
				}
				}

				
			}

		}

	}

	public abstract class KeyboardUtility
	{
		[Flags]
		private enum KeyStates
		{
			None = 0,
			Down = 1,
			Toggled = 2
		}

		private static KeyStates GetKeyState(Keys key)
		{
			KeyStates state = KeyStates.None;

			short retVal = Utilities.GetKeyState((int)key);

			//If the high-order bit is 1, the key is down
			//otherwise, it is up.
			if ((retVal & 0x8000) == 0x8000)
				state |= KeyStates.Down;

			//If the low-order bit is 1, the key is toggled.
			if ((retVal & 1) == 1)
				state |= KeyStates.Toggled;

			return state;
		}

		public static bool IsKeyDown(Keys key)
		{
			return KeyStates.Down == (GetKeyState(key) & KeyStates.Down);
		}

		public static bool IsKeyToggled(Keys key)
		{
			return KeyStates.Toggled == (GetKeyState(key) & KeyStates.Toggled);
		}
	}

}

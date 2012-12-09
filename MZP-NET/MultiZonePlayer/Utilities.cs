//#define DEBUG

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
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

using System.Web;

namespace MultiZonePlayer
{
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

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(
            int hWnd, // handle to destination window 
            uint Msg, // message 
            int wParam, // first message parameter 
            int lParam // second message parameter 
        );

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
        internal static extern void MoveWindow(IntPtr hwnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

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


        [System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public struct WAVEOUTCAPS
        {
            public short wMid;
            public short wPid;
            public int vDriverVersion;

            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szPname;

            public int dwFormats;
            public short wChannels;
            public short wReserved;
            public int dwSupport;

            public override string ToString()
            {
                return string.Format("wMid:{0}|wPid:{1}|vDriverVersion:{2}|'szPname:{3}'|dwFormats:{4}|wChannels:{5}|wReserved:{6}|dwSupport:{7}",
                    new object[] { wMid, wPid, vDriverVersion, szPname, dwFormats, wChannels, wReserved, dwSupport });
            }
        }

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint waveOutGetDevCaps(IntPtr hwo, ref WAVEOUTCAPS pwoc, uint cbwoc);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint waveOutGetDevCaps(int hwo, ref WAVEOUTCAPS pwoc, /*uint*/ int cbwoc);

        [DllImport("winmm.dll", SetLastError = true)]
        static extern uint waveOutGetNumDevs();

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


        private static Process extProc = null;

        public static IntPtr MakeLParam(int LoWord, int HiWord)
        {
            return (IntPtr)((HiWord << 16) | (LoWord & 0xffff));
        }

        public static List<WAVEOUTCAPS> GetDevCapsPlayback()
        {
            uint waveOutDevicesCount = waveOutGetNumDevs();
            if (waveOutDevicesCount > 0)
            {
                List<WAVEOUTCAPS> list = new List<WAVEOUTCAPS>();
                for (int uDeviceID = 0; uDeviceID < waveOutDevicesCount; uDeviceID++)
                {
                    WAVEOUTCAPS waveOutCaps = new WAVEOUTCAPS();
                    waveOutGetDevCaps(uDeviceID, ref waveOutCaps, Marshal.SizeOf(typeof(WAVEOUTCAPS)));
                    //MLog.Log(null,waveOutCaps.ToString());
                    list.Add(waveOutCaps);
                }
                return list;
            }
            else
            {
                return null;
            }
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

        public static Process RunProcessWait(String command)
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
            extProc.StartInfo.UseShellExecute = false;
            extProc.StartInfo.RedirectStandardInput = true;
            //extProc.StartInfo.ErrorDialog = false;
            //extProc.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            extProc.Start();
#if DEBUG 
            System.Threading.Thread.Sleep(100);
#else
             System.Threading.Thread.Sleep(30000);
#endif

            MLog.Log(null,"running proc completed " + command);
            return extProc;
        }

        public static void RunProcessWaitExit(String command)
        {
            MLog.Log(null,"running wait exit proc=" + command);
            RunProcessWait(command);
#if DEBUG
#else
            //System.Threading.Thread.Sleep(500);
#endif
            MLog.Log(null,"running completed wait exit proc " + command);
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

            StringBuilder temp = new StringBuilder(2048);
            int i = Utilities.GetPrivateProfileString(Section, Key, "", temp, 2048, path);
            return temp.ToString();
        }

        public static void AppendToGenericLogFile(String text, MZPEvent.EventSource logType)
        {
            StreamWriter str;
            switch (logType)
            {
                case MZPEvent.EventSource.System:
                    str = File.AppendText(IniFile.CurrentPath() + IniFile.LOG_GENERAL_FILE);
                    break;
                case MZPEvent.EventSource.Modem:
                    str = File.AppendText(IniFile.CurrentPath() + IniFile.LOG_MODEM_FILE);
                    break;
                default:
                    str = File.AppendText(IniFile.CurrentPath() + IniFile.LOG_EVENTS_FILE);
                    break;
            }

            
                
            lock (str)
            {
                str.Write(text);
                str.Close();
            }
        }

        public static String ReadFile(String fileName)
        {
            StreamReader str = File.OpenText(IniFile.CurrentPath() + fileName);
            String result = str.ReadToEnd();
            str.Close();
            return result;
        }

        public static byte[] ReadBinaryFile(String fileName)
        {
            FileStream fs = File.OpenRead(IniFile.CurrentPath() + fileName);
            byte[] data = new byte[fs.Length];
            fs.Read(data, 0, data.Length);
            fs.Close();
            return data;

            /*
            //Open image as byte stream to send to requestor
            FileInfo fInfo = new FileInfo("c:\\temp.png");
            long numBytes = fInfo.Length;

            FileStream fStream = new FileStream("c:\\temp.png", FileMode.Open, FileAccess.Read);

            BinaryReader br = new BinaryReader(fStream);

            byte[] bOutput = br.ReadBytes((int)numBytes);

            br.Close();

            fStream.Close();
             * */
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
                return textInfo.ToTitleCase(title.ToLower());
            }
            else return null;
        }

        public static String SanitiseInternational(String text)
        {
            String charsource = "ăşîâĂÎŞȚ";
            String chardest =   "asiiAIST";

            StringBuilder sb = new StringBuilder(text);

            if ((text != null) && (text != ""))
            {
                for (int i = 0; i < charsource.Length; i++)
                {
                    sb = sb.Replace(charsource[i], chardest[i]);
                }
            }
            
            return sb.ToString();
        }

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

        public static String PostURLMessage(String url, String form)
        {
            WebPostRequest post = new WebPostRequest("http://192.168.0.10:12347/jsonrpc?SendRemoteKey");
            String msg = @"{""jsonrpc"": ""2.0"", ""method"": ""Application.SetVolume"", ""params"": { ""volume"": 54 }, ""id"": 1}";
            post.Add(msg,"");
            MessageBox.Show(msg);
            String res = post.GetResponse();
            MLog.Log(null, res);
            return res;

        }
    }

    public class WebPostRequest
    {
        WebRequest theRequest;
        HttpWebResponse theResponse;
        ArrayList theQueryData;

        public WebPostRequest(string url)
        {
            theRequest = WebRequest.Create(url);
            theRequest.Method = "POST";
            theQueryData = new ArrayList();
        }

        public void Add(string key, string value)
        {
            theQueryData.Add(String.Format("{0}={1}", key, HttpUtility.UrlEncode(value)));
        }

        public string GetResponse()
        {
            // Set the encoding type
            theRequest.ContentType = "application/x-www-form-urlencoded";

            // Build a string containing all the parameters
            string Parameters = String.Join("&", (String[])theQueryData.ToArray(typeof(string)));
            theRequest.ContentLength = Parameters.Length;

            // We write the parameters into the request
            StreamWriter sw = new StreamWriter(theRequest.GetRequestStream());
            sw.Write(Parameters);
            sw.Close();

            // Execute the query
            theResponse = (HttpWebResponse)theRequest.GetResponse();
            StreamReader sr = new StreamReader(theResponse.GetResponseStream());
            return sr.ReadToEnd();
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
        public static void PlayError(String text, ZonesForm zone)
        {
            PlayMessageClean(text, zone);
        }

        public static void PlayMessage2(String text, ZonesForm zone)
        {

            try
            {

                ZoneAux aux = null;
                if (zone != null)
                {
                    aux = new ZoneAux(zone, (String)MZPState.Instance.zoneDefaultInputs["0"], zone.GetClonedZones()[0].ZoneDetails.OutputDeviceAutoCompleted);
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

        public static void PlayMessage(String text, ZonesForm zone)
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
                    ZoneMusic music = new ZoneMusic(zone, stream.Name);
                }
            }
            catch (Exception ex)
            {
                MLog.Log(null, "error t2s playerr: " + ex.Message + ex.StackTrace);
            }
        }

        public static void PlayMessageClean(String text, ZonesForm zone)
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
        public static void Log(Object o, Metadata.CommandResult res)
        {
            Log(o, res.ErrorMessage);
        }

        public static void Log(Exception e, Object o, String text)
        {
            if (e!=null)
                Log(e, e.StackTrace + "|" + text + " from sender:" + o.ToString());
            else
                Log(e, text + " from sender:" + o.ToString());
        }

        public static void Log(Object e, String text)
        {
            try
            {
                if (e != null)
                {
                    if (e.GetType().ToString().ToLower().Contains("exception"))
                        text += " err=" + ((Exception)e).Message + " \nstack=" + ((Exception)e).StackTrace;
                    else
                        text += " sender=" + e.ToString();
                }
            }
            catch (Exception)
            { }
            try
            {
                Utilities.AppendToGenericLogFile(System.DateTime.Now.ToString() + "-" + System.DateTime.Now.Millisecond + ": " + text + "\n", MZPEvent.EventSource.System);
            }
            catch (Exception)
            { }
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
            Utilities.AppendToGenericLogFile(text, MZPEvent.EventSource.Modem);
            }
            catch (Exception)
            { }
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
}

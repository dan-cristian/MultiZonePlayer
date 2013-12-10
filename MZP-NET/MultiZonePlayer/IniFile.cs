using System;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;

namespace MultiZonePlayer
{

        /// <summary>
        /// Create a New INI file to store or load data
        /// </summary>
        public class IniFile
        {

            public const String INI_FILE = "\\MultiZonePlayer.ini";
			public const String MEDIA_MUSIC_STORAGE_FILE = "\\MediaMusic.data";
			public const String MEDIA_VIDEO_STORAGE_FILE = "\\MediaVideo.data";
			public const String MEDIA_PICTURE_STORAGE_FILE = "\\MediaPicture.data";
			public const String SCHEDULER_FILE = "\\scheduler.ini";
			public const String RULES_FILE = "\\rules.ini";
			public const String CSV_TEMPERATURE_HUMIDITY = "\\temp-hum.csv";
			public const String CSV_CLOSURES = "\\closures.csv";
			public const String CSV_VOLTAGE = "\\voltage.csv";
			public const String SYSLOG_FILE = "syslog.csv";
			public const String LOG_GENERAL_FILE = "MultiZonePlayer.log";
            public const String LOG_EVENTS_FILE = "\\MultiZonePlayerEvents.log";
            public const String LOG_MODEM_FILE = "\\MultiZonePlayerModem.log";
            public const String LOG_WEB_FILE = "\\MultiZonePlayerWeb.log";
            public const String LOG_KEY_FILE = "\\MultiZonePlayerKeyboard.log";
            public const String LOG_RAWINPUT_FILE = "\\MultiZonePlayerRawInput.log";
			public const String MEDIA_META_FILE_NAME = "\\mzp-meta.txt"; 
            public const String TEMP_EXTENSION = ".temp.wav";
            public const String BEEP_OK_FILE_NAME = "\\BeepOK.wav";
            public const String BEEP_ERROR_FILE_NAME = "\\BeepError.wav";
            public const String TEST_FILE_NAME = "\\TestCond.mp3";
			public const String WEBROOT_SUBFOLDER = "\\webroot\\";
			public const String WEB_TMP_IMG_SUBFOLDER = WEBROOT_SUBFOLDER+ "m\\tmp\\";
			public const String WEB_PICTURES_SUBFOLDER = WEBROOT_SUBFOLDER + "m\\pictures\\";
			
			public static String DEFAULT_AUTO_DEV_NAME = "-AUTO BY KEYWORDS-";
            public static String DEFAULT_AUTO_DEV_ID = "-1";
			public const String SCHEDULER_SECTION_MAIN = "Scheduler";
			public const String SCHEDULER_SECTION_RFX = "RFX";
			public const String SCHEDULER_SECTION_LOG = "LOG";
            
            public const String INI_SECTION_ZONES = "Zones";
            public const String INI_SECTION_ZONESTATE = "ZoneState";
            public const String INI_SECTION_ZONEDEVICE = "ZoneDevice";
            public const String INI_SECTION_ZONEOUTPUTDEVICEKEYWORDS = "ZoneOutputDeviceKeywords";
            public const String INI_SECTION_ZONECONTROLDEVICENAME = "ZoneControlDevice";
            public const String INI_SECTION_ZONEPOWERONCTRL_GEMBIRD = "ZonePowerOnControlCmd";
            public const String INI_SECTION_ZONEPOWEROFFCTRL_GEMBIRD = "ZonePowerOffControlCmd";
            public const String INI_SECTION_ZONEPOWERCTRLINDEX_DK = "ZonePowerControlIndexDK";
            public const String INI_SECTION_ZONEDEFAULTVOLUMEPERCENT = "ZoneDefaultVolume";
            public const String INI_SECTION_ZONECAMERAID = "ZoneCameraId";
            public const String INI_SECTION_ZONEALARMZONEID = "ZoneAlarmZoneId";
            public const String INI_SECTION_ZONERESUME = "ZoneResume";
            public const String INI_SECTION_ZONE_WAKEUP = "ZoneWakeUp";
            public const String INI_SECTION_PLAYLIST = "Playlist";
            public const String INI_SECTION_INPUTDEVICE = "InputDevice";
            public const String INI_SECTION_INPUTDEVICENAME = "InputDeviceName";
            public const String INI_SECTION_INPUTS = "Inputs";
            public const String INI_SECTION_USERS = "Users";
            public const String INI_SECTION_PARAMS = "Parameters";
            public const String INI_SECTION_MUSICMOOD = "MusicMood";
            public const String INI_SECTION_MUSICSCHEDULE = "MusicSchedule";
			
            public const String DEFAULT_EXT_PLAYLIST = ".m3u";
            public const String POPM_CREATOR_NAME = "MZP";
            public static String[] MUSIC_EXTENSION = new String[] { "mp3", "wav", "flac" };
			public static String[] PICTURE_EXTENSION = new String[] { "jpg", "bmp", "png" };
#if DEBUG
            public static String[] VIDEO_EXTENSION = new String[] {"mp4" };
#else
            public static String[] VIDEO_EXTENSION = new String[] { "mkv", "avi", "mp4" };
#endif
            public  const String VIDEO_INFO_FILE = "\\info.txt";
            public  const String VIDEO_INFO_INI_SECTION = "General";
            public  const String VIDEO_INFO_INI_NAME = "Name";
            public  const String VIDEO_INFO_INI_IMDBID = "ImdbId";
            public  const String VIDEO_INFO_INI_YEAR = "Year";
            public  const String VIDEO_INFO_INI_GENRE = "Genre";
            public  const String VIDEO_INFO_INI_DIRECTOR = "Director";
            public  const String VIDEO_INFO_INI_SEEN = "Seen";
            public  const String VIDEO_INFO_INI_IMAGEURL = "ImageURL";
            public  const String VIDEO_INFO_INI_ACTORS = "Actors";
            public  const String VIDEO_INFO_INI_RATING = "Rating";
            public  const String VIDEO_INFO_INI_RESOLUTION = "Resolution";
            public  const String VIDEO_INFO_INI_DESCRIPTION = "Description";
            public  const String VIDEO_INFO_IMDB_URL = "http://www.imdb.com/title/tt";
            public  const String VIDEO_INFO_IMDB_FIND_URL = "http://www.imdb.com/search/title?title=";
            public const String MEDIA_TAG_LIBRARY_ID = "MZPADDED:";
            public const String MEDIA_TAG_FAVORITE = "MZPFAVORITE:";
            public const String PLAYLIST_DEFAULT = "default";

            public  const int VOLUME_STEP = 250;
            public  const int ZONE_TICK_FAST_SLEEP = 500;
			public const int ZONE_TICK_SLOW_SLEEP = 30000;
            public  const int ZONE_INACTIVITY_MAX_CYCLES = 120;
            public  const int POWERSAVING_CYCLE_DURATION = 10000;
            public  const int POWERSAVING_MAX_CYCLES = 60;
            public  const int POWERSAVING_CYCLE_WARNING = 8;

			public static String[] PARAM_ALARM_SECURE_AREA_ID = new String[] { "PARAM_ALARM_SECURE_AREA_ID", "1" , "Area ID to secure when armed"};
            public static String[] PARAM_RADIO_APP = new String[] { "RADIO_APP", "c:\\Program Files\\VideoMate\\ComproFM 2\\ComproFM.exe" };
            public static String[] PARAM_RADIO_APP_WIN_TITLE = new String[] { "PARAM_RADIO_APP_WIN_TITLE", "ComproFM" };
            public static String[] PARAM_POWER_CONTROL_APP_PROCESSNAME = new String[] {"POWER_CONTROL_APP_PROCESSNAME", "pm"};
            public static String[] PARAM_POWER_SAVING_APP = new String[] {"POWER_SAVING_APP", "\\powersave.bat"};
            public static String[] PARAM_POWER_RESUME_APP = new String[] {"POWER_RESUME_APP", "\\powerresume.bat"};
            public static String[] PARAM_POWER_CONTROL_APP_PATH_GEMBIRD = new String[] {"POWER_CONTROL_APP_PATH", "c:\\Program Files\\Gembird\\Power Manager\\pm.exe"};
            public static String[] PARAM_POWER_CONTROL_STATUS_FILE_GEMBIRD = new String[] { "PARAM_POWER_CONTROL_STATUS_FILE", "c:\\Program Files\\Gembird\\Power Manager\\Info.ini" };
            public static String[] PARAM_POWER_CONTROL_STATUS_APP_GEMBIRD = new String[] { "PARAM_POWER_CONTROL_STATUS_APP", "'c:\\Program Files\\Gembird\\Power Manager\\pm.exe' -info" };
            public static String[] PARAM_POWER_CONTROL_DEVICE_NAME_GEMBIRD = new String[] { "PARAM_POWER_CONTROL_DEVICE_NAME", "Multi1" };
            public static String[] PARAM_FOLDER_DELETED_ITEMS = new String[] { "PARAM_FOLDER_DELETED_ITEMS", "\\DeletedMultiPlayerItems\\" };

			public static String[] PARAM_NOTIFYUSER_SOUND_FILE = new String[] { "PARAM_NOTIFYUSER_SOUND_FILE", "\\notifyuser_x.mp3" };

			public static String[] PARAM_VIDEO_APP_VLC_DIR_PATH = new String[] { "PARAM_VIDEO_APP_VLC_DIR_PATH", "C:\\Program Files (x86)\\VideoLAN\\VLC\\vlc.exe" };
            public static String[] PARAM_VIDEO_APP_VLC_PROCESSNAME = new String[] { "PARAM_VIDEO_APP_VLC_PROCESSNAME", "vlc" };
            public static String[] PARAM_VIDEO_APP_VLC_WINDOWTITLE_KEY = new String[] { "PARAM_VIDEO_APP_VLC_WINDOWTITLE_KEY", "VLC (" };

            public static String[] PARAM_VIDEO_APP_MPC_PATH = new String[] { "PARAM_VIDEO_APP_MPC_PATH", "c:\\Program Files\\Media Player Classic - Home Cinema\\mpc-hc.exe"  };
            public static String[] PARAM_VIDEO_APP_MPC_PROCESSNAME = new String[] { "PARAM_VIDEO_APP_MPC_PROCESSNAME", "mpc-hc" };
            public static String[] PARAM_VIDEO_APP_MPC_WINDOWTITLE_KEY = new String[] { "PARAM_VIDEO_APP_MPC_WINDOWTITLE_KEY", "Media Player Classic Home Cinema" };
            public static String[] PARAM_VIDEO_APP_MPC_WEBPORT = new String[] { "PARAM_VIDEO_APP_MPC_WEBPORT", "13579" };

            public static String[] PARAM_VIDEO_STORE_ROOT_PATH = new String[] { "PARAM_VIDEO_STORE_ROOT_PATH", "c:\\temp\\" };
            public static String[] PARAM_MUSIC_STORE_ROOT_PATH = new String[] { "PARAM_MUSIC_STORE_ROOT_PATH", "c:\\temp\\" };
			public static String[] PARAM_PICTURE_STORE_ROOT_PATH = new String[] { "PARAM_PICTURE_STORE_ROOT_PATH", "c:\\temp\\" };
			public static String[] PARAM_PICTURE_TAG_IGNORE = new String[] { "PARAM_PICTURE_TAG_IGNORE", "private" };
            public static String[] PARAM_KEYBOARD_DEVICE_IDENTIFIER = new String[] { "PARAM_KEYBOARD_DEVICE_IDENTIFIER", "ACPI#PNP" };

            public static String[] PARAM_WEBSERVER_PORT_EXT = new String[] { "PARAM_WEBSERVER_PORT_EXT", "12345" };
            public static String[] PARAM_WEBSERVER_PORT_EXT_SAFE = new String[] { "PARAM_WEBSERVER_PORT_EXT_SAFE", "12347" };
            public static String[] PARAM_WEBSERVER_PORT_INT = new String[] { "PARAM_WEBSERVER_PORT_INT", "888" };
			public static String[] PARAM_ISPY_URL = new String[] { "PARAM_ISPY_URL", "http://localhost" };
			public static String[] PARAM_ISPY_PORT = new String[] { "PARAM_ISPY_PORT", "8080" };
            public static String[] PARAM_ISPY_PASS = new String[] { "PARAM_ISPY_PASS", "poi" };
            public static String[] PARAM_ISPY_PROCNAME = new String[] { "PARAM_ISPY_PROCNAME", "ispy" };
            public static String[] PARAM_ISPY_OTHERPROC = new String[] { "PARAM_ISPY_OTHERPROC", "ffmpeg" };
            public static String[] PARAM_ISPY_APP_PATH = new String[] { "PARAM_ISPY_APP_PATH", "c:\\Program Files\\iSpy\\iSpy\\iSpy.exe"};

            public static String[] PARAM_WEB_USER = new String[] { "PARAM_WEB_USER", "a" };
            public static String[] PARAM_WEB_PASS = new String[] { "PARAM_WEB_PASS", "poi" };
            public static String[] PARAM_WEB_DEFAULT_PAGE = new String[] { "PARAM_WEB_DEFAULT_PAGE", "default.html" };
            
            public static String[] PARAM_SILENCE_SAFE_HOUR_INTERVAL = new String[] { "PARAM_SILENCE_SAFE_HOUR_INTERVAL", "21:00-07:00" };
            public static String[] PARAM_LIBRARY_AUTOUPDATE_HOUR = new String[] { "PARAM_LIBRARY_AUTOUPDATE_HOUR", "04:00" };
			public static String[] PARAM_SILENCE_SAFE_VOLUME = new String[] { "PARAM_SILENCE_SAFE_VOLUME", "50", "Volume percent during safe interval, minimum audible" };
			public static String[] PARAM_NOTIFY_VOLUME_INCREASE = new String[] { "PARAM_NOTIFY_VOLUME_INCREASE", "25", "Volume percent increase on top of default volume for user notifications" };
			
            public static String[] PARAM_PARADOX_WINLOAD_DATA_FILE = new String[] { "PARAM_PARADOX_WINLOAD_DATA_FILE", "1.txt" };
            public static String[] PARAM_PARADOX_WINLOAD_PROCNAME = new String[] { "PARAM_PARADOX_WINLOAD_PROCNAME", "WinLoad" };
            public static String[] PARAM_PARADOX_WINLOAD_USERNAME = new String[] { "PARAM_PARADOX_WINLOAD_USERNAME", "MASTER" };
            public static String[] PARAM_PARADOX_WINLOAD_USERPASS = new String[] { "PARAM_PARADOX_WINLOAD_USERPASS", "1234" };
            public static String[] PARAM_PARADOX_WINLOAD_APP_PATH = new String[] { "PARAM_PARADOX_WINLOAD_APP_PATH", "c:\\Program Files\\Paradox Security Systems\\WinLoad\\WinLoad.exe" };

            public static String[] PARAM_GTALK_USERNAME = new String[] { "PARAM_GTALK_USERNAME", "antonio.gaudi33@gmail.com" };
            public static String[] PARAM_GTALK_USERPASS = new String[] { "PARAM_GTALK_USERPASS", "S9mRs4vm" };
            //public static String[] PARAM_GTALK_SERVER = new String[] { "PARAM_GTALK_SERVER", "gmail.com" };
            public static String[] PARAM_GTALK_TARGETUSER = new String[] { "PARAM_GTALK_TARGETUSER", "dan.cristian@gmail.com" };
            public static String[] PARAM_SMS_TARGETNUMBER = new String[] { "PARAM_SMS_TARGETNUMBER", "0040726159521" };
            public static String[] PARAM_SMS_COMPORT = new String[] { "PARAM_SMS_COMPORT", "COM3" };
            public static String[] PARAM_SMS_AT_LINES_COUNT = new String[] { "PARAM_SMS_AT_LINES_COUNT", "2", "Number of new lines expected to be received after an AT command" };
            public static String[] PARAM_SMS_ATD_LINES_COUNT = new String[] { "PARAM_SMS_ATD_LINES_COUNT", "5", "Number of new lines expected to be received after an ATD (dial) command" };

			public static String[] PARAM_MODEM_TARGETNUMBER = new String[] { "PARAM_MODEM_TARGETNUMBER", "0726159521" };
			public static String[] PARAM_MODEM_COMPORT = new String[] { "PARAM_MODEM_COMPORT", "COM8" };
			public static String[] PARAM_MODEM_AT_LINES_COUNT = new String[] { "PARAM_MODEM_AT_LINES_COUNT", "2", "Number of new lines expected to be received after an AT command" };
			public static String[] PARAM_MODEM_ATD_LINES_COUNT = new String[] { "PARAM_MODEM_ATD_LINES_COUNT", "5", "Number of new lines expected to be received after an ATD (dial) command" };
			public static String[] PARAM_MODEM_DEVICE_NAME = new String[] { "PARAM_MODEM_DEVICE_NAME", "Zoom V92", "Entire or partial modem name used to find modem port" };
			

			public static String[] PARAM_RELAY_DEVICE_NAME = new String[] { "PARAM_RELAY_DEVICE_NAME", "FT245R USB FIFO" };

			public static String[] PARAM_RFXCOM_PORT = new String[] { "PARAM_RFXCOM_PORT", "COM42" };
			public static String[] PARAM_RFX_DEVICE_NAME = new String[] { "PARAM_RFX_DEVICE_NAME", "RFXtrx433" };
            public static String[] PARAM_VLC_AUDIOOUT = new String[] { "PARAM_VLC_AUDIOOUT", "directx", "DIRECTX or WAVEOUT" };
            public static String[] PARAM_GENERIC_INTERVAL_SPLIT = new String[] { "PARAM_GENERIC_INTERVAL_SPLIT", "10-60-240", "3 numbers, in minutes, separated by -" };

			public static String[] PARAM_UPS_APC_LOG_SOURCE = new String[] { "PARAM_UPS_APC_LOG_SOURCE", "apc ups service", "should be lowercase" };
			public static String[] PARAM_UPS_APC_LOG_POWERFAILURE_ACTIVE = new String[] { "PARAM_UPS_APC_LOG_POWERFAILURE_ACTIVE", "blackout" };
			public static String[] PARAM_UPS_APC_LOG_POWERFAILURE_INACTIVE = new String[] {"PARAM_UPS_APC_LOG_POWERFAILURE_INACTIVE", "ac utility power" };

            public static String[] PARAM_ACCEPTED_WEB_SAFE_DEVICES_HEADERS = new String[] { "PARAM_ACCEPTED_WEB_SAFE_DEVICES_HEADERS", "BlackBerry 9360","list of values separated by |" };

            public static String[] PARAM_XBMC_PROCESS_NAME = new String[] { "PARAM_XBMC_PROCESS_NAME", "XBMC" };
            public static String[] PARAM_XBMC_APP_PATH = new String[] { "PARAM_XBMC_APP_PATH", "c:\\Program Files (x86)\\XBMC\\XBMC.exe" };
            public static String[] PARAM_BOOT_TIME_SECONDS = new String[] { "PARAM_BOOT_TIME_SECONDS", "30" };

			public static String[] PARAM_LASTFM_API_KEY = new String[] { "PARAM_LASTFM_API_KEY", "9d320007e2d23c9571f466030c7f2ddc" };
			public static String[] PARAM_LASTFM_SECRET_KEY = new String[] { "PARAM_LASTFM_SECRET_KEY", "812f830d23965715878d5cd1390ada1a" };
			public static String[] PARAM_LASTFM_WS_URL = new String[] { "PARAM_LASTFM_WS_URL", "http://ws.audioscrobbler.com/2.0/?" };
			public static String[] PARAM_LASTFM_FORCE_META_UPDATE = new String[] { "PARAM_LASTFM_FORCE_META_UPDATE", "1", "1 to force meta refresh, 0 to load from disk" };

			public static String[] PARAM_GPIO_CDC_MANUFACTURER_NAME = new String[] { "PARAM_GPIO_CDC_MANUFACTURER_NAME", "Microchip Technology, Inc.", "GPIO manufacturer name used to auto identify COM port name" };
			public static String[] PARAM_GPIO_CDC_COMPORT = new String[] { "PARAM_GPIO_CDC_COMPORT", "COM7" };

			public static String[] PARAM_WDIO_MANUFACTURER_NAME = new String[] { "PARAM_WDIO_MANUFACTURER_NAME", "Prolific", "WDIO manufacturer name used to auto identify COM port name" };
			public static String[] PARAM_WDIO_COMPORT = new String[] { "PARAM_WDIO_COMPORT", "COM20" };
			public static String[] PARAM_WDIO_CHANNEL = new String[] { "PARAM_WDIO_CHANNEL", "A" };

			public static String[] PARAM_UPS_MUSTEK_STATUS_URL = new String[] { "PARAM_UPS_MUSTEK_STATUS_URL", "http://localhost:12349", "Web port for UPS Assistant app - mustek UPS" };

			public static String[] PARAM_TIMESPAN_FORMAT = new String[] { "PARAM_TIMESPAN_LONG_FORMAT", @"d\dh\hm\ms\s", "Long Format to display time intervals" };
			public static String[] PARAM_ROUTER_HOST = new String[] { "PARAM_ROUTER_HOST", "http://192.168.0.1:88/", "Home router address and port" };
			public static String[] PARAM_ROUTER_ACTIVE_WIFI_CLIENTS_URL = new String[] { "PARAM_ROUTER_ACTIVE_WIFI_CLIENTS_URL", "cgi-bin/luci/;stok=02bfb70c6bed15bd5fffa8d759fe2a0b/admin/network/wireless_status/radio0.network1", "Address to get wireless active clients fro home router" };
			public static String[] PARAM_ROUTER_ACTIVE_WIFI_CLIENTS_URL_CUSTOM = new String[] { "PARAM_ROUTER_ACTIVE_WIFI_CLIENTS_URL_CUSTOM", "cgi-bin/luci/command/cfg089944", "Custom address to get wireless active clients from home router" };
			public static String[] PARAM_ROUTER_LOGOUT_URL = new String[] { "PARAM_ROUTER_LOGOUT_URL", "cgi-bin/luci/;stok=#TOKEN#/admin/logout/", "Logout url to clean temp session file, #TOKEN# represents the replaced variable" };
			public static String[] PARAM_ROUTER_USER_NAME = new String[] { "PARAM_ROUTER_USER_NAME", "root", "User name for router" };
			public static String[] PARAM_ROUTER_USER_PASS = new String[] { "PARAM_ROUTER_USER_PASS", "abcd1234", "router pass" };

			public static String[] PARAM_REMOTE_SERVER_LIST = new String[] { "PARAM_REMOTE_SERVER_LIST", "http://192.168.0.110/", "URL list of remote servers (PIs) comma separated" };
			public static String[] PARAM_REMOTE_SERVER_BT_STATUS_FILE = new String[] { "PARAM_REMOTE_SERVER_BT_STATUS_FILE", "btstat.txt", "file name with BT status" };
			public static String[] PARAM_REMOTE_SERVER_WIFI_STATUS_FILE = new String[] { "PARAM_REMOTE_SERVER_WIFI_STATUS_FILE", "wifi.txt", "file name with wifi status" };

			public static String DATETIME_MINSECMILI_FORMAT = "mm:ss:fff";
            public static String DATETIME_DAYHR_FORMAT = "HH:mm";
			public static String DATETIME_MONTH_FORMAT = "MMM";
			public static String DATETIME_FULL_FORMAT = "yyyy-MM-dd HH:mm:ss";
			public static String DATETIME_FULL_FORMAT_FOR_FILE = "yyyy-MM-dd_HH-mm-ss";

            public static Object[] PARAMS = new Object[]
            {
				PARAM_ALARM_SECURE_AREA_ID,
                PARAM_RADIO_APP,
                PARAM_RADIO_APP_WIN_TITLE,
                PARAM_POWER_SAVING_APP,
                PARAM_POWER_RESUME_APP,
                PARAM_POWER_CONTROL_APP_PROCESSNAME,
                PARAM_POWER_CONTROL_APP_PATH_GEMBIRD,
                PARAM_POWER_CONTROL_STATUS_FILE_GEMBIRD,
                PARAM_POWER_CONTROL_STATUS_APP_GEMBIRD,
                PARAM_POWER_CONTROL_DEVICE_NAME_GEMBIRD,
                PARAM_FOLDER_DELETED_ITEMS,
				PARAM_NOTIFYUSER_SOUND_FILE,
                PARAM_VIDEO_APP_VLC_DIR_PATH,
                PARAM_VIDEO_APP_VLC_PROCESSNAME,
                PARAM_VIDEO_APP_VLC_WINDOWTITLE_KEY,
                PARAM_VIDEO_APP_MPC_PATH,
                PARAM_VIDEO_APP_MPC_PROCESSNAME,
                PARAM_VIDEO_APP_MPC_WINDOWTITLE_KEY,
                PARAM_VIDEO_APP_MPC_WEBPORT,
                PARAM_VIDEO_STORE_ROOT_PATH,
                PARAM_MUSIC_STORE_ROOT_PATH,
				PARAM_PICTURE_STORE_ROOT_PATH,
				PARAM_PICTURE_TAG_IGNORE,
                PARAM_KEYBOARD_DEVICE_IDENTIFIER,
                //PARAM_WEBSERVER_PORT_EXT,
				PARAM_WEBSERVER_PORT_INT,
				PARAM_ISPY_URL,
                PARAM_ISPY_PASS,
                PARAM_ISPY_PORT,
                PARAM_ISPY_PROCNAME,
                PARAM_ISPY_OTHERPROC,
                PARAM_ISPY_APP_PATH,
                PARAM_WEB_USER,
                PARAM_WEB_PASS,
                PARAM_WEB_DEFAULT_PAGE,
                PARAM_SILENCE_SAFE_HOUR_INTERVAL,
				PARAM_SILENCE_SAFE_VOLUME,
				PARAM_NOTIFY_VOLUME_INCREASE,
                PARAM_LIBRARY_AUTOUPDATE_HOUR,
                PARAM_PARADOX_WINLOAD_DATA_FILE,
                PARAM_PARADOX_WINLOAD_APP_PATH,
                PARAM_PARADOX_WINLOAD_PROCNAME,
                PARAM_PARADOX_WINLOAD_USERNAME,
                PARAM_PARADOX_WINLOAD_USERPASS,
                PARAM_GTALK_USERNAME,
                PARAM_GTALK_USERPASS,
                //PARAM_GTALK_SERVER,
                PARAM_GTALK_TARGETUSER,
                PARAM_SMS_TARGETNUMBER,
                PARAM_SMS_COMPORT,
                PARAM_SMS_AT_LINES_COUNT,
                PARAM_SMS_ATD_LINES_COUNT,
				PARAM_MODEM_TARGETNUMBER,
                //PARAM_MODEM_COMPORT,
                PARAM_MODEM_AT_LINES_COUNT,
                PARAM_MODEM_ATD_LINES_COUNT,
				PARAM_MODEM_DEVICE_NAME,
                PARAM_VLC_AUDIOOUT,
                PARAM_GENERIC_INTERVAL_SPLIT,
                PARAM_UPS_APC_LOG_SOURCE,
                PARAM_UPS_APC_LOG_POWERFAILURE_ACTIVE,
                PARAM_UPS_APC_LOG_POWERFAILURE_INACTIVE,
				PARAM_UPS_MUSTEK_STATUS_URL,
                PARAM_ACCEPTED_WEB_SAFE_DEVICES_HEADERS,
                PARAM_XBMC_PROCESS_NAME,
                PARAM_XBMC_APP_PATH,
                PARAM_BOOT_TIME_SECONDS,
				PARAM_LASTFM_API_KEY,
				PARAM_LASTFM_SECRET_KEY,
				PARAM_LASTFM_WS_URL,
				PARAM_LASTFM_FORCE_META_UPDATE,
				//PARAM_RFXCOM_PORT,
				PARAM_RFX_DEVICE_NAME,
				PARAM_RELAY_DEVICE_NAME,
				PARAM_GPIO_CDC_MANUFACTURER_NAME,
				PARAM_WDIO_MANUFACTURER_NAME,
				PARAM_WDIO_CHANNEL,
				PARAM_TIMESPAN_FORMAT,
				PARAM_ROUTER_HOST,
				PARAM_ROUTER_ACTIVE_WIFI_CLIENTS_URL,
				PARAM_ROUTER_ACTIVE_WIFI_CLIENTS_URL_CUSTOM,
				PARAM_ROUTER_LOGOUT_URL,
				PARAM_REMOTE_SERVER_LIST,
				PARAM_REMOTE_SERVER_BT_STATUS_FILE,
				PARAM_REMOTE_SERVER_WIFI_STATUS_FILE
             };
            private static string m_iniFinalPath =  null;
            private static string m_iniTempPath = null;
            private static string m_iniOldPath = null;
            
            /// <summary>
            /// INIFile Constructor.
            /// </summary>
            /// <PARAM name="INIPath"></PARAM>
            
            /// <summary>
            /// Write Data to the INI File
            /// </summary>
            /// <PARAM name="Section"></PARAM>
            /// Section name
            /// <PARAM name="Key"></PARAM>
            /// Key Name
            /// <PARAM name="Value"></PARAM>
            /// Value Name
            public static void IniWriteValuetoTemp(string Section, string Key, string Value)
            {
                /*if (m_iniTempPath == null)
                    initPath();
                
                long res = Utilities.WritePrivateProfileString(Section, Key, Value, m_iniTempPath);
                 */
				if (Value == "") Value = ".";
                IniWriteValuetoFinal(Section, Key, Value);
            }

            public static void IniWriteValuetoFinal(string Section, string Key, string Value)
            {
                if (m_iniTempPath == null)
                    initPath();
                if (Value == "") Value = ".";
                long res = Utilities.WritePrivateProfileString(Section, Key, Value, m_iniFinalPath);
            }

            public static void IniBeginSave()
            {
                if (m_iniFinalPath == null)
                    initPath();
                try
                {
                    File.Delete(m_iniTempPath);
                }
                catch (Exception ex)
                {
                    MLog.Log(ex, "Cannot delete " + m_iniTempPath);
                }
            }

            public static void IniCompleteSave()
            {
                try
                {
                    File.Delete(m_iniOldPath);
                    File.Move(m_iniFinalPath, m_iniOldPath);
                    File.Move(m_iniTempPath, m_iniFinalPath);
                }
                catch (Exception ex)
                {
                    MLog.Log(ex, "Cannot delete " + m_iniOldPath);
                }
            }

            private static void initPath()
            {
                //path = Directory.GetCurrentDirectory() + INI_FILE;
                m_iniFinalPath = CurrentPath() + INI_FILE;
                m_iniTempPath = CurrentPath() + INI_FILE + ".tmp";
                m_iniOldPath = CurrentPath() + INI_FILE + ".old";
                
            }

            public static String CurrentPath()
            {
                //return Directory.GetCurrentDirectory();
                return Directory.GetParent(Application.ExecutablePath).FullName+"\\";
            }

            public static Hashtable LoadAllIniEntriesByKey(String sectionName)
            {

                Hashtable entries = new Hashtable();
                String entry;

                int i = 0;
                do
                {
                    entry = IniReadValue(sectionName, i.ToString());
                    if (entry == "") break;
                    if (entry == ".") entry = "";
                    //hack to support network paths
                    //if (entry.Contains("\\\\"))
                    //    entry = entry.Replace("\\\\", "\\");
                    entries.Add(i.ToString(), entry);
                    i++;
                }
                while (true);

                return entries;
            }

            public static Hashtable LoadAllIniEntriesByIntKey(String sectionName)
            {

                Hashtable entries = new Hashtable();
                String entry;


                int i = 0;
                do
                {
                    entry = IniReadValue(sectionName, i.ToString());
                    if (entry == "") break;
                    if (entry == ".") entry = "";
                    //hack to support network paths
                    //if (entry.Contains("\\\\"))
                    //    entry = entry.Replace("\\\\", "\\");
                    entries.Add(i, entry);
                    i++;
                }
                while (true);

                return entries;
            }

            public static String LoadIniEntryByKey(String sectionName, String key)
            {
                String entry;

                entry = IniReadValue(sectionName, key);
                //hack to support network paths
                //if (entry.Contains("\\\\"))
                //    entry = entry.Replace("\\\\", "\\");

                if (entry == ".") entry = "";
                return entry;
            }

            public static Hashtable LoadAllIniSectionsByKey(String sectionName)
            {

                Hashtable entries = new Hashtable();
                Hashtable entry = new Hashtable();

                int j = 0;
                do
                {
                    entry = LoadAllIniEntriesByKey(sectionName + j);
                    if (entry.Count > 0)
                        entries.Add(j.ToString(), entry);
                    else
                        break;
                    j++;
                }
                while (true);

                return entries;
            }

            
            public static string IniReadValue(string Section, string Key)
            {
                if (m_iniFinalPath == null)
                    initPath();
                return Utilities.IniReadValue(Section, Key, m_iniFinalPath);
            }

			public static bool IsWhiteListed(string window)
			{
				MLog.Log(null, "whitelist:["+window+"]");
				return window.Contains("WinLoad");
			}
        }

}

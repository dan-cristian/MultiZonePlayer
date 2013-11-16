﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace MultiZonePlayer {
	
	public abstract class Singleton {
		//private static volatile Singleton instance;
		//private static object syncRoot = new Object();
		
		
		/*public static Singleton Instance {
			get {
				if (instance == null) {
					lock (syncRoot) {
						if (instance == null)
							instance = new Singleton();
					}
				}
				return instance;
			}
		}*/
		[Description("Edit")]
		public String UniqueIdentifier="";
		public int Id;

		public Singleton() { }
		public abstract List<Singleton> ValueList {
			get;
		}
		public int Index {
			get { return ValueList.IndexOf(this); }
		}
		/*public abstract List<int> IdList {
			get;
		}*/
		
		public virtual Singleton Instance{get{return this;}}

		public abstract void SaveToIni();
	}

	public class Performance:Singleton {
		[Flags]
		public enum PerformanceFlags {
			///<summary>Indicated this element had error, parameter is error count</summary>
			IsError,
			///<summary>Speed of operation in miliseconds</summary>
			Speed
		}
		protected static List<Singleton> m_valueList = new List<Singleton>();
		private List<PerformanceFlags> Flags;
		private List<Object> FlagVars;
		private List<Int32> Speed = new List<Int32>();
		
		public List<DateTime> When = new List<DateTime>();
		public String InitialMessage = null, LastMessage = null;
		public DateTime FirstOccurence, LastOccurence;
		public int OccurenceCount = 0;
		public int ErrorCount = 0;
		protected static int m_id = 0;
		protected int id;
		
		public Performance(){
		}
		public Performance(String uniqueIdentifier, String message, params Object[] flagVars) {
			Id = m_id;
			m_id++;
			UniqueIdentifier = uniqueIdentifier;
			Update(message, flagVars);
		}
		public override List<Singleton> ValueList {
			get { return m_valueList; }
		}
		public static Performance StaticInstance { 
			get {
				if (m_valueList != null && m_valueList.Count > 0)
					return (Performance)m_valueList[0];
				else return new Performance();
			}
		}
		public override Singleton Instance {
			get {return Performance.StaticInstance;}
		}
		protected static void Add(Singleton element) {
			m_valueList.Add(element);
		}
		public int MaxSpeed {
			get { return Speed.Max(); }
		}
		public int MinSpeed {
			get { return Speed.Min(); }
		}
		public int AverageSpeed {
			get {
				if (Speed.Count > 0)
					return Speed.Aggregate((acc, cur) => acc + cur) / Speed.Count;
				else
					return -1;
			}
		}

		public static Singleton Create(String message, Boolean showInLog, params Object[] flagVars) {
			var callingMethod = new System.Diagnostics.StackTrace(1, false).GetFrame(0).GetMethod();
			var callingFrame = new System.Diagnostics.StackTrace(1, false).GetFrame(0);
			String uniqueId = callingMethod.Name + "-" + callingFrame.GetNativeOffset() + "-" + callingFrame.GetILOffset();
			Performance element = (Performance)m_valueList.Find(x => x.UniqueIdentifier == uniqueId);
			if (element == null) {
				element = new Performance(uniqueId, message, flagVars);
				m_valueList.Add(element);
			}
			else {
				element.Update(message, flagVars);
			}
			if (showInLog) MLog.Log(null, message);
			return element;
		}

		protected void Update(String message, params Object[] flagVars) {
			if (InitialMessage == null) {
				InitialMessage = message;
				LastMessage = "";
			}
			else
				LastMessage = message;
			When.Add(DateTime.Now);
			FirstOccurence = When[0];
			LastOccurence = When[When.Count - 1];
			OccurenceCount = When.Count;
			Flags = new List<PerformanceFlags>();
			FlagVars = new List<object>();
			foreach (Object o in flagVars) {
				if (o.GetType() == typeof(PerformanceFlags)) {
					Flags.Add((PerformanceFlags)o);
				}
				else
					FlagVars.Add(o);
			}
			PerformanceFlags flag;
			for (int i = 0; i < Flags.Count; i++) {
				flag = Flags[i];
				switch (flag) {
					case PerformanceFlags.IsError:
						ErrorCount += Convert.ToInt16(FlagVars[i]);
						break;
					case PerformanceFlags.Speed:
						Speed.Add(Convert.ToInt32(FlagVars[i]));
						break;
				}
			}
		}

		public override void SaveToIni() {
		//TODO
		}
	}

	public class User:Singleton {
		[Description("Edit")]
		public String Name;
		public String Code;
		[Description("Edit")]
		public String PhoneBTAddress;
		[Description("Edit")]
		public String Email;
		[Description("Edit")]
		public String MobileNumber;
		[Description("Edit")]
		public String WifiMACAddress;
		public UserRole Role;
		public DateTime NearbyPresentSince;
		public UserPresence.PresenceType NearbyPresenceType;
		[Description("Edit")]
		/// <summary>
		/// format is [pattern]:macroname:zoneid;repeat same format
		/// </summary>
		public String MacroClosurePatterns;

		public enum UserRole {
			Owner,
			Family,
			Friend,
			Public
		}
		protected static List<Singleton> m_valueList = new List<Singleton>();

		public static Singleton StaticInstance {
			get {
				if (m_valueList != null && m_valueList.Count > 0)
					return m_valueList[0];
				else return new User();
			}
		}
		public override Singleton Instance { get { return User.StaticInstance; } }

		public User() {
		}

		public User(int id, String name, String code) {
			this.Id = id;
			this.Name = name;
			this.Code = code;
		}
		public override List<Singleton> ValueList {
			get { return m_valueList; }
		}
		public static List<User> UserList {
			get { return m_valueList.Select(x => (User)x).ToList(); }
		}
		public String NearbyPresentAgeAsTimeSpan {
			get { return Utilities.DurationAsTimeSpan(NearbyPresentAge); }
		}
		public TimeSpan NearbyPresentAge {
			get { return DateTime.Now.Subtract(NearbyPresentSince); }
		}

		public String GetMacroNameByPattern(String pattern, int zoneId) {
			if (MacroClosurePatterns != null) {
				int startIndex = MacroClosurePatterns.IndexOf("[" + pattern + "]");
				if (startIndex != -1) {
					String partial = MacroClosurePatterns.Substring(startIndex);
					string[] defs = partial.Split(';');
					string[] atoms = defs[0].Split(':');
					String macro = atoms[1];
					if (atoms[2] == zoneId.ToString())
						return macro;
				}
			}
			return null;
		}
		public static User GetUser(int id) {
			return (User)m_valueList.Find(x => x.Id == id);
		}
		public static User GetUserByBT(String BTAddress) {
			BTAddress = BTAddress.ToUpper();
			return (User)m_valueList.Find(x => ((User)x).PhoneBTAddress != null && ((User)x).PhoneBTAddress.ToUpper() == BTAddress);
		}
		public static User GetUserByWifi(String WifiAddress) {
			WifiAddress = WifiAddress.ToUpper();
			return (User)m_valueList.Find(x => ((User)x).WifiMACAddress != null && ((User)x).WifiMACAddress.ToUpper() == WifiAddress);
		}
		public static void Add(User user) {
			m_valueList.Add(user);
		}
		public static void LoadFromIni() {
			Hashtable values = IniFile.LoadAllIniEntriesByIntKey(IniFile.INI_SECTION_USERS);
			User user;

			try {
				foreach (String json in values.Values) {
					user = fastJSON.JSON.Instance.ToObject<User>(json);
					Add(user);
				}
			}
			catch (Exception ex) {
				MLog.Log(ex, "Error loading User");
				throw new Exception("Error load User", ex);
			}
		}

		public override void SaveToIni() {
			String json;
			int r = 0;
			fastJSON.JSONParameters param = new fastJSON.JSONParameters(); param.UseExtensions = false;
			foreach (User user in m_valueList) {
				json = fastJSON.JSON.Instance.ToJSON(user, param);
				IniFile.IniWriteValuetoTemp(IniFile.INI_SECTION_USERS, r.ToString(), json);
				r++;
			}
		}
	}

	public sealed class UserPresence {
		private static List<UserPresence> m_presenceList = new List<UserPresence>();
		public User User;
		public DateTime LastBTActiveDate, LastWifiActiveDate;
		public DateTime LastMessengerActiveDate;
		public PresenceType Type;
		public int BTLostContactCount = 0, WifiLostContactCount = 0;
		private static List<Bluetooth.Device> m_lastBTDeviceList = new List<Bluetooth.Device>();
		private static List<String> m_lastWifiAddressList = new List<string>();
		public enum PresenceType {
			Bluetooth,
			Messenger,
			Wifi,
			Internet
		}

		private static volatile UserPresence instance;
		private static object syncRoot = new Object();
		public static UserPresence Instance {
			get {
				if (instance == null) {
					lock (syncRoot) {
						if (instance == null)
							instance = new UserPresence();
					}
				}

				return instance;
			}
		}

		public class OpenWrtWifiAssociations {
			public String ifname, encryption, bssid, mode;
			public int quality, noise;
			public String ssid, link, id;
			public int txpoweroff;
			public WifiDevice device;
			public String country;
			public double bitrate;
			public int txpower;
			public String name;
			public int channel;
			public AssocList[] assoclist;
			public int signal;
			public Boolean up;
			public float frequency;

			public class WifiDevice {
				public String device, name, up;
				public WifiDevice() {
				}
			}
			public class AssocList {
				public String Address;
				public Boolean rx_short_gi;
				public int noise, rx_mcs;
				public Boolean tx_40mhz, rx_40mhz;
				public int tx_rate, tx_packets;
				public Boolean tx_short_gi;
				public int rx_packets, tx_mcs, inactive, rx_rate, signal;
				public AssocList() {
				}
			}
		}

		public static List<UserPresence> PresenceList {
			get { return m_presenceList; }
		}
		public static List<User> UserIsNearList {
			get {
				return m_presenceList.FindAll(x => x.Type == UserPresence.PresenceType.Bluetooth || x.Type == UserPresence.PresenceType.Wifi)
					.Select(x => x.User).Distinct().ToList();
			}
		}
		public static String UserIsNearSummary {
			get {
				String result = "";
				foreach (User user in UserIsNearList) {
					result += user.Name + ",";
				}
				return result;
			}
		}
		public UserPresence() {
		}

		public UserPresence(User user, PresenceType presenceType) {
			User = user;
			switch (presenceType) {
				case PresenceType.Bluetooth:
					LastBTActiveDate = DateTime.Now;
					break;
				case PresenceType.Wifi:
					LastWifiActiveDate = DateTime.Now;
					break;
			}
			Type = presenceType;
		}
		public static void AddPresence(User user, PresenceType type) {
			UserPresence up = m_presenceList.Find(x => x.User == user && x.Type == type);
			if (up == null) {
				user.NearbyPresentSince = DateTime.Now;
				user.NearbyPresenceType = type;
				MLog.Log(null, "NEW " + type + " DEVICE FOUND User: " + user.Name);
				switch (type) {
					case PresenceType.Bluetooth:
						m_presenceList.Add(new UserPresence(user, PresenceType.Bluetooth));
						break;
					case PresenceType.Wifi:
						m_presenceList.Add(new UserPresence(user, PresenceType.Wifi));
						break;
				}

				Rules.ExecuteRule(up, "User arrived on " + type + " user=" + user.Name);
			}
			else
				MLog.Log(null, "Error, unexpected user presence found adding usr=" + user);
		}
		//returns true if presence was removed/user left
		public static bool RemovePresence(User user, PresenceType type) {
			Boolean left = false;
			int lostCount = 0;
			UserPresence up = m_presenceList.Find(x => x.User == user && x.Type == type);
			MLog.Assert(up != null, "cannot remove null device " + type);

			switch (type) {
				case PresenceType.Bluetooth:
					up.BTLostContactCount++;
					lostCount = up.BTLostContactCount;
					break;
				case PresenceType.Wifi:
					up.WifiLostContactCount++;
					lostCount = up.WifiLostContactCount;
					break;
			}
			if (lostCount > 4) {
				m_presenceList.Remove(up);
				if (user.NearbyPresenceType == type) {
					user.NearbyPresentSince = DateTime.MaxValue;
				}
				left = true;
			}
			if (left) {
				MLog.Log(null, "DEVICE " + type + " LEFT user: " + user.Name);
				Rules.ExecuteRule(up, "User left on " + type + " user=" + user.Name);
			}
			else
				MLog.Log(null, "DEVICE " + type + " lost contact count= " + lostCount + " user=" + user.Name);

			return left;
		}
		public static void UpdatePresence(User user, PresenceType type) {
			UserPresence up = m_presenceList.Find(x => x.User == user && x.Type == type);
			MLog.Assert(up != null, "presence cannot be null at update " + type);
			switch (type) {
				case PresenceType.Bluetooth:
					up.LastBTActiveDate = DateTime.Now;
					up.BTLostContactCount = 0;
					break;
				case PresenceType.Wifi:
					up.LastWifiActiveDate = DateTime.Now;
					up.WifiLostContactCount = 0;
					break;
			}

		}

		public static void CheckBluetooth() {
			User user;
			List<Bluetooth.Device> currentList, newList, leftList, existingList;
			try {
				currentList = Bluetooth.DiscoverDevices();
				newList = currentList.Except(m_lastBTDeviceList).ToList();
				leftList = m_lastBTDeviceList.Except(currentList).ToList();
				existingList = m_lastBTDeviceList.Intersect(currentList).ToList();

				foreach (Bluetooth.Device dev in newList) {
					user = User.GetUserByBT(dev.Address.ToString());
					if (user != null)
						UserPresence.AddPresence(user, PresenceType.Bluetooth);
					else
						MLog.Log(null, "Unknown user with Bluetooth device=" + dev.DeviceName + " Addr=" + dev.Address);
					m_lastBTDeviceList.Add(dev);
				}
				foreach (Bluetooth.Device dev in leftList) {
					user = User.GetUserByBT(dev.Address.ToString());
					if (user != null && UserPresence.RemovePresence(User.GetUserByBT(dev.Address.ToString()), PresenceType.Bluetooth))
						m_lastBTDeviceList.Remove(dev);
				}
				foreach (Bluetooth.Device dev in existingList) {
					user = User.GetUserByBT(dev.Address.ToString());
					if (user != null)
						UserPresence.UpdatePresence(User.GetUserByBT(dev.Address.ToString()), PresenceType.Bluetooth);
				}
			}
			catch (Exception ex) {
				MLog.Log(null, "Check Bluetooth error " + ex.Message);
			}
		}

		public static void CheckWifi() {
			User user;
			try {
				String url = IniFile.PARAM_ROUTER_HOST[1] + IniFile.PARAM_ROUTER_ACTIVE_WIFI_CLIENTS_URL_CUSTOM[1];
				//WebPostRequest post = new WebPostRequest(IniFile.PARAM_ROUTER_HOST[1] 
					//+ IniFile.PARAM_ROUTER_ACTIVE_WIFI_CLIENTS_URL[1], null);
				//post.Add("username", IniFile.PARAM_ROUTER_USER_NAME[1]);
				//post.Add("password", IniFile.PARAM_ROUTER_USER_PASS[1]);
				
				//String json = post.GetResponse().ToUpper();
				System.Net.WebClient client = new System.Net.WebClient();
                String json= client.DownloadString(url).ToUpper();

				//String token = json.Find("stok=", "/");
				//string logoutReq = IniFile.PARAM_ROUTER_HOST[1] + IniFile.PARAM_ROUTER_LOGOUT_URL[1];
				//logoutReq = logoutReq.Replace("#TOKEN#", token);
				//post = new WebPostRequest(logoutReq, null);
				//post.Add("username", IniFile.PARAM_ROUTER_USER_NAME[1]);
				//post.Add("password", IniFile.PARAM_ROUTER_USER_PASS[1]);
				//String logout = post.GetResponse();
				/*UserPresence.OpenWrtWifiAssociations[] wrt = fastJSON.JSON.Instance.ToObject<UserPresence.OpenWrtWifiAssociations[]>(json);
				if (wrt[0].assoclist != null) {
					foreach (UserPresence.OpenWrtWifiAssociations.AssocList assoc in wrt[0].assoclist){
						user = User.GetUserByWifi(assoc.Address);
					}
				}*/
				String regex = "([0-9A-F]{2}[:-]){5}([0-9A-F]{2})";

				System.Text.RegularExpressions.MatchCollection match = System.Text.RegularExpressions.Regex.Matches(json, regex);
				List<String> currentList, newList, leftList, existingList;
				currentList = new List<string>();
				if (match.Count > 0) {
					foreach (System.Text.RegularExpressions.Match mat in match) {
						currentList.Add(mat.Groups[0].Value);
					}
				}
				newList = currentList.Except(m_lastWifiAddressList).ToList();
				leftList = m_lastWifiAddressList.Except(currentList).ToList();
				existingList = m_lastWifiAddressList.Intersect(currentList).ToList();

				foreach (String dev in newList) {
					user = User.GetUserByWifi(dev);
					if (user != null)
						UserPresence.AddPresence(user, PresenceType.Wifi);
					else
						MLog.Log(null, "Unknown user with Wifi addr=" + dev);
					m_lastWifiAddressList.Add(dev);
				}
				foreach (String dev in leftList) {
					user = User.GetUserByWifi(dev);
					if (user != null && UserPresence.RemovePresence(User.GetUserByWifi(dev), PresenceType.Wifi))
						m_lastWifiAddressList.Remove(dev);
				}
				foreach (String dev in existingList) {
					user = User.GetUserByBT(dev);
					if (user != null)
						UserPresence.UpdatePresence(User.GetUserByWifi(dev), PresenceType.Wifi);
				}
			}
			catch (Exception ex) {
				MLog.Log(null, "CheckWifi error " + ex.Message);
			}
		}
	}

}

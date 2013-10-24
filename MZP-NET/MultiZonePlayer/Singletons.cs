using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace MultiZonePlayer {
	public sealed class Singleton {
		private static volatile Singleton instance;
		private static object syncRoot = new Object();

		private Singleton() { }

		public static Singleton Instance {
			get {
				if (instance == null) {
					lock (syncRoot) {
						if (instance == null)
							instance = new Singleton();
					}
				}

				return instance;
			}
		}
	}


	public sealed class User {
		private static List<User> m_userList = new List<User>();
		public static List<User> UserList {
			get { return User.m_userList; }
		}
		public int Id;
		public String Name;
		public String Code;
		public String PhoneBTAddress;
		public String Email;
		public String MobileNumber;
		public String WifiMACAddress;
		public UserRole Role;
		public DateTime NearbyPresentSince;
		public UserPresence.PresenceType NearbyPresenceType;

		public enum UserRole {
			Owner,
			Family,
			Friend,
			Public
		}

		private static volatile User instance;
		private static object syncRoot = new Object();
		public static User Instance {
			get {
				if (instance == null) {
					lock (syncRoot) {
						if (instance == null)
							instance = new User();
					}
				}

				return instance;
			}
		}

		public User() {
		}

		public User(int id, String name, String code) {
			this.Id = id;
			this.Name = name;
			this.Code = code;
		}
		public String NearbyPresentAgeAsTimeSpan {
			get { return Utilities.DurationAsTimeSpan(NearbyPresentAge); }
		}
		public TimeSpan NearbyPresentAge {
			get { return DateTime.Now.Subtract(NearbyPresentSince); }
		}
		public static User GetUser(int id) {
			return m_userList.Find(x => x.Id == id);
		}
		public static User GetUserByBT(String BTAddress) {
			return m_userList.Find(x => x.PhoneBTAddress == BTAddress);
		}
		public static User GetUserByWifi(String WifiAddress) {
			return m_userList.Find(x => x.WifiMACAddress == WifiAddress);
		}
		public static void Add(User user) {
			m_userList.Add(user);
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

		public static void SaveToIni() {
			String json;
			int r = 0;
			foreach (User user in m_userList) {
				json = fastJSON.JSON.Instance.ToJSON(user);
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
				WebPostRequest post = new WebPostRequest(IniFile.PARAM_ROUTER_HOST[1] + IniFile.PARAM_ROUTER_ACTIVE_WIFI_CLIENTS_URL[1], null);
				post.Add("username", IniFile.PARAM_ROUTER_USER_NAME[1]);
				post.Add("password", IniFile.PARAM_ROUTER_USER_PASS[1]);
				String json = post.GetResponse();
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

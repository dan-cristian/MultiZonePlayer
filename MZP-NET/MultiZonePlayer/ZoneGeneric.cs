using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MultiZonePlayer {
	public class ZoneGeneric : IPlayEvent {
		private User m_zoneUser = null;
		private String m_currentCmd = null;
		private int m_sourceZoneId;
		//private int m_inactiveCyclesCount = 0;
		private IZoneActivity m_mainZoneActivity;
		//private String m_controlDevice = null;
		private ZoneDetails m_zoneDetails;
		private List<ZoneDetails> m_clonedZones;
		private static int m_recIndex = 0;
		private ZonesForm m_zoneForm;
		private DateTime m_lastContactMade = DateTime.MinValue, m_lastContactReleased = DateTime.MinValue;
		private Boolean m_userConfirm = false;
		/// <summary>
		/// Pattern is x-x-xx- where x is a press longer than 1 second and - is a press less than 1 second
		/// </summary>
		private String m_closurePatern = "";

		public User ZoneUser {
			get { return m_zoneUser; }
			set { m_zoneUser = value; }
		}

		public String CurrentCmd {
			get { return m_currentCmd; }
			set { m_currentCmd = value; }
		}

		/*public int InactiveCyclesCount {
			get { return m_inactiveCyclesCount; }
			set { m_inactiveCyclesCount = value; }
		}*/

		public IZoneActivity MainZoneActivity {
			get { return m_mainZoneActivity; }
			set { m_mainZoneActivity = value; }
		}
		/*
		public String ControlDevice {
			get { return m_controlDevice; }
			set { m_controlDevice = value; }
		}*/

		public ZoneDetails ZoneDetails {
			get { return m_zoneDetails; }
		}

		public ZonesForm ZoneForm {
			get { return m_zoneForm; }
			set { m_zoneForm = value; }
		}

		public ZoneGeneric(ZoneDetails zone) {
			m_zoneDetails = zone;
			this.m_sourceZoneId = zone.ZoneId;
			m_clonedZones = new List<ZoneDetails>();
			//default user - all
			m_zoneUser = new User(0, "all", "000"); //SystemState.iniUserList["000"] as User;
			LoadZoneIni();
		}

		public void CloseZone() {
			try {
				MLog.Log(null, "Closing zone " + m_zoneDetails.ZoneId + ", activity=" + m_mainZoneActivity);
				if (m_mainZoneActivity != null) {
					m_mainZoneActivity.Close();
				}
				m_zoneDetails.Close();
				//MZPState.Instance.ActiveZones.Remove(this);
			}
			catch (Exception ex) {
				MLog.Log(ex, "Error closing generic zone " + m_zoneDetails.ZoneName);
			}
		}

		private void StopRemoveClonedZones() {
			ZoneGeneric zone;
			foreach (ZoneDetails izone in m_clonedZones) {
				zone = ZoneDetails.ActiveZones.Find(x => x.ZoneDetails.ZoneId == izone.ZoneId);
				if (zone != null && zone.MainZoneActivity != null) {
					zone.MainZoneActivity.Stop();
				}
			}
			m_clonedZones.Clear();
		}

		private void InitZoneMusic() {
			MLog.Log(this, "Initialising music");
			if ((m_mainZoneActivity == null) || (m_mainZoneActivity.GetType() != typeof (MusicActivity))) {
				//ControlCenter.Instance.OpenZone(m_zoneDetails.ZoneId);
				StopRemoveClonedZones();

				m_mainZoneActivity = new MusicActivity(this);
				m_zoneDetails.ActivityType = GlobalCommands.music;
				//m_zoneDetails.IsActive = true;

				AddClonedZone(m_mainZoneActivity.ZoneDetails);
			}
			else {
				MLog.Log(this, "Initialising music not needed, already init");
			}
		}

		private void InitZoneMusicClone() {
			MLog.Log(null, "Initialising music clone");
			///ControlCenter.Instance.OpenZone(m_zoneDetails.ZoneId);
			StopRemoveClonedZones();

			ZoneGeneric clonesource = ZoneDetails.ActiveZones.Find
				(x => x.ZoneDetails.ActivityType == GlobalCommands.musicclone);
			if (clonesource != null) {
				m_mainZoneActivity = new ZoneMusicClone(this, ((ZoneMusicClone) clonesource.MainZoneActivity).CloneSourceZone);
			}
			else {
				m_mainZoneActivity = new ZoneMusicClone(this, MZPState.Instance.GetFirstZoneMusic());
			}
			m_zoneDetails.ActivityType = GlobalCommands.musicclone;
			//m_zoneDetails.IsActive = true;
		}

		private void InitZoneRadio() {
			MLog.Log(null, "Initialising radio");
			// input zone 0 is Radio
			//ControlCenter.Instance.OpenZone(m_zoneDetails.ZoneId);
			String inDevice = (String) MZPState.Instance.zoneDefaultInputs["0"];
			m_mainZoneActivity = new ZoneRadio(this, inDevice, m_zoneDetails.OutputDeviceAutoCompleted());
			m_zoneDetails.ActivityType = GlobalCommands.radio;
			//m_zoneDetails.IsActive = true;
			m_mainZoneActivity.Play();
		}

		private void InitZoneStreamVLC() {
			MLog.Log(null, "Initialising mp3 VLC stream");
			//ControlCenter.Instance.OpenZone(m_zoneDetails.ZoneId);
			m_mainZoneActivity = new ZoneStreamVLC(m_zoneDetails);
			m_zoneDetails.ActivityType = GlobalCommands.streammp3;
			//m_zoneDetails.IsActive = true;
			//m_mainZoneActivity.Play();
		}

		private void InitZoneMic() {
			MLog.Log(null, "Initialising microphone");
			//ControlCenter.Instance.OpenZone(m_zoneDetails.ZoneId);
			String inDevice = (String) MZPState.Instance.zoneDefaultInputs[m_zoneDetails.ZoneId];
			m_mainZoneActivity = new ZoneMic(this, inDevice, m_zoneDetails.OutputDeviceAutoCompleted());
			m_zoneDetails.ActivityType = GlobalCommands.microphone;
			//m_zoneDetails.IsActive = true;
		}

		private void InitZoneVideoApp() {
			MLog.Log(null, "Initialising video");
			//ControlCenter.Instance.OpenZone(m_zoneDetails.ZoneId);
			//m_mainZoneActivity = new ZoneVideoVlc(this);
			m_mainZoneActivity = new ZoneVideoMPC(this);
			m_zoneDetails.ActivityType = GlobalCommands.video;
			// m_zoneDetails.IsActive = true;
		}

		private void InitZoneDisplayTV() {
			MLog.Log(null, "Initialising TV");
			//ControlCenter.Instance.OpenZone(m_zoneDetails.ZoneId);
			//m_mainZoneActivity = new ZoneVideoVlc(this);
			m_mainZoneActivity = new ZoneDisplayLG(this.ZoneDetails);
			m_zoneDetails.LastLocalCommandDateTime = DateTime.Now;
			m_zoneDetails.ActivityType = GlobalCommands.tv;
			//m_zoneDetails.IsActive = true;
		}

		private void InitZonePlayerXBMC() {
			MLog.Log(null, "Initialising XBMC zone");
			//ControlCenter.Instance.OpenZone(m_zoneDetails.ZoneId);
			//m_mainZoneActivity = new ZoneVideoVlc(this);
			if (m_zoneDetails.DisplayConnection != "") {
				m_mainZoneActivity = new ZonePlayerXBMC(this.ZoneDetails);
				m_zoneDetails.ActivityType = GlobalCommands.xbmc;
				//m_zoneDetails.IsActive = true;
			}
			else {
				MLog.Log(this, "no display connection details, zone not initialised " + m_zoneDetails.ZoneName);
			}
		}

		public void ProcessAction(GlobalCommands cmdRemote, ValueList vals, ref CommandResult cmdresult) {
			//Metadata.ValueList result = new Metadata.ValueList();
			String cmdSource = vals.GetValue(GlobalParams.cmdsource);
			CommandSources cmdSourceEnum;
			if (cmdSource == null) {
				cmdSourceEnum = vals.CommandSource;
			}
			else {
				cmdSourceEnum = (CommandSources) Enum.Parse(typeof (CommandSources), cmdSource);
			}

			//reset inactivity counter
			//m_inactiveCyclesCount = -1;

			if (CommandSyntax.UserCommands.Contains(cmdRemote)) {
				m_zoneDetails.LastLocalCommandDateTime = DateTime.Now;
			}
			if (m_zoneDetails.WaitForControlDeviceSetup) {
				//cancelling ctrl device setup, should have been done by now in parent API call
				m_zoneDetails.WaitForControlDeviceSetup = false;
			}
			String date, weekday, action;
			switch (cmdRemote) {
					#region commands without activity

				case GlobalCommands.cameraevent: //video camera event
					ZoneOpenActions();
					m_zoneDetails.MovementAlert = true;
					m_zoneDetails.IncreaseCameraCountIfArmed();
					m_zoneDetails.LastCamAlertDateTime = DateTime.Now;
					MZPState.Instance.ZoneEvents.AddCamAlert(vals);
					String camId = vals.GetValue(GlobalParams.oid);
					string message = "Cam alert from camid=" + camId + " zone is " + m_zoneDetails.ZoneName;
					if (m_zoneDetails.IsArmed || (MZPState.Instance.SystemAlarm.IsArmed && MZPState.Instance.SystemAlarm.AreaId==m_zoneDetails.AlarmAreaId)) {
						Alert.CreateAlert(message, m_zoneDetails, false, null, Alert.NotificationFlags.NeedsImmediateUserAck, 3);
					}
					//MZPState.Instance.LogEvent(MZPEvent.EventSource.Cam, message, 
					//	MZPEvent.EventType.Security, MZPEvent.EventImportance.Informative, m_zoneDetails);

					m_zoneDetails.MovementAlert = false;

					if (MZPState.Instance.IsFollowMeMusic & m_zoneDetails.HasSpeakers) {
						cmdRemote = GlobalCommands.musicclone;
					}

					Utilities.AppendToCsvFile(IniFile.CSV_CLOSURES, ",", m_zoneDetails.ZoneName, camId,
						DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT), "CamMove", m_zoneDetails.ZoneId.ToString(),
						Constants.EVENT_TYPE_CAMALERT);
					SaveCurrentPicture(EventSource.Cam);
					//TODO
					break;
				case GlobalCommands.togglecameraalert:
					MZPState.Instance.ZoneEvents.ToggleAlertStatus(vals);
					break;
				case GlobalCommands.alarmevent: //alarm sensor event
					ZoneOpenActions();
					m_zoneDetails.MovementAlert = true;
					m_zoneDetails.IncreaseAlarmsensorCountIfArmed();
					String zonestate = vals.GetValue(GlobalParams.status);
					m_zoneDetails.MovementAlert = zonestate.Equals(Alarm.EnumZoneState.opened.ToString());//sensor is first opened then closed
					DateTime eventDateTime = Convert.ToDateTime(vals.GetValue(GlobalParams.datetime));
					m_zoneDetails.LastAlarmMovementDateTime = eventDateTime;

					if (m_zoneDetails.IsArmed || (MZPState.Instance.SystemAlarm.IsArmed && m_zoneDetails.AlarmAreaId == MZPState.Instance.SystemAlarm.AreaId)) {
						Alert.CreateAlert(
							vals.GetValue(GlobalParams.action) + " ZoneEvent " + m_zoneDetails.ZoneName + " is " +
							vals.GetValue(GlobalParams.status),
							m_zoneDetails, false, null, Alert.NotificationFlags.NeedsImmediateUserAck, 3);
					}
					//MZPState.Instance.LogEvent(eventDateTime, MZPEvent.EventSource.Alarm,
					//        vals.GetValue(GlobalParams.action) + " ZoneEvent " + m_zoneDetails.ZoneName + " is " + vals.GetValue(GlobalParams.status),
					//        MZPEvent.EventType.Security, MZPEvent.EventImportance.Informative, m_zoneDetails);

					Utilities.AppendToCsvFile(IniFile.CSV_CLOSURES, ",", m_zoneDetails.ZoneName, "",
						eventDateTime.ToString(IniFile.DATETIME_FULL_FORMAT), "Sensor" + zonestate, m_zoneDetails.ZoneId.ToString(),
						Constants.EVENT_TYPE_SENSORALERT);
					if (m_zoneDetails.HasCamera) {
						SaveCurrentPicture(EventSource.Alarm);
					}

					if (MZPState.Instance.IsFollowMeMusic & m_zoneDetails.HasSpeakers) {
						cmdRemote = GlobalCommands.musicclone;
					}
					break;
				case GlobalCommands.setwaketimer:
					date = vals.GetValue(GlobalParams.datetime);
					weekday = vals.GetValue(GlobalParams.weekday);
					m_zoneDetails.WakeTime = date == null ? "" : date;
					m_zoneDetails.WakeWeekDay = weekday == null ? "" : weekday;
					break;
				case GlobalCommands.sleeptimer:
					date = vals.GetValue(GlobalParams.datetime);
					if (date.Equals("")) {
						m_zoneDetails.SleepHourMin = "";
					}
					else {
						m_zoneDetails.SleepHourMin = Convert.ToDateTime(date).ToString(IniFile.DATETIME_DAYHR_FORMAT);
					}

					break;
				case GlobalCommands.micrecord:
					m_recIndex++;
					Utilities.WriteBinaryFile("\\webroot\\direct\\zone" + m_zoneDetails.ZoneId + "_" + m_recIndex + ".wav",
						vals.BinaryData);
					break;
				case GlobalCommands.micplay:
					InitZoneMusic();
					MusicActivity music = (MusicActivity) m_mainZoneActivity;
					music.Play(IniFile.CurrentPath() + "\\webroot\\direct\\zone" + m_zoneDetails.ZoneId + "_" + m_recIndex + ".wav");
					break;
				case GlobalCommands.zonearm:
				case GlobalCommands.zonedisarm:
					m_zoneDetails.IsArmed = (cmdRemote == GlobalCommands.zonearm);
					cmdresult.OutputMessage += "Zone " + m_zoneDetails.ZoneName + " armed status=" + m_zoneDetails.IsArmed;
					break;
				case GlobalCommands.powercycle:
					m_zoneDetails.PowerControlOn();
					Thread.Sleep(Convert.ToInt16(vals.GetValue(GlobalParams.interval)));
					m_zoneDetails.PowerControlOff();
					break;
				case GlobalCommands.heatscheduleon:
					m_zoneDetails.ScheduledHeatActive = true;
					break;
				case GlobalCommands.heatscheduleoff:
					m_zoneDetails.ScheduledHeatActive = false;
					break;
				case GlobalCommands.poweron:
					MLog.Log(this, "Permanent power ON zone=" + m_zoneDetails.ZoneName);
					m_zoneDetails.RequirePowerForced = true;
					m_zoneDetails.PowerControlOn();
					cmdresult.OutputMessage += "power on ok";
					break;
				case GlobalCommands.poweroff:
					MLog.Log(this, "Permanent power OFF zone=" + m_zoneDetails.ZoneName);
					m_zoneDetails.PowerControlOff();
					m_zoneDetails.RequirePowerForced = false;
					cmdresult.OutputMessage += "power off ok";
					break;
				case GlobalCommands.powertoggle:
					MLog.Log(this, "Power toggle zone=" + m_zoneDetails.ZoneName);
					if (m_zoneDetails.IsPowerOn) {
						m_zoneDetails.PowerControlOff();
						m_zoneDetails.RequirePowerForced = false;
						cmdresult.OutputMessage += "power toggled OFF";
					}
					else {
						m_zoneDetails.PowerControlOn();
						m_zoneDetails.RequirePowerForced = true;
						cmdresult.OutputMessage += "power toggled ON";
					}
					break;
				case GlobalCommands.closure:
				case GlobalCommands.pulse:
					Boolean contactMade = vals.GetValue(GlobalParams.iscontactmade).ToLower() == "true";
					double contactMadeDuration, contactReleasedDuration;
					if (contactMade) {
						contactReleasedDuration = DateTime.Now.Subtract(m_lastContactReleased).TotalSeconds;
						m_lastContactMade = DateTime.Now;
					}
					else {
						contactMadeDuration = DateTime.Now.Subtract(m_lastContactMade).TotalSeconds;
						m_lastContactReleased = DateTime.Now;
						if (contactMadeDuration > 1) {
							m_closurePatern += "x";
						}
						else {
							m_closurePatern += "-";
						}
					}

					//MLog.Log(this, "Closure Patern="+m_closurePatern + " on zone " + m_zoneDetails.ZoneName);
					String key = vals.GetValue(GlobalParams.id);
					MLog.Log(this, "ClosureEvent zone=" + m_zoneDetails.ZoneName + " key=" + key + " contactmade=" + contactMade);
					Boolean isContactMadeLast = m_zoneDetails.IsClosureContactMade;
					m_zoneDetails.IsClosureContactMade = contactMade;
					m_zoneDetails.MovementAlert = true;
					m_zoneDetails.IncreaseClosureCountIfArmed();
					ZoneOpenActions();

					switch (m_zoneDetails.ClosureType) {
							case EnumClosureType.PresenceContact:
							case EnumClosureType.Contact:
							case EnumClosureType.LockZoneContact:
								string msg = "Closure contact state " + key + " is " + contactMade
									 + " on zone " + m_zoneDetails.ZoneName;
								MLog.Log(this, msg);
								Utilities.AppendToCsvFile(IniFile.CSV_CLOSURES, ",", m_zoneDetails.ZoneName, key,
									DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT), m_zoneDetails.RelayState.ToString(),
									m_zoneDetails.ZoneId.ToString(), Constants.EVENT_TYPE_CLOSURE, key);

								if (contactMade) {
									if (m_zoneDetails.IsArmed ||
										(MZPState.Instance.SystemAlarm.IsArmed && MZPState.Instance.SystemAlarm.AreaId == m_zoneDetails.AlarmAreaId)) {
										Alert.CreateAlert(msg, m_zoneDetails, false, null, Alert.NotificationFlags.NeedsImmediateUserAck, 3);
									}
								}	
								break;
							case EnumClosureType.Pulse:
								m_zoneDetails.RecordPulse(key);
								break;
						default:
							MLog.Log(this, "Error, undefined closure type for zone="+m_zoneDetails.ZoneName);
							break;
					}
			
					m_zoneDetails.MovementAlert = false;
					//ClosureEvent(vals.GetValue(GlobalParams.id), contactMade);
					User user = User.UserList.Find(x => x.GetMacroNameByPattern(m_closurePatern, m_zoneDetails.ZoneId) != null);
					if (user != null) {
						String macro = user.GetMacroNameByPattern(m_closurePatern, m_zoneDetails.ZoneId);
						MLog.Log(this, "User " + user.Name + " executed macro from closure " + macro);
						MacroEntry.ExecuteMacro(macro);
					}
					if (m_zoneDetails.HasCamera) {
						SaveCurrentPicture(EventSource.Closure);
					}
					break;
				case GlobalCommands.closurearm:
				case GlobalCommands.closuredisarm:
					m_zoneDetails.IsClosureArmed = (cmdRemote == GlobalCommands.closurearm);
					cmdresult.OutputMessage += "Zone " + m_zoneDetails.ZoneName + " closure armed status=" +
					                           m_zoneDetails.IsClosureArmed;
					break;
				case GlobalCommands.closureopen:
					if (m_zoneDetails.HasWDIORelay) {
						MZPState.Instance.WDIO.SetOutputLow(m_zoneDetails.WDIORelayOutputPinIndex);
					}
					else {
						int closureid = Convert.ToInt16(vals.GetValue(GlobalParams.id));
						m_zoneDetails.SetClosureCmdIO(closureid, true);
					}
					break;
				case GlobalCommands.closureclose:
					if (m_zoneDetails.HasWDIORelay) {
						MZPState.Instance.WDIO.SetOutputHigh(m_zoneDetails.WDIORelayOutputPinIndex);
					}
					else {
						int closureid = Convert.ToInt16(vals.GetValue(GlobalParams.id));
						m_zoneDetails.SetClosureCmdIO(closureid, false);
					}
					break;
				case GlobalCommands.closuretoggle:
					if (m_zoneDetails.HasWDIORelay) { 
						if (m_zoneDetails.IsClosureContactMade)
							MZPState.Instance.WDIO.SetOutputLow(m_zoneDetails.WDIORelayOutputPinIndex);
						else
							MZPState.Instance.WDIO.SetOutputHigh(m_zoneDetails.WDIORelayOutputPinIndex);
					}
					else {
						int closureid = Convert.ToInt16(vals.GetValue(GlobalParams.id));	
						m_zoneDetails.SetClosureCmdIO(closureid, !m_zoneDetails.IsClosureCmdIOOn(closureid));
					}
					break;
				case GlobalCommands.counter:
					ulong counter = 0;
					string id;
					ulong.TryParse(vals.GetValue(GlobalParams.count), out counter);
					id = vals.GetValue(GlobalParams.id);
					CounterEvent(id, counter);
					break;
				case GlobalCommands.notifyuser:
					if (m_zoneDetails.HasSpeakers) {
						bool needsPower = m_zoneDetails.RequirePowerForced;
						m_zoneDetails.RequirePowerForced = true;
						if (!MZPState.Instance.PowerControlIsOn(m_zoneDetails.ZoneId)) {
							m_zoneDetails.PowerControlOn();
							System.Threading.Thread.Sleep(m_zoneDetails.PowerOnDelay); //ensure we can hear this
						}
						string sourcezoneid = vals.GetValue(GlobalParams.sourcezoneid);
						string file = IniFile.CurrentPath() + IniFile.PARAM_NOTIFYUSER_SOUND_FILE[1].Replace("x", sourcezoneid);
						if (!System.IO.File.Exists(file)) {
							file = IniFile.CurrentPath() + IniFile.PARAM_NOTIFYUSER_SOUND_FILE[1];
						}
						DCPlayer tempPlay = new DCPlayer(this, file, m_zoneDetails.GetDefaultNotifyUserVolume());
						//System.Threading.Thread.Sleep(4000);//ensure we can hear this
						m_zoneDetails.RequirePowerForced = needsPower;
					}
					else {
						MLog.Log(this, "Error, notify user on zone without speakers=" + m_zoneDetails.ZoneName);
					}
					break;

				case GlobalCommands.generategraph:
					int ageHours = Convert.ToInt16(vals.GetValue(GlobalParams.interval));
					string type = vals.GetValue(GlobalParams.type).ToLower();
					int direction = 0;
							if (vals.GetValue(GlobalParams.direction) != null)
								direction = Convert.ToInt16(vals.GetValue(GlobalParams.direction));
					SimpleGraph graph = new SimpleGraph(type == "temphum", type == Constants.EVENT_TYPE_CLOSURE, type == Constants.CAPABILITY_VOLTAGE,
						type == Constants.CAPABILITY_ELECTRICITY || type == Constants.CAPABILITY_WATER, type == Constants.CAPABILITY_ERROR);
					List<int> zoneIdList = new List<int>();
					zoneIdList.Add(m_zoneDetails.ZoneId);
					if (type == "temphum") {
						graph.ShowTempHumGraph(m_zoneDetails.ZoneId, ageHours, direction);
					}
					if (type == Constants.EVENT_TYPE_CLOSURE) {
						graph.ShowEventGraph(m_zoneDetails.ZoneId, ageHours, direction);
					}
					if (type == Constants.CAPABILITY_VOLTAGE) {
						graph.ShowVoltageGraph( m_zoneDetails.ZoneId.ToString(), zoneIdList, ageHours, false, direction);
					}
					if (type == Constants.CAPABILITY_ELECTRICITY || type == Constants.CAPABILITY_WATER) {
						graph.ShowUtilitiesGraph(m_zoneDetails.ZoneId, m_zoneDetails.ZoneName, ageHours, type, direction);
					}
					if (type == Constants.CAPABILITY_ERROR) {
						graph.ShowErrorGraph(ageHours, m_zoneDetails.ZoneId, false, direction);
					}
					break;
				case GlobalCommands.doorring:
					List<ZoneDetails> zonesToNotify = MultiZonePlayer.ZoneDetails.ZoneWithPotentialUserPresence_All;
					//int sourcezoneid = Convert.ToInt16(vals.GetValue(GlobalParams.sourcezoneid));
					Alert.CreateAlert("Entry Door Ring", m_zoneDetails, false, null, Alert.NotificationFlags.NeedsImmediateUserAck, 120);
					if (m_zoneDetails.HasCamera) {
						SaveCurrentPicture(EventSource.Closure);
					}
					ValueList val1 = new ValueList();
					val1.Add(GlobalParams.command, GlobalCommands.notifyuser.ToString());
					foreach (ZoneDetails zone in zonesToNotify) {
						val1.Set(GlobalParams.zoneid, zone.ZoneId.ToString());
						val1.Set(GlobalParams.sourcezoneid, m_zoneDetails.ZoneId.ToString());
						API.DoCommand(val1);
					}
					break;
				case GlobalCommands.doorentry:
					zonesToNotify = MultiZonePlayer.ZoneDetails.ZoneWithPotentialUserPresence_All;
					//sourcezoneid = Convert.ToInt16(vals.GetValue(GlobalParams.sourcezoneid));
					zonesToNotify.RemoveAll(x => x.ZoneId == m_zoneDetails.ZoneId);
					ValueList val2 = new ValueList();
					val2.Add(GlobalParams.command, GlobalCommands.notifyuser.ToString());
					foreach (ZoneDetails zone in zonesToNotify) {
						if (!m_zoneDetails.IsNearbyZone(zone.ZoneId)
						    && !zone.IsNearbyZone(m_zoneDetails.ZoneId)) {
							val2.Set(GlobalParams.zoneid, zone.ZoneId.ToString());
							val2.Set(GlobalParams.sourcezoneid, m_zoneDetails.ZoneId.ToString());
							API.DoCommand(val2);
						}
					}
					if (m_zoneDetails.HasCamera) {
						SaveCurrentPicture(EventSource.Closure);
					}
					break;
				case GlobalCommands.userconfirm:
					m_userConfirm = true;
					break;
				case GlobalCommands.zoneautosetup:
					//TODO: power on all amps
					foreach (ZoneDetails zone in ZoneDetails.ZoneDetailsList) {
						zone.RequirePowerForced = true;
						zone.PowerControlOn();
						Text2Speech.PlayMessage("Power on in zone " + ZoneName, zone.ZoneGeneric);
					}
					//Identifying associated sound card
					/*
					foreach (soundcard in cardlist) {
						m_userConfirm = false;
						Text2Speech.PlayMessage("Press button confirm in maximum 5 seconds after you hear the sound", zone.ZoneGeneric);
						DateTime starttime = DateTime.Now;
						do {
							Thread.Sleep(100);
						}
						while (m_userConfirm == false && DateTime.Now.Subtract(starttime).TotalSeconds<=5);
						//todo: pick selected raw input device
						//todo: repeat selection for multiple devices
					}
					*/
					//TODO: play sound in each sound card and wait for user confirmation via web
					
					//identify raw inputs
					//TODO: ask user to press keyboard in the zone and identify pressed raw input
					//todo: remove invalid raw inpt devs
					m_userConfirm = false;
					Text2Speech.PlayMessage("Press button confirm from your device in maximum 5 seconds after you hear the sound", m_zoneDetails.ZoneGeneric);
					
					//associate amp power index
					//TODO: close all amps and open each one after another. ask user to press key when can hear sound and set power index
					foreach (ZoneDetails zone in ZoneDetails.ZoneDetailsList) {
						zone.RequirePowerForced = false;
					}
					//TODO: skip elements for zones auto configured or put them as last options?
					break;
				case GlobalCommands.waitforcontroldevice:
					m_zoneDetails.WaitForControlDeviceSetup = true;
					break;
			#endregion

				default:

					action = vals.GetValue(GlobalParams.action);
					MusicActivity zMusic;
					switch (cmdRemote) {
							#region Zone init commands

						case GlobalCommands.music:
						case GlobalCommands.musicalarm:
							//if cmd is music, if not on Music already stop current activity and init Music
							if ((m_mainZoneActivity != null) && (m_mainZoneActivity.GetType() != typeof (MusicActivity))) {
								m_mainZoneActivity.Close();
								m_mainZoneActivity = null;
							}

							if (m_mainZoneActivity == null) {
								InitZoneMusic();
								zMusic = (MusicActivity) m_mainZoneActivity;
								//MoodMusic mood = MZPState.Instance.MoodMusicList.Find(x => x.Name.Equals("default"));
								//if (mood != null)
								//    zMusic.SetMood(mood);
							}
							zMusic = (MusicActivity) m_mainZoneActivity;

							if (m_mainZoneActivity != null) {
								switch (cmdRemote) {
									case GlobalCommands.musicalarm:
										if (!zMusic.IsAlarm) //(!m_mainZoneActivity.GetState().Equals(Metadata.ZoneState.Running))
										{
											zMusic.IsAlarm = true;
											//set musicalarm mood if exists
											MoodMusic mood =
												MZPState.Instance.MoodMusicList.Find(x => x.Name.Equals(GlobalCommands.musicalarm.ToString()));
											if (mood != null) {
												zMusic.SetMood(mood);
											}
											m_mainZoneActivity.Play();
										}
										break;
									case GlobalCommands.music: //play once opened unless is playing, cmd is from web or explicit play
										if (zMusic.GetState() != ZoneState.Running
										    &&
										    (!cmdSourceEnum.Equals(CommandSources.web) ||
										     (action != null && action.Equals(GlobalCommands.play.ToString())))) {
											zMusic.Play();
										}
										break;
								}
							}
							break;

						case GlobalCommands.streammp3:
						case GlobalCommands.radio:
							//check if is stream cmd
							if ((m_mainZoneActivity != null) && (m_mainZoneActivity.GetType() != typeof (ZoneStreamVLC))) {
								m_mainZoneActivity.Close();
								m_mainZoneActivity = null;
							}
							if (m_mainZoneActivity == null) {
								InitZoneStreamVLC();
							}

							if (!vals.CommandSource.Equals(CommandSources.web) ||
							    (action != null && action.Equals(GlobalCommands.play.ToString()))) {
								m_mainZoneActivity.Play();
							}
							break;
							/*
                    case Metadata.GlobalCommands.dvd:
                        //check if Microphone

                        if ((m_mainZoneActivity != null) && (m_mainZoneActivity.GetType() != typeof(ZoneMic)))
                        {
                            m_mainZoneActivity.Close();
                            m_mainZoneActivity = null;
                        }
                        if (m_mainZoneActivity == null)
                            InitZoneMic();
                        m_mainZoneActivity.Play();
                        break;*/

							//check if video
							/*
                    case Metadata.GlobalCommands.video:
                        if ((m_mainZoneActivity != null) && (m_mainZoneActivity.GetType() != typeof(ZoneVideoMPC)))
                        {
                            m_mainZoneActivity.Close();
                            m_mainZoneActivity = null;
                        }

                        if (m_mainZoneActivity == null)
                            InitZoneVideoApp();
                        break;
                        */
							//check if photo (clone zone)
						case GlobalCommands.photo:
						
						case GlobalCommands.musicclone:
							if (MZPState.Instance.GetFirstZoneMusic() != null && !this.Equals(MZPState.Instance.GetFirstZoneMusic()))
								//cannot clone myself
							{
								if ((m_mainZoneActivity != null) && (m_mainZoneActivity.GetType() != typeof (ZoneMusicClone))) {
									m_mainZoneActivity.Close();
									m_mainZoneActivity = null;
								}

								if (m_mainZoneActivity == null) {
									InitZoneMusicClone();
								}
							}
							else {
								MLog.Log(this, "Cannot clone myself or no existing zone to clone");
							}
							break;

							//check if TV
						case GlobalCommands.tv:
							if ((m_mainZoneActivity != null) && (m_mainZoneActivity.GetType() != typeof (ZoneDisplayLG))) {
								m_mainZoneActivity.Close();
								m_mainZoneActivity = null;
							}

							if (m_mainZoneActivity == null) {
								InitZoneDisplayTV();
							}

							if (action != null && action.Equals(GlobalCommands.play.ToString())) {
								m_mainZoneActivity.Play();
							}
							break;

							//check if XBMC
						case GlobalCommands.xbmc:
						case GlobalCommands.video:
							if ((m_mainZoneActivity != null) && (m_mainZoneActivity.GetType() != typeof (ZonePlayerXBMC))) {
								m_mainZoneActivity.Close();
								m_mainZoneActivity = null;
							}

							if (m_zoneDetails.ZoneState==ZoneState.NotInitialised || m_mainZoneActivity == null && m_zoneDetails.HasVideoPlayer) {
								InitZonePlayerXBMC();
							}
							else {
								MLog.Log(this, "Not initialising the video Mainactivity is null=" + (m_mainZoneActivity == null)
								               + " missing video player on zone=" + m_zoneDetails.ZoneName + " hasvideo=" +
								               m_zoneDetails.HasVideoPlayer);
							}

							if (action != null && action.Equals(GlobalCommands.play.ToString())) {
								m_mainZoneActivity.Play();
							}
							break;
						default:
							if (m_mainZoneActivity == null) {
								cmdresult.OutputMessage += "No suitable zoneactivity for cmd=" + cmdRemote + " zone=" + m_zoneDetails.ZoneName;
								MLog.Log(this, cmdresult.OutputMessage);
								return;
							}

							#endregion

							switch (cmdRemote) {
									#region Generic commands

								case GlobalCommands.play:
									m_mainZoneActivity.Play();
									break;
								case GlobalCommands.back://as stop key does not work on all remotes
								case GlobalCommands.stop:
									m_mainZoneActivity.Stop();
									//foreach (m_
									break;
								case GlobalCommands.pause:
									m_mainZoneActivity.Pause();
									break;
								case GlobalCommands.next:
									m_mainZoneActivity.Next();
									break;
								case GlobalCommands.previous:
									m_mainZoneActivity.Previous();
									break;

								case GlobalCommands.mute:
									m_mainZoneActivity.Mute();
									break;
								case GlobalCommands.chup:
								case GlobalCommands.volumeup:
									m_mainZoneActivity.VolumeUp();
									if (cmdSourceEnum.Equals(CommandSources.mobileslow)) {
										m_mainZoneActivity.VolumeUp();
									}
									break;
								case GlobalCommands.chdown:
								case GlobalCommands.volumedown:
									m_mainZoneActivity.VolumeDown();
									if (cmdSourceEnum.Equals(CommandSources.mobileslow)) {
										m_mainZoneActivity.VolumeDown();
									}
									break;
								case GlobalCommands.volumeset:
									m_mainZoneActivity.SetVolumeLevel(Convert.ToInt16(vals.GetValue(GlobalParams.volumelevel)));
									break;
								case GlobalCommands.guide:
									m_mainZoneActivity.Guide();
									break;

									#endregion

								default:

									#region specific commands

									//specific commands only to Music / Clone
									if (m_mainZoneActivity.GetType() == typeof (MusicActivity)) {
										zMusic = ((MusicActivity) m_mainZoneActivity);
										zMusic.ProcessAction(cmdRemote, vals, ref cmdresult);
									}

									//music clone commands
									if (m_mainZoneActivity.GetType() == typeof (ZoneMusicClone)) {
										ZoneMusicClone zMusicClone = ((ZoneMusicClone) m_mainZoneActivity);
										zMusicClone.ProcessAction(cmdRemote, vals, ref cmdresult);
									}

									//specific commands only to internettream
									if (m_mainZoneActivity.GetType() == typeof (ZoneStreamVLC)) {
										ZoneStreamVLC zStream = (ZoneStreamVLC) m_mainZoneActivity;

										switch (cmdRemote) {
											case GlobalCommands.enter: //for numpads
												zStream.Next();
												break;
											case GlobalCommands.right:
												m_mainZoneActivity.NextPlaylist();
												break;
											case GlobalCommands.left:
												m_mainZoneActivity.PreviousPlaylist();
												break;
										}
									}

									//specific commands only to TV
									if (m_mainZoneActivity.GetType() == typeof (ZoneDisplayLG)) {
										((ZoneDisplayLG) m_mainZoneActivity).ProcessAction(cmdRemote, vals, ref cmdresult);
									}

									//specific commands only to XBMC
									if (m_mainZoneActivity.GetType() == typeof (ZonePlayerXBMC)) {
										((ZonePlayerXBMC) m_mainZoneActivity).ProcessAction(cmdRemote, vals, ref cmdresult);
									}

									//specific commands only to Video
									if (m_mainZoneActivity.GetType() == typeof (ZoneVideoMPC)) {
										ZoneVideoMPC zVideo = (ZoneVideoMPC) m_mainZoneActivity;
										switch (cmdRemote) {
											case GlobalCommands.fullscreen:
												zVideo.GoToFullScreen();
												break;
											case GlobalCommands.down:
												zVideo.VideoSelectionDown();
												break;
											case GlobalCommands.up:
												zVideo.VideoSelectionUp();
												break;
											case GlobalCommands.right:
												zVideo.NextSearchCriteria();
												break;
											case GlobalCommands.left:
												zVideo.PreviousSearchCriteria();
												break;
											case GlobalCommands.enter:
												zVideo.ToggleSearchCriteria();
												break;
											case GlobalCommands.ffwd:
												zVideo.Ffwd();
												break;
											case GlobalCommands.rewind:
												zVideo.Rewind();
												break;
										}
									}
									break;

									#endregion specific commands
							}
							break;
					}
					break;
			}

			this.m_currentCmd = cmdRemote.ToString();
			if (m_mainZoneActivity != null) {
				m_mainZoneActivity.Tick(); //update zone details after command
			}
			//RefreshState();
			return;
		}

		public IZoneActivity GetCurrentActivity() {
			return m_mainZoneActivity;
		}

		public int GetZoneId() {
			return m_zoneDetails.ZoneId;
		}

		public String ZoneName {
			get { return m_zoneDetails.ZoneName; }
		}

		public List<ZoneDetails> GetClonedZones() {
			return m_clonedZones;
			//return m_zoneDetails.OutputDevice;
		}

		public void AddClonedZone(ZoneDetails zone) {
			m_clonedZones.Add(zone);
		}

		public void RemoveClonedZone(ZoneDetails zone) {
			m_clonedZones.Remove(zone);
		}

		public User GetUser() {
			return m_zoneUser;
		}

		public void SetSourceZone(int zoneId) {
			m_sourceZoneId = zoneId;
		}

		public int GetVolumeLevel() {
			if (m_mainZoneActivity != null) {
				return m_mainZoneActivity.GetVolumeLevel();
			}
			else {
				return m_zoneDetails.GetDefaultVolume();
			}
		}

		public void EventNextAuto() {
			m_mainZoneActivity.Next();
		}

		public void EventStop() {
			//m_mainZoneActivity.s();
		}

		private void LoadZoneIni() {
			//m_controlDevice = MZPState.Instance.GetControlDeviceByZone(m_zoneDetails.ZoneId);
		}

		private void CheckForSleep() {
			if (m_zoneDetails.SleepHourMin.Equals(DateTime.Now.ToString(IniFile.DATETIME_DAYHR_FORMAT))) {
				MLog.Log(this, "Zone sleeping");
				if (m_zoneForm != null) {
					m_zoneForm.Close();
				}
				//m_zoneDetails.SleepHourMin = "";
			}
		}

		private void ZoneOpenActions() {
			if ((m_zoneDetails.NotifyZoneEventTriggered == ZoneNotifyState.Closed)
			    || (DateTime.Now.Subtract(m_zoneDetails.LastNotifyZoneEventTriggered).TotalHours > 1)) {
				Utilities.RunProcessWait(IniFile.CurrentPath() + "\\zone-open-" + m_zoneDetails.ZoneId + ".bat",
					System.Diagnostics.ProcessWindowStyle.Hidden, System.Diagnostics.ProcessPriorityClass.BelowNormal);
				m_zoneDetails.NotifyZoneEventTriggered = ZoneNotifyState.Open;
				m_zoneDetails.LastNotifyZoneEventTriggered = DateTime.Now;
			}
		}

		public static void ZoneInactiveActions(ZoneDetails p_zoneDetails) {
			if (p_zoneDetails.NotifyZoneEventTriggered == ZoneNotifyState.Open) {
				Utilities.RunProcessWait(IniFile.CurrentPath() + "\\zone-close-" + p_zoneDetails.ZoneId + ".bat",
					System.Diagnostics.ProcessWindowStyle.Hidden, System.Diagnostics.ProcessPriorityClass.BelowNormal);
				p_zoneDetails.NotifyZoneEventTriggered = ZoneNotifyState.Closed;
				p_zoneDetails.LastNotifyZoneEventTriggered = DateTime.Now;
			}
		}
		/*
		public void ClosureEvent(string key, Boolean isContactMade) // KeyDetail kd)
		{
			

			/*ClosureOpenCloseRelay lastState = m_zoneDetails.ClosureOpenCloseRelay;

			if (lastState == null)
			{
				lastState = new ClosureOpenCloseRelay(isContactMade);
				m_zoneDetails.ClosureOpenCloseRelay = lastState;
			}
			else
			{
				if (m_zoneDetails.ClosureOpenCloseRelay.RelayType != ClosureOpenCloseRelay.EnumRelayType.Button)
					if (lastState.RelayState == m_zoneDetails.ClosureOpenCloseRelay.GetRelayState(isContactMade))
						return;//return if state does not change, for closures, not buttons
				lastState.RelayContactMade = isContactMade;
			}
			
			RecordMoveEvent(MoveTypeEnum.Closure);

			string message = "Closure contact state " + key + " is " + lastState.RelayState + " on zone " + m_zoneDetails.ZoneName;
			MLog.Log(this, message);
			Utilities.AppendToCsvFile(IniFile.CSV_CLOSURES, ",", m_zoneDetails.ZoneName, key, 
				DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT), lastState.RelayState.ToString(), m_zoneDetails.ZoneId.ToString(),
				Constants.EVENT_TYPE_CLOSURE);
			if (lastState.RelayState == ClosureOpenCloseRelay.EnumRelayState.ContactClosed)
			{
				if (m_zoneDetails.IsArmed || MZPState.Instance.SystemAlarm.IsArmed)
				{
					Alert.CreateAlert(message, m_zoneDetails, false, Alert.NotificationFlags.NeedsImmediateUserAck, 3);
				}
				//MZPState.Instance.LogEvent(MZPEvent.EventSource.Closure, message, MZPEvent.EventType.Security,
				//	MZPEvent.EventImportance.Informative, m_zoneDetails);
			}
			m_zoneDetails.MovementAlert = false;
			 */
		//}
	
		public void CounterEvent(string id, ulong counter)
		{
			//MLog.Log(this, "CounterEvent zone=" + m_zoneDetails.ZoneName + " id=" + id+ " count=" + counter);
			
			switch (m_zoneDetails.ClosureType) {
				case EnumClosureType.Counter:
					m_zoneDetails.RecordCounter(id, counter);
					break;
				default:
					MLog.Log(this, "Error, undefined closure type in CounterEvent for zone=" + m_zoneDetails.ZoneName);
					break;
			}

			m_zoneDetails.MovementAlert = false;
		}
		private String SaveCurrentPicture(EventSource source) {
			string url = "", fullfilepath = "";
			try {
				url = IniFile.PARAM_ISPY_URL[1] + ":" + IniFile.PARAM_ISPY_PORT[1]
				      + "/livefeed?oid=" + m_zoneDetails.CameraId + "&r=" + ReflectionInterface.Instance.Random;
				string filename = m_zoneDetails.ZoneId + "_" + m_zoneDetails.ZoneName + "_" + source + "_"
				                  + DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT_FOR_FILE) + ".jpg";
				fullfilepath = IniFile.CurrentPath() + IniFile.WEB_PICTURES_SUBFOLDER + filename;

				if (Utilities.DownloadRemoteImageFile(url, fullfilepath)) {
					PictureSnapshot pict = new PictureSnapshot();
					pict.FileName = filename;
					pict.EventSource = source;
					pict.DateTimeTaken = DateTime.Now;
					pict.ThumbFileName = System.IO.Path.GetFileName(Utilities.GenerateThumb(fullfilepath, "png", 48));
					m_zoneDetails.PictureList.Add(pict);

					return fullfilepath;
				}
			}
			catch (Exception ex) {
				MLog.Log(this, "Error save picture err=" + ex.Message + " url=" + url + " file=" + fullfilepath);
			}
			return "";
		}

		public void TickSlow() {
			if (m_zoneDetails.HasTemperatureSensor && m_zoneDetails.TemperatureAge.TotalMinutes > 5) {
				Alert.CreateAlert("Lost contact with temp sensor", m_zoneDetails, false, null);
			}
		}

		public void TickFast() {
			try {
				if (m_mainZoneActivity != null) {
					if (!m_mainZoneActivity.IsActive()) {
						//m_inactiveCyclesCount++;
					}
					m_mainZoneActivity.Tick();
					m_zoneDetails.Position = MainZoneActivity.Position;
					m_zoneDetails.PositionPercent = MainZoneActivity.PositionPercent;
					m_zoneDetails.VolumeLevel = MainZoneActivity.GetVolumeLevel();
					m_zoneDetails.ZoneState = MainZoneActivity.GetState();
				}
				else {
					//m_inactiveCyclesCount++;
				}

				if (DateTime.Now.Subtract(m_zoneDetails.LastClosureEventDateTime).TotalSeconds > 4) {
					if (m_closurePatern != "") {
						MLog.Log(this, "reseting closure pattern "
						               + m_closurePatern + " on zone " + m_zoneDetails.ZoneName);
					}
					m_closurePatern = "";
				}
				CheckForSleep();
				Boolean toBeClosed = m_zoneDetails.IsInactiveToBeStopped;
				if (toBeClosed) {
					MLog.Log(this, "Zone " + m_zoneDetails.ZoneName + " active=" + m_zoneDetails.IsActive +
						            " closed due to inactivity lapse, type=" + m_zoneDetails.ActivityType.ToString() + " lastcmd=" +
						            m_zoneDetails.LastLocalCommandDateTime);
					m_zoneDetails.Close();
					if (m_zoneForm != null) {
						m_zoneForm.CloseFormSafe();
					}
				}
				if (m_zoneForm != null) {
					m_zoneForm.RefreshState();
				}
			}
			catch (Exception ex) {
				MLog.Log(ex, this, "Error tick fast");
			}
		}
	}
}
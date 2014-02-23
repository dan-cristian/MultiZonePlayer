using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;

using System.Runtime.Serialization;
using System.ComponentModel;
using fastJSON;



namespace MultiZonePlayer
{
    public static class API
    {
        public static void DoCommandFromRawInput(KeyDetail kd)
        {
			int zoneId = -1;
			ZoneDetails zoneDetails;
			try
            {
				
				RemotePipiCommand cmdRemote;
				
				zoneId = MZPState.Instance.GetZoneByControlDevice(kd.Device, kd.Key);
				//ignore console KEYBOARD commands
				if (kd.Device.Contains(IniFile.PARAM_KEYBOARD_DEVICE_IDENTIFIER[1])	&& zoneId == -1)
				{
					//MLog.Log(null,"Ignoring key=" + e.Keyboard.vKey + " from device=" + e.Keyboard.deviceName);
					return;
				}

				ValueList val = new ValueList(GlobalParams.zoneid, zoneId.ToString(), CommandSources.rawinput);
				val.Add(GlobalParams.cmdsource, CommandSources.rawinput.ToString());
				zoneDetails = ZoneDetails.GetZoneById(zoneId);
				if (zoneDetails != null && zoneDetails.RelayType != EnumRelayType.Undefined
					&& zoneDetails.ClosureIdList==kd.Key)
				{
					val.Add(GlobalParams.command, GlobalCommands.closure.ToString());
					val.Add(GlobalParams.id, kd.Key);
					val.Add(GlobalParams.iscontactmade, kd.IsKeyDown.ToString());
					//ZoneClosures.ProcessAction(zoneDetails, kd.Key, kd.IsKeyDown);
					//return;
					Thread th = new Thread(() => DoCommand(val));
					th.Name = "RawInput Closure Key " + kd.Key;
					th.Start();
				}
				else
				{
					//normally let only key down message to pass through
					if (kd.IsKeyUp)
					{
						return;
					}

					cmdRemote = RemotePipi.GetCommandByCode(kd.Key);
					int macroId = MZPState.Instance.GetMacroIdByShortcut(kd.Key, kd.DeviceName);

					MLog.Log(null, "DO key event key=" + kd.Key + " device=" + kd.Device + " keyup=" + kd.IsKeyUp + " keydown=" + kd.IsKeyDown
						+ " apicmd=" + cmdRemote + (cmdRemote == null ? " IGNORING CMD" : "") + " zoneid=" + zoneId + " macroid="+macroId);
					
					if (cmdRemote == null || macroId != -1)
					{
						if (macroId != -1)
						{
							MLog.Log(null, "Hook command not found key=" + kd.Key + ", macro execution id=" + macroId);
							MZPState.Instance.ExecuteMacro(macroId);
						}
						return;
					}

					//check if is not a numeric key
					short intResult;
					string apicmd;
					if (Int16.TryParse(cmdRemote.CommandName, out intResult))
						apicmd = "k" + intResult;
					else
						apicmd = cmdRemote.CommandName.ToLower();
					val.Add(GlobalParams.command, apicmd);
					
					Thread th = new Thread(() => DoCommand(val));
					th.Name = "RawInput Key " + cmdRemote.CommandName;
					th.Start();
				}
            }
            catch (Exception ex)
            {
                MLog.Log(ex, "Error keypressed key=" + kd.Key + " zoneid=" + zoneId.ToString()
                    + " mostrecentzone=" + MZPState.Instance.MostRecentZoneWithContext);
            }
        }

        public static CommandResult DoCommandFromGUIInput(String cmdName, String zoneId)
        {
            ValueList vals = new ValueList(GlobalParams.zoneid, zoneId, CommandSources.gui);
            vals.Add(GlobalParams.command, cmdName);
            return DoCommand(vals);
        }

        /*public static String DoCommandFromWeb(Metadata.ValueList vals)
        {
            return JsonResult(DoCommand(vals));
        }*/

		public static String DoCommandDirect(GlobalCommands cmd, params string[] paramNameValuePair)
		{
			ValueList vals = new ValueList(GlobalParams.command,
								cmd.ToString(), CommandSources.system);

			for (int i = 0; i < paramNameValuePair.Length; i=i+2)
			{
				vals.Add(paramNameValuePair[i], paramNameValuePair[i + 1]);
			}

			return JsonResult(DoCommand(vals));
		}

		public static CommandResult DoCommandFromWeb(ValueList vals)//, out Metadata.ValueList retvalues)
		{
			CommandResult cmdres = DoCommand(vals);
			//retvalues = cmdres.ValueList;
			//return JsonResult(cmdres);
			return cmdres;
		}
        
        public static CommandResult DoCommand(ValueList vals)
        {
            //Metadata.ValueList resvalue;
            CommandResult cmdresult = new CommandResult();
            String cmdName, temp;
            String detailedStatus="";
			int remoteid, macroid,zoneId, id;

            try
            {
                //MZPState.Instance.PowerControl.ResumeFromPowerSaving();
				if (!Int32.TryParse(vals.GetValue(GlobalParams.zoneid), out zoneId))
					zoneId = -1;
                cmdName = vals.GetValue(GlobalParams.command);
				cmdresult.Command = cmdName;
				if (cmdName != GlobalCommands.closure.ToString() 
					&& cmdName != GlobalCommands.getpicture.ToString()
					&& cmdName != GlobalCommands.alarmevent.ToString())
					MLog.Log(null, "Executing DOCommand " + cmdName + " zoneid="+zoneId);
				bool isCmdDefined = Enum.IsDefined(typeof(GlobalCommands), cmdName);
                
                if (isCmdDefined)
				{
					#region standard commands
					GlobalCommands apicmd = (GlobalCommands)Enum.Parse(typeof(GlobalCommands), cmdName);
                    if (!CommandSyntax.Validate(vals))
                    {
						cmdresult.ErrorMessage = "ERROR, potential invalid command";
                        MLog.Log(null, cmdresult.ErrorMessage);
                    }
					cmdresult.Result = ResultEnum.OK;
                    //global commands are processed here, zone based commands are delegated to m_valueList
                    switch (apicmd)
                    {
                        case GlobalCommands.help:
                            foreach (String item in Enum.GetNames(typeof(GlobalCommands)))
                            {
                               detailedStatus += item + ";" ;
                            }
							cmdresult.OutputMessage = "Available commands: " + detailedStatus;
                            //resvalue = new Metadata.ValueList(Metadata.GlobalParams.msg, "Available commands: " + detailedStatus, Metadata.CommandSources.web);
                            //result = JsonResult(Metadata.ResultEnum.OK, "", null);
                            break;
                        /*case Metadata.GlobalCommands.selectzone:
                            ControlCenter.Instance.OpenZone(zoneId);//vals.GetValue(Metadata.GlobalParams.activity));
                            result = JsonResult(Metadata.ResultEnum.OK, "", null);
                            break;
                         */
                        case GlobalCommands.cameraevent:
                            //we don't have zoneid, add it from camera id
                            int oid = Convert.ToInt16(vals.GetValue(GlobalParams.oid));
                            vals.Add(GlobalParams.zoneid, MZPState.Instance.GetZoneIdByCamZoneId(oid).ToString());
                            //then pass command
                            DoZoneCommand(apicmd, vals, ref cmdresult);
                            //resvalue = values;
                            //result = JsonResult(res, err, values);
                            break;
                        case GlobalCommands.sleep:
                            cmdresult.Result = GoToSleep(out cmdresult.ErrorMessage);
                            //result = JsonResult(res, err, null);
                            break;
                        case GlobalCommands.status:
							detailedStatus += MZPState.Instance.GetZonesStatus();
							cmdresult.OutputMessage = detailedStatus;
                            //result = JsonResult(Metadata.ResultEnum.OK, detailedStatus, null);
                            break;
                        case GlobalCommands.genrelist:
							cmdresult.ValueList = MediaLibrary.MusicGenres;
							
                            //result = JsonResult(Metadata.ResultEnum.OK, "", resvalue);
                            break;
                        case GlobalCommands.artistlist:
							cmdresult.ValueList = MediaLibrary.MusicArtists;
                            //result = JsonResult(Metadata.ResultEnum.OK, "", resvalue);
                            break;
						case GlobalCommands.movielist:
							cmdresult.ValueList = MediaLibrary.MovieList;
							//result = JsonResult(Metadata.ResultEnum.OK, "", resvalue);
							break;
                        case GlobalCommands.alarmarm:
                            String areaid = vals.GetValue(GlobalParams.areaid);
                            cmdresult = MZPState.Instance.ZoneEvents.SendCommand_PARADOX(ZoneEvents.EnumParadoxCommands.Arm, areaid);
                            //result = JsonResult(cmdres.Result, cmdres.ErrorMessage, null);
                            break;
                        case GlobalCommands.alarmdisarm:
                            cmdresult = MZPState.Instance.ZoneEvents.SendCommand_PARADOX(ZoneEvents.EnumParadoxCommands.Disarm, vals.GetValue(GlobalParams.areaid));
                            //result = JsonResult(cmdres.Result, cmdres.ErrorMessage, null);
                            break;
                        case GlobalCommands.alarmkeypadbeep:
                            cmdresult = MZPState.Instance.ZoneEvents.SendCommand_PARADOX(ZoneEvents.EnumParadoxCommands.KeypadBeep, vals.GetValue(GlobalParams.areaid));
                            //result = JsonResult(cmdres.Result, cmdres.ErrorMessage, null);
                            break;
                        case GlobalCommands.alarmstay:
                            cmdresult = MZPState.Instance.ZoneEvents.SendCommand_PARADOX(ZoneEvents.EnumParadoxCommands.Stay, vals.GetValue(GlobalParams.areaid));
                            //result = JsonResult(cmdres.Result, cmdres.ErrorMessage, null);
                            break;
                        case GlobalCommands.restartispy:
                            Utilities.CloseProcSync(IniFile.PARAM_ISPY_OTHERPROC[1]);
							MZPState.RestartGenericProc(IniFile.PARAM_ISPY_PROCNAME[1], 
								IniFile.PARAM_ISPY_APP_PATH[1], System.Diagnostics.ProcessWindowStyle.Minimized, System.Diagnostics.ProcessPriorityClass.BelowNormal);
							//resvalue = new Metadata.ValueList(Metadata.GlobalParams.msg, "all ok", Metadata.CommandSources.system);
                            //result = JsonResult(Metadata.ResultEnum.OK, "", null);
                            break;
                        case GlobalCommands.restartwinload:
                            MZPState.RestartGenericProc(IniFile.PARAM_PARADOX_WINLOAD_PROCNAME[1],
								IniFile.PARAM_PARADOX_WINLOAD_APP_PATH[1], System.Diagnostics.ProcessWindowStyle.Normal, System.Diagnostics.ProcessPriorityClass.BelowNormal);
                            //resvalue = new Metadata.ValueList(Metadata.GlobalParams.msg, "all ok", Metadata.CommandSources.system);
                            //result = JsonResult(Metadata.ResultEnum.OK, "", null);
                            break;
                        case GlobalCommands.setnotify:
                            String action;
                            action = vals.GetValue(GlobalParams.action);
                            switch (action)
                            {
                                case "enablegtalk":
                                case "disablegtalk":
                                    MZPState.Instance.NotifyState.GTalkEnabled = (action == "enablegtalk");
                                    detailedStatus += " enablegtalk=" + MZPState.Instance.NotifyState.GTalkEnabled;
                                    break;
                                case "enablesms":
                                case "disablesms":
                                    MZPState.Instance.NotifyState.SMSEnabled = (action=="enablesms");
                                    detailedStatus+= " enablesms=" + MZPState.Instance.NotifyState.SMSEnabled;
                                    break;
                                default:
                                    detailedStatus= "Unknown setnotify action";
                                    MLog.Log(null, detailedStatus);
                                    break;
                            }
							cmdresult.OutputMessage = detailedStatus;
                            //result = JsonResult(Metadata.ResultEnum.OK, detailedStatus, null);
                            //resvalue = new Metadata.ValueList(Metadata.GlobalParams.msg, status, Metadata.CommandSources.Internal);
                            break;
                        case GlobalCommands.sendsms:
                            String message = vals.GetValue(GlobalParams.msg);
                            MZPState.Instance.m_messengerList.Find(x => x.GetType().Equals(typeof(SMS))).SendMessageToTarget(message);
                            //result = JsonResult(Metadata.ResultEnum.OK, "", null);
                            break;
                        case GlobalCommands.restartsystem:
                            MZPState.RestartComputer("restart command received");
                            //result = JsonResult(Metadata.ResultEnum.OK, "", null);
                            break;
                        case GlobalCommands.makebuzz:
                            MZPState.Instance.MessengerMakeBuzz();
                            //result = JsonResult(Metadata.ResultEnum.OK, "", null);
                            break;
                        case GlobalCommands.powerevent:
                            Boolean failure = vals.GetValue(GlobalParams.action).ToLower().Equals("true");
                            String datetime = vals.GetValue(GlobalParams.datetime);
                            if (MZPState.Instance.IsPowerFailure != failure)
                            {
                                MZPState.Instance.IsPowerFailure = failure;
								Alert.CreateAlert("Power Failure state is " + failure, null, !failure, Alert.NotificationFlags.NotifyUserAfterXMinutes, 1);
                                //MZPState.Instance.LogEvent(MZPEvent.EventSource.Environment, "Power Failure state is " + failure + " at " + datetime, 
                                //    MZPEvent.EventType.Security, MZPEvent.EventImportance.Critical, null);
                            }
							cmdresult.OutputMessage = "power failure=" + failure;
                            //result = JsonResult(Metadata.ResultEnum.OK, , null);
                            break;
						case GlobalCommands.getpicture:
							int pictcount = Convert.ToInt16(vals.GetValue(GlobalParams.count));
							int interval = Convert.ToInt16(vals.GetValue(GlobalParams.interval));
							string face = vals.GetValue(GlobalParams.face);
							string picture = MediaLibrary.AllPictureFiles.IteratePicture(pictcount, interval,face);
							string[] pictsplit = picture.Split('.');
							cmdresult.ValueList= new ValueList();
							cmdresult.ValueList.BinaryData = Utilities.ReadBinaryFile(picture);
							cmdresult.ValueList.Add(GlobalParams.contenttype, "image/" + pictsplit[pictsplit.Length - 1]);
							cmdresult.OutputMessage = "picture retrieved=" + picture;
							
							//result = JsonResult(Metadata.ResultEnum.OK, , null);
							break;
						case GlobalCommands.nextpicture:
							MediaLibrary.AllPictureFiles.ForceNextPicture();
							cmdresult.OutputMessage = "next picture set";
							break;
						case GlobalCommands.previouspicture:
							MediaLibrary.AllPictureFiles.ForcePreviousPicture();
							cmdresult.OutputMessage = "next picture set";
							break;
						case GlobalCommands.remotepoweron:
							remoteid=Convert.ToInt16(vals.GetValue(GlobalParams.remoteid));
							cmdresult.OutputMessage = RemotePowerControl.SwitchOn(remoteid);
							//result = MZPState.Instance.RemoteControl.RFOn(remoteid);
							//result = JsonResult(Metadata.ResultEnum.OK, "remoteon="+result, null);
							break;
						case GlobalCommands.remotepoweroff:
							remoteid=Convert.ToInt16(vals.GetValue(GlobalParams.remoteid));
							cmdresult.OutputMessage = RemotePowerControl.SwitchOff(remoteid);
							//result = MZPState.Instance.RemoteControl.RFOff(remoteid);
							//result = JsonResult(Metadata.ResultEnum.OK, "remoteoff="+result, null);
							break;
						case GlobalCommands.remoteadjustdim:
							remoteid = Convert.ToInt16(vals.GetValue(GlobalParams.remoteid));
							int dimvalue = Convert.ToInt16(vals.GetValue(GlobalParams.dimvalue));
							cmdresult.OutputMessage = RemotePowerControl.AdjustDim(remoteid, dimvalue);
							//result = MZPState.Instance.RemoteControl.RFOff(remoteid);
							//result = JsonResult(Metadata.ResultEnum.OK, "remotedim=" + result, null);
							break;
						case GlobalCommands.macro:
							string shortcut = vals.GetValue(GlobalParams.singleparamvalue);
							macroid = MZPState.Instance.GetMacroIdByShortcut(shortcut, "");
							if (macroid == -1)
							{
								temp = vals.GetValue(GlobalParams.id);
								macroid = Convert.ToInt16(temp);
							}
							if (macroid != -1)
							{
								cmdresult = MZPState.Instance.ExecuteMacro(macroid);
								cmdresult.OutputMessage += "macro id=" + macroid;
								//result = JsonResult(Metadata.ResultEnum.OK, "macro id=" + macroid, null);
							}
							else
								cmdresult.ErrorMessage = "macro not found name="+shortcut;
							break;
						case GlobalCommands.rfxcmd:
							string rfxc = vals.GetValue(GlobalParams.action);
							//string device = vals.GetValue(Metadata.GlobalParams.rfxdevicename);
							cmdresult = MZPState.Instance.ExecuteRFXCmd(rfxc);//device, rfxc);
							break;
						case GlobalCommands.runscript:
							System.Diagnostics.Process proc = Utilities.RunProcessWait(IniFile.CurrentPath() + "\\scripts\\"+vals.GetValue(GlobalParams.name),
								System.Diagnostics.ProcessWindowStyle.Hidden, System.Diagnostics.ProcessPriorityClass.BelowNormal);
							cmdresult.OutputMessage = "Exit code="+proc.ExitCode;
							break;
						case GlobalCommands.generatefullgraph:
							int _zoneid;
							int ageHours = Convert.ToInt16(vals.GetValue(GlobalParams.interval));
							SimpleGraph graph = new SimpleGraph(true, true, true);
							string[] zones = vals.GetValue(GlobalParams.zoneidlist).Split(',');
							List<ZoneDetails> zoneList = new List<ZoneDetails>();
							foreach (string _zone in zones)
							{
								_zoneid = Convert.ToInt16(_zone);
								zoneList.Add(ZoneDetails.GetZoneById(_zoneid));
							}
							graph.ShowTempGraph(ageHours, zoneList);
							break;
						case GlobalCommands.dismissalert:
							int aid = Convert.ToInt16(vals.GetValue(GlobalParams.id));
							//MZPState.Instance.ZoneEvents.DismissAlert(vals);
							Alert.DismissAlert(aid);
							break;
						case GlobalCommands.setfield:
							string classname = vals.GetValue(GlobalParams.classname);
							string field = vals.GetValue(GlobalParams.field);
							string text = vals.GetValue(GlobalParams.text);
							string valId = vals.GetValue(GlobalParams.id);
							if (!Int32.TryParse(valId, out id))
								MLog.Log(null, "Error, ID parse not ok for id="+valId);
							
							Object fieldObj=null;
							switch (classname) {
								case "ZoneDetails":
									fieldObj = ZoneDetails.GetZoneById(id);
									break;
								case "User":
									fieldObj = User.GetUser(id);
									break;
							}
							if (Reflect.SetFieldValue(ref fieldObj, field, text)) {
								((Singleton)fieldObj).SaveToIni();
								cmdresult.OutputMessage += "Field " + field + " set to " + text;
							}
							else {
								cmdresult.OutputMessage += "Error setting value, not found field=" + field + " value=" + text;
								MLog.Log(null, cmdresult.OutputMessage);
							}
							
							break;
						case GlobalCommands.userlist:
							field = vals.GetValue(GlobalParams.field);
							string line,val1;
							string[] fields = field.Split(',');
							foreach (User user in User.UserList) {
								line = ""; val1 = "";
								foreach (String fld in fields) {
									switch (fld) {
										case "PhoneBTAddress":
											val1 = user.PhoneBTAddress;
											break;
										case "WifiMACAddress":
											val1 = user.WifiMACAddress;
											break;
										case "Name":
											val1 = user.Name;
											break;
										case "Email":
											val1 = user.Email;
											break;
									}

									if (val1 != null && val1.Trim() != "") {
										if (line != "") line += "\t";
										line += val1;
									}
								}
								if (line != "")
									cmdresult.OutputMessage += line + "\n";
							}
							break;
						case GlobalCommands.reloadlibrarymusic:
							MediaLibrary.InitialiseMusic();
							break;
						case GlobalCommands.reloadlibrarypictures:
							MediaLibrary.InitialisePictures();
							break;
						case GlobalCommands.reloadlibraryvideo:
							MediaLibrary.InitialiseVideos();
							break;
                        default:
                            DoZoneCommand(apicmd, vals, ref cmdresult);
                            break;
					}
					#endregion
				}
                else
                {//looking if this is a macro name
					macroid = MZPState.Instance.GetMacroIdByShortcut(cmdName, "");
					if (macroid != -1)
					{
						cmdresult = MZPState.Instance.ExecuteMacro(macroid);
						cmdresult.OutputMessage += "macro2 id=" + macroid;
					}
					else
					{
						cmdresult.ErrorMessage += "Unhandled API Command " + cmdName;
						MLog.Log(null, cmdresult.ErrorMessage);
						cmdresult.Result = ResultEnum.ERR;
					}
                }
            }
            catch (Exception ex)
            {
                MLog.Log(ex, "Error DoCommand");
                cmdresult.ErrorMessage += "Exception DoCommand " + ex.Message;
				cmdresult.Result = ResultEnum.ERR;
            }
            return cmdresult;
        }

        public static void DoZoneCommand(GlobalCommands apicmd, ValueList vals, ref CommandResult result)
            //private static String DoZoneCommand(String cmdName, int zoneId, out String errorMessage)
        {
            try
            {
                int zoneId = InferZone(vals.GetValue(GlobalParams.zoneid),
					vals.GetValue(GlobalParams.zonename), vals.GetValue(GlobalParams.singleparamvalue));

                if (zoneId == -1)
                {
					result.ErrorMessage = "ERROR no zone found to process command" + apicmd;
					MLog.Log(null, result.ErrorMessage);
					result.Result = ResultEnum.ERR;
					return;
                }

                ZoneGeneric zone;
                //zone for cmd received is not active
                if (MZPState.Instance != null){
                    if (MZPState.Instance.GetZoneIfActive(zoneId) == null){
						MLog.Log(null, "No active zone found in zone=" + zoneId + " cmd=" + apicmd);
                        if (Enum.IsDefined(typeof(GlobalCommandsUniversal), apicmd.ToString())){
                            MLog.Log(null, "Universal cmd received for zone recent=" + zoneId + " cmd=" + apicmd);
                            zoneId = MZPState.Instance.GetActiveChildZone(zoneId);
                        }
                        else{
                            if (MZPState.Instance.GetZone(zoneId) == null)
                                MZPState.Instance.OpenZone(zoneId);
                        }
                    }

                    zone = MZPState.Instance.GetZone(zoneId);
                    if (zone == null)
                    {
                        MLog.Log(null, "No current zone for cmd=" + apicmd + " zoneid=" + zoneId);
                        result.ErrorMessage = "Zone not active " + zoneId;
                        result.Result = ResultEnum.ERR;
						return;
                    }
                    else
                    {
                        lock (zone)
                        {
							result.ErrorMessage = "cmdzone="+zone.ZoneName;
                            zone.ProcessAction(apicmd, vals, ref result);
                        }
                    }
                }
                else
                {
                    result.ErrorMessage = "ControlCenter instance is null";
					result.Result = ResultEnum.ERR;
					MLog.Log(null, result.ErrorMessage);
					return;
                }
            }
            catch (Exception ex)
            {
                MLog.Log(ex, "Error DoZoneCommand cmd=" + apicmd);
                result.ErrorMessage = ex.Message;
                result.Result = ResultEnum.ERR;
            }
        }

		private static int InferZone(String zoneidentifier, String zonename, String singleParamValue)
		{
			int zoneId = -1;

			if (zoneidentifier != null)
				zoneId = Convert.ToInt16(zoneidentifier);
			else
			{
				if (zonename == null) zonename = singleParamValue;
				ZoneDetails zonedetails;
				if (zonename != null)
				{
					zonedetails = ZoneDetails.ZoneDetailsList.Find(x =>
						x.ZoneName.ToLower().Contains(zonename.ToLower()));
					if (zonedetails != null) zoneId = zonedetails.ZoneId;
				}
			}
			if (zoneId == -1)
			{
				zoneId = MZPState.Instance.MostRecentZoneWithContext;
				MLog.Log(null, "Infered zoneid " + zoneId + " had zoneidentifier=" + zoneidentifier + " zonename=" + zonename);

			}
			return zoneId;
		}

        private static ResultEnum GoToSleep(out String errMessage)
        {
            ControlCenter.Instance.CloseAllZones();
            bool retVal = true;// Application.SetSuspendState(PowerState.Suspend, false, false);

            if (retVal == false)
            {
                errMessage = "Could not suspend the system";  
                return ResultEnum.ERR;
            }
            else
            {
                errMessage = "";
                return ResultEnum.OK;
            }
        }

        private static String JsonResult(CommandResult cmdres)//Metadata.ResultEnum result, String errorMessage, Metadata.ValueList values)
        {
            //Metadata.CommandResult cmdres = new Metadata.CommandResult();
            //cmdres.Result = result;
            //cmdres.ErrorMessage = errorMessage;

            ServerStatus srv = new ServerStatus();
            srv.IsServerOn = true;
            if (MZPState.Instance != null)
            {
                srv.ZoneDetails = ZoneDetails.ZoneDetailsList.ToArray();
                srv.CamAlertList = MZPState.Instance.ZoneEvents.CamAlertList;
            }
            cmdres.ServerStatus = srv;
            //cmdres.ValueList = values;
			JSONParameters param=new JSONParameters();param.UseExtensions = true;
			return fastJSON.JSON.Instance.ToJSON(cmdres, param);
        }

        /*
        private static String GetJsonResponse(Metadata.CommandResult res)
        {
            Metadata.ServerStatus srv = new Metadata.ServerStatus();
            srv.ZoneDetails = ControlCenter.Instance.Zones.ToArray();
            res.ServerStatus = srv;
            string jsonStatus = fastJSON.JSON.Instance.ToJSON(res, false);
            return jsonStatus;
        }
         * */
    }
}

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
			Metadata.ZoneDetails zoneDetails;
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

				Metadata.ValueList val = new Metadata.ValueList(Metadata.GlobalParams.zoneid, zoneId.ToString(), Metadata.CommandSources.rawinput);
				val.Add(Metadata.GlobalParams.cmdsource, Metadata.CommandSources.rawinput.ToString());
				zoneDetails = MZPState.Instance.GetZoneById(zoneId);
				if (zoneDetails != null && zoneDetails.ClosureOpenCloseRelay.RelayType != Metadata.ClosureOpenCloseRelay.EnumRelayType.Undefined
					&& zoneDetails.ClosureIdList==kd.Key)
				{
					val.Add(Metadata.GlobalParams.command, Metadata.GlobalCommands.closure.ToString());
					val.Add(Metadata.GlobalParams.id, kd.Key);
					val.Add(Metadata.GlobalParams.iscontactmade, kd.IsKeyDown.ToString());
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
					val.Add(Metadata.GlobalParams.command, apicmd);
					
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

        public static Metadata.CommandResult DoCommandFromGUIInput(String cmdName, String zoneId)
        {
            Metadata.ValueList vals = new Metadata.ValueList(Metadata.GlobalParams.zoneid, zoneId, Metadata.CommandSources.gui);
            vals.Add(Metadata.GlobalParams.command, cmdName);
            return DoCommand(vals);
        }

        public static String DoCommandFromWeb(Metadata.ValueList vals)
        {
            return JsonResult(DoCommand(vals));
        }

		public static String DoCommandDirect(Metadata.GlobalCommands cmd, params string[] paramNameValuePair)
		{
			Metadata.ValueList vals = new Metadata.ValueList(Metadata.GlobalParams.command,
								cmd.ToString(), Metadata.CommandSources.system);

			for (int i = 0; i < paramNameValuePair.Length; i=i+2)
			{
				vals.Add(paramNameValuePair[i], paramNameValuePair[i + 1]);
			}

			return JsonResult(DoCommand(vals));
		}

		public static String DoCommandFromWeb(Metadata.ValueList vals, out Metadata.ValueList retvalues)
		{
			Metadata.CommandResult cmdres = DoCommand(vals);
			retvalues = cmdres.ValueList;
			return JsonResult(cmdres);
		}
        
        public static Metadata.CommandResult DoCommand(Metadata.ValueList vals)
        {
            //Metadata.ValueList resvalue;
            Metadata.CommandResult cmdresult = new Metadata.CommandResult();
            String cmdName;
            String detailedStatus="";
			int remoteid, macroid;

            try
            {
                MZPState.Instance.PowerControl.ResumeFromPowerSaving();
                //zoneId = Convert.ToInt16(vals.GetValue(Metadata.GlobalParams.zoneid));
                cmdName = vals.GetValue(Metadata.GlobalParams.command);
				cmdresult.Command = cmdName;
				if (cmdName != Metadata.GlobalCommands.closure.ToString() 
					&& cmdName != Metadata.GlobalCommands.getpicture.ToString()
					&& cmdName != Metadata.GlobalCommands.alarmevent.ToString())
					MLog.Log(null, "Executing DOCommand " + cmdName);
				bool isCmdDefined = Enum.IsDefined(typeof(Metadata.GlobalCommands), cmdName);
                
                if (isCmdDefined)
				{
					#region standard commands
					Metadata.GlobalCommands apicmd = (Metadata.GlobalCommands)Enum.Parse(typeof(Metadata.GlobalCommands), cmdName);
                    if (!Metadata.CommandSyntax.Validate(vals))
                    {
						cmdresult.ErrorMessage = "ERROR, potential invalid command";
                        MLog.Log(null, cmdresult.ErrorMessage);
                    }
					cmdresult.Result = Metadata.ResultEnum.OK;
                    //global commands are processed here, zone based commands are delegated to zones
                    switch (apicmd)
                    {
                        case Metadata.GlobalCommands.help:
                            foreach (String item in Enum.GetNames(typeof(Metadata.GlobalCommands)))
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
                        case Metadata.GlobalCommands.cameraevent:
                            //we don't have zoneid, add it from camera id
                            int oid = Convert.ToInt16(vals.GetValue(Metadata.GlobalParams.oid));
                            vals.Add(Metadata.GlobalParams.zoneid, MZPState.Instance.GetZoneIdByCamZoneId(oid).ToString());
                            //then pass command
                            DoZoneCommand(apicmd, vals, ref cmdresult);
                            //resvalue = values;
                            //result = JsonResult(res, err, values);
                            break;
                        case Metadata.GlobalCommands.sleep:
                            cmdresult.Result = GoToSleep(out cmdresult.ErrorMessage);
                            //result = JsonResult(res, err, null);
                            break;
                        case Metadata.GlobalCommands.status:
							detailedStatus += "\r\n" + MZPState.Instance.SystemAlarm.AreaState;
							foreach (Metadata.ZoneDetails zone in MZPState.Instance.ZoneDetails)
                            {
                                detailedStatus += "\r\n" + zone.SummaryStatus;
                            }
							cmdresult.OutputMessage = detailedStatus;
                            //result = JsonResult(Metadata.ResultEnum.OK, detailedStatus, null);
                            break;
                        case Metadata.GlobalCommands.genrelist:
							cmdresult.ValueList = MediaLibrary.MusicGenres;
							
                            //result = JsonResult(Metadata.ResultEnum.OK, "", resvalue);
                            break;
                        case Metadata.GlobalCommands.artistlist:
							cmdresult.ValueList = MediaLibrary.MusicArtists;
                            //result = JsonResult(Metadata.ResultEnum.OK, "", resvalue);
                            break;
                        case Metadata.GlobalCommands.alarmarm:
                            String areaid = vals.GetValue(Metadata.GlobalParams.areaid);
                            cmdresult = MZPState.Instance.ZoneEvents.SendCommand_PARADOX(ZoneEvents.EnumParadoxCommands.Arm, areaid);
                            //result = JsonResult(cmdres.Result, cmdres.ErrorMessage, null);
                            break;
                        case Metadata.GlobalCommands.alarmdisarm:
                            cmdresult = MZPState.Instance.ZoneEvents.SendCommand_PARADOX(ZoneEvents.EnumParadoxCommands.Disarm, vals.GetValue(Metadata.GlobalParams.areaid));
                            //result = JsonResult(cmdres.Result, cmdres.ErrorMessage, null);
                            break;
                        case Metadata.GlobalCommands.alarmkeypadbeep:
                            cmdresult = MZPState.Instance.ZoneEvents.SendCommand_PARADOX(ZoneEvents.EnumParadoxCommands.KeypadBeep, vals.GetValue(Metadata.GlobalParams.areaid));
                            //result = JsonResult(cmdres.Result, cmdres.ErrorMessage, null);
                            break;
                        case Metadata.GlobalCommands.alarmstay:
                            cmdresult = MZPState.Instance.ZoneEvents.SendCommand_PARADOX(ZoneEvents.EnumParadoxCommands.Stay, vals.GetValue(Metadata.GlobalParams.areaid));
                            //result = JsonResult(cmdres.Result, cmdres.ErrorMessage, null);
                            break;
                        case Metadata.GlobalCommands.restartispy:
                            Utilities.CloseProcSync(IniFile.PARAM_ISPY_OTHERPROC[1]);
							MZPState.RestartGenericProc(IniFile.PARAM_ISPY_PROCNAME[1], IniFile.PARAM_ISPY_APP_PATH[1], System.Diagnostics.ProcessWindowStyle.Minimized);
							//resvalue = new Metadata.ValueList(Metadata.GlobalParams.msg, "all ok", Metadata.CommandSources.system);
                            //result = JsonResult(Metadata.ResultEnum.OK, "", null);
                            break;
                        case Metadata.GlobalCommands.restartwinload:
                            MZPState.RestartGenericProc(IniFile.PARAM_PARADOX_WINLOAD_PROCNAME[1],
								IniFile.PARAM_PARADOX_WINLOAD_APP_PATH[1], System.Diagnostics.ProcessWindowStyle.Normal);
                            //resvalue = new Metadata.ValueList(Metadata.GlobalParams.msg, "all ok", Metadata.CommandSources.system);
                            //result = JsonResult(Metadata.ResultEnum.OK, "", null);
                            break;
                        case Metadata.GlobalCommands.setnotify:
                            String action;
                            action = vals.GetValue(Metadata.GlobalParams.action);
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
                        case Metadata.GlobalCommands.sendsms:
                            String message = vals.GetValue(Metadata.GlobalParams.msg);
                            MZPState.Instance.m_messengerList.Find(x => x.GetType().Equals(typeof(SMS))).SendMessageToTarget(message);
                            //result = JsonResult(Metadata.ResultEnum.OK, "", null);
                            break;
                        case Metadata.GlobalCommands.restartsystem:
                            MZPState.RestartComputer("restart command received");
                            //result = JsonResult(Metadata.ResultEnum.OK, "", null);
                            break;
                        case Metadata.GlobalCommands.makebuzz:
                            MZPState.Instance.MessengerMakeBuzz();
                            //result = JsonResult(Metadata.ResultEnum.OK, "", null);
                            break;
                        case Metadata.GlobalCommands.powerevent:
                            Boolean failure = vals.GetValue(Metadata.GlobalParams.action).ToLower().Equals("true");
                            String datetime = vals.GetValue(Metadata.GlobalParams.datetime);
                            if (MZPState.Instance.IsPowerFailure != failure)
                            {
                                MZPState.Instance.IsPowerFailure = failure;
                                MZPState.Instance.LogEvent(MZPEvent.EventSource.Environment, "Power Failure state is " + failure + " at " + datetime, 
                                    MZPEvent.EventType.Security, MZPEvent.EventImportance.Critical, null);
                            }
							cmdresult.OutputMessage = "power failure=" + failure;
                            //result = JsonResult(Metadata.ResultEnum.OK, , null);
                            break;
						case Metadata.GlobalCommands.getpicture:
							int pictcount = Convert.ToInt16(vals.GetValue(Metadata.GlobalParams.count));
							int interval = Convert.ToInt16(vals.GetValue(Metadata.GlobalParams.interval));
							string face = vals.GetValue(Metadata.GlobalParams.face);
							string picture = MediaLibrary.AllPictureFiles.IteratePicture(pictcount, interval,face);
							string[] pictsplit = picture.Split('.');
							cmdresult.ValueList= new Metadata.ValueList();
							cmdresult.ValueList.BinaryData = Utilities.ReadBinaryFile(picture);
							cmdresult.ValueList.Add(Metadata.GlobalParams.contenttype, "image/" + pictsplit[pictsplit.Length - 1]);
							cmdresult.OutputMessage = "picture retrieved=" + picture;
							
							//result = JsonResult(Metadata.ResultEnum.OK, , null);
							break;
						case Metadata.GlobalCommands.remotepoweron:
							remoteid=Convert.ToInt16(vals.GetValue(Metadata.GlobalParams.remoteid));
							cmdresult.OutputMessage = RemotePowerControl.SwitchOn(remoteid);
							//result = MZPState.Instance.RemoteControl.RFOn(remoteid);
							//result = JsonResult(Metadata.ResultEnum.OK, "remoteon="+result, null);
							break;
						case Metadata.GlobalCommands.remotepoweroff:
							remoteid=Convert.ToInt16(vals.GetValue(Metadata.GlobalParams.remoteid));
							cmdresult.OutputMessage = RemotePowerControl.SwitchOff(remoteid);
							//result = MZPState.Instance.RemoteControl.RFOff(remoteid);
							//result = JsonResult(Metadata.ResultEnum.OK, "remoteoff="+result, null);
							break;
						case Metadata.GlobalCommands.remoteadjustdim:
							remoteid = Convert.ToInt16(vals.GetValue(Metadata.GlobalParams.remoteid));
							int dimvalue = Convert.ToInt16(vals.GetValue(Metadata.GlobalParams.dimvalue));
							cmdresult.OutputMessage = RemotePowerControl.AdjustDim(remoteid, dimvalue);
							//result = MZPState.Instance.RemoteControl.RFOff(remoteid);
							//result = JsonResult(Metadata.ResultEnum.OK, "remotedim=" + result, null);
							break;
						case Metadata.GlobalCommands.macro:
							string shortcut = vals.GetValue(Metadata.GlobalParams.singleparamvalue);
							macroid = MZPState.Instance.GetMacroIdByShortcut(shortcut, "");
							if (macroid != -1)
							{
								cmdresult = MZPState.Instance.ExecuteMacro(macroid);
								cmdresult.OutputMessage += "macro id=" + macroid;
								//result = JsonResult(Metadata.ResultEnum.OK, "macro id=" + macroid, null);
							}
							else
								cmdresult.ErrorMessage = "macro not found name="+shortcut;
							break;
						case Metadata.GlobalCommands.rfxcmd:
							string rfxc = vals.GetValue(Metadata.GlobalParams.action);
							//string device = vals.GetValue(Metadata.GlobalParams.rfxdevicename);
							cmdresult = MZPState.Instance.ExecuteRFXCmd(rfxc);//device, rfxc);
							break;
						case Metadata.GlobalCommands.runscript:
							System.Diagnostics.Process proc = Utilities.RunProcessWait(IniFile.CurrentPath() + "\\scripts\\"+vals.GetValue(Metadata.GlobalParams.name),
								System.Diagnostics.ProcessWindowStyle.Hidden);
							cmdresult.OutputMessage = "Exit code="+proc.ExitCode;
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
						cmdresult.Result = Metadata.ResultEnum.ERR;
					}
                }
            }
            catch (Exception ex)
            {
                MLog.Log(ex, "Error DoCommand");
                cmdresult.ErrorMessage += "Exception DoCommand " + ex.Message;
				cmdresult.Result = Metadata.ResultEnum.ERR;
            }
            return cmdresult;
        }

        public static void DoZoneCommand(Metadata.GlobalCommands apicmd, Metadata.ValueList vals, ref Metadata.CommandResult result)
            //private static String DoZoneCommand(String cmdName, int zoneId, out String errorMessage)
        {
            try
            {
                int zoneId = InferZone(vals.GetValue(Metadata.GlobalParams.zoneid),
					vals.GetValue(Metadata.GlobalParams.zonename), vals.GetValue(Metadata.GlobalParams.singleparamvalue));

                if (zoneId == -1)
                {
					result.ErrorMessage = "ERROR no zone found to process command" + apicmd;
					MLog.Log(null, result.ErrorMessage);
					result.Result = Metadata.ResultEnum.ERR;
					return;
                }

                ZoneGeneric zone;
                //zone for cmd received is not active
                if (ControlCenter.Instance != null)
                {
                    if (ControlCenter.Instance.GetZoneIfActive(zoneId) == null)
                    {
                        if (Enum.IsDefined(typeof(Metadata.GlobalCommandsUniversal), apicmd.ToString()))
                        {
                            MLog.Log(null, "Universal cmd received for zone recent=" + zoneId + " cmd=" + apicmd);
                            zoneId = MZPState.Instance.GetActiveChildZone(zoneId);
                        }
                        else
                        {
                            if (ControlCenter.Instance.GetZone(zoneId) == null)
                                ControlCenter.Instance.OpenZone(zoneId);
                        }
                    }

                    zone = ControlCenter.Instance.GetZone(zoneId);
                    if (zone == null)
                    {
                        MLog.Log(null, "No current zone for cmd=" + apicmd + " zoneid=" + zoneId);
                        result.ErrorMessage = "Zone not active " + zoneId;
                        result.Result = Metadata.ResultEnum.ERR;
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
					result.Result = Metadata.ResultEnum.ERR;
					return;
                }
            }
            catch (Exception ex)
            {
                MLog.Log(ex, "Error DoZoneCommand cmd=" + apicmd);
                result.ErrorMessage = ex.Message;
                result.Result = Metadata.ResultEnum.ERR;
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
				Metadata.ZoneDetails zonedetails;
				if (zonename != null)
				{
					zonedetails = MZPState.Instance.ZoneDetails.Find(x =>
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

        private static Metadata.ResultEnum GoToSleep(out String errMessage)
        {
            ControlCenter.Instance.CloseAllZones();
            bool retVal = true;// Application.SetSuspendState(PowerState.Suspend, false, false);

            if (retVal == false)
            {
                errMessage = "Could not suspend the system";  
                return Metadata.ResultEnum.ERR;
            }
            else
            {
                errMessage = "";
                return Metadata.ResultEnum.OK;
            }
        }

        private static String JsonResult(Metadata.CommandResult cmdres)//Metadata.ResultEnum result, String errorMessage, Metadata.ValueList values)
        {
            //Metadata.CommandResult cmdres = new Metadata.CommandResult();
            //cmdres.Result = result;
            //cmdres.ErrorMessage = errorMessage;

            Metadata.ServerStatus srv = new Metadata.ServerStatus();
            srv.IsServerOn = true;
            if (MZPState.Instance != null)
            {
                srv.ZoneDetails = MZPState.Instance.ZoneDetails.ToArray();
                srv.CamAlertList = MZPState.Instance.ZoneEvents.CamAlertList;
            }
            cmdres.ServerStatus = srv;
            //cmdres.ValueList = values;
            return fastJSON.JSON.Instance.ToJSON(cmdres, true);
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

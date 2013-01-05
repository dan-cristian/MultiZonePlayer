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
            
            //ignore console KEYBOARD commands
            if (kd.Device.Contains(IniFile.PARAM_KEYBOARD_DEVICE_IDENTIFIER[1]))
            {
                //MLog.Log(null,"Ignoring key=" + e.Keyboard.vKey + " from device=" + e.Keyboard.deviceName);
                return;
            }

            try
            {
                RemotePipiCommand cmdRemote;
                cmdRemote = RemotePipi.GetCommandByCode(kd.Key);
                zoneId = MZPState.Instance.GetZoneByControlDevice(kd.Device);
                MLog.Log(null, "DO key event key=" + kd.Key + " device=" + kd.Device + " keyup="+kd.IsKeyUp + " keydown="+kd.IsKeyDown
                    +" apicmd="+cmdRemote+(cmdRemote==null?" IGNORING CMD":"") + " zoneid="+zoneId);
                if (cmdRemote == null)
                {
                    //MLog.Log(null, "Hook command not found key=" + kd.Key);
                    return;
                }
               
                //check if is not a numeric key
                short intResult;
                string apicmd;
                if (Int16.TryParse(cmdRemote.CommandName, out intResult))
                    apicmd = "k" + intResult;
                else
                    apicmd = cmdRemote.CommandName.ToLower();

                Metadata.ValueList retvalue = null;
                Metadata.ValueList val = new Metadata.ValueList(Metadata.GlobalParams.zoneid, zoneId.ToString(), Metadata.CommandSources.rawinput);
                val.Add(Metadata.GlobalParams.command, apicmd);
				val.Add(Metadata.GlobalParams.cmdsource, Metadata.CommandSources.rawinput.ToString());
                Thread th = new Thread(() => DoCommand(val, out retvalue));
                th.Name = "RawInput Key " + cmdRemote.CommandName;
                th.Start();
            }
            catch (Exception ex)
            {
                MLog.Log(ex, "Error keypressed key=" + kd.Key + " zoneid=" + zoneId.ToString()
                    + " mostrecentzone=" + MZPState.Instance.MostRecentZoneWithContext);
            }
        }

        public static String DoCommandDirect(Metadata.ValueList vals, out Metadata.ValueList retvalue)
        {
            return DoCommand(vals, out retvalue);
        }

        public static String DoCommandFromGUIInput(String cmdName, String zoneId)
        {
            Metadata.ValueList vals = new Metadata.ValueList(Metadata.GlobalParams.zoneid, zoneId, Metadata.CommandSources.gui);
            vals.Add(Metadata.GlobalParams.command, cmdName);
            Metadata.ValueList resvalue;
            
            String result = DoCommand(vals, out resvalue);
            return result;
        }

        public static String DoCommandFromGUIInput(Metadata.ValueList vals)
        {
            Metadata.ValueList resvalue;
            String result = DoCommand(vals, out resvalue);
            return result;
        }

        public static String DoCommandFromWeb(Metadata.ValueList vals, Metadata.CommandSources cmdSourceEnum, out Metadata.ValueList resvalue)
        {
            String result;
            result = DoCommand(vals, out resvalue);
            return result;
        }

        public static String DoCommandFromWeb(Metadata.ValueList vals, out Metadata.ValueList resvalue)
        {
            return DoCommand(vals, out resvalue);
        }
        
        private static String DoCommand(Metadata.ValueList vals, out Metadata.ValueList retvals)
        {
            Metadata.ResultEnum res;
            Metadata.ValueList values, resvalue=null;
            Metadata.CommandResult cmdres;
            String cmdName;
            String result,err,detailedStatus="";

            try
            {
                MZPState.Instance.PowerControl.ResumeFromPowerSaving();
                //zoneId = Convert.ToInt16(vals.GetValue(Metadata.GlobalParams.zoneid));
                cmdName = vals.GetValue(Metadata.GlobalParams.command);
                bool isCmdDefined = Enum.IsDefined(typeof(Metadata.GlobalCommands), cmdName);
                
                if (isCmdDefined)
                {
                    Metadata.GlobalCommands apicmd = (Metadata.GlobalCommands)Enum.Parse(typeof(Metadata.GlobalCommands), cmdName);
                    if (!Metadata.CommandSyntax.Validate(vals))
                    {
                        MLog.Log(null, "ERROR, potential invalid command");
                    }
                    
                    //global commands are processed here, zone based commands are delegated to zones
                    switch (apicmd)
                    {
                        case Metadata.GlobalCommands.help:
                            foreach (String item in Enum.GetNames(typeof(Metadata.GlobalCommands)))
                            {
                               detailedStatus += item + ";" ;
                            }
                            resvalue = new Metadata.ValueList(Metadata.GlobalParams.msg, "Available commands: " + detailedStatus, Metadata.CommandSources.web);
                            result = JsonResult(Metadata.ResultEnum.OK, "", null);
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
                            res = DoZoneCommand(apicmd, vals, out err, out values);
                            resvalue = values;
                            result = JsonResult(res, err, values);
                            break;
                        case Metadata.GlobalCommands.sleep:
                            res = GoToSleep(out err);
                            result = JsonResult(res, err, null);
                            break;
                        case Metadata.GlobalCommands.status:
                            foreach (Metadata.ZoneDetails zone in MZPState.Instance.ZoneDetails)
                            {
                                detailedStatus += "\r\n" + zone.SummaryStatus;
                            }
                            result = JsonResult(Metadata.ResultEnum.OK, detailedStatus, null);
                            break;
                        case Metadata.GlobalCommands.genrelist:
                            resvalue = MediaLibrary.MusicGenres;
                            result = JsonResult(Metadata.ResultEnum.OK, "", resvalue);
                            break;
                        case Metadata.GlobalCommands.artistlist:
                            resvalue = MediaLibrary.MusicArtists;
                            result = JsonResult(Metadata.ResultEnum.OK, "", resvalue);
                            break;
                        case Metadata.GlobalCommands.alarmarm:
                            String areaid = vals.GetValue(Metadata.GlobalParams.areaid);
                            cmdres = MZPState.Instance.ZoneEvents.SendCommand_PARADOX(ZoneEvents.EnumParadoxCommands.Arm, areaid);
                            result = JsonResult(cmdres.Result, cmdres.ErrorMessage, null);
                            break;
                        case Metadata.GlobalCommands.alarmdisarm:
                            cmdres = MZPState.Instance.ZoneEvents.SendCommand_PARADOX(ZoneEvents.EnumParadoxCommands.Disarm, vals.GetValue(Metadata.GlobalParams.areaid));
                            result = JsonResult(cmdres.Result, cmdres.ErrorMessage, null);
                            break;
                        case Metadata.GlobalCommands.alarmkeypadbeep:
                            cmdres = MZPState.Instance.ZoneEvents.SendCommand_PARADOX(ZoneEvents.EnumParadoxCommands.KeypadBeep, vals.GetValue(Metadata.GlobalParams.areaid));
                            result = JsonResult(cmdres.Result, cmdres.ErrorMessage, null);
                            break;
                        case Metadata.GlobalCommands.alarmstay:
                            cmdres = MZPState.Instance.ZoneEvents.SendCommand_PARADOX(ZoneEvents.EnumParadoxCommands.Stay, vals.GetValue(Metadata.GlobalParams.areaid));
                            result = JsonResult(cmdres.Result, cmdres.ErrorMessage, null);
                            break;
                        case Metadata.GlobalCommands.restartispy:
                            Utilities.CloseProcSync(IniFile.PARAM_ISPY_PROCNAME[1]);
                            Utilities.CloseProcSync(IniFile.PARAM_ISPY_OTHERPROC[1]);
                            Utilities.RunProcessWait(IniFile.PARAM_ISPY_APP_PATH[1]);
                            resvalue = new Metadata.ValueList(Metadata.GlobalParams.msg, "all ok", Metadata.CommandSources.system);
                            result = JsonResult(Metadata.ResultEnum.OK, "", null);
                            break;
                        case Metadata.GlobalCommands.restartwinload:
                            MZPState.RestartWinload();
                            resvalue = new Metadata.ValueList(Metadata.GlobalParams.msg, "all ok", Metadata.CommandSources.system);
                            result = JsonResult(Metadata.ResultEnum.OK, "", null);
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
                            result = JsonResult(Metadata.ResultEnum.OK, detailedStatus, null);
                            //resvalue = new Metadata.ValueList(Metadata.GlobalParams.msg, status, Metadata.CommandSources.Internal);
                            break;
                        case Metadata.GlobalCommands.sendsms:
                            String message = vals.GetValue(Metadata.GlobalParams.msg);
                            MZPState.Instance.m_messengerList.Find(x => x.GetType().Equals(typeof(SMS))).SendMessage(message, IniFile.PARAM_SMS_TARGETNUMBER[1]);
                            result = JsonResult(Metadata.ResultEnum.OK, "", null);
                            break;
                        case Metadata.GlobalCommands.restartsystem:
                            MZPState.RestartComputer("restart command received");
                            result = JsonResult(Metadata.ResultEnum.OK, "", null);
                            break;
                        case Metadata.GlobalCommands.makebuzz:
                            MZPState.Instance.MessengerMakeBuzz();
                            result = JsonResult(Metadata.ResultEnum.OK, "", null);
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
                            result = JsonResult(Metadata.ResultEnum.OK, "power failure=" + failure, null);
                            break;
                        default:
                            res = DoZoneCommand(apicmd, vals, out err, out values);
                            resvalue = values;
                            result = JsonResult(res, err, values);
                            break;
                    }
                }
                else
                {
                    MLog.Log(null, "Error unknown docommand " + cmdName);
                    result = JsonResult(Metadata.ResultEnum.ERR, "Unhandled API Command " + cmdName, null);
                }
            }
            catch (Exception ex)
            {
                MLog.Log(ex, "Error DoCommand");
                result = JsonResult(Metadata.ResultEnum.ERR, "Exception " + ex.Message, null);
            }
            retvals = resvalue;
            return result;
        }

        private static int InferZone(String zoneidentifier, String zonename)
        {
            int zoneId =-1;

            if (zoneidentifier != null)
                zoneId = Convert.ToInt16(zoneidentifier);
            else
            {
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
                MLog.Log(null, "Infered zoneid " + zoneId + " had zoneidentifier="+zoneidentifier + " zonename="+zonename);
                
            }
            return zoneId;
        }

        public static Metadata.ResultEnum DoZoneCommand(Metadata.GlobalCommands apicmd, Metadata.ValueList vals, out String errorMessage, out Metadata.ValueList values)
            //private static String DoZoneCommand(String cmdName, int zoneId, out String errorMessage)
        {
            try
            {
                values = null;
                errorMessage = "";
                int zoneId = InferZone(vals.GetValue(Metadata.GlobalParams.zoneid), vals.GetValue(Metadata.GlobalParams.zonename));

                if (zoneId == -1)
                {
                    errorMessage = "ERROR no zone found to process command" + apicmd;
                    MLog.Log(null, errorMessage);
                    return Metadata.ResultEnum.ERR; ;
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
                        errorMessage = "Zone not active " + zoneId;
                        return Metadata.ResultEnum.ERR;
                    }
                    else
                    {
                        lock (zone)
                        {
                            values = zone.ProcessAction(apicmd, vals);
                        }
                        return Metadata.ResultEnum.OK;
                    }
                }
                else
                {
                    errorMessage = "ControlCenter instance is null";
                    return Metadata.ResultEnum.ERR;
                }
            }
            catch (Exception ex)
            {
                MLog.Log(ex, "Error DoZoneCommand cmd=" + apicmd);
                errorMessage = ex.Message;
                values = null;
                return Metadata.ResultEnum.ERR;
            }
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

        private static String JsonResult(Metadata.ResultEnum result, String errorMessage, Metadata.ValueList values)
        {
            Metadata.CommandResult cmdres = new Metadata.CommandResult();
            cmdres.Result = result;
            cmdres.ErrorMessage = errorMessage;

            Metadata.ServerStatus srv = new Metadata.ServerStatus();
            srv.IsServerOn = true;
            if (MZPState.Instance != null)
            {
                srv.ZoneDetails = MZPState.Instance.ZoneDetails.ToArray();
                srv.CamAlertList = MZPState.Instance.ZoneEvents.CamAlertList;
            }
            cmdres.ServerStatus = srv;
            cmdres.ValueList = values;
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

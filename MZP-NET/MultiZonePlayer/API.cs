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
        private static Hashtable m_hsResult = new Hashtable();
        private static int m_cmdIndex = 0;

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
                MLog.Log(null, "DO key event key=" + kd.Key + " device=" + kd.Device);
                RemotePipiCommand cmdRemote;
                cmdRemote = RemotePipi.GetCommandByCode(kd.Key);
                if (cmdRemote == null)
                {
                    MLog.Log(null, "Hook command not found key=" + kd.Key);
                    //ControlCenter.PlayBeepError();
                    return;
                }
                
                //ControlCenter.PlayBeepOK();

                zoneId = MZPState.Instance.GetZoneByControlDevice(kd.Device);
                //set current zone
                /*
                if (zoneId == -1)
                {
                    zoneId = ControlCenter.Instance.CurrentZoneMusicId;
                    if (zoneId == -1)
                        zoneId = ControlCenter.Instance.CurrentZoneStreamId;
                    MLog.Log(null, "Switch to default music zone id="+zoneId+" , unknown input device " + kd.Key + " device=" + kd.Device);
                }
                */
                if (zoneId == -1)
                {
                    MLog.Log(null,"Unknown zone, device name=" + kd.Device + " key=" + kd.Key, null);
                }
                //else
                //{
                    MLog.Log(null, "Event is on zoneid=" + zoneId);
                    int index = GetResultIndex();
                    //ControlCenter.Instance.CurrentZoneId = zoneId;
                    Metadata.ValueList val = new Metadata.ValueList(Metadata.GlobalParams.zoneid, zoneId.ToString(), Metadata.CommandSources.RawInput);
                    val.Add(Metadata.GlobalParams.command, cmdRemote.CommandName.ToLower());
                    Thread th = new Thread(() => DoCommandAsynch(index, val));
                    th.Start();
                //}
            }
            catch (Exception ex)
            {
                MLog.Log(ex, "Error keypressed key=" + kd.Key + " zoneid=" + zoneId.ToString()
                    + " currentzonemusic=" + ControlCenter.Instance.CurrentZoneMusicId + " currzonestreamid=" + ControlCenter.Instance.CurrentZoneStreamId);
            }
        }

        public static String DoCommandDirect(Metadata.ValueList vals, out Metadata.ValueList retvalue)
        {
            return DoCommand(-1, vals, out retvalue);
        }

        public static String DoCommandFromGUIInput(String cmdName, String zoneId)
        {
            int index = GetResultIndex();
            Metadata.ValueList vals = new Metadata.ValueList(Metadata.GlobalParams.zoneid, zoneId, Metadata.CommandSources.GUI);
            vals.Add(Metadata.GlobalParams.command, cmdName);
            Thread th = new Thread(() => DoCommandAsynch(index, vals));
            th.Start();
            Metadata.ValueList resvalue;
            String result = WaitForResult(index, out resvalue);
            return result;
        }

        public static String DoCommandFromGUIInput(Metadata.ValueList vals)
        {
            int index = GetResultIndex();
            Thread th = new Thread(() => DoCommandAsynch(index, vals));
            th.Start();
            Metadata.ValueList resvalue;
            String result = WaitForResult(index, out resvalue);
            return result;
        }

        public static String DoCommandFromWeb(Metadata.ValueList vals, out Metadata.ValueList resvalue)
        {
            int index = GetResultIndex();
            DoCommandAsynch(index, vals);
            String result = WaitForResult(index, out resvalue);
            return result;
        }

        private static int GetResultIndex()
        {
            m_cmdIndex++;
            return m_cmdIndex;
        }

        private static void AddResult(int index, String result, Metadata.ValueList resvalue)
        {
            m_hsResult.Add(index, new CmdParams(result, resvalue));
        }

        private static String WaitForResult(int index, out Metadata.ValueList resvalue)
        {
            do
            {
                Application.DoEvents();
                Thread.Sleep(1);
            }
            while (!m_hsResult.ContainsKey(index) && MZPState.IsInitialised);

            String result = (String)(m_hsResult[index] as CmdParams).Name;
            resvalue = (Metadata.ValueList)(m_hsResult[index] as CmdParams).Value;
            m_hsResult.Remove(index);
            return result;
        }

        delegate void DoCommandAsynchDelegate(int cmdIndex, Metadata.ValueList vals);
        private static void DoCommandAsynch(int cmdIndex, Metadata.ValueList vals)
        {
            
            if (ControlCenter.Instance.InvokeRequired)
            {
                DoCommandAsynchDelegate dlg = new DoCommandAsynchDelegate(DoCommandAsynch);
                //ControlCenter.Instance.Invoke(dlg, cmdIndex, vals, resvalue);
                ControlCenter.Instance.BeginInvoke(dlg, cmdIndex, vals);//???????????????????
                //dlg.EndInvoke(out resvalue, ar);
            }
            else
            {
                Metadata.ValueList unused;
                DoCommand(cmdIndex, vals, out unused);
            }
        }

        private static String DoCommand(int cmdIndex, Metadata.ValueList vals, out Metadata.ValueList retvals)
        {

            Metadata.ResultEnum res;
            Metadata.ValueList values, resvalue=null;
            Metadata.CommandResult cmdres;
            String cmdName;
            String result,err,detailedStatus="";

            try
            {
                MZPState.Instance.PowerControl.ResumeFromPowerSaving();

                cmdName = vals.GetValue(Metadata.GlobalParams.command);
                if (Enum.IsDefined(typeof(Metadata.GlobalCommands), cmdName))
                {
                    Metadata.GlobalCommands apicmd = (Metadata.GlobalCommands)Enum.Parse(typeof(Metadata.GlobalCommands), cmdName);
                    //global commands are processed here, zone based commands are delegated to zones
                    switch (apicmd)
                    {
                        case Metadata.GlobalCommands.help:
                            foreach (String item in Enum.GetNames(typeof(Metadata.GlobalCommands)))
                            {
                               detailedStatus += item + ";" ;
                            }
                            resvalue = new Metadata.ValueList(Metadata.GlobalParams.msg, "Available commands: " + detailedStatus, Metadata.CommandSources.Web);
                            result = JsonResult(Metadata.ResultEnum.OK, "", null);
                            break;
                        case Metadata.GlobalCommands.selectzone:
                            SelectZone(Convert.ToInt16(vals.GetValue(Metadata.GlobalParams.zoneid)), vals.GetValue(Metadata.GlobalParams.activity));
                            result = JsonResult(Metadata.ResultEnum.OK, "", null);
                            break;
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
                            cmdres = MZPState.Instance.ZoneEvents.SendCommand_PARADOX(ZoneEvents.EnumParadoxCommands.Arm, vals.GetValue(Metadata.GlobalParams.areaid));
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
                            resvalue = new Metadata.ValueList(Metadata.GlobalParams.msg, "all ok", Metadata.CommandSources.Internal);
                            result = JsonResult(Metadata.ResultEnum.OK, "", null);
                            break;
                        case Metadata.GlobalCommands.restartwinload:
                            MZPState.RestartWinload();
                            //Thread.Sleep(2000);
                            //MZPState.Instance.ZoneEvents.SendLoginCmd();
                            //Thread.Sleep(500);
                            //MZPState.Instance.ZoneEvents.OpenDefaultAlarmAccount();
                            //Thread.Sleep(500);
                            //MZPState.Instance.ZoneEvents.ConnectDirect();
                            //Thread.Sleep(5000);
                            //MZPState.Instance.ZoneEvents.SelectMonitoringTab();
                            resvalue = new Metadata.ValueList(Metadata.GlobalParams.msg, "all ok", Metadata.CommandSources.Internal);
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
                                    MZPEvent.EventType.Security, MZPEvent.EventImportance.Critical);
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
            if (cmdIndex >=0)
                AddResult(cmdIndex, result, resvalue);
            return result;
        }

        public static Metadata.ResultEnum DoZoneCommand(Metadata.GlobalCommands apicmd, Metadata.ValueList vals, out String errorMessage, out Metadata.ValueList values)
            //private static String DoZoneCommand(String cmdName, int zoneId, out String errorMessage)
        {
            try
            {
                String zoneIdStr = vals.GetValue(Metadata.GlobalParams.zoneid);
                int zoneId = -1;
                if (zoneIdStr != null)
                    zoneId = Convert.ToInt16(zoneIdStr);
                else
                {
                    String zonename = vals.GetValue(Metadata.GlobalParams.zonename);
                    Metadata.ZoneDetails zonedetails;
                    if (zonename != null)
                    {
                        zonedetails = MZPState.Instance.ZoneDetails.Find(x => x.ZoneName.ToLower().Contains(zonename.ToLower()));
                        if (zonedetails != null)
                            zoneId = zonedetails.ZoneId;
                    }
                }
                if (zoneId == -1)
                {
                    zoneId = ControlCenter.Instance.CurrentZoneMusicId;
                    if (zoneId == -1)
                        zoneId = ControlCenter.Instance.CurrentZoneStreamId;
                    MLog.Log(null, "Zoneid in command was -1, found active zone " + zoneId);
                }

                //vals.Values.RemoveAt(0);
                ZonesForm zone;
                SelectZone(zoneId, null);
                zone = ControlCenter.Instance.GetZone(zoneId);
                if (zone == null)
                {
                    
                    MLog.Log(null, "No current zone for cmd=" + apicmd + " zoneid=" + zoneId);
                    //ControlCenter.PlayErrorMessage("Select a zone first", null);
                    errorMessage = "Zone not active " + zoneId;
                    values = null;
                    return Metadata.ResultEnum.ERR;
                }
                
                
                values = zone.ProcessAction(apicmd, vals);
                ControlCenter.Instance.RefreshState();
                errorMessage = "";
                return Metadata.ResultEnum.OK;
            }
            catch (Exception ex)
            {
                MLog.Log(ex, "Error DoZoneCommand cmd=" + apicmd);
                errorMessage = ex.Message;
                values = null;
                return Metadata.ResultEnum.ERR;
            }
        }

        private static void SelectZone(int zoneId, string cmd)
        {
            //MLog.Log(null, "API selecting zone " + zoneId);
            if (!ControlCenter.Instance.IsZoneActive(zoneId))
            {
                ControlCenter.Instance.OpenZone(zoneId, cmd);
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
            //if (values != null)
            //    values.Values.RemoveAll(delegate (String v) {return v == null;});
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MultiZonePlayer
{
    public class ZoneEvents
    {
        private List<CamAlert> m_camAlarmList;

        public ZoneEvents()
        {
            m_camAlarmList = new List<CamAlert>();
        }

        #region CamCode

        
        public void AddCamAlert(ValueList vals)
        {
            String camId = vals.GetValue(GlobalParams.oid);
            ZoneDetails zone = ZoneDetails.ZoneDetailsList.Find(x => x.CameraId.Equals(camId));
            
            //MLog.Log(this, message);
            if (zone != null)
            {
                CamAlert alarm = new CamAlert(vals.GetValue(GlobalParams.alertsource),
                    vals.GetValue(GlobalParams.msg), camId, zone.ZoneId, zone.CameraAlertActive);
                zone.LastCamAlertDateTime = alarm.AlarmTime;
                m_camAlarmList.Add(alarm);
                
            }
            else
                MLog.Log(this, "error zone not found for camalert, oid="+camId);
        }

        public void DismissAlert(ValueList vals)
        {
            if (vals.ContainsKey(GlobalParams.alertindex))
            {
                int index = Convert.ToInt16(vals.GetValue(GlobalParams.alertindex));
                m_camAlarmList.Find(x => x.Index == index).WasAcknowledged = true;
            }

            //dismiss multiple alerts
            if (vals.IndexList != null)
            {
                foreach (String index in vals.IndexList)
                {
                    m_camAlarmList.Find(x => x.Index == Convert.ToInt16(index)).WasAcknowledged = true;
                }
            }

            if (vals.ContainsKey(GlobalParams.alertsource))
            {
                String source = vals.GetValue(GlobalParams.alertsource);
                var list = m_camAlarmList.FindAll(x => x.AlarmSource == source).ToList();

                foreach (CamAlert alert in list)
                {
                    alert.WasAcknowledged = true;
                }
            }
        }

        public void ToggleAlertStatus(ValueList vals)
        {
            int zoneId = Convert.ToInt16(vals.GetValue(GlobalParams.zoneid));
            ZoneDetails zone = ZoneDetails.ZoneDetailsList.Find(x => x.ZoneId == zoneId);
            zone.CameraAlertActive = !zone.CameraAlertActive;
        }

        public List<CamAlert> CamAlertList
        {
            get
            {
                return m_camAlarmList.FindAll(x => (x.WasAcknowledged == false) && (x.WasIgnored == false)).OrderByDescending(x => x.AlarmTime).ToList();
            }
        }
        #endregion


        #region AlarmEvents

        public enum EnumParadoxCommands
        {
            KeypadBeep=Keys.K,
            Arm = Keys.A,
            Disarm = Keys.D,
            Stay = Keys.S
        }

        public void Tail_MoreData_PARADOX(object tailObject, string newData)
        {
            /*
            if (newData.Length<1024)
                MLog.Log(this, "New data received:" + newData);
            else
                MLog.Log(this, "New data received, too large to display, length=" + newData.Length);
            */
            String[] lines = newData.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            //MLog.Log(this, "Parsing event lines count:" + lines.Length);
            String[] atoms;

            string[] sep = new string[] { "  ", "   " };
            try
            {
                foreach (String line in lines)
                {
                    atoms = line.ToLower().Split(sep, StringSplitOptions.RemoveEmptyEntries);

                    if (atoms.Length >= 6)
                    {
                        NewMotionAction_PARADOX(atoms[0].Replace(" ", ""), atoms[1].Replace(" ", ""), atoms[2].Replace(" ", ""),
                            atoms[3].Replace(" ", ""), atoms[4].Replace(" ", ""), atoms[5].Replace(" ", ""), lines.Length);
                    }


                }
            }
            catch (Exception ex)
            {
                MLog.Log(ex, this, "err tailmoredata");
            }
            //MLog.Log(this, "New data received parse completed");
        }
        // action=Restore/New, scope=area/zone, state=Opened/Closed
        private void NewMotionAction_PARADOX(String date, String time, String action, String scope, String alarmzoneid, String state, int eventsBulkCount)
        {
            try
            {
                DateTime eventDateTime = Convert.ToDateTime(date + " " + time);
				if (MZPState.Instance.IsWinloadLoading)
					MZPState.Instance.IsWinloadLoading = false;

                if (DateTime.Now.Subtract(eventDateTime).TotalMinutes < 15)//ignore older events
                {

                    Alarm.EnumScope sc = (Alarm.EnumScope)Enum.Parse(typeof(Alarm.EnumScope), scope);
                    ValueList vals;

                    if (DateTime.Now.Subtract(eventDateTime).Duration().TotalMinutes < 5)
                        MZPState.Instance.SystemAlarm.LastAlarmEventDateTime = eventDateTime;
                    //else
                    //    MLog.Log(this, "Past PARADOX alarm event received, timestamp ignored");
                    switch (sc)
                    {
                        case Alarm.EnumScope.zone:
                            int zoneId = MZPState.Instance.GetZoneIdByAlarmZoneId(Convert.ToInt16(alarmzoneid));
                            if (zoneId != -1)
                            {
                                vals = new ValueList(GlobalParams.command, GlobalCommands.alarmevent.ToString(), CommandSources.events);
                                vals.Add(GlobalParams.zoneid, zoneId.ToString());
                                vals.Add(GlobalParams.datetime, date + " " + time);
                                vals.Add(GlobalParams.action, action);
                                vals.Add(GlobalParams.status, state);
                                vals.Add(GlobalParams.scope, scope);
                                vals.Add(GlobalParams.alertsource, IniFile.PARAM_PARADOX_WINLOAD_DATA_FILE[0]);
                                API.DoCommand(vals);

                                //if (eventsBulkCount < 10)
                                //    MZPState.Instance.LogEvent(eventDateTime, MZPEvent.EventSource.Alarm, action + " ZoneEvent " + MZPState.Instance.GetZoneById(zoneId).ZoneName + " is " + state,
                                //        MZPEvent.EventType.Security, MZPEvent.EventImportance.Informative);
                            }

                            break;
                        case Alarm.EnumScope.area:
							Alarm.EnumAreaState areastate;
							if (!Enum.TryParse<Alarm.EnumAreaState>(state, out areastate))
							{
								MLog.Log(this, "Unknown area state "+state);
								areastate = Alarm.EnumAreaState.UNKNOWN;
							}
                            //areastate = (Alarm.EnumAreaState)Enum.Parse(typeof(Alarm.EnumAreaState), state);
                            MZPState.Instance.SystemAlarm.AreaState = areastate;
                            MZPState.Instance.SystemAlarm.LastAreaStateChange = eventDateTime;
							/*switch (areastate)
							{
								case Alarm.EnumAreaState.armed:
									MZPState.Instance.SystemAlarm.IsArmed = true;
									break;
								case Alarm.EnumAreaState.entrydelayfinished:
									MZPState.Instance.SystemAlarm.IsArmed = false;
									break;
							}*/
							switch (areastate)
							{
								case Alarm.EnumAreaState.alarm:
								case Alarm.EnumAreaState.sirenon:
									Alert.CreateAlert(action + " AreaEvent " + areastate.ToString() + " is " + state, null, false, null,
										Alert.NotificationFlags.NotifyUserAfterXSeconds, 1);
									break;
							}
							
                            //MZPState.Instance.LogEvent(eventDateTime, MZPEvent.EventSource.Alarm, action + " AreaEvent " + areastate.ToString() + " is " + state, 
                            //    MZPEvent.EventType.Security, MZPEvent.EventImportance.Informative, null);
                            break;
                        case Alarm.EnumScope.trouble:
							Alert.CreateAlert(action + " TroubleEvent " + state, null, false, null,
								Alert.NotificationFlags.NotifyUserAfterXSeconds, 1);
                            //MZPState.Instance.LogEvent(eventDateTime, MZPEvent.EventSource.Alarm, action + " TroubleEvent " + state, MZPEvent.EventType.Security,
                            //    MZPEvent.EventImportance.Informative, null);
                            break;
                        default:
                            MLog.Log(this, "unknown alarm scope " + scope + " action=" + action + " state=" + state);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MLog.Log(ex, this, "Error newmotion:" + action + "-" + scope + "-" + alarmzoneid + "-" + state);
            }
            
        }

        public CommandResult SendCommand_PARADOX(EnumParadoxCommands cmd, String areaid)
        {
            CommandResult res = new CommandResult();
            IntPtr handle = Utilities.FindWindow("TMainForm", "WinLoad");
            //find and show winload toolbar command menu
            if (handle.ToInt32() != 0)
            {
                ApiWindow hw = FindControl(handle.ToInt32(), "TBitBtn", "", "TPanel", areaid);
                if (hw.hWnd != 0)
                {
                    MLog.Log(null, "focus=" + Utilities.PostMessage((IntPtr)hw.hWnd, Utilities.WM_SETFOCUS, IntPtr.Zero, IntPtr.Zero));
                    Thread.Sleep(50);
                    MLog.Log(null, "press=" + Utilities.PostMessage((IntPtr)hw.hWnd, Utilities.WM_KEYDOWN, (IntPtr)Utilities.VK_RETURN, IntPtr.Zero));
                    Thread.Sleep(50);
                    //find toolbar with commands and execute winload command
                    handle = Utilities.FindWindow("TfrmShowArmingBtn", "ACTION");
                    if (handle.ToInt32() != 0)
                    {
                        MLog.Log(null, "found handle " + handle);
                        MLog.Log(null, "focus=" + Utilities.PostMessage((IntPtr)handle, Utilities.WM_SETFOCUS, IntPtr.Zero, IntPtr.Zero));
                        Thread.Sleep(50);
                        MLog.Log(null, "key down " + Utilities.PostMessage((IntPtr)handle, Utilities.WM_KEYDOWN, (IntPtr)cmd, IntPtr.Zero));
                        res.Result = ResultEnum.OK;
                    }
                    else
                    {
                        res.ErrorMessage = "Unable to locate just opened commands toolbar";
                        res.Result = ResultEnum.ERR;
                        MLog.Log(this, res);
                    }
                }
                else
                {
                    res.ErrorMessage = "Unable to locate show commands button";
                    res.Result = ResultEnum.ERR;
                    MLog.Log(this, res);
                }
            }
            else
            {
                res.ErrorMessage = "Unable to locate winload";
                res.Result = ResultEnum.ERR;
                MLog.Log(this, res);
            }
            return res;
        }

        /*
        public void SendLoginCmd()
        {
            IntPtr handle = Utilities.FindWindow("TPasswordDlg", "1");
            ApiWindow hw = FindControl(handle.ToInt32(), "TEdit", "", "TEdit", "");
            
            MLog.Log(null, "focus handle="+handle + " hw=" + hw.hWnd + Utilities.PostMessage((IntPtr)hw.hWnd, Utilities.WM_SETFOCUS, IntPtr.Zero, IntPtr.Zero));
            for (int i = 0; i < IniFile.PARAM_PARADOX_WINLOAD_USERNAME[1].Length; i++)
            {
                Thread.Sleep(50);
                MLog.Log(null, "press=" + Utilities.PostMessage((IntPtr)hw.hWnd, Utilities.WM_KEYDOWN, (IntPtr)IniFile.PARAM_PARADOX_WINLOAD_USERNAME[1][i], IntPtr.Zero));
            }
            hw = FindControl(handle.ToInt32(), "TEdit", "", null, null);
            for (int i = 0; i < IniFile.PARAM_PARADOX_WINLOAD_USERPASS[1].Length; i++)
            {
                Thread.Sleep(50);
                MLog.Log(null, "press=" + Utilities.PostMessage((IntPtr)hw.hWnd, Utilities.WM_KEYDOWN, (IntPtr)IniFile.PARAM_PARADOX_WINLOAD_USERPASS[1][i], IntPtr.Zero));
            }
            Thread.Sleep(50);      
            MLog.Log(null, "press=" + Utilities.PostMessage((IntPtr)hw.hWnd, Utilities.WM_KEYDOWN, (IntPtr)Utilities.VK_RETURN, IntPtr.Zero));
        }

        public void OpenDefaultAlarmAccount()
        {
            IntPtr handle = Utilities.FindWindow("TMainForm", "WinLoad");
            ApiWindow hw = FindControl(handle.ToInt32(), "TDBGrid", "", null, null);
            MLog.Log(null, "focus handle=" + handle + " hw=" + hw.hWnd + Utilities.PostMessage((IntPtr)hw.hWnd, Utilities.WM_SETFOCUS, IntPtr.Zero, IntPtr.Zero));
            MLog.Log(null, "press=" + Utilities.PostMessage((IntPtr)hw.hWnd, Utilities.WM_KEYDOWN, (IntPtr)Utilities.VK_RETURN, IntPtr.Zero));
        }

        public void ConnectDirect()
        {
            IntPtr handle = Utilities.FindWindow("TMainForm", "WinLoad");
            ApiWindow hw = FindControl(handle.ToInt32(), "TRzBitBtn", "&Connect", null, null);
            MLog.Log(null, "focus handle=" + handle + " hw=" + hw.hWnd + Utilities.PostMessage((IntPtr)hw.hWnd, Utilities.WM_SETFOCUS, IntPtr.Zero, IntPtr.Zero));
            MLog.Log(null, "press=" + Utilities.PostMessage((IntPtr)hw.hWnd, Utilities.WM_KEYDOWN, (IntPtr)Utilities.VK_RETURN, IntPtr.Zero));
        }

        public void SelectMonitoringTab()
        {
            IntPtr handle = Utilities.FindWindow("TMainForm", "WinLoad");
            ApiWindow hw = FindControl(handle.ToInt32(), "TTabSheet", "System", null, null);
            MLog.Log(null, "focus handle=" + handle + " hw=" + hw.hWnd + Utilities.PostMessage((IntPtr)hw.hWnd, Utilities.WM_SETFOCUS, IntPtr.Zero, IntPtr.Zero));
            MLog.Log(null, "press=" + Utilities.PostMessage((IntPtr)hw.hWnd, Utilities.WM_KEYDOWN, (IntPtr)Keys.Right, IntPtr.Zero));
            Thread.Sleep(50);
            hw = FindControl(handle.ToInt32(), "TTabSheet", "EventList", null, null);
            MLog.Log(null, "focus handle=" + handle + " hw=" + hw.hWnd + Utilities.PostMessage((IntPtr)hw.hWnd, Utilities.WM_SETFOCUS, IntPtr.Zero, IntPtr.Zero));
            MLog.Log(null, "press=" + Utilities.PostMessage((IntPtr)hw.hWnd, Utilities.WM_KEYDOWN, (IntPtr)Keys.Right, IntPtr.Zero));
        }
        */
        private ApiWindow FindControl(int parent, String className, String title, String stopClassName, String stopTitle)
        {
            WindowsEnumerator enumerator = new WindowsEnumerator();
            List<ApiWindow> enumer = enumerator.GetChildWindows(parent);
            ApiWindow result = new ApiWindow();

            foreach (ApiWindow child in enumer)
            {
                if (child.ClassName.Equals(className) && ((title == null) || child.MainWindowTitle.Equals(title)))
                    result = child;
                if ((stopClassName != null) && (child.ClassName.Equals(stopClassName) && (child.MainWindowTitle.Equals(stopTitle))))
                    break;
            }
            return result;
        }
          
        #endregion

        public void WinEventLogEntryMatch(String sourceString, String message, DateTime datetime)
        {
            if (sourceString.ToLower().Equals(IniFile.PARAM_UPS_APC_LOG_SOURCE[1].ToLower()))
            {
                Boolean failure = MZPState.Instance.IsPowerFailure;

                if (message.ToLower().Contains(IniFile.PARAM_UPS_APC_LOG_POWERFAILURE_ACTIVE[1].ToLower()))
                {
                    //power failure
                    failure = true;
                }
                else
                {
                    if (message.ToLower().Contains(IniFile.PARAM_UPS_APC_LOG_POWERFAILURE_INACTIVE[1].ToLower()))
                    {
                        //power resumed
                        failure = false;
                    }
                }

                if (failure != MZPState.Instance.IsPowerFailure)
                {
                    ValueList val = new ValueList(GlobalParams.command, GlobalCommands.powerevent.ToString(), CommandSources.system);
                    val.Add(GlobalParams.action, failure.ToString());
                    val.Add(GlobalParams.datetime, datetime.ToString());
                    //Metadata.ValueList retval;
					CommandResult retcmd = API.DoCommandFromWeb(val);//, out retval);
                    //Metadata.CommandResult retcmd = fastJSON.JSON.Instance.ToObject(json) as Metadata.CommandResult;

                }
            }
        }

    }
}

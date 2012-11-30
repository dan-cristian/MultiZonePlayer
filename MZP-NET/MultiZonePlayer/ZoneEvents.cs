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
        private List<Metadata.CamAlert> m_camAlarmList;

        public ZoneEvents()
        {
            m_camAlarmList = new List<Metadata.CamAlert>();
        }

        #region CamCode

        public void AddCamAlert(Metadata.ValueList vals)
        {
            String camId = vals.GetValue(Metadata.GlobalParams.oid);
            Metadata.ZoneDetails zone = MZPState.Instance.ZoneDetails.Find(x => x.CameraId.Equals(camId));
            string message = "Cam alert from camid=" + camId + " zone is " + zone.ZoneName;
            //MLog.Log(this, message);
            if (zone != null)
            {
                Metadata.CamAlert alarm = new Metadata.CamAlert(vals.GetValue(Metadata.GlobalParams.alertsource),
                    vals.GetValue(Metadata.GlobalParams.msg), camId, zone.ZoneId, zone.CameraAlertActive);
                zone.LastCamAlertDateTime = alarm.AlarmTime;
                m_camAlarmList.Add(alarm);
                MZPState.Instance.LogEvent(MZPEvent.EventSource.Cam, message, MZPEvent.EventType.Security, MZPEvent.EventImportance.Informative);
            }
            else
                MLog.Log(this, "error zone not found for camalert, oid="+camId);
        }

        public void DismissAlert(Metadata.ValueList vals)
        {
            if (vals.ContainsKey(Metadata.GlobalParams.alertindex))
            {
                int index = Convert.ToInt16(vals.GetValue(Metadata.GlobalParams.alertindex));
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

            if (vals.ContainsKey(Metadata.GlobalParams.alertsource))
            {
                String source = vals.GetValue(Metadata.GlobalParams.alertsource);
                var list = m_camAlarmList.FindAll(x => x.AlarmSource == source).ToList();

                foreach (Metadata.CamAlert alert in list)
                {
                    alert.WasAcknowledged = true;
                }
            }
        }

        public void ToggleAlertStatus(Metadata.ValueList vals)
        {
            int zoneId = Convert.ToInt16(vals.GetValue(Metadata.GlobalParams.zoneid));
            Metadata.ZoneDetails zone = MZPState.Instance.ZoneDetails.Find(x => x.ZoneId == zoneId);
            zone.CameraAlertActive = !zone.CameraAlertActive;
        }

        public List<Metadata.CamAlert> CamAlertList
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

                if (DateTime.Now.Subtract(eventDateTime).TotalMinutes < 15)//ignore older events
                {

                    Alarm.EnumScope sc = (Alarm.EnumScope)Enum.Parse(typeof(Alarm.EnumScope), scope);
                    Metadata.ValueList vals, resvalue;

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
                                vals = new Metadata.ValueList(Metadata.GlobalParams.command, Metadata.GlobalCommands.alarmevent.ToString(), Metadata.CommandSources.Event);
                                vals.Add(Metadata.GlobalParams.zoneid, zoneId.ToString());
                                vals.Add(Metadata.GlobalParams.datetime, date + " " + time);
                                vals.Add(Metadata.GlobalParams.action, action);
                                vals.Add(Metadata.GlobalParams.status, state);
                                vals.Add(Metadata.GlobalParams.scope, scope);
                                vals.Add(Metadata.GlobalParams.alertsource, IniFile.PARAM_PARADOX_WINLOAD_DATA_FILE[0]);
                                API.DoCommandFromWeb(vals, out resvalue);

                                if (eventsBulkCount < 10)
                                    MZPState.Instance.LogEvent(eventDateTime, MZPEvent.EventSource.Alarm, action + " Zone " + MZPState.Instance.GetZoneById(zoneId).ZoneName + " is " + state,
                                        MZPEvent.EventType.Security, MZPEvent.EventImportance.Informative);
                            }

                            break;
                        case Alarm.EnumScope.area:
                            Alarm.EnumAreaState areastate = (Alarm.EnumAreaState)Enum.Parse(typeof(Alarm.EnumAreaState), state);
                            MZPState.Instance.SystemAlarm.AreaState = areastate;
                            MZPState.Instance.SystemAlarm.LastAreaStateChange = eventDateTime;
                            MZPState.Instance.LogEvent(eventDateTime, MZPEvent.EventSource.Alarm, action + " Area " + areastate.ToString() + " is " + state, 
                                MZPEvent.EventType.Security, MZPEvent.EventImportance.Informative);
                            break;
                        case Alarm.EnumScope.trouble:
                            MZPState.Instance.LogEvent(eventDateTime, MZPEvent.EventSource.Alarm, action + " Trouble " + state, MZPEvent.EventType.Security, MZPEvent.EventImportance.Informative);
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

        public Metadata.CommandResult SendCommand_PARADOX(EnumParadoxCommands cmd, string areaid)
        {
            Metadata.CommandResult res = new Metadata.CommandResult();
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
                        res.Result = Metadata.ResultEnum.OK;
                    }
                    else
                    {
                        res.ErrorMessage = "Unable to locate just opened commands toolbar";
                        res.Result = Metadata.ResultEnum.ERR;
                        MLog.Log(this, res);
                    }
                }
                else
                {
                    res.ErrorMessage = "Unable to locate show commands button";
                    res.Result = Metadata.ResultEnum.ERR;
                    MLog.Log(this, res);
                }
            }
            else
            {
                res.ErrorMessage = "Unable to locate winload";
                res.Result = Metadata.ResultEnum.ERR;
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
            if (sourceString.ToLower().Equals(IniFile.PARAM_UPS_LOG_SOURCE[1].ToLower()))
            {
                Boolean failure = MZPState.Instance.IsPowerFailure;

                if (message.ToLower().Contains(IniFile.PARAM_UPS_LOG_POWERFAILURE_ACTIVE[1].ToLower()))
                {
                    //power failure
                    failure = true;
                }
                else
                {
                    if (message.ToLower().Contains(IniFile.PARAM_UPS_LOG_POWERFAILURE_INACTIVE[1].ToLower()))
                    {
                        //power resumed
                        failure = false;
                    }
                }

                if (failure != MZPState.Instance.IsPowerFailure)
                {
                    Metadata.ValueList val = new Metadata.ValueList(Metadata.GlobalParams.command, Metadata.GlobalCommands.powerevent.ToString(), Metadata.CommandSources.Internal);
                    val.Add(Metadata.GlobalParams.action, failure.ToString());
                    val.Add(Metadata.GlobalParams.datetime, datetime.ToString());
                    Metadata.ValueList retval;
                    String json = API.DoCommandFromWeb(val, out retval);
                    Metadata.CommandResult retcmd = fastJSON.JSON.Instance.ToObject(json) as Metadata.CommandResult;

                }
            }
        }

    }
}

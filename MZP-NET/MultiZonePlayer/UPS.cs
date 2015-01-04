using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace MultiZonePlayer {
    public abstract class GenericUPS {
        public class UPSState {
            public Boolean LastPowerFailDateTime;
            //common
            public string Temperature, BatteryVoltage;
            public Boolean PowerFail, TestInProgress;
            //Mustek only
            public string IPVoltage, IPFaultVoltage, OPVoltage, OPCurrent, IPFrequency, /*BatteryVoltage,*/ UPSStatus;
            public Boolean BatteryLow, AVR, UPSFailed, StandbyUPS, ShutdownActive, BeeperOn;
            //Niky only
            public String InputVoltage, RemainingMinutes, OutputVoltage, LoadPercent, PowerFrequency, OtherStatus;
        }

        public UPSState LastStatus = new UPSState();
        public abstract void Initialise();
        public abstract void GetStatus();
        public abstract String Name();
    }

    public class NikysUPS : GenericUPS, IMZPDevice {
        private SimpleSerialConnector m_serialConnector;
        private string m_lastResponse;
        public enum COMMANDS {
            GETUPSSTATUS_Q,
            GETUPSINFO_I,
            GETRATING_F,
            GETBATTERY_BTT,
            BEEPERTOOGLE_Q,
            CANCELSHUTDOWN_C,
            TESTTENSEC_T
        }
        //status bits, 0 default value, 1 for activity
        //01234(ups on?)5(ups on battery/power fail)67(beeper off)

        public NikysUPS() {
            Initialise();
        }
        public bool IsFunctional() {
            return !m_serialConnector.IsFaulty();
        }

        public bool IsEnabled() {
            return true;
        }

        public void Enable() {
        }

        public void Disable() {
        }

        public string Type() {
            return IniFile.DEVICE_TYPE_UPS;
        }

        public override string Name() {
            return this.GetType().Name;
        }

        public override void Initialise() {
            //note it works only with CR (CR+LF gives timeouts)
            m_serialConnector = new SimpleSerialConnector("COM1", "2400", CommunicationManager.TransmissionType.TextR);
            m_serialConnector.StandardTimeoutMili = 3000;
            if (m_serialConnector.TestConnection()) {
                MLog.Log(this, Name() + " connected succesfully to port ");
                //FIRST COMMAND does not get a response from UPS for some reason
                m_serialConnector.SendCommand(COMMANDS.GETUPSINFO_I, "");
            }
        }

        public override void GetStatus() {
            String res = m_serialConnector.SendCommand(COMMANDS.GETUPSSTATUS_Q, "1");
            MLog.Log(this,"NIKY=" + res);
            string[] values = res.Split(' ');
            if (values.Length >= 8) {
                LastStatus.InputVoltage = values[0];
                LastStatus.RemainingMinutes = values[1];
                LastStatus.OutputVoltage = values[2];
                LastStatus.LoadPercent = values[3];
                LastStatus.PowerFrequency = values[4];
                LastStatus.BatteryVoltage = values[5];
                LastStatus.Temperature = values[6];
                LastStatus.OtherStatus= values[7];
                if (LastStatus.OtherStatus.Length >= 8) {
                    LastStatus.PowerFail = (LastStatus.OtherStatus[0] == '1');
                    LastStatus.TestInProgress = (LastStatus.OtherStatus[5] == '1');
                    LastStatus.BeeperOn = (LastStatus.OtherStatus[7] == '1');
                }

                if (m_lastResponse != res && res != Constants.STR_TIMEOUT) {
                    DB.WriteRecord(DB.TABLE_POWERFEED, DB.COL_POWERFEED_DATETIME.ToString(), DateTime.Now,
                            DB.COL_POWERFEED_inputvoltage.ToString(), LastStatus.InputVoltage,
                            DB.COL_POWERFEED_loadpercent.ToString(), LastStatus.LoadPercent,
                            DB.COL_POWERFEED_outputvoltage.ToString(), LastStatus.OutputVoltage,
                            DB.COL_POWERFEED_powerfailed.ToString(), LastStatus.PowerFail,
                            DB.COL_POWERFEED_remainingminutes.ToString(), LastStatus.RemainingMinutes,
                            DB.COL_POWERFEED_powersource.ToString(), Name());
                    m_lastResponse = res;
                }
            }
            else MLog.Log("Unexpected number of values from UPS received, cnt= " + values.Length);
        }
    }

    public class APCUPS : GenericUPS, IMZPDevice {
        private WinEventLogReader m_winEventLogReader;
        private String m_eventLog, m_eventSource;

        public APCUPS(String eventLog, String eventSource) {
            m_eventLog = eventLog;
            m_eventSource = eventSource;
            Initialise();
        }

        public override void Initialise() {
            m_winEventLogReader = new WinEventLogReader(m_eventLog);
            m_winEventLogReader.AddSource(m_eventSource);
        }

        public override void GetStatus() {
        }

        public bool IsFunctional() {
            return m_winEventLogReader != null && DateTime.Now.Subtract(m_winEventLogReader.LastLogEntryDate).TotalDays <= 1;
        }

        public bool IsEnabled() {
            throw new NotImplementedException();
        }

        public void Enable() {
            throw new NotImplementedException();
        }

        public void Disable() {
            throw new NotImplementedException();
        }

        public string Type() {
            return IniFile.DEVICE_TYPE_UPS;
        }

        public override string Name() {
            return m_eventSource;
        }
    }

    public class MustekUPS : GenericUPS, IMZPDevice {
        private string m_statusurl, m_lastHtml = "";
        private WebClient m_client;
        private DateTime m_lastErrorReadStatus = DateTime.MinValue;

        public MustekUPS(String statusurl) {
            m_statusurl = statusurl;
            Initialise();
        }

        public override void Initialise() {
            m_client = new WebClient();
            GetStatus();
        }

        public override void GetStatus() {
            try {
                m_lastHtml = m_client.DownloadString(m_statusurl);
                string[] atoms, pairs;
                Boolean failure = LastStatus.PowerFail;

                atoms = m_lastHtml.Split(new string[] { "</br>\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string atom in atoms) {
                    pairs = atom.Replace(" ", "").Split('=');
                    if (pairs.Length > 1) {
                        switch (pairs[0]) {
                            case "I/PVoltage":
                                LastStatus.IPVoltage = pairs[1];
                                break;
                            case "I/PFaultVoltage":
                                LastStatus.IPFaultVoltage = pairs[1];
                                break;
                            case "O/PVoltage":
                                LastStatus.OPVoltage = pairs[1];
                                break;
                            case "O/PCurrent":
                                LastStatus.OPCurrent = pairs[1];
                                break;
                            case "I/PFrequency":
                                LastStatus.IPFrequency = pairs[1];
                                break;
                            case "BatteryVoltage":
                                LastStatus.BatteryVoltage = pairs[1];
                                break;
                            case "m_temperature ":
                                LastStatus.Temperature = pairs[1];
                                break;
                            case "UPSStatus":
                                LastStatus.UPSStatus = pairs[1];

                                if (LastStatus.UPSStatus.Length >= 8) {
                                    LastStatus.PowerFail = LastStatus.UPSStatus[0] == '1';
                                    LastStatus.BatteryLow = LastStatus.UPSStatus[1] == '1';
                                    LastStatus.AVR = LastStatus.UPSStatus[2] == '1';
                                    LastStatus.UPSFailed = LastStatus.UPSStatus[3] == '1';
                                    LastStatus.StandbyUPS = LastStatus.UPSStatus[4] == '1';
                                    LastStatus.TestInProgress = LastStatus.UPSStatus[5] == '1';
                                    LastStatus.ShutdownActive = LastStatus.UPSStatus[6] == '1';
                                    LastStatus.BeeperOn = LastStatus.UPSStatus[7] == '1';

                                    failure = LastStatus.PowerFail;
                                }
                                break;
                        }
                    }
                }

                if (failure != MZPState.Instance.IsPowerFailure) {
                    MLog.Log(this, "MUSTEK power event failure=" + LastStatus.PowerFail);
                    ValueList val = new ValueList(GlobalParams.command, GlobalCommands.powerevent.ToString(), CommandSources.system);
                    val.Add(GlobalParams.action, failure.ToString());
                    val.Add(GlobalParams.datetime, DateTime.Now.ToString());
                    CommandResult retcmd = API.DoCommandFromWeb(val); //, out retval);
                    //Metadata.CommandResult retcmd = fastJSON.JSON.ToObject(json) as Metadata.CommandResult;
                }
            }
            catch (Exception ex) {
                if (DateTime.Now.Subtract(m_lastErrorReadStatus).TotalMinutes > 60) {
                    MLog.Log(this, "Unable to read Mustek UPS status, UPS might not be connected, " + ex.Message);
                    m_lastErrorReadStatus = DateTime.Now;
                }
            }
        }

        public bool IsFunctional() {
            return m_lastHtml != "";
        }

        public bool IsEnabled() {
            throw new NotImplementedException();
        }

        public void Enable() {
            throw new NotImplementedException();
        }

        public void Disable() {
            throw new NotImplementedException();
        }

        public string Type() {
            return IniFile.DEVICE_TYPE_UPS;
        }

        public override string Name() {
            return "Mustek";
        }
    }

}

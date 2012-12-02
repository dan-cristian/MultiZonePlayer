using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using agsXMPP;


namespace MultiZonePlayer
{
    public interface IMessenger
    {
        void SendMessage(String message, String targetId);
        void ReceiveMessage(String message, String sender);
        Boolean TestConnection();
        void Reinitialise();
        Boolean IsTargetAvailable();
        void MakeBuzz();
    }

    public abstract class SerialBase
    {
        public String Connection;
        protected CommunicationManager comm;
        protected Boolean m_waitForResponse = false, m_isProcessing = false;
        protected Boolean m_lastOperationWasOK = true;
        protected String m_lastMessageResponse;
        private Boolean m_isSerialDeviceOn = true;

        public abstract String SendCommand(Enum cmd, String value);
        public abstract String GetCommandStatus(Enum cmd);

        private System.Object lockThisSend = new System.Object();
        private System.Object lockThisReceive = new System.Object();
        
        public void Initialise(String baud, String parity, String stopbits, String databits, String port)
        {
            comm = new CommunicationManager(baud, parity, stopbits, databits, port, handler);
            comm.OpenPort();
            m_waitForResponse = false;
            m_lastOperationWasOK = true;
            Connection = port;
        }

        public void Disconnect()
        {
            comm.ClosePort();
        }

        protected bool WriteCommand(String cmd)
        {

            lock (lockThisSend)
            {
                int i = 0;
                if (m_waitForResponse == true)
                {
                    MLog.Log(this, "Error, trying to write " + this.ToString() + " command: " + cmd + " while waiting for response, now looping");
                    do
                    {
                        Thread.Sleep(10);
                        System.Windows.Forms.Application.DoEvents();
                        i++;
                    }
                    while (m_waitForResponse && i < 500);
                    if (i >= 300)
                    {
                        MLog.Log(this, "WARNING no response received while looping");
                        return false;
                    }
                }
                m_isProcessing = true;
                //MLog.Log(this, "Write LG comm=" + cmd);
                comm.WriteData(cmd);
                
                //WAIT FOR RESPONSE - ----------------------
                m_waitForResponse = true;
                i = 0;
                do
                {
                    Thread.Sleep(10);
                    System.Windows.Forms.Application.DoEvents();
                    i++;
                }
                while (m_waitForResponse && i < 500);

                if (m_waitForResponse == true && m_isSerialDeviceOn)
                {
                    MLog.Log(this, "Error WaitForOK " + this.ToString() + " exit with timeout");
                    m_lastMessageResponse = "timeout";
                    m_waitForResponse = false;
                }
                m_isProcessing = false;
                return m_lastOperationWasOK;
            }
        }


        protected int handler(String message)
        {
            lock (lockThisReceive)
            {
                if (message.Length == 1 && Convert.ToByte(message[0]) == 0)
                {//lost connection with TV
                    MLog.Log(this, "Serial connection potentially lost to " + comm.PortName);
                    m_isSerialDeviceOn = false;
                }
                else
                    m_isSerialDeviceOn = true;

                //MLog.Log(this, "received LG response=" + message);
                m_lastMessageResponse = message.ToLower();
                m_lastOperationWasOK = m_lastMessageResponse.Contains("ok");
                m_waitForResponse = false;
                /*int i = 0;
                do
                {
                    Thread.Sleep(10);
                    System.Windows.Forms.Application.DoEvents();
                    i++;
                }
                while (m_isProcessing && i<700);
                if (m_isProcessing)
                {
                    MLog.Log(this, "ERROR on receive handler, timeout");
                    m_isProcessing = false;
                }
                */
                return 0;
            }
        }

        public Boolean IsBusy
        {
            get { return m_waitForResponse; }
        }

        
    }

    class GTalkMessengers:IMessenger
    {

        agsXMPP.XmppClientConnection objXmpp;
        String m_user, m_password;
        //List<String> m_rosterList;
        List<agsXMPP.protocol.client.Presence> m_presenceList;

        public GTalkMessengers(String user, String password)
        {
            m_user = user;
            m_password = password;
            Reinitialise();
        }

        ~GTalkMessengers()
        {
            if (objXmpp != null)
                objXmpp.Close();
        }

        public void Reinitialise()
        {
            m_presenceList = new List<agsXMPP.protocol.client.Presence>();
            objXmpp = new agsXMPP.XmppClientConnection();
            agsXMPP.Jid jid = null;
            jid = new agsXMPP.Jid(m_user);
            objXmpp.Password = m_password;
            objXmpp.Username = jid.User;
            objXmpp.Server = jid.Server;
            objXmpp.AutoResolveConnectServer = true;
            objXmpp.OnPresence += new agsXMPP.protocol.client.PresenceHandler(xmpp_OnPresence);
            /*objXmpp.OnRosterStart += new ObjectHandler(OnRosterStart);
            objXmpp.OnRosterEnd += new ObjectHandler(OnRosterEnd);
            objXmpp.OnRosterItem += new XmppClientConnection.RosterHandler(OnRosterItem);*/
            Login();
        }

        private void Login()
        {
            MLog.Log(this, "GTalk Messenger login start");
            try
            {
                objXmpp.OnMessage += messageReceived;
                objXmpp.OnAuthError += loginFailed;
                objXmpp.OnLogin += loggedIn;
                objXmpp.Open();
                MLog.Log(this, "GTalk Messenger login completed");
            }
            catch (Exception ex)
            {
                MLog.Log(ex, this, "GTalk Messenger login error");
            } 
        }
        
        private void messageReceived(object sender, agsXMPP.protocol.client.Message msg)
        {
            string[] chatMessage = null;
            chatMessage = msg.From.ToString().Split('/');
            //agsXMPP.Jid jid = null;
            //jid = new agsXMPP.Jid(chatMessage[0]);
            //agsXMPP.protocol.client.Message reply = null;

            ReceiveMessage(msg.Body, chatMessage[0]);

            
        }
        
        private void loginFailed(object o, agsXMPP.Xml.Dom.Element el)
        {
            MLog.Log(this, "GTalk Messenger NOT authenticated " + el.Value);
        }

        private void loggedIn(object o)
        {
            MLog.Log(this, "GTalk Messenger authenticated OK");
        }

        private void xmpp_OnPresence(object sender, agsXMPP.protocol.client.Presence pres)
        {
            agsXMPP.protocol.client.Presence item;
            //MLog.Log(this, "GTalkMessengers roster: "+ pres.From +" type=" + pres.Type + " status=" + pres.Status);
            item = m_presenceList.Find(x => x.From.Equals(pres.From));
            if (item != null)
                m_presenceList.Remove(item);
            m_presenceList.Add(pres);
        }
        /*
        void OnRosterItem(object sender, agsXMPP.protocol.iq.roster.RosterItem item)
        {
            m_rosterList.Add(item.Jid.Bare);
        }

        void OnRosterEnd(object sender)
        {

            objXmpp.SendMyPresence();
        }

        void OnRosterStart(object sender)
        {
            m_rosterList = new List<string>();
        }*/


        public void SendMessage(String message, String targetId)
        {
            if (!objXmpp.Authenticated)
            {
                MLog.Log(this, "Messenger not authenticated, try relogin");
                Login();
            }
            
            if (objXmpp != null && objXmpp.Authenticated)
                objXmpp.Send(new agsXMPP.protocol.client.Message(new agsXMPP.Jid(targetId), agsXMPP.protocol.client.MessageType.chat, message));
            else
                MLog.Log(this, "Messenger not authenticated yet, message dropped");
        }

        public void ReceiveMessage(String message, String sender)
        {
            if (sender.ToLower().Equals(IniFile.PARAM_GTALK_TARGETUSER[1].ToLower()))
            {

                string replymessage;
                string cmd = message.ToLower();
                /*if (Enum.IsDefined(typeof(Metadata.GlobalCommands), msg.Body.ToLower()))
                {
                    Metadata.GlobalCommands gc = (Metadata.GlobalCommands)Enum.Parse(typeof(Metadata.GlobalCommands), msg.Body.ToLower());
                
                 */
                String[] atoms, pair;
                try
                {
                    atoms = cmd.Split(' ');
                    Metadata.ValueList val = new Metadata.ValueList(Metadata.GlobalParams.command, atoms[0], Metadata.CommandSources.Web);
                    for (int i = 1; i < atoms.Length; i++)
                    {
                        pair = atoms[i].Split(':');
                        if (pair.Length >= 2)
                        {
                            val.Add(pair[0], pair[1]);
                        }
                        else
                            MLog.Log(this, "Invalid parameter in " + atoms[i]);
                    }
                    Metadata.ValueList retval;
                    String json = API.DoCommandFromWeb(val, out retval);
                    Metadata.CommandResult retcmd = fastJSON.JSON.Instance.ToObject(json) as Metadata.CommandResult;
                    if (retval != null)
                        replymessage = retcmd.Result + " - " + retcmd.ErrorMessage + " - " + retval.GetValue(Metadata.GlobalParams.msg);
                    else
                        replymessage = retcmd.Result + " - " + retcmd.ErrorMessage;
                }
                catch (Exception ex)
                {
                    MLog.Log(ex, "Error parsing GTALK command");
                    replymessage = "Error parsing command, " + ex.Message;
                }

                SendMessage(replymessage, sender);
            }
            else
                MZPState.Instance.LogEvent(MZPEvent.EventSource.System, "Unknown gtalk sender " + sender, MZPEvent.EventType.Security, MZPEvent.EventImportance.Critical);
        }

        public bool TestConnection()
        {
            if (!objXmpp.Authenticated)
            {
                MLog.Log(this, "Error GTALK, not authenticated");
                return false;
            }
            else
                return true;
        }

        public Boolean IsTargetAvailable()
        {
            agsXMPP.protocol.client.Presence item = null;
            item = m_presenceList.Find(x => x.From.Bare.ToLower().Equals(IniFile.PARAM_GTALK_TARGETUSER[1].ToLower()) 
                && x.Type.Equals(agsXMPP.protocol.client.PresenceType.available));
            return (item != null);
        }

        public void MakeBuzz()
        {
            SendMessage("BUZZ", IniFile.PARAM_GTALK_TARGETUSER[1]);
        }
    }

    class SMS : IMessenger
    {
        private CommunicationManager comm;
        private Boolean m_waitForResponse = false;
        private Boolean m_lastOperationWasOK = true;
        private String m_lastCommand, m_lastResponse;
        private bool m_CommandEchoReceived = false;
        private List<SMSCommand> m_commandList;

        public class SMSCommand
        {
            public String Id;
            public String ATCommand;
            public String Response;
            public SMSCommand(String Id, String ATCommand, String Response)
            {
                this.Id = Id;
                this.ATCommand = ATCommand;
                this.Response = Response;
            }
        }

        public SMS()
        {
            Reinitialise();
        }

        ~SMS()
        {
            comm.ClosePort();
        }

        public void Reinitialise()
        {
            comm = new CommunicationManager("9600", "None", "One", "8", IniFile.PARAM_SMS_COMPORT[1], handler);
            comm.OpenPort();
            m_waitForResponse = false;
            m_lastOperationWasOK = true;
            m_commandList = new List<SMSCommand>();
        }

        public void SendMessage(String message, String targetId)
        {
            WriteCommand("AT+CMGF=1");
            WriteCommand("AT + CMGS = \"" + IniFile.PARAM_SMS_TARGETNUMBER[1] + "\"");
            WriteCommand(message + (char)26);
        }

        public void ReceiveMessage(String message, String sender)
        {
            //MLog.Log(this, "SMS from " +sender + " = " + message);
        }

        public bool TestConnection()
        {
            //bool result = false ;
            
            if (m_waitForResponse == true)
            {
                MLog.Log(this, "Trying to test SMS conn while waiting for response, skip test");
                return true;
            }

            return comm.IsPortOpen();

            /*try
            {
                result = WriteCommand("AT");
            }
            catch (Exception ex)
            {
                MLog.Log(ex, "Exception Test SMS");
                result =false;
            }

            return result;*/
        }

        private bool WriteCommand(String cmd)
        {
            if (m_waitForResponse == true)
            {
                MLog.Log(this, "Error, trying to write SMS command: " + cmd + " while waiting for response");
                return false;
            }

            //String id = DateTime.Now.ToString() + cmd;
            //m_commandList.Add(new SMSCommand(id, cmd, ""));

            m_lastCommand = cmd.ToLower();
            m_CommandEchoReceived = false;
            comm.WriteData(cmd);
            WaitForOK();
            return m_lastOperationWasOK;
        }

        private void WaitForOK()
        {
            m_waitForResponse = true;
            int i = 0;
            do
            {
                Thread.Sleep(100);
                System.Windows.Forms.Application.DoEvents();
                i++;
            }
            while (m_waitForResponse == true && i<200);

            if (m_waitForResponse == true)
                MLog.Log(this, "Error WaitForOK exit with timeout, last cmd="+m_lastCommand+" lastresp=" + m_lastResponse);
            m_waitForResponse = false;
        }

        private int handler(String message)
        {
            message = message.Replace("\r", "").Replace("\n","").ToLower();
            if (m_waitForResponse == false)
                MLog.Log(this, "Err, Response received unexpected:" + message + " last cmd=" + m_lastCommand + " resp=" + m_lastResponse);
            
            if (message=="") return 0;
            if (message.Equals(m_lastCommand))
            {
                m_CommandEchoReceived = true;
                return 0;
            }
            
            m_lastResponse = message + " at " + DateTime.Now;
            if (m_CommandEchoReceived) m_waitForResponse = false;

            if (message.Contains("ok") || message.Contains(">") || message.Contains("busy") 
                || message.Contains("no answer"))// || message.Contains("AT"))
            {
                m_lastOperationWasOK = true;
            }
            else
                if (message.Contains("error") || message.Contains("no dialtone"))
                {
                    m_lastOperationWasOK = false;
                    
                    MLog.Log(this, "SMS received response ERROR="+message);
                }
                else
                    MLog.Log(this, "SMS Unclear response=" + message);

            ReceiveMessage(message, "");
            return 0;
        }

        public Boolean IsTargetAvailable()
        {
            return true;
        }

        public void MakeBuzz()
        {
            MLog.Log(this, "Calling target number " + IniFile.PARAM_SMS_TARGETNUMBER[1]);
            WriteCommand("ATD " + IniFile.PARAM_SMS_TARGETNUMBER[1] + ";");
        }
    }
}

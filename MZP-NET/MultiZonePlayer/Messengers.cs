using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;
using agsXMPP;


namespace MultiZonePlayer
{
    public interface IMessenger
    {
        void SendMessage(String message, String targetId);
        void ReceiveMessage(String message, String sender);
        Boolean TestConnection();
        void Reinitialise();
        void Close();
        Boolean IsTargetAvailable();
        void MakeBuzz();
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
            Close();
        }

        public void Close()
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
			if (Thread.CurrentThread.Name == null 
				|| !Thread.CurrentThread.Name.StartsWith("GTalk"))
				Thread.CurrentThread.Name = "GTalk " + msg.Body;

            string[] chatMessage = null;
            chatMessage = msg.From.ToString().Split('/');
            //agsXMPP.Jid jid = null;
            //jid = new agsXMPP.Jid(chatMessage[0]);
            //agsXMPP.protocol.client.Message reply = null;

            ReceiveMessage(msg.Body, chatMessage[0]);

            
        }
        
        private void loginFailed(object o, agsXMPP.Xml.Dom.Element el)
        {
            Thread.CurrentThread.Name = "GTalk Event LoggedIn Failed";
            MLog.Log(this, "GTalk Messenger NOT authenticated " + el.Value);
        }

        private void loggedIn(object o)
        {
            Thread.CurrentThread.Name = "GTalk Event LoggedIn OK";
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
			String[] atoms, pair;
			String cmdName, replymessage;
			string cmd = message;
			atoms = cmd.Split(' ');
			sender = sender.ToLower();
			bool isPowerUser = IniFile.PARAM_GTALK_TARGETUSER[1].ToLower().Contains(sender);
			if (atoms.Length > 0)
			{
				cmdName = atoms[0].ToLower();
				int macroId = MZPState.Instance.GetMacroIdByShortcut(cmdName, "");
				if (!isPowerUser)
				{
					Metadata.MacroEntry entry = MZPState.Instance.MacroList.Find(x => x.Id == macroId);
					if (entry == null || !entry.AllowUserList.Contains(sender))
					{
						replymessage = "Not authorised gtalk sender " + sender;
						MZPState.Instance.LogEvent(MZPEvent.EventSource.System, replymessage, MZPEvent.EventType.Security, MZPEvent.EventImportance.Critical, null);
						SendMessage(replymessage, sender);
						return;	
					}
				}
				try
				{
					if (cmd.StartsWith("?"))
					{
						replymessage = message;
						HTMLInterface.GenericReflect(HTMLInterface.Instance, ref replymessage);
					}
					else
					{
						Metadata.ValueList val = new Metadata.ValueList(Metadata.GlobalParams.command, cmdName, Metadata.CommandSources.messenger);
						for (int i = 1; i < atoms.Length; i++)
						{
							pair = atoms[i].Split(':');
							if (pair.Length >= 2)
							{
								val.Add(pair[0].ToLower(), pair[1].Replace('_', ' '));
							}
							else
							{
								if (i == 1)//assume first param is zonename. replace _ to allow space in words
									val.Add(Metadata.GlobalParams.singleparamvalue, atoms[i].Replace('_', ' '));
								else
									MLog.Log(this, "Invalid parameter in " + atoms[i]);
							}
						}
						Metadata.CommandResult res = API.DoCommand(val);
						string sysname = System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString();
						replymessage = sysname + " (" + res.Command + "): " + res.Result + " - " + res.ErrorMessage + " - " + res.OutputMessage
							+ res.ValuesToString();
					}
				}
				catch (Exception ex)
				{
					replymessage = "Error parsing command " + ex.Message;
					MLog.Log(ex, replymessage);
				}
			}
			else
				replymessage = "Error no command received";
			SendMessage(replymessage, sender);
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
			string[] targets = IniFile.PARAM_GTALK_TARGETUSER[1].Split(';');
			for (int i=0;i<targets.Length;i++)
				SendMessage("BUZZ", targets[i]);
        }
    }

    class SMS : SerialBase, IMessenger
    {
        public enum SMSCommandsEnum
        {
            [Description("AT+CMGF=1")] SMS_ENABLE,
            [Description("AT+CMGS=")] SMS_SEND,
            [Description("ATD ")] SMS_CALL,
            [Description("AT")] SMS_CHECK,
            [Description("AT+GMM")] SMS_DEVICEINFO,
        };

        private List<SMSCommand> m_commandList;
        private int m_stdtimeout = 2000;
        private int m_calltimeout = 80000;
        private int m_atlinescount, m_atdlinescount;

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
            Close();
        }

        public void Close()
        {
            Disconnect();
        }

        public void Reinitialise()
        {
            comm = new CommunicationManager("9600", "None", "One", "8", IniFile.PARAM_SMS_COMPORT[1], this.handler);
            m_atlinescount = Convert.ToInt16(IniFile.PARAM_SMS_AT_LINES_COUNT[1]);
            m_atdlinescount = Convert.ToInt16(IniFile.PARAM_SMS_ATD_LINES_COUNT[1]);
            comm.OpenPort();
            m_waitForResponse = false;
            m_lastOperationWasOK = true;
            m_commandList = new List<SMSCommand>();

            //WriteCommand(Utilities.GetEnumDescription((SMSCommandsEnum)SMSCommandsEnum.SMS_DEVICEINFO), m_atlinescount, m_stdtimeout);
        }

        public override string SendCommand(Enum cmd, string value)
        {
            throw new NotImplementedException();
        }

        public void SendMessage(String message, String targetId)
        {
            WriteCommand(Utilities.GetEnumDescription((SMSCommandsEnum)SMSCommandsEnum.SMS_ENABLE), m_atlinescount, m_stdtimeout);
            WriteCommand(Utilities.GetEnumDescription((SMSCommandsEnum)SMSCommandsEnum.SMS_SEND) + "\"" + IniFile.PARAM_SMS_TARGETNUMBER[1] + "\"", m_atlinescount, m_stdtimeout);
            WriteCommand(message + (char)26, 6, m_stdtimeout * 3);
        }

        public override string GetCommandStatus(Enum cmd)
        {
            throw new NotImplementedException();
        }

        public void ReceiveMessage(String message, String sender)
        {
            //MLog.Log(this, "SMS from " +sender + " = " + message);
        }

        public bool TestConnection()
        {
            //bool result = false ;
            
            /*if (m_waitForResponse == true)
            {
                MLog.Log(this, "Trying to test SMS conn while waiting for response, skip test");
                return true;
            }*/

            if (!comm.IsPortOpen())
                return false;

            try
            {
                String res = WriteCommand(Utilities.GetEnumDescription((SMSCommandsEnum)SMSCommandsEnum.SMS_CHECK), m_atlinescount, m_stdtimeout).ToLower();
                if (res.Contains("ok") || res.Contains("at"))
                    return true;
                else
                {
                    MLog.Log(this, "Error health check res=" + res);
                    return true;
                }
            }
            catch (Exception ex)
            {
                MLog.Log(ex, "Exception Test SMS");
                return false;
            }
        }
        
        protected override void ReceiveSerialResponse(string response)
        {
            MLog.Log(this, "Received unexpected CELL MODEM serial response: " + response);

            String message = response.ToLower().Replace("\r", "").Replace("\n","").ToLower();
            /*
            if (m_waitForResponse == false)
                MLog.Log(this, "Err, Response received unexpected:" + message + " last cmd=" + m_lastCommand + " resp=" + m_lastResponse);
            
            if (message=="") return;
            if (message.Equals(m_lastCommand))
            {
                m_CommandEchoReceived = true;
                return;
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
            */
            ReceiveMessage(message, "");
        }

        public Boolean IsTargetAvailable()
        {
            return true;
        }

        public void MakeBuzz()
        {
            MLog.Log(this, "Calling target number " + IniFile.PARAM_SMS_TARGETNUMBER[1]);
            String res = WriteCommand(Utilities.GetEnumDescription((SMSCommandsEnum)SMSCommandsEnum.SMS_CALL) + IniFile.PARAM_SMS_TARGETNUMBER[1] + ";", m_atdlinescount, m_calltimeout);
            MLog.Log(this, "Calling target number done, res=" + res);
        }
    }
}

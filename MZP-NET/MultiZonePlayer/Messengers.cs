﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;
using agsXMPP;

using System.Net;
using System.Net.Mail;

namespace MultiZonePlayer
{
    public interface IMessenger
    {
        Boolean SendMessageToTarget(String message);
        void ReceiveMessage(String message, String sender);
        Boolean TestConnection();
        void Reinitialise();
        void Close();
        Boolean IsConnected();
        void MakeBuzz();
		Boolean IsFaulty();
    }

    class GTalkMessengers:IMessenger, IMZPDevice
    {
		private int m_reinitTries=0;
        private DateTime m_lastInitFailed = DateTime.MinValue;
        agsXMPP.XmppClientConnection objXmpp;
        String m_user, m_password;
        //ValueList<String> m_rosterList;
        List<agsXMPP.protocol.client.Presence> m_presenceList;
		private ValueList m_lastCommand = null;

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

        public void Reinitialise(){
            try {
                m_reinitTries++;
                m_presenceList = new List<agsXMPP.protocol.client.Presence>();
                objXmpp = new agsXMPP.XmppClientConnection();
                agsXMPP.Jid jid = null;
                jid = new agsXMPP.Jid(m_user);
                objXmpp.Password = m_password;
                objXmpp.Username = jid.User;
                objXmpp.Server = jid.Server;
                objXmpp.AutoResolveConnectServer = true;
                objXmpp.OnPresence += new agsXMPP.protocol.client.PresenceHandler(xmpp_OnPresence);
                objXmpp.OnError += new ErrorHandler(objXmpp_OnError);
                Login();
            }
            catch (Exception ex) {
                if (DateTime.Now.Subtract(m_lastInitFailed).TotalMinutes > Constants.INTERVAL_SHOW_INIT_ERRORS_MINUTES) {
                    MLog.Log(this, "Error reinit gtalk retry count="+m_reinitTries+", ex=" + ex.ToString());
                    m_lastInitFailed = DateTime.Now;
                }
            }
        }

        private void Login()
        {
            //MLog.Log(this, "GTalk Messenger login start");
            objXmpp.OnMessage += messageReceived;
            objXmpp.OnAuthError += loginFailed;
            objXmpp.OnLogin += loggedIn;
            objXmpp.Open();
			m_reinitTries = 0;
            //MLog.Log(this, "GTalk Messenger login completed");
        }

		public Boolean IsFaulty()
		{
			return false;//gtalk canot be faulty
			//return (!objXmpp.Authenticated);// || m_reinitTries>5);
		}

        private void messageReceived(object sender, agsXMPP.protocol.client.Message msg)
        {
            try {
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
            catch (Exception ex) {
                Alert.CreateAlertOnce("Error in Gtalk message received, err="+ex.Message, "GTalk receive");
            }
        }
        
        private void loginFailed(object o, agsXMPP.Xml.Dom.Element el)
        {
            try{
            //Thread.CurrentThread.Name = "GTalk Event LoggedIn Failed";
            MLog.Log(this, "GTalk Messenger NOT authenticated " + el.Value);
             }
            catch (Exception ex) {
                Alert.CreateAlertOnce("Error in Gtalk loginFailed, err=" + ex.Message, "GTalk loginFailed");
            }
        }

        private void loggedIn(object o)
        {
            //Thread.CurrentThread.Name = "GTalk Event LoggedIn OK";
            MLog.Log(this, "GTalk Messenger authenticated OK");
        }

        private void xmpp_OnPresence(object sender, agsXMPP.protocol.client.Presence pres)
        {
            try{
            agsXMPP.protocol.client.Presence item;
            //MLog.Log(this, "GTalkMessengers roster: "+ pres.From +" type=" + pres.Type + " status=" + pres.Status);
            item = m_presenceList.Find(x => x.From.Equals(pres.From));
            if (item != null)
                m_presenceList.Remove(item);
            m_presenceList.Add(pres);
            }
            catch (Exception ex) {
                Alert.CreateAlertOnce("Error in Gtalk xmpp_OnPresence, err=" + ex.Message, "GTalk xmpp_OnPresence");
            }
        }
		private void objXmpp_OnError(object sender, Exception ex)
		{
			//MLog.Log(ex, this, "GTALK ON ERROR");
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
            m_rosterList = new ValueList<string>();
        }*/


        public Boolean SendMessageToTarget(String message)
        {
			Boolean result = false ;
            if (!objXmpp.Authenticated){
                MLog.Log(this, "Messenger not authenticated, try relogin");
                Reinitialise();
            }

			string[] targets = IniFile.PARAM_GTALK_TARGETUSER[1].Split(';');
			for (int i = 0; i < targets.Length; i++)
			{
				if (objXmpp != null && objXmpp.Authenticated)
				{
					objXmpp.Send(new agsXMPP.protocol.client.Message(new agsXMPP.Jid(targets[i]),
						agsXMPP.protocol.client.MessageType.chat, message));
					result = true;
				}
				else
					MLog.Log(this, "Messenger not authenticated yet, message dropped");
			}
			return result;
        }

		public void SendMessageToUser(String message, string user)
		{
			if (!objXmpp.Authenticated)
			{
				MLog.Log(this, "Messenger not authenticated, try relogin");
				Reinitialise();
			}

			if (objXmpp != null && objXmpp.Authenticated)
				objXmpp.Send(new agsXMPP.protocol.client.Message(new agsXMPP.Jid(user),
					agsXMPP.protocol.client.MessageType.chat, message));
			else
				MLog.Log(this, "Messenger not authenticated yet, message dropped");
		}

        public void ReceiveMessage(String message, String sender)
        {
			String[] atoms, pair;
			String cmdName, replymessage;
			string cmd = message;
			MLog.Log(this, "Message:"+message+", from:"+sender);
			if (cmd !=null && cmd.Length > 0)
			{
				atoms = cmd.Split(' ');
				sender = sender.ToLower();
				bool isPowerUser = IniFile.PARAM_GTALK_TARGETUSER[1].ToLower().Contains(sender);
				if (atoms.Length > 0)
				{
					cmdName = atoms[0].ToLower();
					int macroId = MacroEntry.GetMacroIdByShortcut(cmdName, "");
					if (!isPowerUser)
					{
						MacroEntry entry = MacroEntry.ValueList.Find(x => x.Id == macroId);
						if (entry == null || !entry.AllowUserList.Contains(sender))
						{
							replymessage = "Not authorised gtalk sender " + sender;
							Alert.CreateAlert(replymessage, null, false, null,Alert.NotificationFlags.NotifyUserAfterXSeconds, 1);
							//MZPState.Instance.LogEvent(MZPEvent.EventSource.System, replymessage, MZPEvent.EventType.Security, MZPEvent.EventImportance.Critical, null);
							SendMessageToUser(replymessage, sender);
							return;
						}
					}
					try
					{
						if (cmd.StartsWith("?"))
						{
							replymessage = message;
							Reflect.GenericReflect(ref replymessage);
						}
						else
						{
							ValueList val;
							if (cmdName != GlobalCommands.r.ToString())
							{
								val = new ValueList(GlobalParams.command, cmdName, CommandSources.messenger);
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
											val.Add(GlobalParams.singleparamvalue, atoms[i].Replace('_', ' '));
										else
											MLog.Log(this, "Invalid parameter in " + atoms[i]);
									}
								}
							}
							else
							{
								val = m_lastCommand;
							}
							CommandResult res;
							if (val != null)
							{
								res = API.DoCommand(val);
								m_lastCommand = val;
							}
							else
								res = new CommandResult(ResultEnum.ERR, "Error", "Error no last command exist");
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
				SendMessageToUser(replymessage, sender);
			}
			//"Empty cmd received";
        }

        public bool TestConnection()
        {
			if (!objXmpp.Binded)
			{
				//MLog.Log(this, "Error GTALK, not binded");
				return false;
			}
            if (!objXmpp.Authenticated)
            {
                MLog.Log(this, "Error GTALK, not authenticated");
                return false;
            }
            else
                return true;
        }

        public Boolean IsConnected()
        {
            agsXMPP.protocol.client.Presence item = null;

			string[] targets = IniFile.PARAM_GTALK_TARGETUSER[1].Split(';');
			for (int i = 0; i < targets.Length; i++)
			{
				item = m_presenceList.Find(x => x.From.Bare.ToLower().Equals(targets[i].ToLower())
					&& x.Type.Equals(agsXMPP.protocol.client.PresenceType.available));
				if (item != null)
					return true;
			}
			return false;
        }

        public void MakeBuzz()
        {
			SendMessageToTarget("BUZZ");
        }

		public bool IsFunctional() {
			return objXmpp.Binded && objXmpp.Authenticated;
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
			return IniFile.DEVICE_TYPE_CHAT;
		}

		public string Name() {
			return "GTALK";
		}


        public string Status() {
            return "";
        }
    }

	class EmailNotifier
	{
		public static Boolean SendEmail(string messageText)
		{
			Boolean result = false;
			try{
				var fromAddress = new MailAddress(IniFile.PARAM_GTALK_USERNAME[1], IniFile.PARAM_GTALK_USERNAME_EMAILNAME[1]);
				var toAddress = new MailAddress(IniFile.PARAM_GTALK_TARGETUSER[1].Split(';')[0], "");
				string fromPassword = IniFile.PARAM_GTALK_USERPASS[1];
				string subject = messageText;
				string body = MZPState.Instance.GetZonesStatus();

				var smtp = new SmtpClient{
					Host = "smtp.gmail.com",
					Port = 587,
					EnableSsl = true,
					DeliveryMethod = SmtpDeliveryMethod.Network,
					UseDefaultCredentials = false,
					Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
				};
				using (var message = new MailMessage(fromAddress, toAddress)
				{
					Subject = subject,
					Body = body
				})
				{
					smtp.Send(message);
					result = true;
				}
			}
			catch (Exception ex){
				MLog.Log("Error sending email, err="+ex.Message);
			}
			return result;
		}
	}
}

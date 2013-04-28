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
        void SendMessageToTarget(String message);
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
		private Metadata.ValueList m_lastCommand = null;

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


        public void SendMessageToTarget(String message)
        {
            if (!objXmpp.Authenticated)
            {
                MLog.Log(this, "Messenger not authenticated, try relogin");
                Login();
            }

			string[] targets = IniFile.PARAM_GTALK_TARGETUSER[1].Split(';');
			for (int i = 0; i < targets.Length; i++)
			{
				if (objXmpp != null && objXmpp.Authenticated)
					objXmpp.Send(new agsXMPP.protocol.client.Message(new agsXMPP.Jid(targets[i]), 
						agsXMPP.protocol.client.MessageType.chat, message));
				else
					MLog.Log(this, "Messenger not authenticated yet, message dropped");
			}
        }

		public void SendMessageToUser(String message, string user)
		{
			if (!objXmpp.Authenticated)
			{
				MLog.Log(this, "Messenger not authenticated, try relogin");
				Login();
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
						Metadata.ValueList val;
						if (cmdName != Metadata.GlobalCommands.r.ToString())
						{
							val = new Metadata.ValueList(Metadata.GlobalParams.command, cmdName, Metadata.CommandSources.messenger);
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
						}
						else
						{
							val = m_lastCommand;
						}
						Metadata.CommandResult res;
						if (val != null)
						{
							res = API.DoCommand(val);
							m_lastCommand = val;
						}
						else
							res = new Metadata.CommandResult(Metadata.ResultEnum.ERR, "Error", "Error no last command exist");
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
    }

	class GenericModem : SerialBase, IMessenger
	{
		public enum ModemCommandsEnum
		{
			[Description("ATD ")]
			MODEM_CALL,
			[Description("AT")]
			MODEM_CHECK
			
		};

		public class ModemCommand
		{
			public String Id;
			public String ATCommand;
			public String Response;
			public ModemCommand(String Id, String ATCommand, String Response)
			{
				this.Id = Id;
				this.ATCommand = ATCommand;
				this.Response = Response;
			}
		}
		private List<ModemCommand> m_commandList;
		protected int m_stdtimeout = 2000;
		protected int m_calltimeout = 60000;
		protected int m_atlinescount, m_atdlinescount;
		
        ~GenericModem()
        {
            Close();
        }
		
		public void Close()
		{
			Disconnect();
		}

		public virtual void Reinitialise()
		{
		}

		public void Reinitialise(string baud, string parity, string stopBits, string dataBits, string comport,
			int atlinescount, int atdlinescount)
		{
			comm = new CommunicationManager(baud, parity, stopBits, dataBits, comport, this.handler);
			m_atlinescount = atlinescount;
			m_atdlinescount = atdlinescount;
			comm.OpenPort();
			m_waitForResponse = false;
			m_lastOperationWasOK = true;
			m_commandList = new List<ModemCommand>();
		}

		public override string SendCommand(Enum cmd, string value)
		{
			throw new NotImplementedException();
		}
		public override string GetCommandStatus(Enum cmd)
		{
			throw new NotImplementedException();
		}

		protected override void ReceiveSerialResponse(string response)
		{
			MLog.Log(this, "Received unexpected MODEM serial response: " + response);

			String message = response.ToLower().Replace("\r", "").Replace("\n", "").ToLower();

			ReceiveMessage(message, "");
		}

		public void ReceiveMessage(String message, String sender)
		{
			//MLog.Log(this, "SMS from " +sender + " = " + message);
		}

		public void SendMessageToTarget(String message)
		{
			MLog.Log(this, "ERROR send message not implemented in modem");
		}

		public bool TestConnection()
		{
			if (comm==null || !comm.IsPortOpen())
				return false;

			try
			{
				String res = WriteCommand(Utilities.GetEnumDescription((ModemCommandsEnum)ModemCommandsEnum.MODEM_CHECK), m_atlinescount, m_stdtimeout).ToLower();
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

		public Boolean IsTargetAvailable()
		{
			return (comm!=null && comm.IsPortOpen());
		}
		public void MakeBuzz()
		{
			MLog.Log(this, "Calling target number " + IniFile.PARAM_MODEM_TARGETNUMBER[1]);
			String res = WriteCommand(Utilities.GetEnumDescription((ModemCommandsEnum)ModemCommandsEnum.MODEM_CALL) 
				+ IniFile.PARAM_MODEM_TARGETNUMBER[1] + ";", m_atdlinescount, m_calltimeout);
			MLog.Log(this, "Calling target number done, res=" + res);
			//comm.ClosePort();
			//Reinitialise();
			WriteCommand("ATH", 2, 3000);
			MLog.Log(this, "Call reset");
		}
	}

	class Modem : GenericModem
	{
		public override void Reinitialise()
		{
			Reinitialise("9600", "None", "One", "8", IniFile.PARAM_MODEM_COMPORT[1],
				Convert.ToInt16(IniFile.PARAM_MODEM_AT_LINES_COUNT[1]),
				Convert.ToInt16(IniFile.PARAM_MODEM_ATD_LINES_COUNT[1]));

		}
	}

    class SMS : GenericModem
    {
        public enum SMSCommandsEnum
        {
            [Description("AT+CMGF=1")] SMS_ENABLE,
            [Description("AT+CMGS=")] SMS_SEND,
            [Description("AT+GMM")] SMS_DEVICEINFO,
        };

		public override void Reinitialise()
		{
			Reinitialise("9600", "None", "One", "8", IniFile.PARAM_SMS_COMPORT[1],
				Convert.ToInt16(IniFile.PARAM_SMS_AT_LINES_COUNT[1]),
				Convert.ToInt16(IniFile.PARAM_SMS_ATD_LINES_COUNT[1]));
		}

        public new void SendMessageToTarget(String message)
        {
			WriteCommand(Utilities.GetEnumDescription((SMSCommandsEnum)SMSCommandsEnum.SMS_ENABLE), 
				m_atlinescount, m_stdtimeout);
			WriteCommand(Utilities.GetEnumDescription((SMSCommandsEnum)SMSCommandsEnum.SMS_SEND) + "\"" 
				+ IniFile.PARAM_SMS_TARGETNUMBER[1] + "\"", m_atlinescount, m_stdtimeout);
            WriteCommand(message + (char)26, 6, m_stdtimeout * 3);
        }  
    }

	

	class RFXCom : SerialBase, IMessenger
	{
		private static string CMD_RESET		=	"0D 00 00 00 00 00 00 00 00 00 00 00 00 00";
		private static string CMD_GETSTATUS =	"0D 00 00 01 02 00 00 00 00 00 00 00 00 00";
		private static string CMD_SETMODE =		"0D 00 00 05 03 53 00 00 00 2E 00 00 00 00";//oregon&arc&ac&h eu	
			//"0D 00 00 06 03 53 00 00 00 26 00 00 00 00";//oregon&arc&ac
			//"0D 00 00 02 03 53 00 00 00 22 00 00 00 00";//oregon & arc
		
		
		public RFXCom()
		{
			Reinitialise();
		}
		public void Reinitialise()
		{
			Initialise("38400", "None", "One", "8", IniFile.PARAM_RFXCOM_PORT[1]);
			//comm = new CommunicationManager("38400", "None", "One", "8", 
			//	IniFile.PARAM_RFXCOM_PORT[1], this.handler);
			//comm.OpenPort();
			comm.CurrentTransmissionType = CommunicationManager.TransmissionType.Hex;
			//comm.WriteData("FF FA 2C 01 00 00 96 00 FF F0 FF FA 2C 02 08 FF F0 FF FA 2C 03 01 FF F0 FF FA 2C 04 01 FF F0 FF FA 2C 05 01 FF F0");
			WriteCommand(CMD_RESET, 1, 1000);
			//comm.WriteData(CMD_RESET);
			Thread.Sleep(1000);
			comm.Flush();
			string status = WriteCommand(CMD_GETSTATUS, 1, 1000);
			MLog.Log(this, "RFX status is " + status);
			WriteCommand(CMD_SETMODE, 1, 1000);
			
			//m_waitForResponse = false;
			//m_lastOperationWasOK = true;
		}
		protected override void ReceiveSerialResponse(string response)
		{
			MLog.Log(this, "RFXCOMM: " + response);
			do
			{
				RFXDeviceDefinition.RFXDevice dev = RFXDeviceDefinition.GetDevice(ref response);
				if (dev != null)
				{
					MLog.Log(this, "RFX result: " + dev.DisplayValues());
					if (dev.ZoneId != -1)
					{
						Metadata.ZoneDetails zone = MZPState.Instance.GetZoneById(dev.ZoneId);
						switch(dev.DeviceType)
						{
							case RFXDeviceDefinition.DeviceTypeEnum.temp_hum:
								zone.Temperature = (Convert.ToDecimal(dev.FieldValues.Find(x => x.Name == RFXDeviceDefinition.DeviceAttributes.temperature.ToString()).Value)/10).ToString();
								zone.Humidity = dev.FieldValues.Find(x => x.Name == RFXDeviceDefinition.DeviceAttributes.humidity.ToString()).Value;
								break;
							case RFXDeviceDefinition.DeviceTypeEnum.lighting1:
								
								break;
						}
						
					}
				}
			} while (response.Length > 0);

			
			//String message = response.ToLower().Replace("\r", "").Replace("\n", "").ToLower();
		}
		public override string GetCommandStatus(Enum cmd)
		{
			throw new NotImplementedException();
		}

		public override string SendCommand(Enum cmd, string value)
		{
			throw new NotImplementedException();
		}

		public string SendCommand(string value)
		{
			return WriteCommand(value, 1, 1000);
		}

		public void Close()
		{
			Disconnect();
		}

		public Boolean IsTargetAvailable()
		{
			return true;
		}

		public void MakeBuzz()
		{
		}
		public void ReceiveMessage(String message, String sender)
		{
			//MLog.Log(this, "SMS from " +sender + " = " + message);
		}

		public void SendMessageToTarget(String message)
		{
			MLog.Log(this, "RFX comm does not implement sendmessage");
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
				return true;
			}
			catch (Exception ex)
			{
				MLog.Log(ex, "Exception Test RFXComm");
				return false;
			}
		}
	}
}

using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

// http://www.codeproject.com/Tips/441233/Multithreaded-Customizable-SysLog-Server-Csharp
namespace MultiZonePlayer {
	public class SysLog {
		private static UdpClient udpListener;
		public void Start()
        {
			try {
				IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
				udpListener = new UdpClient(514);
				byte[] bReceive; string sReceive; string sourceIP;

				/* Main Loop */
				/* Listen for incoming data on udp port 514 (default for SysLog events) */
				while (MZPState.Instance != null && !MZPState.Instance.IsShuttingDown) {
					bReceive = udpListener.Receive(ref anyIP);
					/* Convert incoming data from bytes to ASCII */
					sReceive = Encoding.ASCII.GetString(bReceive);
					/* Get the IP of the device sending the syslog */
					sourceIP = anyIP.Address.ToString();
					new Thread(new logHandler(sourceIP, sReceive).handleLog).Start();
					/* Start a new thread to handle received syslog event */
				}
			}
			catch (Exception ex) {
				MLog.Log(null, ex.Message);
			}
        }
		public void Stop() {
			try {
				udpListener.Close();
			}
			catch (Exception ex) { 
				MLog.Log(null, ex.Message); }
		}
    }

	
    class logHandler
    {
        /* Phrases within the syslog that will trigger an email notification */
        private string[] emailTriggers = new string[] { "link loss", "help please" };
        private string outputPath =  IniFile.CurrentPath()+ IniFile.SYSLOG_FILE ; /* Location to store events */
        private string source; private string log;
        
        public logHandler(string sourceIP, string logData) /* Initialize object and clean up the raw data */
        {
            source = sourceIP.Trim(); /* Client IP */
            log = logData.Replace(Environment.NewLine, "").Trim(); /* Syslog data */
        }

        public void handleLog() /* Store the syslog and determine whether to trigger an email notification */
        {
            /* Store the syslog using a new thread */
            new Thread(new outputCsvRow(outputPath, new string[] { source, log }).addRow).Start();
            for (int i = 0; i < emailTriggers.Count(); i++) { 
				if (log.Contains(emailTriggers[i])) { 
					//emailEvent(); 
				} }
            /* Search for trigger strings and send email if found */

            return;
        }

        private void emailEvent() /* Send email notification */
        {
            try
            {
                MailMessage notificationEmail = new MailMessage();
                notificationEmail.Subject = "SysLog Event";
                notificationEmail.IsBodyHtml = true;
                notificationEmail.Body = "<b>SysLog Event Triggered:<br/><br/>Time: </b><br/>" + 
                    DateTime.Now.ToString() + "<br/><b>Source IP: </b><br/>" +
					source + "<br/><b>Event: </b><br/>" + log; 
						/* Throw in some basic HTML for readability */
                notificationEmail.From = new MailAddress("SysLog@metastruct.com", "SysLog Server"); /* From Address */
                notificationEmail.To.Add(new MailAddress("metastructblog@gmail.com", "metastruct")); /* To Address */
                SmtpClient emailClient = new SmtpClient("10.10.10.10"); /* Address of your SMTP server of choice */
                //emailClient.UseDefaultCredentials = false; /* If your SMTP server requires credentials to send email */
                //emailClient.Credentials = new NetworkCredential(“username”, “password”); /* Supply User Name and Password */
                emailClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                emailClient.Send(notificationEmail); /* Send the email */
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            return;
        }
    }

    class outputCsvRow
    {
        private string formattedRow = null;
        private string outputPath = null;

        public outputCsvRow(string filePath, string[] columns) /* Initialize object */
        {
            outputPath = filePath;
            formattedRow = (char)34 + DateTime.Now.ToString() + (char)34; /* Construct csv row starting with the timestamp */
            for (int i = 0; i < columns.Count(); i++) { formattedRow += "," + (char)34 + columns[i] + (char)34; }
        }

        public void addRow()
        {
            int attempts = 0;
            bool canAccess = false;
            StreamWriter logWriter = null;
            if (!File.Exists(outputPath)) /* If the file doesn't exist, give it some column headers */
            {
                logWriter = new StreamWriter(outputPath, true);
                logWriter.WriteLine((char)34 + "Event_Time" + (char)34 + "," + 
                  (char)34 + "Device_IP" + (char)34 + "," + (char)34 + "SysLog" + (char)34);
                logWriter.Close();
            }
            /* Thread safety first! This is a poor man's SpinLock */
            while (true)
            {
                try
                {
                    logWriter = new StreamWriter(outputPath, true); /* Try to open the file for writing */
                    canAccess = true; /* Success! */
                    break;
                }
                catch (IOException ex)
                {
                    if (attempts < 15) { attempts++; Thread.Sleep(50); }
                    else { Console.WriteLine(ex.ToString()); break; } /* Give up after 15 attempts */
                }
            }
            if (canAccess) /* Write the line if the file is accessible */
            {
                logWriter.WriteLine(formattedRow);
                logWriter.Close();
            }
            return;
        }
	}
}

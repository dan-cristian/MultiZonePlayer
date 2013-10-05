using System;
using System.IO;
using System.Collections;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

namespace MultiZonePlayer
{
	/// <summary>
	/// An implementation of the Cron service.
	/// http://www.codeproject.com/Articles/10992/Implementing-a-small-Cron-service-in-C
	/// 
	/// # DoThis every hour
	/// 0 * * * * C:\Program\ Files\MyProg\DoThis.exe arg1 arg2
	///# DoThat -- well, you figure out when :-)
	///8-9/2 12,13,14-18 * * * C:\Program\ Files\MyProg\DoThat.exe
	/// 
	/// 0 * * * * <%if "#Zones,2.Temperature"<"24";"command=poweron;zonename=heat-living"
	/// 
	/// </summary>
	public class Cron
	{
		private ArrayList crontab;
		private ArrayList processes;
		int lastMinute;
		string m_cronTabText="";
		Thread m_thread;

		public string CronTabText
		{
			get { return m_cronTabText; }
		}

		public Cron()
		{}

		public void stop()
		{
			m_thread.Abort();
		}
		public void start ()
		{
			m_thread = Thread.CurrentThread;
			DateTime now;
			lastMinute = DateTime.Now.Minute - 1;

			crontab   = new ArrayList ();
			processes = new ArrayList ();

			try
			{
				//readCrontab (Application.StartupPath + "\\cron.tab");

				while (true)
				{
					Thread.Sleep (30000); // half a minute
					try
					{
						now = DateTime.Now;
						checkProcesses(now);
						doCrontab(now);
					}
					catch (Exception ex)
					{
						MLog.Log(ex, this, "Crontab while failed");
					}
				}
			}
			catch (Exception e)
			{
				MLog.Log(e, this, "Crontab run failed");
			}		
		}

		public void checkProcesses (DateTime now)
		{
			ArrayList toRemove = new ArrayList ();

			for (int i = 0; i < processes.Count; i++)
			{
				Process proc = (Process) processes[i];

				if (proc.HasExited)
				{
					toRemove.Add (proc);

					if (proc.ExitCode != 0)
					{
						reportError ("The process " + proc.StartInfo.FileName + " " + 
							proc.StartInfo.Arguments + " returned with error " + proc.ExitCode.ToString ());
					}
				}
				else if (DateTime.Compare (proc.StartTime, DateTime.Now.Subtract (new System.TimeSpan (0, 20, 0))) < 0)
				{
					reportError (proc.StartInfo.FileName + " takes longer than 20 minutes and will be killed.");
					proc.Kill ();
				}
			}

			for (int i = toRemove.Count - 1; i >= 0; i--)
			{
				processes.Remove (toRemove[i]);
			}
		}

		public void doCrontab (DateTime now)
		{
			if (now.Minute.Equals (lastMinute))
				return;

			// for loop: deal with the highly unexpected eventuality of
			// having lost more than one minute to unavailable processor time
			lock (crontab)
			{
				for (int minute = (lastMinute == 59 ? 0 : lastMinute + 1); minute <= now.Minute; minute++)
				{
					foreach (ArrayList entry in crontab)
					{
						if (contains(entry[0], now.Month) &&
							contains(entry[1], getMDay(now)) &&
							contains(entry[2], getWDay(now)) &&
							contains(entry[3], now.Hour) &&
							contains(entry[4], now.Minute))
						{
							// yay, we get to execute something!

							String var1 = ((String)entry[5]).Trim();
							String var2 = ((String)entry[6]).Trim();

							if (var2.ToLower().Contains("command="))
							{
								Reflect.GenericReflect(ref var1);
								String reseval = ExpressionEvaluator.EvaluateBoolToString(var1);
								if (Convert.ToBoolean(reseval))
								{
									ValueList val = new ValueList();
									ValueList.ParseStringToValues(var2, ";", "=", ref val);
									API.DoCommand(val);
								}
							}
							/*Process proc = new Process();
							proc.StartInfo.FileName = var1;
							proc.StartInfo.Arguments = var2;

							if (!proc.Start())
								reportError("Could not start " + proc.StartInfo.FileName);
							else
								processes.Add(proc);
							 */
						}
					}
				}
			}
			lastMinute = now.Minute;
		}

		// sort of a macro to keep the if-statement above readable
		private bool contains (Object list, int val)
		{
			// -1 represents the star * from the crontab
			return ((ArrayList) list).Contains (val) || ((ArrayList) list).Contains (-1);
		}

		private int getMDay (DateTime date)
		{
			date.AddMonths (-(date.Month - 1));
			return date.DayOfYear;
		}

		private int getWDay (DateTime date)
		{
			if (date.DayOfWeek.Equals (DayOfWeek.Sunday))
				return 7;
			else
				return (int) date.DayOfWeek;
		}

		public void readCrontabFile (String filename)
		{
			StreamReader sr;

			try
			{
				String line;

				sr = new StreamReader (filename);

				while ((line = sr.ReadLine ()) != null)
				{
					ArrayList minutes, hours, mDays, months, wDays;

					line = line.Trim ();

					if (line.Length == 0 ||  line.StartsWith("#"))
						continue;
					
					// re-escape space- and backslash-escapes in a cheap fashion
					line = line.Replace ("\\\\", "<BACKSLASH>");
					line = line.Replace ("\\ ", "<SPACE>");

					// split string on whitespace
					String[] cols = line.Split (new char[] {' ','\t'});

					for (int i = 0; i < cols.Length; i++)
					{
						cols[i] = cols[i].Replace ("<BACKSLASH>", "\\");
						cols[i] = cols[i].Replace ("<SPACE>", " ");
					}

					if (cols.Length < 6)
					{
						reportError ("Parse error in crontab (line too short).");
						crontab = new ArrayList();
					}

					minutes = parseTimes (cols[0], 0, 59);
					hours   = parseTimes (cols[1], 0, 23);
					months  = parseTimes (cols[3], 1, 12);

					if (!cols[2].Equals("*") && cols[3].Equals ("*"))
					{
						// every n monthdays, disregarding weekdays
						mDays = parseTimes (cols[2], 1, 31);
						wDays = new ArrayList ();
						wDays.Add (-1); // empty value
					}
					else if (cols[2].Equals("*") && !cols[3].Equals ("*"))
					{
						// every n weekdays, disregarding monthdays
						mDays = new ArrayList ();
						mDays.Add (-1); // empty value
						wDays = parseTimes (cols[4], 1, 7); // 60 * 24 * 7
					}
					else
					{
						// every n weekdays, every m monthdays
						mDays = parseTimes (cols[2], 1, 31);
						wDays = parseTimes (cols[4], 1, 7); // 60 * 24 * 7
					}
				
					String args = "";

					for (int i = 6; i < cols.Length; i++)
						args += " " + cols[i];

					ArrayList entry = new ArrayList (6);

					entry.Add (months);
					entry.Add (mDays);
					entry.Add (wDays);
					entry.Add (hours);
					entry.Add (minutes);
					entry.Add (cols[5]);
					entry.Add (args);

					crontab.Add (entry);
				}

				sr.Close ();
			}
			catch (Exception e)
			{
				reportError (e.ToString ());
			}
		}

		public void readCrontabString(String cronTabText)//line separated by \n
		{
			try
			{
				lock (crontab)
				{
					crontab.Clear();
					MLog.Log(this, "Reading cron text len="+cronTabText.Length);
					foreach (string sline in cronTabText.Split('\n'))
					{
						String line = sline;
						ArrayList minutes, hours, mDays, months, wDays;

						line = line.Trim();

						if (line.Length == 0 || line.StartsWith("#"))
							continue;

						// re-escape space- and backslash-escapes in a cheap fashion
						line = line.Replace("\\\\", "<BACKSLASH>");
						line = line.Replace("\\ ", "<SPACE>");

						// split string on whitespace
						String[] cols = line.Split(new char[] { ' ', '\t' });

						for (int i = 0; i < cols.Length; i++)
						{
							cols[i] = cols[i].Replace("<BACKSLASH>", "\\");
							cols[i] = cols[i].Replace("<SPACE>", " ");
						}

						if (cols.Length < 6)
						{
							reportError("Parse error in crontab (line too short).");
							crontab = new ArrayList();
						}

						minutes = parseTimes(cols[0], 0, 59);
						hours = parseTimes(cols[1], 0, 23);
						months = parseTimes(cols[3], 1, 12);

						if (!cols[2].Equals("*") && cols[3].Equals("*"))
						{
							// every n monthdays, disregarding weekdays
							mDays = parseTimes(cols[2], 1, 31);
							wDays = new ArrayList();
							wDays.Add(-1); // empty value
						}
						else if (cols[2].Equals("*") && !cols[3].Equals("*"))
						{
							// every n weekdays, disregarding monthdays
							mDays = new ArrayList();
							mDays.Add(-1); // empty value
							wDays = parseTimes(cols[4], 1, 7); // 60 * 24 * 7
						}
						else
						{
							// every n weekdays, every m monthdays
							mDays = parseTimes(cols[2], 1, 31);
							wDays = parseTimes(cols[4], 1, 7); // 60 * 24 * 7
						}

						String args = "";

						for (int i = 6; i < cols.Length; i++)
							args += " " + cols[i];

						ArrayList entry = new ArrayList(6);

						entry.Add(months);
						entry.Add(mDays);
						entry.Add(wDays);
						entry.Add(hours);
						entry.Add(minutes);
						entry.Add(cols[5]);
						entry.Add(args);

						crontab.Add(entry);
					}
				}
				m_cronTabText = cronTabText;
				MLog.Log(this, "Added crontab lines=" + crontab.Count);
			}
			catch (Exception e)
			{
				MLog.Log(e, "Crontab load error");
			}
		}

		public ArrayList parseTimes (String line, int startNr, int maxNr)
		{
			ArrayList vals = new ArrayList ();
			String[] list, parts;
 
			list = line.Split (new char[] {','});

			foreach (String entry in list)
			{
				int start, end, interval;

				parts = entry.Split (new char[] {'-','/'});
				
				if (parts[0].Equals ("*"))
				{
					if (parts.Length > 1)
					{
						start = startNr;
						end = maxNr;

						interval = int.Parse (parts[1]);
					}
					else
					{
						// put a -1 in place
						start = -1;
						end = -1;
						interval = 1;
					}
				}
				else
				{
					// format is 0-8/2
					start    = int.Parse (parts[0]);
					end      = parts.Length > 1 ? int.Parse (parts[1]) : int.Parse (parts[0]);
					interval = parts.Length > 2 ? int.Parse (parts[2]) : 1;
				}

				for (int i = start; i <= end; i += interval)
				{
					vals.Add (i);
				}
			}
			return vals;
		}

		public void reportError (String error)
		{
			// Error reporting is left up to you; this is a case apart
			// (besides, my implementation was too specific to post here)
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;


namespace MultiZonePlayer
{
    class WinEventLogReader
    {
        private EventLog myLog ;
        private static List<String> m_eventSourceList;
	    private DateTime m_lastLogEntryDate = DateTime.MinValue;

        public WinEventLogReader(String logname)
        {
            // check for the event log source on specified machine
            // the Application event log source on MCBcomputer
            if (!EventLog.Exists(logname, System.Environment.MachineName))
            {
                MLog.Log(this, "The log "+logname+" does not exist!");
                return;
            }
            
            myLog = new EventLog();
            m_eventSourceList = new List<string>();
            myLog.Log = logname;
            myLog.MachineName = System.Environment.MachineName;
            MLog.Log(this, "There are " + myLog.Entries.Count + " entr[y|ies] in the "+logname+" Application log");

            /*foreach (EventLogEntry entry in myLog.Entries)
            {
                Console.WriteLine("\tEntry: " + entry.Message);

            }*/

            
            myLog.EntryWritten += new EntryWrittenEventHandler(OnEntryWritten);
            myLog.EnableRaisingEvents = true;
        }

	    public DateTime LastLogEntryDate {
		    get { return m_lastLogEntryDate; }
	    }

	    public void OnEntryWritten(Object source, EntryWrittenEventArgs e)
        {
            Console.WriteLine("written entry: " + e.Entry.Message);

            foreach (String sourceString in m_eventSourceList)
            {
                if (e.Entry.Source.ToLower().Equals(sourceString.ToLower())) {
	                m_lastLogEntryDate = e.Entry.TimeGenerated;
                    MZPState.Instance.ZoneEvents.WinEventLogEntryMatch(sourceString, e.Entry.Message, e.Entry.TimeGenerated);
                }
            }
        }

        public void AddSource(String source)
        {
            m_eventSourceList.Add(source);
        }
    }
}

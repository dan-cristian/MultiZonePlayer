using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiZonePlayer
{
	public static class ZoneClosures
	{
		public static void ProcessAction(Metadata.ZoneDetails zone, string key, Boolean isKeyDown)// KeyDetail kd)
		{
			Metadata.ClosureOpenCloseRelayState lastState = zone.ClosureOpenCloseRelayState;
			if (lastState == null)
			{
				lastState = new Metadata.ClosureOpenCloseRelayState(key, isKeyDown);
				zone.ClosureOpenCloseRelayState = lastState;
			}
			//else
			//	MLog.Log(null, "Last state " + cmdRemote.CommandCode + " was " + lastState.RelayState + " at " + lastState.LastChange);
			

			if (lastState.RelayState == Metadata.ClosureOpenCloseRelayState.GetRelayState(isKeyDown))
				return;//return is state does not change
			zone.ClosureCounts++;
			lastState.RelayStateClosed = isKeyDown;

			string message = "Closure state " + key + " is " + lastState.RelayState;
			Utilities.AppendToCsvFile(IniFile.CSV_CLOSURES, ",", zone.ZoneName, key, DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT), lastState.RelayState.ToString());
			if (lastState.RelayState == Metadata.ClosureOpenCloseRelayState.EnumState.Closed)
			{
				MZPState.Instance.LogEvent(MZPEvent.EventSource.Closure, message,	MZPEvent.EventType.Security, 
					MZPEvent.EventImportance.Informative, zone);
			}
			MLog.Log(null, message);
			
		}
	}
}

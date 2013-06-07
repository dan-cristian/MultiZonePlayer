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
			else
			{
				if (lastState.RelayState == Metadata.ClosureOpenCloseRelayState.GetRelayState(isKeyDown))
					return;//return if state does not change
				lastState.RelayStateClosed = isKeyDown;
			}
			zone.ClosureCounts++;
			zone.MovementAlert = true;
			zone.LastClosureEventDateTime = DateTime.Now;
			string message = "Closure state " + key + " is " + lastState.RelayState;
			MLog.Log(null, message);
			Utilities.AppendToCsvFile(IniFile.CSV_CLOSURES, ",", zone.ZoneName, key, DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT), lastState.RelayState.ToString());
			if (lastState.RelayState == Metadata.ClosureOpenCloseRelayState.EnumState.Closed)
			{
				MZPState.Instance.LogEvent(MZPEvent.EventSource.Closure, message,	MZPEvent.EventType.Security, 
					MZPEvent.EventImportance.Informative, zone);
			}
			zone.MovementAlert = false;
		}
	}
}

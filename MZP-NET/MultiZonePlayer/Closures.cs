using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiZonePlayer
{
	public static class Closures
	{
		public static void ProcessAction(Metadata.ZoneDetails zone, KeyDetail kd)
		{
			Metadata.ClosureOpenCloseRelayState lastState = zone.ClosureOpenCloseRelayState;
			if (lastState == null)
			{
				lastState = new Metadata.ClosureOpenCloseRelayState(kd.Key, kd.IsKeyDown);
				zone.ClosureOpenCloseRelayState = lastState;
			}
			//else
			//	MLog.Log(null, "Last state " + cmdRemote.CommandCode + " was " + lastState.RelayState + " at " + lastState.LastChange);

			if (lastState.RelayState == Metadata.ClosureOpenCloseRelayState.GetRelayState(kd.IsKeyDown))
				return;

			lastState.RelayStateClosed = kd.IsKeyDown;
			MLog.Log(null, "Current state " + kd.Key + " is " + lastState.RelayState);
			
		}
	}
}

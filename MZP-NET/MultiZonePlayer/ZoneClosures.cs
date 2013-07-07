﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiZonePlayer
{
	public static class ZoneClosures
	{
		public static void ProcessAction(Metadata.ZoneDetails zone, string key, Boolean isKeyDown)// KeyDetail kd)
		{
			Metadata.ClosureOpenCloseRelay lastState = zone.ClosureOpenCloseRelay;
			if (lastState == null)
			{
				lastState = new Metadata.ClosureOpenCloseRelay(isKeyDown);
				zone.ClosureOpenCloseRelay = lastState;
			}
			else
			{
				if (lastState.RelayState == zone.ClosureOpenCloseRelay.GetRelayState(isKeyDown))
					return;//return if state does not change
				lastState.RelayContactMade = isKeyDown;
			}
			zone.ClosureCounts++;
			zone.MovementAlert = true;
			zone.LastClosureEventDateTime = DateTime.Now;
			string message = "Closure state " + key + " is " + lastState.RelayState;
			MLog.Log(null, message);
			Utilities.AppendToCsvFile(IniFile.CSV_CLOSURES, ",", zone.ZoneName, key, DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT), lastState.RelayState.ToString());
			if (lastState.RelayState == Metadata.ClosureOpenCloseRelay.EnumState.ContactClosed)
			{
				MZPState.Instance.LogEvent(MZPEvent.EventSource.Closure, message,	MZPEvent.EventType.Security, 
					MZPEvent.EventImportance.Informative, zone);
			}
			zone.MovementAlert = false;
		}

		public static void Tick()
		{
            System.Windows.Forms.Keys key;
			foreach (Metadata.ZoneDetails zone in MZPState.Instance.ZoneDetails)
			{
				if (zone.ClosureIdList.Length > 0)
				{
					if (Enum.IsDefined(typeof(System.Windows.Forms.Keys), zone.ClosureIdList.ToUpper()))
					{
						key = (System.Windows.Forms.Keys)Enum.Parse(typeof(System.Windows.Forms.Keys),
							zone.ClosureIdList.ToUpper());
						//ensure key is not stuck, signal key release
						if (!KeyboardUtility.IsKeyDown(key))
							ProcessAction(zone,zone.ClosureIdList,KeyboardUtility.IsKeyDown(key));
					}
				}
			}
		}
	}
}
﻿/*
  LICENSE
  -------
  Copyright (C) 2007-2010 Ray Molenkamp

  This source code is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this source code or the software it produces.

  Permission is granted to anyone to use this source code for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this source code must not be misrepresented; you must not
     claim that you wrote the original source code.  If you use this source code
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original source code.
  3. This notice may not be removed or altered from any source distribution.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreAudio;
using System.Diagnostics;

//http://whenimbored.xfx.net/2011/01/core-audio-for-net/
namespace MultiZonePlayer
{
    public class SoundSession {
        public String Id1;
        public String Id2;
    }

    /// <summary>
    /// This will find an active audio session, print some information about it and dispay the value of the peak meter and allow simple volume control.
    /// </summary>
    public class CoreAudioSoundDetector
    {
        public static List<SoundSession> GetActiveSoundSessions() {
            MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
            MMDeviceCollection device_enum;
            device_enum = DevEnum.EnumerateAudioEndPoints(EDataFlow.eRender, DEVICE_STATE.DEVICE_STATE_ACTIVE);

            List<SoundSession> sound_sessions = new List<SoundSession>();
            for (int k = 0; k < device_enum.Count; k++) {
                //MMDevice device = DevEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
                MMDevice device = device_enum[k];
                // Note the AudioSession manager did not have a method to enumerate all sessions in windows Vista
                // this will only work on Win7 and newer.
                for (int i = 0; i < device.AudioSessionManager2.Sessions.Count; i++) {
                    AudioSessionControl2 session = device.AudioSessionManager2.Sessions[i];
                    if (session.State == AudioSessionState.AudioSessionStateActive) {
                        SoundSession sound_session_item = new SoundSession();
                        String[] session_atoms = session.GetSessionIdentifier.SplitTwo("|");
                        String id1 = session_atoms[0].SplitTwo("}.")[1];
                        String id2 = "{" + session_atoms[1].SplitTwo("{")[1];
                        sound_session_item.Id1 = id1;
                        sound_session_item.Id2 = id2;
                        sound_sessions.Add(sound_session_item);
                        /*Console.WriteLine("Session :{0}", i);
                        Console.WriteLine("DisplayName: {0}", session.DisplayName);
                        Console.WriteLine("State: {0}", session.State);
                        Console.WriteLine("IconPath: {0}", session.IconPath);
                        Console.WriteLine("SessionIdentifier: {0}", session.GetSessionIdentifier);
                        Console.WriteLine("SessionInstanceIdentifier: {0}", session.GetSessionInstanceIdentifier);
                        Console.WriteLine("ProcessID: {0}", session.GetProcessID);
                        Console.WriteLine("IsSystemIsSystemSoundsSession: {0}", session.IsSystemSoundsSession);
                        Process p = Process.GetProcessById((int)session.GetProcessID);
                        Console.WriteLine("ProcessName: {0}", p.ProcessName);
                        Console.WriteLine("MainWindowTitle: {0}", p.MainWindowTitle);
                        AudioMeterInformation mi = session.AudioMeterInformation;
                        SimpleAudioVolume vol = session.SimpleAudioVolume;
                        Console.WriteLine("Mute   : {0}    ", vol.Mute);
                        Console.WriteLine("Master : {0:0.00}    ", vol.MasterVolume * 100);
                         */
                        /*while (true) {
                            Console.WriteLine("---[Hotkeys]---");
                            Console.WriteLine("M  Toggle Mute");
                            Console.WriteLine(",  Lower volume");
                            Console.WriteLine(",  Raise volume");
                            Console.WriteLine("Q  Quit");
                            int start = Console.CursorTop;
                        
                            //Draw a VU meter
                            int len = (int)(mi.MasterPeakValue * 79);
                            Console.SetCursorPosition(0, start);
                            for (int j = 0; j < len; j++)
                                Console.Write("*");
                            for (int j = 0; j < 79 - len; j++)
                                Console.Write(" ");
                            Console.SetCursorPosition(0, start + 1);
                            Console.WriteLine("Mute   : {0}    ", vol.Mute);
                            Console.WriteLine("Master : {0:0.00}    ", vol.MasterVolume * 100);
                            if (Console.KeyAvailable) {
                                ConsoleKeyInfo key = Console.ReadKey();
                                switch (key.Key) {
                                    case ConsoleKey.M:
                                        vol.Mute = !vol.Mute;
                                        break;
                                    case ConsoleKey.Q:
                                        return sound_sessions;
                                    case ConsoleKey.OemComma:
                                        float curvol = vol.MasterVolume - 0.1f;
                                        if (curvol < 0) curvol = 0;
                                        vol.MasterVolume = curvol;
                                        break;
                                    case ConsoleKey.OemPeriod:
                                        float curvold = vol.MasterVolume + 0.1f;
                                        if (curvold > 1) curvold = 1;
                                        vol.MasterVolume = curvold;
                                        break;
                                }

                            }
                        }
                         */
                    }
                }
            }
            //If we end up here there where no open audio sessions to monitor.
            Console.WriteLine("No Audio sessions found");
            return sound_sessions;
        }
    }
}
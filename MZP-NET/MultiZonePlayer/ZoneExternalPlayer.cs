﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace MultiZonePlayer
{
    public class ZoneExternalPlayerBase :ZoneBase
    {

        protected Display m_display;

    }

    public class ZonePlayerXBMC:ZoneExternalPlayerBase, INavigableUI
    {
        //http://wiki.xbmc.org/index.php?title=JSON-RPC_API/v4
        private String m_playerId = "1";
        private String m_playerIdParam = "playerid";
        //private String STATUS_URL = "/jsonrpc?UpdateState";
        private String CMD_URL = "/jsonrpc?SendRemoteKey";
		private String GET_URL = "/jsonrpc?";
		private bool m_bringToForegroundOnce = false;
		private bool m_isXBMCProcessOn, m_isXBMCPlayerRunning;

        public class XBMCLimits
        {
            public int end;
            public int start;
            public int total;

            public XBMCLimits()
            { }
        }
        public class XBMCItems
        {
            public int episode;
            public int id;
            public string label;
            public string plot;
            public string runtime;
            public int season;
            public string showtitle;
            public string thumbnail;
            public string title;
            public string type;

            public XBMCItems()
            { }

        }
        public class XBMCResult
        {
            public int playerid;
            public string type;
            public int volume;
            public XBMCItems[] items;
            public XBMCLimits limits;
   
            public XBMCResult()
            {}
        }
        public class XBMCResponse
        {
            public int id;
            public string jsonrpc;
            public XBMCResult result;
            public XBMCResponse()
            {}
        }
        public class XBMCSimpleResponse
        {
            public int id;
            public string jsonrpc;
            public XBMCResult[] result;
            public XBMCSimpleResponse()
            { }
        }
        

        public ZonePlayerXBMC(ZoneDetails zoneDetails)
        {
            m_zoneDetails = zoneDetails;
			m_zoneDetails.ZoneClose();
			m_zoneDetails.ZoneState = ZoneState.NotStarted;
			m_zoneDetails.Genre = "video";

			if (!Utilities.IsProcAlive(IniFile.PARAM_XBMC_PROCESS_NAME[1]))
			{
				MZPState.RestartGenericProc(IniFile.PARAM_XBMC_PROCESS_NAME[1], IniFile.PARAM_XBMC_APP_PATH[1],
					System.Diagnostics.ProcessWindowStyle.Normal, System.Diagnostics.ProcessPriorityClass.AboveNormal);
			}
            Display displayZone = MZPState.Instance.DisplayList.Find(x => x.ZoneDetails.ParentZoneId == zoneDetails.ZoneId && x.ZoneDetails.HasDisplay);
            if (displayZone != null)
            {
                if (!displayZone.IsOn)
                {
                    MLog.Log(this, "XBMC initialising display TV");
                    displayZone.IsOn = true;
                    System.Threading.Thread.Sleep(10000);
                }
                else
                    MLog.Log(this, "Display is ON already for XBMC");
                if (displayZone.InputType != Display.InputTypeEnum.HDMI)
                {
                    MLog.Log(this, "XBMC set input to HDMI");
                    displayZone.InputType = Display.InputTypeEnum.HDMI;
                }
                else
                    MLog.Log(this, "Display is set to HDMI already for XBMC");
            }
            else
                MLog.Log(this, "No display found for XBMC, CHECK!, zonename="+zoneDetails.ZoneName);
			Monitor.RefreshFrequencySecondary();
        }

		public ValueList ProcessAction(GlobalCommands cmdRemote, ValueList vals, ref CommandResult cmdresult)
        {
            ValueList result = new ValueList();
            String action = action = vals.GetValue(GlobalParams.action);
			String source = vals.GetValue(GlobalParams.cmdsource) ?? "";
            switch (cmdRemote)
            {
					
                case GlobalCommands.right:
                    if (!source.Equals(CommandSources.rawinput.ToString()))//avoid duplicate actions
						DirectionRight();
                    break;
                case GlobalCommands.left:
					if (!source.Equals(CommandSources.rawinput.ToString()))//avoid duplicate actions
						DirectionLeft();
                    break;
                case GlobalCommands.up:
					if (!source.Equals(CommandSources.rawinput.ToString()))//avoid duplicate actions
						DirectionUp();
                    break;
                case GlobalCommands.down:
					if (!source.Equals(CommandSources.rawinput.ToString()))//avoid duplicate actions
						DirectionDown();
                    break;
                case GlobalCommands.enter:
					if (!source.Equals(CommandSources.rawinput.ToString()))//avoid duplicate actions
						Select();
                    break;
                case GlobalCommands.back:
                    DirectionBack();
                    break;
                default:
                    MLog.Log(this, "WARNING, unprocessed zone command " + cmdRemote);
                    break;
            }
            return result;
        }

        //shortcut for cmds
        private String PostURLCmdMessage(String method, params String[] parameters)
        {
            return PostURLMessage(CMD_URL, method, parameters);
        }

        private String PostURLMessage(String URL, String method, params String[] parameters)
        {
            WebPostRequest post;
            String paramName,paramValue;
			String res;
            String msg, conn="";
			try
			{
				if (URL == GET_URL)
					conn = m_zoneDetails.DisplayConnection + URL + method;
				else
					conn = m_zoneDetails.DisplayConnection + URL;
				post = new WebPostRequest(conn, "application/json");//IniFile.PARAM_XBMC_COMMAND_URL[1]);
				if (parameters.Length > 0)
					msg = @"{""jsonrpc"":""2.0"",""method"":""<M>"",""params"":{<P>},""id"":1}";
				else
					msg = @"{""jsonrpc"":""2.0"",""method"":""<M>"",""id"": 1}"; ;
				String pair = @"""<N>"":<V>,<P>";
				msg = msg.Replace("<M>", method);

				for (int i = 0; i < parameters.Length; i = i + 2)
				{
					paramName = parameters[i];
					paramValue = parameters[i + 1];

					msg = msg.Replace("<P>", pair);
					msg = msg.Replace("<N>", paramName);
					msg = msg.Replace("<V>", paramValue);
					/*if (paramName != "")
					{

						msg = @"{""jsonrpc"": ""2.0"", ""method"": """ + method + @""", ""params"": { """ + paramName + @""": " + paramValue + @" }, ""id"": 1}";

					}
					else
						msg = @"{""jsonrpc"": ""2.0"", ""method"": """ + method + @""", ""id"": 1}";
					 * */
				}
				msg = msg.Replace(",<P>", "");
				post.Add(msg, "");
				//MultiZonePlayer.MLog.Log(this, msg);
				
				try
				{
					res = post.GetResponse();
				}
				catch (Exception ex)
				{
					res = "{xbmc web server error:" + ex.Message + "}";
				}
				//MLog.Log(null, res);
			}
			catch (Exception ex)
			{
				MLog.Log(ex, this, "Error post XBMC cmd="+method+" zone="+m_zoneDetails.ZoneName + " conn="+conn);
				res = "{}";
			}
            return res;
        }

		public override void Close()
		{
			base.Close();
			if (!m_isXBMCProcessOn || !m_isXBMCPlayerRunning)
			{
				MLog.Log(this, "Closing XBMC, process=" + m_isXBMCProcessOn + " player=" + m_isXBMCPlayerRunning);
				PostURLCmdMessage("Application.Quit");
			}
		}

        public override void Play()
        {
            base.Play();
            PostURLCmdMessage("Player.PlayPause", m_playerIdParam, m_playerId);
        }

        public override void VolumeDown()
        {
 	        base.VolumeDown();
            m_zoneDetails.VolumeLevel = Math.Max(0, m_zoneDetails.VolumeLevel-2);
            PostURLCmdMessage("Application.SetVolume", "volume", m_zoneDetails.VolumeLevel.ToString());
        }

        public override void VolumeUp()
        {
            base.VolumeUp();
            m_zoneDetails.VolumeLevel += 2;
            PostURLCmdMessage("Application.SetVolume", "volume", m_zoneDetails.VolumeLevel.ToString());
        }

        public override void Stop()
        {
			if (m_zoneDetails.ZoneState == ZoneState.NotStarted)
			{
				Close();
			}
			else
			{
				base.Stop();
				PostURLCmdMessage("Player.Stop", m_playerIdParam, m_playerId);
			}
        }

        public override void Pause()
        {
            base.Pause();
            PostURLCmdMessage("Player.PlayPause", m_playerIdParam, m_playerId);
        }

        public override void Mute()
        {
            base.Mute();
            PostURLCmdMessage("Application.SetMute", "mute", "toggle");
        }

        public void DirectionUp()
        {
            PostURLCmdMessage("Input.Up");
        }
        public void DirectionDown()
        {
            PostURLCmdMessage("Input.Down");
        }
        public void DirectionLeft()
        {
            PostURLCmdMessage("Input.Left");
        }
        public void DirectionRight()
        {
            PostURLCmdMessage("Input.Right");
        }
        public void Select()
        {
            PostURLCmdMessage("Input.Select");
        }
        public void DirectionBack()
        {
            PostURLCmdMessage("Input.Back");
        }

		private void MoveToSecondScreen()
		{
			Screen[] sc = Screen.AllScreens;
			if (sc.Length > 1)
			{
				MLog.Log(null, "Multiple screens detected, count=" + sc.Length);
				foreach (Screen scr in sc)
				{
					if (!scr.Primary)
					{
						Rectangle area = scr.Bounds;
						IntPtr handle = GetXBMCHandle();
						Utilities.SetWindowPos(handle, 0, area.Left, area.Top,
							area.Width, area.Height, Utilities.SWP.SHOWWINDOW);
						MLog.Log(null, "XBMC h="+handle+" sent to second screen at x="+area.Left+" y="+area.Top + " scrname="+scr.DeviceName);
						break;
					}
				}
			}
		}

		private IntPtr GetXBMCHandle()
		{
			return Utilities.FindWindow("XBMC", "XBMC");
		}

        private void GetXBMCStatus()
        {
            XBMCResponse resp;
            XBMCSimpleResponse sresp;

            if (!Utilities.IsProcAlive(IniFile.PARAM_XBMC_PROCESS_NAME[1]))
            {
				m_isXBMCProcessOn = false;
                Close();
            }
            else
            {
				m_isXBMCProcessOn = true;
                String result = PostURLMessage(GET_URL, "Player.GetActivePlayers");
                try
                {
					if (Utilities.GetForegroundWindow() != GetXBMCHandle())//(!m_bringToForegroundOnce)
					{
						MLog.Log(this, "Sending XMBC to foreground, previous foregradoun win="
							+Utilities.GetForegroundWindow()+" xmbc="+GetXBMCHandle());
						int i = 0;//wait for xbmc to launch
						do { System.Threading.Thread.Sleep(100); i++; } 
						while (GetXBMCHandle() == IntPtr.Zero && i < 50);
						MoveToSecondScreen();
						Utilities.SetForegroundWindow(GetXBMCHandle());
						Monitor.RefreshFrequencySecondary();
						m_bringToForegroundOnce = true;
					}

                    sresp = fastJSON.JSON.Instance.ToObject<XBMCSimpleResponse>(result);
					if (sresp.result != null && sresp.result.Length > 0 && sresp.result[0].playerid == 1)
					{
						if (m_zoneDetails.ZoneState.Equals(ZoneState.NotStarted))
						{
							MLog.Log(this, "XBMC has active player");
							base.Play();
							//Play();
						}
						m_zoneDetails.LastLocalCommandDateTime = DateTime.Now;
						result = PostURLMessage(GET_URL, "Application.GetProperties", "properties", @"[""volume""]");
						resp = fastJSON.JSON.Instance.ToObject<XBMCResponse>(result);
						if (resp.result != null)
						{
							SetVolumeLevel(resp.result.volume);
						}

						result = PostURLMessage(GET_URL, "Playlist.GetItems", "playlistid", "1", "properties", @"[""title""]");
						resp = fastJSON.JSON.Instance.ToObject<XBMCResponse>(result);
						if (resp.result != null && resp.result.items != null && resp.result.items.Length > 0)
						{
							m_isXBMCPlayerRunning = true;
							m_zoneDetails.Title = resp.result.items[0].label;
							m_zoneDetails.Author = "xbmc";
							m_zoneDetails.RequirePower = true;
							m_zoneDetails.IsActive = true;
							m_zoneDetails.Genre = "";//resp.result.type;
							m_zoneDetails.ActivityType = GlobalCommands.xbmc;
							if (!MZPState.Instance.IsWinloadLoading)
								Utilities.SetForegroundWindow(GetXBMCHandle());
						}
						else
						{
							if (m_zoneDetails.IsActive)
								m_zoneDetails.ZoneStop();
							m_isXBMCPlayerRunning = false;
						}
					}
					else
					{
						if (m_zoneDetails.IsActive)
							m_zoneDetails.ZoneStop();
						m_isXBMCPlayerRunning = false;
					}
                }
                catch (Exception ex)
                {
                    MLog.Log(this, "Unable to json parse XBMC response " + result + " err="+ex.Message);
                }
            }
        }

        public override void Tick()
        {
            base.Tick();
            //slower tick
            if (DateTime.Now.Subtract(m_lastSlowTickDateTime).Duration().TotalSeconds > 10)
            {
                GetXBMCStatus();
                m_lastSlowTickDateTime = DateTime.Now;

                /*if (m_zoneDetails.IsActive && !MZPState.Instance.PowerControlIsOn(m_zoneDetails.ParentZoneId))
                {
                    MLog.Log(this, "Powering on parent zone id " + m_zoneDetails.ParentZoneId + " for XBMC child " + m_zoneDetails.ZoneName);
                    MZPState.Instance.PowerControlOn(m_zoneDetails.ParentZoneId);
                }*/
            }
        }
    }
}

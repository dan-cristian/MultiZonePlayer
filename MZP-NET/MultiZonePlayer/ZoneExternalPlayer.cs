﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiZonePlayer
{
    public class ZoneExternalPlayerBase :ZoneBase, IZoneActivity
    {

        protected Metadata.ZoneDetails m_zoneDetails;
        protected Display m_display;

        public virtual void Stop()
        {
            m_zoneDetails.ZoneState = Metadata.ZoneState.NotStarted;
            m_zoneDetails.IsActive = false;
        }

        public void Close()
        {
            m_zoneDetails.ZoneState = Metadata.ZoneState.NotInitialised;
            m_zoneDetails.IsActive = false;
        }
        public virtual void Next()
        {
        }

        public void NextPlaylist()
        {
        }

        public void Previous()
        {
        }

        public void PreviousPlaylist()
        {
        }

        public void NextMood()
        {
        }

        public void PreviousMood()
        {
        }

        public virtual void Play()
        {
            m_zoneDetails.ZoneState = Metadata.ZoneState.Running;
            m_zoneDetails.IsActive = true;

        }
        public virtual void Pause()
        {
            m_zoneDetails.ZoneState = Metadata.ZoneState.Paused;
        }
        public virtual void Mute()
        {
        }
        public virtual void VolumeUp()
        {
        }
        public virtual void VolumeDown()
        {
        }
        public void SaveStateIni()
        {
        }
        public void Guide()
        {
        }

        public Metadata.ZoneState GetState()
        {

            return m_zoneDetails.ZoneState;
        }

        public Metadata.ZoneDetails ZoneDetails
        {
            get
            {
                return m_zoneDetails;
            }
        }

        public bool IsActive()
        {
            return m_zoneDetails.IsActive;

        }

        public void SetVolumeLevel(int volume)
        {
            m_zoneDetails.VolumeLevel = volume;
        }

        public int GetVolumeLevel()
        {
            return m_zoneDetails.VolumeLevel;
        }

        public long Position
        {
            get { return -1; }
        }

        public int PositionPercent
        {
            get
            {
                return -1;
            }
        }

        public virtual void Tick()
        {
            //not implemented
        }
    }

    public class ZonePlayerXBMC:ZoneExternalPlayerBase
    {
        //http://wiki.xbmc.org/index.php?title=JSON-RPC_API/v4
        private int m_playerId = 1;
        private String STATUS_URL = "/jsonrpc?UpdateState";
        private String CMD_URL = "/jsonrpc?SendRemoteKey";

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
        /*
        public class XBMCSimpleResult
        {
            public int playerid;
            public string type;
            public int volume;

            public XBMCSimpleResult()
            { }
        }*/

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
        

        public ZonePlayerXBMC(Metadata.ZoneDetails zoneDetails)
        {
            m_zoneDetails = zoneDetails;
            if (!Utilities.IsProcAlive(IniFile.PARAM_XBMC_PROCESS_NAME[1]))
                MZPState.RestartXBMC();
            Display displayZone = MZPState.Instance.DisplayList.Find(x => x.ZoneDetails.ParentZoneId == zoneDetails.ParentZoneId && x.ZoneDetails.HasDisplay);
            if (displayZone != null && !displayZone.IsOn)
            {
                displayZone.IsOn = true;
                System.Threading.Thread.Sleep(10000);
                if (displayZone.InputType != Display.InputTypeEnum.HDMI)
                    displayZone.InputType = Display.InputTypeEnum.HDMI;
            }
        }

        //shortcut for cmds
        private String PostURLCmdMessage(String method, params String[] parameters)
        {
            return PostURLMessage(CMD_URL, method, parameters);
        }

        private String PostURLMessage(String URL, String method, params String[] parameters)
        {
            WebPostRequest post = new WebPostRequest(m_zoneDetails.DisplayConnection+URL);//IniFile.PARAM_XBMC_COMMAND_URL[1]);
            String paramName,paramValue;

            String msg;
            if (parameters.Length>0)
                msg = @"{""jsonrpc"": ""2.0"", ""method"": ""<M>"", ""params"": { <P> }, ""id"": 1}";
            else
                msg = @"{""jsonrpc"": ""2.0"", ""method"": ""<M>"", ""id"": 1}"; ;
            String pair = @"""<N>"": <V>,<P>";
            msg = msg.Replace("<M>", method);

            for (int i = 0; i < parameters.Length; i=i+2)
            {
                paramName = parameters[i];
                paramValue = parameters[i+1];
                
                msg = msg.Replace("<P>",pair);
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
            //MessageBox.Show(msg);
            String res;
            try
            {
                res = post.GetResponse();
            }
            catch (Exception ex)
            {
                res = "{xbmc web server error:" + ex.Message+"}";
            }
            //MLog.Log(null, res);
            return res;
        }

        public override void Play()
        {
            base.Play();
            PostURLCmdMessage("Player.PlayPause", "playerid", m_playerId.ToString());
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
 	        base.Stop();
            PostURLCmdMessage("Player.Stop", "playerid", m_playerId.ToString());
        }

        public override void Pause()
        {
            base.Pause();
            PostURLCmdMessage("Player.PlayPause", "playerid", m_playerId.ToString());
        }

        public override void Mute()
        {
            base.Mute();
            PostURLCmdMessage("Application.SetMute", "mute", "toggle");
        }

        private void GetXBMCStatus()
        {
            XBMCResponse resp;
            XBMCSimpleResponse sresp;

            if (!Utilities.IsProcAlive(IniFile.PARAM_XBMC_PROCESS_NAME[1]))
            {
                Stop();
            }
            else
            {
                String result = PostURLMessage(STATUS_URL, "Player.GetActivePlayers");
                try
                {
                    sresp = fastJSON.JSON.Instance.ToObject<XBMCSimpleResponse>(result);
                    if (sresp.result != null && sresp.result.Length > 0 && sresp.result[0].playerid == 1)
                    {
                        if (!m_zoneDetails.ZoneState.Equals(Metadata.ZoneState.Running))
                        {
                            MLog.Log(this, "XBMC has active player");
                            base.Play();
                        }

                        result = PostURLMessage(STATUS_URL, "Application.GetProperties", "properties", @"[""volume""]");
                        resp = fastJSON.JSON.Instance.ToObject<XBMCResponse>(result);
                        if (resp.result != null)
                        {
                            SetVolumeLevel(resp.result.volume);
                        }

                        result = PostURLMessage(STATUS_URL, "Playlist.GetItems", "playlistid", "1", "properties", @"[""title""]");
                        resp = fastJSON.JSON.Instance.ToObject<XBMCResponse>(result);
                        if (resp.result != null && resp.result.items != null && resp.result.items.Length > 0)
                        {
                            m_zoneDetails.Title = resp.result.items[0].label;
                        }
                    }
                    else
                    {
                        base.Stop();
                    }
                }
                catch (Exception ex)
                {
                    MLog.Log(this, "Unable to json parse XBMC response " + result + " err="+ex.Message);
                    base.Stop();
                }
            }
        }

        public override void Tick()
        {
            base.Tick();
            //slower tick
            if (DateTime.Now.Subtract(m_lastSlowTickDateTime).Duration().TotalSeconds > 1)
            {
                GetXBMCStatus();
                m_lastSlowTickDateTime = DateTime.Now;

                if (m_zoneDetails.IsActive && !MZPState.Instance.PowerControl.IsPowerOn(m_zoneDetails.ParentZoneId))
                {
                    MLog.Log(this, "Powering on parent zone id " + m_zoneDetails.ParentZoneId + " for XBMC child " + m_zoneDetails.ZoneName);
                    MZPState.Instance.PowerControl.PowerOn(m_zoneDetails.ParentZoneId);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiZonePlayer
{
    public class ZoneExternalPlayerBase : IZoneActivity
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
        public void Mute()
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

        public ZonePlayerXBMC(Metadata.ZoneDetails zoneDetails)
        {
            m_zoneDetails = zoneDetails;
        }

        //shortcut for cmds
        private String PostURLCmdMessage(String method, String paramName, String paramValue)
        {
            return PostURLMessage(CMD_URL, method, paramName, paramValue);
        }

        private String PostURLMessage(String URL, String method, String paramName, String paramValue)
        {
            WebPostRequest post = new WebPostRequest(m_zoneDetails.DisplayConnection+URL);//IniFile.PARAM_XBMC_COMMAND_URL[1]);
            String msg;
            if (paramName!="")
                msg= @"{""jsonrpc"": ""2.0"", ""method"": """+ method + @""", ""params"": { """ + paramName + @""": " + paramValue + @" }, ""id"": 1}";
            else
                msg = @"{""jsonrpc"": ""2.0"", ""method"": """ + method + @""", ""id"": 1}";
            post.Add(msg, "");
            //MessageBox.Show(msg);
            String res = post.GetResponse();
            MLog.Log(null, res);
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
            m_zoneDetails.VolumeLevel = Math.Max(0, m_zoneDetails.VolumeLevel-1);
            PostURLCmdMessage("Application.SetVolume", "volume", m_zoneDetails.VolumeLevel.ToString());
        }

        public override void VolumeUp()
        {
            base.VolumeUp();
            m_zoneDetails.VolumeLevel++;
            PostURLCmdMessage("Application.SetVolume", "volume", m_zoneDetails.VolumeLevel.ToString());
        }

        public override void  Stop()
        {
 	        base.Stop();
            PostURLCmdMessage("Player.Stop", "playerid", m_playerId.ToString());
        }

        public override void Pause()
        {
            base.Pause();
            PostURLCmdMessage("Player.PlayPause", "playerid", m_playerId.ToString());
        }

        private void GetXBMCStatus()
        {
            String result=PostURLMessage(STATUS_URL, "Player.GetActivePlayers", "", "");
            if (result.Contains("playerid"))
            {
                base.Play();
            }
            else
            {
                base.Stop();
            }

        }

        public override void Tick()
        {
            base.Tick();
            GetXBMCStatus();
        }
    }
}

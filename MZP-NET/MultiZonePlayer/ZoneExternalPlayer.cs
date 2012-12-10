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
        public void Pause()
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

        private static String PostURLMessage(String method, String paramName, String paramValue)
        {
            WebPostRequest post = new WebPostRequest("http://192.168.0.10:12347/jsonrpc?SendRemoteKey");
            String msg = @"{""jsonrpc"": ""2.0"", ""method"": """+ method + @""", ""params"": { """ + paramName + @""": " + paramValue + @" }, ""id"": 1}";
            post.Add(msg, "");
            //MessageBox.Show(msg);
            String res = post.GetResponse();
            MLog.Log(null, res);
            return res;
        }

        public override void VolumeDown()
        {
 	        base.VolumeDown();
            m_zoneDetails.VolumeLevel++;
            PostURLMessage("Application.SetVolume", "volume", m_zoneDetails.VolumeLevel.ToString());
        }

        public override void VolumeUp()
        {
            base.VolumeUp();
            m_zoneDetails.VolumeLevel--;
            PostURLMessage("Application.SetVolume", "volume", m_zoneDetails.VolumeLevel.ToString());
        }
    }
}

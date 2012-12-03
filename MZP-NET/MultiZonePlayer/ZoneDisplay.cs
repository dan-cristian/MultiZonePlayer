using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiZonePlayer
{
    public abstract class ZoneDisplay : IZoneActivity
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
           get{ return -1;}
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

    public class ZoneDisplayLG:ZoneDisplay
    {
        private String m_inputType, m_input;
        private Boolean m_isOn;
        private DateTime m_lastSlowTickDateTime = DateTime.Now;
        DisplayLGTV m_tv;

        public String InputType
        {
            get { return m_inputType; }
            set { m_inputType = value; }
        }

        public ZoneDisplayLG(Metadata.ZoneDetails p_zoneDetails)
        {
            m_zoneDetails = p_zoneDetails;

            m_display = MZPState.Instance.DisplayList.Find(x => x.Connection.Equals(p_zoneDetails.DisplayConnection));
            if (m_display == null)
            {
                m_display = new DisplayLGTV(p_zoneDetails.DisplayConnection);
                MLog.Log(this, "Warning, display " +p_zoneDetails.DisplayConnection+ " not found in MZPSTATE list, cmd done too fast");
            }
            m_tv = (DisplayLGTV)m_display;

            CacheCurrentState();
        }

        public override void Stop()
        {
            base.Stop();
            m_tv.IsOn = false;
        }

        public override void Play()
        {
            base.Play();
            m_tv.IsOn = true;
        }

        public override void VolumeUp()
        {
            base.VolumeUp();
            m_zoneDetails.VolumeLevel = m_tv.VolumeLevel;
            m_zoneDetails.VolumeLevel++;
            m_tv.VolumeLevel = m_zoneDetails.VolumeLevel;
        }

        public override void VolumeDown()
        {
            base.VolumeDown();
            m_zoneDetails.VolumeLevel = m_tv.VolumeLevel;
            m_zoneDetails.VolumeLevel--;
            m_tv.VolumeLevel = m_zoneDetails.VolumeLevel;
        }

        public override void Next()
        {
            base.Next();
            m_tv.NextInput();
        }

        public String Input
        {
            get
            { return m_tv.Input; }
            set
            { m_tv.Input = value; }
        }

        private void CacheCurrentState()
        {
            if (!m_tv.IsBusy)
            {
                m_inputType = m_tv.InputType;
                m_input = m_tv.InputCached;
                m_zoneDetails.VolumeLevel = m_tv.VolumeLevel;
                m_zoneDetails.Title = InputType + " ("+ m_input + ")";
                m_isOn = m_tv.IsOn;
            }
        }


        public Metadata.ValueList ProcessAction(Metadata.GlobalCommands cmdRemote, Metadata.ValueList vals)
        {
            Metadata.ValueList result = new Metadata.ValueList();
            String action = action = vals.GetValue(Metadata.GlobalParams.action);
            switch (cmdRemote)
            {
                case Metadata.GlobalCommands.tvsetinput:
                    Input = action;
                    break;
            }
            return result;
        }

        public override void Tick()
        {
            base.Tick();

            // SLOW TICK
            if (DateTime.Now.Subtract(m_lastSlowTickDateTime).Duration().TotalSeconds > 30)
            {
                m_lastSlowTickDateTime = DateTime.Now;
                CacheCurrentState();
                
                if (m_isOn)
                {
                    m_zoneDetails.ZoneState = Metadata.ZoneState.Running;
                    m_zoneDetails.IsActive = true;
                    if (m_inputType.Equals(DisplayLGTV.InputTypeEnum.HDMI.ToString()) && !MZPState.Instance.PowerControl.IsPowerOn(m_zoneDetails.ParentZoneId))
                    {
                        MLog.Log(this, "Powering on parent zone id " + m_zoneDetails.ParentZoneId + " for child " + m_zoneDetails.ZoneName);
                        MZPState.Instance.PowerControl.PowerOn(m_zoneDetails.ParentZoneId);
                    }
                    //else
                    //    MLog.Log(this, "Power on Display not needed, input="+m_inputType + " IsPowerOnParent="+MZPState.Instance.PowerControl.IsPowerOn(m_zoneDetails.ParentZoneId));
                }
                else
                {
                    m_zoneDetails.ZoneState = Metadata.ZoneState.NotStarted;
                    m_zoneDetails.IsActive = false;
                    MLog.Log(this, "DisplayTV is off");

                }

                
            }
        }

        
    }
}

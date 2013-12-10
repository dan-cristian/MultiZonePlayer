using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiZonePlayer
{
    public abstract class ZoneDisplay : ZoneBase
    {
        protected Display m_display;

    }

    public class ZoneDisplayLG:ZoneDisplay
    {
        private DisplayLGTV.InputTypeEnum m_inputType;
        private String m_input;
        private Boolean m_isOn;
        DisplayLGTV m_tv;

        public DisplayLGTV.InputTypeEnum InputType{
            get { return m_inputType; }
        }

        public ZoneDisplayLG(ZoneDetails p_zoneDetails){
            m_zoneDetails = p_zoneDetails;

            m_display = MZPState.Instance.DisplayList.Find(x => x.Connection.Equals(p_zoneDetails.DisplayConnection));
            if (m_display == null)
            {
                m_display = new DisplayLGTV(p_zoneDetails.DisplayConnection, p_zoneDetails);
                MLog.Log(this, "Warning, display " +p_zoneDetails.DisplayConnection+ " not found in MZPSTATE list, cmd done too fast");
            }
            m_tv = (DisplayLGTV)m_display;

            CacheCurrentState();
        }

        public override void Stop(){
            base.Stop();
            m_tv.IsOn = false;
        }

        public override void Play(){
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
            m_input = m_tv.InputCached;
        }

        public String Input
        {
            get
            { return m_tv.Input; }
            set
            { 
                m_tv.Input = value;
                m_input = m_tv.InputCached;
            }
        }

        private void CacheCurrentState()
        {
            if (!m_tv.IsBusy)
            {
                m_inputType = m_tv.InputTypeCached;
                m_input = m_tv.InputCached;
                m_zoneDetails.VolumeLevel = m_tv.VolumeLevel;
                m_zoneDetails.Title = InputType + " ("+ m_input + ")";
                m_isOn = m_tv.IsOnCached;
            }
        }


		public ValueList ProcessAction(GlobalCommands cmdRemote, ValueList vals, ref CommandResult cmdresult)
        {
            ValueList result = new ValueList();
            String action = action = vals.GetValue(GlobalParams.action);
            switch (cmdRemote)
            {
                case GlobalCommands.tvsetinput:
                    Input = action;
                    break;
                default:
                    MLog.Log(this, "WARNING, unprocessed zone command " + cmdRemote);
                    break;
            }
            return result;
        }

        public override void Tick()
        {
            // SLOW TICK
            if (DateTime.Now.Subtract(m_lastSlowTickDateTime).Duration().TotalSeconds > 45)
            {
                m_lastSlowTickDateTime = DateTime.Now;
                CacheCurrentState();
                
                if (m_isOn)
                {
                    m_zoneDetails.ZoneState = ZoneState.Running;
                    //m_zoneDetails.IsActive = true;
                    m_zoneDetails.RequirePower = m_inputType == DisplayLGTV.InputTypeEnum.HDMI;

					if (m_zoneDetails.RequirePower && !MZPState.Instance.PowerControlIsOn(m_zoneDetails.ZoneId))
                    {
                        MLog.Log(this, "Powering on display zone id " + m_zoneDetails.ZoneId 
							+ " for LGTV child " + m_zoneDetails.ZoneName);
                        MZPState.Instance.PowerControlOn(m_zoneDetails.ZoneId);
                    }
                }
                else
                {
                    m_zoneDetails.ZoneState = ZoneState.NotStarted;
                    //m_zoneDetails.IsActive = false;
                    m_zoneDetails.RequirePower = false;
                    MLog.Log(this, "Zone Display has TV off");
                }
            }
        }
    }
}

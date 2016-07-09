using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiZonePlayer
{
    
    public interface IZoneActivity
    {
        void Stop();
        void Close();
        void Next();
        void NextPlaylist();
        void NextMood();
        void Previous();
        void PreviousPlaylist();
        void PreviousMood();
        void Play();
        void Pause();
        void Mute();
        void VolumeUp();
        void VolumeDown();
        void SaveStateIni();
        void Guide();
        ZoneState GetState();
        ZoneDetails ZoneDetails
        {
            get;
        }
        long Position
        {
            get;
        }
        int PositionPercent
        {
            get;
        }
        bool IsActive();
        void SetVolumeLevel(int volume);
        int GetVolumeLevel();
        void Tick();
    }

    public class BaseActivity:IZoneActivity
    {
        protected DateTime m_lastSlowTickDateTime = DateTime.Now;
		protected ZoneDetails m_zoneDetails;

		public virtual void Stop()
		{
			m_zoneDetails.ZoneStop();
		}

		public virtual void Close()
		{
			m_zoneDetails.ZoneClose();
		}

		public virtual void Next()
		{
		}

		public virtual void NextPlaylist()
		{
		}

		public virtual void Previous()
		{
		}

		public virtual void PreviousPlaylist()
		{
		}

		public virtual void NextMood()
		{
		}

		public virtual void PreviousMood()
		{
		}

		public virtual void Play()
		{
			m_zoneDetails.ZoneState = ZoneState.Running;
			//m_zoneDetails.IsActive = true;

		}
		public virtual void Pause()
		{
			m_zoneDetails.ZoneState = ZoneState.Paused;
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
		public virtual void SaveStateIni()
		{
		}

		public virtual void Guide()
		{
		}

		public virtual ZoneState GetState()
		{

			return m_zoneDetails.ZoneState;
		}

		public virtual ZoneDetails ZoneDetails
		{
			get
			{
				return m_zoneDetails;
			}
		}

		public virtual bool IsActive()
		{

			return m_zoneDetails.IsActive;

		}

		public virtual void SetVolumeLevel(int volume)
		{
			m_zoneDetails.VolumeLevel = volume;
		}

		public virtual int GetVolumeLevel()
		{
			return m_zoneDetails.VolumeLevel;
		}

		public virtual long Position
		{
			get { return -1; }
		}

		public virtual int PositionPercent
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

    public interface INavigableUI
    {
        void DirectionUp();
        void DirectionDown();
        void DirectionLeft();
        void DirectionRight();
        void Select();
        void DirectionBack();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MultiZonePlayer
{
    class ZoneAux : IZoneActivity
    {
        private AuxPlayer auxPlayer = null;
        private ZonesForm zoneForm;
        private String inputDeviceName;
        private String outputDeviceName;

        public ZoneAux(ZonesForm zoneForm, String inputDeviceName, String outputDeviceName)
        {
            auxPlayer = new AuxPlayer();
            this.zoneForm = zoneForm;
            this.inputDeviceName = inputDeviceName;
            this.outputDeviceName = outputDeviceName;
        }

        public virtual void Play()
        {
            auxPlayer.OpenClip(outputDeviceName, inputDeviceName, zoneForm);
        }

        public virtual void Stop()
        {
            auxPlayer.StopClip();
        }

        public virtual void Close()
        {
            auxPlayer.CloseClip();
            auxPlayer.Close();
        }

        public void Pause()
        {
        }

        public virtual void Guide()
        {
        }
        public void Mute()
        {
        }

        public void VolumeUp()
        {
            auxPlayer.ChangeVolume(IniFile.VOLUME_STEP);
        }

        public void VolumeDown()
        {
            auxPlayer.ChangeVolume(-IniFile.VOLUME_STEP);
        }


        public void SaveStateIni()
        {
            IniFile.IniWriteValuetoTemp(IniFile.INI_SECTION_ZONERESUME + zoneForm.GetZoneId(), "VOLUME", GetVolumeLevel().ToString());
        }

        public int GetVolumeLevel()
        {
            return auxPlayer.GetVolumeLevel();
        }

        public void SetVolumeLevel(int volume)
        {
            auxPlayer.SetVolume(volume);
        }

        public Metadata.ZoneState GetState()
        {
            return auxPlayer.GetState();
        }

        public Metadata.ZoneDetails ZoneDetails
        {
            get
            {
                return zoneForm.ZoneDetails;
            }
        }

        public virtual bool IsActive()
        {
            return (auxPlayer.GetState() == Metadata.ZoneState.Running);
        }

        public virtual void Next()
        {
            MLog.Log(null,"Next() not implemented for ZoneAux");
        }

        public virtual void NextPlaylist()
        {
            MLog.Log(null, "Nextplaylist() not implemented for ZoneAux");
        }

        public virtual void Previous()
        {
            MLog.Log(null,"Previous() not implemented for ZoneAux");
        }

        public virtual void PreviousPlaylist()
        {
            MLog.Log(null, "Prevplaylist() not implemented for ZoneAux");
        }

        public virtual void NextMood()
        {
        }

        public virtual void PreviousMood()
        {
        }

        public long Position
        {
            get
            {
                return auxPlayer.Position;
            }
        }

        public int PositionPercent
        {
            get
            {
                return auxPlayer.PositionPercent;
            }
        }

        public void Tick()
        {
            //not implemented
        }
    }
}

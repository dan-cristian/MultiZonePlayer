using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MultiZonePlayer
{
    class ZoneAux : BaseActivity
    {
        private AuxPlayer auxPlayer = null;
        private ZoneGeneric zoneForm;
        private String inputDeviceName;
        private String outputDeviceName;

        public ZoneAux(ZoneGeneric zoneForm, String inputDeviceName, String outputDeviceName)
        {
            auxPlayer = new AuxPlayer();
            this.zoneForm = zoneForm;
            this.inputDeviceName = inputDeviceName;
            this.outputDeviceName = outputDeviceName;
        }

        public override void Play()
        {
			base.Play();
            auxPlayer.OpenClip(inputDeviceName, zoneForm);
        }

        public override void Stop()
        {
			base.Stop();
            auxPlayer.StopClip();
        }

        public override void Close()
        {
			base.Close();
            auxPlayer.CloseClip();
            auxPlayer.Close();
        }

        public override void VolumeUp()
        {
			base.VolumeUp();
            auxPlayer.ChangeVolume(IniFile.VOLUME_STEP);
        }

		public override void VolumeDown()
        {
			base.VolumeDown();
            auxPlayer.ChangeVolume(-IniFile.VOLUME_STEP);
        }


		public override int GetVolumeLevel()
        {
            return auxPlayer.GetVolumeLevel();
        }

		public override void SetVolumeLevel(int volume)
        {
            auxPlayer.SetVolume(volume);
        }

        public override ZoneState GetState()
        {
            return auxPlayer.GetState();
        }

        

        public override bool IsActive()
        {
            return (auxPlayer.GetState() == ZoneState.Running);
        }

        public override void Next()
        {
            MLog.Log(null,"Next() not implemented for ZoneAux");
        }

		public override void NextPlaylist()
        {
            MLog.Log(null, "Nextplaylist() not implemented for ZoneAux");
        }

		public override void Previous()
        {
            MLog.Log(null,"Previous() not implemented for ZoneAux");
        }

		public override void PreviousPlaylist()
        {
            MLog.Log(null, "Prevplaylist() not implemented for ZoneAux");
        }

		public override void NextMood()
        {
        }

		public override void PreviousMood()
        {
        }

		public override long Position
        {
            get
            {
                return auxPlayer.Position;
            }
        }

		public override int PositionPercent
        {
            get
            {
                return auxPlayer.PositionPercent;
            }
        }

		public override void Tick()
        {
            //not implemented
        }
    }
}

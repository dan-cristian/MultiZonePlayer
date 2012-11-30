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
        Metadata.ZoneState GetState();
        Metadata.ZoneDetails ZoneDetails
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
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using NAudio.Wave;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;

namespace NAudioDemo
{
    public partial class MP3StreamingPanel : UserControl
    {
        private VolumeWaveProvider16 volumeProvider;

        public MP3StreamingPanel()
        {
            InitializeComponent();
            this.volumeSlider1.VolumeChanged += new EventHandler(volumeSlider1_VolumeChanged);
            this.Disposed += this.MP3StreamingPanel_Disposing;
        }

        void volumeSlider1_VolumeChanged(object sender, EventArgs e)
        {
            if (this.volumeProvider != null)
            {
                this.volumeProvider.Volume = this.volumeSlider1.Volume;
            }
        }

        

        delegate void ShowErrorDelegate(string message);

        private void ShowError(string message)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new ShowErrorDelegate(ShowError), message);
            }
            else
            {
                MessageBox.Show(message);
            }
        }

        

        private void buttonPlay_Click(object sender, EventArgs e)
        {
            
        }

        

        private void ShowBufferState(double totalSeconds)
        {
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
        }

        

        private void MP3StreamingPanel_Disposing(object sender, EventArgs e)
        {
            //StopPlayback();
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            
        }

        
    }
}

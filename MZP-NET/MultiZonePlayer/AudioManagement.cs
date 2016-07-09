using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MultiZonePlayer
{
    public partial class AudioManagement : Form
    {
        public AudioManagement()
        {
            InitializeComponent();
        }

        private void btPlayCount_Click(object sender, EventArgs e)
        {
            progressBar.Maximum = MediaLibrary.AllAudioFiles.PlaylistItems.Count;
            foreach (AudioItem item in MediaLibrary.AllAudioFiles.PlaylistItems)
            {
                item.SetPlayCount(0);
                progressBar.Value++;
                Application.DoEvents();
            }
            MediaLibrary.AllAudioFiles.SaveUpdatedItems();
        }
    }
}

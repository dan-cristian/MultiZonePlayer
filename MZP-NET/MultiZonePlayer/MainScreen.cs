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
    public partial class MainScreen : Form
    {
        public FormOptions m_formOptions=null;
        private ControlCenter m_controlForm;
        private VideoPlayList m_videoForm;

        public MainScreen()
        {
            MZPState.Initialise();
            InitializeComponent();
            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;
            
        }

        private void ShowNewForm(object sender, EventArgs e)
        {
            
        }

        private void OpenFile(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            openFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string FileName = openFileDialog.FileName;
            }
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string FileName = saveFileDialog.FileName;
            }
        }

        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CutToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void ToolBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStrip.Visible = toolBarToolStripMenuItem.Checked;
        }

        private void StatusBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statusStrip.Visible = statusBarToolStripMenuItem.Checked;
        }

        private void CascadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        private void TileVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        private void TileHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void ArrangeIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.ArrangeIcons);
        }

        private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form childForm in MdiChildren)
            {
                childForm.Close();
            }
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_formOptions = new FormOptions();
            m_formOptions.MdiParent = this;
            m_formOptions.Show();

        }

        private void MainScreen_Load(object sender, EventArgs e)
        {
            m_controlForm = new ControlCenter(this);
            m_controlForm.MdiParent = this;
            m_controlForm.Show();

            //Set up the event handler for the KeyboardHook's
            //KeyIntercepted event
            //Program.kh.KeyIntercepted += new KeyboardHook.KeyboardHookEventHandler(ControlCenter.kh_KeyIntercepted);
		}

        private void timerGetFocus_Tick(object sender, EventArgs e)
        {

		}

        private void MainScreen_FormClosing(object sender, FormClosingEventArgs e)
        {
			MLog.Log(this, "Form closing");
            m_controlForm.Close();
        }

        private void videoMamagementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_videoForm = new VideoPlayList();
            m_videoForm.MdiParent = this;
            m_videoForm.Show();
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form test = new Test();
            test.MdiParent = this;
            test.Show();
            
        }

        private void audioManagementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form audio = new AudioManagement();
            audio.MdiParent = this;
            audio.Show();
        }

		private void MainScreen_Activated(object sender, EventArgs e)
		{
			MLog.Log(this, "Got focus");
		}

		private void chartToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SimpleGraph form = new SimpleGraph(true, true, true, true, true);
			form.MdiParent = this;
			form.Show();
			form.ShowEventGraph(19,24);
			SimpleGraph form2 = new SimpleGraph(true, true, true, true, true);
			form2.MdiParent = this;
			form2.Show();
			Alert.CreateAlert("A new alert", ZoneDetails.ZoneDetailsList[0], false, null,
				Alert.NotificationFlags.NeedsImmediateUserAck, 1, Alert.NotificationFlags.NotifyUserAfterXMinutes, 30);
			form2.ShowTempGraph(12, ZoneDetails.ZoneDetailsList);
			Alert.GetAlertsToSend();
		}
    }
}

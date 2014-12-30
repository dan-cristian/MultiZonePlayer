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
        public static MainScreen parentForm = null;
        public FormOptions m_formOptions=null;
        private ControlCenter m_controlForm;
        private VideoPlayList m_videoForm;
        private RawInputDevice m_rawDeviceId = null;

        public RawInputDevice RawDeviceId {
            get { return m_rawDeviceId; }
        }
        private RawInputDevice.DeviceEventHandler m_rawevent = null;
        public MainScreen()
        {
            parentForm = this;
            MZPState.Initialise();
            InitializeComponent();
        }

        // required by RawInput, The WndProc is overridden to allow InputDevice to intercept
        // messages to the window and thus catch WM_INPUT messages
        protected override void WndProc(ref Message message) {
            if (m_rawDeviceId != null) {
                m_rawDeviceId.ProcessMessage(message);
            }
            base.WndProc(ref message);
        }

        public void RegisterRawInput() {
            //clean previous rawdevice
            try {
                if (m_rawDeviceId != null && m_rawevent != null) {
                    m_rawDeviceId.KeyPressed -= m_rawevent;
                    m_rawDeviceId = null;
                }
            }
            catch (Exception ex) {
                MLog.Log(this, "Err removing event handler ex=" + ex.Message);
            }

            //not needed
            //Program.kh = new KeyboardHook(KeyboardHook.Parameters.PassAllKeysToNextApp);
            
            // Create a new InputDevice object, get the number of
            // keyboards, and register the method which will handle the 
            // InputDevice KeyPressed event
            m_rawDeviceId = new RawInputDevice(Handle);
            m_rawevent = new RawInputDevice.DeviceEventHandler(m_KeyPressed);
            m_rawDeviceId.KeyPressed += m_rawevent;
        }

        
        /*public void RegisterRawInput() {
            RegisterRawInput();
            Program.kh = new KeyboardHook(KeyboardHook.Parameters.PassAllKeysToNextApp);
            
            //Program.kh.KeyIntercepted += new KeyboardHook.KeyboardHookEventHandler(kh_KeyIntercepted);
        }*/

        /*void kh_KeyIntercepted(KeyboardHook.KeyboardHookEventArgs e) {
            //Check if this key event is being passed to
            //other applications and disable TopMost in 
            //case they need to come to the front
            if (e.PassThrough) {
                this.TopMost = false;
            }

            MLog.LogKey(String.Format("KeyHook: {0} code={1} pass={2} name={3} up={4}",
                e.keySet, e.KeyCode, e.PassThrough, e.KeyName, e.keyUp));
            //ds.Draw(e.KeyName);
        }*/

        private void m_KeyPressed(object sender, ref RawInputDevice.KeyControlEventArgs e) {
            MLog.LogKey(String.Format("Raw: {0} down={1}", e.Keyboard.vKey, e.Keyboard.isKeyDownWinMessage));
            KeyDetail kd = new KeyDetail(e.Keyboard.vKey, e.Keyboard.deviceName, e.Keyboard.Name,
                e.Keyboard.isKeyDownWinMessage, e.Keyboard.isKeyUpWinMessage);
            if (parentForm.m_formOptions != null && parentForm.m_formOptions.Visible) {
                parentForm.m_formOptions.m_KeyPressed(kd);
                return;
            }

            API.DoCommandFromRawInput(kd);
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
			//MLog.Log(this, "Got focus");
		}

		private void chartToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SimpleGraph form = new SimpleGraph(true, true, true, true, true);
			form.MdiParent = this;
			form.Show();
			//form.ShowEventGraph(19,24);
			SimpleGraph form2 = new SimpleGraph(true, true, true, true, true);
			form2.MdiParent = this;
			form2.Show();
			Alert.CreateAlert("A new alert", ZoneDetails.ZoneDetailsList[0], false, null,
				Alert.NotificationFlags.NeedsImmediateUserAck, 1, Alert.NotificationFlags.NotifyUserAfterXMinutes, 30);
			//form2.ShowTempGraph(12, ZoneDetails.ZoneDetailsList);
			Alert.GetAlertsToSend();
		}
    }
}

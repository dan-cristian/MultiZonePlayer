using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MultiZonePlayer
{
    class ZoneRadio:ZoneAux
    {
        
        private Process extProc = null;
        private int appHandle = 0;
        private String inputDeviceName;

        public ZoneRadio(ZoneGeneric zoneForm, String inputDeviceName, String outputDeviceName):base(zoneForm,inputDeviceName,outputDeviceName)
        {
            this.inputDeviceName = inputDeviceName;
            LaunchExtApp(IniFile.PARAM_RADIO_APP[1]);
            //connect input to output
        }

        private void LaunchExtApp(String fileName)
        {
            try
            {
                MLog.Log(null,"launching ext app " + fileName);
                if (!File.Exists(fileName))
                {
                    MLog.Log(null,"Process File does not exist");
                    return;
                }
                extProc = new Process();
                extProc.StartInfo.FileName = fileName;
                extProc.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                extProc.Exited += new EventHandler(this.process_Exited);
                extProc.Start();
                extProc.WaitForInputIdle(10000);
                MLog.Log(this,"launched ext app " + fileName );
            }
            catch (Exception ex)
            {
                extProc = null;
                MLog.Log(this,"Error launch ext app " + ex.Message + ex.StackTrace);
            }
        }
        private void process_Exited(Object o, EventArgs e)
        {
            MLog.Log(null,"Application has exited");
        }

        public override void Play()
        {
            base.Play();
        }

        public override void Stop()
        {
            MLog.Log(null,"stopping ext app proc=" + extProc);

            if ((extProc != null) && (!extProc.HasExited))
            {
                extProc.CloseMainWindow();
                if (!extProc.WaitForExit(3000))
                    extProc.Kill();
                extProc.Dispose();
                extProc = null;
            }
            base.Stop();
            MLog.Log(null,"stopped ext app proc=" + extProc);
        }

        public override bool IsActive()
        {
            return (GetAppHandle() != 0);
        }

        public int GetAppHandle()
        {
            if ((extProc == null)||(extProc.HasExited))
            {
                appHandle = 0;
            }
            else
            {
                //appHandle = 0;// Utilities.FindWindowEx(extProc.MainWindowHandle, IntPtr.Zero, null, null).ToInt32();
                if (appHandle == 0)
                {
                    MLog.Log(null,"lost apphandle");
                    appHandle = WindowsEnumerator.GetWindowHandleByTitle(IniFile.PARAM_RADIO_APP_WIN_TITLE[1]);
                }
            }
            return appHandle;
        }

        public override void Next()
        {
            if (IsActive())
            {
                Utilities.PostMessageInt(appHandle, Utilities.WM_KEYDOWN, Utilities.VK_PRIOR, 0);
            }
        }

        public void SelectChannel(Char channel)
        {
            if (IsActive())
            {
                Utilities.PostMessageInt(appHandle, Utilities.WM_KEYDOWN, Utilities.VkKeyScan(channel), 0);
            }
        }

        public String GetInputDeviceName()
        {
            return inputDeviceName;
        }

        public String GetExtProcessStatus()
        {
            String status;
            if (extProc != null)
            {
                if (extProc.HasExited)
                    status = "Has exited";
                else
                    status = "Responding=" + extProc.Responding;
            }
            else
                status = "Process Killed";
            return status;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Configuration;
using System.Collections.Specialized;
using Vlc.DotNet.Core;

namespace MultiZonePlayer
{
    static class Program
    {
        public static KeyboardHook kh;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                System.Threading.Thread.CurrentThread.Name = "Main GUI";
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                initVLC();
                Application.Run(new MainScreen());
            }
            catch (Exception ex)
            {
                MLog.Log(ex, "Fatal Exception");
                if (MZPState.Instance != null)
                    MZPState.Instance.Shutdown();
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
            finally
            {
                if (kh != null)
                    ((IDisposable)kh).Dispose();

                if (VlcContext.IsInitialized)
                {
                    VlcContext.CloseAll();
                }
            }
        }

        static void initVLC()
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Configuration;
using System.Collections.Specialized;
using Vlc.DotNet.Core;
using SpecialServices;

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

				using (SingleProgramInstance spi = new SingleProgramInstance("MZP1770515310010"))
				{
					if (spi.IsSingleInstance)
					{
						Application.EnableVisualStyles();
						Application.SetCompatibleTextRenderingDefault(false);
						Application.Run(new MainScreen());
						Test.RunTest();
					}
					else
					{
						MessageBox.Show("ANother MZP instance already running");
						spi.RaiseOtherProcess();
					}
				}
            }
            catch (Exception ex)
            {
                MLog.Log(ex, "Fatal Exception");
				MessageBox.Show(ex.Message + "\n" + ex.StackTrace );
				if (ex.InnerException != null)
					MessageBox.Show(ex.InnerException.Message + "\n" + ex.InnerException.StackTrace);
                if (MZPState.Instance != null)
                    MZPState.Instance.Shutdown();
                
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

		
    }
}

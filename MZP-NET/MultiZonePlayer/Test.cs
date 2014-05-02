using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Speech.Synthesis;
using System.Speech.AudioFormat;
using System.Media;
using System.IO;
using DirectShowLib;
using System.Threading;
using System.Net;
using System.Web;
using System.Collections;
using System.Runtime.InteropServices;
using Microsoft.Win32;

using System.Management; // need to add System.Management to your project references.

using InTheHand.Net.Sockets;

using System.Text.RegularExpressions;
using Vlc.DotNet.Core;
using Vlc.DotNet.Core.Medias;
using Vlc.DotNet.Forms;

namespace MultiZonePlayer
{
    public partial class Test : Form
    {
		public static void RunTest()
		{
			//ScriptingRule rules = new ScriptingRule();
			//rules.LoadFromIni("Rules");
			//if (rules.Values != null) { };
		}

       // VlcControl vlcControl1;

        public Test()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
			foreach (ManagementItem comPort in ManagementItem.GetManagementItemsInfo())
			{
				MLog.Log(null, "Item: "+ string.Format("{0} – {1} - {2}", comPort.Name, comPort.Description, comPort.Manufacturer));
			}

			SetupDiWrap.ComPortNameFromFriendlyNamePrefix("modem");

			ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT * FROM Win32_POTSModem");
			foreach (ManagementObject mo in mos.Get())
			{
				// for modem name
				MLog.Log(this, "Modem " + mo["Caption"].ToString());

				// for modem port
				MLog.Log(this, "Port " + mo["AttachedTo"].ToString());
			}
 
        }

        /// <summary>
        /// Event handler for the <see cref="VlcControl.PositionChanged"/> event. 
        /// Updates the label containing the playback position. 
        /// </summary>
        /// <param name="sender">Event sending <see cref="VlcControl"/>. </param>
        /// <param name="e">Event arguments, containing the current position. </param>
        private void VlcControlOnPositionChanged(VlcControl sender, VlcEventArgs<float> e)
        {
            label1.Text = (e.Data * 100).ToString("000") + " %";
        }

        private const uint BN_CLICKED = 245;



        private void button2_Click(object sender, EventArgs e)
        {
            var usbDevices = GetUSBDevices();

            foreach (var usbDevice in usbDevices)
            {
                txt.AppendText(String.Format("SensorDevice ID: {0}, PNP SensorDevice ID: {1}, Description: {2}, Friendly {3} \n",
                    usbDevice.DeviceID, usbDevice.PnpDeviceID, usbDevice.Description, usbDevice.FriendlyName));
            }
        }

        static List<USBDeviceInfo> GetUSBDevices()
    {
      List<USBDeviceInfo> devices = new List<USBDeviceInfo>();

      ManagementObjectCollection collection;
      using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_USBHub"))
        collection = searcher.Get();      

      foreach (var device in collection)
      {
        devices.Add(new USBDeviceInfo(
        (string)device.GetPropertyValue("DeviceID"),
        (string)device.GetPropertyValue("PNPDeviceID"),
        (string)device.GetPropertyValue("Description"),
        //(string)device.GetPropertyValue("Name")
        GetDeviceFriendlyName((string)device.GetPropertyValue("DeviceID"))
        ));
      }

      collection.Dispose();
      return devices;
    }
  
    class USBDeviceInfo
  {
    public USBDeviceInfo(string deviceID, string pnpDeviceID, string description, string friendlyName)
    {
      this.DeviceID = deviceID;
      this.PnpDeviceID = pnpDeviceID;
      this.Description = description;
      this.FriendlyName = friendlyName;
    }
    public string DeviceID { get; private set; }
    public string PnpDeviceID { get; private set; }
    public string Description { get; private set; }
    public string FriendlyName { get; private set; }
  }



    private static string GetDeviceFriendlyName(String devUniqueId)
    {

        if (devUniqueId != string.Empty)
        {
            //string DevType = Parts[0].Substring(Parts[0].IndexOf(@"?\") + 2);
            //string DeviceInstanceId = Parts[1];
            //string DeviceUniqueID = Parts[2];
            string RegPath = @"SYSTEM\CurrentControlSet\Enum\" + devUniqueId;
            RegistryKey key = Registry.LocalMachine.OpenSubKey(RegPath);
            if (key != null)
            {
                object result = key.GetValue("FriendlyName");
                if (result != null)
                    return result.ToString();
                result = key.GetValue("DeviceDesc");
                if (result != null)
                    return result.ToString();
            }
        }
        return "no friedly name found";
    }


        private void button3_Click(object sender, EventArgs e)
        {
            //PlatformInvokeTst.MainA();
            SomeMethod();
        }

          protected void SomeMethod()
    {
        ManagementScope scope = new ManagementScope("root\\CIMV2");
       scope.Options.EnablePrivileges = true;
        string Win32_USBControlerDevice = "Select * From Win32_USBControllerDevice";
       ObjectQuery query = new ObjectQuery(Win32_USBControlerDevice);
     ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
        foreach (ManagementObject mgmtObj in searcher.Get())
        {
            Application.DoEvents();
           string strDeviceName = mgmtObj["Dependent"].ToString();
           string strQuotes = "'";
           strDeviceName = strDeviceName.Replace("\"", strQuotes);
           string[] arrDeviceName = strDeviceName.Split('=');
           strDeviceName = arrDeviceName[1];
           string Win32_PnPEntity = "Select * From Win32_PnPEntity "
               + "Where DeviceID =" + strDeviceName;
           ManagementObjectSearcher mySearcher = 
               new ManagementObjectSearcher(Win32_PnPEntity);
           foreach (ManagementObject mobj in mySearcher.Get())
           {
               string strDeviceID = mobj["DeviceID"].ToString();
               string[] arrDeviceID = strDeviceID.Split('\\');
               
               txt.AppendText("\nDescription = "                        + mobj["Description"].ToString());
               
               if (mobj["Manufacturer"] != null)
               {
                   txt.AppendText(" | Manufacturer = "                        + mobj["Manufacturer"].ToString());
               }
               txt.AppendText(" | SensorDevice Version ID & Vendor ID = " + arrDeviceID[1]);
               txt.AppendText(" | SensorDevice ID = " + arrDeviceID[2].Trim('{', '}'));
           }
       }
   }

        private void button4_Click(object sender, EventArgs e)
        {


        }

        private void button5_Click(object sender, EventArgs e)
        {
            //MZPState.Instance.DisplayList[0].SendCommand(DisplayLGTV.LGCommands.SCREENMUTE_kd, "0");
        }

        /// <summary>
		/// Create new file without overwritin existing files with the same fullfilepath.
		/// </summary>
		/// <param name="destPath">destination path of the new file</param>
		/// <param name="fullfilepath">fullfilepath of the file to be created</param>
		/// <returns>an output stream on the file</returns>
		private static Stream createNewFile(String destPath, String filename)
		{
			// replace characters, that are not allowed in filenames. (quick and dirrrrrty ;) )
			filename = filename.Replace(":", "");
			filename = filename.Replace("/", "");
			filename = filename.Replace("\\", "");
			filename = filename.Replace("<", "");
			filename = filename.Replace(">", "");
			filename = filename.Replace("|", "");
			filename = filename.Replace("?", "");
			filename = filename.Replace("*", "");
			filename = filename.Replace("\"", "");

			try
			{
				// create directory, if it doesn't exist
				if (!Directory.Exists(destPath))
					Directory.CreateDirectory(destPath);

				// create new file
				if (!File.Exists(destPath + filename + ".mp3"))
				{
					return File.Create(destPath + filename + ".mp3");
				}
				else // if file already exists, don't overwrite it. Instead, create a new file named <fullfilepath>(i).mp3
				{
					for (int i=1;; i++)
					{
						if (!File.Exists(destPath + filename + "(" + i + ").mp3"))
						{
							return File.Create(destPath + filename + "(" + i + ").mp3", 4096, FileOptions.Asynchronous);
						}
					}
				}
			}
			catch (IOException)
			{
				return null;
			}
		}

      private void button6_Click(object sender, EventArgs e)
      {
			Utilities.InternetConnectionState istate = Utilities.InternetConnectionState.INTERNET_CONNECTION_OFFLINE;
			Utilities.InternetGetConnectedState(ref istate, 0);
			MLog.Log(this, "Internet Connection state is " + istate);


      }
	  
      private void button7_Click(object sender, EventArgs e)
      {
          int vol = getvol() + 1;
          setvol(vol);
          textBox1.Text = getvol().ToString();
      }

      

      private void button8_Click(object sender, EventArgs e)
      {
          
      }


      private int getvol()
      {
          // By the default  set the volume to 0
          uint CurrVol = 0;
          // At this point, CurrVol gets assigned the volume
          Utilities.waveOutGetVolume(IntPtr.Zero, out CurrVol);
          // Calculate the volume
          //ushort CalcVol = (ushort)(CurrVol & 0x0000ffff);
          // Get the volume on a scale of 1 to 10 (to fit the trackbar)
          return (int)CurrVol/ (ushort.MaxValue / 100);
      }
        private void setvol(int vol)
        {
            // Calculate the volume that's being set
            int NewVolume = ((ushort.MaxValue / 100) * vol);
            // Set the same volume for both the left and the right channels
            //uint NewVolumeAllChannels = (((uint)NewVolume & 0x0000ffff) | ((uint)NewVolume << 16));
            // Set the volume
            Utilities.waveOutSetVolume(IntPtr.Zero, (uint)NewVolume);
        }

		private void txt_TextChanged(object sender, EventArgs e)
		{

		}

	}

        



    }


    




using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using DirectShowLib;


namespace MultiZonePlayer
{
    class DShowUtility
    {
        public static readonly Guid Clsid_SystemDeviceEnum = new Guid(0x62BE5D10, 0x60EB, 0x11d0, 0xBD, 0x3B, 0x00, 0xA0, 0xC9, 0x11, 0xCE, 0x86);
        /// <summary> MEDIATYPE_Audio 'auds' </summary>
        public static readonly Guid Clsid_AudioOutRender = new Guid("E0F158E1-CB04-11D0-BD4E-00A0C911CE86");
        public static readonly Guid Clsid_AudioInput = new Guid("33D9A762-90C8-11D0-BD43-00A0C911CE86");
        public static readonly String InfinitePinTeeFilter = "@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\\{F8388A40-D5BB-11D0-BE5A-0080C706568E}";

		[DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern uint waveOutGetDevCaps(IntPtr hwo, ref WAVEOUTCAPS pwoc, uint cbwoc);

		[DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern uint waveOutGetDevCaps(int hwo, ref WAVEOUTCAPS pwoc, /*uint*/ int cbwoc);

		[DllImport("winmm.dll", SetLastError = true)]
		static extern uint waveOutGetNumDevs();

		[System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
		public struct WAVEOUTCAPS
		{
			public short wMid;
			public short wPid;
			public int vDriverVersion;

			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 32)]
			public string szPname;

			public int dwFormats;
			public short wChannels;
			public short wReserved;
			public int dwSupport;

			public override string ToString()
			{
				return string.Format("wMid:{0}|wPid:{1}|vDriverVersion:{2}|'szPname:{3}'|dwFormats:{4}|wChannels:{5}|wReserved:{6}|dwSupport:{7}",
					new object[] { wMid, wPid, vDriverVersion, szPname, dwFormats, wChannels, wReserved, dwSupport });
			}
		}

        public static String GetDisplayName(IMoniker mon)
        {
            String displayName;
            mon.GetDisplayName(null, null, out displayName);
            return displayName;
        }

        public static String GetFriendlyName(IMoniker mon)
        {
            object bagObj = null;
            IPropertyBag bag = null;
            IErrorLog errLog = null;
            try
            {
                Guid bagId = typeof(IPropertyBag).GUID;
                mon.BindToStorage(null, null, ref bagId, out bagObj);
                bag = (IPropertyBag)bagObj;
                object val = "";

                int hr = bag.Read("FriendlyName", out val, errLog);
                if (hr != 0)
                    Marshal.ThrowExceptionForHR(hr);
                string ret = val as string;
                if ((ret == null) || (ret.Length < 1))
                    throw new NotImplementedException("Device FriendlyName");
                return ret;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                bag = null;
                if (bagObj != null)
                    Marshal.ReleaseComObject(bagObj); bagObj = null;
            }
        }

        public static bool GetDeviceOfCategory(Guid deviceCategory, out ArrayList deviceList)
        {
            int hr = 0;
            ICreateDevEnum deviceEnum = null;

            IEnumMoniker enumMoniker = null;
            IMoniker[] mon = new IMoniker[1];
            object comObj = null;
            //this.outputPin = this.graphBuilder.
            //DC - output pin
            //this.deviceEnum = (ICreateDevEnum) new SystemDeviceEnum();
            // Guid cat = new Guid(MediaSubType.WAVE);

            deviceList = null;

            try
            {
                Type srvType = Type.GetTypeFromCLSID(Clsid_SystemDeviceEnum);
                if (srvType == null) throw new NotImplementedException("System Device Enumerator");



                comObj = Activator.CreateInstance(srvType);
                deviceEnum = (ICreateDevEnum)comObj;
                Activator.CreateInstance(srvType);

                hr = deviceEnum.CreateClassEnumerator(deviceCategory, out enumMoniker, 0);
                if (hr != 0)
                    throw new NotSupportedException("No devices of the category");

                int count = 0;
                IntPtr f = IntPtr.Zero;

                do
                {
                    hr = enumMoniker.Next(1, mon, f);
                    //hr = enumMon.Next(1, mon, out f);
                    if ((hr != 0) || (mon[0] == null))
                        break;
                    String dname = GetFriendlyName(mon[0]);
                    if (deviceList == null)
                        deviceList = new ArrayList();
                    deviceList.Add(mon[0]);
                    mon[0] = null;
                    count++;
                }
                while (true);
                return true;
            }
            catch (Exception)
            {
                if (deviceList != null)
                {
                    //foreach( IMoniker d in deviceList)
                    //        d = null;
                    deviceList = null;
                }
                return false;
            }
            finally
            {
                deviceEnum = null;
                if (mon[0] != null)
                    Marshal.ReleaseComObject(mon[0]); mon[0] = null;
                if (enumMoniker != null)
                    Marshal.ReleaseComObject(enumMoniker); enumMoniker = null;
                if (comObj != null)
                    Marshal.ReleaseComObject(comObj); comObj = null;
            }

        }

        public static void GetFilterPins(IBaseFilter filter, out int pinCount, out IPin[] pinList)
        {

            // Get a pin enumerator off the filter, max 100 pins, should be enough
            pinCount = 0;
            pinList = new IPin[100];
            IEnumPins pinEnum;
            IntPtr iPtr = new IntPtr();
            int hr = filter.EnumPins(out pinEnum);
            pinEnum.Reset();



            if ((hr == 0) && (pinEnum != null))
            {
                // Loop through each pin
                IPin[] pins = new IPin[1];
                //int f;

                do
                {
                    // Get the next pin
                    hr = pinEnum.Next(1, pins, iPtr);
                    if ((hr == 0) && (pins[0] != null))
                    {
                        pinList[pinCount] = pins[0];
                        pinCount++;
                        // Get the pin it is connected to
                        IPin pinTo = null;

                        pins[0].ConnectedTo(out pinTo);
                        if (pinTo != null)
                        {
                            // Is this an input pin?
                            PinInfo info = new PinInfo();
                            hr = pinTo.QueryPinInfo(out info);
                            if ((hr == 0) && (info.dir == (PinDirection.Input)))
                            {
                                // Recurse down this branch
                                //removeDownstream( info.filter, true );

                                // Disconnect 
                                //graphBuilder.Disconnect( pinTo );
                                //graphBuilder.Disconnect( pins[0] );

                                // Remove this filter
                                // but don't remove the video or audio compressors
                                //if ( ( info.filter != videoCompressorFilter ) &&
                                //	 ( info.filter != audioCompressorFilter ) )
                                //	graphBuilder.RemoveFilter( info.filter );
                            }
                            Marshal.ReleaseComObject(info.filter);
                            Marshal.ReleaseComObject(pinTo);
                        }
                        //Marshal.ReleaseComObject( pins[0] );
                    }
                }
                while (hr == 0 && pinCount < 100);
                Marshal.ReleaseComObject(pinEnum); pinEnum = null;
            }

        }

        public static bool ValidateDevice(String ioDeviceName)
        {
            IBaseFilter ioFilter;
            IGraphBuilder graphBuilder;
            int hr;
            try
            {
                graphBuilder = (IGraphBuilder)new FilterGraph();
                ioFilter = (IBaseFilter)Marshal.BindToMoniker(ioDeviceName);
                hr = graphBuilder.AddFilter(ioFilter, "TestCond Input Output Renderer");
                DsError.ThrowExceptionForHR(hr);

                graphBuilder.Abort();
                graphBuilder = null;
                return true;
            }
            catch (Exception ex)
            {
                MLog.Log(null,"Validating device " + ioDeviceName + " exception " + ex.Message);
                return false;
            }
        }

		public static List<string> SystemDeviceNameList
		{
			get
			{
				List<String> systemOutputDeviceNames = new List<string>();
				ArrayList systemOutputDeviceList = null;
				DShowUtility.GetDeviceOfCategory(DShowUtility.Clsid_AudioOutRender, out systemOutputDeviceList);

				List<WAVEOUTCAPS> m_waveoutdeviceList = GetDevCapsPlayback();
				String deviceName;

				foreach (Object m in systemOutputDeviceList)
				{
					deviceName = DShowUtility.GetDisplayName((IMoniker)m);
					systemOutputDeviceNames.Add(deviceName);
				}

				return systemOutputDeviceNames;
			}
		}

		public static List<WAVEOUTCAPS> GetDevCapsPlayback()
		{
			uint waveOutDevicesCount = waveOutGetNumDevs();
			if (waveOutDevicesCount > 0)
			{
				List<WAVEOUTCAPS> list = new List<WAVEOUTCAPS>();
				for (int uDeviceID = 0; uDeviceID < waveOutDevicesCount; uDeviceID++)
				{
					WAVEOUTCAPS waveOutCaps = new WAVEOUTCAPS();
					waveOutGetDevCaps(uDeviceID, ref waveOutCaps, Marshal.SizeOf(typeof(WAVEOUTCAPS)));
					//MLog.Log(null,waveOutCaps.ToString());
					list.Add(waveOutCaps);
				}
				return list;
			}
			else
			{
				return null;
			}
		}

		

    }
}

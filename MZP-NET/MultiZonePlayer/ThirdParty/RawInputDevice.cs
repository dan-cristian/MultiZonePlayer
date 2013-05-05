using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace MultiZonePlayer
{
    /// <summary>
    /// Handles raw input from keyboard devices.
    /// </summary>
    public sealed class RawInputDevice
    {
        #region const definitions

        // The following constants are defined in Windows.h

        private const int RIDEV_INPUTSINK   = 0x00000100;
        private const int RID_INPUT         = 0x10000003;

        private const int FAPPCOMMAND_MASK  = 0xF000;
        private const int FAPPCOMMAND_MOUSE = 0x8000;
        private const int FAPPCOMMAND_OEM   = 0x1000;

        private const int RIM_TYPEMOUSE     = 0;
        private const int RIM_TYPEKEYBOARD  = 1;
        private const int RIM_TYPEHID       = 2;

        private const int RIM_INPUT = 0;
        private const int RIM_INPUTSINK = 1;

        private const int RI_KEY_BREAK = 1;//The key is up.
        private const int RI_KEY_E0 = 2;//This is the left version of the key.
        private const int RI_KEY_E1 = 4;//This is the right version of the key.
        private const int RI_KEY_MAKE = 0;//The key is down.

        private const string MODIFIER_CONTROL = "controlkey";
        private const string MODIFIER_SHIFT     = "shiftkey";
        private const string MODIFIER_ALT = "menu";
        private const string MODIFIER_WIN = "lwin";

        private const int RIDI_DEVICENAME   = 0x20000007;

        private const int WM_NULL           = 0x0000;
        private const int WM_KEYDOWN	    = 0x0100;
        private const int WM_SYSKEYDOWN     = 0x0104;
        private const int WM_KEYUP          = 0x0101;
        private const int WM_SYSKEYUP       = 0x0105;

		private const int WM_INPUT		    = 0x00FF;
        private const int VK_OEM_CLEAR      = 0xFE;
        private const int VK_LAST_KEY       = VK_OEM_CLEAR; // this is a made up value used as a sentinel
       
        #endregion const definitions

        #region structs & enums
        
        /// <summary>
        /// An enum representing the different types of input devices.
        /// </summary>
        public enum DeviceType
        {
            Key,
            Mouse,
            OEM
        }

        /// <summary>
        /// Class encapsulating the information about a
        /// keyboard event, including the device it
        /// originated with and what key was pressed
        /// </summary>
        public class DeviceInfo
        {
            public string deviceName = "";
            public string deviceType = "";
            public IntPtr deviceHandle;
            public string Name = "";
            public string source = "";
            public ushort key;
            public string vKey = "";
            public bool isKeyDownWinMessage = false;
            public bool isKeyUpWinMessage = false;
            public bool isAppInForeground;
            public bool isLeftKey = false;
            public bool isRightKey = false;
            public bool isLeftButton = false;
            public bool isRightButton = false;
            public bool isMiddleButton = false;
            public uint extraInformation;
            public ushort flags;
            public uint message;
            public uint reserved;
            
        }

        #region Windows.h structure declarations
        
        // The following structures are defined in Windows.h

        [StructLayout(LayoutKind.Sequential)]
        internal struct RAWINPUTDEVICELIST
        {
            public IntPtr hDevice;
            [MarshalAs(UnmanagedType.U4)]
            public int dwType;
        }

        
        [StructLayout(LayoutKind.Explicit)]
        internal struct RAWINPUT //x86 only
        {
            [FieldOffset(0)]
            public RAWINPUTHEADER header;
            [FieldOffset(16)]
            public RAWMOUSE mouse;
            [FieldOffset(16)]
            public RAWKEYBOARD keyboard;
            [FieldOffset(16)]
            public RAWHID hid;
        }
        /* ORIG CODE x86 only
        [StructLayout(LayoutKind.Sequential)]
        internal struct RAWINPUTHEADER
        {
            [MarshalAs(UnmanagedType.U4)]
            public int dwType;
            [MarshalAs(UnmanagedType.U4)]
            public int dwSize;
            public IntPtr hDevice;
            [MarshalAs(UnmanagedType.U4)]
            public int wParam;
        }
        */


        //const int RAWSIZE = System.Runtime.InteropServices.Marshal.SizeOf(typeof(RAWINPUTHEADER));

        [StructLayout(LayoutKind.Explicit)]
        internal struct RAWINPUTx64
        {
            [FieldOffset(0)]
            public RAWINPUTHEADER header;
            [FieldOffset(24)]
            public RAWMOUSE mouse;
            [FieldOffset(24)]
            public RAWKEYBOARD keyboard;
            [FieldOffset(24)]
            public RAWHID hid;
        }
        
        [StructLayout(LayoutKind.Sequential)]
        internal struct RAWINPUTHEADER
        {
            [MarshalAs(UnmanagedType.U4)]
            public int dwType;
            [MarshalAs(UnmanagedType.U4)]
            public int dwSize;
            public IntPtr hDevice;
            public IntPtr wParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RAWHID
        {
            [MarshalAs(UnmanagedType.U4)]
            public int dwSizHid;
            [MarshalAs(UnmanagedType.U4)]
            public int dwCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct BUTTONSSTR
        {
            [MarshalAs(UnmanagedType.U2)]
            public ushort usButtonFlags;
            [MarshalAs(UnmanagedType.U2)]
            public ushort usButtonData;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct RAWMOUSE
        {
            [MarshalAs(UnmanagedType.U2)]
            [FieldOffset(0)] 
            public ushort usFlags;
            [MarshalAs(UnmanagedType.U4)]
            [FieldOffset(4)] 
            public uint ulButtons; 
            [FieldOffset(4)] 
            public BUTTONSSTR buttonsStr;
            [MarshalAs(UnmanagedType.U4)][FieldOffset(8)] 
            public uint ulRawButtons;
            [FieldOffset(12)]
            public int lLastX;
            [FieldOffset(16)]
            public int lLastY;
            [MarshalAs(UnmanagedType.U4)][FieldOffset(20)]
            public uint ulExtraInformation;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RAWKEYBOARD
        {
            [MarshalAs(UnmanagedType.U2)]
            public ushort MakeCode;
            [MarshalAs(UnmanagedType.U2)]
            public ushort Flags;
            [MarshalAs(UnmanagedType.U2)]
            public ushort Reserved;
            [MarshalAs(UnmanagedType.U2)]
            public ushort VKey;
            [MarshalAs(UnmanagedType.U4)]
            public uint Message;
            [MarshalAs(UnmanagedType.U4)]
            public uint ExtraInformation;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RAWINPUTDEVICE
        {
            [MarshalAs(UnmanagedType.U2)]
            public ushort usUsagePage;
            [MarshalAs(UnmanagedType.U2)]
            public ushort usUsage;
            [MarshalAs(UnmanagedType.U4)]
            public int dwFlags;
            public IntPtr hwndTarget;
        }
        #endregion Windows.h structure declarations


        #endregion structs & enums
        
        #region DllImports
        
        [DllImport("User32.dll")]
        extern static uint GetRawInputDeviceList(IntPtr pRawInputDeviceList, ref uint uiNumDevices, uint cbSize);
        
        [DllImport("User32.dll")]
        extern static uint GetRawInputDeviceInfo(IntPtr hDevice, uint uiCommand, IntPtr pData, ref uint pcbSize);
        
        [DllImport("User32.dll")]
        extern static bool RegisterRawInputDevices(RAWINPUTDEVICE[] pRawInputDevice, uint uiNumDevices, uint cbSize);

        [DllImport("User32.dll")]
        extern static uint GetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader);

        #endregion DllImports

        
        #region Variables and event handling
        
        /// <summary>
        /// List of keyboard devices. Key: the device handle
        /// Value: the device info class
        /// </summary>
        private static Hashtable deviceList = new Hashtable();

        //stores modifiers
        private Hashtable modifierOnState = new Hashtable();

        /// <summary>
        /// The delegate to handle KeyPressed events.
        /// </summary>
        /// <param name="sender">The object sending the event.</param>
        /// <param name="e">A set of KeyControlEventArgs information about the key that was pressed and the device it was on.</param>
        public delegate void DeviceEventHandler(object sender, ref KeyControlEventArgs e);

        /// <summary>
        /// The event raised when InputDevice detects that a key was pressed.
        /// </summary>
        public event DeviceEventHandler KeyPressed;

        /// <summary>
        /// Arguments provided by the handler for the KeyPressed
        /// event.
        /// </summary>
        public class KeyControlEventArgs : EventArgs
        {
            private DeviceInfo  m_deviceInfo;
            private DeviceType  m_device;
            public bool messageProcessedbyApp = false;
           
            public KeyControlEventArgs(DeviceInfo dInfo, DeviceType device )
            {
                m_deviceInfo = dInfo;
                m_device = device;
            }
            
            public KeyControlEventArgs()
            {
            }

            public DeviceInfo Keyboard
            {
                get { return m_deviceInfo; }
                set { m_deviceInfo = value; }
            }

            public DeviceType Device
            {
                get { return m_device; }
                set { m_device = value; }
            }
        }

        #endregion Variables and event handling

        #region InputDevice( IntPtr hwnd )

        /// <summary>
        /// InputDevice constructor; registers the raw input devices
        /// for the calling window.
        /// </summary>
        /// <param name="hwnd">Handle of the window listening for key presses</param>
        public RawInputDevice( IntPtr hwnd )
        {
            MLog.Log(this, "Registering RawInputDevice on handle "+hwnd + ", APP runs on 64 bit="+(IntPtr.Size==8));
            //Create an array of all the raw input devices we want to 
            //listen to. In this case, only keyboard devices.
            //RIDEV_INPUTSINK determines that the window will continue
            //to receive messages even when it doesn't have the focus.
            RAWINPUTDEVICE[] rid_keyboard = new RAWINPUTDEVICE[1];

            rid_keyboard[0].usUsagePage = 0x01;
            rid_keyboard[0].usUsage = 0x06;
            rid_keyboard[0].dwFlags = RIDEV_INPUTSINK;
            rid_keyboard[0].hwndTarget = hwnd;

            if (!RegisterRawInputDevices(rid_keyboard, (uint)rid_keyboard.Length, (uint)Marshal.SizeOf(rid_keyboard[0])))
            {
                throw new ApplicationException( "Failed to register raw input keyboard device(s)." );
            }

            RAWINPUTDEVICE[] rid_keypad = new RAWINPUTDEVICE[1];

            rid_keypad[0].usUsagePage = 0x01;
            rid_keypad[0].usUsage = 0x07;
            rid_keypad[0].dwFlags = RIDEV_INPUTSINK;
            rid_keypad[0].hwndTarget = hwnd;

            if (!RegisterRawInputDevices(rid_keypad, (uint)rid_keypad.Length, (uint)Marshal.SizeOf(rid_keypad[0])))
            {
                throw new ApplicationException("Failed to register raw input keypad device(s).");
            }

            //Create an array of all the raw input devices we want to 
            //listen to. In this case, only mouse devices.
            //RIDEV_INPUTSINK determines that the window will continue
            //to receive messages even when it doesn't have the focus.
            
            /*RAWINPUTDEVICE[] rid_mouse = new RAWINPUTDEVICE[1];

            rid_mouse[0].usUsagePage = 0x01;
            rid_mouse[0].usUsage = 0x02;
            rid_mouse[0].dwFlags = RIDEV_INPUTSINK;
            rid_mouse[0].hwndTarget = hwnd;

            if (!RegisterRawInputDevices(rid_mouse, (uint)rid_mouse.Length, (uint)Marshal.SizeOf(rid_mouse[0])))
            {
                throw new ApplicationException("Failed to register raw input mouse device(s).");
            }*/
            

            EnumerateDevices();
        }

        #endregion InputDevice( IntPtr hwnd )

        #region ReadReg( string item, ref bool isKeyboard )
        
        /// <summary>
        /// Reads the Registry to retrieve a friendly description
        /// of the device, and determine whether it is a keyboard.
        /// </summary>
        /// <param name="item">The device name to search for, as provided by GetRawInputDeviceInfo.</param>
        /// <param name="isKeyboard">Determines whether the device's class is "Keyboard".</param>
        /// <returns>The device description stored in the Registry entry's DeviceDesc value.</returns>
        private static string ReadReg( string item, ref bool isKeyboard )
        {
            try
            {

                // Example Device Identification string
                // @"\??\ACPI#PNP0303#3&13c0b0c5&0#{884b96c3-56ef-11d1-bc8c-00a0c91405dd}";

                // remove the \??\
                item = item.Substring(4);

                string[] split = item.Split('#');

                string id_01 = split[0];    // ACPI (Class code)
                string id_02 = split[1];    // PNP0303 (SubClass code)
                string id_03 = split[2];    // 3&13c0b0c5&0 (Protocol code)
                //The final part is the class GUID and is not needed here

                //Open the appropriate key as read-only so no permissions
                //are needed.
                RegistryKey OurKey = Registry.LocalMachine;

                string findme = string.Format(@"System\CurrentControlSet\Enum\{0}\{1}\{2}", id_01, id_02, id_03);

                OurKey = OurKey.OpenSubKey(findme, false);

                //Retrieve the desired information and set isKeyboard
                string deviceDesc = (string)OurKey.GetValue("DeviceDesc");
                string deviceFriendlyName = (string)OurKey.GetValue("FriendlyName");
                string deviceClass = (string)OurKey.GetValue("Class");

                String[] deviceAtoms = deviceDesc.Split(';');
                String result;

                if (deviceFriendlyName != null)
                    result = deviceFriendlyName;
                else

                    if (deviceAtoms.Length == 2)
                        result = deviceAtoms[1];
                    else
                        result = deviceDesc;
    
                if (deviceClass.ToUpper().Equals("KEYBOARD"))
                {
                    isKeyboard = true;
                }
                else
                {
                    isKeyboard = false;
                }
                return result;
            }
            catch (Exception ex)
            {
                MLog.Log(ex, null, "Error reading rawinput item:"+item);
                return "n/a";
            }
            
        }

		

        #endregion ReadReg( string item, ref bool isKeyboard )

        #region int EnumerateDevices()

        /// <summary>
        /// Iterates through the list provided by GetRawInputDeviceList,
        /// counting keyboard devices and adding them to deviceList.
        /// </summary>
        /// <returns>The number of keyboard devices found.</returns>
        public static int EnumerateDevices()
        {
            MLog.Log(null, "Enumerating RawInput Devices");
            int NumberOfDevices = 0;
            uint deviceCount = 0;
            int dwSize = ( Marshal.SizeOf( typeof( RAWINPUTDEVICELIST )));

            // Get the number of raw input devices in the list,
            // then allocate sufficient memory and get the entire list
            if( GetRawInputDeviceList( IntPtr.Zero, ref deviceCount, (uint)dwSize ) == 0 )
            {
                IntPtr pRawInputDeviceList = Marshal.AllocHGlobal((int)(dwSize * deviceCount));
                GetRawInputDeviceList(pRawInputDeviceList, ref deviceCount, (uint)dwSize);

                MLog.Log(null, "Enumerating RawInput Devices found potential devicecount="+deviceCount);

                // Iterate through the list, discarding undesired items
                // and retrieving further information on keyboard devices
                for (int i = 0; i < deviceCount; i++)
                {
                    DeviceInfo dInfo;
                    string deviceName;
                    uint pcbSize = 0;

                    RAWINPUTDEVICELIST rid = (RAWINPUTDEVICELIST)Marshal.PtrToStructure(
                                               new IntPtr((pRawInputDeviceList.ToInt32() + (dwSize * i))),
                                               typeof(RAWINPUTDEVICELIST));

                    GetRawInputDeviceInfo(rid.hDevice, RIDI_DEVICENAME, IntPtr.Zero, ref pcbSize);

                    if (pcbSize > 0)
                    {
                        IntPtr pData = Marshal.AllocHGlobal((int)pcbSize);
                        GetRawInputDeviceInfo(rid.hDevice, RIDI_DEVICENAME, pData, ref pcbSize);
                        deviceName = (string)Marshal.PtrToStringAnsi(pData);

						

                        // Drop the "root" keyboard and mouse devices used for Terminal 
                        // Services and the Remote Desktop
                        if (deviceName.ToUpper().Contains("ROOT"))
                        {
                            //continue;
                        }

                        // If the device is identified in the list as a keyboard or 
                        // HID device, create a DeviceInfo object to store information 
                        // about it
                        if (rid.dwType == RIM_TYPEKEYBOARD || rid.dwType == RIM_TYPEHID || rid.dwType == RIM_TYPEMOUSE)
                        {
                            dInfo = new DeviceInfo();

                            dInfo.deviceName = (string)Marshal.PtrToStringAnsi(pData);
                            dInfo.deviceHandle = rid.hDevice;
                            dInfo.deviceType = GetDeviceType(rid.dwType);

                            // Check the Registry to see whether this is actually a 
                            // keyboard, and to retrieve a more friendly description.
                            bool IsKeyboardDevice = false;
                            string DeviceDesc = ReadReg(deviceName, ref IsKeyboardDevice);
                            dInfo.Name = DeviceDesc;

                            // If it is a keyboard and it isn't already in the list,
                            // add it to the deviceList hashtable and increase the
                            // NumberOfDevices count
                            if (!deviceList.Contains(rid.hDevice))// && IsKeyboardDevice)
                            {
                                NumberOfDevices++;
                                deviceList.Add(rid.hDevice, dInfo);
                                MLog.Log(null, "Added rawinput device name=" +dInfo.deviceName 
                                    + " desc="+ dInfo.Name + " type=" + dInfo.deviceType);
                            }
                        }
                        Marshal.FreeHGlobal(pData);
                    }
                }

                Marshal.FreeHGlobal(pRawInputDeviceList);
                MLog.Log(null, "Enumerating RawInput Devices completed with qualified devicecount=" + NumberOfDevices);
                return NumberOfDevices;
            }
            else
            {
                throw new ApplicationException( "An error occurred while retrieving the list of devices." );
            }

        }

        #endregion EnumerateDevices()
        
        #region ProcessInputCommand( Message message )
        
        /// <summary>
        /// Processes WM_INPUT messages to retrieve information about any
        /// keyboard events that occur. 
        /// DC: Returns false is message was not processed, true if message was handled (further processing by windows is canceled?).
        /// </summary>
        /// <param name="message">The WM_INPUT message to process.</param>
        public bool ProcessInputCommand( Message message )
        {
            uint dwSize = 0;
            bool result = false;
            // First call to GetRawInputData sets the value of dwSize,
            // which can then be used to allocate the appropriate amount of memory,
            // storing the pointer in "buffer".
            
            GetRawInputData( message.LParam, 
                             RID_INPUT, IntPtr.Zero, 
                             ref dwSize, 
                             (uint)Marshal.SizeOf( typeof( RAWINPUTHEADER )));
            
            
            IntPtr buffer = Marshal.AllocHGlobal( (int)dwSize );
            try
            {
                // Check that buffer points to something, and if so,
                // call GetRawInputData again to fill the allocated memory
                // with information about the input
                if (buffer != IntPtr.Zero &&
                    GetRawInputData(message.LParam,
                                     RID_INPUT,
                                     buffer,
                                     ref dwSize,
                                     (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER))) == dwSize)
                {
                    // Store the message information in "raw", then check
                    // that the input comes from a keyboard device before
                    // processing it to raise an appropriate KeyPressed event.

                    RAWINPUT rawx86 = (RAWINPUT)Marshal.PtrToStructure(buffer, typeof(RAWINPUT));
                    
                    #region keyboard
                    if (rawx86.header.dwType == RIM_TYPEKEYBOARD)
                    {
                        // Retrieve information about the device and the
                        // key that was pressed.
                        DeviceInfo dInfo = null;
                        Keys myKey;
                        dInfo = (DeviceInfo)deviceList[rawx86.header.hDevice];
                        if (dInfo == null)
                        {
                            dInfo = new DeviceInfo();
                        }

                        if (IntPtr.Size == 8)
                        {
                            RAWINPUTx64 rawx64 = (RAWINPUTx64)Marshal.PtrToStructure(buffer, typeof(RAWINPUTx64));
                            dInfo.key = rawx64.keyboard.VKey;
                            dInfo.message = rawx64.keyboard.Message;
                            dInfo.flags = rawx64.keyboard.Flags;
                            dInfo.extraInformation = rawx64.keyboard.ExtraInformation;
                            dInfo.reserved = rawx64.keyboard.Reserved;
                        }
                        else
                        {
                            dInfo.key = rawx86.keyboard.VKey;
                            dInfo.message = rawx86.keyboard.Message;
                            dInfo.flags = rawx86.keyboard.Flags;
                            dInfo.extraInformation = rawx86.keyboard.ExtraInformation;
                            dInfo.reserved = rawx86.keyboard.Reserved;
                        }

                        dInfo.isKeyDownWinMessage = (dInfo.message == WM_KEYDOWN) || (dInfo.message == WM_SYSKEYDOWN);
                        dInfo.isKeyUpWinMessage = (dInfo.message == WM_KEYUP) || (dInfo.message == WM_SYSKEYUP);
                        
                        // On most keyboards, "extended" keys such as the arrow or 
                        // page keys return two codes - the key's own code, and an
                        // "extended key" flag, which translates to 255. This flag
                        // isn't useful to us, so it can be disregarded.
                        /*if (key > VK_LAST_KEY)
                        {
                            return;
                        }*/

                        if (Enum.GetName(typeof(Keys), dInfo.key) != null)
                        {
                            myKey = (Keys)Enum.Parse(typeof(Keys), Enum.GetName(typeof(Keys), dInfo.key));
                        }
                        else
                            myKey = Keys.NoName;

                        //MLog.Log2(myKey.ToString());
                        dInfo.vKey = myKey.ToString().ToLower();
                        //dInfo.isAppInForeground = ( == WM_INPUT);
                        switch (dInfo.flags)
                        {
                            case RI_KEY_E0:
                                dInfo.isLeftKey = true;
                                break;
                            case RI_KEY_E1:
                                dInfo.isRightKey = true;
                                break;
                        }
                        
                        //update modifiers
                        if (dInfo.vKey == MODIFIER_CONTROL || dInfo.vKey == MODIFIER_SHIFT 
                            || dInfo.vKey == MODIFIER_ALT )//|| dInfo.vKey == MODIFIER_WIN)
                        {
                            if (dInfo.isKeyDownWinMessage && (!modifierOnState.ContainsKey(dInfo.vKey)))
                            {
                                modifierOnState.Add(dInfo.vKey, dInfo.isKeyDownWinMessage);
                            }
                            if (dInfo.isKeyUpWinMessage)
                                modifierOnState.Remove(dInfo.vKey);
                        }
                        else
                        {
                            //if (modifierOnState.ContainsKey(MODIFIER_WIN))
                            //    dInfo.vKey = "Win, " + dInfo.vKey;

                            if (modifierOnState.ContainsKey(MODIFIER_ALT))
                                dInfo.vKey = "alt, " + dInfo.vKey;

                            if (modifierOnState.ContainsKey(MODIFIER_SHIFT))
                                dInfo.vKey = "shift, " + dInfo.vKey;

                            if (modifierOnState.ContainsKey(MODIFIER_CONTROL))
                                dInfo.vKey = "control, " + dInfo.vKey;

                            MLog.LogRawInput(String.Format("KBD {0} vkey={1} \t msg={2} \t flags={3} \t makecode={4} \t extra={5} device={6} Down={7} Up={8}\r", 
								DateTime.Now.ToString("hh:mm:ss-ff"),
                                        dInfo.vKey, dInfo.message, dInfo.flags, "", dInfo.extraInformation, rawx86.header.hDevice,
										dInfo.isKeyDownWinMessage,dInfo.isKeyUpWinMessage));

                            // If the key that was pressed is valid and there
                            // was no problem retrieving information on the device,
                            // raise the KeyPressed event.
                            if (KeyPressed != null && dInfo != null)
                            {
                                KeyControlEventArgs keyArgs = new KeyControlEventArgs(dInfo, GetDevice(message.LParam.ToInt32()));
                                //if (dInfo.isKeyDownWinMessage) //normally only key down, but all states required for IO key state
                                    KeyPressed(this, ref keyArgs);
                                result = keyArgs.messageProcessedbyApp;
                            }
                            else
                            {
                                MLog.Log(null,"Received Unknown Key: {0}. Possibly an unknown device " + dInfo.key);
                            }
                        }
                    }
                    #endregion

                    #region mouse
                    if (rawx86.header.dwType == RIM_TYPEMOUSE)
                    {
                        /*BUTTONSSTR bt = raw.mouse.buttonsStr;
                        DeviceInfo dInfo = null;

                        dInfo = (DeviceInfo)deviceList[raw.header.hDevice];
                        if (dInfo == null)
                        {
                            dInfo = new DeviceInfo();
                            MLog.Log2("NO MOUSE DEV");
                        }
                         * */
                        MLog.LogRawInput(String.Format("MOUSE btnstr={0} rawbtns={1} x={2} y={3} flags={4}\r",
                            rawx86.mouse.buttonsStr, rawx86.mouse.ulRawButtons, rawx86.mouse.lLastX, rawx86.mouse.lLastY, rawx86.mouse.usFlags));
                    }
                    #endregion

                    #region HID
                    if (rawx86.header.dwType == RIM_TYPEHID)
                    {
                        RAWHID hid = rawx86.hid;
                        MLog.LogRawInput(String.Format("HID count={0} device={1}\r", rawx86.hid.dwCount, rawx86.header.hDevice));
                    }
                    #endregion
                }
            }
            finally
            {
                Marshal.FreeHGlobal( buffer );
            }
            return result;
        }

        #endregion ProcessInputCommand( Message message )

        #region DeviceType GetDevice( int param )

        /// <summary>
        /// Determines what type of device triggered a WM_INPUT message.
        /// (Used in the ProcessInputCommand method).
        /// </summary>
        /// <param name="param">The LParam from a WM_INPUT message.</param>
        /// <returns>A DeviceType enum value.</returns>
        private DeviceType GetDevice(int param)
        {
            DeviceType deviceType;

            switch( (int)(((ushort)(param >> 16)) & FAPPCOMMAND_MASK ))
            {
                case FAPPCOMMAND_OEM:
                    deviceType = DeviceType.OEM;
                    break;
                case FAPPCOMMAND_MOUSE:
                    deviceType = DeviceType.Mouse;
                    break;
                default:
                    deviceType = DeviceType.Key;
                    break;
            }

            return deviceType;
        }

        #endregion DeviceType GetDevice( int param )

        public static Hashtable GetDeviceList()
        {
            return deviceList;
        }
        #region ProcessMessage( Message message )

        /// <summary>
        /// Filters Windows messages for WM_INPUT messages and calls
        /// ProcessInputCommand if necessary.
        /// </summary>
        /// <param name="message">The Windows message.</param>
        public void ProcessMessage( Message message )
		{
			switch( message.Msg )
			{
				case WM_INPUT:
		        {
                    if (ProcessInputCommand(message))
                        message.Msg = WM_NULL;
		        }
				break;
			}
        }

        #endregion ProcessMessage( Message message )

        #region GetDeviceType( int device )
        
        /// <summary>
        /// Converts a RAWINPUTDEVICELIST dwType value to a string
        /// describing the device type.
        /// </summary>
        /// <param name="device">A dwType value (RIM_TYPEMOUSE, 
        /// RIM_TYPEKEYBOARD or RIM_TYPEHID).</param>
        /// <returns>A string representation of the input value.</returns>
        private static string GetDeviceType( int device )
        {
            string deviceType;
            switch( device ) 
            {
                case RIM_TYPEMOUSE: deviceType    = "MOUSE";    break;
                case RIM_TYPEKEYBOARD: deviceType = "KEYBOARD"; break;
                case RIM_TYPEHID: deviceType      = "HID";      break;
                default: deviceType               = "UNKNOWN";  break;
            }
            return deviceType;
        }

        #endregion GetDeviceType( int device )

    }
}

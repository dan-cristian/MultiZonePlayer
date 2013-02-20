namespace USB_RC2
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal class DeviceManagement
    {
        private const string ModuleName = "DeviceManagement";

        internal bool DeviceNameMatch(Message m, string mydevicePathName)
        {
            try
            {
                DeviceManagementApiDeclarations.DEV_BROADCAST_DEVICEINTERFACE_1 structure = new DeviceManagementApiDeclarations.DEV_BROADCAST_DEVICEINTERFACE_1();
                DeviceManagementApiDeclarations.DEV_BROADCAST_HDR dev_broadcast_hdr = new DeviceManagementApiDeclarations.DEV_BROADCAST_HDR();
                Marshal.PtrToStructure(m.LParam, dev_broadcast_hdr);
                if (dev_broadcast_hdr.dbch_devicetype == 5)
                {
                    int length = Convert.ToInt32((int) ((dev_broadcast_hdr.dbch_size - 0x1c) / 2));
                    structure.dbcc_name = new char[length + 1];
                    Marshal.PtrToStructure(m.LParam, structure);
                    string strA = new string(structure.dbcc_name, 0, length);
                    Debug.WriteLine("Device Name = " + strA);
                    Debug.WriteLine("");
                    if (string.Compare(strA, mydevicePathName, true) == 0)
                    {
                        return true;
                    }
                }
            }
            catch (Exception exception)
            {
                HandleException("DeviceManagement:" + MethodBase.GetCurrentMethod(), exception);
            }
            return false;
        }

        internal bool FindDeviceFromGuid(Guid myGuid, ref string[] devicePathName)
        {
            bool flag = false;
            bool flag2 = false;
            int requiredSize = 0;
            int memberIndex = 0;
            DeviceManagementApiDeclarations.SP_DEVICE_INTERFACE_DETAIL_DATA structure 
				= new DeviceManagementApiDeclarations.SP_DEVICE_INTERFACE_DETAIL_DATA();
            DeviceManagementApiDeclarations.SP_DEVICE_INTERFACE_DATA deviceInterfaceData 
				= new DeviceManagementApiDeclarations.SP_DEVICE_INTERFACE_DATA();
            int num3 = 0;
            bool flag3 = false;
            try
            {
                IntPtr deviceInfoSet = DeviceManagementApiDeclarations.SetupDiGetClassDevs(
					ref myGuid, null, IntPtr.Zero, 0x12);
                flag = false;
                memberIndex = 0;
                do
                {
					deviceInterfaceData.cbSize = Marshal.SizeOf(deviceInterfaceData);//0x1c;
                    if (DeviceManagementApiDeclarations.SetupDiEnumDeviceInterfaces(
						deviceInfoSet, 0, ref myGuid, memberIndex, ref deviceInterfaceData) == IntPtr.Zero)
                    {
                        flag2 = true;
                    }
                    else
                    {
                        Debug.WriteLine("  DeviceInfoSet for device #" + memberIndex.ToString() + ": ");
                        Debug.WriteLine("  cbSize = " + deviceInterfaceData.cbSize.ToString());
                        Debug.WriteLine("  InterfaceclassGuid = " + deviceInterfaceData.InterfaceClassGuid.ToString());
                        Debug.WriteLine("  Flags = " + deviceInterfaceData.Flags.ToString("x"));
                        flag3 = DeviceManagementApiDeclarations.SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref deviceInterfaceData, IntPtr.Zero, 0, ref requiredSize, IntPtr.Zero);
                        structure.cbSize = Marshal.SizeOf(structure);
                        IntPtr ptr = Marshal.AllocHGlobal(requiredSize);
                        Marshal.WriteInt32(ptr, IntPtr.Size + Marshal.SystemDefaultCharSize);
                        Debug.WriteLine("cbsize = " + structure.cbSize);
                        flag3 = DeviceManagementApiDeclarations.SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref deviceInterfaceData, ptr, requiredSize, ref requiredSize, IntPtr.Zero);
                        IntPtr ptr3 = new IntPtr(ptr.ToInt32() + IntPtr.Size);
                        devicePathName[memberIndex] = Marshal.PtrToStringAuto(ptr3);
                        Debug.WriteLine("Device Path = " + devicePathName[memberIndex]);
                        Debug.WriteLine("Device Path Length = " + devicePathName[memberIndex].Length);
                        Marshal.FreeHGlobal(ptr);
                        flag = true;
                    }
                    memberIndex++;
                }
                while (!flag2);
                Debug.WriteLine("Number of HIDs found = " + ((memberIndex - 1)).ToString());
                num3 = DeviceManagementApiDeclarations.SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }
            catch (Exception exception)
            {
                HandleException("DeviceManagement:" + MethodBase.GetCurrentMethod(), exception);
            }
            return flag;
        }

        public static void HandleException(string moduleName, Exception e)
        {
            try
            {
                string text = "Exception: " + e.Message + Environment.NewLine + "Module: " + moduleName + Environment.NewLine + "Method: " + e.TargetSite.Name;
                string caption = "Unexpected Exception";
                MessageBox.Show(text, caption, MessageBoxButtons.OK);
                Debug.Write(text);
            }
            finally
            {
            }
        }

        internal bool RegisterForDeviceNotifications(string devicePathName, IntPtr formHandle, Guid classGuid, ref IntPtr deviceNotificationHandle)
        {
            DeviceManagementApiDeclarations.DEV_BROADCAST_DEVICEINTERFACE structure = new DeviceManagementApiDeclarations.DEV_BROADCAST_DEVICEINTERFACE();
            try
            {
                int cb = Marshal.SizeOf(structure);
                structure.dbcc_size = cb;
                structure.dbcc_devicetype = 5;
                structure.dbcc_reserved = 0;
                structure.dbcc_classguid = classGuid;
                IntPtr ptr = Marshal.AllocHGlobal(cb);
                Marshal.StructureToPtr(structure, ptr, true);
                deviceNotificationHandle = DeviceManagementApiDeclarations.RegisterDeviceNotification(formHandle, ptr, 0);
                Marshal.PtrToStructure(ptr, structure);
                Marshal.FreeHGlobal(ptr);
                if (deviceNotificationHandle.ToInt32() == IntPtr.Zero.ToInt32())
                {
                    Debug.WriteLine("RegisterDeviceNotification error");
                    return false;
                }
            }
            catch (Exception exception)
            {
                HandleException("DeviceManagement:" + MethodBase.GetCurrentMethod(), exception);
            }
            return true;
        }

        internal void StopReceivingDeviceNotifications(IntPtr deviceNotificationHandle)
        {
            try
            {
                DeviceManagementApiDeclarations.UnregisterDeviceNotification(deviceNotificationHandle);
            }
            catch (Exception exception)
            {
                HandleException("DeviceManagement:" + MethodBase.GetCurrentMethod(), exception);
            }
        }
    }
}


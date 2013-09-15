namespace USB_RC2
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal class Hid
    {
        internal HidApiDeclarations.HIDP_CAPS Capabilities;
        internal HidApiDeclarations.HIDD_ATTRIBUTES DeviceAttributes;
        private const string ModuleName = "Hid";

        internal bool FlushQueue(int hidHandle)
        {
            bool flag = false;
            try
            {
                flag = HidApiDeclarations.HidD_FlushQueue(hidHandle);
            }
            catch (Exception exception)
            {
                HandleException("Hid:" + MethodBase.GetCurrentMethod(), exception);
            }
            return flag;
        }

        internal HidApiDeclarations.HIDP_CAPS GetDeviceCapabilities(int hidHandle)
        {
            byte[] inArray = new byte[30];
            IntPtr preparsedData = new IntPtr();
            bool flag = false;
            byte[] buffer2 = new byte[0x400];
            try
            {
                flag = HidApiDeclarations.HidD_GetPreparsedData(hidHandle, ref preparsedData);
                string str = Convert.ToBase64String(inArray);
                if (HidApiDeclarations.HidP_GetCaps(preparsedData, ref this.Capabilities) != 0)
                {
                    int num = HidApiDeclarations.HidP_GetValueCaps(0, ref buffer2[0], ref this.Capabilities.NumberInputValueCaps, preparsedData);
                    flag = HidApiDeclarations.HidD_FreePreparsedData(ref preparsedData);
                }
            }
            catch (Exception exception)
            {
                HandleException("Hid:" + MethodBase.GetCurrentMethod(), exception);
            }
            return this.Capabilities;
        }

        internal string GetHIDUsage(HidApiDeclarations.HIDP_CAPS MyCapabilities)
        {
            string str = "";
            try
            {
                switch (((MyCapabilities.UsagePage * 0x100) + MyCapabilities.Usage))
                {
                    case 0x102:
                        str = "mouse";
                        break;

                    case 0x106:
                        str = "keyboard";
                        break;
                }
            }
            catch (Exception exception)
            {
                HandleException("Hid:" + MethodBase.GetCurrentMethod(), exception);
            }
            return str;
        }

        internal bool GetNumberOfInputBuffers(int hidDeviceObject, ref int numberOfInputBuffers)
        {
            bool flag = false;
            try
            {
                if (!this.IsWindows98Gold())
                {
                    return HidApiDeclarations.HidD_GetNumInputBuffers(hidDeviceObject, ref numberOfInputBuffers);
                }
                numberOfInputBuffers = 2;
                flag = true;
            }
            catch (Exception exception)
            {
                HandleException("Hid:" + MethodBase.GetCurrentMethod(), exception);
            }
            return flag;
        }

        public static void HandleException(string moduleName, Exception e)
        {
            try
            {
                string text = "Exception: " + e.Message + Environment.NewLine + "Module: " + moduleName + Environment.NewLine + "Method: " + e.TargetSite.Name;
                string caption = "Unexpected Exception";
                MultiZonePlayer.MLog.Log(null, text + caption);
                Debug.Write(text);
            }
            finally
            {
            }
        }

        internal bool IsWindows98Gold()
        {
            bool flag = false;
            try
            {
                OperatingSystem oSVersion = Environment.OSVersion;
                Version version = new Version(4, 10, 0x887);
                if (oSVersion.Version >= version)
                {
                    Debug.Write("The OS is Windows 98 Gold.");
                    return true;
                }
                Debug.Write("The OS is more recent than Windows 98 Gold.");
                flag = false;
            }
            catch (Exception exception)
            {
                HandleException("Hid:" + MethodBase.GetCurrentMethod(), exception);
            }
            return flag;
        }

        internal bool IsWindowsXpOrLater()
        {
            bool flag = false;
            try
            {
                OperatingSystem oSVersion = Environment.OSVersion;
                Version version = new Version(5, 1);
                if (oSVersion.Version >= version)
                {
                    Debug.Write("The OS is Windows XP or later.");
                    return true;
                }
                Debug.Write("The OS is earlier than Windows XP.");
                flag = false;
            }
            catch (Exception exception)
            {
                HandleException("Hid:" + MethodBase.GetCurrentMethod(), exception);
            }
            return flag;
        }

        internal bool SetNumberOfInputBuffers(int hidDeviceObject, int numberBuffers)
        {
            bool flag = false;
            try
            {
                if (!this.IsWindows98Gold())
                {
                    return HidApiDeclarations.HidD_SetNumInputBuffers(hidDeviceObject, numberBuffers);
                }
                flag = false;
            }
            catch (Exception exception)
            {
                HandleException("Hid:" + MethodBase.GetCurrentMethod(), exception);
            }
            return flag;
        }

        internal abstract class DeviceReport
        {
            internal int HIDHandle;
            internal bool MyDeviceDetected;
            internal int ReadHandle;
            internal int Result;
            internal int WriteHandle;

            protected DeviceReport()
            {
            }

            protected abstract void ProtectedRead(int readHandle, int hidHandle, int writeHandle, ref bool myDeviceDetected, ref byte[] readBuffer, ref bool success);
            internal void Read(int readHandle, int hidHandle, int writeHandle, ref bool myDeviceDetected, ref byte[] readBuffer, ref bool success)
            {
                try
                {
                    this.ProtectedRead(readHandle, hidHandle, writeHandle, ref myDeviceDetected, ref readBuffer, ref success);
                }
                catch (Exception exception)
                {
                    Hid.HandleException("Hid:" + MethodBase.GetCurrentMethod(), exception);
                }
            }
        }

        internal abstract class HostReport
        {
            protected HostReport()
            {
            }

            protected abstract bool ProtectedWrite(int deviceHandle, byte[] reportBuffer);
            internal bool Write(byte[] reportBuffer, int deviceHandle)
            {
                bool flag = false;
                try
                {
                    flag = this.ProtectedWrite(deviceHandle, reportBuffer);
                }
                catch (Exception exception)
                {
                    MultiZonePlayer.MLog.Log(this, exception.Message);
                }
                return flag;
            }
        }

        internal class InFeatureReport : Hid.DeviceReport
        {
            protected override void ProtectedRead(int readHandle, int hidHandle, int writeHandle, ref bool myDeviceDetected, ref byte[] inFeatureReportBuffer, ref bool success)
            {
                try
                {
                    success = HidApiDeclarations.HidD_GetFeature(hidHandle, ref inFeatureReportBuffer[0], inFeatureReportBuffer.Length);
                }
                catch (Exception exception)
                {
                    Hid.HandleException("Hid:" + MethodBase.GetCurrentMethod(), exception);
                }
            }
        }

        internal class InputReport : Hid.DeviceReport
        {
            private bool ReadyForOverlappedTransfer;

            internal void CancelTransfer(int readHandle, int hidHandle)
            {
                try
                {
                    base.Result = FileIOApiDeclarations.CancelIo(readHandle);
                    if (hidHandle != 0)
                    {
                        FileIOApiDeclarations.CloseHandle(hidHandle);
                    }
                    if (hidHandle != 0)
                    {
                        FileIOApiDeclarations.CloseHandle(readHandle);
                    }
                }
                catch (Exception exception)
                {
                    Hid.HandleException("Hid:" + MethodBase.GetCurrentMethod(), exception);
                }
            }

            internal void PrepareForOverlappedTransfer(ref FileIOApiDeclarations.OVERLAPPED hidOverlapped, ref int eventObject)
            {
                FileIOApiDeclarations.SECURITY_ATTRIBUTES structure = new FileIOApiDeclarations.SECURITY_ATTRIBUTES();
                try
                {
                    structure.lpSecurityDescriptor = IntPtr.Zero;
                    structure.bInheritHandle = true;
                    structure.nLength = Marshal.SizeOf(structure);
                    eventObject = FileIOApiDeclarations.CreateEvent(ref structure, Convert.ToInt32(false), Convert.ToInt32(true), "");
                    hidOverlapped.Offset = 0;
                    hidOverlapped.OffsetHigh = 0;
                    hidOverlapped.hEvent = eventObject;
                    this.ReadyForOverlappedTransfer = true;
                }
                catch (Exception exception)
                {
                    Hid.HandleException("Hid:" + MethodBase.GetCurrentMethod(), exception);
                }
            }

            protected override void ProtectedRead(int readHandle, int hidHandle, int writeHandle, ref bool myDeviceDetected, ref byte[] inputReportBuffer, ref bool success)
            {
                int eventObject = 0;
                int lpNumberOfBytesRead = 0;
                long num3 = 0L;
                FileIOApiDeclarations.OVERLAPPED hidOverlapped = new FileIOApiDeclarations.OVERLAPPED();
                try
                {
                    if (!this.ReadyForOverlappedTransfer)
                    {
                        this.PrepareForOverlappedTransfer(ref hidOverlapped, ref eventObject);
                    }
                    Debug.Write("input report length = " + inputReportBuffer.Length);
                    num3 = FileIOApiDeclarations.ReadFile(readHandle, ref inputReportBuffer[0], inputReportBuffer.Length, ref lpNumberOfBytesRead, ref hidOverlapped);
                    num3 = FileIOApiDeclarations.WaitForSingleObject(eventObject, 0xbb8);
                    switch (num3)
                    {
                        case 0L:
                            Debug.WriteLine("");
                            success = true;
                            Debug.WriteLine("ReadFile completed successfully.");
                            break;

                        case 0x102L:
                            this.CancelTransfer(readHandle, hidHandle);
                            Debug.WriteLine("Readfile timeout");
                            Debug.WriteLine("");
                            success = false;
                            myDeviceDetected = false;
                            break;

                        default:
                            this.CancelTransfer(readHandle, hidHandle);
                            Debug.WriteLine("");
                            Debug.WriteLine("Readfile undefined error");
                            success = false;
                            myDeviceDetected = false;
                            break;
                    }
                    success = num3 == 0L;
                }
                catch (Exception exception)
                {
                    Hid.HandleException("Hid:" + MethodBase.GetCurrentMethod(), exception);
                }
            }
        }

        internal class InputReportViaControlTransfer : Hid.DeviceReport
        {
            protected override void ProtectedRead(int readHandle, int hidHandle, int writeHandle, ref bool myDeviceDetected, ref byte[] inputReportBuffer, ref bool success)
            {
                try
                {
                    success = HidApiDeclarations.HidD_GetInputReport(hidHandle, ref inputReportBuffer[0], inputReportBuffer.Length);
                }
                catch (Exception exception)
                {
                    Hid.HandleException("Hid:" + MethodBase.GetCurrentMethod(), exception);
                }
            }
        }

        internal class OutFeatureReport : Hid.HostReport
        {
            protected override bool ProtectedWrite(int hidHandle, byte[] outFeatureReportBuffer)
            {
                bool flag = false;
                try
                {
                    flag = HidApiDeclarations.HidD_SetFeature(hidHandle, ref outFeatureReportBuffer[0], outFeatureReportBuffer.Length);
                    for (int i = 0; i < outFeatureReportBuffer.Length; i++)
                    {
                        Debug.WriteLine(" " + outFeatureReportBuffer[i].ToString("x"));
                    }
                }
                catch (Exception exception)
                {
                    Hid.HandleException("Hid:" + MethodBase.GetCurrentMethod(), exception);
                }
                return flag;
            }
        }

        internal class OutputReport : Hid.HostReport
        {
            protected override bool ProtectedWrite(int hidHandle, byte[] outputReportBuffer)
            {
                int lpNumberOfBytesWritten = 0;
                bool flag = false;
                try
                {
                    int num2 = FileIOApiDeclarations.WriteFile(hidHandle, ref outputReportBuffer[0], outputReportBuffer.Length, ref lpNumberOfBytesWritten, 0);
                    for (int i = 0; i < outputReportBuffer.Length; i++)
                    {
                        Debug.WriteLine("   " + outputReportBuffer[i].ToString("x"));
                    }
                    flag = num2 != 0;
                }
                catch (Exception exception)
                {
                    Hid.HandleException("Hid:" + MethodBase.GetCurrentMethod(), exception);
                }
                return flag;
            }
        }

        internal class OutputReportViaControlTransfer : Hid.HostReport
        {
            protected override bool ProtectedWrite(int hidHandle, byte[] outputReportBuffer)
            {
                bool flag = false;
                try
                {
                    flag = HidApiDeclarations.HidD_SetOutputReport(hidHandle, ref outputReportBuffer[0], outputReportBuffer.Length);
                }
                catch (Exception exception)
                {
                    Hid.HandleException("Hid:" + MethodBase.GetCurrentMethod(), exception);
                }
                return flag;
            }
        }
    }
}


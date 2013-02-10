namespace USB_RC2
{
    using System;
    using System.Runtime.InteropServices;

    internal class FileIOApiDeclarations
    {
        public const uint FILE_FLAG_OVERLAPPED = 0x40000000;
        public const uint FILE_SHARE_READ = 1;
        public const uint FILE_SHARE_WRITE = 2;
        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;
        public const int INVALID_HANDLE_VALUE = -1;
        public const short OPEN_EXISTING = 3;
        public const short WAIT_OBJECT_0 = 0;
        public const int WAIT_TIMEOUT = 0x102;

        [DllImport("kernel32.dll")]
        public static extern int CancelIo(int hFile);
        [DllImport("kernel32.dll")]
        public static extern int CloseHandle(int hObject);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern int CreateEvent(ref SECURITY_ATTRIBUTES SecurityAttributes, int bManualReset, int bInitialState, string lpName);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern int CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, ref SECURITY_ATTRIBUTES lpSecurityAttributes, int dwCreationDisposition, uint dwFlagsAndAttributes, int hTemplateFile);
        [DllImport("kernel32.dll")]
        public static extern int ReadFile(int hFile, ref byte lpBuffer, int nNumberOfBytesToRead, ref int lpNumberOfBytesRead, ref OVERLAPPED lpOverlapped);
        [DllImport("kernel32.dll")]
        public static extern int WaitForSingleObject(int hHandle, int dwMilliseconds);
        [DllImport("kernel32.dll")]
        public static extern int WriteFile(int hFile, ref byte lpBuffer, int nNumberOfBytesToWrite, ref int lpNumberOfBytesWritten, int lpOverlapped);

        [StructLayout(LayoutKind.Sequential)]
        public struct OVERLAPPED
        {
            public int Internal;
            public int InternalHigh;
            public int Offset;
            public int OffsetHigh;
            public int hEvent;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public int lpSecurityDescriptor;
            public int bInheritHandle;
        }
    }
}


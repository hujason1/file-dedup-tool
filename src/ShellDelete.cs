using System;
using System.Runtime.InteropServices;

namespace FileDedup
{
    public static class ShellDelete
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            public uint wFunc;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pFrom;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pTo;
            public ushort fFlags;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszProgressTitle;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int SHFileOperation(ref SHFILEOPSTRUCT lpFileOp);

        private const uint FO_DELETE = 0x0003;
        private const ushort FOF_SILENT = 0x0004;
        private const ushort FOF_NOCONFIRMATION = 0x0010;
        private const ushort FOF_ALLOWUNDO = 0x0040;
        private const ushort FOF_NOERRORUI = 0x0400;
        private const ushort FOF_NOCONFIRMMKDIR = 0x0200;

        public static int Run(string[] paths, bool toRecycleBin, IntPtr owner)
        {
            if (paths == null || paths.Length == 0) return 0;
            string from = string.Join("\0", paths) + "\0\0";

            SHFILEOPSTRUCT op = new SHFILEOPSTRUCT();
            op.hwnd = owner;
            op.wFunc = FO_DELETE;
            op.pFrom = from;
            op.pTo = null;
            op.fFlags = (ushort)(FOF_NOCONFIRMATION | FOF_NOERRORUI | FOF_SILENT | FOF_NOCONFIRMMKDIR | (toRecycleBin ? FOF_ALLOWUNDO : 0));
            op.fAnyOperationsAborted = false;
            op.hNameMappings = IntPtr.Zero;
            op.lpszProgressTitle = null;

            try
            {
                return SHFileOperation(ref op);
            }
            catch
            {
                return -1;
            }
        }
    }
}



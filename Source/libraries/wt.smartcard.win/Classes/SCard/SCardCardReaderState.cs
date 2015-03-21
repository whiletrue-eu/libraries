// ReSharper disable InconsistentNaming
using System;
using System.Runtime.InteropServices;

namespace WhileTrue.Classes.SCard
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct SCardCardReaderState
    {
        [MarshalAs(UnmanagedType.LPTStr)] public string szCardReader;
        public IntPtr pvUserData;
        public SCardReaderState dwCurrentState;
        public SCardReaderState dwEventState;
        public uint cbAtr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 36)] public byte[] rgbAtr;
    }
}
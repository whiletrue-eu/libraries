using System;

namespace WhileTrue.Components.CardReaderSubsystem.SCP
{
    internal abstract class SCAPCommands
    {
        public abstract byte[] PowerOn(IntPtr sctp, bool reset);
        public abstract byte[] SendAPDU(IntPtr sctp, byte[] data);
    }
}
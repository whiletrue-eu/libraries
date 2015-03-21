using System;

namespace WhileTrue.Components.CardReaderSubsystem.SCP
{
    internal abstract class ScapCommands
    {
        public abstract byte[] PowerOn(IntPtr sctp, bool reset);
        public abstract byte[] SendApdu(IntPtr sctp, byte[] data);
    }
}
using System;

namespace WhileTrue.Components.CardReaderSubsystem.SCP
{
    internal class ScpException : Exception
    {
        internal ScpException(LowLevelError error)
            : base(error.ToString())
        {
            this.Error = error;
        }

        public LowLevelError Error { get; }
    }
}
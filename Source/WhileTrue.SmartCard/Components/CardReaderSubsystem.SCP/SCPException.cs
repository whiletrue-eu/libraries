using System;

namespace WhileTrue.Components.CardReaderSubsystem.SCP
{
    internal class SCPException : Exception
    {
        private readonly LowLevelError error;

        internal SCPException(LowLevelError error)
            : base(error.ToString())
        {
            this.error = error;
        }

        public LowLevelError Error
        {
            get { return this.error; }
        }
    }
}
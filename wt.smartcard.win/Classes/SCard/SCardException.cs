using System;

namespace WhileTrue.Classes.SCard
{
    public class SCardException : Exception
    {
        internal SCardException(SCardError error)
            : base(error.ToString())
        {
            this.Error = error;
        }

        public SCardError Error { get; }
    }
}
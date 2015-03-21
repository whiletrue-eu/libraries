using System;

namespace WhileTrue.Classes.SCard
{
    public class SCardException : Exception
    {
        private readonly SCardError error;

        internal SCardException(SCardError error)
            : base(error.ToString())
        {
            this.error = error;
        }

        public SCardError Error
        {
            get { return this.error; }
        }
    }
}
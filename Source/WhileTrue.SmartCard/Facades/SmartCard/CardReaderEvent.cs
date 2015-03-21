using System;

namespace WhileTrue.Facades.SmartCard
{
    /// <supportingClass/>
    /// <summary/>
    public class CardReaderEventArgs : EventArgs
    {
        private readonly ICardReader cardReader;

        ///<summary/>
        public CardReaderEventArgs(ICardReader cardReader)
        {
            this.cardReader = cardReader;
        }

        /// <summary>
        /// Gets the card reader the event was fired for
        /// </summary>
        public ICardReader CardReader
        {
            get { return this.cardReader; }
        }
    }
}
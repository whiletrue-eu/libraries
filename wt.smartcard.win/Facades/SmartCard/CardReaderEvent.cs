using System;

namespace WhileTrue.Facades.SmartCard
{
    /// <supportingClass/>
    /// <summary/>
    public class CardReaderEventArgs : EventArgs
    {
        ///<summary/>
        public CardReaderEventArgs(ICardReader cardReader)
        {
            this.CardReader = cardReader;
        }

        /// <summary>
        /// Gets the card reader the event was fired for
        /// </summary>
        public ICardReader CardReader { get; }
    }
}
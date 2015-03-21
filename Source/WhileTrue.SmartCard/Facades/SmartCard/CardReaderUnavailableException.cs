using System;

namespace WhileTrue.Facades.SmartCard
{
    /// <summary>
    /// <see cref="ICardReader"/> specific.
    /// </summary>
    public class CardReaderUnavailableException : SmartCardExceptionBase
    {
        private readonly ICardReader cardReader;

        /// <summary>
        /// Creates the exception
        /// </summary>
        public CardReaderUnavailableException(ICardReader cardReader)
            : base("Smart card is unavailable")
        {
            this.cardReader = cardReader;
        }

        /// <summary>
        /// Gets the card reader the exception was thrown for
        /// </summary>
        public ICardReader CardReader
        {
            get { return this.cardReader; }
        }
    }
}
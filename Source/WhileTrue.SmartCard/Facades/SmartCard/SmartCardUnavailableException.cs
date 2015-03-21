using System;

namespace WhileTrue.Facades.SmartCard
{
    /// <summary>
    /// <see cref="ISmartCard"/> specific.
    /// Used, if the card is not present in a card reader anymore.
    /// </summary>
    public class SmartCardUnavailableException : SmartCardExceptionBase
    {
        private readonly ISmartCard smartCard;

        /// <summary>
        /// Creates the exception
        /// </summary>
        public SmartCardUnavailableException(ISmartCard smartCard)
            : base("Smart card is unavailable")
        {
            this.smartCard = smartCard;
        }

        /// <summary>
        /// Gets the smart card the exception was thrown for
        /// </summary>
        public ISmartCard SmartCard
        {
            get { return this.smartCard; }
        }
    }
}
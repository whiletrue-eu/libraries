namespace WhileTrue.Facades.SmartCard
{
    /// <summary>
    /// <see cref="ISmartCard"/> specific.
    /// Used, if the card is not connected in an operaton requiring a connection to the card.
    /// </summary>
    public class SmartCardNotConnectedException : SmartCardExceptionBase
    {
        /// <summary>
        /// Creates the exception
        /// </summary>
        public SmartCardNotConnectedException(ISmartCard smartCard)
            : base("Smart card is not connected")
        {
            this.SmartCard = smartCard;
        }

        /// <summary>
        /// Gets the smart card the exception was thrown for
        /// </summary>
        public ISmartCard SmartCard { get; }
    }
}
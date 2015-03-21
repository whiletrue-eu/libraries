namespace WhileTrue.Facades.SmartCard
{
    /// <summary>
    /// <see cref="ISmartCard"/> specific.
    /// Used, if the card is in use by another application.
    /// </summary>
    public class SmartCardInUseException : SmartCardExceptionBase
    {
        private readonly ISmartCard smartCard;

        /// <summary>
        /// Creates the exception
        /// </summary>
        public SmartCardInUseException(ISmartCard smartCard)
            : base("Smart card is in use")
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
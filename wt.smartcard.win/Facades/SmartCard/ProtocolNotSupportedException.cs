namespace WhileTrue.Facades.SmartCard
{
    /// <summary>
    /// <see cref="ISmartCard"/> specific.
    /// Used, if the card / card reader does not support the protocol given.
    /// </summary>
    public class ProtocolNotSupportedException : SmartCardExceptionBase
    {
        /// <summary>
        /// Creates the exception
        /// </summary>
        public ProtocolNotSupportedException(ISmartCard smartcard, Protocol protocol)
            : base($"Protocol {protocol} is not supported by the card")
        {
            this.Protocol = protocol;
            this.Smartcard = smartcard;
        }

        /// <summary>
        /// Gets the protocol that was requested
        /// </summary>
        public Protocol Protocol { get; }

        /// <summary>
        /// Gets the smart card the exception was thrown for
        /// </summary>
        public ISmartCard Smartcard { get; }
    }
}
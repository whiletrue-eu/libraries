using System;

namespace WhileTrue.Facades.SmartCard
{
    /// <summary>
    /// <see cref="ISmartCard"/> specific.
    /// Used, if the card / card reader does not support the protocol given.
    /// </summary>
    public class ProtocolNotSupportedException : SmartCardExceptionBase
    {
        private readonly Protocol protocol;
        private readonly ISmartCard smartcard;

        /// <summary>
        /// Creates the exception
        /// </summary>
        public ProtocolNotSupportedException(ISmartCard smartcard, Protocol protocol)
            : base(string.Format("Protocol {0} is not supported by the card", protocol))
        {
            this.protocol = protocol;
            this.smartcard = smartcard;
        }

        /// <summary>
        /// Gets the protocol that was requested
        /// </summary>
        public Protocol Protocol
        {
            get { return this.protocol; }
        }

        /// <summary>
        /// Gets the smart card the exception was thrown for
        /// </summary>
        public ISmartCard Smartcard
        {
            get { return this.smartcard; }
        }
    }
}
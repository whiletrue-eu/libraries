namespace WhileTrue.Classes.SCard
{
    public enum SCardProtocol
    {
        /// <summary>
        /// T=0 Protocol
        /// </summary>
        T0 = 0x00000001,
        /// <summary>
        /// T=1 Protocol
        /// </summary>
        T1 = 0x00000002,
        /// <summary>
        /// Raw Protocol
        /// </summary>
        Raw = 0x00010000,
        /// <summary>
        /// No protocol, for direct reader access
        /// </summary>
        None = 0x00000000,
        /// <summary>
        /// T0 or T1 possible
        /// </summary>
        All = SCardProtocol.T0|SCardProtocol.T1,
    }
}
namespace WhileTrue.Components.CardReaderSubsystem.SCP
{
    internal enum LowLevelError : uint
    {
        /// <summary>
        /// No error
        /// </summary>
        NoError = 0,
        /// <summary>
        /// No chip card-reader found
        /// </summary>
        NoCardReader = 1,
        /// <summary>
        /// No chip card in the card reader
        /// </summary>
        NoCardInserted = 2,
        /// <summary>
        /// Wrong chip card, no ATR, wrong format
        /// </summary>
        CardNotSupported = 3,
        /// <summary>
        /// Error on communication with chip card reader
        /// </summary>
        ReaderCommunicationError = 4,
        /// <summary>
        /// Error on communication with chip card
        /// </summary>
        CardCommunicationError = 5,
        /// <summary>
        /// No ICB in the S2-block, only level-intern
        /// </summary>
        NoIcb = 6,
        /// <summary>
        /// Received block number is greater than from ICL-level expected
        /// </summary>
        IclOverrun = 7,
        /// <summary>
        /// Power off at chip card-interface
        /// </summary>
        PowerOff = 8,
        /// <summary>
        /// Illegal function call
        /// </summary>
        IllegalFunctionCall = 9,
        /// <summary>
        /// cc440x: card removed since last IMAGE READ
        /// </summary>
        CardRemovedError = 10,
    }
}
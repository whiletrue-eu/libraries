namespace WhileTrue.Facades.SmartCard
{
    public interface ICardReaderConnectionInformation
    {
        /// <summary>
        /// Gets the system name of the reader
        /// </summary>
        string SystemName { get; }

        /// <summary>
        /// Gets the type of channel the reader is connected to
        /// </summary>
        string Channel { get; }

        /// <summary>
        /// Gets whether ther reader supports card swallowing
        /// </summary>
        bool SupportsSwallowing { get; }

        /// <summary>
        /// Gets whether ther reader supports card capturing
        /// </summary>
        bool SupportsCapture { get; }

        /// <summary>
        /// Gets whether ther reader supports card ejection
        /// </summary>
        bool SupportsEject { get; }
       
        /// <summary>
        /// Gets the Default clock rate, in kHz.
        /// </summary>
        uint? DefaultClockRate { get;}

        /// <summary>
        /// Gets the Default data rate, in bps.
        /// </summary>
        uint? DefaultDataRate { get;}
    }
}
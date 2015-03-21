namespace WhileTrue.Facades.SmartCard
{
    public interface ISmartCardConnectionInformation
    {
        /// <summary>
        /// Gets the Current block waiting time.
        /// </summary>
        uint? CurrentBlockWaitingTime { get; }

        /// <summary>
        /// Gets the Current clock rate, in kHz.
        /// </summary>
        uint? CurrentClockRate { get; }

        /// <summary>
        /// Gets the Current character waiting time.
        /// </summary>
        uint? CurrentCharacterWaitingTime { get; }

        /// <summary>
        /// Gets the Current Bit rate conversion factor D.
        /// </summary>
        uint? CurrentD { get; }

        /// <summary>
        /// Gets the Current error block control encoding.
        /// </summary>
        EbcEncoding? CurrentEbcEncoding { get; }

        /// <summary>
        /// Gets the Clock conversion factor.
        /// </summary>
        uint? CurrentF { get; }

        /// <summary>
        /// Gets the Current guard time.
        /// </summary>
        uint? CurrentN { get; }

        /// <summary>
        /// Gets the Current work waiting time.
        /// </summary>
        uint? CurrentW { get; }
    }
}
namespace Mz.Facades.SmartCard
{
    /// <summary>
    /// States a card reader / smart card can be in
    /// <seealso cref="ICardReader.State"/>
    /// <seealso cref="ISmartCard.State"/>
    /// </summary>
    public enum State
    {
        /// <summary>State is unknown</summary>
        Unknown,
        /// <summary>The card reader does not contain a card</summary>
        /// <remarks>Does not apply on <see cref="ISmartCard.State"/></remarks>
        NoCard,
        /// <summary>Card is present but no communication established</summary>
        CardPresent,
        /// <summary>Card is present and in use</summary>
        /// <remarks>Card is in use, but not necesarrily by this application, but any application that can access the card</remarks>
        CardInUse,
        /// <summary>Card is present and exclusively in use</summary>
        /// <remarks>Card is in use, but not necesarrily by this application, but any application that can access the card</remarks>
        CardExclusivelyInUse,
        /// <summary>Card is present, but the card reader is unable to communicate with the card (on a low level layer)</summary>
        CardMute,
    }
}
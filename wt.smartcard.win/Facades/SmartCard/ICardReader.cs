using System;

namespace WhileTrue.Facades.SmartCard
{
    /// <summary>
    /// Instance that is implemented by card reader wrapper classes
    /// </summary>
    public interface ICardReader
    {
        /// <summary>
        /// Gets/sets the name of the card reader
        /// </summary>
        string FriendlyName { get; } 
        
        /// <summary>
        /// Gets the name of the card reader
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets information about the card reader
        /// </summary>
        ICardReaderConnectionInformation ReaderConnectionInformation { get; }
        /// <summary>
        /// Gets information about the smart card in the reader
        /// </summary>
        ISmartCardConnectionInformation CardConnectionInformation { get; }

        /// <summary>
        /// Gets the state of the card reader
        /// </summary>
        CardReaderState State { get; }

        /// <summary>
        /// Gets the <see cref="ISmartCard"/> object for the smart card currently inserted in the card reader
        /// </summary>
        /// <remarks>
        /// Returns <c>null</c> if no smart card is present
        /// </remarks>
        ISmartCard SmartCard { get; }

        /// <summary>
        /// Fired, when the reader is removed
        /// </summary>
        /// <remarks>
        /// The event may be fired in another thread.
        /// </remarks>
        event EventHandler<EventArgs> Removed;

        /// <summary>
        /// Fired, when the <see cref="State"/> of the card reader changes
        /// </summary>
        /// <remarks>
        /// The event may be fired in another thread.
        /// </remarks>
        event EventHandler<CardReaderEventArgs> StateChanged;

        /// <summary>
        /// Fired, when a smart card was inserted.
        /// </summary>
        /// <remarks>
        /// The <see cref="SmartCard"/> property will be set before the event is fired.
        /// The event may be fired in another thread.
        /// </remarks>
        event EventHandler<CardReaderEventArgs> SmartCardInserted;

        /// <summary>
        /// Fired, when a smart card was removed
        /// </summary>
        /// <remarks>
        /// The <see cref="SmartCard"/> property will be set to <c>null</c> before the event is fired.
        /// The event may be fired in another thread.
        /// </remarks>
        event EventHandler<CardReaderEventArgs> SmartCardRemoved;

        /// <summary>
        /// Gets whether it is possible to update reader information
        /// </summary>
        /// <remarks>note that, due to another application accesing the card reader,
        /// it still may be possible, that <see cref="UpdateConnectionInformation"/> may fail even
        /// if this property was <c>true</c> during call</remarks>
        bool CanUpdateConnectionInformation { get; }

        /// <summary>
        /// Updates the <see cref="ReaderConnectionInformation"/> Member. 
        /// </summary>
        /// <remarks>Updating is not always possible as other applications may have exclusive
        /// access to the card reader. In this case, an exception is thrown 
        /// </remarks>
        /// <exception cref="CardReaderUnavailableException">Thrown if no access to the card reader can be acquired.</exception>
        void UpdateConnectionInformation();
    }
}
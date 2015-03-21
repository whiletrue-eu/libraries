using System;
using WhileTrue.Classes.Components;

namespace WhileTrue.Facades.SmartCard
{
    ///<summary>
    /// provides access to card readers and smart cards 
    ///</summary>
    [ComponentInterface]
    public interface ISmartCardService
    {
        /// <summary>
        /// Gets the collection of <see cref="ICardReader"/>s that contains all card readers known to the framework.
        /// <seealso cref="SmartCards"/>
        /// <seealso cref="CardReaderAdded"/>
        /// <seealso cref="CardReaderRemoved"/>
        /// </summary>
        CardReaderCollection CardReaders { get; }

        /// <summary>
        /// Gets the collection of <see cref="ISmartCard"/>s that contains all smart cards inserted in any card readers known to the framework.
        /// <seealso cref="SmartCardAdded"/>
        /// <seealso cref="SmartCardRemoved"/>
        /// </summary>
        SmartCardCollection SmartCards { get; }

        /// <summary>
        /// Is fired when a new card reader is introduced in the system (e.g. by Plug'n'Play)
        /// </summary>
        event EventHandler<CardReaderEventArgs> CardReaderAdded;

        /// <summary>
        /// Is fired when a card reader is removed from the system (e.g. by Plug'n'Play)
        /// </summary>
        event EventHandler<CardReaderEventArgs> CardReaderRemoved;

        /// <summary>
        /// Is fired, when a smart card is inserted in a reader known to the system
        /// </summary>
        event EventHandler<SmartCardEventArgs> SmartCardAdded;

        /// <summary>
        /// Is fired, when a smart card is removed from a reader known to the system
        /// </summary>
        event EventHandler<SmartCardEventArgs> SmartCardRemoved;
    }
}
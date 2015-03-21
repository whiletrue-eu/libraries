using System;
using WhileTrue.Classes.Components;

namespace WhileTrue.Facades.SmartCard
{
    [ComponentInterface]
    public interface ICardReaderSubsystem
    {
        /// <summary>
        /// Gets the collection of readers known in this subsystem
        /// </summary>
        CardReaderCollection Readers { get; }

        /// <summary>
        /// Fired, if a card reader is added to the subsystem
        /// </summary>
        event EventHandler<CardReaderEventArgs> CardReaderRemoved;

        /// <summary>
        /// Fired, if a card reader is removed from the subsystem
        /// </summary>
        event EventHandler<CardReaderEventArgs> CardReaderAdded;
    }
}
using System;
using WhileTrue.Facades.SmartCard;

namespace WhileTrue.Components.CardReaderSubsystem.Base
{
    /// <summary>
    /// Base class for a card reader subsystem implementation
    /// </summary>
    /// <remarks>
    /// see <a href="Overview/CreateCardReaderSubsystem.html">Create card reader subsystem</a> for details
    /// </remarks>
    public class CardReaderSubsystemBase : ICardReaderSubsystem
    {
        protected CardReaderSubsystemBase()
        {
        }

        #region ICardReaderSubsystem Members

        /// <summary>
        /// Fired, if a card reader is added to the subsystem
        /// </summary>
        public event EventHandler<CardReaderEventArgs> CardReaderRemoved;

        /// <summary>
        /// Fired, if a card reader is removed from the subsystem
        /// </summary>
        public event EventHandler<CardReaderEventArgs> CardReaderAdded;

        /// <summary>
        /// Gets the collection of readers known in this subsystem
        /// </summary>
        public CardReaderCollection Readers { get; } = new CardReaderCollection();

        #endregion

        /// <summary>
        /// Call to add a card reader to this subsystem
        /// </summary>
        /// <param name="reader">reader to add</param>
        protected void AddCardReader(CardReaderBase reader)
        {
            this.Readers.Add(reader);
            this.InvokeCardReaderAdded(reader);
        }

        /// <summary>
        /// Call to remove a card reader from the subsystem
        /// </summary>
        /// <param name="reader">reader to remove</param>
        protected void RemoveCardReader(CardReaderBase reader)
        {
            this.Readers.Remove(reader);
            this.InvokeCardReaderRemoved(reader);
            reader.NotifyRemoved();
        }

        private void InvokeCardReaderAdded(CardReaderBase cardReader)
        {
            if (this.CardReaderAdded != null)
            {
                this.CardReaderAdded(cardReader, new CardReaderEventArgs(cardReader));
            }
        }

        private void InvokeCardReaderRemoved(CardReaderBase cardReader)
        {
            if (this.CardReaderRemoved != null)
            {
                this.CardReaderRemoved(cardReader, new CardReaderEventArgs(cardReader));
            }
        }
    }
}
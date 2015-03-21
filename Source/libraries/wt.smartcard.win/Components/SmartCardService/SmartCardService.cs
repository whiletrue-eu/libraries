using System;
using System.Diagnostics;
using WhileTrue.Classes.Components;
using WhileTrue.Classes.Framework;
using WhileTrue.Facades.SmartCard;

namespace WhileTrue.Components.SmartCardService
{
    /// <summary>
    /// This is the main class of the Smart Card Access framework. It provudes static methods to enumerate card readers and smart cards and publishes events to get notified, when a card reader or smart card changes.
    /// </summary>
    [Component("Smart Card Service")]
    public class SmartCardService : ObservableObject, ISmartCardService
    {
        #region [Private fields] ------------------------------------------------------------------

        #endregion //------------------------------------------------------------------------------

        #region [Construction] --------------------------------------------------------------------

        ///<summary/>
        public SmartCardService(ICardReaderSubsystem[] readerSubsystems)
        {
            foreach (ICardReaderSubsystem ReaderSubsystem in readerSubsystems)
            {
                this.AddSubsystem(ReaderSubsystem);
            }
        }

        #endregion //------------------------------------------------------------------------------

        #region [Private methods] -----------------------------------------------------------------

        private void InvokeCardReaderRemoved(ICardReader cardReader)
        {
            if (this.CardReaderRemoved != null)
            {
                this.CardReaderRemoved(cardReader, new CardReaderEventArgs(cardReader));
            }
        }

        private void InvokeCardReaderAdded(ICardReader cardReader)
        {
            if (this.CardReaderAdded != null)
            {
                this.CardReaderAdded(cardReader, new CardReaderEventArgs(cardReader));
            }
        }

        private void InvokeSmartCardRemoved(ISmartCard smartCard)
        {
            if (this.SmartCardRemoved != null)
            {
                this.SmartCardRemoved(smartCard, new SmartCardEventArgs(smartCard));
            }
        }

        private void InvokeSmartCardAdded(ISmartCard smartCard)
        {
            if (this.SmartCardAdded != null)
            {
                this.SmartCardAdded(smartCard, new SmartCardEventArgs(smartCard));
            }
        }

        private void SubsystemCardReaderAdded(object sender, CardReaderEventArgs e)
        {
            this.AddCardReader(e.CardReader);
        }

        private void SubsystemCardReaderRemoved(object sender, CardReaderEventArgs e)
        {
            this.RemoveCardReader(e.CardReader);
        }

        private void SubsystemSmartCardInserted(object sender, CardReaderEventArgs e)
        {
            this.AddSmartCard(e.CardReader);
        }

        private void SubsystemSmartCardRemoved(object sender, CardReaderEventArgs e)
        {
            this.RemoveSmartCard(e.CardReader);
        }

        private void AddCardReader(ICardReader cardReader)
        {
            //cardReader.SetCardReaderUICallback( CardReaders.cardReaderUICallback );
            this.CardReaders.Add(cardReader);
            this.InvokeCardReaderAdded(cardReader);

            cardReader.SmartCardInserted += this.SubsystemSmartCardInserted;
            cardReader.SmartCardRemoved += this.SubsystemSmartCardRemoved;
            if (cardReader.SmartCard != null)
            {
                this.AddSmartCard(cardReader);
            }
        }

        private void RemoveCardReader(ICardReader cardReader)
        {
            cardReader.SmartCardInserted -= this.SubsystemSmartCardInserted;
            cardReader.SmartCardRemoved -= this.SubsystemSmartCardRemoved;
            if (cardReader.SmartCard != null)
            {
                this.RemoveSmartCard(cardReader);
            }

            this.CardReaders.Remove(cardReader);
            this.InvokeCardReaderRemoved(cardReader);
        }


        private void AddSmartCard(ICardReader cardReader)
        {
            Debug.Assert(cardReader != null);
            Debug.Assert(cardReader.SmartCard != null);

            ISmartCard SmartCard = cardReader.SmartCard;
            this.SmartCards.Add(SmartCard);
            this.InvokeSmartCardAdded(SmartCard);
        }

        private void RemoveSmartCard(ICardReader cardReader)
        {
            Trace.Assert(cardReader != null);

            ISmartCard SmartCard = this.SmartCards[cardReader];
            this.SmartCards.Remove(SmartCard);
            this.InvokeSmartCardRemoved(SmartCard);
        }

        #endregion //------------------------------------------------------------------------------

        #region [Public class interface] ---------------------------------------------------

        /// <summary>
        /// Gets the collection of <see cref="ICardReader"/>s that contains all card readers known to the framework.
        /// <seealso cref="SmartCards"/>
        /// <seealso cref="CardReaderAdded"/>
        /// <seealso cref="CardReaderRemoved"/>
        /// </summary>
        public CardReaderCollection CardReaders { get; } = new CardReaderCollection();

        /// <summary>
        /// Is fired when a new card reader is introduced in the system (e.g. by Plug'n'Play)
        /// </summary>
        public event EventHandler<CardReaderEventArgs> CardReaderAdded;

        /// <summary>
        /// Is fired when a card reader is removed from the system (e.g. by Plug'n'Play)
        /// </summary>
        public event EventHandler<CardReaderEventArgs> CardReaderRemoved;

        /// <summary>
        /// Gets the collection of <see cref="ISmartCard"/>s that contains all smart cards inserted in any card readers known to the framework.
        /// <seealso cref="SmartCardAdded"/>
        /// <seealso cref="SmartCardRemoved"/>
        /// </summary>
        public SmartCardCollection SmartCards { get; } = new SmartCardCollection();

        /// <summary>
        /// Is fired, when a smart card is inserted in a reader known to the system
        /// </summary>
        public event EventHandler<SmartCardEventArgs> SmartCardAdded;

        /// <summary>
        /// Is fired, when a smart card is removed from a reader known to the system
        /// </summary>
        public event EventHandler<SmartCardEventArgs> SmartCardRemoved;

        #endregion --------------------------------------------------------------------------------

        private void AddSubsystem(ICardReaderSubsystem subSystem)
        {
            subSystem.CardReaderAdded += this.SubsystemCardReaderAdded;
            subSystem.CardReaderRemoved += this.SubsystemCardReaderRemoved;
            foreach (ICardReader CardReader in subSystem.Readers)
            {
                this.AddCardReader(CardReader);
            }
        }
    }
}
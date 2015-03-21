using System;
using System.Threading.Tasks;
using WhileTrue.Classes.Framework;
using WhileTrue.Facades.SmartCard;
using WhileTrue.Types.SmartCard;


namespace WhileTrue.Components.CardReaderSubsystem.Base
{
    /// <summary>
    /// Base class for card reader wrappers.
    /// </summary>
    public abstract class CardReaderBase : ObservableObject, ICardReader
    {
        private string friendlyName;
        private SmartCard smartCard;


        /// <summary>
        /// Constructs a card reader with the given name
        /// </summary>
        protected CardReaderBase(string name)
        {
            this.friendlyName = name;
            this.Name = name;
        }

        /// <summary>
        /// gets the ATR of the smart card currently inserted.
        /// </summary>
        /// <remarks>
        /// called only if a smart card is inserted.
        /// </remarks>
        protected internal abstract byte[] Atr { get; }

        #region ICardReader Members

        /// <summary>
        /// Gets the name of the card reader
        /// </summary>
        public string FriendlyName
        {
            get { return this.friendlyName; }
            set
            {
                this.SetAndInvoke(ref this.friendlyName, value);
            }
        }   
        
        /// <summary>
        /// Gets the name of the card reader
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets information about the connection to the card reader
        /// </summary>
        public abstract ICardReaderConnectionInformation ReaderConnectionInformation { get; } 
        
        /// <summary>
        /// Gets information about the connection to the smart card
        /// </summary>
        public abstract ISmartCardConnectionInformation CardConnectionInformation { get; }

        /// <summary>
        /// Gets whether it is possible to update connection information
        /// </summary>
        /// <remarks>
        /// Due to another application accesing the card reader, it still may be possible,
        /// that <see cref="ICardReader.UpdateConnectionInformation"/> may fail even
        /// if this property was <c>true</c> during call</remarks>
        public abstract bool CanUpdateConnectionInformation { get; }

        /// <summary>
        /// Updates the <see cref="ICardReader.ReaderConnectionInformation"/> Member. 
        /// </summary>
        /// <remarks>Updating is not always possible as other applications may have exclusive
        /// access to the card reader. In this case, an exception is thrown 
        /// </remarks>
        /// <exception cref="SmartCardUnavailableException">Thrown if no access to the card reader can be acquired.</exception>
        public abstract void UpdateConnectionInformation();


        /// <summary>
        /// Gets the state of the card reader
        /// </summary>
        public abstract CardReaderState State { get; }

        /// <summary>
        /// Gets the <see cref="ISmartCard"/> object for the smart card currently inserted in the card reader
        /// </summary>
        /// <remarks>
        /// Returns <c>null</c> if no smart card is present
        /// </remarks>
        public ISmartCard SmartCard => this.smartCard;

        /// <summary>
        /// Fired, when the reader is removed
        /// </summary>
        /// <remarks>
        /// The event may be fired in another thread.
        /// </remarks>
        public event EventHandler<EventArgs> Removed = delegate{};

        /// <summary>
        /// Fired, when the <see cref="State"/> of the card reader changes
        /// </summary>
        /// <remarks>
        /// The event may be fired in another thread.
        /// </remarks>
        public event EventHandler<CardReaderEventArgs> StateChanged = delegate { };

        /// <summary>
        /// Fired, when a smart card was inserted.
        /// </summary>
        /// <remarks>
        /// The <see cref="smartCard"/> property will be set before the event is fired.
        /// The event may be fired in another thread.
        /// </remarks>
        public event EventHandler<CardReaderEventArgs> SmartCardInserted = delegate { };

        /// <summary>
        /// Fired, when a smart card was removed
        /// </summary>
        /// <remarks>
        /// The <see cref="smartCard"/> property will be set to <c>null</c> before the event is fired.
        /// The event may be fired in another thread.
        /// </remarks>
        public event EventHandler<CardReaderEventArgs> SmartCardRemoved = delegate { };

        #endregion

        /// <summary>
        /// Invokes the <see cref="StateChanged"/> event.
        /// </summary>
        protected void InvokeStateChanged()
        {
            if (this.smartCard != null)
            {
                if (! this.AccessibleCardInReader())
                {
                    SmartCard RemovedSmartCard = this.smartCard;
                    this.smartCard = null;
                    this.DisconnectCard();
                    this.InvokeSmartCardRemoved();
                    RemovedSmartCard.NotifyRemoved();
                }
                else
                {
                    this.smartCard.NotifyStateChanged();
                }
            }
            else
            {
                if (this.AccessibleCardInReader())
                {
                    this.smartCard = new SmartCard(this);
                    this.InvokeSmartCardInserted();
                }
            }

            if (this.StateChanged != null)
            {
                this.StateChanged(this, new CardReaderEventArgs(this));
            }
            this.InvokePropertyChanged(nameof(CardReaderBase.State));
        }

        private void InvokeSmartCardInserted()
        {
            if (this.SmartCardInserted != null)
            {
                this.SmartCardInserted(this, new CardReaderEventArgs(this));
            }
            this.InvokePropertyChanged(nameof(CardReaderBase.SmartCard));
        }

        private void InvokeSmartCardRemoved()
        {
            if (this.SmartCardRemoved != null)
            {
                this.SmartCardRemoved(this, new CardReaderEventArgs(this));
            }
            this.InvokePropertyChanged(nameof(CardReaderBase.SmartCard));
        }

        /// <summary>
        /// Returns, whether the smart card in the reader is accessible or not.
        /// </summary>
        /// <remarks>
        /// The default implementation returns <c>true</c> for a card reader which state is not <c>NoCard</c>, <c>CardMute</c>nor <c>Unknown</c>
        /// </remarks>
        private bool AccessibleCardInReader()
        {
            return this.State != CardReaderState.NoCard &&
                   this.State != CardReaderState.CardMute &&
                   this.State != CardReaderState.Unknown;
        }

        /// <summary>
        /// Transmits the byte array given to the card in the card reader.
        /// </summary>
        /// <param name="data">data to be transferred to the card</param>
        /// <returns>data that was returned from the card</returns>
        protected abstract byte[] Transmit(byte[] data);

        /// <summary>
        /// Transmits the given command to the card. If the command conatins variables, teh variables are resolved using the <see cref="ResolveVariable"/> method.
        /// </summary>
        protected internal CardResponse Transmit(CardCommand command, IVariableResolver resolver)
        {
            if (command is VariableCardCommand)
            {
                //Resolve variables
                VariableCardCommand VariableCommand = (VariableCardCommand) command;
                foreach (Variable Variable in VariableCommand.Variables)
                {
                    if( Variable.IsResolved ==false)
                    {
                        this.ResolveVariable(Variable, resolver);
                    }
                }
            }
            return new CardResponse(this.Transmit(command.Serialize(this.SmartCard.Protocol == Protocol.T1)));
        }   

        /// <summary>
        /// Transmits the given command to the card. If the command conatins variables, teh variables are resolved using the <see cref="ResolveVariable"/> method.
        /// </summary>
        protected internal Task<CardResponse> TransmitAsync(CardCommand command, IVariableResolver resolver)
        {
            return Task.Run(() => this.Transmit(command, resolver));
        }

        protected virtual void ResolveVariable(Variable variable, IVariableResolver resolver)
        {
            resolver.ResolveVariable(variable);
        }

        /// <summary>
        /// Connect to card with the given protocol
        /// </summary>
        /// <param name="protocol">Protocol to be used</param>
        /// <exception cref="ProtocolNotSupportedException">Thrown if the protocol is ot supported by the card or the card reader</exception>
        protected internal abstract void ConnectCard(Protocol protocol);

        /// <summary>
        /// Disconnect from the card (end connection established by a previous call to <see cref="ConnectCard"/>). This call shall always succeed even if the card was not connected.
        /// </summary>
        protected internal abstract void DisconnectCard();


        /// <summary>
        /// Reconnects to the card peforming a reset. A change of used protocol is possible
        /// </summary>
        protected internal abstract void ResetCard(Protocol protocol);

        /// <summary>
        /// Disconnect card and eject card from the card reader
        /// </summary>
        /// <remarks>
        /// The default implementation disconnects the card by calling <see cref="DisconnectCard"/> and prompts
        /// the user to remove tha card from the reader manually.
        /// </remarks>
        protected internal virtual void EjectCard()
        {
            this.DisconnectCard();
        }

        #region overrides

        /// <summary>
        /// Returns the <see cref="FriendlyName"/> of the card reader
        /// </summary>
        public override string ToString()
        {
            return this.FriendlyName;
        }

        #endregion

        internal void NotifyRemoved()
        {
            this.InvokeRemoved();
        }

        private void InvokeRemoved()
        {
            this.Removed(this, EventArgs.Empty);
        }
    }
}
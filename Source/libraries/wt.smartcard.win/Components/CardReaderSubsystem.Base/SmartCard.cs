using System;
using WhileTrue.Facades.SmartCard;
using WhileTrue.Types.SmartCard;
using System.Threading;
using System.Threading.Tasks;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Components.CardReaderSubsystem.Base
{
    /// <summary>
    /// Implements a wrapper for a smart card in a card rader.
    /// </summary>
    /// <remarks>
    /// Card reader subsystem specific functionality is delegated to the card reader implementation through the <c>...Card</c> methods.
    /// </remarks>
    internal class SmartCard : ObservableObject, ISmartCard
    {
        private readonly CardReaderBase reader;
        private bool isConnected;
        private Protocol protocol = Protocol.Na;
        private bool isRemoved;


        internal SmartCard(CardReaderBase reader)
        {
            this.reader = reader;
        }

        #region ISmartCard Members

        public void Connect(Protocol protocol)
        {
            this.Connect(protocol,0);
        }

        public void Connect(Protocol protocol, int timeout)
        {
            this.InternalConnect(protocol, timeout, CancellationToken.None);
        }

        public Task ConnectAsync(Protocol protocol)
        {
            return this.ConnectAsync(protocol, 0, CancellationToken.None);
        }

        public Task ConnectAsync(Protocol protocol, int timeout)
        {
            return this.ConnectAsync(protocol, timeout, CancellationToken.None);
        }

        public Task ConnectAsync(Protocol protocol, int timeout, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(_ => this.InternalConnect(protocol, timeout, cancellationToken), cancellationToken, TaskCreationOptions.LongRunning);
        }

        private void InternalConnect(Protocol protocol, int timeout, CancellationToken cancellationToken)
        {
            const int waitTime = 10;
            this.CheckRemoved();
            do
            {
                try
                {
                    this.reader.ConnectCard(protocol);
                    this.IsConnected = true;
                    this.Protocol = protocol;
                }
                catch (SmartCardInUseException)
                {
                    if (timeout != Timeout.Infinite)
                    {
                        timeout -= (timeout >= waitTime ? waitTime : timeout);
                    }
                    Thread.Sleep(waitTime);
                }
            } while (this.isConnected==false && (timeout > 0 || cancellationToken.IsCancellationRequested));

            if (this.IsConnected == false && cancellationToken.IsCancellationRequested == false)
            {
                //timeout is gone, and no cancel -> connect did not succeed because of card is used elsewhere
                throw new SmartCardInUseException(this);
            }
        }

        /// <summary>
        /// Reconnects to the card peforming a reset. A change of used protocol is possible
        /// </summary>
        public void ResetCard(Protocol protocol)
        {
            this.CheckRemoved();
            this.reader.ResetCard(protocol);
            this.Protocol = protocol;
        }


        public void Disconnect()
        {
            if (this.isConnected)
            {
                this.reader.DisconnectCard();
                this.IsConnected = false;
                this.Protocol = Protocol.Na;
            }
            else
            {
                throw new SmartCardNotConnectedException(this);
            }
        }

        public CardResponse Transmit(CardCommand command)
        {
            return this.Transmit(command, new UnsupportedVariableResolver());
        }

        public CardResponse Transmit(CardCommand command, IVariableResolver resolver)
        {
            this.CheckRemoved();
            this.CheckConnected();

            return this.reader.Transmit(command,resolver);
        }  

        public Task<CardResponse> TransmitAsync(CardCommand command)
        {
            return this.TransmitAsync(command, new UnsupportedVariableResolver());
        }

        public Task<CardResponse> TransmitAsync(CardCommand command, IVariableResolver resolver)
        {
            this.CheckRemoved();
            this.CheckConnected();

            return this.reader.TransmitAsync(command,resolver);
        }


        /// <summary>
        /// Implements <see cref="ISmartCard.CardReader"/>
        /// </summary>
        public ICardReader CardReader
        {
            get
            {
                this.CheckRemoved();

                return this.reader;
            }
        }

        /// <summary>
        /// Implements <see cref="ISmartCard.Atr"/>
        /// </summary>
        public byte[] Atr => this.reader.Atr;

        /// <summary>
        /// Implements <see cref="ISmartCard.Protocol"/>
        /// </summary>
        public Protocol Protocol
        {
            get
            {
                return this.protocol;
            }
            private set { this.SetAndInvoke(ref this.protocol, value); }
        }

        /// <summary>
        /// Implements <see cref="ISmartCard.State"/>
        /// </summary>
        public CardReaderState State => this.CardReader.State;

        /// <summary>
        /// Implements <see cref="ISmartCard.Eject"/>
        /// </summary>
        public void Eject()
        {
            this.CheckRemoved();

            if (this.isConnected)
            {
                this.reader.EjectCard();
                this.IsConnected = false;
            }
            else
            {
                throw new SmartCardNotConnectedException(this);
            }
        }

        /// <summary>
        /// Implements <see cref="ISmartCard.StateChanged"/>
        /// </summary>
        public event EventHandler<SmartCardEventArgs> StateChanged;

        /// <summary>
        /// Implements <see cref="ISmartCard.RemovedFromReader"/>
        /// </summary>
        public event EventHandler<SmartCardEventArgs> RemovedFromReader;

        /// <summary>
        /// Implements <see cref="ISmartCard.IsRemoved"/>
        /// </summary>
        public bool IsRemoved
        {
            get { return this.isRemoved; }
            private set { this.SetAndInvoke(ref this.isRemoved, value);}
        }

        /// <summary>
        /// Gets the name of the smart card
        /// </summary>
        public string Name => $"unknown card in {this.CardReader.FriendlyName}";

        public bool IsConnected
        {
            get { return this.isConnected; }
            private set { this.SetAndInvoke(ref this.isConnected, value); }
        }

        #endregion

        private void CheckRemoved()
        {
            if (this.isRemoved)
            {
                throw new SmartCardUnavailableException(this);
            }
        }

        private void CheckConnected()
        {
            if (!this.isConnected)
            {
                throw new SmartCardNotConnectedException(this);
            }
        }

        private void InvokeStateChanged()
        {
            if (this.StateChanged != null)
            {
                this.StateChanged(this, new SmartCardEventArgs(this));
            }

            this.InvokePropertyChanged(nameof(SmartCard.State));
        }

        private void InvokeRemovedFromReader()
        {
            if (this.RemovedFromReader != null)
            {
                this.RemovedFromReader(this, new SmartCardEventArgs(this));
            }
        }

        internal void NotifyRemoved()
        {
            this.IsRemoved = true;
            this.IsConnected = false;
            this.Protocol=Protocol.Na;
            this.InvokeRemovedFromReader();
        }

        internal void NotifyStateChanged()
        {
            this.InvokeStateChanged();
        }
    }

    internal class UnsupportedVariableResolver : IVariableResolver
    {
        public void ResolveVariable(Variable variable)
        {
            if( variable.IsResolved == false )
            {
                throw new InvalidOperationException("Variable content not supported");
            }
        }

        public void NotifyVariableEntryBegins(string name)
        {
        }

        public void NotifyVariableEntryEnded()
        {
        }
    }
}
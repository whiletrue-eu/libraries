using System;
using System.Drawing;
using System.Threading;
using WhileTrue.Classes.Utilities;
using WhileTrue.Components.CardReaderSubsystem.Base;
using WhileTrue.Facades.SmartCard;
using WhileTrue.Types.SmartCard;

namespace WhileTrue.Components.CardReaderSubsystem.SCP
{
// ReSharper disable InconsistentNaming
    internal class SCPCardReader : CardReaderBase, IDisposable
// ReSharper restore InconsistentNaming
    {
        private readonly PollThread pollThread;
        private readonly int port;
        private byte[] atr;
        private bool connected;
        private IntPtr handle;
        private int handleCount;
        private CardReaderState state = CardReaderState.Unknown;

        internal SCPCardReader(int port)
            : base("unknown SCP device")
        {
            this.port = port;
            this.handle = IntPtr.Zero;

            this.pollThread = new PollThread(this);
            this.pollThread.Start();
        }

        #region ICardReader Members

        public override ICardReaderConnectionInformation ReaderConnectionInformation
        {
            get { throw new System.NotImplementedException(); }
        }

        public override ISmartCardConnectionInformation CardConnectionInformation
        {
            get { throw new System.NotImplementedException(); }
        }

        public override bool CanUpdateConnectionInformation
        {
            get { throw new System.NotImplementedException(); }
        }

        public override void UpdateConnectionInformation()
        {
            throw new System.NotImplementedException();
        }

        public override CardReaderState State
        {
            get
            {
                this.BeginAtomic();
                CardReaderState State = this.state;
                this.EndAtomic();
                return State;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this.pollThread.Stop();
        }

        #endregion

        private bool ProbeDevice()
        {
            this.BeginAtomic();
            try
            {
                IntPtr Handle = this.AcquireHandle();

                try
                {
                    // Handle could be acquired. try to read out version string
                    this.FriendlyName = string.Format("{0} on COM{1}", SCPCommands.GetVersionString(Handle), this.port);
                }
                catch
                {
                    //Do nothing in case of error
                    return false;
                }
                finally
                {
                    this.ReleaseHandle();
                }
            }
            catch
            {
                //Do nothing in case of error
                return false;
            }
            finally
            {
                this.EndAtomic();
            }
            return true;
        }

        private void PollCardReaderState()
        {
            CardReaderState NewState = CardReaderState.Unknown;
            bool SimulateCardRemoved = false;

            this.BeginAtomic();
            try
            {
                IntPtr Handle = this.AcquireHandle();

                try
                {
                    // Get Card status
                    SCPCommands.CardStatus Status = SCPCommands.GetStatus(Handle);
                    switch (Status)
                    {
                        case SCPCommands.CardStatus.NoCard:
                            NewState = CardReaderState.NoCard;
                            break;
                        case SCPCommands.CardStatus.CardInsertedPowerOff:
                            NewState = CardReaderState.CardPresent;
                            break;
                        case SCPCommands.CardStatus.CardInsertedPowerOn:
                            NewState = CardReaderState.CardExclusivelyInUse;
                            break;
                        default:
                            NewState = CardReaderState.Unknown;
                            break;
                    }
                    // Get ATR
                    if (Status == SCPCommands.CardStatus.CardInsertedPowerOff) //do NOT check power on cards, because the ATR cannot chnage when the card is powered!
                    {
                        SCPCommands.SelectApplicationProtocol(Handle, SCAP.T0);
                        byte[] ATR = SCPCommands.PowerOn(Handle, false);
                        if (this.atr != null && !ATR.HasEqualValue(this.atr))
                        {
                            //Card was changed during polls -> simulate 'card removed' event
                            SimulateCardRemoved = true;
                        }
                        this.atr = ATR;
                        SCPCommands.PowerOff(Handle);
                    }
                    else if (Status == SCPCommands.CardStatus.CardInsertedPowerOn && this.handleCount == 0)
                    {
                        // the card is powered, but we didn't do it (is done at 'connect', but then the handle count must be greater)
                        NewState = CardReaderState.Unknown;
                    }
                }
// ReSharper disable EmptyGeneralCatchClause
                catch
                {
                    //Do nothing in case of error (next loop)
                }
                finally
                {
                    this.ReleaseHandle();
                }
            }
            catch
            {
                //Do nothing in case of error (next loop)
            }
            finally
            {
                this.EndAtomic();
            }
// ReSharper restore EmptyGeneralCatchClause


            if (SimulateCardRemoved)
            {
                this.state = CardReaderState.NoCard;
                this.InvokeStateChanged();
            }
            if (this.State != NewState)
            {
                this.state = NewState;
                this.InvokeStateChanged();
            }

            Thread.Sleep(1000);
        }

        private void BeginAtomic()
        {
            Monitor.Enter(this);
        }

        private void EndAtomic()
        {
            Monitor.Exit(this);
        }

        private IntPtr AcquireHandle()
        {
            IntPtr Handle;
            Monitor.Enter(this);
            try
            {
                if (this.handleCount == 0)
                {
                    int RetryConnectCount = 100;
                    while (this.handle == IntPtr.Zero)
                    {
                        try
                        {
                            this.handle = SCPCommands.Open(this.port);
                        }
                        catch
                        {
                            if (RetryConnectCount > 0)
                            {
                                Thread.Sleep(new Random().Next(1, 50));
                                RetryConnectCount--;
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                }

                this.handleCount++;
                Handle = this.handle;
            }
            finally
            {
                Monitor.Exit(this);
            }
            return Handle;
        }

        private void ReleaseHandle()
        {
            Monitor.Enter(this);
            try
            {
                this.handleCount--;

                if (this.handleCount == 0)
                {
                    SCPCommands.Close(this.handle);
                    this.handle = IntPtr.Zero;
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        #region CardReaderBase overrides

        protected internal override byte[] ATR
        {
            get
            {
                this.BeginAtomic();
                byte[] ATR = this.atr;
                this.EndAtomic();
                return ATR;
            }
        }

        protected override byte[] Transmit(byte[] data)
        {
            this.BeginAtomic();
            try
            {
                if (this.connected)
                {
                    IntPtr Handle = this.AcquireHandle();

                    byte[] Response = SCPCommands.SendAPDU(Handle, data);

                    this.ReleaseHandle();

                    return Response;
                }
                else
                {
                    throw new SmartCardNotConnectedException(this.SmartCard);
                }
            }
            finally
            {
                this.EndAtomic();
            }
        }

        protected internal override void ConnectCard(Protocol protocol)
        {
            this.BeginAtomic();
            try
            {
                IntPtr Handle;
                try
                {
                    Handle = this.AcquireHandle();
                }
                catch
                {
                    throw new SCPException(LowLevelError.CardCommunicationError);
                }

                SCAP SCPProtocol;
                switch (protocol)
                {
                    case Protocol.T0:
                        SCPProtocol = SCAP.T0;
                        break;
                    case Protocol.T1:
                        SCPProtocol = SCAP.T1;
                        break;
                    default:
                        throw new Exception("Unknown Protocol");
                }

                SCPCommands.SelectApplicationProtocol(Handle, SCPProtocol);
                SCPCommands.PowerOn(Handle, true);

                this.connected = true;
            }
            finally
            {
                this.EndAtomic();
            }
        }

        protected internal override void DisconnectCard()
        {
            this.BeginAtomic();
            try
            {
                if (this.connected)
                {
                    this.connected = false;
                    this.ReleaseHandle();
                }
            }
            finally
            {
                this.EndAtomic();
            }
        }

        protected internal override void ResetCard(Protocol protocol)
        {
            throw new System.NotImplementedException();
        }

        protected override void ResolveVariable(Variable variable, IVariableResolver resolver)
        {
            if (variable.Format == VariableFormat.Ascii)
            {
                if (variable.VerifyEntry == false)
                {
                    string Message = string.Format("Enter {0}\n(L:{1}-{2}) ", variable.Name, variable.MinLength, variable.Length);
                    variable.Value = this.GetVariableValue(Message, variable);
                }
                else
                {
                    do
                    {
                        string Message = string.Format("Enter {0}\n(L:{1}-{2}) ", variable.Name, variable.MinLength, variable.Length);
                        byte[] FirstPIN = this.GetVariableValue(Message, variable);
                        Message = string.Format("Re-Enter {0}\n(L:{1}-{2}) ", variable.Name, variable.MinLength, variable.Length);
                        byte[] SecondPIN = this.GetVariableValue(Message, variable);

                        if (FirstPIN.HasEqualValue(SecondPIN))
                        {
                            variable.Value = FirstPIN;
                        }
                        else
                        {
                            this.DisplayMessage(string.Format("Error: Value mismatch\nPlease try again."), TimeSpan.FromSeconds(5));
                        }
                    } while (true);
                }
            }
            else
            {
                base.ResolveVariable(variable,resolver);
            }
        }

        private byte[] GetVariableValue(string message, Variable variable)
        {
            this.BeginAtomic();
            try
            {
                IntPtr Handle = this.AcquireHandle();
                try
                {
                    Size DisplaySize = SCPCommands.InitDisplay(Handle);
                    SCPCommands.Display(Handle, message);

                    SCPCommands.SetCursor(Handle, new Point(DisplaySize.Width - variable.Length, 1));
                    SCPCommands.WriteOnDisplay(Handle, new string('_', variable.Length));
                    SCPCommands.SetCursor(Handle, new Point(DisplaySize.Width - variable.Length, 1));

                    SCPCommands.StartInput(Handle, variable.MinLength, variable.Length);
                    while (SCPCommands.GetInputHasEnded(Handle) == false)
                    {
                        Thread.Sleep(250);
                    }
                    SCPCommands.ClearDisplay(Handle);
                    return SCPCommands.EndInput(Handle);
                }
                finally
                {
                    this.ReleaseHandle();
                }
            }
            finally
            {
                this.EndAtomic();
            }
        }

        private void DisplayMessage(string message, TimeSpan timeout)
        {
            this.BeginAtomic();
            try
            {
                IntPtr Handle = this.AcquireHandle();
                try
                {
                    SCPCommands.Display(Handle, message);
                    Thread.Sleep(timeout);
                    SCPCommands.ClearDisplay(Handle);
                }
                finally
                {
                    this.ReleaseHandle();
                }
            }
            finally
            {
                this.EndAtomic();
            }
        }

        #endregion

        #region Nested type: PollThread

        private class PollThread : ThreadBase
        {
            private readonly SCPCardReader owner;
            private bool continueCardReaderStatePolling = true;

            public PollThread(SCPCardReader owner) : base("PCPSCPollThread", true)
            {
                this.owner = owner;
            }

            protected override void Run()
            {
                if (this.owner.ProbeDevice())
                {
                    while (this.continueCardReaderStatePolling)
                    {
                        this.owner.PollCardReaderState();
                    }
                }
            }


            public new void Stop()
            {
                this.continueCardReaderStatePolling = false;
                this.Join();
            }
        }

        #endregion
    }
}
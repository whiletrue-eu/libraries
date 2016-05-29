using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using WhileTrue.Classes.Components;
using WhileTrue.Classes.SCard;
using WhileTrue.Components.CardReaderSubsystem.Base;
using ThreadBase=WhileTrue.Classes.Utilities.ThreadBase;

namespace WhileTrue.Components.CardReaderSubsystem.PCSC
{
    ///<summary>
    /// Provides implementation for access to PCSC card readers
    ///</summary>
    [Component("Smart Card Service (PC/SC)")]
    public class PcscSmartCardSubsystem : CardReaderSubsystemBase, IDisposable
    {
        private readonly SCardApi scardApi;
        private readonly PollThread pollThread;
        private readonly ManualResetEvent synchronousStateUpdateInProgress = new ManualResetEvent(true);
        private readonly ManualResetEvent currentStatusUpdateFinished = new ManualResetEvent(true);

        ///<summary/>
        public PcscSmartCardSubsystem(Options options) : this(options, new SCardApi())
        {
            
        }

        private PcscSmartCardSubsystem(Options options, SCardApi scardApi)
        {
            this.scardApi = scardApi;
            this.pollThread = new PollThread(this, scardApi);
            this.pollThread.Start();
            
            if( options.SynchronousInitialisation )
            {
                this.pollThread.WaitForInitialisation();    
            }
        }

        /// <summary>
        /// Options for the PC/SC Smartcard Subsystem
        /// </summary>
        public class Options
        {
            /// <summary>
            /// if set to <c>true</c>, the initialisation of the reader list
            /// will be done synchronously during initialisation of the component.
            /// </summary>
            /// <remarks>
            /// Synchronuous initialisation guarantees that the current readers are directly listed by the smart card service.
            /// By default the readers are queried asynchronously, i.e. they may not directly available after instanciation 
            /// of the component. Note that synchrounous initialisation may block the calling thread until the reader list and status is read.
            /// </remarks>
            public bool SynchronousInitialisation { get; set; }
        }

        ///<summary/>
        public PcscSmartCardSubsystem() : this(new Options{SynchronousInitialisation = false})
        {
        }

        #region IDisposable Members

        ///<summary/>
        public void Dispose()
        {
            this.pollThread.Stop();
            this.scardApi.Dispose();
        }

        #endregion

        private void InsertNewCardReaders()
        {
            string[] CardReaderNames = this.scardApi.ListReaders();
            foreach (string CardReadername in CardReaderNames)
            {
                if (! this.Readers.ContainsName(CardReadername))
                {
                    PcscCardReader NewCardReader = this.CreateReader(CardReadername);
                    this.AddCardReader(NewCardReader);
                }
            }
        }

        private PcscCardReader CreateReader(string name)
        {
            if (name.StartsWith("ORGA MKT-5"))
            {
                return new Orgamkt5CardReader(this, this.scardApi, name);
            }
            else
            {
                return new PcscCardReader(this, this.scardApi, name);
            }
        }


        #region Nested type: PollThread

        private class PollThread : ThreadBase
        {
            private readonly PcscSmartCardSubsystem owner;
            private readonly SCardApi scardApi;
            private bool continueCardReaderStatePolling = true;

            private const string PnpNotificationDeviceName = "\\\\?PnP?\\Notification";
            private SCardReaderState pnpNotificationDeviceState = SCardReaderState.Unaware;

            public PollThread(PcscSmartCardSubsystem owner, SCardApi scardApi) : base()
            {
                this.owner = owner;
                this.scardApi = scardApi;
            }

            protected override void Initialise()
            {
                this.InitialSetupCardReaders();
                this.UpdateCardReaderStatus();
            }

            protected override void Run()
            {
                while (this.continueCardReaderStatePolling)
                {
                    this.owner.currentStatusUpdateFinished.Set();
                    try
                    {
                        this.owner.synchronousStateUpdateInProgress.WaitOne();
                        this.owner.currentStatusUpdateFinished.Reset();
                        this.UpdateCardReaderStatus();
                    }
                    catch (SCardException Error)
                    {
                        if (Error.Error == SCardError.Cancelled)
                        {
                            //If canceled, let check again. If cancelled to stop the thread, the continue... member will be set to false.
                            //Otherwise, a syncvhronus update request may be pending. In this case, the loop will wait at the synchronousStateUpdateInProgress event
                            continue;
                        }
                        else if (Error.Error == SCardError.SystemCancelled)
                        {
                            // This may have different causes: the smart card manager service was stopped, or
                            // it is redirected because of a terminal services session. In this case, refresh the card
                            // reader list and continue polling.
                            this.ResetCardReaderStates();
                            continue;
                        }
                        else if (Error.Error == SCardError.NoService || Error.Error == SCardError.UnknownInterface)
                        {
                            //wait for service to become available again
                            this.Sleep(1000);
                            continue;
                        }
                        else
                        {
                            Debug.Fail(Error.Message);
                        }
                    }
                    catch (Exception Error)
                    {
                        Debug.Fail(Error.Message);
                    }
                }
            }

            private void ResetCardReaderStates()
            {
                this.pnpNotificationDeviceState = SCardReaderState.Unaware;
                foreach (CardReaderBase CardReader in this.owner.Readers.ToArray())
                {
                    this.owner.RemoveCardReader(CardReader);
                }

                this.InitialSetupCardReaders();
            }

            private void InitialSetupCardReaders()
            {
                try
                {
                    this.owner.InsertNewCardReaders();
                }
                catch (SCardException)
                {
                    //Ignore errors. Most likely the service was shut down
                }
            }

            private void UpdateCardReaderStatus()
            {
                SCardCardReaderState[] CardReaderstates = new SCardCardReaderState[this.owner.Readers.Count + 1];

                for (int Index = 0; Index < this.owner.Readers.Count; Index++)
                {
                    CardReaderstates[Index] = ((PcscCardReader) this.owner.Readers[Index]).CardReaderState;
                }

                SCardCardReaderState PnpNotificationReaderstate = new SCardCardReaderState
                                                                   {
                                                                       szCardReader = PollThread.PnpNotificationDeviceName,
                                                                       dwCurrentState =
                                                                           this.pnpNotificationDeviceState
                                                                   };
                CardReaderstates[CardReaderstates.Length - 1] = PnpNotificationReaderstate;

                if (this.scardApi.GetStatusChange(-1, CardReaderstates))
                {
                    //Only update if not cancelled (at app end)
                    foreach (SCardCardReaderState Readerstate in CardReaderstates)
                    {
                        if ((Readerstate.dwEventState & SCardReaderState.Changed) != 0)
                        {
                            this.HandleCardReaderStateChange(Readerstate.szCardReader, Readerstate);
                        }
                    }
                }
            }

            private void HandleCardReaderStateChange(string cardReaderName, SCardCardReaderState state)
            {
                if (cardReaderName == PollThread.PnpNotificationDeviceName)
                {
                    this.pnpNotificationDeviceState = state.dwEventState;
                    this.owner.InsertNewCardReaders();
                }
                else
                {
                    this.owner.UpdateReaderState(state, cardReaderName);
                    return;
                }
            }



            public new void Stop()
            {
                this.continueCardReaderStatePolling = false;
                // ReSharper disable EmptyGeneralCatchClause
                try //try, as if the scardservice is not available, the call will trow an exception
                {
                    this.scardApi.Cancel();
                }
                catch{}
                // ReSharper restore EmptyGeneralCatchClause
                this.Join();
            }
        }

        #endregion

        private void UpdateReaderState(SCardCardReaderState state, string cardReaderName)
        {
            PcscCardReader CardReader = (PcscCardReader)this.Readers[cardReaderName];

            SCardReaderState NewState = state.dwEventState;
            if ((NewState & SCardReaderState.Unavailable) != 0 && this.scardApi.ListReaders().Contains(cardReaderName) == false)
            {
                //CardReader was removed, if state changes to unavailiable _and_ reader is not in the list anymore.
                //Otherwise, reader was opened in direct mode
                this.RemoveCardReader(CardReader);
                return;
            }
            else
            {
                try
                {
                    // CardReader state changed
                    CardReader.CardReaderState = state;
                    return;
                }
                catch (Exception Error)
                {
                    Trace.Fail($"Exception was thrown inside status change handler. This may lead to a blocking application! Origianl Exception was: {Error.Message}");
                    throw;
                }
            }
        }

        internal void UpdateReaderStates()
        {
            //suspend poll thread & wait until current update is finished
            this.synchronousStateUpdateInProgress.Reset();
            this.scardApi.Cancel();
            this.currentStatusUpdateFinished.WaitOne();

            SCardCardReaderState[] CardReaderstates = new SCardCardReaderState[this.Readers.Count];

            for (int Index = 0; Index < this.Readers.Count; Index++)
            {
                CardReaderstates[Index] = ((PcscCardReader)this.Readers[Index]).CardReaderState;
                CardReaderstates[Index].dwCurrentState = SCardReaderState.Unaware;
            }

            if (this.scardApi.GetStatusChange(-1, CardReaderstates))
            {
                //Only update if not cancelled (at app end)
                foreach (SCardCardReaderState Readerstate in CardReaderstates)
                {
                    if ((Readerstate.dwEventState & SCardReaderState.Changed) != 0)
                    {
                        this.UpdateReaderState(Readerstate, Readerstate.szCardReader);
                    }
                }
            }

            //resume polling thread
            this.synchronousStateUpdateInProgress.Set();
        }
    }
}
using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using WhileTrue.Classes.Commanding;
using WhileTrue.Classes.Components;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;
using WhileTrue.Common.Facades.CommonDialogs;
using WhileTrue.Facades.SmartCard;
using WhileTrue.Facades.SmartCardUI;
using WhileTrue.Types.SmartCard;
#if NET4x
using System.Threading.Tasks;
#endif


namespace WhileTrue.SmartCard.Modules.APDUSender
{
    [Component]
    internal class APDUSenderModel : ObservableObject, IAPDUSenderModel, IDisposable
    {
        private readonly ISmartCardUIProvider smartCardUIProvider;
        private readonly ISmartCardService smartCardService;
        private readonly ICommonDialogProvider commonDialogProvider;
        private ICardReader cardReader;
        private string lastCommand;
        private string lastResponse;
        private string command;
        private readonly ReadOnlyPropertyAdapter<UnavailableState> unavailableStateAdapter;
        private readonly ReadOnlyPropertyAdapter<bool> isAvailableAdapter;
        private readonly ReadOnlyPropertyAdapter<string> smartCardATRAdapter;
        private readonly ReadOnlyPropertyAdapter<bool> hasLastCommandAdapter;
        private readonly ReadOnlyPropertyAdapter<bool> isSmartCardConnectedAdapter;
        private readonly ICommand selectReaderCommand;
        private readonly DelegateCommand sendAPDUCommand;
        private readonly DelegateCommand connectT0Command;
        private readonly DelegateCommand connectT1Command;
        private readonly DelegateCommand disconnectCommand;
        private bool isSending;

        /// <summary/>
        public APDUSenderModel(ISmartCardUIProvider smartCardUIProvider, ISmartCardService smartCardService, ICommonDialogProvider commonDialogProvider)
        {
            this.smartCardUIProvider = smartCardUIProvider;
            this.smartCardService = smartCardService;
            this.commonDialogProvider = commonDialogProvider;

            this.unavailableStateAdapter = this.CreatePropertyAdapter(
                ()=>UnavailableState,
                ()=>this.CardReader == null ? UnavailableState.NoCardReaderSelected :
                    this.CardReader.SmartCard == null ? UnavailableState.NoSmartCardInReader : 
                    UnavailableState.Available,
                EventBindingMode.Weak, ValueRetrievalMode.Lazy
                );

            this.isAvailableAdapter = this.CreatePropertyAdapter(
                () => IsAvailable,
                () => this.UnavailableState == UnavailableState.Available,
                EventBindingMode.Weak, ValueRetrievalMode.Lazy
                );

            this.smartCardATRAdapter = this.CreatePropertyAdapter(
                () => SmartCardATR,
                ()=>this.CardReader!=null?
                    this.CardReader.SmartCard!=null?
                    this.CardReader.SmartCard.ATR.ToHexString(" "):
                    "":"",
                    EventBindingMode.Weak, ValueRetrievalMode.Lazy);

            this.isSmartCardConnectedAdapter = this.CreatePropertyAdapter(
                () => IsSmartCardConnected,
                () => this.CardReader != null ?
                    this.CardReader.SmartCard != null ?
                    this.CardReader.SmartCard.IsConnected :
                    false : false,
                    EventBindingMode.Weak, ValueRetrievalMode.Lazy);

            this.hasLastCommandAdapter = this.CreatePropertyAdapter(
                ()=>HasLastCommand,
                ()=>this.LastCommand != null,
                EventBindingMode.Weak,ValueRetrievalMode.Lazy
                );

            this.selectReaderCommand = new DelegateCommand(this.SelectReader);
            this.sendAPDUCommand = new DelegateCommand(this.SendCommand, ()=>this.HasErrors("Command")==false && this.IsSmartCardConnected, EventBindingMode.Weak);
            this.connectT0Command = new DelegateCommand(()=>this.Connect(Protocol.T0), ()=>this.IsSmartCardConnected==false, EventBindingMode.Weak);
            this.connectT1Command = new DelegateCommand(() => this.Connect(Protocol.T1), () => this.IsSmartCardConnected == false, EventBindingMode.Weak);
            this.disconnectCommand = new DelegateCommand(this.Disconnect, ()=>this.IsSmartCardConnected);

            this.AddValidationForProperty(() => Command)
                .AddValidation(value => string.IsNullOrEmpty(value) == false, value=> new ValidationMessage(ValidationSeverity.Info, "Please enter an APDU command to send"))
                .AddValidation(value => string.IsNullOrEmpty(value) || value.CanConvertToByteArray(), value=>"Command is not a valid hexadecimal string");
        }

        public APDUSenderModel(ISmartCardUIProvider smartCardUIProvider, WhileTrue.Modules.ModelInspector.IModelInspector modelInspector, ISmartCardService smartCardService, ICommonDialogProvider commonDialogProvider)
            : this(smartCardUIProvider, smartCardService, commonDialogProvider)
        {
            modelInspector.Inspect(this, "APDU Sender Model");
            this.smartCardService = smartCardService;
        }

        public bool IsSmartCardConnected
        {
            get { return this.isSmartCardConnectedAdapter.GetValue(); }
        }

        public bool HasLastCommand
        {
            get { return this.hasLastCommandAdapter.GetValue(); }
        }


        public string LastCommand
        {
            get { return this.lastCommand; }
        }


        public string LastResponse
        {
            get { return this.lastResponse; }
        }


        public string Command
        {
            get { return this.command; }
            set { this.SetAndInvoke(()=>Command, ref this.command, value); }
        }

        public string SmartCardATR
        {
            get { return this.smartCardATRAdapter.GetValue(); }
        }

#if DEBUG
#endif

        public UnavailableState UnavailableState
        {
            get { return this.unavailableStateAdapter.GetValue(); }
        }

        public bool IsAvailable
        {
            get { return this.isAvailableAdapter.GetValue(); }
        }

        public ICommand SelectReaderCommand
        {
            get
            {
                return this.selectReaderCommand;
            }
        }

        public DelegateCommand SendAPDUCommand
        {
            get
            {
                return this.sendAPDUCommand;
            }
        }

        public DelegateCommand ConnectT0Command
        {
            get { return this.connectT0Command; }
        }

        public DelegateCommand ConnectT1Command
        {
            get { return this.connectT1Command; }
        }

        public DelegateCommand DisconnectCommand
        {
            get { return this.disconnectCommand; }
        }

#if NET35  
        private void Connect(Protocol protocol)
        {
            this.CardReader.SmartCard.Connect(protocol);
        }
#elif NET40
        private void Connect(Protocol protocol)
        {
            this.IsSending = true;
            Task ConnectTask = this.CardReader.SmartCard.ConnectAsync(protocol);
            ConnectTask.ContinueWith(delegate
                {
                    if (ConnectTask.IsFaulted)
                    {
                        this.commonDialogProvider.ShowError(ConnectTask.Exception);
                    }
                    this.IsSending = false;
                });
        }
#elif NET45
        private async void Connect(Protocol protocol)
        {
            try
            {
                this.IsSending = true;
                await this.CardReader.SmartCard.ConnectAsync(protocol);
            }
            catch (Exception Error)
            {
                this.commonDialogProvider.ShowError(Error);
            }
            finally
            {
                this.IsSending = false;
            }
        }
#endif

        public bool IsSending
        {
            get
            {
                return this.isSending;
            }
            set
            {
                this.SetAndInvoke(()=>IsSending, ref this.isSending, value);
            }
        }

        private void Disconnect()
        {
            try
            {
                this.CardReader.SmartCard.Disconnect();
            }
            catch (Exception Exception)
            {
                this.commonDialogProvider.ShowError(Exception);
            }
        }

#if !NET45
        private void SendCommand()
#else
        private async void SendCommand()
#endif
        {
            byte[] CommandData = this.Command.ToByteArray();
            CardCommand Command = new CardCommand(CommandData);

#if NET35
            CardResponse Response = this.CardReader.SmartCard.Transmit(Command);
            this.SetAndInvoke(() => this.LastCommand, ref this.lastCommand, Command.ToString());
            this.SetAndInvoke(() => this.LastResponse, ref this.lastResponse, Response.ToString());
#elif NET40
            this.IsSending = true;
            Task<CardResponse> TransmitTask = this.CardReader.SmartCard.TransmitAsync(Command);
            TransmitTask.ContinueWith(delegate
                {
                    if (TransmitTask.IsFaulted)
                    {
                        this.commonDialogProvider.ShowError(TransmitTask.Exception);
                    }
                    else
                    {
                        this.SetAndInvoke(() => this.LastCommand, ref this.lastCommand, Command.ToString());
                        this.SetAndInvoke(() => this.LastResponse, ref this.lastResponse, TransmitTask.Result.ToString());
                    }
                    this.IsSending = false;
                });
#else
            try
            {
                this.IsSending = true;
                CardResponse Response = await this.CardReader.SmartCard.TransmitAsync(Command);
                this.SetAndInvoke(() => this.LastCommand, ref this.lastCommand, Command.ToString());
                this.SetAndInvoke(() => this.LastResponse, ref this.lastResponse, Response.ToString());
            }
            catch (Exception Error)
            {
            }
            finally
            {
                this.IsSending = false;
            }
#endif
        }

        private void SelectReader()
        {
            lock (this)
            {
                try
                {
                    this.CardReader = this.smartCardUIProvider.SelectCardReader(this.smartCardService, "Please select a card reader");
                    this.CardReader.Removed += this.CardReader_Removed;
                }
                catch (UserCancelException)
                {
                    this.CardReader = null;
                }
            }
        }

        private void CardReader_Removed(object sender, EventArgs e)
        {
            lock (this)
            {
                this.CardReader.Removed -= this.CardReader_Removed;
                this.CardReader = null;
            }
        }

        public ICardReader CardReader
        {
            get { return this.cardReader; }
            set { this.SetAndInvoke(() => CardReader, ref this.cardReader, value); }
        }

        public void Dispose()
        {
            //to avoid memory leaks
            if (this.CardReader != null)
            {
                this.CardReader.Removed -= this.CardReader_Removed;
            }
        }
    }

    internal enum UnavailableState
    {
        NoCardReaderSelected,
        NoSmartCardInReader,
        Available
    }
}
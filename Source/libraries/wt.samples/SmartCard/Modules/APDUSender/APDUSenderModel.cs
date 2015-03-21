using System;
using System.Windows.Input;
using WhileTrue.Classes.Commands;
using WhileTrue.Classes.Components;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;
using WhileTrue.Common.Facades.CommonDialogs;
using WhileTrue.Facades.SmartCard;
using WhileTrue.Facades.SmartCardUI;
using WhileTrue.Types.SmartCard;

namespace WhileTrue.SmartCard.Modules.APDUSender
{
    [Component]
    internal class ApduSenderModel : ObservableObject, IApduSenderModel, IDisposable
    {
        private readonly ISmartCardUiProvider smartCardUiProvider;
        private readonly ISmartCardService smartCardService;
        private readonly ICommonDialogProvider commonDialogProvider;
        private ICardReader cardReader;
        private string lastCommand;
        private string lastResponse;
        private string command;
        private readonly ReadOnlyPropertyAdapter<UnavailableState> unavailableStateAdapter;
        private readonly ReadOnlyPropertyAdapter<bool> isAvailableAdapter;
        private readonly ReadOnlyPropertyAdapter<string> smartCardAtrAdapter;
        private readonly ReadOnlyPropertyAdapter<bool> hasLastCommandAdapter;
        private readonly ReadOnlyPropertyAdapter<bool> isSmartCardConnectedAdapter;
        private bool isSending;

        /// <summary/>
        public ApduSenderModel(ISmartCardUiProvider smartCardUiProvider, ISmartCardService smartCardService, ICommonDialogProvider commonDialogProvider)
        {
            this.smartCardUiProvider = smartCardUiProvider;
            this.smartCardService = smartCardService;
            this.commonDialogProvider = commonDialogProvider;

            this.unavailableStateAdapter = this.CreatePropertyAdapter(
                nameof(ApduSenderModel.UnavailableState),
                ()=>this.CardReader == null ? UnavailableState.NoCardReaderSelected :
                    this.CardReader.SmartCard == null ? UnavailableState.NoSmartCardInReader : UnavailableState.Available
                );

            this.isAvailableAdapter = this.CreatePropertyAdapter(
                nameof(ApduSenderModel.IsAvailable),
                () => this.UnavailableState == UnavailableState.Available
                );

            this.smartCardAtrAdapter = this.CreatePropertyAdapter(
                nameof(this.SmartCardAtr),
                ()=>this.CardReader!=null?
                    this.CardReader.SmartCard!=null?
                    this.CardReader.SmartCard.Atr.ToHexString(" "):"":""
                    );

            this.isSmartCardConnectedAdapter = this.CreatePropertyAdapter(
                nameof(ApduSenderModel.IsSmartCardConnected),
                () => this.CardReader != null ?
                    this.CardReader.SmartCard != null ?
                    this.CardReader.SmartCard.IsConnected :false : false
                    );

            this.hasLastCommandAdapter = this.CreatePropertyAdapter(
                nameof(ApduSenderModel.HasLastCommand),
                ()=>this.LastCommand != null
                );

            this.SelectReaderCommand = new DelegateCommand(this.SelectReader);
            this.SendApduCommand = new DelegateCommand(this.SendCommand, ()=>this.HasErrors("Command")==false && this.IsSmartCardConnected);
            this.ConnectT0Command = new DelegateCommand(()=>this.Connect(Protocol.T0), ()=>this.IsSmartCardConnected==false);
            this.ConnectT1Command = new DelegateCommand(() => this.Connect(Protocol.T1), () => this.IsSmartCardConnected == false);
            this.DisconnectCommand = new DelegateCommand(this.Disconnect, ()=>this.IsSmartCardConnected);

            this.AddValidationForProperty(() => this.Command)
                .AddValidation(value => string.IsNullOrEmpty(value) == false, value=> new ValidationMessage(ValidationSeverity.Info, "Please enter an APDU command to send"))
                .AddValidation(value => string.IsNullOrEmpty(value) || value.CanConvertToByteArray(), value=>"Command is not a valid hexadecimal string");
        }

        public ApduSenderModel(ISmartCardUiProvider smartCardUiProvider, WhileTrue.Modules.ModelInspector.IModelInspector modelInspector, ISmartCardService smartCardService, ICommonDialogProvider commonDialogProvider)
            : this(smartCardUiProvider, smartCardService, commonDialogProvider)
        {
            modelInspector.Inspect(this, "APDU Sender Model");
            this.smartCardService = smartCardService;
        }

        public bool IsSmartCardConnected => this.isSmartCardConnectedAdapter.GetValue();

        public bool HasLastCommand => this.hasLastCommandAdapter.GetValue();


        public string LastCommand => this.lastCommand;


        public string LastResponse => this.lastResponse;


        public string Command
        {
            get { return this.command; }
            set { this.SetAndInvoke(ref this.command, value); }
        }

        public string SmartCardAtr => this.smartCardAtrAdapter.GetValue();

        public UnavailableState UnavailableState => this.unavailableStateAdapter.GetValue();

        public bool IsAvailable => this.isAvailableAdapter.GetValue();

        public ICommand SelectReaderCommand { get; }

        public ICommand SendApduCommand { get; }

        public ICommand ConnectT0Command { get; }

        public ICommand ConnectT1Command { get; }

        public ICommand DisconnectCommand { get; }

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

        public bool IsSending
        {
            get
            {
                return this.isSending;
            }
            set
            {
                this.SetAndInvoke(ref this.isSending, value);
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

        private async void SendCommand()
        {
            byte[] CommandData = this.Command.ToByteArray();
            CardCommand Command = new CardCommand(CommandData);


            try
            {
                this.IsSending = true;
                CardResponse Response = await this.CardReader.SmartCard.TransmitAsync(Command);
                this.SetAndInvoke(nameof(this.LastCommand), ref this.lastCommand, Command.ToString());
                this.SetAndInvoke(nameof(this.LastResponse), ref this.lastResponse, Response.ToString());
            }
            catch 
            {
                //Ignore errors in this sample
            }
            finally
            {
                this.IsSending = false;
            }

        }

        private void SelectReader()
        {
            lock (this)
            {
                try
                {
                    this.CardReader = this.smartCardUiProvider.SelectCardReader(this.smartCardService, "Please select a card reader");
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
            set { this.SetAndInvoke(ref this.cardReader, value); }
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
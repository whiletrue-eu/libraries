using System;
using System.Text;
using WhileTrue.Classes.Utilities;
using WhileTrue.Classes.SCard;
using WhileTrue.Components.CardReaderSubsystem.Base;
using WhileTrue.Facades.SmartCard;

namespace WhileTrue.Components.CardReaderSubsystem.PCSC
{
    internal class PcscCardReader : CardReaderBase
    {
        private readonly PcscSmartCardSubsystem pcscSubsystem;
        private readonly SCardApi scardApi;
        private SCardCardReaderState cardReaderState;
        private PcscCardReaderConnectionInformation readerConnectionInformation;
        private PcscSmartCardConnectionInformation cardConnectionInformation;

        internal PcscCardReader(PcscSmartCardSubsystem pcscSubsystem, SCardApi scardApi, string name)
            : base(name)
        {
            this.pcscSubsystem = pcscSubsystem;
            this.scardApi = scardApi;
            this.cardReaderState = new SCardCardReaderState
                                   {
                                       dwCurrentState = SCardReaderState.Unaware,
                                       szCardReader = name
                                   };
        }

        protected IntPtr CardHandle { get; private set; } = IntPtr.Zero;

        internal SCardCardReaderState CardReaderState
        {
            set
            {
                if ((this.cardReaderState.dwCurrentState & SCardReaderState.Mask) != (value.dwEventState & SCardReaderState.Mask))
                {
                    this.cardReaderState = value;
                    this.cardReaderState.dwCurrentState = this.cardReaderState.dwEventState;

                    this.SetConnectionInformation(null, null);

                    this.InvokeStateChanged();
                    this.InvokePropertyChanged(nameof(PcscCardReader.CanUpdateConnectionInformation));
                }
                else
                {
                    this.cardReaderState = value;
                    this.cardReaderState.dwCurrentState = this.cardReaderState.dwEventState;
                }
            }
            get { return this.cardReaderState; }
        }

        private SCardReaderState ScardState => this.cardReaderState.dwCurrentState;

        private bool? CanEject
        {
            get
            {
                try
                {
                    byte[] Attribute = this.scardApi.GetAttribute(this.CardHandle, SCardAttributes.Characteristics);
                    uint Characteristics = (uint)(Attribute[3] << 24 | Attribute[2] << 8 | Attribute[1] << 16 | Attribute[0]);

                    return (Characteristics & (uint)SCardCharacteristics.Eject) == (uint)SCardCharacteristics.Eject;
                }
                catch
                {
                    return null;
                }
            }
        }

        #region CardReaderBase overrides

        protected internal override byte[] Atr
        {
            get
            {
                if (this.CardReaderState.rgbAtr != null)
                {
                    byte[] Atr = new byte[this.CardReaderState.cbAtr];
                    Array.Copy(this.CardReaderState.rgbAtr, 0, Atr, 0, this.CardReaderState.cbAtr);
                    return Atr;
                }
                else
                {
                    return null;
                }
            }
        }

        protected override byte[] Transmit(byte[] data)
        {
            return this.scardApi.Transmit(this.CardHandle, data);
        }

        protected internal override void ConnectCard(Protocol protocol)
        {
            if( this.CardHandle != IntPtr.Zero )
            {
                throw new InvalidOperationException("Smart Card already connected");
            }

            try
            {
                this.CardHandle = this.scardApi.Connect(this.Name, SCardShareMode.Exclusive, PcscCardReader.ProtocolToScardProtocol(protocol));
            }
            catch (SCardException Exception)
            {
                switch (Exception.Error)
                {
                    case SCardError.RequestNotSupported:
                    case SCardError.ProtocolMismatch:
                        throw new ProtocolNotSupportedException(this.SmartCard, protocol);
                    case SCardError.SharingViolation:
                        throw new SmartCardInUseException(this.SmartCard);
                    default:
                        throw;
                }
            }
        }

        private static SCardProtocol ProtocolToScardProtocol(Protocol protocol)
        {
            SCardProtocol SCardProtocol;
            switch (protocol)
            {
                case Protocol.T0:
                    SCardProtocol = SCardProtocol.T0;
                    break;
                case Protocol.T1:
                    SCardProtocol = SCardProtocol.T1;
                    break;
                default:
                    throw new Exception("Unknown Protocol");
            }
            return SCardProtocol;
        }

        protected internal override void DisconnectCard()
        {
            if (this.CardHandle != IntPtr.Zero)
            {
                this.scardApi.Disconnect(this.CardHandle, SCardDisposition.Unpower);
                this.CardHandle = IntPtr.Zero;
            }
        }

        protected internal override void ResetCard(Protocol protocol)
        {
            if (this.CardHandle != IntPtr.Zero)
            {
                this.scardApi.Reconnect(this.CardHandle, SCardDisposition.Reset, SCardShareMode.Exclusive, PcscCardReader.ProtocolToScardProtocol(protocol));
            }
        }
        protected internal override void EjectCard()
        {
            if (this.CanEject.GetValueOrDefault(false))
            {
                if (this.CardHandle != IntPtr.Zero)
                {
                    this.scardApi.Disconnect(this.CardHandle, SCardDisposition.Eject);
                    this.CardHandle = IntPtr.Zero;
                }
            }
            else
            {
                base.EjectCard();
            }
        }

        #endregion

        #region ICardReader Members

        public override ICardReaderConnectionInformation ReaderConnectionInformation
        {
            get
            {
                if (this.readerConnectionInformation == null && this.CardHandle != IntPtr.Zero)
                {   //If we are connected, we can easily update the reader infos. If not, it must be done manually using the cor. method
                    this.SetConnectionInformation(new PcscCardReaderConnectionInformation(this.CardHandle, this.scardApi), new PcscSmartCardConnectionInformation(this.CardHandle, this.scardApi));
                }
                return this.readerConnectionInformation;
            }
        }     
        
        public override ISmartCardConnectionInformation CardConnectionInformation
        {
            get
            {
                if ( this.cardConnectionInformation == null && this.CardHandle != IntPtr.Zero)
                {   //If we are connected, we can easily update the reader infos. If not, it must be done manually using the cor. method
                    this.SetConnectionInformation(new PcscCardReaderConnectionInformation(this.CardHandle, this.scardApi), new PcscSmartCardConnectionInformation(this.CardHandle, this.scardApi));
                }
                return this.cardConnectionInformation;
            }
        }

        public override bool CanUpdateConnectionInformation
        {
            get { 
                switch( this.State )
                {
                    case Facades.SmartCard.CardReaderState.Unknown:
                        return false; //Unknown state, better not connect...
                    case Facades.SmartCard.CardReaderState.NoCard:
                    case Facades.SmartCard.CardReaderState.CardMute:
                        return true; //No card or inaccessible card. Try to connect to reader in raw mode
                    case Facades.SmartCard.CardReaderState.CardPresent:
                    case Facades.SmartCard.CardReaderState.CardInUse:
                        return true; //card is inserted but not exclusively used. Connect card possible
                    case Facades.SmartCard.CardReaderState.CardExclusivelyInUse:
                        return this.CardHandle != IntPtr.Zero; //Possible, if we are connected, but not, if another app is connected
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public override void UpdateConnectionInformation()
        {
            IntPtr CardHandle;
            PcscCardReaderConnectionInformation ReaderInformation;
            PcscSmartCardConnectionInformation CardInformation;
            switch (this.State)
            {
                case Facades.SmartCard.CardReaderState.Unknown:
                    //Unknown state, better not connect...
                    throw new CardReaderUnavailableException(this);

                case Facades.SmartCard.CardReaderState.NoCard:
                case Facades.SmartCard.CardReaderState.CardMute:
                    //No card or inaccessible card. Try to connect to reader in raw mode.
                    CardHandle = this.scardApi.Connect(this.Name, SCardShareMode.Direct, SCardProtocol.None);
                    ReaderInformation = new PcscCardReaderConnectionInformation(CardHandle, this.scardApi);
                    CardInformation = null;
                    this.scardApi.Disconnect(CardHandle, SCardDisposition.Leave);
                    break;

                case Facades.SmartCard.CardReaderState.CardPresent:
                case Facades.SmartCard.CardReaderState.CardInUse:
                    //card is inserted but not exclusively used. Connect card possible (or just read info if already connected)
                    if (this.CardHandle != IntPtr.Zero)
                    {
                        ReaderInformation = new PcscCardReaderConnectionInformation(this.CardHandle, this.scardApi);
                        CardInformation = new PcscSmartCardConnectionInformation(this.CardHandle, this.scardApi);
                    }
                    else
                    {
                        try
                        {
                            CardHandle = this.scardApi.Connect(this.Name, SCardShareMode.Shared, SCardProtocol.All);
                            ReaderInformation = new PcscCardReaderConnectionInformation(CardHandle, this.scardApi);
                            CardInformation = new PcscSmartCardConnectionInformation(CardHandle, this.scardApi);
                            this.scardApi.Disconnect(CardHandle, SCardDisposition.Leave);
                        }
                        catch (Exception)
                        {
                            throw new CardReaderUnavailableException(this);
                        }
                    }
                    break;

                case Facades.SmartCard.CardReaderState.CardExclusivelyInUse:
                    //Possible, if we are connected, but not, if another app is connected
                    if (this.CardHandle != IntPtr.Zero)
                    {
                        ReaderInformation = new PcscCardReaderConnectionInformation(this.CardHandle, this.scardApi);
                        CardInformation = new PcscSmartCardConnectionInformation(this.CardHandle, this.scardApi);
                    }
                    else
                    {
                        throw new CardReaderUnavailableException(this);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            this.pcscSubsystem.UpdateReaderStates();
            this.SetConnectionInformation(ReaderInformation, CardInformation);
        }

        private void SetConnectionInformation(PcscCardReaderConnectionInformation readerInformation, PcscSmartCardConnectionInformation cardInformation)
        {
            this.SetAndInvoke(nameof(PcscCardReader.ReaderConnectionInformation), ref this.readerConnectionInformation, readerInformation);
            this.SetAndInvoke(nameof(PcscCardReader.CardConnectionInformation), ref this.cardConnectionInformation, cardInformation);
        }

        public override CardReaderState State
        {
            get
            {
                if ((this.ScardState & SCardReaderState.Empty) != 0)
                {
                    return Facades.SmartCard.CardReaderState.NoCard;
                }
                else if ((this.ScardState & SCardReaderState.Exclusive) != 0)
                {   //check exclusive _before_ inuse, because with exclusive, inuse is also set!
                    return Facades.SmartCard.CardReaderState.CardExclusivelyInUse;
                }
                else if ((this.ScardState & SCardReaderState.InUse) != 0) 
                {
                    return Facades.SmartCard.CardReaderState.CardInUse;
                }
                else if ((this.ScardState & SCardReaderState.Mute) != 0)
                {
                    return Facades.SmartCard.CardReaderState.CardMute;
                }
                else if ((this.ScardState & SCardReaderState.Present) != 0)
                {
                    return Facades.SmartCard.CardReaderState.CardPresent;
                }         
                else
                {
                    return Facades.SmartCard.CardReaderState.Unknown;
                }
            }
        }

        #endregion
        private class PcscCardReaderConnectionInformation : ICardReaderConnectionInformation
        {
            public PcscCardReaderConnectionInformation(IntPtr readerHandle, SCardApi scardApi)
            {
                string SystemName =
                    Encoding.Unicode.GetString(scardApi.GetAttribute(readerHandle, SCardAttributes.DeviceSystemName).GetSubArray(0, -2));//remove trailing null character
                this.SystemName = SystemName;

                uint ChannelInfo = scardApi.GetAttribute(readerHandle, SCardAttributes.ChannelID).ToUInt32();
                this.Channel = PcscCardReaderConnectionInformation.GetChannelString(ChannelInfo);

                byte Characteristics = scardApi.GetAttribute(readerHandle, SCardAttributes.Characteristics)[0];
                this.SupportsSwallowing = Characteristics.IsBitSet(1);
                this.SupportsEject = Characteristics.IsBitSet(2);
                this.SupportsCapture = Characteristics.IsBitSet(3);

                this.DefaultClockRate = PcscCardReaderConnectionInformation.ToUInt32(scardApi.GetAttribute(readerHandle, SCardAttributes.DefaultClock));
                this.DefaultDataRate = PcscCardReaderConnectionInformation.ToUInt32(scardApi.GetAttribute(readerHandle, SCardAttributes.DefaultDataRate));
            }

            private static uint? ToUInt32(byte[] value)
            {
                return value?.ToUInt32();
            }

            private static string GetChannelString(uint channelInfo)
            {
                ushort ChannelNo = channelInfo.GetLoUShort();
                switch (channelInfo.GetHiUShort())
                {
                    case 0x01:
                        return $"COM {ChannelNo}";
                    case 0x02:
                        return $"LPT {ChannelNo}";
                    case 0x04:
                        return "PS/2 keyboard";
                    case 0x08:
                        return $"SCSI (ID {ChannelNo})";
                    case 0x10:
                        return $"ID (Device {ChannelNo})";
                    case 0x20:
                        return $"USB (Device {ChannelNo})";
                    case 0xF0:
                        return $"Vendor specific (0/{ChannelNo})";
                    case 0xF1:
                        return $"Vendor specific (1/{ChannelNo})";
                    case 0xF2:
                        return $"Vendor specific (2/{ChannelNo})";
                    case 0xF3:
                        return $"Vendor specific (3/{ChannelNo})";
                    case 0xF4:
                        return $"Vendor specific (4/{ChannelNo})";
                    case 0xF5:
                        return $"Vendor specific (5/{ChannelNo})";
                    case 0xF6:
                        return $"Vendor specific (6/{ChannelNo})";
                    case 0xF7:
                        return $"Vendor specific (7/{ChannelNo})";
                    case 0xF8:
                        return $"Vendor specific (8/{ChannelNo})";
                    case 0xF9:
                        return $"Vendor specific (9/{ChannelNo})";
                    case 0xFA:
                        return $"Vendor specific (A/{ChannelNo})";
                    case 0xFB:
                        return $"Vendor specific (B/{ChannelNo})";
                    case 0xFC:
                        return $"Vendor specific (C/{ChannelNo})";
                    case 0xFD:
                        return $"Vendor specific (D/{ChannelNo})";
                    case 0xFE:
                        return $"Vendor specific (E/{ChannelNo})";
                    case 0xFF:
                        return $"Vendor specific (F/{ChannelNo})";
                    default:
                        return "unknown";
                }
            }

            /// <summary>
            /// Gets the system name of the reader
            /// </summary>
            public string SystemName { get; }

            /// <summary>
            /// Gets the type of channel the reader is connected to
            /// </summary>
            public string Channel { get; }

            /// <summary>
            /// Gets whether ther reader supports card swallowing
            /// </summary>
            public bool SupportsSwallowing { get; }

            /// <summary>
            /// Gets whether ther reader supports card capturing
            /// </summary>
            public bool SupportsCapture { get; }

            /// <summary>
            /// Gets whether ther reader supports card ejection
            /// </summary>
            public bool SupportsEject { get; }
            
            /// <summary>
            /// Gets the Default clock rate, in kHz.
            /// </summary>
            public uint? DefaultClockRate { get; }

            /// <summary>
            /// Gets the Default data rate, in bps.
            /// </summary>
            public uint? DefaultDataRate { get; }
        }
        private class PcscSmartCardConnectionInformation : ISmartCardConnectionInformation
        {
            public PcscSmartCardConnectionInformation(IntPtr readerHandle, SCardApi scardApi)
            {
                this.CurrentBlockWaitingTime = PcscSmartCardConnectionInformation.ToUInt32(scardApi.GetAttribute(readerHandle, SCardAttributes.CurrentBWT));
                this.CurrentCharacterWaitingTime = PcscSmartCardConnectionInformation.ToUInt32(scardApi.GetAttribute(readerHandle, SCardAttributes.CurrentBWT));
                this.CurrentClockRate = PcscSmartCardConnectionInformation.ToUInt32(scardApi.GetAttribute(readerHandle, SCardAttributes.CurrentClock));
                this.CurrentD = PcscSmartCardConnectionInformation.ToUInt32(scardApi.GetAttribute(readerHandle, SCardAttributes.CurrentD));
                this.CurrentEbcEncoding = PcscSmartCardConnectionInformation.ToEcbEncoding(scardApi.GetAttribute(readerHandle, SCardAttributes.CurrentEBCEncoding));
                this.CurrentF = PcscSmartCardConnectionInformation.ToUInt32(scardApi.GetAttribute(readerHandle, SCardAttributes.CurrentF));
                this.CurrentN = PcscSmartCardConnectionInformation.ToUInt32(scardApi.GetAttribute(readerHandle, SCardAttributes.CurrentN));
                this.CurrentW = PcscSmartCardConnectionInformation.ToUInt32(scardApi.GetAttribute(readerHandle, SCardAttributes.CurrentW));
            }

            private static EbcEncoding? ToEcbEncoding(byte[] value)
            {
                return value?.ToUInt32().UInt32ToEcbEncoding();
            }

            private static uint? ToUInt32(byte[] value)
            {
                return value?.ToUInt32();
            }

            /// <summary>
            /// Gets the Current block waiting time.
            /// </summary>
            public uint? CurrentBlockWaitingTime { get; }

            /// <summary>
            /// Gets the Current clock rate, in kHz.
            /// </summary>
            public uint? CurrentClockRate { get; }

            /// <summary>
            /// Gets the Current character waiting time.
            /// </summary>
            public uint? CurrentCharacterWaitingTime { get; }

            /// <summary>
            /// Gets the Current Bit rate conversion factor D.
            /// </summary>
            public uint? CurrentD { get; }

            /// <summary>
            /// Gets the Current error block control encoding.
            /// </summary>
            public EbcEncoding? CurrentEbcEncoding { get; }

            /// <summary>
            /// Gets the Clock conversion factor.
            /// </summary>
            public uint? CurrentF { get; }

            /// <summary>
            /// Gets the Current guard time.
            /// </summary>
            public uint? CurrentN { get; }

            /// <summary>
            /// Gets the Current work waiting time.
            /// </summary>
            public uint? CurrentW { get; }
        }
    }
}
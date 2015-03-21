using System;
using System.Text;
using WhileTrue.Classes.Utilities;
using WhileTrue.Classes.SCard;
using WhileTrue.Components.CardReaderSubsystem.Base;
using WhileTrue.Facades.SmartCard;

namespace WhileTrue.Components.CardReaderSubsystem.PCSC
{
    internal class PCSCCardReader : CardReaderBase
    {
        private readonly PCSCSmartCardSubsystem pcscSubsystem;
        private readonly SCardAPI scardApi;
        private IntPtr cardHandle = IntPtr.Zero;
        private SCardCardReaderState cardReaderState;
        private PCSCCardReaderConnectionInformation readerConnectionInformation;
        private PCSCSmartCardConnectionInformation cardConnectionInformation;

        internal PCSCCardReader(PCSCSmartCardSubsystem pcscSubsystem, SCardAPI scardApi, string name)
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

        protected IntPtr CardHandle
        {
            get { return this.cardHandle; }
        }
        
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
                    this.InvokePropertyChanged(()=>CanUpdateConnectionInformation);
                }
                else
                {
                    this.cardReaderState = value;
                    this.cardReaderState.dwCurrentState = this.cardReaderState.dwEventState;
                }
            }
            get { return this.cardReaderState; }
        }

        private SCardReaderState ScardState
        {
            get { return this.cardReaderState.dwCurrentState; }
        }

        private bool? CanEject
        {
            get
            {
                try
                {
                    byte[] Attribute = this.scardApi.GetAttribute(this.cardHandle, SCardAttributes.Characteristics);
                    uint Characteristics = (uint)(Attribute[3] << 24 | Attribute[2] << 8 | Attribute[1] << 16 | Attribute[0]);

                    return ((Characteristics & (uint)SCardCharacteristics.Eject) == (uint)SCardCharacteristics.Eject ? true : false);
                }
                catch
                {
                    return null;
                }
            }
        }

        #region CardReaderBase overrides

        protected internal override byte[] ATR
        {
            get
            {
                if (this.CardReaderState.rgbAtr != null)
                {
                    byte[] ATR = new byte[this.CardReaderState.cbAtr];
                    Array.Copy(this.CardReaderState.rgbAtr, 0, ATR, 0, this.CardReaderState.cbAtr);
                    return ATR;
                }
                else
                {
                    return null;
                }
            }
        }

        protected override byte[] Transmit(byte[] data)
        {
            return this.scardApi.Transmit(this.cardHandle, data);
        }

        protected internal override void ConnectCard(Protocol protocol)
        {
            if( this.cardHandle != IntPtr.Zero )
            {
                throw new InvalidOperationException("Smart Card already connected");
            }

            try
            {
                this.cardHandle = this.scardApi.Connect(this.Name, SCardShareMode.Exclusive, ProtocolToScardProtocol(protocol));
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
            if (this.cardHandle != IntPtr.Zero)
            {
                this.scardApi.Disconnect(this.cardHandle, SCardDisposition.Unpower);
                this.cardHandle = IntPtr.Zero;
            }
        }

        protected internal override void ResetCard(Protocol protocol)
        {
            if (this.cardHandle != IntPtr.Zero)
            {
                this.scardApi.Reconnect(this.cardHandle, SCardDisposition.Reset, SCardShareMode.Exclusive, ProtocolToScardProtocol(protocol));
            }
        }
        protected internal override void EjectCard()
        {
            if (this.CanEject.GetValueOrDefault(false))
            {
                if (this.cardHandle != IntPtr.Zero)
                {
                    this.scardApi.Disconnect(this.cardHandle, SCardDisposition.Eject);
                    this.cardHandle = IntPtr.Zero;
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
                if (this.readerConnectionInformation == null && this.cardHandle != IntPtr.Zero)
                {   //If we are connected, we can easily update the reader infos. If not, it must be done manually using the cor. method
                    this.SetConnectionInformation(new PCSCCardReaderConnectionInformation(this.cardHandle, this.scardApi), new PCSCSmartCardConnectionInformation(this.cardHandle, this.scardApi));
                }
                return this.readerConnectionInformation;
            }
        }     
        
        public override ISmartCardConnectionInformation CardConnectionInformation
        {
            get
            {
                if ( this.cardConnectionInformation == null && this.cardHandle != IntPtr.Zero)
                {   //If we are connected, we can easily update the reader infos. If not, it must be done manually using the cor. method
                    this.SetConnectionInformation(new PCSCCardReaderConnectionInformation(this.cardHandle, this.scardApi), new PCSCSmartCardConnectionInformation(this.cardHandle, this.scardApi));
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
                        return this.cardHandle != IntPtr.Zero; //Possible, if we are connected, but not, if another app is connected
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public override void UpdateConnectionInformation()
        {
            IntPtr CardHandle;
            PCSCCardReaderConnectionInformation ReaderInformation;
            PCSCSmartCardConnectionInformation CardInformation;
            switch (this.State)
            {
                case Facades.SmartCard.CardReaderState.Unknown:
                    //Unknown state, better not connect...
                    throw new CardReaderUnavailableException(this);

                case Facades.SmartCard.CardReaderState.NoCard:
                case Facades.SmartCard.CardReaderState.CardMute:
                    //No card or inaccessible card. Try to connect to reader in raw mode.
                    CardHandle = this.scardApi.Connect(this.Name, SCardShareMode.Direct, SCardProtocol.None);
                    ReaderInformation = new PCSCCardReaderConnectionInformation(CardHandle, this.scardApi);
                    CardInformation = null;
                    this.scardApi.Disconnect(CardHandle, SCardDisposition.Leave);
                    break;

                case Facades.SmartCard.CardReaderState.CardPresent:
                case Facades.SmartCard.CardReaderState.CardInUse:
                    //card is inserted but not exclusively used. Connect card possible (or just read info if already connected)
                    if (this.cardHandle != IntPtr.Zero)
                    {
                        ReaderInformation = new PCSCCardReaderConnectionInformation(this.cardHandle, this.scardApi);
                        CardInformation = new PCSCSmartCardConnectionInformation(this.cardHandle, this.scardApi);
                    }
                    else
                    {
                        try
                        {
                            CardHandle = this.scardApi.Connect(this.Name, SCardShareMode.Shared, SCardProtocol.All);
                            ReaderInformation = new PCSCCardReaderConnectionInformation(CardHandle, this.scardApi);
                            CardInformation = new PCSCSmartCardConnectionInformation(CardHandle, this.scardApi);
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
                    if (this.cardHandle != IntPtr.Zero)
                    {
                        ReaderInformation = new PCSCCardReaderConnectionInformation(this.cardHandle, this.scardApi);
                        CardInformation = new PCSCSmartCardConnectionInformation(this.cardHandle, this.scardApi);
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

        private void SetConnectionInformation(PCSCCardReaderConnectionInformation readerInformation, PCSCSmartCardConnectionInformation cardInformation)
        {
            this.SetAndInvoke(()=>ReaderConnectionInformation, ref this.readerConnectionInformation, readerInformation);
            this.SetAndInvoke(()=>CardConnectionInformation, ref this.cardConnectionInformation, cardInformation);
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
        private class PCSCCardReaderConnectionInformation : ICardReaderConnectionInformation
        {
            public PCSCCardReaderConnectionInformation(IntPtr readerHandle, SCardAPI scardApi)
            {
                string SystemName =
                    Encoding.Unicode.GetString(scardApi.GetAttribute(readerHandle, SCardAttributes.DeviceSystemName).GetSubArray(0, -2));//remove trailing null character
                this.SystemName = SystemName;

                uint ChannelInfo = scardApi.GetAttribute(readerHandle, SCardAttributes.ChannelID).ToUInt32();
                this.Channel = GetChannelString(ChannelInfo);

                byte Characteristics = scardApi.GetAttribute(readerHandle, SCardAttributes.Characteristics)[0];
                this.SupportsSwallowing = Characteristics.IsBitSet(1);
                this.SupportsEject = Characteristics.IsBitSet(2);
                this.SupportsCapture = Characteristics.IsBitSet(3);

                this.DefaultClockRate = ToUInt32(scardApi.GetAttribute(readerHandle, SCardAttributes.DefaultClock));
                this.DefaultDataRate = ToUInt32(scardApi.GetAttribute(readerHandle, SCardAttributes.DefaultDataRate));
            }

            private static uint? ToUInt32(byte[] value)
            {
                if( value != null )
                {
                    return value.ToUInt32();
                }
                else
                {
                    return null;
                }
            }

            private static string GetChannelString(uint channelInfo)
            {
                ushort ChannelNo = channelInfo.GetLoUShort();
                switch (channelInfo.GetHiUShort())
                {
                    case 0x01:
                        return string.Format("COM {0}", ChannelNo);
                    case 0x02:
                        return string.Format("LPT {0}", ChannelNo);
                    case 0x04:
                        return "PS/2 keyboard";
                    case 0x08:
                        return string.Format("SCSI (ID {0})", ChannelNo);
                    case 0x10:
                        return string.Format("ID (Device {0})", ChannelNo);
                    case 0x20:
                        return string.Format("USB (Device {0})", ChannelNo);
                    case 0xF0:
                        return string.Format("Vendor specific (0/{0})", ChannelNo);
                    case 0xF1:
                        return string.Format("Vendor specific (1/{0})", ChannelNo);
                    case 0xF2:
                        return string.Format("Vendor specific (2/{0})", ChannelNo);
                    case 0xF3:
                        return string.Format("Vendor specific (3/{0})", ChannelNo);
                    case 0xF4:
                        return string.Format("Vendor specific (4/{0})", ChannelNo);
                    case 0xF5:
                        return string.Format("Vendor specific (5/{0})", ChannelNo);
                    case 0xF6:
                        return string.Format("Vendor specific (6/{0})", ChannelNo);
                    case 0xF7:
                        return string.Format("Vendor specific (7/{0})", ChannelNo);
                    case 0xF8:
                        return string.Format("Vendor specific (8/{0})", ChannelNo);
                    case 0xF9:
                        return string.Format("Vendor specific (9/{0})", ChannelNo);
                    case 0xFA:
                        return string.Format("Vendor specific (A/{0})", ChannelNo);
                    case 0xFB:
                        return string.Format("Vendor specific (B/{0})", ChannelNo);
                    case 0xFC:
                        return string.Format("Vendor specific (C/{0})", ChannelNo);
                    case 0xFD:
                        return string.Format("Vendor specific (D/{0})", ChannelNo);
                    case 0xFE:
                        return string.Format("Vendor specific (E/{0})", ChannelNo);
                    case 0xFF:
                        return string.Format("Vendor specific (F/{0})", ChannelNo);
                    default:
                        return "unknown";
                }
            }

            /// <summary>
            /// Gets the system name of the reader
            /// </summary>
            public string SystemName { get; private set; }

            /// <summary>
            /// Gets the type of channel the reader is connected to
            /// </summary>
            public string Channel { get; private set; }

            /// <summary>
            /// Gets whether ther reader supports card swallowing
            /// </summary>
            public bool SupportsSwallowing { get; private set; }

            /// <summary>
            /// Gets whether ther reader supports card capturing
            /// </summary>
            public bool SupportsCapture { get; private set; }

            /// <summary>
            /// Gets whether ther reader supports card ejection
            /// </summary>
            public bool SupportsEject { get; private set; }
            
            /// <summary>
            /// Gets the Default clock rate, in kHz.
            /// </summary>
            public uint? DefaultClockRate { get; private set; }

            /// <summary>
            /// Gets the Default data rate, in bps.
            /// </summary>
            public uint? DefaultDataRate { get; private set; }
        }
        private class PCSCSmartCardConnectionInformation : ISmartCardConnectionInformation
        {
            public PCSCSmartCardConnectionInformation(IntPtr readerHandle, SCardAPI scardApi)
            {
                this.CurrentBlockWaitingTime = ToUInt32(scardApi.GetAttribute(readerHandle, SCardAttributes.CurrentBWT));
                this.CurrentCharacterWaitingTime = ToUInt32(scardApi.GetAttribute(readerHandle, SCardAttributes.CurrentBWT));
                this.CurrentClockRate = ToUInt32(scardApi.GetAttribute(readerHandle, SCardAttributes.CurrentClock));
                this.CurrentD = ToUInt32(scardApi.GetAttribute(readerHandle, SCardAttributes.CurrentD));
                this.CurrentEBCEncoding = ToECBEncoding(scardApi.GetAttribute(readerHandle, SCardAttributes.CurrentEBCEncoding));
                this.CurrentF = ToUInt32(scardApi.GetAttribute(readerHandle, SCardAttributes.CurrentF));
                this.CurrentN = ToUInt32(scardApi.GetAttribute(readerHandle, SCardAttributes.CurrentN));
                this.CurrentW = ToUInt32(scardApi.GetAttribute(readerHandle, SCardAttributes.CurrentW));
            }

            private static EBCEncoding? ToECBEncoding(byte[] value)
            {
                if (value != null)
                {
                    return value.ToUInt32().UInt32ToECBEncoding();
                }
                else
                {
                    return null;
                }
            }

            private static uint? ToUInt32(byte[] value)
            {
                if (value != null)
                {
                    return value.ToUInt32();
                }
                else
                {
                    return null;
                }
            }

            /// <summary>
            /// Gets the Current block waiting time.
            /// </summary>
            public uint? CurrentBlockWaitingTime { get; private set; }

            /// <summary>
            /// Gets the Current clock rate, in kHz.
            /// </summary>
            public uint? CurrentClockRate { get; private set; }

            /// <summary>
            /// Gets the Current character waiting time.
            /// </summary>
            public uint? CurrentCharacterWaitingTime { get; private set; }

            /// <summary>
            /// Gets the Current Bit rate conversion factor D.
            /// </summary>
            public uint? CurrentD { get; private set; }

            /// <summary>
            /// Gets the Current error block control encoding.
            /// </summary>
            public EBCEncoding? CurrentEBCEncoding { get; private set; }

            /// <summary>
            /// Gets the Clock conversion factor.
            /// </summary>
            public uint? CurrentF { get; private set; }

            /// <summary>
            /// Gets the Current guard time.
            /// </summary>
            public uint? CurrentN { get; private set; }

            /// <summary>
            /// Gets the Current work waiting time.
            /// </summary>
            public uint? CurrentW { get; private set; }
        }
    }
}
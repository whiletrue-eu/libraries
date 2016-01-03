using System;

namespace WhileTrue.Classes.ATR
{
    public class CompactTlvDataObjectStatusIndicator : CompactTlvDataObjectBase
    {
        private byte? lifeCycle;
        private ushort? sw1Sw2;
        private bool includedInTlv;
        /*     
     ISO 7816-4 ch. 8.4 Status information
     The status information consists of 3 bytes: the card life status (1 byte) and the two status bytes
     SW1-SW2.
     The value '00' of the card life status indicates that no card life status is provided. The values
     '80' to 'FE' are proprietary. All other values are RFU.
     The value '9000' of SW1-SW2 indicates normal processing as defined in 5.4.5.
     The value '0000' of SW1-SW2 indicates that the status is not indicated.
     If the category indicator is valued to '80', then the status information may be present in a
     COMPACT-TLV data object. In this case, the tag number is '8'. When the length is '1', then the
     value is the card life status. When the length is '2', then the value is SW1-SW2. When the length
     is '3', then the value is the card life status followed by SW1-SW2. Other values of the length are
     reserved for ISO.
*/
        public CompactTlvDataObjectStatusIndicator(AtrCompactTlvHistoricalCharacters owner):base(owner)
        {
        }

        public bool IncludedInTlv
        {
            get { return this.includedInTlv; }
            set
            {
                this.SetAndInvoke(ref this.includedInTlv, value);
                if (value == false)
                {//only 3 byte status is allowed when not a TLV -> define missing data with defaults
                    this.LifeCycle = this.LifeCycle ?? 0x00;
                    this.StatusWord = this.StatusWord ?? 0x9000;
                }
                this.InvokePropertyChanged(nameof(CompactTlvDataObjectStatusIndicator.CanUndefineLifeCycle));
                this.InvokePropertyChanged(nameof(CompactTlvDataObjectStatusIndicator.CanUndefineStatusWordIndication));
                this.Tag = this.includedInTlv ? (byte?) 0x48 : null;
                this.NotifyChanged();
            }
        }

        protected override byte[] GetValue()
        {
            if (this.lifeCycle != null && this.sw1Sw2 != null)
            {
                return new[]{(byte)this.lifeCycle, (byte)( this.sw1Sw2 >> 8 ), (byte) (this.sw1Sw2 &0x00FF) };
            }
            else if (this.sw1Sw2 != null)
            {
                return new[] { (byte)(this.sw1Sw2 >> 8), (byte)(this.sw1Sw2 & 0x00FF) };
            }
            else if (this.lifeCycle != null)
            {
                return new[] {(byte) this.lifeCycle};
            }
            else
            {
                //Cannot happen; setter will throw exceptions
                throw new InvalidOperationException();
            }
        }

        protected override void UpdateValue(byte[] data)
        {
            if (data != null)
            {
                this.IsApplicable = true;
                if (data.Length == 1)
                {
                    this.LifeCycle = data[0];
                    this.SetStatusIndication(null);
                }
                else if (data.Length == 2)
                {
                    this.LifeCycle = null;
                    this.SetStatusIndication((ushort) (data[1] << 8 | data[2]));
                }
                else if (data.Length == 3)
                {
                    this.LifeCycle = data[0];
                    this.SetStatusIndication((ushort) (data[1] << 8 | data[2]));
                }
                else
                {
                    throw new ArgumentException("Length must be 1,2 or 3");
                }
            }
            else
            {
                this.IsApplicable = false;
            }
        }

        public override CompactTlvTypes Type => CompactTlvTypes.StatusIndicator;

        protected override byte[] GetDefaultValue()
        {
            this.includedInTlv = true; //don't use the property; notifyChanged will be called after add anyway, and it results in an exception if done here
            return new byte[] { 0x81, 0x00 };
        }

        private void SetStatusIndication(ushort? sw1Sw2)
        {
            this.sw1Sw2 = sw1Sw2;
            this.InvokePropertyChanged(nameof(CompactTlvDataObjectStatusIndicator.StatusWordIndication));
            this.InvokePropertyChanged(nameof(CompactTlvDataObjectStatusIndicator.StatusWord));
            this.InvokePropertyChanged(nameof(CompactTlvDataObjectStatusIndicator.CanUndefineLifeCycle));
            this.NotifyChanged();
        }

        public StatusWordIndication? StatusWordIndication
        {
            get
            {
                if (this.sw1Sw2 != null)
                {
                    return Enum.IsDefined(typeof(ATR.StatusWordIndication), (int)this.sw1Sw2.Value) ? (ATR.StatusWordIndication)this.sw1Sw2.Value : ATR.StatusWordIndication.Rfu;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value == null)
                {
                    if (this.CanUndefineStatusWordIndication)
                    {
                        this.SetStatusIndication(null);
                    }
                    else
                    {
                        throw new ArgumentException("Status word indication cannot be unset, as this would lead to an invalid status");
                    }
                }
                else
                {
                    switch (value.Value)
                    {
                        case ATR.StatusWordIndication.NormalProcessing:
                        case ATR.StatusWordIndication.StatusNotIndicated:
                            this.SetStatusIndication((ushort?) value.Value);
                            break;
                        case ATR.StatusWordIndication.Rfu:
                            throw new ArgumentException("RFU value cannot be set");
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                this.InvokePropertyChanged(nameof(CompactTlvDataObjectStatusIndicator.StatusWordIndication));
                this.InvokePropertyChanged(nameof(CompactTlvDataObjectStatusIndicator.StatusWord));
            }
        }      
        
        public ushort? StatusWord
        {
            get
            {
                return this.sw1Sw2;
            }
            set
            {
                if (value == null)
                {
                    if (this.CanUndefineStatusWordIndication)
                    {
                        this.SetStatusIndication(null);
                    }
                    else
                    {
                        throw new ArgumentException("Status word indication cannot be unset, as this would lead to an invalid status");
                    }
                }
                else
                {
                    this.SetStatusIndication(value);
                }
                this.InvokePropertyChanged(nameof(CompactTlvDataObjectStatusIndicator.StatusWordIndication));
                this.InvokePropertyChanged(nameof(CompactTlvDataObjectStatusIndicator.StatusWord));
            }
        }

        public bool CanUndefineStatusWordIndication => this.LifeCycle != null && this.IncludedInTlv;

        public byte? LifeCycle
        {
            get { return this.lifeCycle; }
            set
            {
                if (value == null)
                {
                    if (this.CanUndefineLifeCycle)
                    {
                        this.SetAndInvoke(ref this.lifeCycle, null, _ => this.NotifyChanged());
                    }
                    else
                    {
                        throw new ArgumentException("Status word indication cannot be unset, as this would lead to an invalid status");
                    }
                }
                else
                {
                    this.SetAndInvoke(ref this.lifeCycle, value, _ => this.NotifyChanged());
                }
                this.InvokePropertyChanged(nameof(CompactTlvDataObjectStatusIndicator.CanUndefineStatusWordIndication));
                this.InvokePropertyChanged(nameof(CompactTlvDataObjectStatusIndicator.LifeCycleInformation));
            }
        }

        public KnownLifeCycle? LifeCycleInformation
        {
            get
            {
                if (this.lifeCycle != null)
                {
                    return Enum.IsDefined(typeof(ATR.KnownLifeCycle), (int)this.lifeCycle.Value) ? (ATR.KnownLifeCycle)this.lifeCycle.Value : ATR.KnownLifeCycle.Rfu;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value == null)
                {
                    if (this.CanUndefineLifeCycle)
                    {
                        this.LifeCycle=null;
                    }
                    else
                    {
                        throw new ArgumentException("Life cycle cannot be unset, as this would lead to an invalid status");
                    }
                }
                else
                {
                    switch (value.Value)
                    {
                        case ATR.KnownLifeCycle.NotIndicated:
                            this.LifeCycle = (byte?)value.Value;
                            break;
                        case ATR.KnownLifeCycle.Rfu:
                            throw new ArgumentException("RFU value cannot be set");
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                this.InvokePropertyChanged(nameof(CompactTlvDataObjectStatusIndicator.LifeCycleInformation));
                this.InvokePropertyChanged(nameof(CompactTlvDataObjectStatusIndicator.LifeCycle));
            }
        }      


        public bool CanUndefineLifeCycle => this.sw1Sw2 != null && this.IncludedInTlv;
    }
}
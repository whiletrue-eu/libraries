using System;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.ATR
{
    public class CompactTLVDataObjectStatusIndicator : CompactTLVDataObjectBase
    {
        private byte? lifeCycle;
        private ushort? sw1sw2;
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
        public CompactTLVDataObjectStatusIndicator(AtrCompactTlvHistoricalCharacters owner):base(owner)
        {
        }

        public bool IncludedInTlv
        {
            get { return this.includedInTlv; }
            set
            {
                this.SetAndInvoke(()=>IncludedInTlv, ref this.includedInTlv, value);
                if (value == false)
                {//only 3 byte status is allowed when not a TLV -> define missing data with defaults
                    this.LifeCycle = this.LifeCycle ?? 0x00;
                    this.StatusWord = this.StatusWord ?? 0x9000;
                }
                this.InvokePropertyChanged(()=>CanUndefineLifeCycle);
                this.InvokePropertyChanged(()=>CanUndefineStatusWordIndication);
                this.Tag = this.includedInTlv ? (byte?) 0x48 : null;
                this.NotifyChanged();
            }
        }

        protected override byte[] GetValue()
        {
            if (this.lifeCycle != null && this.sw1sw2 != null)
            {
                return new[]{(byte)this.lifeCycle, (byte)( this.sw1sw2 >> 8 ), (byte) (this.sw1sw2 &0x00FF) };
            }
            else if (this.sw1sw2 != null)
            {
                return new[] { (byte)(this.sw1sw2 >> 8), (byte)(this.sw1sw2 & 0x00FF) };
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

        public override CompactTLVTypes Type
        {
            get { return CompactTLVTypes.StatusIndicator; }
        }

        protected override byte[] GetDefaultValue()
        {
            this.includedInTlv = true; //don't use the property; notifyChanged will be called after add anyway, and it results in an exception if done here
            return new byte[] { 0x81, 0x00 };
        }

        private void SetStatusIndication(ushort? sw1sw2)
        {
            this.sw1sw2 = sw1sw2;
            this.InvokePropertyChanged(() => StatusWordIndication);
            this.InvokePropertyChanged(() => StatusWord);
            this.InvokePropertyChanged(() => CanUndefineLifeCycle);
            this.NotifyChanged();
        }

        public StatusWordIndication? StatusWordIndication
        {
            get
            {
                if (this.sw1sw2 != null)
                {
                    return Enum.IsDefined(typeof(ATR.StatusWordIndication), (int)this.sw1sw2.Value) ? (ATR.StatusWordIndication)this.sw1sw2.Value : ATR.StatusWordIndication.RFU;
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
                        case ATR.StatusWordIndication.RFU:
                            throw new ArgumentException("RFU value cannot be set");
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                this.InvokePropertyChanged(()=>StatusWordIndication);
                this.InvokePropertyChanged(()=>StatusWord);
            }
        }      
        
        public ushort? StatusWord
        {
            get
            {
                return this.sw1sw2;
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
                this.InvokePropertyChanged(() => StatusWordIndication);
                this.InvokePropertyChanged(() => StatusWord);
            }
        }

        public bool CanUndefineStatusWordIndication
        {
            get { return this.LifeCycle != null && this.IncludedInTlv; }
        }

        public byte? LifeCycle
        {
            get { return this.lifeCycle; }
            set
            {
                if (value == null)
                {
                    if (this.CanUndefineLifeCycle)
                    {
                        this.SetAndInvoke(() => this.LifeCycle, ref this.lifeCycle, null, null, _ => this.NotifyChanged());
                    }
                    else
                    {
                        throw new ArgumentException("Status word indication cannot be unset, as this would lead to an invalid status");
                    }
                }
                else
                {
                    this.SetAndInvoke(() => this.LifeCycle, ref this.lifeCycle, value, null, _ => this.NotifyChanged());
                }
                this.InvokePropertyChanged(() => CanUndefineStatusWordIndication);
                this.InvokePropertyChanged(() => LifeCycleInformation);
            }
        }

        public KnownLifeCycle? LifeCycleInformation
        {
            get
            {
                if (this.lifeCycle != null)
                {
                    return Enum.IsDefined(typeof(ATR.KnownLifeCycle), (int)this.lifeCycle.Value) ? (ATR.KnownLifeCycle)this.lifeCycle.Value : ATR.KnownLifeCycle.RFU;
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
                        case ATR.KnownLifeCycle.RFU:
                            throw new ArgumentException("RFU value cannot be set");
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                this.InvokePropertyChanged(() => LifeCycleInformation);
                this.InvokePropertyChanged(() => LifeCycle);
            }
        }      


        public bool CanUndefineLifeCycle
        {
            get { return this.sw1sw2 != null && this.IncludedInTlv; }
        }
    }
}
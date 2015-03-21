namespace WhileTrue.Classes.ATR
{
    public class CompactTLVDataObjectRFU : CompactTLVDataObjectBase
    {
        private readonly byte tag;

        public CompactTLVDataObjectRFU(AtrCompactTlvHistoricalCharacters owner, byte tag)
            : base(owner)
        {
            this.tag = tag;
        }

        private byte[] rfuValue;

        protected override byte[] GetValue()
        {
            return this.rfuValue;
        }

        protected override void UpdateValue(byte[] data)
        {
            if (data != null)
            {
                this.IsApplicable = true;
                this.RFUValue = data;
            }
            else
            {
                this.IsApplicable = false;
            }
        }

        public override CompactTLVTypes Type
        {
            get { return (CompactTLVTypes) this.tag; }
        }

        protected override byte[] GetDefaultValue()
        {
            return new byte[] { (byte) ((this.tag<<4) | 0x00) };
        }

        public byte[] RFUValue
        {
            get { return this.rfuValue; }
            set
            {
                this.SetAndInvoke(() => this.RFUValue, ref this.rfuValue, value);
                this.NotifyChanged();
            }
        }
    }
}
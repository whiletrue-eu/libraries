namespace WhileTrue.Classes.ATR
{
    public class CompactTlvDataObjectRfu : CompactTlvDataObjectBase
    {
        private readonly byte tag;

        public CompactTlvDataObjectRfu(AtrCompactTlvHistoricalCharacters owner, byte tag)
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
                this.RfuValue = data;
            }
            else
            {
                this.IsApplicable = false;
            }
        }

        public override CompactTlvTypes Type => (CompactTlvTypes) this.tag;

        protected override byte[] GetDefaultValue()
        {
            return new byte[] { (byte) ((this.tag<<4) | 0x00) };
        }

        public byte[] RfuValue
        {
            get { return this.rfuValue; }
            set
            {
                this.SetAndInvoke(ref this.rfuValue, value);
                this.NotifyChanged();
            }
        }
    }
}
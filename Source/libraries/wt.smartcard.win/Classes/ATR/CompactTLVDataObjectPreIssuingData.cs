namespace WhileTrue.Classes.ATR
{
    public class CompactTlvDataObjectPreIssuingData : CompactTlvDataObjectBase
    {
/*
      ISO 7816-4 ch. 8.3.5 Pre-issuing data
     This data object is optional and of variable length. Structure and coding are not defined in this
     part of ISO/IEC 7816. It may be used for indicating
      * card manufacturer
      * integrated circuit type
      * integrated circuit manufacturer
      * ROM mask version
      * operation system version
      This data object is introduced by '6Y'.
 */
        public CompactTlvDataObjectPreIssuingData(AtrCompactTlvHistoricalCharacters owner)
            : base(owner)
        {
        }

        private byte[] preIssuingData;

        protected override byte[] GetValue()
        {
            return this.preIssuingData;
        }

        protected override void UpdateValue(byte[] data)
        {
            if (data != null)
            {
                this.IsApplicable = true;
                this.PreIssuingData = data;
            }
            else
            {
                this.IsApplicable = false;
            }
        }

        public override CompactTlvTypes Type => CompactTlvTypes.PreIssuingData;

        protected override byte[] GetDefaultValue()
        {
            return new byte[] { 0x60 };
        }

        public byte[] PreIssuingData
        {
            get { return this.preIssuingData; }
            set
            {
                this.SetAndInvoke(ref this.preIssuingData, value);
                this.NotifyChanged();
            }
        }
    }
}
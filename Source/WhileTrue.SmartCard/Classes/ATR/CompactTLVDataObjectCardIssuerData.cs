namespace WhileTrue.Classes.ATR
{
    public class CompactTLVDataObjectCardIssuerData : CompactTLVDataObjectBase
    {
        /*
              ISO 7816-4 ch. 8.3.4 Card issuer's data
     Thsi data object is optional and of variable length. Structure and coding are defined by the card
     issuer.
     This data object is introduced by '5Y'.
         */
        public CompactTLVDataObjectCardIssuerData(AtrCompactTlvHistoricalCharacters owner)
            : base(owner)
        {
        }

        private byte[] cardIssuerData;

        protected override byte[] GetValue()
        {
            return this.cardIssuerData;
        }

        protected override void UpdateValue(byte[] data)
        {
            if (data != null)
            {
                this.IsApplicable = true;
                this.CardIssuerData = data;
            }
            else
            {
                this.IsApplicable = false;
            }
        }

        public override CompactTLVTypes Type
        {
            get { return CompactTLVTypes.CardIssuerData; }
        }

        protected override byte[] GetDefaultValue()
        {
            return new byte[] { 0x50 };
        }

        public byte[] CardIssuerData
        {
            get { return this.cardIssuerData; }
            set
            {
                this.SetAndInvoke(()=>CardIssuerData, ref this.cardIssuerData, value);
                this.NotifyChanged();
            }
        }
    }
}
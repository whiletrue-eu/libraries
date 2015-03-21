namespace WhileTrue.Classes.ATR
{/*
    ISO 7816-4 8.5 DIR data reference
    If the category indicator is '10', then the following byte is the DIR data reference. The coding
    and meaning of this byte are outside the scope of this part of the ISO/IEC 7816. 
  */

    public class AtrDirDataReferenceHistoricalCharacters : AtrHistoricalCharactersBase
    {
        private byte dirDataReference;
        private bool isParsing;

        public AtrDirDataReferenceHistoricalCharacters(Atr owner)
            :base(owner)
        {
        }

        public byte DirDataReference
        {
            get { return this.dirDataReference; }
            set
            {
                this.SetAndInvoke(ref this.dirDataReference, value);
                this.UpdateData();
            }
        }

        private void UpdateData()
        {
            if (this.isParsing == false)
            {
                this.UpdateHistoricalCharacters(new byte[] {0x10, this.dirDataReference});
            }
        }

        protected override void HistoricalCharactersChanged(byte[] historicalCharacters1)
        {
            this.isParsing = true;
            try
            {
                this.ParseError = null;
                if (this.HistoricalCharacters.Length > 0 && this.HistoricalCharacters[0] == 0x10)
                {
                    this.IsApplicable = true;
                    if (this.HistoricalCharacters.Length >= 2)
                    {
                        this.DirDataReference = this.HistoricalCharacters[1];
                    }
                    if (this.HistoricalCharacters.Length > 2)
                    {
                        this.ParseError = new ParseError("Dir data reference is only defined with 1 byte length",0);
                    }
                }
                else
                {
                    this.IsApplicable = false;
                }
            }
            finally
            {
                this.isParsing = false;
            }
        }
    }
}
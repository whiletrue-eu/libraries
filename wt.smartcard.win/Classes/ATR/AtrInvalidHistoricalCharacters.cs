namespace WhileTrue.Classes.ATR
{
    public class AtrInvalidHistoricalCharacters : AtrHistoricalCharactersBase
    {
        private byte[] bytes;
        private bool isParsing;

        public AtrInvalidHistoricalCharacters( Atr owner )
            : base(owner)
        {
            this.IsApplicable = true;
        }

        private void UpdateData()
        {
            if (this.isParsing == false)
            {
                this.UpdateHistoricalCharacters(this.Bytes);
            }
        }

        internal void UpdateError(ParseError error)
        {
            this.ParseError = error;
        }

        protected override void HistoricalCharactersChanged(byte[] historicalCharacters1)
        {
            this.isParsing = true;
            try
            {
                if (this.HistoricalCharacters.Length >= 1)
                {
                    this.Bytes = this.HistoricalCharacters;
                }
            }
            finally
            {
                this.isParsing = false;
            }
        }

        public byte[] Bytes
        {
            get { return this.bytes; }
            private set
            {
                this.SetAndInvoke(ref this.bytes, value);
                this.UpdateData();
            }
        }
    }
}
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.ATR
{
    /*
     ISO 7816-4 8.2 Category indicator (mandatory)
     The category indicator is the first historical byte. If the category indicator is equal to '00',
     '10' or '8X', then the format of the historical bytes shall be according to this part of ISO/IEC
     7816. 
 
     Table 78 - Coding of the category indicator
     Value  Meaning
     '00'      Status information shall be present at the end of the historical bytes (not in TLV).
     '10'      Specified in 8.5
     '80'      Status information if present is contained in an optional COMPACT-TLV data object.
     '81'-'8F' RFU
     Other values Proprietary
     */
    public abstract class AtrHistoricalCharactersBase : ObservableObject 
    {
        private byte[] historicalCharacters;
        private readonly Atr owner;
        private bool isApplicable;
        private ParseError parseError;

        public byte[] HistoricalCharacters
        {
            get { return this.historicalCharacters; }
            private set { this.SetAndInvoke(ref this.historicalCharacters,value); }
        }

        public bool IsApplicable
        {
            get { return this.isApplicable; }
            protected set { this.SetAndInvoke(ref this.isApplicable, value);}
        }

        public ParseError ParseError
        {
            get { return this.parseError; }
            protected set { this.SetAndInvoke(ref this.parseError, value); }
        }

        protected AtrHistoricalCharactersBase(Atr owner)
        {
            this.owner = owner;
        }

        internal void NotifyAtrChanged()
        {
            this.EvaluateHistoricalCharactersChanged(this.owner.TokenizedAtr.HistoricalCharacters.HistoricalCharacters);
        }

        protected void EvaluateHistoricalCharactersChanged(byte[] historicalCharacters)
        {
            if (this.historicalCharacters==null || this.historicalCharacters.HasEqualValue(historicalCharacters)==false)
            {
                this.HistoricalCharacters = historicalCharacters;
                this.HistoricalCharactersChanged(historicalCharacters);
            }
        }

        protected abstract void HistoricalCharactersChanged(byte[] historicalCharacters1);

        protected void UpdateHistoricalCharacters(byte[] historicalCharacters)
        {
            try
            {
                this.HistoricalCharacters = historicalCharacters;
                this.owner.TokenizedAtr.HistoricalCharacters.HistoricalCharacters = historicalCharacters;
            }
            catch
            {
                //Reset data to last valid value and then rethrow exception
                this.historicalCharacters = null;//Force update with old value
                this.EvaluateHistoricalCharactersChanged(this.owner.TokenizedAtr.HistoricalCharacters.HistoricalCharacters);
                throw;
            }
        }
    }
}
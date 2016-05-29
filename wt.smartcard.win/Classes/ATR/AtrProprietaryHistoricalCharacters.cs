using System;
using System.Linq;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.ATR
{
    /*
     Table 78 - Coding of the category indicator
     Value  Meaning
     '00'      Status information shall be present at the end of the historical bytes (not in TLV).
     '10'      Specified in 8.5
     '80'      Status information if present is contained in an optional COMPACT-TLV data object.
     '81'-'8F' RFU
     Other values Proprietary
*/
    public class AtrProprietaryHistoricalCharacters : AtrHistoricalCharactersBase
    {
        private byte[] bytes;
        private byte categoryIndicator;
        private bool isParsing;

        public AtrProprietaryHistoricalCharacters( Atr owner)
            : base(owner)
        {
        }

        private void UpdateData()
        {
            if (this.isParsing == false)
            {
                this.UpdateHistoricalCharacters(new[] {this.CategoryIndicator}.Concat(this.Bytes).ToArray());
            }
        }

        protected override void HistoricalCharactersChanged(byte[] historicalCharacters1)
        {
            this.isParsing = true;
            try
            {
                this.ParseError = null;
                if (this.HistoricalCharacters.Length > 0 && this.HistoricalCharacters[0] != 0x00 && this.HistoricalCharacters[0] != 0x10 &&
                    (this.HistoricalCharacters[0] & 0xF0) != 0x80)
                {
                    this.IsApplicable = true;

                    this.CategoryIndicator = this.HistoricalCharacters[0];

                    if (this.HistoricalCharacters.Length >= 1)
                    {
                        this.Bytes = this.HistoricalCharacters.GetSubArray(1);
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

        public byte CategoryIndicator
        {
            get { return this.categoryIndicator; }
            set
            {
                if (value != 0x00 &&
                    value != 0x10 &&
                    (value & 0xF0) != 0x80)

                {
                    this.SetAndInvoke(ref this.categoryIndicator, value);
                    this.UpdateData();
                }
                else
                {
                    throw new ArgumentException("Category indicator of proprietary historical characters must be unequal to 0x00, 0x10 and 0x8X");
                }
            }
        }

        public byte[] Bytes
        {
            get { return this.bytes; }
            set
            {
                this.SetAndInvoke(ref this.bytes, value); 
                this.UpdateData();
            }
        }
    }
}
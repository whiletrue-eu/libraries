using System;
using System.Linq;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.ATR
{
    public class AtrRfuHistoricalCharacters : AtrHistoricalCharactersBase
    {
        private byte categoryIndicator;
        private byte[] bytes;
        private bool isParsing;

        public AtrRfuHistoricalCharacters( Atr owner)
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
                if (this.HistoricalCharacters.Length > 0 &&
                    (this.HistoricalCharacters[0] & 0xF0) == 0x80 && this.HistoricalCharacters[0] != 0x80)
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
                if ((value & 0xF0) == 0x80 && value != 0x80)
                {
                    this.SetAndInvoke(ref this.categoryIndicator, value);
                    this.UpdateData();
                }
                else
                {
                    throw new ArgumentException("Category indicator of RFU historical characters must be 0x81 to 0x8F");
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
using WhileTrue.Classes.ATR.Tokenized;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls.ATRView
{
    public class AtrChecksumTokenAdapter : AtrTokenAdapterBase
    {
        public AtrChecksumTokenAdapter(AtrChecksumToken atrChecksum):base(atrChecksum)
        {
            this.ChecksumExists = atrChecksum != null;
            if( this.ChecksumExists )
            {
                this.ChecksumValid = atrChecksum.ChecksumValid?"valid":string.Format("invalid (calculated checksum: {0})", atrChecksum.CalculatedChecksum.ToHexString());
            }
        }

        public bool ChecksumExists { get; private set; }

        public string ChecksumValid{get; private set; }
    }
}
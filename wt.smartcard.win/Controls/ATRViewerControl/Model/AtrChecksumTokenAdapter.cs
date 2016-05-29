using WhileTrue.Classes.ATR.Tokenized;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls.ATRViewerControl.Model
{
    public class AtrChecksumTokenAdapter : AtrTokenAdapterBase
    {
        public AtrChecksumTokenAdapter(AtrChecksumToken atrChecksum):base(atrChecksum)
        {
            this.ChecksumExists = atrChecksum != null;
            if( this.ChecksumExists )
            {
                this.ChecksumValid = atrChecksum.ChecksumValid?"valid": $"invalid (calculated checksum: {atrChecksum.CalculatedChecksum.ToHexString()})";
            }
        }

        public bool ChecksumExists { get; }

        public string ChecksumValid{get; private set; }
    }
}
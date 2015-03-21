using WhileTrue.Classes.ATR.Tokenized;

namespace WhileTrue.Controls.ATRView
{
    public class AtrExtraBytesAdapter : AtrTokenAdapterBase
    {
        public AtrExtraBytesAdapter(AtrExtraBytesToken extraBytes) : base(extraBytes)
        {
            this.ExtraBytesAvailable = extraBytes != null;
        }

        public bool ExtraBytesAvailable { get; private set; }
    }
}
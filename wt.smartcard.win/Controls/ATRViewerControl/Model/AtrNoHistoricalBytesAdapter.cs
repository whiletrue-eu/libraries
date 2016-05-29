using WhileTrue.Classes.ATR;

namespace WhileTrue.Controls.ATRViewerControl.Model
{
    public class AtrNoHistoricalBytesAdapter : AtrHistoricalBytesAdapterBase
    {
        public AtrNoHistoricalBytesAdapter(AtrNoHistoricalCharacters atrHistoricalCharactersBase, InterpretedAtrAdapter interpretedAtrAdapter):base(atrHistoricalCharactersBase,interpretedAtrAdapter)
        {
        }
    }
}
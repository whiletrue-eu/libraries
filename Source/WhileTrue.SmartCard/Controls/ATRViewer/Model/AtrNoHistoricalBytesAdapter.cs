using WhileTrue.Classes.ATR;

namespace WhileTrue.Controls.ATRView
{
    public class AtrNoHistoricalBytesAdapter : AtrHistoricalBytesAdapterBase
    {
        public AtrNoHistoricalBytesAdapter(AtrNoHistoricalCharacters atrHistoricalCharactersBase, InterpretedAtrAdapter interpretedAtrAdapter):base(atrHistoricalCharactersBase,interpretedAtrAdapter)
        {
        }
    }
}
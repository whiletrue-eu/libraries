using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls.ATRViewerControl.Model
{
    public class AtrProprietaryHistoricalBytesAdapter : AtrHistoricalBytesAdapterBase
    {
        private readonly AtrProprietaryHistoricalCharacters historicalCharacters;
        private static readonly PropertyAdapter<AtrProprietaryHistoricalBytesAdapter, string> dataAdapter;
        private static readonly PropertyAdapter<AtrProprietaryHistoricalBytesAdapter, string> structureIndicatorAdapter;

        static AtrProprietaryHistoricalBytesAdapter()
        {
            IPropertyAdapterFactory<AtrProprietaryHistoricalBytesAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<AtrProprietaryHistoricalBytesAdapter>();

            AtrProprietaryHistoricalBytesAdapter.structureIndicatorAdapter = PropertyFactory.Create(
                nameof(AtrProprietaryHistoricalBytesAdapter.CategoryIndicator),
                instance => instance.historicalCharacters.CategoryIndicator.ToHexString(),
                (instance, value) => Helper.SetAsHexByteValue(value, _ => instance.historicalCharacters.CategoryIndicator = _)
                );

            AtrProprietaryHistoricalBytesAdapter.dataAdapter = PropertyFactory.Create(
                nameof(AtrProprietaryHistoricalBytesAdapter.Bytes),
                instance => instance.historicalCharacters.Bytes.ToHexString(" "),
                (instance, value) => Helper.SetAsHexValue(value, 0, 15, _ => instance.historicalCharacters.Bytes = _)
                );
        }

        public string CategoryIndicator
        {
            get { return AtrProprietaryHistoricalBytesAdapter.structureIndicatorAdapter.GetValue(this); }
            set
            {
                AtrProprietaryHistoricalBytesAdapter.structureIndicatorAdapter.SetValue(this, value); 
            }
        }

        public string Bytes
        {
            get { return AtrProprietaryHistoricalBytesAdapter.dataAdapter.GetValue(this); }
            set
            {
                AtrProprietaryHistoricalBytesAdapter.dataAdapter.SetValue(this, value); 
            }
        }

        public AtrProprietaryHistoricalBytesAdapter(AtrProprietaryHistoricalCharacters historicalCharacters, InterpretedAtrAdapter interpretedAtr)
            : base(historicalCharacters, interpretedAtr)
        {
            this.historicalCharacters = historicalCharacters;
        }

    }
}
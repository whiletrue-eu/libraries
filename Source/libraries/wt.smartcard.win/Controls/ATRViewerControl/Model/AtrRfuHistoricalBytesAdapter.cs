using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls.ATRViewerControl.Model
{
    public class AtrRfuHistoricalBytesAdapter : AtrHistoricalBytesAdapterBase
    {
        private readonly AtrRfuHistoricalCharacters historicalCharacters;
        private static readonly PropertyAdapter<AtrRfuHistoricalBytesAdapter, string> dataAdapter;
        private static readonly PropertyAdapter<AtrRfuHistoricalBytesAdapter, string> structureIndicatorAdapter;

        static AtrRfuHistoricalBytesAdapter()
        {
            IPropertyAdapterFactory<AtrRfuHistoricalBytesAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<AtrRfuHistoricalBytesAdapter>();

            AtrRfuHistoricalBytesAdapter.structureIndicatorAdapter = PropertyFactory.Create(
                nameof(AtrRfuHistoricalBytesAdapter.CategoryIndicator),
                instance => instance.historicalCharacters.CategoryIndicator.ToHexString(),
                (instance, value) => Helper.SetAsHexByteValue(value, _ => instance.historicalCharacters.CategoryIndicator = _)
                );

            AtrRfuHistoricalBytesAdapter.dataAdapter = PropertyFactory.Create(
                nameof(AtrRfuHistoricalBytesAdapter.Bytes),
                instance => instance.historicalCharacters.Bytes.ToHexString(" "),
                (instance, value) => Helper.SetAsHexValue(value, 0, 15, _ => instance.historicalCharacters.Bytes = _)
                );
        }

        public string CategoryIndicator
        {
            get { return AtrRfuHistoricalBytesAdapter.structureIndicatorAdapter.GetValue(this); }
            set
            {
                AtrRfuHistoricalBytesAdapter.structureIndicatorAdapter.SetValue(this, value); 
            }
        }

        public string Bytes
        {
            get { return AtrRfuHistoricalBytesAdapter.dataAdapter.GetValue(this); }
            set
            {
                AtrRfuHistoricalBytesAdapter.dataAdapter.SetValue(this, value); 
            }
        }

        public AtrRfuHistoricalBytesAdapter(AtrRfuHistoricalCharacters historicalCharacters, InterpretedAtrAdapter interpretedAtr)
            : base(historicalCharacters, interpretedAtr)
        {
            this.historicalCharacters = historicalCharacters;
        }
    }
}
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls.ATRView
{
    public class AtrRfuHistoricalBytesAdapter : AtrHistoricalBytesAdapterBase
    {
        private readonly AtrRFUHistoricalCharacters historicalCharacters;
        private static readonly PropertyAdapter<AtrRfuHistoricalBytesAdapter, string> dataAdapter;
        private static readonly PropertyAdapter<AtrRfuHistoricalBytesAdapter, string> structureIndicatorAdapter;

        static AtrRfuHistoricalBytesAdapter()
        {
            IPropertyAdapterFactory<AtrRfuHistoricalBytesAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<AtrRfuHistoricalBytesAdapter>();

            structureIndicatorAdapter = PropertyFactory.Create(
                @this => @this.CategoryIndicator,
                @this => @this.historicalCharacters.CategoryIndicator.ToHexString(),
                (@this, value) => Helper.SetAsHexByteValue(value, _ => @this.historicalCharacters.CategoryIndicator = _)
                );

            dataAdapter = PropertyFactory.Create(
                @this => @this.Bytes,
                @this => @this.historicalCharacters.Bytes.ToHexString(" "),
                (@this, value) => Helper.SetAsHexValue(value, 0, 15, _ => @this.historicalCharacters.Bytes = _)
                );
        }

        public string CategoryIndicator
        {
            get { return structureIndicatorAdapter.GetValue(this); }
            set
            {
                structureIndicatorAdapter.SetValue(this, value); 
            }
        }

        public string Bytes
        {
            get { return dataAdapter.GetValue(this); }
            set
            {
                dataAdapter.SetValue(this, value); 
            }
        }

        public AtrRfuHistoricalBytesAdapter(AtrRFUHistoricalCharacters historicalCharacters, InterpretedAtrAdapter interpretedAtr)
            : base(historicalCharacters, interpretedAtr)
        {
            this.historicalCharacters = historicalCharacters;
        }
    }
}
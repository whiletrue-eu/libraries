using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls.ATRViewerControl.Model
{
    public class DataObjectCardIssuerDataAdapter : DataObjectBaseAdapter
    {
        private readonly CompactTlvDataObjectCardIssuerData value;
        private static readonly ReadOnlyPropertyAdapter<DataObjectCardIssuerDataAdapter, string> cardIssuerDataAdapter;

        static DataObjectCardIssuerDataAdapter()
        {
            ObservableObject.IPropertyAdapterFactory<DataObjectCardIssuerDataAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<DataObjectCardIssuerDataAdapter>();

            DataObjectCardIssuerDataAdapter.cardIssuerDataAdapter = PropertyFactory.Create(
                nameof(DataObjectCardIssuerDataAdapter.CardIssuerData),
                instance => instance.value.CardIssuerData.ToHexString(" ")
                );
        }

        public string CardIssuerData
        {
            get {return DataObjectCardIssuerDataAdapter.cardIssuerDataAdapter.GetValue(this); }
            set {Helper.SetAsHexValue(value, 0, 15, _ => this.value.CardIssuerData=_);}
        }

        public DataObjectCardIssuerDataAdapter(CompactTlvDataObjectCardIssuerData value)
            : base(value)
        {
            this.value = value;
        }
    }
}
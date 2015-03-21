using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls.ATRViewerControl.Model
{
    public class DataObjectIssuerIdentificationNumberAdapter : DataObjectBaseAdapter
    {
        private readonly CompactTlvDataObjectIssuerIdentificationNumber value;
        private static readonly PropertyAdapter<DataObjectIssuerIdentificationNumberAdapter, string> issuerIdentificationNumberAdapter;

        static DataObjectIssuerIdentificationNumberAdapter()
        {
            ObservableObject.IPropertyAdapterFactory<DataObjectIssuerIdentificationNumberAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<DataObjectIssuerIdentificationNumberAdapter>();

            DataObjectIssuerIdentificationNumberAdapter.issuerIdentificationNumberAdapter=PropertyFactory.Create(
                nameof(DataObjectIssuerIdentificationNumberAdapter.IssuerIdentificationNumber),
                instance=>instance.value.IssuerIdentificationNumber,
                (instance,value)=>instance.value.IssuerIdentificationNumber=value
                );
        }

        public string IssuerIdentificationNumber
        {
            get { return DataObjectIssuerIdentificationNumberAdapter.issuerIdentificationNumberAdapter.GetValue(this); }
            set { DataObjectIssuerIdentificationNumberAdapter.issuerIdentificationNumberAdapter.SetValue(this, value); }
        }

        public DataObjectIssuerIdentificationNumberAdapter(CompactTlvDataObjectIssuerIdentificationNumber value)
            : base(value)
        {
            this.value = value;
        }
    }
}
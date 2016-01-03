using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls.ATRViewerControl.Model
{
    public class DataObjectPreIssuingDataAdapter : DataObjectBaseAdapter
    {
        private readonly CompactTlvDataObjectPreIssuingData value;
        private static readonly ReadOnlyPropertyAdapter<DataObjectPreIssuingDataAdapter, string> preIssuingDataAdapter;

        static DataObjectPreIssuingDataAdapter()
        {
            ObservableObject.IPropertyAdapterFactory<DataObjectPreIssuingDataAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<DataObjectPreIssuingDataAdapter>();
            DataObjectPreIssuingDataAdapter.preIssuingDataAdapter = PropertyFactory.Create(
                nameof(DataObjectPreIssuingDataAdapter.PreIssuingData),
                instance => instance.value.PreIssuingData.ToHexString(" ")
                );
        }

        public string PreIssuingData
        {
            get { return DataObjectPreIssuingDataAdapter.preIssuingDataAdapter.GetValue(this); }
            set
            {
                Helper.SetAsHexValue(value, 0,15,_=>this.value.PreIssuingData=_);
            }
        }

        public DataObjectPreIssuingDataAdapter(CompactTlvDataObjectPreIssuingData value)
            : base(value)
        {
            this.value = value;
        }
    }
}
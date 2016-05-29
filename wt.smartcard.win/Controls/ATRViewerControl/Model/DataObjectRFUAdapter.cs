using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls.ATRViewerControl.Model
{
    public class DataObjectRfuAdapter : DataObjectBaseAdapter
    {
        private readonly CompactTlvDataObjectRfu value;
        private static readonly ReadOnlyPropertyAdapter<DataObjectRfuAdapter, string> rfuValueAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectRfuAdapter, byte?> tagAdapter;

        static DataObjectRfuAdapter()
        {
            ObservableObject.IPropertyAdapterFactory<DataObjectRfuAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<DataObjectRfuAdapter>();
            DataObjectRfuAdapter.rfuValueAdapter = PropertyFactory.Create(
                       nameof(DataObjectRfuAdapter.RfuValue),
                       instance => instance.value.RfuValue.ToHexString(" ")
                       );
            DataObjectRfuAdapter.tagAdapter = PropertyFactory.Create(
                nameof(DataObjectRfuAdapter.Tag),
                instance => instance.value.Tag
                );
        }

        public byte? Tag => DataObjectRfuAdapter.tagAdapter.GetValue(this);

        public string RfuValue
        {
            get { return DataObjectRfuAdapter.rfuValueAdapter.GetValue(this); }
            set
            {
                Helper.SetAsHexValue(value, 0,15,_=>this.value.RfuValue=_);
            }
        }
        public DataObjectRfuAdapter(CompactTlvDataObjectRfu value):base(value)
        {
            this.value = value;
        }
    }
}
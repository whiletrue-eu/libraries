using System;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls.ATRView
{
    public class DataObjectPreIssuingDataAdapter : DataObjectBaseAdapter
    {
        private readonly CompactTLVDataObjectPreIssuingData value;
        private static readonly ReadOnlyPropertyAdapter<DataObjectPreIssuingDataAdapter, string> preIssuingDataAdapter;

        static DataObjectPreIssuingDataAdapter()
        {
            ObservableObject.IPropertyAdapterFactory<DataObjectPreIssuingDataAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<DataObjectPreIssuingDataAdapter>();
            preIssuingDataAdapter = PropertyFactory.Create(
                @this => @this.PreIssuingData,
                @this => @this.value.PreIssuingData.ToHexString(" ")
                );
        }

        public string PreIssuingData
        {
            get { return preIssuingDataAdapter.GetValue(this); }
            set
            {
                Helper.SetAsHexValue(value, 0,15,_=>this.value.PreIssuingData=_);
            }
        }

        public DataObjectPreIssuingDataAdapter(CompactTLVDataObjectPreIssuingData value)
            : base(value)
        {
            this.value = value;
        }
    }
}
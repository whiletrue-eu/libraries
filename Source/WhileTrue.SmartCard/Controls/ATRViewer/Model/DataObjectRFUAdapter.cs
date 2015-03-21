using System;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls.ATRView
{
    public class DataObjectRFUAdapter : DataObjectBaseAdapter
    {
        private readonly CompactTLVDataObjectRFU value;
        private static readonly ReadOnlyPropertyAdapter<DataObjectRFUAdapter, string> rfuValueAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectRFUAdapter, byte?> tagAdapter;

        static DataObjectRFUAdapter()
        {
            ObservableObject.IPropertyAdapterFactory<DataObjectRFUAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<DataObjectRFUAdapter>();
            rfuValueAdapter = PropertyFactory.Create(
                       @this => @this.RFUValue,
                       @this => @this.value.RFUValue.ToHexString(" ")
                       );
            tagAdapter = PropertyFactory.Create(
                @this => @this.Tag,
                @this => @this.value.Tag
                );
        }

        public byte? Tag { get { return tagAdapter.GetValue(this); } }

        public string RFUValue
        {
            get { return rfuValueAdapter.GetValue(this); }
            set
            {
                Helper.SetAsHexValue(value, 0,15,_=>this.value.RFUValue=_);
            }
        }
        public DataObjectRFUAdapter(CompactTLVDataObjectRFU value):base(value)
        {
            this.value = value;
        }
    }
}
using System;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls.ATRView
{
    public class DataObjectIssuerIdentificationNumberAdapter : DataObjectBaseAdapter
    {
        private readonly CompactTLVDataObjectIssuerIdentificationNumber value;
        private static readonly PropertyAdapter<DataObjectIssuerIdentificationNumberAdapter, string> issuerIdentificationNumberAdapter;

        static DataObjectIssuerIdentificationNumberAdapter()
        {
            ObservableObject.IPropertyAdapterFactory<DataObjectIssuerIdentificationNumberAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<DataObjectIssuerIdentificationNumberAdapter>();

            issuerIdentificationNumberAdapter=PropertyFactory.Create(
                @this => @this.IssuerIdentificationNumber,
                @this=>@this.value.IssuerIdentificationNumber,
                (@this,value)=>@this.value.IssuerIdentificationNumber=value
                );
        }

        public string IssuerIdentificationNumber
        {
            get { return issuerIdentificationNumberAdapter.GetValue(this); }
            set { issuerIdentificationNumberAdapter.SetValue(this, value); }
        }

        public DataObjectIssuerIdentificationNumberAdapter(CompactTLVDataObjectIssuerIdentificationNumber value)
            : base(value)
        {
            this.value = value;
        }
    }
}
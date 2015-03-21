using System;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls.ATRView
{
    public class DataObjectCardIssuerDataAdapter : DataObjectBaseAdapter
    {
        private readonly CompactTLVDataObjectCardIssuerData value;
        private static readonly ReadOnlyPropertyAdapter<DataObjectCardIssuerDataAdapter, string> cardIssuerDataAdapter;

        static DataObjectCardIssuerDataAdapter()
        {
            ObservableObject.IPropertyAdapterFactory<DataObjectCardIssuerDataAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<DataObjectCardIssuerDataAdapter>();

            cardIssuerDataAdapter = PropertyFactory.Create(
                @this => @this.CardIssuerData,
                @this => @this.value.CardIssuerData.ToHexString(" ")
                );
        }

        public string CardIssuerData
        {
            get {return cardIssuerDataAdapter.GetValue(this); }
            set {Helper.SetAsHexValue(value, 0, 15, _ => this.value.CardIssuerData=_);}
        }

        public DataObjectCardIssuerDataAdapter(CompactTLVDataObjectCardIssuerData value)
            : base(value)
        {
            this.value = value;
        }
    }
}
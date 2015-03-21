using System;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls.ATRView
{
    public class DataObjectApplicationIdentifierAdapter : DataObjectBaseAdapter
    {
        private readonly CompactTLVDataObjectApplicationIdentifier value;
        private static readonly ReadOnlyPropertyAdapter<DataObjectApplicationIdentifierAdapter, string> aidAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectApplicationIdentifierAdapter, string> ridAdapter;

        static DataObjectApplicationIdentifierAdapter()
        {
            ObservableObject.IPropertyAdapterFactory<DataObjectApplicationIdentifierAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<DataObjectApplicationIdentifierAdapter>();

            aidAdapter = PropertyFactory.Create(
                @this => @this.Aid,
                @this => @this.value.Aid.ToHexString()
                );
            ridAdapter = PropertyFactory.Create(
                @this => @this.Rid,
                @this => GetRidInfo(@this.value.Aid)
                );
        }

        public string Rid { get { return ridAdapter.GetValue(this); } }

        private static string GetRidInfo(byte[] aid)
        {
            if (aid.Length >= 5)
            {
                RidValue Rid = RidValue.GetFromRid(aid.GetSubArray(0, 5));
                return Rid != null ? Rid.ToString() : null;
            }
            else
            {
                return "(value too short for a RID)";
            }
        }

        public string Aid
        {
            get { return aidAdapter.GetValue(this); }
            set {
                Helper.SetAsHexValue(value, 0,15,_=>this.value.Aid=_);
            }
        }

        public DataObjectApplicationIdentifierAdapter(CompactTLVDataObjectApplicationIdentifier value):base(value)
        {
            this.value = value;
        }
    }
}
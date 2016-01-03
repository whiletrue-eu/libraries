using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls.ATRViewerControl.Model
{
    public class DataObjectApplicationIdentifierAdapter : DataObjectBaseAdapter
    {
        private readonly CompactTlvDataObjectApplicationIdentifier value;
        private static readonly ReadOnlyPropertyAdapter<DataObjectApplicationIdentifierAdapter, string> aidAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectApplicationIdentifierAdapter, string> ridAdapter;

        static DataObjectApplicationIdentifierAdapter()
        {
            ObservableObject.IPropertyAdapterFactory<DataObjectApplicationIdentifierAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<DataObjectApplicationIdentifierAdapter>();

            DataObjectApplicationIdentifierAdapter.aidAdapter = PropertyFactory.Create(
                nameof(DataObjectApplicationIdentifierAdapter.Aid),
                instance => instance.value.Aid.ToHexString()
                );
            DataObjectApplicationIdentifierAdapter.ridAdapter = PropertyFactory.Create(
                nameof(DataObjectApplicationIdentifierAdapter.Rid),
                instance => DataObjectApplicationIdentifierAdapter.GetRidInfo(instance.value.Aid)
                );
        }

        public string Rid => DataObjectApplicationIdentifierAdapter.ridAdapter.GetValue(this);

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
            get { return DataObjectApplicationIdentifierAdapter.aidAdapter.GetValue(this); }
            set {
                Helper.SetAsHexValue(value, 0,15,_=>this.value.Aid=_);
            }
        }

        public DataObjectApplicationIdentifierAdapter(CompactTlvDataObjectApplicationIdentifier value):base(value)
        {
            this.value = value;
        }
    }
}
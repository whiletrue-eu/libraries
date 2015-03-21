using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls.ATRView
{
    public class AtrDirDataReferenceHistoricalBytesAdapter : AtrHistoricalBytesAdapterBase
    {
        private readonly AtrDirDataReferenceHistoricalCharacters historicalCharacters;
        private static readonly PropertyAdapter<AtrDirDataReferenceHistoricalBytesAdapter, string> dirDataReferenceAdapter;

        static AtrDirDataReferenceHistoricalBytesAdapter()
        {
            IPropertyAdapterFactory<AtrDirDataReferenceHistoricalBytesAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<AtrDirDataReferenceHistoricalBytesAdapter>();

            dirDataReferenceAdapter = PropertyFactory.Create(
                @this => @this.DirDataReference,
                @this => @this.historicalCharacters.DirDataReference.ToHexString(),
                (@this, value) => Helper.SetAsHexByteValue(value, _ => @this.historicalCharacters.DirDataReference=_)
                );
        }

        public string DirDataReference
        {
            get { return dirDataReferenceAdapter.GetValue(this); }
            set
            {
                dirDataReferenceAdapter.SetValue(this,value); 
            }
        }

        public AtrDirDataReferenceHistoricalBytesAdapter(AtrDirDataReferenceHistoricalCharacters historicalCharacters, InterpretedAtrAdapter interpretedAtr)
            : base(historicalCharacters, interpretedAtr)
        {
            this.historicalCharacters = historicalCharacters;
        }
    }
}
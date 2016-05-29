using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls.ATRViewerControl.Model
{
    public class AtrDirDataReferenceHistoricalBytesAdapter : AtrHistoricalBytesAdapterBase
    {
        private readonly AtrDirDataReferenceHistoricalCharacters historicalCharacters;
        private static readonly PropertyAdapter<AtrDirDataReferenceHistoricalBytesAdapter, string> dirDataReferenceAdapter;

        static AtrDirDataReferenceHistoricalBytesAdapter()
        {
            ObservableObject.IPropertyAdapterFactory<AtrDirDataReferenceHistoricalBytesAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<AtrDirDataReferenceHistoricalBytesAdapter>();

            AtrDirDataReferenceHistoricalBytesAdapter.dirDataReferenceAdapter = PropertyFactory.Create(
                nameof(AtrDirDataReferenceHistoricalBytesAdapter.DirDataReference),
                instance => instance.historicalCharacters.DirDataReference.ToHexString(),
                (instance, value) => Helper.SetAsHexByteValue(value, _ => instance.historicalCharacters.DirDataReference=_)
                );
        }

        public string DirDataReference
        {
            get { return AtrDirDataReferenceHistoricalBytesAdapter.dirDataReferenceAdapter.GetValue(this); }
            set
            {
                AtrDirDataReferenceHistoricalBytesAdapter.dirDataReferenceAdapter.SetValue(this,value); 
            }
        }

        public AtrDirDataReferenceHistoricalBytesAdapter(AtrDirDataReferenceHistoricalCharacters historicalCharacters, InterpretedAtrAdapter interpretedAtr)
            : base(historicalCharacters, interpretedAtr)
        {
            this.historicalCharacters = historicalCharacters;
        }
    }
}
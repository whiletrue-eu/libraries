using System.Linq;
using WhileTrue.Classes.ATR.Tokenized;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls.ATRViewerControl.Model
{
    public class AtrTokenAdapterBase : ObservableObject
    {
        private readonly IAtrToken atrToken;
        private static readonly ReadOnlyPropertyAdapter<AtrTokenAdapterBase, string> bytesAdapter;

        static AtrTokenAdapterBase()
        {
            IPropertyAdapterFactory<AtrTokenAdapterBase> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<AtrTokenAdapterBase>();
            
            AtrTokenAdapterBase.bytesAdapter = PropertyFactory.Create(
                nameof(AtrTokenAdapterBase.Bytes),
                instance => instance.atrToken != null ? instance.atrToken.Bytes.ToHexString(" ") : null
                );            
        }
        public AtrTokenAdapterBase(IAtrToken atrToken)
        {
            this.atrToken = atrToken;
        }

        public string Bytes => AtrTokenAdapterBase.bytesAdapter.GetValue(this);

        protected static string ToString(bool taExists, bool tbExists, bool tcExists, bool tdExists)
        {
            return 
                       taExists||tbExists||tcExists||tdExists
                             ? string.Join(", ", new[]
                                                     {
                                                         taExists ? "TA" : null,
                                                         tbExists ? "TB" : null,
                                                         tcExists ? "TC" : null,
                                                         tdExists ? "TD" : null
                                                     }.Where(_ => _ != null).ToArray())
                             : "none";
        }

    }
}
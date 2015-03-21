using System;
using System.Linq;
using WhileTrue.Classes.ATR.Tokenized;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls.ATRView
{
    public class AtrTokenAdapterBase : ObservableObject
    {
        private readonly IAtrToken atrToken;
        private static readonly ReadOnlyPropertyAdapter<AtrTokenAdapterBase, string> bytesAdapter;

        static AtrTokenAdapterBase()
        {
            IPropertyAdapterFactory<AtrTokenAdapterBase> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<AtrTokenAdapterBase>();
            
            bytesAdapter = PropertyFactory.Create(
                @this => @this.Bytes,
                @this => @this.atrToken != null ? @this.atrToken.Bytes.ToHexString(" ") : null
                );            
        }
        public AtrTokenAdapterBase(IAtrToken atrToken)
        {
            this.atrToken = atrToken;
        }

        public string Bytes
        {
            get { return bytesAdapter.GetValue(this); }
        }

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
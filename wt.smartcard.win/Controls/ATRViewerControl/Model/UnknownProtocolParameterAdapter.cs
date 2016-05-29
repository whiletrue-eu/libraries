using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls.ATRViewerControl.Model
{
    public class UnknownProtocolParameterAdapter : ProtocolParameterAdapterBase
    {
        private readonly UnknownProtocolParameters protocolParameters;
        private static readonly ReadOnlyPropertyAdapter<UnknownProtocolParameterAdapter,EnumerationAdapter<ProtocolType>> protocolTypeAdapter;

        static UnknownProtocolParameterAdapter()
        {
            IPropertyAdapterFactory<UnknownProtocolParameterAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<UnknownProtocolParameterAdapter>();

            UnknownProtocolParameterAdapter.protocolTypeAdapter = PropertyFactory.Create(
                nameof(UnknownProtocolParameterAdapter.ProtocolType),
                instance => EnumerationAdapter<ProtocolType>.GetInstanceFor(instance.protocolParameters.ProtocolType)
                );   
        }
        public UnknownProtocolParameterAdapter(UnknownProtocolParameters protocolParameters)
            : base(protocolParameters)
        {
            this.protocolParameters = protocolParameters;
        }

        public EnumerationAdapter<ProtocolType> ProtocolType => UnknownProtocolParameterAdapter.protocolTypeAdapter.GetValue(this);
    }
}
using System;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls.ATRView
{
    public class UnknownProtocolParameterAdapter : ProtocolParameterAdapterBase
    {
        private readonly UnknownProtocolParameters protocolParameters;
        private static readonly ReadOnlyPropertyAdapter<UnknownProtocolParameterAdapter,EnumerationAdapter<ProtocolType>> protocolTypeAdapter;

        static UnknownProtocolParameterAdapter()
        {
            IPropertyAdapterFactory<UnknownProtocolParameterAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<UnknownProtocolParameterAdapter>();

            protocolTypeAdapter = PropertyFactory.Create(
                @this => @this.ProtocolType,
                @this => EnumerationAdapter<ProtocolType>.GetInstanceFor(@this.protocolParameters.ProtocolType)
                );   
        }
        public UnknownProtocolParameterAdapter(UnknownProtocolParameters protocolParameters)
            : base(protocolParameters)
        {
            this.protocolParameters = protocolParameters;
        }

        public EnumerationAdapter<ProtocolType> ProtocolType
        {
            get { return protocolTypeAdapter.GetValue(this); }
        }
    }
}
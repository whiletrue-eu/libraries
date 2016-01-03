using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls.ATRViewerControl.Model
{
    public class T0ProtocolParameterAdapter : ProtocolParameterAdapterBase
    {
        private readonly T0ProtocolParameters protocolParameters;
        private static readonly PropertyAdapter<T0ProtocolParameterAdapter,byte> waitingTimeIntegerAdapter;
        private static readonly ReadOnlyPropertyAdapter<T0ProtocolParameterAdapter,bool> waitingTimeIntegerIsDefaultAdapter;

        static T0ProtocolParameterAdapter()
        {
            IPropertyAdapterFactory<T0ProtocolParameterAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<T0ProtocolParameterAdapter>();
            T0ProtocolParameterAdapter.waitingTimeIntegerAdapter = PropertyFactory.Create(
                nameof(T0ProtocolParameterAdapter.WaitingTimeInteger),
                instance => instance.protocolParameters.WaitingTimeIntegerValue,          // read value or default
                (instance, value)=>instance.protocolParameters.WaitingTimeInteger=value   // set fix value
                );
            T0ProtocolParameterAdapter.waitingTimeIntegerIsDefaultAdapter = PropertyFactory.Create(
                nameof(T0ProtocolParameterAdapter.WaitingTimeIntegerIsDefault),
                instance => instance.protocolParameters.WaitingTimeInteger.HasValue == false
                );            
        }

        public T0ProtocolParameterAdapter(T0ProtocolParameters protocolParameters) : base(protocolParameters)
        {
            this.protocolParameters = protocolParameters;
        }

        public bool WaitingTimeIntegerIsDefault
        {
            get { return T0ProtocolParameterAdapter.waitingTimeIntegerIsDefaultAdapter.GetValue(this); }
            set {
                if (value)
                {
                    this.protocolParameters.WaitingTimeInteger = null;
                }
                else
                {
                    this.protocolParameters.WaitingTimeInteger = this.protocolParameters.WaitingTimeIntegerValue;
                }
            }
        }

        public byte WaitingTimeInteger
        {
            get { return T0ProtocolParameterAdapter.waitingTimeIntegerAdapter.GetValue(this); }
            set { T0ProtocolParameterAdapter.waitingTimeIntegerAdapter.SetValue(this,value); }
        }
    }
}
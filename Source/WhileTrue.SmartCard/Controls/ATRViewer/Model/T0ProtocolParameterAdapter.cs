using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls.ATRView
{
    public class T0ProtocolParameterAdapter : ProtocolParameterAdapterBase
    {
        private readonly T0ProtocolParameters protocolParameters;
        private static readonly PropertyAdapter<T0ProtocolParameterAdapter,byte> waitingTimeIntegerAdapter;
        private static readonly ReadOnlyPropertyAdapter<T0ProtocolParameterAdapter,bool> waitingTimeIntegerIsDefaultAdapter;

        static T0ProtocolParameterAdapter()
        {
            IPropertyAdapterFactory<T0ProtocolParameterAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<T0ProtocolParameterAdapter>();
            waitingTimeIntegerAdapter = PropertyFactory.Create(
                @this => @this.WaitingTimeInteger,
                @this => @this.protocolParameters.WaitingTimeIntegerValue,          // read value or default
                (@this, value)=>@this.protocolParameters.WaitingTimeInteger=value   // set fix value
                );
            waitingTimeIntegerIsDefaultAdapter = PropertyFactory.Create(
                @this => @this.WaitingTimeIntegerIsDefault,
                @this => @this.protocolParameters.WaitingTimeInteger.HasValue == false
                );            
        }

        public T0ProtocolParameterAdapter(T0ProtocolParameters protocolParameters) : base(protocolParameters)
        {
            this.protocolParameters = protocolParameters;
        }

        public bool WaitingTimeIntegerIsDefault
        {
            get { return waitingTimeIntegerIsDefaultAdapter.GetValue(this); }
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
            get { return waitingTimeIntegerAdapter.GetValue(this); }
            set { waitingTimeIntegerAdapter.SetValue(this,value); }
        }
    }
}
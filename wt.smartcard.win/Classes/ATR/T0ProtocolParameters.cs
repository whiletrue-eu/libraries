using System.Linq;
using WhileTrue.Classes.ATR.Tokenized;

namespace WhileTrue.Classes.ATR
{
    public sealed class T0ProtocolParameters : ProtocolParametersBase
    {
        public T0ProtocolParameters(Atr owner)
            : base(owner, ProtocolType.T0, _=>_.Type==(InterfaceByteGroupType) ProtocolType.T0)
        {
        }

        public byte? WaitingTimeInteger
        {
            get
            {
                return this.GetInterfaceByte(0, InterfaceByteType.Tc);
            }
            set
            {
                this.SetInterfaceByte(0, InterfaceByteType.Tc, value);
                this.InvokePropertyChanged(nameof(T0ProtocolParameters.WaitingTimeInteger));
                this.InvokePropertyChanged(nameof(T0ProtocolParameters.WaitingTimeIntegerValue));
            }
        }

        public byte WaitingTimeIntegerValue => this.GetInterfaceByte(0, InterfaceByteType.Tc) ?? 10;

        public override System.Collections.Generic.IEnumerable<ParameterByte> ParameterBytes
        {
            get
            {
                foreach (AtrInterfaceByteGroupToken InterfaceByteGroup in this.GetInterfaceGroups())
                {
                    if (InterfaceByteGroup.Number == 2)
                    {
                        //Ta2 and Tb2 are global
                        yield return new ParameterByte(ParameterByte.ValueIndicator.Irrelevant);
                        yield return new ParameterByte(ParameterByte.ValueIndicator.Irrelevant);
                        yield return this.GetInterfaceByte(0, InterfaceByteType.Tc);
                    }
                    else
                    {
                        yield return InterfaceByteGroup.Ta;
                        yield return InterfaceByteGroup.Tb;
                        yield return InterfaceByteGroup.Tc;
                    }
                }

            }
        }

        public override bool IsApplicable
        {
            get
            {
                return base.IsApplicable ||
                       this.Owner.TokenizedAtr.InterfaceByteGroups.Any(_ => _.Type !=InterfaceByteGroupType.Global && _.Type != InterfaceByteGroupType.GlobalExtended )== false; //T=0 indicated by default

            }
        }

        public override void NotifyAtrChanged()
        {
            base.NotifyAtrChanged();
            this.InvokePropertyChanged(nameof(T0ProtocolParameters.WaitingTimeInteger));
            this.InvokePropertyChanged(nameof(T0ProtocolParameters.WaitingTimeIntegerValue));
        }
    }
}
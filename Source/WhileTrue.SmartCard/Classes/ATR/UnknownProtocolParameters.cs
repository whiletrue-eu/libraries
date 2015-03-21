using WhileTrue.Classes.ATR.Tokenized;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.ATR
{
    public sealed class UnknownProtocolParameters : ProtocolParametersBase
    {
        public UnknownProtocolParameters(Atr owner, ProtocolType protocolType)
            : base(owner, protocolType, _ => _.Type == (InterfaceByteGroupType)protocolType && _.Number != 2)
        // if not T=0 is indicated, the T=0 specific byte should not be set. In this case, there cannot be protocol specific information
        // coded in this group. This means, the T=x specific protocol indicated here has to be indicated again in a following group if data has to be set
        {
            protocolType.DbC_Assure(value => value != ProtocolType.T0 && value != ProtocolType.T1);
        }
    }
}
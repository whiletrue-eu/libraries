using WhileTrue.Facades.SmartCard;
using WhileTrue.Types.SmartCard;

namespace WhileTrue.Facades.SmartCardChannels
{
    public interface ITpduChannel : ISmartCardChannel
    {
        void Connect(Protocol protocol);
        void Disconnect();
        void Eject();
        CardResponse Transmit(CardCommand command);
    }
}
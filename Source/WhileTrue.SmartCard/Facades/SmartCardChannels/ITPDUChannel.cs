using WhileTrue.Types.SmartCard;

namespace WhileTrue.Facades.SmartCard.Channels
{
    public interface ITPDUChannel : ISmartCardChannel
    {
        void Connect(Protocol protocol);
        void Disconnect();
        void Eject();
        CardResponse Transmit(CardCommand command);
    }
}
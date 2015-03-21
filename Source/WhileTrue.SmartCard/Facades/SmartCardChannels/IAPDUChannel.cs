using WhileTrue.Types.SmartCard;

namespace WhileTrue.Facades.SmartCard.Channels
{
    public interface IAPDUChannel : ISmartCardChannel
    {
        void Connect();
        void Disconnect();
        void Eject();
        CardResponse Transmit(CardCommand command);
    }
}
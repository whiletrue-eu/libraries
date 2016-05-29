using WhileTrue.Types.SmartCard;

namespace WhileTrue.Facades.SmartCardChannels
{
    public interface IApduChannel : ISmartCardChannel
    {
        void Connect();
        void Disconnect();
        void Eject();
        CardResponse Transmit(CardCommand command);
    }
}
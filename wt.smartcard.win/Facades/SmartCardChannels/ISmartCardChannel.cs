using WhileTrue.Facades.SmartCard;

namespace WhileTrue.Facades.SmartCardChannels
{
    public interface ISmartCardChannel
    {
        ISmartCard SmartCard { get; }
    }
}
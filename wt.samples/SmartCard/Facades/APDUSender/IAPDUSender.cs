using WhileTrue.Classes.Components;

namespace WhileTrue.SmartCard.Facades.APDUSender
{
    [ComponentInterface]
    internal interface IApduSender
    {
        void Open();
    }
}

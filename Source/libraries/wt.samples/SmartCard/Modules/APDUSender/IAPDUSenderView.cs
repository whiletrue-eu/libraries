using WhileTrue.Classes.Components;

namespace WhileTrue.SmartCard.Modules.APDUSender
{
    [ComponentInterface]
    internal interface IApduSenderView
    {
        IApduSenderModel Model { set; }
        void Open();
    }
}
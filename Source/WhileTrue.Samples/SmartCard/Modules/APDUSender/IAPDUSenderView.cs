using WhileTrue.Classes.Components;

namespace WhileTrue.SmartCard.Modules.APDUSender
{
    [ComponentInterface]
    internal interface IAPDUSenderView
    {
        IAPDUSenderModel Model { set; }
        void Open();
    }
}
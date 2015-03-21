using WhileTrue.Classes.Components;
using WhileTrue.SmartCard.Facades.APDUSender;

namespace WhileTrue.SmartCard.Modules.APDUSender
{
    [Component]
    internal class ApduSenderPresenter : IApduSender
    {
        private readonly IApduSenderView view;

        /// <summary/>
        public ApduSenderPresenter(IApduSenderModel model, IApduSenderView view)
        {
            this.view = view;
            this.view.Model = model;
        }

        public void Open()
        {
            this.view.Open();
        }
    }
}
using WhileTrue.Classes.Components;
using WhileTrue.SmartCard.Facades.APDUSender;

namespace WhileTrue.SmartCard.Modules.APDUSender
{
    [Component]
    internal class APDUSenderPresenter : IAPDUSender
    {
        private readonly IAPDUSenderView view;

        /// <summary/>
        public APDUSenderPresenter(IAPDUSenderModel model, IAPDUSenderView view)
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
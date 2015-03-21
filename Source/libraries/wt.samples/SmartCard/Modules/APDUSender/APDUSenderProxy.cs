using WhileTrue.Classes.Components;
using WhileTrue.SmartCard.Facades.APDUSender;

namespace WhileTrue.SmartCard.Modules.APDUSender
{
    [Component]
    internal class ApduSenderProxy : IApduSender
    {
        private readonly ComponentRepository repository;

        /// <summary/>
        public ApduSenderProxy(ComponentRepository repository)
        {
            this.repository = repository;
        }

        public void Open()
        {
            using(ComponentContainer ComponentContainer = new ComponentContainer(this.repository))
            {
                IApduSender ApduSender = ComponentContainer.ResolveInstance<IApduSender>();
                ApduSender.Open();
            }
        }
    }
}
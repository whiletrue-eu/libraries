using WhileTrue.Classes.Components;
using WhileTrue.SmartCard.Facades.APDUSender;

namespace WhileTrue.SmartCard.Modules.APDUSender
{
    [Component]
    internal class APDUSenderProxy : IAPDUSender
    {
        private readonly ComponentRepository repository;

        /// <summary/>
        public APDUSenderProxy(ComponentRepository repository)
        {
            this.repository = repository;
        }

        public void Open()
        {
            using(ComponentContainer ComponentContainer = new ComponentContainer(this.repository))
            {
                IAPDUSender APDUSender = ComponentContainer.ResolveInstance<IAPDUSender>();
                APDUSender.Open();
            }
        }
    }
}
using System;
using System.Threading.Tasks;

namespace WhileTrue.Classes.Components.TestComponents
{
    [Component]
    internal class NoSuitableConstructor : ITestFacade1
    {
        public NoSuitableConstructor(string unsupportedType)
        {
            
        }
        public NoSuitableConstructor(IUnsupportedInterface iface)
        {
        }
        public NoSuitableConstructor(IUnsupportedInterface[] iface)
        {
        }
        public NoSuitableConstructor(Func<IUnsupportedInterface> iface)
        {
        }
        public NoSuitableConstructor(Func<IUnsupportedInterface[]> iface)
        {
        }
        public NoSuitableConstructor(Task<IUnsupportedInterface> iface)
        {
        }
        public NoSuitableConstructor(Task<IUnsupportedInterface[]> iface)
        {
        }

        internal interface IUnsupportedInterface
        {
        }
    }
}
using System.Collections;

namespace WhileTrue.Classes.Components
{
    internal class ComponentDescriptorCollection : CollectionBase
    {
        public void Add(ComponentDescriptor component)
        {
            this.InnerList.Add(component);
        }
    }
}
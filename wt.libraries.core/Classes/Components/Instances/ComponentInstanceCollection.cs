using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace WhileTrue.Classes.Components
{
    internal class ComponentInstanceCollection
    {
        private readonly Collection<ComponentInstance> innerList = new Collection<ComponentInstance>();

        public ComponentInstance this[ComponentDescriptor componentDescriptor]
        {
            get
            {
                foreach (var ComponentInstance in innerList)
                    if (ComponentInstance.Descriptor == componentDescriptor)
                        return ComponentInstance;

                var NewComponentInstance = componentDescriptor.CreateComponentInstance();
                innerList.Add(NewComponentInstance);
                InvokeComponentInstanceAdded(NewComponentInstance);
                return NewComponentInstance;
            }
        }

        public ComponentInstance[] ToArray()
        {
            return innerList.ToArray();
        }

        public event EventHandler<ComponentInstanceEventArgs> ComponentInstanceAdded = delegate { };

        private void InvokeComponentInstanceAdded(ComponentInstance componentInstance)
        {
            ComponentInstanceAdded(this, new ComponentInstanceEventArgs(componentInstance));
        }
    }
}
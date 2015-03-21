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
                foreach (ComponentInstance ComponentInstance in this.innerList)
                {
                    if (ComponentInstance.Descriptor == componentDescriptor)
                    {
                        return ComponentInstance;
                    }
                }

                ComponentInstance NewComponentInstance = componentDescriptor.CreateComponentInstance();
                this.innerList.Add(NewComponentInstance);
                this.InvokeComponentInstanceAdded(NewComponentInstance);
                return NewComponentInstance;
            }
        }

        public void NotifyInstanceCreated(ComponentInstance componentInstance, object instance)
        {
            foreach (ComponentInstance ComponentInstance in this.innerList)
            {
                ComponentInstance.NotifyInstanceCreated(componentInstance, instance);
            }
        }

        public ComponentInstance[] ToArray()
        {
            return this.innerList.ToArray();
        }

        public event EventHandler<ComponentInstanceEventArgs> ComponentInstanceAdded=delegate{};

        private void InvokeComponentInstanceAdded(ComponentInstance componentInstance)
        {
            this.ComponentInstanceAdded(this, new ComponentInstanceEventArgs(componentInstance));
        }
    }
}
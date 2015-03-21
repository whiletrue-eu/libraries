using System;
using System.Collections;

namespace WhileTrue.Classes.Components
{
    internal class ComponentInstanceCollection : CollectionBase
    {
        public ComponentInstance this[ComponentDescriptor componentDescriptor]
        {
            get
            {
                foreach (ComponentInstance ComponentInstance in this)
                {
                    if (ComponentInstance.Descriptor == componentDescriptor)
                    {
                        return ComponentInstance;
                    }
                }

                ComponentInstance NewComponentInstance = componentDescriptor.CreateComponentInstance();
                this.InnerList.Add(NewComponentInstance);
                this.InvokeComponentInstanceAdded(NewComponentInstance);
                return NewComponentInstance;
            }
        }

        public void NotifyInstanceCreated(ComponentInstance componentInstance, object instance)
        {
            foreach (ComponentInstance ComponentInstance in this)
            {
                ComponentInstance.NotifyInstanceCreated(componentInstance, instance);
            }
        }

        public ComponentInstance[] ToArray()
        {
            return (ComponentInstance[]) this.InnerList.ToArray(typeof (ComponentInstance));
        }

        public event EventHandler<ComponentInstanceEventArgs> ComponentInstanceAdded=delegate{};

        private void InvokeComponentInstanceAdded(ComponentInstance componentInstance)
        {
            this.ComponentInstanceAdded(this, new ComponentInstanceEventArgs(componentInstance));
        }
    }
}
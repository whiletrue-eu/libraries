using System;

namespace WhileTrue.Classes.Components
{
    internal class SingletonComponentDescriptor : ComponentDescriptor
    {
        internal SingletonComponentDescriptor(ComponentRepository componentRepository, Type type, object config, ComponentRepository privateRepository)
            : base(componentRepository, type, config, privateRepository)
        {
        }

        internal override ComponentInstance CreateComponentInstance()
        {
            return new SingletonComponentInstance(this);
        }
    }
}
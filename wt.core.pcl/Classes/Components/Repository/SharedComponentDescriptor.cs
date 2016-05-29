using System;

namespace WhileTrue.Classes.Components
{
    internal class SharedComponentDescriptor : ComponentDescriptor
    {
        internal SharedComponentDescriptor(ComponentRepository componentRepository, Type type, object config, ComponentRepository privateRepository)
            : base(componentRepository, type, config, privateRepository)
        {
        }

        internal override ComponentInstance CreateComponentInstance()
        {
            return new SharedComponentInstance(this);
        }
    }
}
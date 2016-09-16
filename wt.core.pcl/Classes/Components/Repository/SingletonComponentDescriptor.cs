using System;
using System.Diagnostics;

namespace WhileTrue.Classes.Components
{
    [DebuggerDisplay("{this.Type.FullName}")]
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
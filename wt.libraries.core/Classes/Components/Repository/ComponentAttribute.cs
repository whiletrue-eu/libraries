using System;
using JetBrains.Annotations;

namespace WhileTrue.Classes.Components
{
    /// <summary>
    ///     Marks a class as an Component implementation
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [MeansImplicitUse]
    public class ComponentAttribute : Attribute
    {
        /// <summary />
        public ComponentAttribute()
        {
            Name = null;
        }

        /// <summary />
        public ComponentAttribute(string name)
        {
            Name = name;
        }

        /// <summary />
        public string Name { get; }
    }
}
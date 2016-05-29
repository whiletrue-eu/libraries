using System;
using JetBrains.Annotations;

namespace WhileTrue.Classes.Components
{
    /// <summary>
    /// Marks a class as an Component implementation
    /// </summary>
    [AttributeUsage(AttributeTargets.Class), MeansImplicitUse]
    public class ComponentAttribute : Attribute
    {
        /// <summary/>
        public ComponentAttribute()
        {
            this.Name = null;
        }

        /// <summary/>
        public ComponentAttribute(string name)
        {
            this.Name = name;
        }

        /// <summary/>
        public string Name { get; }
    }
}
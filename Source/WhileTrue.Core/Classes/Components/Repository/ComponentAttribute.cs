using System;
using WhileTrue.Classes.CodeInspection;

namespace WhileTrue.Classes.Components
{
    /// <summary>
    /// Marks a class as an Component implementation
    /// </summary>
    [AttributeUsage(AttributeTargets.Class), MeansImplicitUse]
    public class ComponentAttribute : Attribute
    {
        private readonly string name;

        /// <summary/>
        public ComponentAttribute()
        {
            this.name = null;
        }

        /// <summary/>
        public ComponentAttribute(string name)
        {
            this.name = name;
        }

        /// <summary/>
        public string Name
        {
            get { return this.name; }
        }
    }
}
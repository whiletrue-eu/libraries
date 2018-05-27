using System;
using JetBrains.Annotations;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Components
{
    /// <summary>
    /// Marks a class as an Component implementation
    /// </summary>
    [AttributeUsage(AttributeTargets.Class), MeansImplicitUse]
    public class ComponentAttribute : Attribute
    {
        /// <summary/>
        public ComponentAttribute(string name = null)
        {
            this.Name = name;
        }

        /// <summary/>
        public string Name { get; }

        /// <summary>
        /// Defines whether the component can be created using a background thread or whether it needs to be created on the UI Thread.
        /// Default is 'automatic', which means the framework tries to guess by inspecing the base classes of the component
        /// </summary>

        public ThreadAffinity ThreadAffinity { get; set; } = ThreadAffinity.Automatic;

        /// <summary>
        /// Returns the ComponenAttribute associated to the given type. Throws exception if there is not attribute defined
        /// </summary>
        public static ComponentAttribute FromType(Type type)
        {
            ComponentAttribute[] Attributes = (ComponentAttribute[])type.GetCustomAttributes<ComponentAttribute>();
            if (Attributes.Length != 1)
            {
                throw new ArgumentException($"'{type.FullName}' does not have a '[Component]' attribute declared.");
            }
            else
            {
                return Attributes[0];
            }
        }
    }
}
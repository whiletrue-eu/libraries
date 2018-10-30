using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace WhileTrue.Classes.Components
{
    /// <summary>
    ///     Marks a property of a component implementation as ComponentBinding.
    ///     Component binding properties can be used as replacement for interface implementations (get only, private set)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    [MeansImplicitUse]
    public class ComponentBindingPropertyAttribute : Attribute
    {
        /// <summary />
        public static bool IsSetFor(PropertyInfo property)
        {
            return property.GetCustomAttributes(typeof(ComponentBindingPropertyAttribute), true).Any();
        }
    }
}
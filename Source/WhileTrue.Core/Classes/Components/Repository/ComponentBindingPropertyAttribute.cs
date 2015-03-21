using System;
using System.Reflection;
using WhileTrue.Classes.CodeInspection;

namespace WhileTrue.Classes.Components
{
    /// <summary>
    /// Marks a property of a component implementation as ComponentBinding.
    /// Component binding properties can be used as replacement for interface implementations (get) and
    /// constructor dependencies (set) which support delayed assignment (in case a recursive dependency must be handled)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property),MeansImplicitUse]
    public class ComponentBindingPropertyAttribute : Attribute
    {
        ///<summary/>
        public static bool IsSetFor(PropertyInfo property)
        {
            return property.GetCustomAttributes(typeof(ComponentBindingPropertyAttribute), true).Length > 0;
        }
    }
}
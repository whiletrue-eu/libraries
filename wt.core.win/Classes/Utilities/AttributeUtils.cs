// ReSharper disable UnusedMember.Global
using System;
using System.Reflection;

namespace WhileTrue.Classes.Utilities
{
    /// <summary>
    /// Provides utility methods for simple, typed attribute access
    /// </summary>
    public static class AttributeUtils
    {
        /// <summary/>
        public static TAttributeType[] GetCustomAttributes<TAttributeType>( this Assembly value ) where TAttributeType : Attribute
        {
            TAttributeType[] Attributes = (TAttributeType[]) value?.GetCustomAttributes(typeof(TAttributeType), true);
            return Attributes;
        }
    }
}

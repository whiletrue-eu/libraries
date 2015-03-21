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
        public static AttributeType[] GetCustomAttributes<AttributeType>( this Assembly value ) where AttributeType : Attribute
        {
            if (value != null)
            {
                AttributeType[] Attributes = (AttributeType[])value.GetCustomAttributes(typeof(AttributeType), true);
                return Attributes;
            }
            else
            {
                return null; 
            }
        }
#if !NET45
        /// <summary/>
        public static AttributeType GetCustomAttribute<AttributeType>(this Assembly value) where AttributeType : Attribute
        {
            if (value != null)
            {
                AttributeType[] Attributes = (AttributeType[]) value.GetCustomAttributes(typeof(AttributeType), true);
                return Attributes.Length == 1 ? Attributes[0] : null;
            }
            else
            {
                return null;
            }
        }
#endif
    }
}

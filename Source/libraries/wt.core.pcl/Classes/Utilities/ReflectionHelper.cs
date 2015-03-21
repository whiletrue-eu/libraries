using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WhileTrue.Classes.Utilities
{
    internal static class ReflectionHelper
    {
        public static ConstructorInfo GetConstructor(this Type type, Type[] paramTypes )
        {
            return type.GetTypeInfo()
                .DeclaredConstructors.FirstOrDefault(
                    constructor => paramTypes.HasEqualValue(
                        constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray()));
        }
        public static IEnumerable<ConstructorInfo> GetConstructors(this Type type )
        {
            return type.GetTypeInfo().DeclaredConstructors;
        }

        public static IEnumerable<TAttribute> GetCustomAttributes<TAttribute>(this Type type) where TAttribute:Attribute
        {
            return type.GetTypeInfo().GetCustomAttributes<TAttribute>();
        }

        public static Type GetInterface( this Type type, string fullName )
        {
            return type.GetTypeInfo().ImplementedInterfaces.FirstOrDefault(iface => iface.FullName == fullName);
        }
        public static IEnumerable<Type> GetInterfaces( this Type type )
        {
            return type.GetTypeInfo().ImplementedInterfaces;
        }

        public static bool IsInterface(this Type type)
        {
            return type.GetTypeInfo().IsInterface;
        }

        public static bool IsInstanceOfType(this Type type, object value)
        {
            return type.GetTypeInfo().IsAssignableFrom(value.GetType().GetTypeInfo());
        }
        public static bool IsAssignableFrom(this Type type, Type otherType)
        {
            return type.GetTypeInfo().IsAssignableFrom(otherType.GetTypeInfo());
        }
        public static bool IsGenericType(this Type type)
        {
            return type.GetTypeInfo().IsGenericType;
        }


    }
}

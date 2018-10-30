// ReSharper disable UnusedMember.Global

using System;
using System.Reflection;

namespace WhileTrue.Classes.Utilities
{
    /// <summary>
    ///     Provides simple access to private members. Be aware that the internal design of classes can change without notice
    ///     and that the
    ///     usage of  through this class will not be checked through the compiler
    /// </summary>
    public static class PrivateAccess
    {
        /// <summary>
        ///     Retrieve a private access handler on the given object
        /// </summary>
        public static Handler PrivateMembers(this object source)
        {
            return new Handler(source, source.GetType(), BindingFlags.Instance);
        }

        /// <summary>
        ///     Retrieve a private access handler on the given type for static fields
        /// </summary>
        public static Handler PrivateMembers(this Type source)
        {
            return new Handler(null, source, BindingFlags.Static);
        }


        /// <summary>
        ///     Implements the wrappers for reflection on the given object
        /// </summary>
        public class Handler
        {
            private readonly object source;
            private readonly BindingFlags sourceAccessType;
            private readonly Type sourceType;
            private readonly Type type;

            internal Handler(object source, Type sourceType, BindingFlags sourceAccessType)
            {
                this.source = source;
                this.sourceType = sourceType;
                this.sourceAccessType = sourceAccessType;
                type = source.GetType();
            }

            /// <summary />
            public TFieldType GetField<TFieldType>(string fieldName)
            {
                var Field = sourceType.GetField(fieldName, sourceAccessType | BindingFlags.NonPublic);

                if (Field == null)
                    throw new MemberAccessException($"Field '{fieldName}' not found on type '{sourceType.FullName}'");

                return (TFieldType) Field.GetValue(source);
            }

            /// <summary />
            public TPropertyType GetProperty<TPropertyType>(string propertyName)
            {
                var Property = sourceType.GetProperty(propertyName, sourceAccessType | BindingFlags.NonPublic);

                if (Property == null)
                    throw new MemberAccessException(
                        $"Property '{propertyName}' not found on type '{sourceType.FullName}'");
                if (Property.CanRead == false)
                    throw new MemberAccessException($"Property '{propertyName}' cannot be read");

                return (TPropertyType) Property.GetValue(source, new object[0]);
            }

            /// <summary />
            public object Call(string methodName, params object[] parameter)
            {
                return type.GetMethod(methodName, sourceAccessType | BindingFlags.NonPublic).Invoke(source, parameter);
            }
        }
    }
}
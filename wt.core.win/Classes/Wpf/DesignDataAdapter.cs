using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;

namespace WhileTrue.Classes.Wpf
{
    public class DesignDataAdapterExtension : MarkupExtension
    {
        private readonly Type baseType;

        public DesignDataAdapterExtension(Type baseType)
        {
            this.baseType = baseType;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new TypeWrapper(baseType);
        }

        private class TypeWrapper : ICustomTypeDescriptor
        {
            private readonly Type baseType;

            public TypeWrapper(Type baseType)
            {
                this.baseType = baseType;
            }

            private class PropertyWrapper : PropertyDescriptor
            {
                private static readonly Dictionary<Type, Func<string, object>> valueCreators;

                private readonly PropertyInfo propertyInfo;

                static PropertyWrapper()
                {
                    try
                    {
                        valueCreators = new Dictionary<Type, Func<string, object>>
                        {
                            {typeof(bool), _ => false},
                            {typeof(bool[]), _ => new[] {false, true}},
                            {typeof(byte), _ => (byte) 42},
                            {typeof(byte[]), _ => new byte[] {42, 21}},
                            {typeof(char), _ => 'A'},
                            {typeof(char[]), _ => new[] {'A', 'B', 'C'}},
                            {typeof(DateTime), _ => DateTime.Now},
                            {typeof(DateTime[]), _ => new[] {DateTime.Now, DateTime.Now.AddDays(1)}},
                            {typeof(TimeSpan), _ => DateTime.Now - DateTime.Today},
                            {
                                typeof(TimeSpan[]),
                                _ => new[] {DateTime.Now - DateTime.Today, DateTime.Now - DateTime.UtcNow}
                            },
                            {typeof(DateTimeOffset), _ => (DateTimeOffset) DateTime.Now},
                            {typeof(DateTimeOffset[]), _ => new DateTimeOffset[] {DateTime.Now, DateTime.Today}},
                            {typeof(decimal), _ => (decimal) 42},
                            {typeof(decimal[]), _ => new decimal[] {42, 21}},
                            {typeof(double), _ => (double) 42},
                            {typeof(double[]), _ => new[] {42, 4.2}},
                            {typeof(Guid), _ => Guid.NewGuid()},
                            {typeof(Guid[]), _ => new[] {Guid.NewGuid(), Guid.NewGuid()}},
                            {typeof(short), _ => (short) 42},
                            {typeof(short[]), _ => new short[] {42, 21}},
                            {typeof(int), _ => 42},
                            {typeof(int[]), _ => new[] {42, 21}},
                            {typeof(long), _ => (long) 42},
                            {typeof(long[]), _ => new long[] {42, 21}},
                            {typeof(sbyte), _ => (sbyte) 42},
                            {typeof(sbyte[]), _ => new sbyte[] {42, 21}},
                            {typeof(float), _ => (float) 42},
                            {typeof(float[]), _ => new[] {42, 4.2f}},
                            {typeof(ushort), _ => (ushort) 42},
                            {typeof(ushort[]), _ => new ushort[] {42, 21}},
                            {typeof(uint), _ => (uint) 42},
                            {typeof(uint[]), _ => new uint[] {42, 21}},
                            {typeof(ulong), _ => (ulong) 42},
                            {typeof(ulong[]), _ => new ulong[] {42, 21}},
                            {typeof(string), property => string.Format("[{0}]", property)}
                        };
                    }
// ReSharper disable EmptyGeneralCatchClause
                    catch
                    {
                    }
// ReSharper restore EmptyGeneralCatchClause
                }

                public PropertyWrapper(Type ownerType, PropertyInfo propertyInfo)
                    : base(propertyInfo.Name, new Attribute[0])
                {
                    ComponentType = ownerType;
                    this.propertyInfo = propertyInfo;
                }

                private static object CreateValue(Type type, string propertyName)
                {
                    if (valueCreators.ContainsKey(type))
                        return valueCreators[type](propertyName);
                    if (type.IsGenericType && typeof(Nullable<>) == type.GetGenericTypeDefinition())
                        return CreateValue(type.GetGenericArguments()[0], propertyName);
                    if (type.IsEnum)
                        return Enum.GetValues(type).Length == 0 ? 0 : Enum.GetValues(type).GetValue(0);
                    return new TypeWrapper(type);
                }

                #region Overrides of PropertyDescriptor

                public override bool CanResetValue(object component)
                {
                    return false;
                }

                public override object GetValue(object component)
                {
                    return CreateValue(propertyInfo.PropertyType, propertyInfo.Name);
                }

                public override void ResetValue(object component)
                {
                }

                public override void SetValue(object component, object value)
                {
                }

                public override bool ShouldSerializeValue(object component)
                {
                    return false;
                }

                public override Type ComponentType { get; }

                public override bool IsReadOnly => true;

                public override Type PropertyType => propertyInfo.PropertyType;

                public override PropertyDescriptorCollection GetChildProperties(object instance, Attribute[] filter)
                {
                    if (instance is ICustomTypeDescriptor)
                        return ((ICustomTypeDescriptor) instance).GetProperties(filter);
                    return base.GetChildProperties(instance, filter);
                }

                #endregion
            }

            #region Implementation of ICustomTypeDescriptor

            public AttributeCollection GetAttributes()
            {
                return new AttributeCollection();
            }

            public string GetClassName()
            {
                return baseType.FullName;
            }

            public string GetComponentName()
            {
                return "Design Data Wrapper";
            }

            public TypeConverter GetConverter()
            {
                return null;
            }

            public EventDescriptor GetDefaultEvent()
            {
                return null;
            }

            public PropertyDescriptor GetDefaultProperty()
            {
                return null;
            }

            public object GetEditor(Type editorBaseType)
            {
                return null;
            }

            public EventDescriptorCollection GetEvents()
            {
                return new EventDescriptorCollection(new EventDescriptor[0]);
            }

            public EventDescriptorCollection GetEvents(Attribute[] attributes)
            {
                return new EventDescriptorCollection(new EventDescriptor[0]);
            }

            public PropertyDescriptorCollection GetProperties()
            {
                return new PropertyDescriptorCollection((
                    from Property in baseType.GetProperties(BindingFlags.Instance | BindingFlags.FlattenHierarchy |
                                                            BindingFlags.Public)
                    select new PropertyWrapper(baseType, Property)
                ).ToArray());
            }

            public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
            {
                return GetProperties();
            }

            public object GetPropertyOwner(PropertyDescriptor pd)
            {
                return this;
            }

            #endregion
        }
    }
}
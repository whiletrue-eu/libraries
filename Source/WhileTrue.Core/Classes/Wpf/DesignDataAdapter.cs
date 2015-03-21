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
            return new TypeWrapper(this.baseType);
        }

        private class TypeWrapper : ICustomTypeDescriptor
        {
            private readonly Type baseType;

            public TypeWrapper(Type baseType)
            {
                this.baseType = baseType;
            }

            #region Implementation of ICustomTypeDescriptor

            public AttributeCollection GetAttributes()
            {
                return new AttributeCollection();
            }

            public string GetClassName()
            {
                return this.baseType.FullName;
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
                                                            from Property in this.baseType.GetProperties(BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public)
                                                            select new PropertyWrapper(this.baseType, Property)
                                                        ).ToArray());
            }

            public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
            {
                return this.GetProperties();
            }

            public object GetPropertyOwner(PropertyDescriptor pd)
            {
                return this;
            }

            #endregion

            private class PropertyWrapper : PropertyDescriptor
            {
                private static readonly Dictionary<Type, Func<string, object>> valueCreators;
                static PropertyWrapper()
                {
                    try
                    {
                        valueCreators = new Dictionary<Type, Func<string, object>>
                                                                                      {
                                                                                          {typeof (Boolean), _ => false},
                                                                                          {typeof (Boolean[]), _ => new[]{false,true}},
                                                                                          {typeof (Byte), _ => (Byte) 42},
                                                                                          {typeof (Byte[]), _ => new byte[]{42,21}},
                                                                                          {typeof (Char), _ => 'A'},
                                                                                          {typeof (Char[]), _ => new[]{'A','B','C'}},
                                                                                          {typeof (DateTime), _ => DateTime.Now},
                                                                                          {typeof (DateTime[]), _ => new[]{DateTime.Now,DateTime.Now.AddDays(1)}},
                                                                                          {typeof (TimeSpan), _ => DateTime.Now - DateTime.Today},
                                                                                          {typeof (TimeSpan[]), _ => new[]{DateTime.Now - DateTime.Today, DateTime.Now-DateTime.UtcNow}},
                                                                                          {typeof (DateTimeOffset), _ => (DateTimeOffset) DateTime.Now},
                                                                                          {typeof (DateTimeOffset[]), _ => new DateTimeOffset[]{DateTime.Now,DateTime.Today}},
                                                                                          {typeof (Decimal), _ => (Decimal) 42},
                                                                                          {typeof (Decimal[]), _ => new Decimal[]{ 42,21 }},
                                                                                          {typeof (Double), _ => (Double) 42},
                                                                                          {typeof (Double[]), _ => new[]{42,4.2}},
                                                                                          {typeof (Guid), _ => Guid.NewGuid()},
                                                                                          {typeof (Guid[]), _ => new[]{Guid.NewGuid(),Guid.NewGuid()}},
                                                                                          {typeof (Int16), _ => (Int16) 42},
                                                                                          {typeof (Int16[]), _ => new Int16[]{ 42,21}},
                                                                                          {typeof (Int32), _ => 42},
                                                                                          {typeof (Int32[]), _ => new[]{ 42,21}},
                                                                                          {typeof (Int64), _ => (Int64) 42},
                                                                                          {typeof (Int64[]), _ => new Int64[]{ 42,21}},
                                                                                          {typeof (SByte), _ => (SByte) 42},
                                                                                          {typeof (SByte[]), _ => new SByte[]{ 42,21}},
                                                                                          {typeof (Single), _ => (Single) 42},
                                                                                          {typeof (Single[]), _ => new[]{ 42, 4.2f}},
                                                                                          {typeof (UInt16), _ => (UInt16) 42},
                                                                                          {typeof (UInt16[]), _ => new UInt16[]{ 42,21}},
                                                                                          {typeof (UInt32), _ => (UInt32) 42},
                                                                                          {typeof (UInt32[]), _ => new UInt32[]{ 42,21}},
                                                                                          {typeof (UInt64), _ => (UInt64) 42},
                                                                                          {typeof (UInt64[]), _ => new UInt64[]{ 42,21}},
                                                                                          {typeof (string), property => string.Format("[{0}]", property)},
                                                                                      };
                    }
// ReSharper disable EmptyGeneralCatchClause
                    catch { }
// ReSharper restore EmptyGeneralCatchClause
                }

                private static object CreateValue(Type type, string propertyName)
                {
                    if (valueCreators.ContainsKey(type))
                    {
                        return valueCreators[type](propertyName);
                    }
                    else if (type.IsGenericType && typeof(Nullable<>) == type.GetGenericTypeDefinition() ) 
                    {
                        return CreateValue(type.GetGenericArguments()[0],propertyName);
                    }
                    else if (type.IsEnum)
                    {
                        return Enum.GetValues(type).Length == 0 ? 0 : Enum.GetValues(type).GetValue(0);
                    }
                    else
                    {
                        return new TypeWrapper(type);
                    }
                }

                private readonly Type ownerType;
                private readonly PropertyInfo propertyInfo;

                public PropertyWrapper(Type ownerType, PropertyInfo propertyInfo)
                    : base(propertyInfo.Name, new Attribute[0])
                {
                    this.ownerType = ownerType;
                    this.propertyInfo = propertyInfo;
                }

                #region Overrides of PropertyDescriptor

                public override bool CanResetValue(object component)
                {
                    return false;
                }

                public override object GetValue(object component)
                {
                    return CreateValue(this.propertyInfo.PropertyType, this.propertyInfo.Name);
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

                public override Type ComponentType
                {
                    get { return this.ownerType; }
                }

                public override bool IsReadOnly
                {
                    get { return true; }
                }

                public override Type PropertyType
                {
                    get { return this.propertyInfo.PropertyType; }
                }

                public override PropertyDescriptorCollection GetChildProperties(object instance, Attribute[] filter)
                {
                    if (instance is ICustomTypeDescriptor)
                    {
                        return ((ICustomTypeDescriptor) instance).GetProperties(filter);
                    }
                    else
                    {
                        return base.GetChildProperties(instance, filter);
                    }
                }
                
                #endregion
            }
        }

    }
}
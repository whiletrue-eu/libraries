using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace WhileTrue.Classes.Components
{
    public abstract class ComponentDescriptor
    {
        private readonly ComponentRepository componentRepository;
        private readonly object config;
        private readonly string name;
        private readonly ComponentRepository privateRepository;
        private readonly Type configType;
        private readonly Type type;

        internal ComponentDescriptor(ComponentRepository componentRepository, Type type, object config, ComponentRepository privateRepository)
        {
            this.componentRepository = componentRepository;
            this.type = type;
            this.name = GetComponentName(type);
            this.config = config;
            this.configType = config != null ? config.GetType() : null;
            this.privateRepository = privateRepository;
        }

        public Type Type
        {
            get { return this.type; }
        }

        public string Name
        {
            get { return this.name; }
        }

        internal object Config
        {
            get { return this.config; }
        }

        internal Type ConfigType
        {
            get { return this.configType; }
        }

        public ComponentRepository Repository
        {
            get { return this.componentRepository; }
        }

        public ComponentRepository PrivateRepository
        {
            get { return this.privateRepository; }
        }

        public IEnumerable<PropertyInfo> GetLazyInitializeProperties()
        {
                return
                    from Property in this.type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                    where Property.CanWrite &&
                          Property.GetSetMethod(false) != null &&
                          Property.PropertyType.IsInterface &&
                          ComponentRepository.IsComponentInterface(Property.PropertyType) &&
                          ComponentBindingPropertyAttribute.IsSetFor(Property)
                    select Property;

        }

        private IEnumerable<Type> GetProvidedInterfaces()
        {
            return (
                       from InterfaceType in this.type.GetInterfaces()
                       where ComponentRepository.IsComponentInterface(InterfaceType)
                       select InterfaceType
                   ).Union(
                       from Property in this.GetProvidedDelegatedProperties()
                       select Property.PropertyType
                );
        }

        private IEnumerable<PropertyInfo> GetProvidedDelegatedProperties()
        {
            return from Property in this.type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                   where Property.CanRead &&
                         Property.GetGetMethod(false) != null && //must have a public get method
                         Property.PropertyType.IsInterface &&
                         ComponentRepository.IsComponentInterface(Property.PropertyType) &&
                         ComponentBindingPropertyAttribute.IsSetFor(Property)
                   select Property;
        }

        public bool ProvidesInterface(Type interfaceType)
        {
            if (this.GetProvidedInterfaces().Contains(interfaceType))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            return this.name;
        }

        internal abstract ComponentInstance CreateComponentInstance();

        public object TryCastTo(object instance, Type interfaceType)
        {
            if (interfaceType.IsAssignableFrom(this.Type))
            {
                return instance;
            }
            else
            {
                return (
                    from Property in this.GetProvidedDelegatedProperties()
                    where interfaceType.IsAssignableFrom(Property.PropertyType)
                    select Property.GetValue(instance,null)
                    ).FirstOrDefault();
            }
        }

        public Expression TryCastExpressionTo(Expression instance, Type interfaceType)
        {
            if (interfaceType.IsAssignableFrom(this.Type))
            {
                return Expression.Convert(instance, interfaceType);
            }
            else
            {
                return (
                    from Property in this.GetProvidedDelegatedProperties() 
                    where interfaceType.IsAssignableFrom(Property.PropertyType) 
                    select Expression.Property(instance, Property)
                    ).FirstOrDefault();
            }
        }

        private static string GetComponentName(Type type)
        {
            ComponentAttribute[] Attributes = (ComponentAttribute[])type.GetCustomAttributes(typeof(ComponentAttribute), true);
            if (Attributes.Length != 1)
            {
                throw new ArgumentException(string.Format("'{0}' does not have a '[Component]' attribute declared.", type.FullName));
            }
            else
            {
                return Attributes[0].Name ?? type.Name;
            }
        }

    }
}
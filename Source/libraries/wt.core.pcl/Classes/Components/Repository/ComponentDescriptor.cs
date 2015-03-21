using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Components
{
    /// <summary>
    /// Container for metadata details of a registered component
    /// </summary>
    [PublicAPI]
    public abstract class ComponentDescriptor
    {
        internal ComponentDescriptor(ComponentRepository componentRepository, Type type, object config, ComponentRepository privateRepository)
        {
            this.Repository = componentRepository;
            this.Type = type;
            this.Name = ComponentDescriptor.GetComponentName(type);
            this.Config = config;
            this.ConfigType = config?.GetType();
            this.PrivateRepository = privateRepository;
        }

        /// <summary>
        /// runtime type of the registered component
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Name of the component as given in the COmponent attribute
        /// </summary>
        public string Name { get; }

        internal object Config { get; }

        internal Type ConfigType { get; }

        /// <summary>
        /// Repository where this component was registred in
        /// </summary>
        public ComponentRepository Repository { get; }

        /// <summary>
        /// Link to the private repository that is given to the component, when set, other wise <c>null</c>
        /// </summary>
        public ComponentRepository PrivateRepository { get; }

        internal IEnumerable<PropertyInfo> GetLazyInitializeProperties()
        {
                return
                    from Property in this.Type.GetRuntimeProperties()
                    where Property.CanWrite && Property.SetMethod?.IsPublic == true && //must have a puzblic set method
                          Property.PropertyType.IsInterface() &&
                          ComponentRepository.IsComponentInterface(Property.PropertyType) &&
                          ComponentBindingPropertyAttribute.IsSetFor(Property)
                    select Property;

        }

        private IEnumerable<Type> GetProvidedInterfaces()
        {
            return (
                       from InterfaceType in this.Type.GetInterfaces()
                       where ComponentRepository.IsComponentInterface(InterfaceType)
                       select InterfaceType
                   ).Union(
                       from Property in this.GetProvidedDelegatedProperties()
                       select Property.PropertyType
                );
        }

        private IEnumerable<PropertyInfo> GetProvidedDelegatedProperties()
        {
            return from Property in this.Type.GetRuntimeProperties()
                   where Property.CanRead && Property.GetMethod?.IsPublic == true && //must have a public get method
                         Property.PropertyType.IsInterface() &&
                         ComponentRepository.IsComponentInterface(Property.PropertyType) &&
                         ComponentBindingPropertyAttribute.IsSetFor(Property)
                   select Property;
        }

        internal bool ProvidesInterface(Type interfaceType)
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

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return this.Name;
        }

        internal abstract ComponentInstance CreateComponentInstance();

        internal object TryCastTo(object instance, Type interfaceType)
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

        internal Expression TryCastExpressionTo(Expression instance, Type interfaceType)
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
            ComponentAttribute[] Attributes = (ComponentAttribute[])type.GetCustomAttributes<ComponentAttribute>();
            if (Attributes.Length != 1)
            {
                throw new ArgumentException($"'{type.FullName}' does not have a '[Component]' attribute declared.");
            }
            else
            {
                return Attributes[0].Name ?? type.Name;
            }
        }

    }
}
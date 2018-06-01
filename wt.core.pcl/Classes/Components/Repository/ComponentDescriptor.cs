using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using WhileTrue.Classes.CodeInspection;
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
            this.MustCreateOnUiThread = ComponentDescriptor.GetComponentUiThreadAffinity(type, this.Repository);
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

        /// <summary>
        /// States whether the component instance can be created on any thread (including background) or if it must be done on the UI Thread
        /// </summary>
        public bool MustCreateOnUiThread { get; private set; }

        private IEnumerable<PropertyInfo> GetProvidedDelegatedProperties()
        {
            return from Property in this.Type.GetRuntimeProperties()
                   where ComponentBindingPropertyAttribute.IsSetFor(Property) 
                   .DbC_Assure(_=> _==false || Property.CanWrite==false || Property.SetMethod?.IsPublic == false, $"{Property.DeclaringType.Name}.{Property.Name}: ComponentBindingProperty on set attributes no longer supported. Use Constructor Parameter with Func<TInterface> instead for deferred component resolution") &&
                         Property.CanRead && Property.GetMethod?.IsPublic == true && //must have a public get method
                         Property.PropertyType.IsInterface() &&
                         ComponentRepository.IsComponentInterface(Property.PropertyType)
                         
                   select Property;
        }

        [ExcludeFromCodeCoverage]
        internal IEnumerable<Type> GetProvidedInterfaces()
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

        [ExcludeFromCodeCoverage]
        internal IEnumerable<Type> GetRequiredInterfaces()
        {
            return (
                from Constructor in this.Type.GetConstructors()
                from ConstructorParameter in Constructor.GetParameters()
                let ParameterType = ConstructorParameter.ParameterType
                let InterfaceType = ParameterType.IsArray
                    ? ParameterType.GetElementType() //interface array
                    : ParameterType.IsConstructedGenericType && ParameterType.GetGenericTypeDefinition() == typeof(Func<>)
                        ? ParameterType.GenericTypeArguments[0].IsArray
                            ? ParameterType.GenericTypeArguments[0].GetElementType() //func interface array
                            : ParameterType.GenericTypeArguments[0] //func interface
                        : ParameterType // interface
                where ComponentRepository.IsComponentInterface(InterfaceType)
                select InterfaceType
            ).Distinct();
        }

        internal bool ProvidesInterface(Type interfaceType)
        {
            return interfaceType.IsAssignableFrom(this.Type) ||
                   (from Property in this.GetProvidedDelegatedProperties() where interfaceType.IsAssignableFrom(Property.PropertyType) select Property).Any();
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

        private static string GetComponentName(Type type)
        {
            return ComponentAttribute.FromType(type).Name ?? type.Name;
        }

        private static bool GetComponentUiThreadAffinity(Type type, ComponentRepository repository)
        {
            ThreadAffinity ThreadAffinity = ComponentAttribute.FromType(type).ThreadAffinity;
            switch (ThreadAffinity)
            {
                case ThreadAffinity.Automatic:
                    return repository.MustRunOnUiThread(type);
                case ThreadAffinity.NeedsUiThread:
                    return true;
                case ThreadAffinity.SupportsBackground:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
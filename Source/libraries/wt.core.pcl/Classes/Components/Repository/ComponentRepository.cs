// ReSharper disable MemberCanBePrivate.Global
using System;
using System.Collections.ObjectModel;
using System.Linq;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Components
{
    /// <summary>
    /// Implements a repository of component implementations. The components can be used in conjunction with a <see cref="ComponentContainer"/>
    /// which uses this repository as the source for implementations
    /// </summary>
    public class ComponentRepository
    {
        private readonly ComponentRepository parentRepository;
        private readonly Collection<ComponentDescriptor> componentDescriptors = new Collection<ComponentDescriptor>();

        /// <summary/>
        public ComponentRepository()
            :this(null)
        {
        }

        /// <summary>
        /// The parent repository is used when no matching component is found in the current repository
        /// </summary>
        public ComponentRepository(ComponentRepository parentRepository)
        {
            this.parentRepository = parentRepository;
        }
        

        #region AddComponent

        /// <summary>
        /// Add component with <see cref="ComponentInstanceScope.Repository"/> scope
        /// </summary>
        public void AddComponent<TComponentType>() where TComponentType:class
        {
            this.AddComponent<TComponentType>(null, null, ComponentInstanceScope.Repository);
        }

        /// <summary>
        /// Add component with the given scope
        /// </summary>
        public void AddComponent<TComponentType>(ComponentInstanceScope scope) where TComponentType : class
        {
            this.AddComponent<TComponentType>(null, null, scope);
        }

        /// <summary>
        /// Add component with <see cref="ComponentInstanceScope.Repository"/> scope and the given configuration data
        /// </summary>
        public void AddComponent<TComponentType>(object configuration) where TComponentType : class
        {
            this.AddComponent <TComponentType>(configuration, null, ComponentInstanceScope.Repository);
        }

        /// <summary>
        /// Add component with the given scope and the given configuration data
        /// </summary>
        public void AddComponent<TComponentType>(object configuration, ComponentInstanceScope scope) where TComponentType : class
        {
            this.AddComponent<TComponentType>(configuration, null, scope);
        }

        /// <summary>
        /// Add component with <see cref="ComponentInstanceScope.Repository"/> scope using a private
        /// repository which can be used by the component istance
        /// </summary>
        public void AddComponent<TComponentType>(ComponentRepository privateRepository) where TComponentType : class
        {
            this.AddComponent<TComponentType>(null, privateRepository, ComponentInstanceScope.Repository);
        }

        /// <summary>
        /// Add component with the given scope using a private
        /// repository which can be used by the component istance
        /// </summary>
        public void AddComponent<TComponentType>(ComponentRepository privateRepository, ComponentInstanceScope scope) where TComponentType : class
        {
            this.AddComponent<TComponentType>(null, privateRepository, scope);
        }

        /// <summary>
        /// Add component with <see cref="ComponentInstanceScope.Repository"/> scope using a private
        /// repository which can be used by the component instance and the given configuration data
        /// </summary>
        public void AddComponent<TComponentType>(object configuration, ComponentRepository privateRepository) where TComponentType : class
        {
            this.AddComponent<TComponentType>(configuration, privateRepository, ComponentInstanceScope.Repository);
        }

        /// <summary>
        /// Add component with the given scope using a private
        /// repository which can be used by the component instance and the given configuration data
        /// </summary>
        public void AddComponent<TComponentType>(object configuration, ComponentRepository privateRepository, ComponentInstanceScope scope) where TComponentType : class
        {
            switch (scope)
            {
                case ComponentInstanceScope.Container:
                    this.AddComponent(new SimpleComponentDescriptor(this, typeof(TComponentType), configuration, privateRepository));
                    break;
                case ComponentInstanceScope.Repository:
                    this.AddComponent(new SharedComponentDescriptor(this, typeof(TComponentType), configuration, privateRepository));
                    break;
                case ComponentInstanceScope.Global:
                    this.AddComponent(new SingletonComponentDescriptor(this, typeof(TComponentType), configuration, privateRepository));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scope));
            }
        }

        #endregion

        
        #region Resolvers
        #endregion


        private void AddComponent(ComponentDescriptor descriptor)
        {
            DbC.Assure((from ComponentDescriptor ComponentDescriptor in this.componentDescriptors
                        where ComponentDescriptor.Type == descriptor.Type
                        select ComponentDescriptor).Any() == false, "Type already registered as a component: {0}", descriptor.Name);

            this.componentDescriptors.Add(descriptor);
        }


        internal ComponentDescriptor[] GetComponentDescriptors(Type interfaceType)
        {
            ComponentDescriptor[] ComponentDescriptors = (
                                                             from ComponentDescriptor ComponentDescriptor in this.componentDescriptors
                                                             where ComponentDescriptor.ProvidesInterface(interfaceType)
                                                             select ComponentDescriptor
                                                         ).ToArray();

            // Try to resolve in this repository
            if (ComponentDescriptors.Length > 0)
            {
                return ComponentDescriptors;
            }
            // if nothing is found, resolve from parent repository, if given
            else if(this.parentRepository != null )
            {
                return this.parentRepository.GetComponentDescriptors(interfaceType);
            }
            // else return an empty list
            else
            {
                return new ComponentDescriptor[0];
            }
        }

        internal static bool IsComponentInterface(Type interfaceType)
        {
            return interfaceType.GetCustomAttributes<ComponentInterfaceAttribute>().Any();
        }
    }
}
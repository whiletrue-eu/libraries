using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Components
{
    /// <summary>
    /// ComponentContainer provides the instantiation environment for components registered within a ComponentRepository.
    /// </summary>
    /// <remarks>
    /// by seperating repository from container, it is possible to create short-lived component instanes by creating them in a supplementary container than can 
    /// be disposed independently from other containers, disposing all instanes created within it at the same time.
    /// Instances can be shared among different containers, depending on the registration type of thee component
    /// </remarks>
    public class ComponentContainer : IDisposable
    {
        private readonly object[] externalInstances;
        private readonly ComponentInstanceCollection instances = new ComponentInstanceCollection();

        private readonly Stack<ComponentDescriptor> resolveStack = new Stack<ComponentDescriptor>();
        private bool disposed;

        /// <summary/>
        public ComponentContainer(ComponentRepository repository, params object[] externalInstances)
        {
            this.Repository = repository;
            this.externalInstances = externalInstances;
        }

        /// <summary>
        /// Returns the repository this container is based on
        /// </summary>
        public ComponentRepository Repository { get; }

        /// <summary>
        /// returns all instances created within this container
        /// </summary>
        private IEnumerable ComponentInstances => this.instances.ToArray();

        private IEnumerable ExternalInstances => this.externalInstances;

        #region Resolve methods

        /// <summary>
        /// returns the one component that implements the given interface. If the component is not instantiated yet, it is created,
        /// including all dependencies that are not instantiated before. Optionally, progress callback about instantiations is provided.
        /// If a repository chain is used, first a lokkup is done in the directly referenced repository, then in its parent etc util the interface
        /// implementation is found. An exception is thrown, if none or multiple implementations are found in the same repository.
        /// </summary>
        public TInterface ResolveInstance<TInterface>(Action<string> progressCallback=null) where TInterface : class
        {
            return this.InternalResolveInstance<TInterface>(true, progressCallback);
        }

        private TInterface InternalResolveInstance<TInterface>(bool throwIfNotFound, Action<string> progressCallback) where TInterface : class
        {
            this.CheckDisposed();

            if (ComponentRepository.IsComponentInterface(typeof (TInterface)))
            {
                return (TInterface)this.InternalResolveInstance(typeof(TInterface), throwIfNotFound, progressCallback);
            }
            else
            {
                throw new ArgumentException($"Interface given is not a component interface: {typeof (TInterface).Name}");
            }
        }

        /// <summary>
        /// returns the one component that implements the given interface. If the component is not instantiated yet, it is created,
        /// including all dependencies that are not instantiated before. Optionally, progress callback about instantiations is provided.
        /// If a repository chain is used, first a lokkup is done in the directly referenced repository, then in its parent etc util the interface
        /// implementation is found. An exception is thrown, if multiple implementations are found in the same repository. If no implementaiton is found, null is returned
        /// </summary>
        public TInterfaceType TryResolveInstance<TInterfaceType>(Action<string> progressCallback=null) where TInterfaceType : class
        {
            return this.InternalResolveInstance<TInterfaceType>(false, progressCallback);
        }

        /// <summary>
        /// returns all components that implement the given interface. If an component is not instantiated yet, it is created,
        /// including all dependencies that are not instantiated before. Optionally, progress callback about instantiations is provided.
        /// If a repository chain is used, first a lokkup is done in the directly referenced repository, then in its parent etc until one or more interface
        /// implementations are found.
        /// </summary>
        public TInterface[] ResolveInstances<TInterface>(Action<string> progressCallback=null) where TInterface : class
        {
            this.CheckDisposed();

            if (ComponentRepository.IsComponentInterface(typeof(TInterface)))
            {
                return this.InternalResolveInstances(typeof(TInterface), progressCallback)
                    .Cast<TInterface>()
                    .ToArray();
            }
            else
            {
                throw new ArgumentException(
                    $"Interface given is not a component interface: {typeof (TInterface).Name}");
            }
        }

# endregion

        #region Internal Resolve & Create methods

        internal bool CanResolveComponent(Type interfaceType) 
        {
            if (this.externalInstances != null)
            {
                if (this.externalInstances.Any(interfaceType.IsInstanceOfType))
                {
                    return true;
                }
            }

            return this.Repository.GetComponentDescriptors(interfaceType).Any();
        }

        internal IEnumerable<object> InternalResolveInstances(Type interfaceType, Action<string> progressCallback)
        {
            foreach (object Instance in this.ExternalInstances)
            {
                if (interfaceType.IsInstanceOfType(Instance))
                {
                    yield return Instance;
                }
            }
            foreach (ComponentDescriptor Descriptor in this.Repository.GetComponentDescriptors(interfaceType))
            {
                yield return this.CreateInstance(interfaceType, Descriptor, progressCallback);
            }
        }

        internal Array InternalResolveInstancesAsArray(Type interfaceType, Action<string> progressCallback)
        {
            object[] Components = this.InternalResolveInstances(interfaceType, progressCallback).ToArray();
            Array ComponentArray = Array.CreateInstance(interfaceType, Components.Length);
            Array.Copy(Components, ComponentArray, Components.Length);
            return ComponentArray;
        }

        internal object InternalResolveInstance(Type interfaceType, bool throwIfNotFound,  Action<string> progessCallback)
        {
                object[] Instances = this.InternalResolveInstances(interfaceType, progessCallback).ToArray();
                if (Instances.Length == 1)
                {
                    return Instances[0];
                }
                else
                {
                    if (throwIfNotFound)
                    {
                        if (Instances.Any() == false)
                        {
                            string Message =
                                $"There is no component that implements the interface {interfaceType.FullName}.There must be exactly one.";
                            throw new ResolveComponentException(Message);
                        }
                        else // count > 1
                        {
                            IEnumerable<Type> ImplementingComponents = (
                                from object Instance in this.ExternalInstances
                                where Instance.GetType().GetInterface(interfaceType.FullName) != null
                                select Instance.GetType()
                            ).Union(
                                from ComponentDescriptor in this.Repository.GetComponentDescriptors(interfaceType)
                                select ComponentDescriptor.Type
                            );

                            string Message = $"There are multiple components that implement the interface {interfaceType.FullName}.There must be exactly one.\n\nThe following components implement the interface:\n{string.Join("\n", ImplementingComponents.ConvertTo(type => type.FullName))}";
                            throw new ResolveComponentException(Message);
                        }
                    }

                    return null;
                }
        }

        private object CreateInstance(Type interfaceType, ComponentDescriptor componentDescriptor, Action<string> progressCallback) 
        {
            try
            {
                this.BeginPreventRecursion(componentDescriptor);
                ComponentInstance ComponentInstance = this.instances[componentDescriptor];
                return ComponentInstance.CreateInstance(interfaceType, this, progressCallback);
            }
            finally
            {
                this.EndPreventRecursion();
            }
        }

        #endregion

        #region Recursion prevention

        private void EndPreventRecursion()
        {
            this.resolveStack.Pop();
        }

        private void BeginPreventRecursion(ComponentDescriptor descriptor)
        {
            if (this.resolveStack.Contains(descriptor))
            {
                this.resolveStack.Push(descriptor); //Must be pushed, because the exception will cause the frame to be popped. If not pushed here, this would cause a 'stack empty' exception
                string Message = $"Recursion during creation of component '{descriptor.Name}':\n{string.Join(" -> ", this.resolveStack.Reverse().ConvertTo(value => value.ToString()).ToArray())}";
                throw new InvalidOperationException(Message);
            }
            else
            {
                this.resolveStack.Push(descriptor);
            }
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            this.CheckDisposed();

            foreach (ComponentInstance ComponentInstance in this.ComponentInstances)
            {
                try
                {
                    ComponentInstance.Dispose(this);
                }
                catch
                {
                    //Ignore
                }
            }

            this.disposed = true;
        }

        private void CheckDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("");
            }
        }

        #endregion
    }
}
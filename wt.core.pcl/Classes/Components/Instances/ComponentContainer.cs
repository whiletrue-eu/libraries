using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
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
        public TInterface ResolveInstance<TInterface>(Action<string> progressCallback = null) where TInterface : class
        {
            return AsyncContext.Run(()=>this.InternalResolveInstanceAsync<TInterface>(true, progressCallback, new ComponentDescriptor[0]));
        }

        /// <summary>
        /// returns the one component that implements the given interface. If the component is not instantiated yet, it is created,
        /// including all dependencies that are not instantiated before. Optionally, progress callback about instantiations is provided.
        /// If a repository chain is used, first a lokkup is done in the directly referenced repository, then in its parent etc util the interface
        /// implementation is found. An exception is thrown, if multiple implementations are found in the same repository. If no implementaiton is found, null is returned
        /// </summary>
        public TInterfaceType TryResolveInstance<TInterfaceType>(Action<string> progressCallback = null) where TInterfaceType : class
        {
            return AsyncContext.Run(() => this.InternalResolveInstanceAsync<TInterfaceType>(false, progressCallback, new ComponentDescriptor[0]));
        }

        /// <summary>
        /// returns all components that implement the given interface. If an component is not instantiated yet, it is created,
        /// including all dependencies that are not instantiated before. Optionally, progress callback about instantiations is provided.
        /// If a repository chain is used, first a lokkup is done in the directly referenced repository, then in its parent etc until one or more interface
        /// implementations are found.
        /// </summary>
        public TInterface[] ResolveInstances<TInterface>(Action<string> progressCallback = null) where TInterface : class
        {
            return AsyncContext.Run(() => this.ResolveInstancesAsync<TInterface>(progressCallback));
        }

        /// <summary>
        /// returns the one component that implements the given interface. If the component is not instantiated yet, it is created,
        /// including all dependencies that are not instantiated before. Optionally, progress callback about instantiations is provided.
        /// If a repository chain is used, first a lokkup is done in the directly referenced repository, then in its parent etc util the interface
        /// implementation is found. An exception is thrown, if none or multiple implementations are found in the same repository.
        /// </summary>
        public async Task<TInterface> ResolveInstanceAsync<TInterface>(Action<string> progressCallback = null) where TInterface : class
        {
            return await this.InternalResolveInstanceAsync<TInterface>(true, progressCallback, new ComponentDescriptor[0]);
        }

        /// <summary>
        /// returns the one component that implements the given interface. If the component is not instantiated yet, it is created,
        /// including all dependencies that are not instantiated before. Optionally, progress callback about instantiations is provided.
        /// If a repository chain is used, first a lokkup is done in the directly referenced repository, then in its parent etc util the interface
        /// implementation is found. An exception is thrown, if multiple implementations are found in the same repository. If no implementaiton is found, null is returned
        /// </summary>
        public async Task<TInterfaceType> TryResolveInstanceAsync<TInterfaceType>(Action<string> progressCallback = null) where TInterfaceType : class
        {
            return await this.InternalResolveInstanceAsync<TInterfaceType>(false, progressCallback, new ComponentDescriptor[0]);
        }

        /// <summary>
        /// returns all components that implement the given interface. If an component is not instantiated yet, it is created,
        /// including all dependencies that are not instantiated before. Optionally, progress callback about instantiations is provided.
        /// If a repository chain is used, first a lokkup is done in the directly referenced repository, then in its parent etc until one or more interface
        /// implementations are found.
        /// </summary>
        public async Task<TInterface[]> ResolveInstancesAsync<TInterface>(Action<string> progressCallback = null) where TInterface : class
        {
            this.CheckDisposed();

            if (ComponentRepository.IsComponentInterface(typeof(TInterface)))
            {
                return (await this.InternalResolveInstancesAsync(typeof(TInterface), progressCallback, new ComponentDescriptor[0]))
                    .Cast<TInterface>()
                    .ToArray();
            }
            else
            {
                throw new ArgumentException(
                    $"Interface given is not a component interface: {typeof(TInterface).Name}");
            }
        }

        #endregion



        #region Internal Resolve & Create methods

        private async Task<TInterface> InternalResolveInstanceAsync<TInterface>(bool throwIfNotFound, Action<string> progressCallback, ComponentDescriptor[] resolveStack) where TInterface : class
        {
            this.CheckDisposed();

            if (ComponentRepository.IsComponentInterface(typeof(TInterface)))
            {
                object Instance = await this.InternalResolveInstanceAsync(typeof(TInterface), throwIfNotFound, progressCallback, resolveStack);
                return (TInterface) Instance;
            }
            else
            {
                throw new ArgumentException($"Interface given is not a component interface: {typeof(TInterface).Name}");
            }
        }

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

        internal async Task<IEnumerable<object>> InternalResolveInstancesAsync(Type interfaceType, Action<string> progressCallback, ComponentDescriptor[] resolveStack)
        {
            List<object> Instances=new List<object>();
            foreach (object Instance in this.ExternalInstances)
            {
                if (interfaceType.IsInstanceOfType(Instance))
                {
                    Instances.Add(Instance);
                }
            }

            Instances.AddRange(
                await Task.WhenAll(
                    this.Repository.GetComponentDescriptors(interfaceType)
                        .Select(descriptor=> this.CreateInstanceAsync(interfaceType, descriptor, progressCallback,resolveStack))
                ));

            return Instances;
        }

        internal async Task<Array> InternalResolveInstancesAsArrayAsync(Type interfaceType, Action<string> progressCallback, ComponentDescriptor[] resolveStack)
        {
            object[] Components = (await this.InternalResolveInstancesAsync(interfaceType, progressCallback, resolveStack)).ToArray();
            Array ComponentArray = Array.CreateInstance(interfaceType, Components.Length);
            Array.Copy(Components, ComponentArray, Components.Length);
            return ComponentArray;
        }

        internal async Task<object> InternalResolveInstanceAsync(Type interfaceType, bool throwIfNotFound, Action<string> progessCallback, ComponentDescriptor[] resolveStack)
        {
            object[] Instances = (await this.InternalResolveInstancesAsync(interfaceType, progessCallback, resolveStack)).ToArray();
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

        private async Task<object> CreateInstanceAsync(Type interfaceType, ComponentDescriptor componentDescriptor, Action<string> progressCallback, ComponentDescriptor[] resolveStack)
        {
            ComponentDescriptor[] ResolveStack = this.CheckPreventRecursion(resolveStack, componentDescriptor);
            ComponentInstance ComponentInstance = this.instances[componentDescriptor];

            Func<Func<Task<object>>, Task<object>> RunCreate = componentDescriptor.MustCreateOnUiThread ? this.Repository.RunOnUiThread : Task.Run;
            object Instance = await RunCreate(async () => await ComponentInstance.CreateInstanceAsync(interfaceType, this, progressCallback, ResolveStack));
            return Instance;
        }

        #endregion

        #region Recursion prevention

        private ComponentDescriptor[] CheckPreventRecursion(ComponentDescriptor[] resolveStack, ComponentDescriptor descriptor)
        {
                if (resolveStack.Contains(descriptor))
                {
                    string Message = $"Recursion during creation of component '{descriptor.Name}':\n{string.Join(" -> ", resolveStack.Reverse().ConvertTo(value => value.ToString()).ToArray())}";
                    throw new InvalidOperationException(Message);
                }
                else
                {
                    return resolveStack.Union(new[]{descriptor}).ToArray();
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
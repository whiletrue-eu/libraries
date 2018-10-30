using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Components
{
    /// <summary>
    ///     ComponentContainer provides the instantiation environment for components registered within a ComponentRepository.
    /// </summary>
    /// <remarks>
    ///     by seperating repository from container, it is possible to create short-lived component instanes by creating them
    ///     in a supplementary container than can
    ///     be disposed independently from other containers, disposing all instanes created within it at the same time.
    ///     Instances can be shared among different containers, depending on the registration type of thee component
    /// </remarks>
    public class ComponentContainer : IDisposable
    {
        private readonly object[] externalInstances;
        private readonly ComponentInstanceCollection instances = new ComponentInstanceCollection();

        private readonly Stack<ComponentDescriptor> resolveStack = new Stack<ComponentDescriptor>();
        private bool disposed;

        /// <summary />
        public ComponentContainer(ComponentRepository repository, params object[] externalInstances)
        {
            Repository = repository;
            this.externalInstances = externalInstances;
        }

        /// <summary>
        ///     Returns the repository this container is based on
        /// </summary>
        public ComponentRepository Repository { get; }

        /// <summary>
        ///     returns all instances created within this container
        /// </summary>
        private IEnumerable ComponentInstances => instances.ToArray();

        private IEnumerable ExternalInstances => externalInstances;

        #region Resolve methods

        /// <summary>
        ///     returns the one component that implements the given interface. If the component is not instantiated yet, it is
        ///     created,
        ///     including all dependencies that are not instantiated before. Optionally, progress callback about instantiations is
        ///     provided.
        ///     If a repository chain is used, first a lokkup is done in the directly referenced repository, then in its parent etc
        ///     util the interface
        ///     implementation is found. An exception is thrown, if none or multiple implementations are found in the same
        ///     repository.
        /// </summary>
        public TInterface ResolveInstance<TInterface>(Action<string> progressCallback = null) where TInterface : class
        {
            return InternalResolveInstance<TInterface>(true, progressCallback);
        }

        private TInterface InternalResolveInstance<TInterface>(bool throwIfNotFound, Action<string> progressCallback)
            where TInterface : class
        {
            CheckDisposed();

            if (ComponentRepository.IsComponentInterface(typeof(TInterface)))
                return (TInterface) InternalResolveInstance(typeof(TInterface), throwIfNotFound, progressCallback);
            throw new ArgumentException($"Interface given is not a component interface: {typeof(TInterface).Name}");
        }

        /// <summary>
        ///     returns the one component that implements the given interface. If the component is not instantiated yet, it is
        ///     created,
        ///     including all dependencies that are not instantiated before. Optionally, progress callback about instantiations is
        ///     provided.
        ///     If a repository chain is used, first a lokkup is done in the directly referenced repository, then in its parent etc
        ///     util the interface
        ///     implementation is found. An exception is thrown, if multiple implementations are found in the same repository. If
        ///     no implementaiton is found, null is returned
        /// </summary>
        public TInterfaceType TryResolveInstance<TInterfaceType>(Action<string> progressCallback = null)
            where TInterfaceType : class
        {
            return InternalResolveInstance<TInterfaceType>(false, progressCallback);
        }

        /// <summary>
        ///     returns all components that implement the given interface. If an component is not instantiated yet, it is created,
        ///     including all dependencies that are not instantiated before. Optionally, progress callback about instantiations is
        ///     provided.
        ///     If a repository chain is used, first a lokkup is done in the directly referenced repository, then in its parent etc
        ///     until one or more interface
        ///     implementations are found.
        /// </summary>
        public TInterface[] ResolveInstances<TInterface>(Action<string> progressCallback = null)
            where TInterface : class
        {
            CheckDisposed();

            if (ComponentRepository.IsComponentInterface(typeof(TInterface)))
                return InternalResolveInstances(typeof(TInterface), progressCallback)
                    .Cast<TInterface>()
                    .ToArray();
            throw new ArgumentException(
                $"Interface given is not a component interface: {typeof(TInterface).Name}");
        }

        # endregion

        #region Internal Resolve & Create methods

        internal bool CanResolveComponent(Type interfaceType)
        {
            if (externalInstances != null)
                if (externalInstances.Any(interfaceType.IsInstanceOfType))
                    return true;

            return Repository.GetComponentDescriptors(interfaceType).Any();
        }

        internal IEnumerable<object> InternalResolveInstances(Type interfaceType, Action<string> progressCallback)
        {
            foreach (var Instance in ExternalInstances)
                if (interfaceType.IsInstanceOfType(Instance))
                    yield return Instance;
            foreach (var Descriptor in Repository.GetComponentDescriptors(interfaceType))
                yield return CreateInstance(interfaceType, Descriptor, progressCallback);
        }

        internal Array InternalResolveInstancesAsArray(Type interfaceType, Action<string> progressCallback)
        {
            var Components = InternalResolveInstances(interfaceType, progressCallback).ToArray();
            var ComponentArray = Array.CreateInstance(interfaceType, Components.Length);
            Array.Copy(Components, ComponentArray, Components.Length);
            return ComponentArray;
        }

        internal object InternalResolveInstance(Type interfaceType, bool throwIfNotFound,
            Action<string> progessCallback)
        {
            var Instances = InternalResolveInstances(interfaceType, progessCallback).ToArray();
            if (Instances.Length == 1) return Instances[0];

            if (throwIfNotFound)
            {
                if (Instances.Any() == false)
                {
                    var Message =
                        $"There is no component that implements the interface {interfaceType.FullName}.There must be exactly one.";
                    throw new ResolveComponentException(Message);
                }
                else // count > 1
                {
                    var ImplementingComponents = (
                        from object Instance in ExternalInstances
                        where Instance.GetType().GetInterface(interfaceType.FullName) != null
                        select Instance.GetType()
                    ).Union(
                        from ComponentDescriptor in Repository.GetComponentDescriptors(interfaceType)
                        select ComponentDescriptor.Type
                    );

                    var Message =
                        $"There are multiple components that implement the interface {interfaceType.FullName}.There must be exactly one.\n\nThe following components implement the interface:\n{string.Join("\n", ImplementingComponents.ConvertTo(type => type.FullName))}";
                    throw new ResolveComponentException(Message);
                }
            }

            return null;
        }

        private object CreateInstance(Type interfaceType, ComponentDescriptor componentDescriptor,
            Action<string> progressCallback)
        {
            try
            {
                BeginPreventRecursion(componentDescriptor);
                var ComponentInstance = instances[componentDescriptor];
                return ComponentInstance.CreateInstance(interfaceType, this, progressCallback);
            }
            finally
            {
                EndPreventRecursion();
            }
        }

        #endregion

        #region Recursion prevention

        private void EndPreventRecursion()
        {
            resolveStack.Pop();
        }

        private void BeginPreventRecursion(ComponentDescriptor descriptor)
        {
            if (resolveStack.Contains(descriptor))
            {
                resolveStack
                    .Push(descriptor); //Must be pushed, because the exception will cause the frame to be popped. If not pushed here, this would cause a 'stack empty' exception
                var Message =
                    $"Recursion during creation of component '{descriptor.Name}':\n{string.Join(" -> ", resolveStack.Reverse().ConvertTo(value => value.ToString()).ToArray())}";
                throw new InvalidOperationException(Message);
            }

            resolveStack.Push(descriptor);
        }

        #endregion

        #region IDisposable

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            CheckDisposed();

            foreach (ComponentInstance ComponentInstance in ComponentInstances)
                try
                {
                    ComponentInstance.Dispose(this);
                }
                catch
                {
                    //Ignore
                }

            disposed = true;
        }

        private void CheckDisposed()
        {
            if (disposed) throw new ObjectDisposedException("");
        }

        #endregion
    }
}
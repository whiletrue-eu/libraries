using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        public IEnumerable ComponentInstances => this.instances.ToArray();

        private IEnumerable ExternalInstances => this.externalInstances;

        #region Resolve methods

        /// <summary>
        /// returns the one component that implements the given interface. If the component is not instantiated yet, it is created,
        /// including all dependencies that are not instantiated before. Optionally, progress callback about instantiations is provided.
        /// If a repository chain is used, first a lokkup is done in the directly referenced repository, then in its parent etc util the interface
        /// implementation is found. An exception is thrown, if none or multiple implementations are found in the same repository.
        /// </summary>
        public TInterfaceType ResolveInstance<TInterfaceType>(Action<int,int,string> progressCallback=null) where TInterfaceType : class
        {
            return this.InternalResolveInstance<TInterfaceType>(true, progressCallback);
        }

        private TInterfaceType InternalResolveInstance<TInterfaceType>(bool throwIfNotFound, Action<int, int, string> progressCallback) where TInterfaceType : class
        {
            this.CheckDisposed();

            if (ComponentRepository.IsComponentInterface(typeof (TInterfaceType)))
            {
                return ComponentContainer.ExecuteCreateExpression<TInterfaceType>(callback=>this.InternalResolveInstance(typeof (TInterfaceType), throwIfNotFound, callback), progressCallback);
            }
            else
            {
                throw new ArgumentException($"Interface given is not a component interface: {typeof (TInterfaceType).Name}");
            }
        }

        private static TInterfaceType ExecuteCreateExpression<TInterfaceType>(Func<Expression<Action<string>>, Expression> getExpression, Action<int, int, string> progressCallback) where TInterfaceType : class
        {
// ReSharper disable AccessToModifiedClosure
            int MaxInstances = 0;
            int CurrentInstance = 1;
            Action<string> InternalCallback = componentName => progressCallback?.Invoke(MaxInstances, CurrentInstance++, componentName);
            // ReSharper restore AccessToModifiedClosure

            Expression<Action<string>> Callback = _ => InternalCallback(_);

            Expression Expression = getExpression(Callback);
            if (Expression != null)
            {
                LambdaExpression CreateExpression = Expression.Lambda(Expression);

                //Replace empty callback by implementation
                CountInstanceCreationExpressionVisitor Counter = new CountInstanceCreationExpressionVisitor();
                Counter.Visit(CreateExpression);
                MaxInstances = Counter.InstanceCount;

                Func<object> Create = (Func<object>) CreateExpression.Compile();
                return (TInterfaceType) Create();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Counts occurences on object creations that are components (some wrapper are also created which have to be ignored)
        /// </summary>
        private class CountInstanceCreationExpressionVisitor : ExpressionVisitor
        {
            protected override Expression VisitNew(NewExpression node)
            {
                if (node.Constructor.DeclaringType.GetCustomAttributes<ComponentAttribute>().Any())
                {
                    this.InstanceCount++;
                }
                return base.VisitNew(node);
            }

            public int InstanceCount { get; private set; }
        }

        /// <summary>
        /// returns the one component that implements the given interface. If the component is not instantiated yet, it is created,
        /// including all dependencies that are not instantiated before. Optionally, progress callback about instantiations is provided.
        /// If a repository chain is used, first a lokkup is done in the directly referenced repository, then in its parent etc util the interface
        /// implementation is found. An exception is thrown, if multiple implementations are found in the same repository. If no implementaiton is found, null is returned
        /// </summary>
        public TInterfaceType TryResolveInstance<TInterfaceType>(Action<int, int, string> progressCallback=null) where TInterfaceType : class
        {
            return this.InternalResolveInstance<TInterfaceType>(false, progressCallback);
        }

        /// <summary>
        /// returns all components that implement the given interface. If an component is not instantiated yet, it is created,
        /// including all dependencies that are not instantiated before. Optionally, progress callback about instantiations is provided.
        /// If a repository chain is used, first a lokkup is done in the directly referenced repository, then in its parent etc until one or more interface
        /// implementations are found.
        /// </summary>
        public TInterfaceType[] ResolveInstances<TInterfaceType>(Action<int,int,string> progressCallback=null) where TInterfaceType : class
        {
            this.CheckDisposed();

            if (ComponentRepository.IsComponentInterface(typeof(TInterfaceType)))
            {
                return ComponentContainer.ExecuteCreateExpression<TInterfaceType[]>(callback=>Expression.NewArrayInit(typeof (TInterfaceType), this.InternalResolveInstances(typeof (TInterfaceType), callback)),progressCallback);
            }
            else
            {
                throw new ArgumentException(
                    $"Interface given is not a component interface: {typeof (TInterfaceType).Name}");
            }
        }

# endregion

        #region Internal Resolve & Create methods

        internal bool CanResolveComponent(Type interfaceType)
        {
            if (this.externalInstances != null)
            {
                if (this.externalInstances.Any(instance => instance.GetType().GetInterface(interfaceType.FullName) != null))
                {
                    return true;
                }
            }

            return this.Repository.GetComponentDescriptors(interfaceType).Length > 0;
        }

        internal IEnumerable<Expression> InternalResolveInstances(Type interfaceType, Expression progressCallback)
        {
            return
                (
                    from object Instance in this.ExternalInstances
                    where Instance.GetType().GetInterface(interfaceType.FullName) != null
                    select Expression.Convert(Expression.Constant(Instance), interfaceType)
                ).Union(
                    from ComponentDescriptor in this.Repository.GetComponentDescriptors(interfaceType)
                    select this.CreateInstance(ComponentDescriptor, interfaceType, progressCallback)
                    );
        }

        internal Expression InternalResolveInstance(Type interfaceType, bool throwIfNotFound, Expression progessCallback)
        {
            IEnumerable<Expression> Instances = this.InternalResolveInstances(interfaceType, progessCallback).ToArray();
            if (Instances.Count() == 1)
            {
                return Instances.First();
            }
            else
            {
                if (throwIfNotFound)
                {
                    if (Instances.Any() == false)
                    {
                        string Message =
                            $"There is no component that implement the interface {interfaceType.Name}.There must be exactly one.";
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

                        string Message = $"There are multiple components that implement the interface {interfaceType.Name}.There must be exactly one.\n\nThe following components implement the interface:\n{string.Join("\n", ImplementingComponents.ConvertTo(type=>type.FullName))}";
                        throw new ResolveComponentException(Message);
                    }
                }

                return null;
            }
        }

        private Expression CreateInstance(ComponentDescriptor componentDescriptor, Type interfaceType, Expression progressCallback)
        {
            try
            {
                this.BeginPreventRecursion(componentDescriptor);
                ComponentInstance ComponentInstance = this.instances[componentDescriptor];

                ParameterExpression InstanceVariable = Expression.Variable(typeof(object));
                return Expression.Block(
                    interfaceType,
                    new[]{InstanceVariable},
                    Expression.Assign(InstanceVariable, ComponentInstance.CreateInstance(interfaceType, this, progressCallback)),
                    Expression.Call(Expression.Constant(this.instances),nameof(ComponentInstanceCollection.NotifyInstanceCreated),new Type[0], Expression.Constant(ComponentInstance), InstanceVariable),
                    Expression.Convert(InstanceVariable,interfaceType)
                    );
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
                ComponentInstance.Dispose(this);
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
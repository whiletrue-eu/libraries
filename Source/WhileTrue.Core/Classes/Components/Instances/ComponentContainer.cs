using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Components
{
    public class ComponentContainer : IDisposable
    {
        private readonly object[] externalInstances;
        private readonly ComponentInstanceCollection instances = new ComponentInstanceCollection();
        private readonly ComponentRepository repository;
       
        private readonly Stack<ComponentDescriptor> resolveStack = new Stack<ComponentDescriptor>();
        private bool disposed;

        public ComponentContainer(ComponentRepository repository, params object[] externalInstances)
        {
            this.repository = repository;
            this.externalInstances = externalInstances;
        }

        protected internal ComponentRepository Repository
        {
            get { return this.repository; }
        }

        public IEnumerable ComponentInstances
        {
            get { return this.instances.ToArray(); }
        }

        internal IEnumerable ExternalInstances
        {
            get { return this.externalInstances; }
        }

        #region Resolve methods

        public InterfaceType ResolveInstance<InterfaceType>(Action<int,int,string> progressCallback=null) where InterfaceType : class
        {
            return this.InternalResolveInstance<InterfaceType>(true, progressCallback);
        }

        private InterfaceType InternalResolveInstance<InterfaceType>(bool throwIfNotFound, Action<int, int, string> progressCallback) where InterfaceType : class
        {
            this.CheckDisposed();

            if (ComponentRepository.IsComponentInterface(typeof (InterfaceType)))
            {
                return ExecuteCreateExpression<InterfaceType>(callback=>this.InternalResolveInstance(typeof (InterfaceType), throwIfNotFound, null, callback), progressCallback);
            }
            else
            {
                throw new ArgumentException(string.Format("Interface given is not a component interface: {0}", typeof (InterfaceType).Name));
            }
        }

        private static InterfaceType ExecuteCreateExpression<InterfaceType>(Func<Expression<Action<string>>, Expression> GetExpression, Action<int, int, string> progressCallback) where InterfaceType : class
        {
// ReSharper disable AccessToModifiedClosure
            int MaxInstances = 0;
            int CurrentInstance = 1;
            Action<string> InternalCallback = (componentName =>
                                               {
                                                   if (progressCallback != null)
                                                   {
                                                       progressCallback(MaxInstances, CurrentInstance++, componentName);
                                                   }
                                               });
            // ReSharper restore AccessToModifiedClosure

            Expression<Action<string>> Callback = _ => InternalCallback(_);

            Expression Expression = GetExpression(Callback);
            if (Expression != null)
            {
                LambdaExpression CreateExpression = Expression.Lambda(Expression);

                //Replace empty callback by implementation
                CountInstanceCreationExpressionVisitor Counter = new CountInstanceCreationExpressionVisitor();
                Counter.Visit(CreateExpression);
                MaxInstances = Counter.InstanceCount;

                Func<object> Create = (Func<object>) CreateExpression.Compile();
                return (InterfaceType) Create();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Counts occurences on object creations that are components (some wrapper are also created which have to be ignored)
        /// </summary>
        public class CountInstanceCreationExpressionVisitor : ExpressionVisitor
        {
            protected override Expression VisitNew(NewExpression node)
            {
                if (node.Constructor.DeclaringType.GetCustomAttributes(typeof (ComponentAttribute), true).Any())
                {
                    this.InstanceCount++;
                }
                return base.VisitNew(node);
            }

            public int InstanceCount { get; private set; }
        }

        public InterfaceType TryResolveInstance<InterfaceType>(Action<int, int, string> progressCallback=null) where InterfaceType : class
        {
            return this.InternalResolveInstance<InterfaceType>(false, progressCallback);
        }

        public InterfaceType[] ResolveInstances<InterfaceType>(Action<int,int,string> progressCallback=null) where InterfaceType : class
        {
            this.CheckDisposed();

            if (ComponentRepository.IsComponentInterface(typeof(InterfaceType)))
            {
                return ExecuteCreateExpression<InterfaceType[]>(callback=>Expression.NewArrayInit(typeof (InterfaceType), this.InternalResolveInstances(typeof (InterfaceType), null, callback)),progressCallback);
            }
            else
            {
                throw new ArgumentException(
                    string.Format("Interface given is not a component interface: {0}", typeof(InterfaceType).Name));
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

            return (this.repository.GetComponentDescriptors(interfaceType).Length > 0);
        }

        internal IEnumerable<Expression> InternalResolveInstances(Type interfaceType, string name, Expression progressCallback)
        {
            return
                (
                    from object Instance in this.ExternalInstances
                    where Instance.GetType().GetInterface(interfaceType.FullName) != null
                    select Expression.Constant(Instance)
                ).Union(
                    from ComponentDescriptor in this.Repository.GetComponentDescriptors(interfaceType)
                    select this.CreateInstance(ComponentDescriptor, interfaceType, progressCallback)
                    );
        }

        internal Expression InternalResolveInstance(Type interfaceType, bool throwIfNotFound, string name, Expression progessCallback)
        {
            IEnumerable<Expression> Instances = this.InternalResolveInstances(interfaceType, name, progessCallback).ToArray();
            if (Instances.Count() == 1)
            {
                return Instances.First();
            }
            else
            {
                if (throwIfNotFound)
                {
                    if (Instances.Any()==false)
                    {
                        string Message =
                            string.Format(
                                "There is no component that implement the interface {0}.There must be exactly one.",
                                interfaceType.Name);
                        throw new ResolveComponentException(Message);
                    }
                    else // count > 1
                    {
                        string Message = string.Format(
                            "There are multiple components that implement the interface {0}.There must be exactly one.\n\nThe following components implement the interface:\n{1}",
                            interfaceType.Name,
                            string.Join("\n", Instances.ConvertTo(value => value.GetType().FullName).ToArray())
                            );
                        throw new ResolveComponentException(Message);
                    }
                }

                return null;
            }
        }

        internal Expression CreateInstance(ComponentDescriptor componentDescriptor, Type interfaceType, Expression progressCallback)
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
                    Expression.Call(Expression.Constant(this.instances),"NotifyInstanceCreated",new Type[0], Expression.Constant(ComponentInstance), InstanceVariable),
                    Expression.Convert(InstanceVariable,interfaceType));
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
                string Message = string.Format("Recursion during creation of component '{0}':\n{1}",
                                               descriptor.Name,
                                               string.Join(" -> ", this.resolveStack.Reverse().ConvertTo(value => value.ToString()).ToArray()));
                throw new InvalidOperationException(Message);
            }
            else
            {
                this.resolveStack.Push(descriptor);
            }
        }

        #endregion

        #region IDisposable

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
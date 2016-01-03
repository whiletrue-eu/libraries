using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Components
{
    internal class SingletonComponentInstance : ComponentInstance
    {
         private static readonly Dictionary<Type, SingletonInstanceWrapper> singletonInstances = new Dictionary<Type, SingletonInstanceWrapper>();

         internal SingletonComponentInstance(ComponentDescriptor componentDescriptor)
            : base(componentDescriptor)
        {
        }

        protected override object Instance => this.InstanceReference != null ? this.InstanceReference.Target:null;

        private SingletonInstanceWrapper InstanceReference
        {
            get
            {
                if (SingletonComponentInstance.singletonInstances.ContainsKey(this.Descriptor.Type))
                {
                    return SingletonComponentInstance.singletonInstances[this.Descriptor.Type];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value != null)
                {
                    SingletonComponentInstance.singletonInstances.Add(this.Descriptor.Type, value);
                }
                else
                {
                    SingletonComponentInstance.singletonInstances.Remove(this.Descriptor.Type);
                }
            }
        }

        internal override Expression CreateInstance(Type interfaceType, ComponentContainer componentContainer, Expression progressCallback)
        {
            /*
            if (this.InstanceReference == null)
            {
                this.InstanceReference = new SingletonInstanceWrapper(base.CreateInstance(interfaceType, componentContainer, progressCallback));
                this.LazyInitializeWithInstancesAlreadyExisting(componentContainer);
            }
            return this.InstanceReference.AddReference(componentContainer);
             */
            Expression InstanceProperty = Expression.Property(Expression.Constant(this), nameof(SingletonComponentInstance.InstanceReference));
            return Expression.Block(
                typeof(object),
                Expression.IfThen(
                    Expression.ReferenceEqual(InstanceProperty, Expression.Constant(null)),
                    Expression.Block(
                        Expression.Assign(InstanceProperty, Expression.New(typeof(SingletonInstanceWrapper).GetConstructor(new[] { typeof(object) }), this.DoCreateInstance(interfaceType, componentContainer, progressCallback))),
                        Expression.Call(Expression.Constant(this), nameof(ComponentInstance.LazyInitializeWithInstancesAlreadyExisting), null, Expression.Constant(componentContainer)))),
                Expression.Call(InstanceProperty, nameof(SingletonInstanceWrapper.AddReference), null, Expression.Constant(componentContainer))
                );
        }

        internal override void Dispose(ComponentContainer componentContainer)
        {
            if (this.InstanceReference != null &&
                this.InstanceReference.ReleaseReference(componentContainer))
            {
                if (this.InstanceReference.Target is IDisposable)
                {
                    ((IDisposable) this.InstanceReference.Target).Dispose();
                }
                this.InstanceReference = null;
                base.Dispose(componentContainer);
            }
        }


        internal override void LazyInitialize(PropertyInfo property, object instance)
        {
            if (this.InstanceReference != null)
            {
                property.SetValue(this.InstanceReference.Target, instance, null);
            }
        }

        #region Nested type: SharedInstanceWrapper

        private class SingletonInstanceWrapper
        {
            private readonly List<ComponentContainer> references = new List<ComponentContainer>();

            public SingletonInstanceWrapper(object instance)
            {
                this.Target = instance;
            }

            public object Target { get; }

            [UsedImplicitly]
            public object AddReference(ComponentContainer componentContainer)
            {
                if (this.references.Contains(componentContainer) == false)
                {
                    this.references.Add(componentContainer);
                }
                return this.Target;
            }

            public bool ReleaseReference(ComponentContainer componentContainer)
            {
                DbC.Assure(this.references.Contains(componentContainer));
                this.references.Remove(componentContainer);
                return this.references.Count == 0;
            }
        }

        #endregion
    }
}
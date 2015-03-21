using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using WhileTrue.Classes.CodeInspection;
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

        protected override object Instance
        {
            get
            {
                return this.InstanceReference != null ? this.InstanceReference.Target:null; 
                //Null can happen while creation is in process
            }
        }

        private SingletonInstanceWrapper InstanceReference
        {
            get
            {
                if (singletonInstances.ContainsKey(this.Descriptor.Type))
                {
                    return singletonInstances[this.Descriptor.Type];
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
                    singletonInstances.Add(this.Descriptor.Type, value);
                }
                else
                {
                    singletonInstances.Remove(this.Descriptor.Type);
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
            Expression InstanceProperty = Expression.Property(Expression.Constant(this), "InstanceReference");
            return Expression.Block(
                typeof(object),
                Expression.IfThen(
                    Expression.ReferenceEqual(InstanceProperty, Expression.Constant(null)),
                    Expression.Block(
                        Expression.Assign(InstanceProperty, Expression.New(typeof(SingletonInstanceWrapper).GetConstructor(new[] { typeof(object) }), DoCreateInstance(interfaceType, componentContainer, progressCallback))),
                        Expression.Call(Expression.Constant(this), "LazyInitializeWithInstancesAlreadyExisting", null, Expression.Constant(componentContainer)))),
                Expression.Call(InstanceProperty, "AddReference", null, Expression.Constant(componentContainer))
                );
        }

        public override void Dispose(ComponentContainer componentContainer)
        {
            if (this.InstanceReference != null &&
                this.InstanceReference.ReleaseReference(componentContainer))
            {
                if (this.InstanceReference.Target is IDisposable)
                {
                    ((IDisposable) this.InstanceReference.Target).Dispose();
                }
                this.InstanceReference = null;
            }
            base.Dispose(componentContainer);
        }


        protected override void LazyInitialize(PropertyInfo property, object instance)
        {
            if (this.InstanceReference != null)
            {
                property.SetValue(this.InstanceReference.Target, instance, null);
            }
        }

        #region Nested type: SharedInstanceWrapper

        private class SingletonInstanceWrapper
        {
            private readonly object instance;
            private readonly List<ComponentContainer> references = new List<ComponentContainer>();

            public SingletonInstanceWrapper(object instance)
            {
                this.instance = instance;
            }

            public object Target
            {
                get
                {
                    return this.instance;
                }
            }

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
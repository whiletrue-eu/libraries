using System;
using System.Linq.Expressions;
using System.Reflection;

namespace WhileTrue.Classes.Components
{
    internal class SimpleComponentInstance : ComponentInstance
    {
        private object instance;

        internal SimpleComponentInstance(ComponentDescriptor componentDescriptor)
            : base(componentDescriptor)
        {
        }

        protected override object Instance => this.instance;

        internal override Expression CreateInstance(Type interfaceType, ComponentContainer componentContainer, Expression progressCallback)
        {
            /*
            if (this.instance == null)
            {
                this.instance = base.CreateInstance(interfaceType, componentContainer, progressCallback);
                this.LazyInitializeWithInstancesAlreadyExisting(componentContainer);
            }
            return this.instance;
             */
            Expression InstanceField = Expression.Field(Expression.Constant(this), nameof(SimpleComponentInstance.instance));
            return Expression.Block(
                typeof(object),
                Expression.IfThen(
                    Expression.ReferenceEqual(InstanceField, Expression.Constant(null)),
                    Expression.Block(
                        Expression.Assign(InstanceField, this.DoCreateInstance(interfaceType, componentContainer, progressCallback)),
                        Expression.Call(Expression.Constant(this), nameof(ComponentInstance.LazyInitializeWithInstancesAlreadyExisting), null, Expression.Constant(componentContainer)))),
                InstanceField
                );
        }

        internal override void Dispose(ComponentContainer componentContainer)
        {
            if (this.instance is IDisposable)
            {
                ((IDisposable) this.instance).Dispose();
            }
            this.instance = null;
            base.Dispose(componentContainer);
        }

        internal override void LazyInitialize(PropertyInfo property, object instance)
        {
            if (this.instance != null)
            {
                property.SetValue(this.instance, instance, null);
            }
        }
    }
}
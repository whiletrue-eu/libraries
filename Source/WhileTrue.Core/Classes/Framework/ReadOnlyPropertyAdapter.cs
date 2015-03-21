using System;
using System.Linq.Expressions;

namespace WhileTrue.Classes.Framework
{
    /// <summary>
    /// Provides an adapter for a read-only property of an underlying model
    /// </summary>
    public class ReadOnlyPropertyAdapter<PropertyType> : PropertyAdapterBase<PropertyType>
    {
        private readonly ValueRetrievalStrategyBase valueRetriever;

        internal ReadOnlyPropertyAdapter(Expression<Func<PropertyType>> getExpression, Action changedCallback, EventBindingMode eventBindingMode, ValueRetrievalMode valueRetrievalMode)
            : base(getExpression, changedCallback, eventBindingMode)
        {
            switch (valueRetrievalMode)
            {
                case ValueRetrievalMode.Immediately:
                    this.valueRetriever = new ImmediateValueRetrievalStrategy(this);
                    break;
                case ValueRetrievalMode.Lazy:
                    this.valueRetriever = new LazyValueRetrievalStrategy(this);
                    break;
                case ValueRetrievalMode.OnDemand:
                    this.valueRetriever = new OnDemandValueRetrievalStrategy(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("valueRetrievalMode");
            } 
            this.valueRetriever.NotifyInitialized();
        }

        /// <summary>
        /// Gets the value of the underlying property
        /// </summary>
        public PropertyType GetValue()
        {
            return this.valueRetriever.GetValue();
        }


        protected override void NotifyExpressionChanged(object sender, EventArgs e)
        {
            this.valueRetriever.NotifyChanged(sender, e);
        }

        
        private abstract class ValueRetrievalStrategyBase
        {
            public abstract PropertyType GetValue();
            public abstract void NotifyChanged(object sender, EventArgs e);
            /// <summary>
            /// apart from constructor to support circular dependency (=> stackoverflow) cases, as this needs the constructor to be run completely
            /// </summary>
            public abstract void NotifyInitialized();
        }
        private class ImmediateValueRetrievalStrategy : ValueRetrievalStrategyBase
        {
            private readonly ReadOnlyPropertyAdapter<PropertyType> owner;
            private Value<PropertyType> value;

            public ImmediateValueRetrievalStrategy(ReadOnlyPropertyAdapter<PropertyType> owner)
            {
                this.owner = owner;
            }

            public override void NotifyInitialized()
            {
                this.value = this.owner.RetrieveValue(_ => _);
            }

            public override PropertyType GetValue()
            {
                return this.value.GetValue();
            }

            public override void NotifyChanged(object sender, EventArgs e)
            {
                Value<PropertyType> Value = this.owner.RetrieveValue(_=>_, sender, e);
                if (Value.Equals(this.value) == false)
                {
                    this.value = Value;
                    this.owner.InvokeChanged();
                }
            }
        }
        private class LazyValueRetrievalStrategy : ValueRetrievalStrategyBase
        {
            private readonly ReadOnlyPropertyAdapter<PropertyType> owner;
            private Value<PropertyType> value;

            public LazyValueRetrievalStrategy(ReadOnlyPropertyAdapter<PropertyType> owner)
            {
                this.owner = owner;
            }

            public override void NotifyInitialized()
            {
            }

            public override PropertyType GetValue()
            {
                return (this.value ?? (this.value = this.owner.RetrieveValue(_=>_))).GetValue();
            }

            public override void NotifyChanged(object sender, EventArgs e)
            {
                this.value = null;
                this.owner.InvokeChanged();
            }
        }
        private class OnDemandValueRetrievalStrategy : ValueRetrievalStrategyBase
        {
            private readonly ReadOnlyPropertyAdapter<PropertyType> owner;

            public OnDemandValueRetrievalStrategy(ReadOnlyPropertyAdapter<PropertyType> owner)
            {
                this.owner = owner;
            }

            public override void NotifyInitialized()
            {
            }

            public override PropertyType GetValue()
            {
                return this.owner.RetrieveValue(_=>_).GetValue();
            }

            public override void NotifyChanged(object sender, EventArgs e)
            {
                this.owner.InvokeChanged();
            }
        }
    }
    public class ReadOnlyPropertyAdapter<TSource, TProperty> : PropertyAdapterBase<TSource, TProperty, TProperty> where TSource : ObservableObject
    {
        internal ReadOnlyPropertyAdapter(Expression<Func<TSource, TProperty>> propertyAccess, Expression<Func<TSource, TProperty>> getExpression)
            : base(propertyAccess, getExpression)
        {
        }

        private ObservableObject.CachedValue<TProperty> RetrieveValue(TSource source)
        {
            return this.RetrieveValue(source, (sender, e) => this.PropertyChangeCallback(source, sender, e));
        }

        private void PropertyChangeCallback(TSource source, object sender, EventArgs e)
        {
            source.GetPropertyValueCache().ClearValue(this);
            source.NotifyPropertyChanged(this.propertyName, sender, e);
        }


        /// <summary>
        /// Gets the value of the underlying property
        /// </summary>
        public TProperty GetValue(TSource source)
        {
            ObservableObject.PropertyValueCache PropertyValues = source.GetPropertyValueCache();
            if (PropertyValues.HasValue(this))
            {
                return PropertyValues.GetValue(this).GetValue();
            }
            else
            {
                return PropertyValues.SetValue(this, this.RetrieveValue(source)).GetValue();
            }
        }
    }
}
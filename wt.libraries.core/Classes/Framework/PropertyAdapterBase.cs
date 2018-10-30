using System;
using System.Linq.Expressions;

namespace WhileTrue.Classes.Framework
{
    /// <summary>
    ///     Base class for the property adapters for the instance version CreatePropertyAdapter
    /// </summary>
    public abstract class PropertyAdapterBase<TPropertyType>
    {
        private readonly Action changedCallback;
        private readonly NotifyChangeExpression<Func<TPropertyType>> getExpression;

        internal PropertyAdapterBase(Expression<Func<TPropertyType>> getExpression, Action changedCallback)
        {
            this.changedCallback = changedCallback;
            this.getExpression = new NotifyChangeExpression<Func<TPropertyType>>(getExpression);
            this.getExpression.Changed += WeakDelegate
                .Connect<PropertyAdapterBase<TPropertyType>, NotifyChangeExpression<Func<TPropertyType>>,
                    EventHandler<EventArgs>, EventArgs>(
                    this,
                    this.getExpression,
                    (target, sender, e) => target.ExpressionChanged(sender, e),
                    (source, handler) => source.Changed -= handler);
        }

        /// <summary>
        ///     Used internally to retrieve the value given by the getExpression
        /// </summary>
        protected Value<TValueType> RetrieveValue<TValueType>(Func<TPropertyType, TValueType> valuePostProcessing)
        {
            try
            {
                lock (getExpression)
                {
                    return new Value<TValueType>(valuePostProcessing(getExpression.Invoke()));
                }
            }
            catch (Exception Exception)
            {
                return new Value<TValueType>(Exception);
            }
        }

        /// <summary>
        ///     INvokes the changed callback on this expression
        /// </summary>
        protected void InvokeChanged()
        {
            changedCallback();
        }

        private void ExpressionChanged(object sender, EventArgs e)
        {
            NotifyExpressionChanged(sender, e);
        }

        /// <summary>
        ///     Notifies inherited classes that some parts of the instances used during expression evaluation changed
        /// </summary>
        protected abstract void NotifyExpressionChanged(object sender, EventArgs e);

        /// <summary>
        ///     Wraps the outcome of the expression evaluation; either a value or an exception
        /// </summary>
        protected class Value<TValueType>
        {
            private readonly Exception exception;
            private readonly TValueType value;

            /// <summary />
            public Value(TValueType value)
            {
                this.value = value;
                exception = null;
            }

            /// <summary />
            public Value(Exception exception)
            {
                value = default(TValueType);
                this.exception = exception;
            }

            /// <summary>
            ///     Retruns the wrapped value or throws the exception that occured
            /// </summary>
            public TValueType GetValue()
            {
                if (exception != null) throw exception;
                return value;
            }

            /// <summary>
            ///     Determines whether the specified object is equal to the current object.
            /// </summary>
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (obj.GetType() != typeof(Value<TValueType>)) return false;
                return Equals((Value<TValueType>) obj);
            }

            /// <summary>
            ///     Determines whether the specified object is equal to the current object.
            /// </summary>
            public bool Equals(Value<TValueType> other)
            {
                if (ReferenceEquals(null, other)) return false;
                return Equals(other.value, value) && Equals(other.exception, exception);
            }

            /// <summary>
            ///     Serves as the default hash function.
            /// </summary>
            public override int GetHashCode()
            {
                return Equals(value, default(TPropertyType))
                    ? 0
                    : value.GetHashCode() ^ (exception?.GetHashCode() ?? 0);
            }
        }
    }

    /// <summary>
    ///     Base class for property adapters for the static version of PropertyAdapterFactory.Create
    /// </summary>
    public abstract class PropertyAdapterBase<TSource, TSourceProperty> where TSource : ObservableObject
    {
        private readonly Func<TSource, ObservableExpressionFactory.EventSink, TSourceProperty> getter;

        /// <summary />
        protected readonly string PropertyName;

        internal PropertyAdapterBase(string propertyName, Expression<Func<TSource, TSourceProperty>> getExpression)
        {
            PropertyName = propertyName;
            getter = ObservableExpressionFactory.Compile(getExpression);
        }

        internal ObservableObject.CachedValue<TSourceProperty> RetrieveValue(TSource instance,
            Action<object, EventArgs> eventHandler)
        {
            var EventSink = new ObservableExpressionFactory.EventSink(eventHandler);
            try
            {
                return new ObservableObject.CachedValue<TSourceProperty>(getter.Invoke(instance, EventSink), EventSink);
            }
            catch (Exception Exception)
            {
                return new ObservableObject.CachedValue<TSourceProperty>(Exception, EventSink);
            }
        }
    }
}
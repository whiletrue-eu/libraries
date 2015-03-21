using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading;

namespace WhileTrue.Classes.Framework
{
    public abstract class PropertyAdapterBase<PropertyType>
    {
        private readonly Action changedCallback;
        private readonly NotifyChangeExpression<Func<PropertyType>> getExpression;
        private readonly List<int> threadIDs =  new List<int>();

        internal PropertyAdapterBase(Expression<Func<PropertyType>> getExpression, Action changedCallback, EventBindingMode eventBindingMode)
        {
            this.changedCallback = changedCallback;
            this.getExpression = new NotifyChangeExpression<Func<PropertyType>>(getExpression, eventBindingMode);
            switch (eventBindingMode)
            {
                case EventBindingMode.Strong:
                    this.getExpression.Changed += this.ExpressionChanged;
                    break;
                case EventBindingMode.Weak:
                    this.getExpression.Changed += WeakDelegate.Connect<PropertyAdapterBase<PropertyType>, NotifyChangeExpression<Func<PropertyType>>, EventHandler<EventArgs>, EventArgs>(
                        this,
                        this.getExpression,
                        (target, sender, e) => target.ExpressionChanged(sender, e),
                        (source, handler) => source.Changed -= handler);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("eventBindingMode");
            }
        }

        protected Value<ValueType> RetrieveValue<ValueType>(Func<PropertyType,ValueType> valuePostProcessing)
        {
            this.CheckRecursion(null, null);
            try
            {
                return new Value<ValueType>(valuePostProcessing(this.getExpression.Invoke()));
            }
            catch (Exception Exception)
            {
                return new Value<ValueType>(Exception);
            }
            finally
            {
                this.UpdateDone();
            }
        }

        protected Value<ValueType> RetrieveValue<ValueType>(Func<PropertyType,ValueType> valuePostProcessing, object sender, EventArgs e)
        {
            this.CheckRecursion(sender, e);
            try
            {
                return new Value<ValueType>(valuePostProcessing(this.getExpression.Invoke()));
            }
            catch (Exception Exception)
            {
                return new Value<ValueType>(Exception);
            }
            finally
            {
                this.UpdateDone();
            }
        }

        protected void InvokeChanged()
        {
            this.changedCallback();
        }

        private void UpdateDone()
        {
            lock (this.threadIDs)
            {
                this.threadIDs.Remove(Thread.CurrentThread.ManagedThreadId);
            }
        }

        private void CheckRecursion(object sender, EventArgs e)
        {
            lock (this.threadIDs)
            {
                //if same thread reenters the update value, the retrieval of the update
                //implicitly called the 'propertychanged' event that invalidated this adapter
                //to call Update...() again...
                int ThreadID = Thread.CurrentThread.ManagedThreadId;
                if (this.threadIDs.Contains(ThreadID))
                {
                    if (e is NotifyCollectionChangedEventArgs)
                    {
                        throw new CircularDependencyException(string.Format("Recursion occured while retrieving the value: event was called on '{0}', performing a '{1}' on the collection.", sender, ((NotifyCollectionChangedEventArgs) e).Action));
                    }
                    else if (e is PropertyChangedEventArgs)
                    {
                        throw new CircularDependencyException(string.Format("Recursion occured while retrieving the value: event was called on '{0}', for property '{1}'.", sender, ((PropertyChangedEventArgs) e).PropertyName));
                    }
                    else if (e == null)
                    {
                        throw new CircularDependencyException("Recursion occured while retrieving the value");
                    }
                    else
                    {
                        throw new CircularDependencyException(string.Format("Recursion occured while retrieving the value: event was called on '{0}'.", sender));
                    }
                }
                else
                {
                    this.threadIDs.Add(ThreadID);
                }
            }
        }

        private void ExpressionChanged(object sender, EventArgs e)
        {
            this.NotifyExpressionChanged(sender,e);
        }

        protected abstract void NotifyExpressionChanged(object sender, EventArgs e);

        protected class Value<ValueType>
        {
            private readonly ValueType value;
            private readonly Exception exception;

            public Value(ValueType value)
            {
                this.value = value;
                this.exception = null;
            }

            public Value(Exception exception)
            {
                this.value = default(ValueType);
                this.exception = exception;
            }

            public ValueType GetValue()
            {
                if (this.exception != null)
                {
                    throw this.exception;
                }
                return this.value;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (obj.GetType() != typeof(Value<ValueType>)) return false;
                return Equals((Value<ValueType>)obj);
            }

            public bool Equals(Value<ValueType> other)
            {
                if (ReferenceEquals(null, other)) return false;
                return Equals(other.value, this.value) && Equals(other.exception, this.exception);
            }

            public override int GetHashCode()
            {
                return (Equals(this.value, default(PropertyType)) ? 0: this.value.GetHashCode() ^ (this.exception != null ? this.exception.GetHashCode() : 0));
            }
        }
    }
    public abstract class PropertyAdapterBase<TSource, TSourceProperty, TProperty> where TSource:ObservableObject
    {
        private readonly Func<TSource, ObservableExpressionFactory.EventSink, TSourceProperty> getter;
        private readonly List<int> threadIDs = new List<int>();
        protected readonly string propertyName;

        internal PropertyAdapterBase(Expression<Func<TSource, TProperty>> propertyAccess, Expression<Func<TSource, TSourceProperty>> getExpression)
        {
            this.propertyName = propertyAccess.GetPropertyName();
            this.getter = ObservableExpressionFactory.Compile(getExpression);
        }

        internal ObservableObject.CachedValue<TSourceProperty> RetrieveValue(TSource instance, Action<object, EventArgs> eventHandler)
        {
            this.CheckRecursion();

            ObservableExpressionFactory.EventSink EventSink = new ObservableExpressionFactory.EventSink(eventHandler);
            try
            {
                return new ObservableObject.CachedValue<TSourceProperty>(this.getter.Invoke(instance, EventSink), EventSink);
            }
            catch (Exception Exception)
            {
                return new ObservableObject.CachedValue<TSourceProperty>(Exception, EventSink);
            }
            finally
            {
                this.UpdateDone();
            }
        }

        private void UpdateDone()
        {
            lock (this.threadIDs)
            {
                this.threadIDs.Remove(Thread.CurrentThread.ManagedThreadId);
            }
        }

        private void CheckRecursion()
        {
            lock (this.threadIDs)
            {
                //if same thread reenters the update value, the retrieval of the update
                //implicitly called the 'propertychanged' event that invalidated this adapter
                //to call Update...() again...
                int ThreadID = Thread.CurrentThread.ManagedThreadId;
                if (this.threadIDs.Contains(ThreadID))
                {
                    throw new CircularDependencyException(string.Format("Recursion occured while retrieving the value for property {0} on type {1}", this.propertyName, typeof(TSource).FullName));
                }
                else
                {
                    this.threadIDs.Add(ThreadID);
                }
            }
        }
    }
}

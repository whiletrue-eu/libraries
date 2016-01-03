using System;
using System.Linq.Expressions;

namespace WhileTrue.Classes.Framework
{
    /// <summary>
    /// Provides an adapter for a read-only property of an underlying model
    /// </summary>
    public class ReadOnlyPropertyAdapter<TPropertyType> : PropertyAdapterBase<TPropertyType>
    {
        private readonly object valueLock=new object();
        private Value<TPropertyType> value;

        internal ReadOnlyPropertyAdapter(Expression<Func<TPropertyType>> getExpression, Action changedCallback)
            : base(getExpression, changedCallback)
        {
        }

        /// <summary>
        /// Gets the value of the underlying property
        /// </summary>
        public TPropertyType GetValue()
        {
			lock(this.valueLock)
			{
                return (this.value ?? (this.value = this.RetrieveValue(_ => _))).GetValue();
            }
        }


        /// <summary>
        /// Notifies inherited classes that some parts of the instances used during expression evaluation changed
        /// </summary>
        protected override void NotifyExpressionChanged(object sender, EventArgs e)
        {
            lock (this.valueLock)
            {
                this.value = null;
                this.InvokeChanged();
            }
        }

        

    }
    /// <summary>
    /// Wraps a property of a source type with change notification forwarding
    /// </summary>
    public class ReadOnlyPropertyAdapter<TSource, TProperty> : PropertyAdapterBase<TSource, TProperty> where TSource : ObservableObject
    {
        internal ReadOnlyPropertyAdapter(string propertyName, Expression<Func<TSource, TProperty>> getExpression)
            : base(propertyName, getExpression)
        {
        }

        private ObservableObject.CachedValue<TProperty> RetrieveValue(TSource source)
        {
            return this.RetrieveValue(source, (sender, e) => this.PropertyChangeCallback(source));
        }

        private void PropertyChangeCallback(TSource source)
        {
            var PropertyValues = source.GetPropertyValueCache();
            lock (PropertyValues)
            {
                PropertyValues.ClearValue(this);
            }
            source.NotifyPropertyChanged(this.PropertyName);
        }


        /// <summary>
        /// Gets the value of the underlying property
        /// </summary>
        public TProperty GetValue(TSource source)
        {
            ObservableObject.PropertyValueCache PropertyValues = source.GetPropertyValueCache();
            lock (PropertyValues)
            {
                if (PropertyValues.HasValue(this))
                {
                    return PropertyValues.GetValue<TSource,TProperty,TProperty>(this).GetValue();
                }
                else
                {
                    return PropertyValues.SetValue(this, this.RetrieveValue(source)).GetValue();
                }
            }
        }
    }
}
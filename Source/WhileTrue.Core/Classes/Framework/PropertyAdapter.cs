using System;
using System.Linq.Expressions;

namespace WhileTrue.Classes.Framework
{
    /// <summary>
    /// Provides an adapter for a read/write property of an underlying model
    /// </summary>
    public class PropertyAdapter<PropertyType> : ReadOnlyPropertyAdapter<PropertyType>
    {
        private readonly Action<PropertyType> setExpression;

        internal PropertyAdapter(Expression<Func<PropertyType>> getExpression, Action<PropertyType> setExpression, Action changedCallback, EventBindingMode eventBindingMode, ValueRetrievalMode valueRetrievalMode)
            : base(getExpression, changedCallback, eventBindingMode, valueRetrievalMode)
        {
            this.setExpression = setExpression;
        }

        /// <summary>
        /// Sets the value of the underlying property
        /// </summary>
        public void SetValue(PropertyType value)
        {
            this.setExpression(value);
        }
    }

    public class PropertyAdapter<TSource, TTargetProperty> : ReadOnlyPropertyAdapter<TSource, TTargetProperty> where TSource : ObservableObject
    {
        private readonly Action<TSource, TTargetProperty> setExpression;

        internal PropertyAdapter(Expression<Func<TSource, TTargetProperty>> propertyAccess, Expression<Func<TSource, TTargetProperty>> getExpression, Action<TSource, TTargetProperty> setExpression)
            : base(propertyAccess, getExpression)
        {
            this.setExpression = setExpression;
        }

        /// <summary>
        /// Sets the value of the underlying property
        /// </summary>
        public void SetValue(TSource source, TTargetProperty value)
        {
            this.setExpression(source, value);
        }
    }
}
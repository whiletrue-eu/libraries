using System;
using System.Linq.Expressions;

namespace WhileTrue.Classes.Framework
{
    /// <summary>
    /// Provides an adapter for a read/write property of an underlying model. Note that the setter is only a convinient functionality; the same result isachieved using a ReadOnlyPropertyAdapter and directly manipulating the source model.
    /// </summary>
    public class PropertyAdapter<TPropertyType> : ReadOnlyPropertyAdapter<TPropertyType>
    {
        private readonly Action<TPropertyType> setExpression;

        internal PropertyAdapter(Expression<Func<TPropertyType>> getExpression, Action<TPropertyType> setExpression, Action changedCallback)
            : base(getExpression, changedCallback)
        {
            this.setExpression = setExpression;
        }

        /// <summary>
        /// Sets the value of the underlying property
        /// </summary>
        public void SetValue(TPropertyType value)
        {
            this.setExpression(value);
        }
    }

    /// <summary>
    /// Provides an adapter for a read/write property of an underlying model. Note that the setter is only a convinient functionality; the same result isachieved using a ReadOnlyPropertyAdapter and directly manipulating the source model.
    /// </summary>
    public class PropertyAdapter<TSource, TTargetProperty> : ReadOnlyPropertyAdapter<TSource, TTargetProperty> where TSource : ObservableObject
    {
        private readonly Action<TSource, TTargetProperty> setExpression;

        internal PropertyAdapter(string propertyName, Expression<Func<TSource, TTargetProperty>> getExpression, Action<TSource, TTargetProperty> setExpression)
            : base(propertyName, getExpression)
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
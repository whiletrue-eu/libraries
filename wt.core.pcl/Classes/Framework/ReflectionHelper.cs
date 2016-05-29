using System;
using System.Linq.Expressions;
using System.Reflection;

namespace WhileTrue.Classes.Framework
{
    internal static class ReflectionHelper
    {
        public static string GetPropertyName(this LambdaExpression expression)
        {
            if ((expression.Body.NodeType == ExpressionType.MemberAccess && ((MemberExpression)expression.Body).Member is PropertyInfo) == false)
            {
                throw new ArgumentException("'expression' must be an property access performed directly on the observable object in the form '()=>this.Property' or just '()=>Property'", nameof(expression));
            }

            return ((PropertyInfo) ((MemberExpression) expression.Body).Member).Name;
        }
    }
}
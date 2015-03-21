using System;
using System.Linq.Expressions;
using System.Reflection;

namespace WhileTrue.Classes.Framework
{
    /// <summary>
    /// Provides simple access on Property data for expressions that only contain Property access (single step access)
    /// </summary>
    public class PropertyInfoReflector
    {
        private readonly PropertyInfo propertyInfo;

        private PropertyInfoReflector(PropertyInfo propertyInfo)
        {
            this.propertyInfo = propertyInfo;
        }

        public PropertyInfoReflector(LambdaExpression expression)
        {
            this.propertyInfo = ((PropertyInfoReflector) expression).propertyInfo;
        }

        /// <summary/>
        public static implicit operator PropertyInfoReflector(LambdaExpression expression)
        {
            if ((expression.Body.NodeType == ExpressionType.MemberAccess && ((MemberExpression)expression.Body).Member is PropertyInfo) == false )
            {
                throw new ArgumentException("'expression' must be an property access performed directly on the observable object in the form '()=>this.Property' or just '()=>Property'", "expression");
            }

            return new PropertyInfoReflector((PropertyInfo) ((MemberExpression)expression.Body).Member);
        }

        /// <summary/>
        public static implicit operator PropertyInfo(PropertyInfoReflector propertyInfo)
        {
            return propertyInfo.propertyInfo;
        }

        /// <summary/>
        public PropertyInfo PropertyInfo
        {
            get
            {
                return propertyInfo;
            }
        }

        public string PropertyName
        {
            get { return this.propertyInfo.Name;
            }
        }
    }
}
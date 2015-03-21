using System;
using System.Linq.Expressions;
using System.Reflection;

namespace WhileTrue.Classes.Framework
{
    /// <summary>
    /// 
    /// </summary>
    public static class ReflectionHelper
    {
        public static string GetPropertyName<T>(Expression<Func<T>> expression)
        {
            return ((PropertyInfoReflector)expression).PropertyName;
        }
        public static PropertyInfo GetPropertyInfo<T>(Expression<Func<T>> expression)
        {
            return ((PropertyInfoReflector)expression).PropertyInfo;
        }
        public static string GetPropertyName(this Expression expression)
        {
            return ((PropertyInfoReflector)expression).PropertyName;
        }
        public static PropertyInfo GetPropertyInfo(this Expression expression)
        {
            return ((PropertyInfoReflector)expression).PropertyInfo;
        }
    }
}
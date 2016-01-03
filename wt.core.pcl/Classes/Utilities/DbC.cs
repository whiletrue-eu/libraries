// ReSharper disable InconsistentNaming

using System;
using JetBrains.Annotations;

namespace WhileTrue.Classes.Utilities
{
    /// <summary>
    /// Implements simple to use 'Design by contract' checkers that throw exceptions if a certain criteria is met.
    /// The methods are usable like a standard assertion (<c>DbC.Assert...</c>) or as a flow interface (<c>value.DbC_Assert...</c>), returning the object they
    /// were called on, so that it is possible to include them into a complex expression involving multiple steps.
    /// </summary>
    [PublicAPI]
    public static class DbC
    {

        #region AssureNotNull
        /// <summary>
        /// Throws a <c>InvalidOperationException</c> in case value is <c>null</c>
        /// </summary>
        [AssertionMethod]
        public static T DbC_AssureNotNull<T>([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]this T value)
        {
            DbC.AssureNotNull(value);
            return value;
        }

        /// <summary>
        /// Throws a <c>InvalidOperationException</c> in case value is <c>null</c>
        /// </summary>
        [AssertionMethod]
        public static ObjectType DbC_AssureNotNull<ObjectType>([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]this ObjectType value, string message, params object[] parameters)
        {
            DbC.AssureNotNull(value, message, parameters);
            return value;
        }

        /// <summary>
        /// Throws a <c>InvalidOperationException</c> in case value is <c>null</c>
        /// </summary>
        [AssertionMethod]
        public static void AssureNotNull<T>(T value, string message = "DbC: value may not be null", params object[] parameters)
        {
            if (object.Equals(value, default(T)))
            {
                throw new InvalidOperationException(string.Format(message, parameters));
            }
        }

        #endregion

        #region AssureNull
        /// <summary>
        /// Throws an <c>InvalidOperationException</c> in case value is not<c>null</c>
        /// </summary>
        [AssertionMethod]
        public static T DbC_AssureNull<T>([AssertionCondition(AssertionConditionType.IS_NULL)]this T value)
        {
            DbC.AssureNull(value);
            return value;
        }

        /// <summary>
        /// Throws an <c>InvalidOperationException</c> in case value is not<c>null</c>
        /// </summary>
        [AssertionMethod]
        public static T DbC_AssureNull<T>([AssertionCondition(AssertionConditionType.IS_NULL)]this T value, string message, params object[] parameters)
        {
            DbC.AssureNull(value, message, parameters);
            return value;
        }

        /// <summary>
        /// Throws an <c>InvalidOperationException</c> in case value is not<c>null</c>
        /// </summary>
        [AssertionMethod]
        public static void AssureNull<T>(T value, string message = "DbC: value may not be != null", params object[] parameters)
        {
            if (object.Equals(value, default(T))==false)
            {
                throw new InvalidOperationException(string.Format(message, parameters));
            }
        }
        #endregion

        /// <summary>
        /// Throws an <c>ArgumentException</c> for the given argumetn name in case value is <c>null</c>
        /// </summary>
        #region AssureArgumentNotNull
        [AssertionMethod]
        public static T DbC_AssureArgumentNotNull<T>([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]this T value, string argumentName)
        {
            DbC.AssureArgumentNotNull(value, argumentName);
            return value;
        }

        /// <summary>
        /// Throws an <c>ArgumentException</c> for the given argumetn name in case value is <c>null</c>
        /// </summary>
        [AssertionMethod]
        public static T DbC_AssureArgumentNotNull<T>([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]this T value, string argumentName, string message, params object[] parameters)
        {
            DbC.AssureArgumentNotNull(value, argumentName, message, parameters);
            return value;
        }

        /// <summary>
        /// Throws an <c>ArgumentException</c> for the given argumetn name in case value is <c>null</c>
        /// </summary>
        [AssertionMethod]
        public static void AssureArgumentNotNull<T>([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]T value, string argumentName)
        {
            DbC.AssureArgumentNotNull(value, argumentName, "DbC: argument {0} may not be null", argumentName);
        }

        /// <summary>
        /// Throws an <c>ArgumentException</c> for the given argumetn name in case value is <c>null</c>
        /// </summary>
        [AssertionMethod]
        public static void AssureArgumentNotNull<T>([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]T value, string argumentName, string message, params object[] parameters)
        {
            if (object.Equals(value, default(T)))
            {
                throw new ArgumentException(string.Format(message, parameters));
            }
        }
        #endregion

        /// <summary>
        /// Throws an <c>ArgumentOutOfRangeException</c> for the given argument name in case value is violating the <c>condition</c>
        /// </summary>
        #region AssureArgumentInRange
        [AssertionMethod]
        public static T DbC_AssureArgumentInRange<T>(this T value, string argumentName, Func<T, bool> condition)
        {
            DbC.AssureArgumentInRange(value, argumentName, condition(value));
            return value;
        }

        /// <summary>
        /// Throws an <c>ArgumentOutOfRangeException</c> for the given argument name in case value is violating the <c>condition</c>
        /// </summary>
        [AssertionMethod]
        public static T DbC_AssureArgumentInRange<T>(this T value, string argumentName, Func<T, bool> condition, string message, params object[] parameters)
        {
            DbC.AssureArgumentInRange(value, argumentName, condition(value), message, parameters);
            return value;
        }

        /// <summary>
        /// Throws an <c>ArgumentOutOfRangeException</c> for the given argument name in case value is violating the <c>condition</c>
        /// </summary>
        [AssertionMethod]
        public static void AssureArgumentInRange<T>(T value, string argumentName, bool condition)
        {
            DbC.AssureArgumentInRange(value, argumentName, condition, "DbC: argument {0} is out of range", argumentName);
        }

        /// <summary>
        /// Throws an <c>ArgumentOutOfRangeException</c> for the given argument name in case value is violating the <c>condition</c>
        /// </summary>
        [AssertionMethod]
        public static void AssureArgumentInRange<T>(T value, string argumentName, bool condition, string message, params object[] parameters)
        {
            if (condition == false)
            {
                throw new ArgumentOutOfRangeException(argumentName, string.Format(message, parameters));
            }
        }
        #endregion

        #region Assure
        /// <summary>
        /// Throws an <c>InvalidOperationException</c> if the <c>condition</c> yields false
        /// </summary>
        [AssertionMethod]
        public static T DbC_Assure<T>(this T value, [AssertionCondition(AssertionConditionType.IS_TRUE)]Predicate<T> condition)
        {
            DbC.Assure(value, condition);
            return value;
        }

        /// <summary>
        /// Throws an <c>InvalidOperationException</c> if the <c>condition</c> yields false
        /// </summary>
        [AssertionMethod]
        public static T DbC_Assure<T>(this T value, [AssertionCondition(AssertionConditionType.IS_TRUE)]Predicate<T> condition, string message,params object[] parameters)
        {
            DbC.Assure(value, condition, message, parameters);
            return value;
        }

        /// <summary>
        /// Throws an user defined exception if the <c>condition</c> yields false
        /// </summary>
        [AssertionMethod]
        public static T DbC_Assure<T>(this T value, [AssertionCondition(AssertionConditionType.IS_TRUE)]Predicate<T> condition, Exception exception)
        {
            DbC.Assure(value, condition, exception);
            return value;
        }

        /// <summary>
        /// Throws an <c>InvalidOperationException</c> if the <c>condition</c> yields false
        /// </summary>
        [AssertionMethod]
        public static void Assure<T>(T value, [AssertionCondition(AssertionConditionType.IS_TRUE)]Predicate<T> condition, string message="",params object[] parameters)
        {
            DbC.Assure(() => condition(value), message, parameters);
        }

        /// <summary>
        /// Throws an user defined exception if the <c>condition</c> yields false
        /// </summary>
        [AssertionMethod]
        public static void Assure<T>(T value, [AssertionCondition(AssertionConditionType.IS_TRUE)]Predicate<T> condition, Exception exception)
        {
            DbC.Assure(()=>condition(value), exception);
        }

        /// <summary>
        /// Throws an <c>InvalidOperationException</c> if the <c>condition</c> yields false
        /// </summary>
        [AssertionMethod]
        public static void Assure([AssertionCondition(AssertionConditionType.IS_TRUE)]Func<bool> condition, string message = "", params object[] parameters)
        {
            DbC.Assure(condition, new InvalidOperationException(string.Format(message, parameters)));
        }

        /// <summary>
        /// Throws an user defined exception if the <c>condition</c> yields false
        /// </summary>
        [AssertionMethod]
        public static void Assure([AssertionCondition(AssertionConditionType.IS_TRUE)]Func<bool> condition, Exception exception)
        {
            DbC.Assure(condition(), exception);
        }

        /// <summary>
        /// Throws an <c>InvalidOperationException</c> if the <c>condition</c> yields false
        /// </summary>
        [AssertionMethod]
        public static void Assure([AssertionCondition(AssertionConditionType.IS_TRUE)]bool condition, string message = "", params object[] parameters)
        {
            DbC.Assure(condition, new InvalidOperationException(string.Format(message, parameters)));
        }

        /// <summary>
        /// Throws an user defined exception if the <c>condition</c> yields false
        /// </summary>
        [AssertionMethod]
        public static void Assure([AssertionCondition(AssertionConditionType.IS_TRUE)]bool condition, Exception exception)
        {
            if (condition == false)
            {
                throw exception;
            }
        }

        #endregion

        /// <summary>
        /// Throws an <c>InvalidOperationException</c> if value does not implement the interface <c>interfaceType</c>
        /// </summary>
        #region AssureImplements
        public static ObjectType DbC_AssureImplements<ObjectType>(this ObjectType value, Type interfaceType)
        {
            DbC.AssureImplements(value,interfaceType);
            return value;
        }

        /// <summary>
        /// Throws an <c>InvalidOperationException</c> if value does not implement the interface <c>interfaceType</c>
        /// </summary>
        public static void AssureImplements<ObjectType>(ObjectType value, Type interfaceType)
        {
            if (interfaceType.IsInstanceOfType(value)==false)
            {
                throw new InvalidOperationException($"value does not implement type {interfaceType.FullName}");
            }
        }
        #endregion
    }
}

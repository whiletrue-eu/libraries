// ReSharper disable InconsistentNaming
using System;
using System.Windows.Resources;
using WhileTrue.Classes.CodeInspection;

namespace WhileTrue.Classes.Utilities
{
    [NoCoverage]
    public static class DbC
    {

        #region AssureNotNull
        [AssertionMethod]
        public static Type DbC_AssureNotNull<Type>([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]this Type value)
        {
            DbC.AssureNotNull(value);
            return value;
        }

        [AssertionMethod]
        public static ObjectType DbC_AssureNotNull<ObjectType>([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]this ObjectType value, string message, params object[] parameters)
        {
            DbC.AssureNotNull(value, message, parameters);
            return value;
        }

        [AssertionMethod]
        public static void AssureNotNull<Type>(Type value, string message = "DbC: value may not be null", params object[] parameters)
        {
            if (Equals(value, default(Type)))
            {
                throw new InvalidOperationException(string.Format(message, parameters));
            }
        }

        #endregion

        #region AssureNull
        [AssertionMethod]
        public static Type DbC_AssureNull<Type>([AssertionCondition(AssertionConditionType.IS_NULL)]this Type value)
        {
            DbC.AssureNull(value);
            return value;
        }

        [AssertionMethod]
        public static Type DbC_AssureNull<Type>([AssertionCondition(AssertionConditionType.IS_NULL)]this Type value, string message, params object[] parameters)
        {
            DbC.AssureNull(value, message, parameters);
            return value;
        }

        [AssertionMethod]
        public static void AssureNull<Type>(Type value, string message = "DbC: value may not be != null", params object[] parameters)
        {
            if (Equals(value, default(Type))==false)
            {
                throw new InvalidOperationException(string.Format(message, parameters));
            }
        }
        #endregion

        #region AssureArgumentNotNull
        [AssertionMethod]
        public static Type DbC_AssureArgumentNotNull<Type>([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]this Type value, string argumentName)
        {
            DbC.AssureArgumentNotNull(value, argumentName);
            return value;
        }

        [AssertionMethod]
        public static Type DbC_AssureArgumentNotNull<Type>([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]this Type value, string argumentName, string message, params object[] parameters)
        {
            DbC.AssureArgumentNotNull(value, argumentName, message, parameters);
            return value;
        }

        [AssertionMethod]
        public static void AssureArgumentNotNull<Type>(Type value, string argumentName)
        {
            DbC.AssureArgumentNotNull(value, argumentName, "DbC: argument {0} may not be null", argumentName);
        }

        [AssertionMethod]
        public static void AssureArgumentNotNull<Type>(Type value, string argumentName, string message, params object[] parameters)
        {
            if (Equals(value, default(Type)))
            {
                throw new ArgumentException(string.Format(message, parameters));
            }
        }
        #endregion

        #region AssureArgumentInRange
        [AssertionMethod]
        public static Type DbC_AssureArgumentInRange<Type>(this Type value, string argumentName, Func<Type, bool> condition)
        {
            DbC.AssureArgumentInRange(value, argumentName, condition(value));
            return value;
        }

        [AssertionMethod]
        public static Type DbC_AssureArgumentInRange<Type>(this Type value, string argumentName, Func<Type, bool> condition, string message, params object[] parameters)
        {
            DbC.AssureArgumentInRange(value, argumentName, condition(value), message, parameters);
            return value;
        }

        [AssertionMethod]
        public static void AssureArgumentInRange<Type>(Type value, string argumentName, bool condition)
        {
            DbC.AssureArgumentInRange(value, argumentName, condition, "DbC: argument {0} is out of range", argumentName);
        }

        [AssertionMethod]
        public static void AssureArgumentInRange<Type>(Type value, string argumentName, bool condition, string message, params object[] parameters)
        {
            if (condition == false)
            {
                throw new ArgumentOutOfRangeException(argumentName, string.Format(message, parameters));
            }
        }
        #endregion

        #region Assure
        [AssertionMethod]
        public static Type DbC_Assure<Type>(this Type value, [AssertionCondition(AssertionConditionType.IS_TRUE)]Predicate<Type> condition)
        {
            DbC.Assure(value, condition);
            return value;
        }

        [AssertionMethod]
        public static Type DbC_Assure<Type>(this Type value, [AssertionCondition(AssertionConditionType.IS_TRUE)]Predicate<Type> condition, string message,params object[] parameters)
        {
            DbC.Assure(value, condition, message, parameters);
            return value;
        }

        [AssertionMethod]
        public static Type DbC_Assure<Type>(this Type value, [AssertionCondition(AssertionConditionType.IS_TRUE)]Predicate<Type> condition, Exception exception)
        {
            DbC.Assure(value, condition, exception);
            return value;
        }

        [AssertionMethod]
        public static void Assure<Type>(Type value, [AssertionCondition(AssertionConditionType.IS_TRUE)]Predicate<Type> condition, string message="",params object[] parameters)
        {
            DbC.Assure(() => condition(value), message, parameters);
        }
        
        [AssertionMethod]
        public static void Assure<Type>(Type value, [AssertionCondition(AssertionConditionType.IS_TRUE)]Predicate<Type> condition, Exception exception)
        {
            DbC.Assure(()=>condition(value), exception);
        }

        [AssertionMethod]
        public static void Assure([AssertionCondition(AssertionConditionType.IS_TRUE)]Func<bool> condition, string message = "", params object[] parameters)
        {
            DbC.Assure(condition, new InvalidOperationException(string.Format(message, parameters)));
        }

        [AssertionMethod]
        public static void Assure([AssertionCondition(AssertionConditionType.IS_TRUE)]Func<bool> condition, Exception exception)
        {
            DbC.Assure(condition(), exception);
        }

        [AssertionMethod]
        public static void Assure([AssertionCondition(AssertionConditionType.IS_TRUE)]bool condition, string message = "", params object[] parameters)
        {
            DbC.Assure(condition, new InvalidOperationException(string.Format(message, parameters)));
        }

        [AssertionMethod]
        public static void Assure([AssertionCondition(AssertionConditionType.IS_TRUE)]bool condition, Exception exception)
        {
            if (condition == false)
            {
                throw exception;
            }
        }

        #endregion

        #region AssureImplements
        public static ObjectType DbC_AssureImplements<ObjectType>(this ObjectType value, Type interfaceType)
        {
            DbC.AssureImplements(value,interfaceType);
            return value;
        }

        public static void AssureImplements<ObjectType>(ObjectType value, Type interfaceType)
        {
            if (interfaceType.IsInstanceOfType(value)==false)
            {
                throw new InvalidOperationException(string.Format("value does not implement type {0}", interfaceType.FullName));
            }
        }
        #endregion
    }
}

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;


namespace WhileTrue.Classes.Framework
{
    /// <summary>
    /// Provides an implementation of <c>INotifyPropertyChanged</c> for classes that cannot inherit from <see cref="ObservableObject"/> because they need anaother base class
    /// </summary>
    [PublicAPI]
    public static class ObservableObjectHelper
    {
        /// <summary>
        /// Sets the given backing <c>field</c> in case that <c>value</c> changed (using the <c>equals</c> method to compare) the <c>propertyName</c> should be given using the nameof() expression
        /// </summary>
        public static void SetAndInvoke<TFieldType>(this object target, string propertyName, PropertyChangedEventHandler propertyChangedHandler, ref TFieldType field, TFieldType newValue, Action<string> changedDelegate = null)
        {
            if (object.Equals(field, newValue) == false)
            {
                field = newValue;
                try
                {
                    propertyChangedHandler?.Invoke(target, new PropertyChangedEventArgs(propertyName));
                    changedDelegate?.Invoke(propertyName);
                }
                catch (Exception Exception)
                {
                    Debug.WriteLine("WARNING: a PropertyChanged event handler threw an exception!\nYou must make sure that exceptions are not thrown from an event handler.\nMessage: {0}\nStackTrace:{1}", Exception.Message, Exception.StackTrace);
                }
            }
        }
        /// <summary>
        /// Sets the given backing <c>field</c> in case that <c>value</c> changed (using the <c>equals</c> method to compare) the <c>propertyName</c> is automatically added fromt he caller information provided by the compiler
        /// </summary>
        public static void SetAndInvoke<TFieldType>(this object target, PropertyChangedEventHandler propertyChangedHandler, ref TFieldType field, TFieldType newValue, Action<string> changedDelegate = null, [CallerMemberName] string propertyName=null)
        {
            target.SetAndInvoke(propertyName, propertyChangedHandler, ref field, newValue, changedDelegate);
        }
    }
}
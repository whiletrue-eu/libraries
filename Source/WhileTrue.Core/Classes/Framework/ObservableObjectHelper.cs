using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;


namespace WhileTrue.Classes.Framework
{
    public static partial class ObservableObjectHelper
    {
        public static void SetAndInvoke<FieldType, PropertyType>(this object target, Expression<Func<PropertyType>> property, ref FieldType field, FieldType newValue, PropertyChangingEventHandler propertyChangingHandler, PropertyChangedEventHandler propertyChangedHandler) where FieldType:PropertyType
        {
            SetAndInvoke<FieldType, PropertyType>(target, propertyChangedHandler, propertyChangingHandler, ref field, newValue, property.GetPropertyName());
        }

        public static void SetAndInvoke<FieldType, PropertyType>(this object target, Expression<Func<PropertyType>> property, ref FieldType field, FieldType newValue, PropertyChangingEventHandler propertyChangingHandler, PropertyChangedEventHandler propertyChangedHandler, Action<string> changingDelegate, Action<string> changedDelegate) where FieldType : PropertyType
        {
            SetAndInvoke<FieldType, PropertyType>(target, propertyChangedHandler, propertyChangingHandler, ref field, newValue, property.GetPropertyName(), changedDelegate, changingDelegate);
        }

        public static void SetAndInvoke<FieldType, PropertyType>(this object target, PropertyChangedEventHandler propertyChangedHandler, PropertyChangingEventHandler propertyChangingHandler, ref FieldType field, FieldType newValue, string propertyName) where FieldType : PropertyType
        {
            SetAndInvoke<FieldType, PropertyType>(target, propertyChangedHandler, propertyChangingHandler, ref field, newValue, propertyName, delegate { }, delegate { });
        }

        public static void SetAndInvoke<FieldType, PropertyType>(this object target, PropertyChangedEventHandler propertyChangedHandler, PropertyChangingEventHandler propertyChangingHandler, ref FieldType field, FieldType newValue, string propertyName, Action<string> changedDelegate, Action<string> changingDelegate) where FieldType : PropertyType
        {
            if (Equals(field, newValue) == false)
            {
                try
                {
                    if (propertyChangingHandler != null)
                    {
                        propertyChangingHandler(target, new PropertyChangingEventArgs(propertyName));
                    }
                    if (changingDelegate != null)
                    {
                        changingDelegate(propertyName);
                    }
                }
                catch (Exception Exception)
                {
                   Trace.Fail(string.Format("WARNING: a PropertyChanging event handler threw an exception!\nYou must make sure that exceptions are not thrown from an event handler.\nMessage: {0}\nStackTrace:{1}",Exception.Message, Exception.StackTrace));
                }
                field = newValue;
                try
                {
                    if (propertyChangedHandler != null)
                    {
                        propertyChangedHandler(target, new PropertyChangedEventArgs(propertyName));
                    }
                    if (changedDelegate != null)
                    {
                        changedDelegate(propertyName);
                    }
                }
                catch (Exception Exception)
                {
                    Trace.Fail(string.Format("WARNING: a PropertyChanged event handler threw an exception!\nYou must make sure that exceptions are not thrown from an event handler.\nMessage: {0}\nStackTrace:{1}", Exception.Message, Exception.StackTrace));
                }
            }
        }




        
        public interface IEventAdapterFactory
        {
        }

        public interface IEventAdapter
        {
            IEventAdapter Attach(string sourcePropertyName, params string[] targetPropertyNames);
            IEventAdapter Ignore(string sourcePropertyName);
            IEventAdapterFactory IgnoreTheRest();
            IEventAdapterFactory AssertOnOthers();
            IEventAdapter AttachSubProperty(string propertyName, Func<INotifyPropertyChanged> source, Func<IEventAdapter, IEventAdapterFactory> registration);
        }
    }
}
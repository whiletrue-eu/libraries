// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable InvokeAsExtensionMethod
// ReSharper disable UnusedMember.Global
#pragma warning disable 1574 //xmldoc
#pragma warning disable 1584,1711,1572,1581,1580 //xmldoc
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using WhileTrue.Classes.Logging;
using WhileTrue.Classes.Utilities;


namespace WhileTrue.Classes.Framework
{
    /// <summary>
    /// Base class for objects that implement change notifications
    /// </summary>
    /// <remarks>
    /// <para>
    /// This base class provides support for model implementation that allow easy binding to its properties
    /// through the implementation of <see cref="INotifyPropertyChanged"/> (and <see cref="INotifyPropertyChanging"/> resp.).
    /// </para>
    /// <para>
    /// It also implements the IDataErrorInfo and thus adds validation features to the class.
    /// </para>
    /// <para>
    /// To make your class send change notifications, you can easily use the <see cref="SetAndInvoke"/> methods to
    /// set a new value for a property. These methods also optimize the events, i.e. they supress chaning when the new value
    /// does not differ from the old one. To have even more control, you can use the <see cref="InvokePropertyChanged"/> and
    /// <see cref="InvokePropertyChanging"/> methods to issue the event when required.
    /// </para>
    /// <para>
    /// The <see cref="AddValidationForProperty"/> method enables you to create validation rules for single properties.
    /// The values will be validated every time the property changes. The results are published using the <see cref="IDataErrorInfo"/>
    /// interface which is also supported by WPF Validators.
    /// </para>
    /// <para>
    /// This class also supports the creation of 'Adapters' for your data models. These features come handy when creating adapter models for your
    /// business models, e.g. for a 'View Model' when using the MVVM pattern. The <see cref="CreatePropertyAdapter"/>
    /// allow you to create special adaptes for the properties of the underlying model, which automatically issue change notifications without
    /// the need for you to write special event handling code, thus making the task of creating model adapters trivial.
    ///   </para>
    /// <para>
    /// All methods of this class are created refactoring firendly by avioding the use of string for property names. Instead, the real propeties are
    /// used and evaluated using the power of lambda expressions.
    /// </para>
    /// </remarks>
    public abstract partial class ObservableObject : INotifyPropertyChanged, INotifyPropertyChanging, IObjectValidation //, IDynamicMetaObjectProvider
    {
        protected ObservableObject()
        {
            this.propertyValueCache = new PropertyValueCache();
        }

        #region Property Change Notification

        /// <summary>
        /// Implementation of the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => DebugLogger.WriteLine(sender, LoggingLevel.Normal, () => string.Format("PropertyChanged({0})", e.PropertyName));

        /// <summary>
        /// Fires the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <remarks>
        /// <para>
        /// With this method you can fire the <see cref="PropertyChanged"/> event for one of your properties.
        /// </para>
        /// <para>
        /// The property is referenced by a lambda expression that makes the call refactoring friendly.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code source="..\Doc\Examples\ObservableObject.cs" region="Invoke" lang="cs"/>
        /// </example>
        protected void InvokePropertyChanged<PropertyType>(Expression<Func<PropertyType>> property)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(property.GetPropertyName()));
        }

        /// <summary>
        /// Implementation of the <see cref="INotifyPropertyChanging.PropertyChanging"/> event.
        /// </summary>
        public event PropertyChangingEventHandler PropertyChanging = delegate { };

        /// <summary>
        /// Fires the <see cref="PropertyChanging"/> event.
        /// </summary>
        /// <remarks>
        /// <para>
        /// With this method you can fire the <see cref="PropertyChanging"/> event for one of your properties.
        /// </para>
        /// <para>
        /// The property is referenced by a lambda expression that makes the call refactoring friendly.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code source="..\Doc\Examples\ObservableObject.cs" region="Invoke" lang="cs"/>
        /// </example>
        protected void InvokePropertyChanging<PropertyType>(Expression<Func<PropertyType>> property)
        {
            this.PropertyChanging(this, new PropertyChangingEventArgs(property.GetPropertyName()));
        }

        /// <summary>
        /// Sets the given property backing field and fires the <see cref="PropertyChanged"/> event if needed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// With this method you can set the backing field of your property.
        /// If the value is different form the old one, it fires the <see cref="PropertyChanging"/> and <see cref="PropertyChanged"/> events.
        /// </para>
        /// <para>
        /// This version of the method allows you also to specify two delegates to call custom event handlers (e.g.
        /// if you also want to implement custom XYZChanged and XYZChanging handlers). These handlers are also only 
        /// called when the value really changed.
        /// </para>
        /// <para>
        /// The property is referenced by a lambda expression that makes the call refactoring friendly.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code source="..\Doc\Examples\ObservableObject.cs" region="SetAndInvokeWithCustomEvents" lang="cs"/>
        /// </example>
        protected void SetAndInvoke<FieldType, PropertyType>(Expression<Func<PropertyType>> property, ref FieldType field, FieldType newValue, Action<string> changingDelegate, Action<string> changeDelegate) where FieldType : PropertyType
        {
            ObservableObjectHelper.SetAndInvoke(this, property, ref field, newValue, this.PropertyChanging, this.PropertyChanged, changingDelegate, changeDelegate);
        }

        /// <summary>
        /// Sets the given property backing field and fires the <see cref="PropertyChanged"/> event if needed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// With this method you can set the backing field of your property.
        /// If the value is different form the old one, it fires the <see cref="PropertyChanging"/> and <see cref="PropertyChanged"/> events.
        /// </para>
        /// <para>
        /// The property is referenced by a lambda expression that makes the call refactoring friendly.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code source="..\Doc\Examples\ObservableObject.cs" region="SetAndInvoke" lang="cs"/>
        /// </example>
        protected void SetAndInvoke<FieldType, PropertyType>(Expression<Func<PropertyType>> property, ref FieldType field, FieldType newValue) where FieldType : PropertyType
        {
            ObservableObjectHelper.SetAndInvoke(this, property, ref field, newValue, this.PropertyChanging, this.PropertyChanged);
        }

        #endregion

        #region Property handling for models / adapters


        protected static IPropertyAdapterFactory<T> GetPropertyAdapterFactory<T>() where T : ObservableObject
        {
            return new PropertyAdapterFactory<T>();
        }

        internal PropertyValueCache GetPropertyValueCache()
        {
            return this.propertyValueCache;
        }

        internal void NotifyPropertyChanged(string propertyName, object sender, EventArgs e)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private class PropertyAdapterFactory<T> : IPropertyAdapterFactory<T> where T : ObservableObject
        {
            public PropertyAdapter<T, TTargetProperty> Create<TTargetProperty>(Expression<Func<T, TTargetProperty>> propertyAccess, Expression<Func<T, TTargetProperty>> getter, Action<T, TTargetProperty> setter)
            {
                return new PropertyAdapter<T, TTargetProperty>(propertyAccess, getter, setter);
            }

            public ReadOnlyPropertyAdapter<T, TTargetProperty> Create<TTargetProperty>(Expression<Func<T, TTargetProperty>> propertyAccess, Expression<Func<T, TTargetProperty>> getter)
            {
                return new ReadOnlyPropertyAdapter<T, TTargetProperty>(propertyAccess, getter);
            }

            public EnumerablePropertyAdapter<T, TSourceEnumerationItem, TTargetEnumerationItem> Create<TSourceEnumerationItem, TTargetEnumerationItem>(
                Expression<Func<T, IEnumerable<TTargetEnumerationItem>>> propertyAccess, Expression<Func<T, IEnumerable<TSourceEnumerationItem>>> getExpression, Expression<Func<T, TSourceEnumerationItem, TTargetEnumerationItem>> adapterCreation)
            {
                return new EnumerablePropertyAdapter<T, TSourceEnumerationItem, TTargetEnumerationItem>(propertyAccess, getExpression, adapterCreation);
            }
        }

        public interface IPropertyAdapterFactory<T> where T : ObservableObject
        {
            PropertyAdapter<T, TTargetProperty> Create<TTargetProperty>(Expression<Func<T, TTargetProperty>> propertyAccess, Expression<Func<T, TTargetProperty>> getter, Action<T, TTargetProperty> setter);
            ReadOnlyPropertyAdapter<T, TTargetProperty> Create<TTargetProperty>(Expression<Func<T, TTargetProperty>> propertyAccess, Expression<Func<T, TTargetProperty>> getter);

            EnumerablePropertyAdapter<T, TSourceEnumerationItem, TTargetEnumerationItem> Create<TSourceEnumerationItem, TTargetEnumerationItem>(
                Expression<Func<T, IEnumerable<TTargetEnumerationItem>>> propertyAccess, Expression<Func<T, IEnumerable<TSourceEnumerationItem>>> getExpression, Expression<Func<T, TSourceEnumerationItem, TTargetEnumerationItem>> adapterCreation);
        }


        /// <summary>
        /// Creates a adapter to an underlying model to support a property that can only be read.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method allows you to create a 'backing adapter' for a get property you want to expose in your
        /// adapter model class.
        /// </para>
        /// <para>
        /// To do that, you can give a lambda expression that retrieves the corresponding property from the underlying model.
        /// The path to that property may have several steps, e.g. not a direct property of the dependend model is retrieved, but
        /// a property of an instance returned by a property of the underlying model.
        /// </para>
        /// <para>
        /// When the value is retrieved using the lambda expression, the adapter automatically records all <see cref="INotifyPropertyChanged.PropertyChanged"/>
        /// and <see cref="INotifyCollectionChanged.CollectionChanged"/> events as needed to be notified when any value within the path
        /// used to retrieve the property value changes. This works as long as all objects returned by any property on the path implement
        /// <see cref="INotifyPropertyChanged"/> and all collections returned implement <see cref="INotifyCollectionChanged"/>.
        /// </para>
        /// <para>
        /// You may even call methods, as long as they do not access sub-properties of the values used as parameters, as these
        /// value retrievals would occur out-of-scope from the expression.
        /// </para>
        /// <para>
        /// For all event handlers registered you can set the <see cref="EventBindingMode"/> to create standard or weak event handlers.
        /// </para>
        /// <para>
        /// To retrieve the value as implementation of the property, you can simply call the <see cref="ReadOnlyPropertyAdapter{PropertyType}.GetValue"/> method.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// public class MyClass : ObservableObject
        /// {
        ///     private readonly IUnderlyingModel underlyingModel;
        ///     private readonly ReadOnlyPropertyAdapter&lt;string> myPropertyAdapter;
        /// 
        ///     public void MyClass(IUnderlyingModel underlyingModel)
        ///     {
        ///         this.underlyingModel = underlyingModel;
        /// 
        ///         this.myPropertyAdapter = this.CreatePropertyAdapter(
        ///             ()=>MyProperty,
        ///             ()=>underlyingModel.TheProperty,
        ///             EventBindingMode.Weak
        ///             );
        ///     }
        /// 
        ///     public string MyProperty
        ///     {
        ///         get
        ///         {
        ///             return this.myPropertyAdapter.GetValue();
        ///         }
        ///     }    
        /// }
        /// </code>
        /// </example>
        protected ReadOnlyPropertyAdapter<PropertyType> CreatePropertyAdapter<Property, PropertyType>(
            Expression<Func<Property>> property, Expression<Func<PropertyType>> getExpression, EventBindingMode eventBindingMode, ValueRetrievalMode valueRetrievalMode)
        {
            return new ReadOnlyPropertyAdapter<PropertyType>(getExpression, () => this.InvokePropertyChanged(property), eventBindingMode, valueRetrievalMode);
        }

        /// <summary>
        /// Creates a adapter to an underlying model to support a property that can be read and written.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method allows you to create a 'backing adapter' for a get/set property you want to expose in your
        /// adapter model class.
        /// </para>
        /// <para>
        /// To do that, you can give a lambda expression that retrieves the corresponding property from the underlying model, as well
        /// as one to set the value.
        /// The path to that property may have several steps, e.g. not a direct property of the dependend model is retrieved, but
        /// a property of an instance returned by a property of the underlying model.
        /// </para>
        /// <para>
        /// When the value is retrieved using the lambda expression, the adapter automatically records all <see cref="INotifyPropertyChanged.PropertyChanged"/>
        /// and <see cref="INotifyCollectionChanged.CollectionChanged"/> events as needed to be notified when any value within the path
        /// used to retrieve the property value changes. This works as long as all objects returned by any property on the path implement
        /// <see cref="INotifyPropertyChanged"/> and all collections returned implement <see cref="INotifyCollectionChanged"/>.
        /// </para>
        /// <para>
        /// You may even call methods, as long as they do not access sub-properties of the values used as parameters, as these
        /// value retrievals would occur out-of-scope from the expression.
        /// </para>
        /// <para>
        /// For all event handlers registered you can set the <see cref="EventBindingMode"/> to create standard or weak event handlers.
        /// </para>
        /// <para>
        /// To retrieve the value as implementation of the property, you can simply call the <see cref="PropertyAdapter{PropertyType}.GetValue"/> method.
        /// To set the value again, use the <see cref="PropertyAdapter{PropertyType}.SetValue"/> method.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// public class MyClass : ObservableObject
        /// {
        ///     private readonly IUnderlyingModel underlyingModel;
        ///     private readonly PropertyAdapter&lt;string> myPropertyAdapter;
        /// 
        ///     public void MyClass(IUnderlyingModel underlyingModel)
        ///     {
        ///         this.underlyingModel = underlyingModel;
        /// 
        ///         this.myPropertyAdapter = this.CreatePropertyAdapter(
        ///             ()=>MyProperty,
        ///             ()=>underlyingModel.TheProperty,
        ///             value=>underlyingModel.TheProperty = value,
        ///             EventBindingMode.Weak
        ///             );
        ///     }
        /// 
        ///     public string MyProperty
        ///     {
        ///         get
        ///         {
        ///             return this.myPropertyAdapter.GetValue();
        ///         }
        ///         set
        ///         {
        ///             this.myPropertyAdapter.SetValue(value);
        ///         }
        ///     }    
        /// }
        /// </code>
        /// </example>
        protected PropertyAdapter<PropertyType> CreatePropertyAdapter<PropertyType, Property>(
            Expression<Func<Property>> property, Expression<Func<PropertyType>> getExpression, Action<PropertyType> setExpression, EventBindingMode eventBindingMode, ValueRetrievalMode valueRetrievalMode)
        {
            return new PropertyAdapter<PropertyType>(getExpression, setExpression, () => this.InvokePropertyChanged(property), eventBindingMode, valueRetrievalMode);
        }

        /*// <summary>
        ///  Creates a adapter to an underlying model to support a property of a class type that can only be read.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method allows you to create a 'backing adapter' for a get property you want to expose in your
        /// adapter model class, which is wrapped by an other adapter class by using a provided delegate.
        /// </para>
        /// <para>
        /// If the value returned is the same one as for the last call, no new adapter instance is created and the change event
        /// is suppressed.
        /// </para>
        /// <para>
        /// To do that, you can give a lambda expression that retrieves the corresponding property from the underlying model.
        /// The path to that property may have several steps, e.g. not a direct property of the dependend model is retrieved, but
        /// a property of an instance returned by a property of the underlying model.
        /// </para>
        /// <para>
        /// When the value is retrieved using the lambda expression, the adapter automatically records all <see cref="INotifyPropertyChanged.PropertyChanged"/>
        /// and <see cref="INotifyCollectionChanged.CollectionChanged"/> events as needed to be notified when any value within the path
        /// used to retrieve the property value changes. This works as long as all objects returned by any property on the path implement
        /// <see cref="INotifyPropertyChanged"/> and all collections returned implement <see cref="INotifyCollectionChanged"/>.
        /// </para>
        /// <para>
        /// You may even call methods, as long as they do not access sub-properties of the values used as parameters, as these
        /// value retrievals would occur out-of-scope from the expression.
        /// </para>
        /// <para>
        /// For all event handlers registered you can set the <see cref="EventBindingMode"/> to create standard or weak event handlers.
        /// </para>
        /// <para>
        /// To retrieve the value wrapped in the adapter class instance as implementation of the property, you can simply call the 
        /// <see cref="ReadOnlyPropertyAdapter{PropertyType}.GetValue"/> method.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// public class MyClass : ObservableObject
        /// {
        ///     private readonly IUnderlyingModel underlyingModel;
        ///     private readonly ReadOnlyObjectPropertyAdapter&lt;ISubModel, SubModelAdapter> subModelPropertyAdapter;
        /// 
        ///     public void MyClass(IUnderlyingModel underlyingModel)
        ///     {
        ///         this.underlyingModel = underlyingModel;
        /// 
        ///         this.subModelPropertyAdapter = this.CreatePropertyAdapter(
        ///             ()=>SubModelProperty,
        ///             ()=>underlyingModel.SubModelProperty,
        ///             value => new SubModelAdapter(value),
        ///             EventBindingMode.Weak
        ///             );
        ///     }
        /// 
        ///     public SubModelAdapter SubModelProperty
        ///     {
        ///         get
        ///         {
        ///             return this.subModelPropertyAdapter.GetValue();
        ///         }
        ///     }    
        /// }
        /// </code>
        /// </example>*/

        protected EnumerablePropertyAdapter<SourcePropertyType, PropertyType> CreatePropertyAdapter<SourcePropertyType, PropertyType>(
            Expression<Func<IEnumerable<PropertyType>>> property, Expression<Func<IEnumerable<SourcePropertyType>>> getExpression, EventBindingMode eventBindingMode, ValueRetrievalMode valueRetrievalMode,
            Expression<Func<SourcePropertyType, PropertyType>> adapterCreation)
        {
            return new EnumerablePropertyAdapter<SourcePropertyType, PropertyType>(getExpression, adapterCreation, () => this.InvokePropertyChanged(property), eventBindingMode, valueRetrievalMode);
        }

        /// <summary>
        /// Creates a adapter to an underlying model to support a property that can only be read.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method allows you to create a 'backing adapter' for a get property you want to expose in your
        /// adapter model class.
        /// </para>
        /// <para>
        /// To do that, you can give a lambda expression that retrieves the corresponding property from the underlying model.
        /// The path to that property may have several steps, e.g. not a direct property of the dependend model is retrieved, but
        /// a property of an instance returned by a property of the underlying model.
        /// </para>
        /// <para>
        /// When the value is retrieved using the lambda expression, the adapter automatically records all <see cref="INotifyPropertyChanged.PropertyChanged"/>
        /// and <see cref="INotifyCollectionChanged.CollectionChanged"/> events as needed to be notified when any value within the path
        /// used to retrieve the property value changes. This works as long as all objects returned by any property on the path implement
        /// <see cref="INotifyPropertyChanged"/> and all collections returned implement <see cref="INotifyCollectionChanged"/>.
        /// </para>
        /// <para>
        /// You may even call methods, as long as they do not access sub-properties of the values used as parameters, as these
        /// value retrievals would occur out-of-scope from the expression.
        /// </para>
        /// <para>
        /// For all event handlers registered you can set the <see cref="EventBindingMode"/> to create standard or weak event handlers.
        /// </para>
        /// <para>
        /// To retrieve the value as implementation of the property, you can simply call the <see cref="ReadOnlyPropertyAdapter{PropertyType}.GetValue"/> method.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// public class MyClass : ObservableObject
        /// {
        ///     private readonly IUnderlyingModel underlyingModel;
        ///     private readonly ReadOnlyPropertyAdapter&lt;string> myPropertyAdapter;
        /// 
        ///     public void MyClass(IUnderlyingModel underlyingModel)
        ///     {
        ///         this.underlyingModel = underlyingModel;
        /// 
        ///         this.myPropertyAdapter = this.CreatePropertyAdapter(
        ///             ()=>MyProperty,
        ///             ()=>underlyingModel.TheProperty,
        ///             EventBindingMode.Weak
        ///             );
        ///     }
        /// 
        ///     public string MyProperty
        ///     {
        ///         get
        ///         {
        ///             return this.myPropertyAdapter.GetValue();
        ///         }
        ///     }    
        /// }
        /// </code>
        /// </example>
        protected ReadOnlyPropertyAdapter<TSourceProperty> CreatePropertyAdapter<TSourceProperty, TProperty>(Expression<Func<TProperty>> property, Expression<Func<TSourceProperty>> getExpression, Action propertyChangedCallback = null)
        {
            return new ReadOnlyPropertyAdapter<TSourceProperty>(
                getExpression,
                () =>
                {
                    if (propertyChangedCallback != null)
                    {
                        propertyChangedCallback();
                    }
                    this.InvokePropertyChanged(property);
                },
                EventBindingMode.Weak, ValueRetrievalMode.Lazy);
        }

        /// <summary>
        /// Creates a adapter to an underlying model to support a property that can be read and written.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method allows you to create a 'backing adapter' for a get/set property you want to expose in your
        /// adapter model class.
        /// </para>
        /// <para>
        /// To do that, you can give a lambda expression that retrieves the corresponding property from the underlying model, as well
        /// as one to set the value.
        /// The path to that property may have several steps, e.g. not a direct property of the dependend model is retrieved, but
        /// a property of an instance returned by a property of the underlying model.
        /// </para>
        /// <para>
        /// When the value is retrieved using the lambda expression, the adapter automatically records all <see cref="INotifyPropertyChanged.PropertyChanged"/>
        /// and <see cref="INotifyCollectionChanged.CollectionChanged"/> events as needed to be notified when any value within the path
        /// used to retrieve the property value changes. This works as long as all objects returned by any property on the path implement
        /// <see cref="INotifyPropertyChanged"/> and all collections returned implement <see cref="INotifyCollectionChanged"/>.
        /// </para>
        /// <para>
        /// You may even call methods, as long as they do not access sub-properties of the values used as parameters, as these
        /// value retrievals would occur out-of-scope from the expression.
        /// </para>
        /// <para>
        /// For all event handlers registered you can set the <see cref="EventBindingMode"/> to create standard or weak event handlers.
        /// </para>
        /// <para>
        /// To retrieve the value as implementation of the property, you can simply call the <see cref="PropertyAdapter{PropertyType}.GetValue"/> method.
        /// To set the value again, use the <see cref="PropertyAdapter{PropertyType}.SetValue"/> method.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// public class MyClass : ObservableObject
        /// {
        ///     private readonly IUnderlyingModel underlyingModel;
        ///     private readonly PropertyAdapter&lt;string> myPropertyAdapter;
        /// 
        ///     public void MyClass(IUnderlyingModel underlyingModel)
        ///     {
        ///         this.underlyingModel = underlyingModel;
        /// 
        ///         this.myPropertyAdapter = this.CreatePropertyAdapter(
        ///             ()=>MyProperty,
        ///             ()=>underlyingModel.TheProperty,
        ///             value=>underlyingModel.TheProperty = value,
        ///             EventBindingMode.Weak
        ///             );
        ///     }
        /// 
        ///     public string MyProperty
        ///     {
        ///         get
        ///         {
        ///             return this.myPropertyAdapter.GetValue();
        ///         }
        ///         set
        ///         {
        ///             this.myPropertyAdapter.SetValue(value);
        ///         }
        ///     }    
        /// }
        /// </code>
        /// </example>
        protected PropertyAdapter<TSourceProperty> CreatePropertyAdapter<TSourceProperty, TProperty>(
            Expression<Func<TProperty>> property, Expression<Func<TSourceProperty>> getExpression, Action<TSourceProperty> setExpression, Action propertyChangedCallback = null)
        {
            return new PropertyAdapter<TSourceProperty>(
                getExpression,
                setExpression,
                () =>
                {
                    if (propertyChangedCallback != null)
                    {
                        propertyChangedCallback();
                    }
                    this.InvokePropertyChanged(property);
                },
                EventBindingMode.Weak, ValueRetrievalMode.Lazy);
        }

        /*// <summary>
        ///  Creates a adapter to an underlying model to support a property of a class type that can only be read.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method allows you to create a 'backing adapter' for a get property you want to expose in your
        /// adapter model class, which is wrapped by an other adapter class by using a provided delegate.
        /// </para>
        /// <para>
        /// If the value returned is the same one as for the last call, no new adapter instance is created and the change event
        /// is suppressed.
        /// </para>
        /// <para>
        /// To do that, you can give a lambda expression that retrieves the corresponding property from the underlying model.
        /// The path to that property may have several steps, e.g. not a direct property of the dependend model is retrieved, but
        /// a property of an instance returned by a property of the underlying model.
        /// </para>
        /// <para>
        /// When the value is retrieved using the lambda expression, the adapter automatically records all <see cref="INotifyPropertyChanged.PropertyChanged"/>
        /// and <see cref="INotifyCollectionChanged.CollectionChanged"/> events as needed to be notified when any value within the path
        /// used to retrieve the property value changes. This works as long as all objects returned by any property on the path implement
        /// <see cref="INotifyPropertyChanged"/> and all collections returned implement <see cref="INotifyCollectionChanged"/>.
        /// </para>
        /// <para>
        /// You may even call methods, as long as they do not access sub-properties of the values used as parameters, as these
        /// value retrievals would occur out-of-scope from the expression.
        /// </para>
        /// <para>
        /// For all event handlers registered you can set the <see cref="EventBindingMode"/> to create standard or weak event handlers.
        /// </para>
        /// <para>
        /// To retrieve the value wrapped in the adapter class instance as implementation of the property, you can simply call the 
        /// <see cref="ReadOnlyPropertyAdapter{PropertyType}.GetValue"/> method.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// public class MyClass : ObservableObject
        /// {
        ///     private readonly IUnderlyingModel underlyingModel;
        ///     private readonly ReadOnlyObjectPropertyAdapter&lt;ISubModel, SubModelAdapter> subModelPropertyAdapter;
        /// 
        ///     public void MyClass(IUnderlyingModel underlyingModel)
        ///     {
        ///         this.underlyingModel = underlyingModel;
        /// 
        ///         this.subModelPropertyAdapter = this.CreatePropertyAdapter(
        ///             ()=>SubModelProperty,
        ///             ()=>underlyingModel.SubModelProperty,
        ///             value => new SubModelAdapter(value),
        ///             EventBindingMode.Weak
        ///             );
        ///     }
        /// 
        ///     public SubModelAdapter SubModelProperty
        ///     {
        ///         get
        ///         {
        ///             return this.subModelPropertyAdapter.GetValue();
        ///         }
        ///     }    
        /// }
        /// </code>
        /// </example>*/

        protected EnumerablePropertyAdapter<TSourceProperty, TProperty> CreatePropertyAdapter<TSourceProperty, TProperty, TPropertyEnumeration>(
            Expression<Func<TPropertyEnumeration>> property, Expression<Func<IEnumerable<TSourceProperty>>> getExpression, Expression<Func<TSourceProperty, TProperty>> adapterCreation) where TPropertyEnumeration : IEnumerable<TProperty>
        {
            return new EnumerablePropertyAdapter<TSourceProperty, TProperty>(getExpression, adapterCreation, () => this.InvokePropertyChanged(property), EventBindingMode.Weak, ValueRetrievalMode.Lazy);
        }


        #endregion

        #region Model validation

        private readonly List<PropertyConstraint> constraints = new List<PropertyConstraint>();
        private PropertyValueCache propertyValueCache;

        /// <summary>
        /// Register validation methods for a given property
        /// </summary>
        /// <remarks>
        /// This method specifies the property to register validation methods for. It returns a flow interface (
        /// <see cref="IPropertyConstraintFactory{PropertyType}"/>) to register multiple methods in a row without
        /// having to specify the property again.
        /// </remarks>
        protected IPropertyConstraintFactory<PropertyType> AddValidationForProperty<PropertyType>(Expression<Func<PropertyType>> property)
        {
            return new PropertyConstraintFactory<PropertyType>(this, property);
        }

        /// <summary>
        /// Register validation methods for two given property
        /// </summary>
        /// <remarks>
        /// This method specifies the properties to register validation methods for. It returns a flow interface (
        /// <see cref="IPropertyConstraintFactory{Property1Type,Property2Type}"/>) to register multiple methods in a row without
        /// having to specify the property again.
        /// </remarks>
        protected IPropertyConstraintFactory<Property1Type, Property2Type> AddValidationForProperties<Property1Type, Property2Type>(Expression<Func<Property1Type>> property1, Expression<Func<Property2Type>> property2)
        {
            return new PropertyConstraintFactory<Property1Type, Property2Type>(this, property1, property2);
        }

        private class PropertyConstraintFactory<PropertyType> : IPropertyConstraintFactory<PropertyType>
        {
            private readonly ObservableObject owner;
            private readonly string propertyName;
            private readonly Func<PropertyType> propertyGetter;

            public PropertyConstraintFactory(ObservableObject owner, Expression<Func<PropertyType>> property)
            {
                this.owner = owner;
                this.propertyName = property.GetPropertyName();
                this.propertyGetter = property.Compile();
            }

            public IPropertyConstraintFactory<PropertyType> AddValidation(Expression<Func<PropertyType, bool>> validation, Expression<Func<PropertyType, ValidationMessage>> message)
            {
                this.owner.constraints.Add(new ExpressionPropertyConstraint<PropertyType>(this.owner, this.propertyName, this.propertyGetter, validation, message));
                return this;
            }
        }

        private class PropertyConstraintFactory<Property1Type, Property2Type> : IPropertyConstraintFactory<Property1Type, Property2Type>
        {
            private readonly ObservableObject owner;
            private readonly string[] propertyNames;
            private readonly Func<Property1Type> property1Getter;
            private readonly Func<Property2Type> property2Getter;

            public PropertyConstraintFactory(ObservableObject owner, Expression<Func<Property1Type>> property1, Expression<Func<Property2Type>> property2)
            {
                this.owner = owner;
                this.propertyNames = new[]
                {
                    property1.GetPropertyName(),
                    property2.GetPropertyName(),
                };
                this.property1Getter = property1.Compile();
                this.property2Getter = property2.Compile();
            }

            public IPropertyConstraintFactory<Property1Type, Property2Type> AddValidation(Expression<Func<Property1Type, Property2Type, bool>> validation, Expression<Func<Property1Type, Property2Type, ValidationMessage>> message)
            {
                this.owner.constraints.Add(new ExpressionPropertyConstraint<Property1Type, Property2Type>(this.owner, this.propertyNames, this.property1Getter, this.property2Getter, validation, message));
                return this;
            }
        }

        private void InvokeValidationChanged(string propertyName)
        {
            this.ValidationChanged(this, new ValidationEventArgs(propertyName));
        }

        private abstract class PropertyConstraint
        {
            public abstract string[] PropertyNames { get; }
            public abstract ValidationMessage ValidationMessage { get; }
            public abstract string PreviewErrors(string propertyName, object value);
        }

        private abstract class ExpressionPropertyConstraintBase : PropertyConstraint
        {
            private readonly ObservableObject owner;
            private readonly string[] propertyNames;
            private bool? success;
            private ValidationMessage validationMessage;
            private static readonly ValidationMessage EmptyMessage = (ValidationMessage) "";

            protected ExpressionPropertyConstraintBase(ObservableObject owner, string[] propertyNames)
            {
                this.owner = owner;
                this.propertyNames = propertyNames;
                this.owner.PropertyChanged += OwnerPropertyChanged;
            }

            private void OwnerPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (Enumerable.Contains(this.propertyNames, e.PropertyName))
                {
                    this.Validate();
                }
            }


            public override ValidationMessage ValidationMessage
            {
                get
                {
                    if (this.validationMessage == null)
                    {
                        this.UpdateMessage();
                    }
                    return this.success.Value ? null : this.validationMessage;
                }
            }

            private void SetValidationMessage(ValidationMessage value)
            {
                this.validationMessage = value;
            }

            public override string[] PropertyNames
            {
                get { return this.propertyNames; }
            }

            protected abstract bool ValidateInternal();
            protected abstract ValidationMessage UpdateMessageInternal();

            protected abstract string PreviewErrorsInternal(string propertyName, object value);

            protected void Validate()
            {
                try
                {
                    bool Success = this.ValidateInternal();
                    if (this.success != Success)
                    {
                        this.success = Success;
                        this.InvalidateMessage();
                    }
                }
                catch
                {
                    this.success = true;
                    this.InvalidateMessage();
                }
            }

            protected void UpdateMessage()
            {
                //Validation is deferred until someone really cares for a validation message
                if (this.success.HasValue == false)
                {
                    this.Validate();
                }

                if (this.success == false)
                {
                    try
                    {
                        string Message = this.UpdateMessageInternal();
                        this.SetValidationMessage(Message);
                    }
                    catch
                    {
                        this.SetValidationMessage(EmptyMessage);
                    }
                }
                else
                {
                    this.SetValidationMessage(EmptyMessage);
                }
            }

            public void InvalidateMessage()
            {
                if (this.validationMessage != null)
                {
                    this.validationMessage = null;
                    this.NotifyValidationChanged();
                }
            }

            public override string PreviewErrors(string propertyName, object value)
            {
                try
                {
                    return this.PreviewErrorsInternal(propertyName, value);
                }
                catch (Exception Exception)
                {
                    return Exception.Message;
                }
            }

            private void NotifyValidationChanged()
            {
                this.propertyNames.ForEach(name => this.owner.InvokeValidationChanged(name));
            }
        }

        private class ExpressionPropertyConstraint<PropertyType> : ExpressionPropertyConstraintBase
        {
            private readonly Func<PropertyType> propertyGetter;
            private readonly NotifyChangeExpression<Func<PropertyType, bool>> validation;
            private readonly NotifyChangeExpression<Func<PropertyType, ValidationMessage>> message;

            public ExpressionPropertyConstraint(ObservableObject owner, string propertyName, Func<PropertyType> propertyGetter, Expression<Func<PropertyType, bool>> validation, Expression<Func<PropertyType, ValidationMessage>> message)
                : base(owner, new[] {propertyName})
            {
                this.propertyGetter = propertyGetter;
                this.validation = new NotifyChangeExpression<Func<PropertyType, bool>>(validation, EventBindingMode.Weak);
                this.validation.Changed += ValidationChanged;
                this.message = new NotifyChangeExpression<Func<PropertyType, ValidationMessage>>(message, EventBindingMode.Weak);
                this.message.Changed += MessageChanged;
            }

            private void ValidationChanged(object sender, EventArgs e)
            {
                this.Validate();
            }

            private void MessageChanged(object sender, EventArgs e)
            {
                this.UpdateMessage();
            }

            protected override bool ValidateInternal()
            {
                PropertyType Value = this.propertyGetter();
                return this.validation.Invoke(Value);
            }

            protected override ValidationMessage UpdateMessageInternal()
            {
                return this.message.Invoke(this.propertyGetter());
            }

            protected override string PreviewErrorsInternal(string propertyName, object value)
            {
                try
                {
                    PropertyType Value = (PropertyType) Convert.ChangeType(value, typeof (PropertyType));
                    if (this.validation.Invoke(Value) == false)
                    {
                        return this.message.Invoke(Value);
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        private class ExpressionPropertyConstraint<Property1Type, Property2Type> : ExpressionPropertyConstraintBase
        {
            private readonly Func<Property1Type> property1Getter;
            private readonly Func<Property2Type> property2Getter;
            private readonly NotifyChangeExpression<Func<Property1Type, Property2Type, bool>> validation;
            private readonly NotifyChangeExpression<Func<Property1Type, Property2Type, ValidationMessage>> message;

            public ExpressionPropertyConstraint(
                ObservableObject owner, string[] propertyNames, Func<Property1Type> property1Getter, Func<Property2Type> property2Getter, Expression<Func<Property1Type, Property2Type, bool>> validation,
                Expression<Func<Property1Type, Property2Type, ValidationMessage>> message)
                : base(owner, propertyNames)
            {
                this.property1Getter = property1Getter;
                this.property2Getter = property2Getter;
                this.validation = new NotifyChangeExpression<Func<Property1Type, Property2Type, bool>>(validation, EventBindingMode.Weak);
                this.validation.Changed += ValidationChanged;
                this.message = new NotifyChangeExpression<Func<Property1Type, Property2Type, ValidationMessage>>(message, EventBindingMode.Weak);
                this.message.Changed += MessageChanged;
            }

            private void ValidationChanged(object sender, EventArgs e)
            {
                this.Validate();
            }

            private void MessageChanged(object sender, EventArgs e)
            {
                this.UpdateMessage();
            }

            protected override bool ValidateInternal()
            {
                Property1Type Value1 = this.property1Getter();
                Property2Type Value2 = this.property2Getter();
                return this.validation.Invoke(Value1, Value2);
            }

            protected override ValidationMessage UpdateMessageInternal()
            {
                Property1Type Value1 = this.property1Getter();
                Property2Type Value2 = this.property2Getter();
                return this.message.Invoke(Value1, Value2);
            }

            protected override string PreviewErrorsInternal(string propertyName, object value)
            {
                try
                {

                    Property1Type Value1 = propertyName == this.PropertyNames[0] ? Conversion.ChangeType<Property1Type>(value) : this.property1Getter();
                    Property2Type Value2 = propertyName == this.PropertyNames[1] ? Conversion.ChangeType<Property2Type>(value) : this.property2Getter();

                    if (this.validation.Invoke(Value1, Value2) == false)
                    {
                        return this.message.Invoke(Value1, Value2);
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        string IDataErrorInfo.this[string propertyName]
        {
            get
            {
                return this.GetValidationResult(propertyName);
            }
        }

        private string GetValidationResult(string propertyName)
        {
            string[] Errors = this.GetValidationResults(propertyName);
            if (Errors.Length > 0)
            {
                return string.Join(ValidationMessage.Separator, Errors);
            }
            else
            {
                return string.Empty;
            }
        }

        private string[] GetValidationResults(string propertyName)
        {
            return (from ValidationMessage in this.GetValidationMessages(propertyName)
                select (string) ValidationMessage
                ).Distinct().ToArray();
        }

        public IEnumerable<ValidationMessage> GetValidationMessages(string propertyName)
        {
            return (from PropertyConstraint Constraint in this.constraints
                where Constraint.PropertyNames.Contains(propertyName)
                where Constraint.ValidationMessage != null
                select Constraint.ValidationMessage).ToArray(); //ToArray() to avoid recursion on deffered execution
        }

        public bool HasErrors(string propertyName)
        {
            return this.GetValidationMessages(propertyName).FirstOrDefault() != null;
        }

        string IObjectValidation.PreviewErrors<PropertyType>(Expression<Func<PropertyType>> property, object value)
        {
            return ((IObjectValidation) this).PreviewErrors(property.GetPropertyName(), value);
        }

        public IEnumerable<ValidationMessage> GetValidationMessages<PropertyType>(Expression<Func<PropertyType>> property)
        {
            return this.GetValidationMessages(property.GetPropertyName());
        }

        public bool HasErrors<PropertyType>(Expression<Func<PropertyType>> property)
        {
            return this.HasErrors(property.GetPropertyName());
        }

        string IDataErrorInfo.Error
        {
            get { return string.Empty; }
        }

        string IObjectValidation.PreviewErrors(string propertyName, object value)
        {
            string[] Errors =
                (from string Message in
                    from PropertyConstraint Constraint in this.constraints
                    where Constraint.PropertyNames.Contains(propertyName)
                    select Constraint.PreviewErrors(propertyName, value)
                    where string.IsNullOrEmpty(Message) == false
                    select Message).Distinct().ToArray();

            if (Errors.Length > 0)
            {
                return string.Join(ValidationMessage.Separator, Errors);
            }
            else
            {
                return string.Empty;
            }
        }

        ///<summary/>
        public event EventHandler<ValidationEventArgs> ValidationChanged = delegate { };

        #endregion

        /*
        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            throw new NotImplementedException();
        }*/


        internal class PropertyValueCache
        {
            private readonly Dictionary<object,object> values = new Dictionary<object, object>();

            public CachedValue<TProperty> SetValue<TSource, TSourceProperty, TProperty>(PropertyAdapterBase<TSource, TSourceProperty, TProperty> adapter, CachedValue<TProperty> value) where TSource : ObservableObject
            {
                if (this.values.ContainsKey(adapter))
                {
                    this.values[adapter] = value;
                }
                else
                {
                    this.values.Add(adapter, value);
                }
                return value;
            }

            public CachedValue<TProperty> GetValue<TSource, TSourceProperty, TProperty>(PropertyAdapterBase<TSource, TSourceProperty, TProperty> adapter) where TSource : ObservableObject
            {
                return (CachedValue<TProperty>) this.values[adapter];
            }

            public bool HasValue<TSource, TSourceProperty, TProperty>(PropertyAdapterBase<TSource, TSourceProperty, TProperty> adapter) where TSource : ObservableObject
            {
                return this.values.ContainsKey(adapter);
            }

            public void ClearValue<TSource, TSourceProperty, TProperty>(PropertyAdapterBase<TSource, TSourceProperty, TProperty> adapter) where TSource : ObservableObject
            {
                this.values.Remove(adapter);
            }
        }

        internal class CachedValue<T>
        {
            private readonly T value;
            // ReSharper disable once NotAccessedField.Local - reference must be stored so that it is not garbage collected
            private readonly ObservableExpressionFactory.EventSink eventSink;
            private readonly Exception exception;

            public CachedValue(T value, ObservableExpressionFactory.EventSink eventSink)
            {
                this.value = value;
                this.eventSink = eventSink;
                this.exception = null;
            }

            public CachedValue(Exception exception, ObservableExpressionFactory.EventSink eventSink)
            {
                this.value = default(T);
                this.exception = exception;
                this.eventSink = eventSink;
            }

            public ObservableExpressionFactory.EventSink EventSink { get { return this.eventSink; } }

            public T GetValue()
            {
                if (this.exception != null)
                {
                    throw this.exception;
                }
                return this.value;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (obj.GetType() != typeof(CachedValue<T>)) return false;
                return Equals((CachedValue<T>)obj);
            }

            public bool Equals(CachedValue<T> other)
            {
                if (ReferenceEquals(null, other)) return false;
                return Equals(other.value, this.value) && Equals(other.exception, this.exception);
            }

            public override int GetHashCode()
            {
                return (Equals(this.value, default(T)) ? 0 : this.value.GetHashCode() ^ (this.exception != null ? this.exception.GetHashCode() : 0));
            }
        }

    }
}
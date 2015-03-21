// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable InvokeAsExtensionMethod
// ReSharper disable UnusedMember.Global
#pragma warning disable 1574 //xmldoc
#pragma warning disable 1584,1711,1572,1581,1580 //xmldoc
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
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
    /// To make your class send change notifications, you can easily use the <see cref="SetAndInvoke{TFieldType}(ref TFieldType,TFieldType,System.Action{string},string)"/> methods to
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
    /// business models, e.g. for a 'View Model' when using the MVVM pattern. The <see cref="CreatePropertyAdapter{TSourceProperty,TProperty}"/>
    /// allow you to create special adaptes for the properties of the underlying model, which automatically issue change notifications without
    /// the need for you to write special event handling code, thus making the task of creating model adapters trivial.
    ///   </para>
    /// <para>
    /// All methods of this class are created refactoring firendly by avioding the use of string for property names. Instead, the real propeties are
    /// used and evaluated using the power of lambda expressions.
    /// </para>
    /// </remarks>
    public abstract partial class ObservableObject : INotifyPropertyChanged, IObjectValidation
    {
        /// <summary/>
        protected ObservableObject()
        {
            this.propertyValueCache = new PropertyValueCache();
        }

        #region Property Change Notification

        /// <summary>
        /// Implementation of the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => DebugLogger.WriteLine(sender, LoggingLevel.Normal, () => $"PropertyChanged({e.PropertyName})");

        /// <summary>
        /// Fires the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <remarks>
        /// <para>
        /// With this method you can fire the <see cref="PropertyChanged"/> event for one of your properties.
        /// </para>
        /// <para>
        /// The property name should be given by a nameof() expression to enable refactoring.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code source="..\Doc\Examples\ObservableObject.cs" region="Invoke" lang="cs"/>
        /// </example>
        protected void InvokePropertyChanged(string propertyName)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Sets the given property backing field and fires the <see cref="PropertyChanged"/> event if needed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// With this method you can set the backing field of your property with name <c>propertyName</c> anywhere in your code.
        /// If the value is different form the old one, it fires the <see cref="PropertyChanged"/> events.
        /// </para>
        /// <para>
        /// This method allows you also to specify a delegate to call custom event handlers (e.g.
        /// if you also want to implement custom XYZChanged handler). This handler is also only 
        /// called when the value really changed.
        /// </para>
        /// <para>
        /// As property, you should specify a nameof() expression to make sure that the name is adopted when refacturing.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code source="..\Doc\Examples\ObservableObject.cs" region="SetAndInvokeWithCustomEvents" lang="cs"/>
        /// </example>
        protected void SetAndInvoke<TFieldType>(string propertyName, ref TFieldType field, TFieldType newValue, Action<string> changeDelegate=null)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            ObservableObjectHelper.SetAndInvoke(this, this.PropertyChanged, ref field, newValue, changeDelegate, propertyName);
        }     
        
        
        /// <summary>
        /// Sets the given backing field and fires the <see cref="PropertyChanged"/> event if needed for the property this method is called from.
        /// </summary>
        /// <remarks>
        /// <para>
        /// With this method you can set the backing field of a property within the property set accessor.
        /// If the value is different form the old one, it fires the  <see cref="PropertyChanged"/> event.
        /// </para>
        /// <para>
        /// This method allows you also to specify a delegate to call custom event handlers (e.g.
        /// if you also want to implement custom XYZChanged handler). This handler is also only 
        /// called when the value really changed.
        /// </para>
        /// <para>
        /// The property name is automatically added by the compiler using the callerMemberName attribute.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code source="..\Doc\Examples\ObservableObject.cs" region="SetAndInvokeWithCustomEvents" lang="cs"/>
        /// </example>
        protected void SetAndInvoke<TFieldType>(ref TFieldType field, TFieldType newValue, Action<string> changeDelegate=null, [CallerMemberName] string propertyName = null)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            ObservableObjectHelper.SetAndInvoke(this, this.PropertyChanged, ref field, newValue, changeDelegate, propertyName);
        }

        #endregion

        #region Property handling for models / adapters


        /// <summary>
        /// Returns the propertyAdapterFactory that allows to create static property adapters that can be used with all instances of the observable object derived class.
        /// As type parameter give the type of the current class. Use within static constructur of this class.
        /// Using the static version over the insatnce version of CreatepropertyAdapter is more complex from the syntactic point of view but is more performant if many instances are created
        /// at a high rate, as the expressions given have only to be compiled once.
        /// </summary>
        protected static IPropertyAdapterFactory<T> GetPropertyAdapterFactory<T>() where T : ObservableObject
        {
            return new PropertyAdapterFactory<T>();
        }

        internal PropertyValueCache GetPropertyValueCache()
        {
            return this.propertyValueCache;
        }

        internal void NotifyPropertyChanged(string propertyName)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private class PropertyAdapterFactory<T> : IPropertyAdapterFactory<T> where T : ObservableObject
        {
            public PropertyAdapter<T, TTargetProperty> Create<TTargetProperty>(string propertyName, Expression<Func<T, TTargetProperty>> getter, Action<T, TTargetProperty> setter)
            {
                return new PropertyAdapter<T, TTargetProperty>(propertyName, getter, setter);
            }

            public ReadOnlyPropertyAdapter<T, TTargetProperty> Create<TTargetProperty>(string propertyName, Expression<Func<T, TTargetProperty>> getter)
            {
                return new ReadOnlyPropertyAdapter<T, TTargetProperty>(propertyName, getter);
            }

            public EnumerablePropertyAdapter<T, TSourceEnumerationItem, TTargetEnumerationItem> Create<TSourceEnumerationItem, TTargetEnumerationItem>(
                string propertyName, Expression<Func<T, IEnumerable<TSourceEnumerationItem>>> getExpression, Expression<Func<T, TSourceEnumerationItem, TTargetEnumerationItem>> adapterCreation)
            {
                return new EnumerablePropertyAdapter<T, TSourceEnumerationItem, TTargetEnumerationItem>(propertyName, getExpression, adapterCreation);
            }
        }

        /// <summary>
        /// This interface is used in the <see cref="ObservableObject.CreatePropertyAdapter{TPropertyType}()"/> call to provide creation of static property adapters that can be shared for all instances of the same type
        /// </summary>
        [PublicAPI]
        public interface IPropertyAdapterFactory<T> where T : ObservableObject
        {
            /// <summary>
            /// Creates a adapter to an underlying model to support a property that can be read and written.
            /// </summary>
            /// <remarks>
            /// <para>
            /// This method allows you to create a 'backing adapter' for a get/set property you want to expose in your
            /// adapter model class.
            /// Note that the write accessor is only a convenience function; you can also use a read only adapter and do writes directly on the source property
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
            /// To retrieve the value as implementation of the property, you can simply call the <see cref="PropertyAdapter{PropertyType}.GetValue"/> method.
            /// To set the value again, use the <see cref="PropertyAdapter{PropertyType}.SetValue"/> method.
            /// </para>
            /// </remarks>
            /// <example>
            /// <code>
            /// public class MyClass : ObservableObject
            /// {
            ///     private readonly IUnderlyingModel underlyingModel;
            ///     private static readonly PropertyAdapter&lt;MyClass,string> myPropertyAdapter;
            /// 
            ///     static MyClass()
            ///     {
            ///         IPropertyAdapterFactory Factory = ObservableObject.CreateAdapterFactory%lt;MyClass>();
            ///         MyCLass.myPropertyAdapter = Factory.Create(
            ///             nameof(MyProperty),
            ///             instance=>instance.underlyingModel.TheProperty,
            ///             (instance,value)=>instance.underlyingModel.TheProperty = value
            ///             );
            ///     }
            /// 
            ///     public void MyClass(IUnderlyingModel underlyingModel)
            ///     {
            ///         this.underlyingModel = underlyingModel;
            ///     }
            /// 
            ///     public string MyProperty
            ///     {
            ///         get
            ///         {
            ///             return this.myPropertyAdapter.GetValue(this);
            ///         }
            ///         set
            ///         {
            ///             this.myPropertyAdapter.SetValue(this, value);
            ///         }
            ///     }    
            /// }
            /// </code>
            /// </example>
            PropertyAdapter<T, TTargetProperty> Create<TTargetProperty>(string propertyName, Expression<Func<T, TTargetProperty>> getter, Action<T, TTargetProperty> setter);

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
            /// To retrieve the value as implementation of the property, you can simply call the <see cref="ReadOnlyPropertyAdapter{PropertyType}.GetValue"/> method.
            /// </para>
            /// </remarks>
            /// <example>
            /// <code>
            /// public class MyClass : ObservableObject
            /// {
            ///     private readonly IUnderlyingModel underlyingModel;
            ///     private static readonly ReadOnlyPropertyAdapter&lt;MyClass,string> myPropertyAdapter;
            /// 
            ///     static MyClass()
            ///     {
            ///         IPropertyAdapterFactory Factory = ObservableObject.CreateAdapterFactory%lt;MyClass>();
            ///         MyCLass.myPropertyAdapter = Factory.Create(
            ///             nameof(MyProperty),
            ///             instance=>instance.underlyingModel.TheProperty
            ///             );
            ///     }
            /// 
            ///     public void MyClass(IUnderlyingModel underlyingModel)
            ///     {
            ///         this.underlyingModel = underlyingModel;
            ///     }
            /// 
            ///     public string MyProperty
            ///     {
            ///         get
            ///         {
            ///             return this.myPropertyAdapter.GetValue(this);
            ///         }
            ///     }    
            /// }
            /// </code>
            /// </example>
            ReadOnlyPropertyAdapter<T, TTargetProperty> Create<TTargetProperty>(string propertyName, Expression<Func<T, TTargetProperty>> getter);

            /// <summary>
            ///  Creates an adapter to an underlying model to support a property of a class that is enumerable.
            /// </summary>
            /// <remarks>
            /// <para>
            /// This method allows you to create a 'backing adapter' for a get property you want to expose in your
            /// adapter model class as an enumaeration. Each enumeration item is wrapped by an other adapter class by using a provided delegate.
            /// </para>
            /// <para>
            /// If the value returned is the same one as for the last call, no new adapter instance is created and the change event
            /// is suppressed.
            /// </para>
            /// <para>
            /// You give a lambda expression that retrieves the corresponding property from the underlying model.
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
            /// To retrieve the value wrapped in the adapter class instance as implementation of the property, you can simply call the 
            /// <see cref="EnumerablePropertyAdapter{TSourcePropertyType,TPropertyType}.GetCollection"/> method. The collection returned is observable
            /// </para>
            /// </remarks>
            /// <example>
            /// <code>
            /// public class MyClass : ObservableObject
            /// {
            ///     private readonly IUnderlyingModel underlyingModel;
            ///     private readonly EnumerablePropertryAdapter&lt;MyClass, IUnderlyingModel, EnumerationItem, SubModelAdapter> subModelPropertyAdapter;
            /// 
            ///     static MyClass()
            ///     {
            ///         IPropertyAdapterFactory Factory = ObservableObject.CreateAdapterFactory%lt;MyClass>();
            ///         MyCLass.myPropertyAdapter = Factory.Create(
            ///             nameof(MyProperty),
            ///             instance=>instance.underlyingModel.SubModelProperty.Where(item=>item.Relevant),
            ///             value => SubModelAdapter.GetObject(value)
            ///             );
            ///     }
            ///  
            ///     public void MyClass(IUnderlyingModel underlyingModel)
            ///     {
            ///         this.underlyingModel = underlyingModel;
            ///     }
            /// 
            ///     public IEnumerable&lt;SubModelAdapter> SubModelProperty
            ///     {
            ///         get
            ///         {
            ///             return this.subModelPropertyAdapter.GetCollection(this);
            ///         }
            ///     }    
            /// }
            /// </code>
            /// </example>
            EnumerablePropertyAdapter<T, TSourceEnumerationItem, TTargetEnumerationItem> Create<TSourceEnumerationItem, TTargetEnumerationItem>(
                string propertyName, Expression<Func<T, IEnumerable<TSourceEnumerationItem>>> getExpression, Expression<Func<T, TSourceEnumerationItem, TTargetEnumerationItem>> adapterCreation);
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
        ///             nameof(MyProperty),
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
        /// <para>
        /// There is also a static version, <see cref="GetPropertyAdapterFactory{T}()"/> that is more performant when many instances of this type are created at a high rate
        /// </para>
        /// </example>
        protected ReadOnlyPropertyAdapter<TSourceProperty> CreatePropertyAdapter<TSourceProperty>(string propertyName, Expression<Func<TSourceProperty>> getExpression, Action propertyChangedCallback = null)
        {
            return new ReadOnlyPropertyAdapter<TSourceProperty>(
                getExpression,
                () =>
                {
                    propertyChangedCallback?.Invoke();
                    this.InvokePropertyChanged(propertyName);
                });
        }

        /// <summary>
        /// Creates a adapter to an underlying model to support a property that can be read and written.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method allows you to create a 'backing adapter' for a get/set property you want to expose in your
        /// adapter model class.
        /// Note that the write accessor is only a convenience function; you can also use a read only adapter and do writes directly on the source property
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
        ///             nameof(MyProperty),
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
        /// <para>
        /// There is also a static version, <see cref="GetPropertyAdapterFactory{T}()"/> that is more performant when many instances of this type are created at a high rate
        /// </para>
        protected PropertyAdapter<TSourceProperty> CreatePropertyAdapter<TSourceProperty>(
            string propertyName, Expression<Func<TSourceProperty>> getExpression, Action<TSourceProperty> setExpression, Action propertyChangedCallback = null)
        {
            return new PropertyAdapter<TSourceProperty>(
                getExpression,
                setExpression,
                () =>
                {
                    propertyChangedCallback?.Invoke();
                    this.InvokePropertyChanged(propertyName);
                });
        }

        /// <summary>
        ///  Creates an adapter to an underlying model to support a property of a class that is enumerable.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method allows you to create a 'backing adapter' for a get property you want to expose in your
        /// adapter model class as an enumaeration. Each enumeration item is wrapped by an other adapter class by using a provided delegate.
        /// </para>
        /// <para>
        /// If the value returned is the same one as for the last call, no new adapter instance is created and the change event
        /// is suppressed.
        /// </para>
        /// <para>
        /// You give a lambda expression that retrieves the corresponding property from the underlying model.
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
        /// To retrieve the value wrapped in the adapter class instance as implementation of the property, you can simply call the 
        /// <see cref="EnumerablePropertyAdapter{TSourcePropertyType,TPropertyType}.GetCollection"/> method. The collection returned is observable
        /// </para>
        /// <para>
        /// There is also a static version, <see cref="GetPropertyAdapterFactory{T}()"/> that is more performant when many instances of this type are created at a high rate
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// public class MyClass : ObservableObject
        /// {
        ///     private readonly IUnderlyingModel underlyingModel;
        ///     private readonly EnumerablePropertryAdapter&lt;IUnderlyingModel, EnumerationItem, SubModelAdapter> subModelPropertyAdapter;
        /// 
        ///     public void MyClass(IUnderlyingModel underlyingModel)
        ///     {
        ///         this.underlyingModel = underlyingModel;
        /// 
        ///         this.subModelPropertyAdapter = this.CreatePropertyAdapter(
        ///             ()=>SubModelProperty,
        ///             ()=>underlyingModel.SubModelProperty.Where(item=>item.Relevant),
        ///             value => SubModelAdapter.GetObject(value)
        ///             );
        ///     }
        /// 
        ///     public IEnumerable&lt;SubModelAdapter> SubModelProperty
        ///     {
        ///         get
        ///         {
        ///             return this.subModelPropertyAdapter.GetCollection();
        ///         }
        ///     }    
        /// }
        /// </code>
        /// </example>

        protected EnumerablePropertyAdapter<TSourceProperty, TProperty> CreatePropertyAdapter<TSourceProperty, TProperty>(
            string propertyName, Expression<Func<IEnumerable<TSourceProperty>>> getExpression, Expression<Func<TSourceProperty, TProperty>> adapterCreation)
        {
            return new EnumerablePropertyAdapter<TSourceProperty, TProperty>(getExpression, adapterCreation, () => this.InvokePropertyChanged(propertyName));
        }


        #endregion

        #region Model validation

        private readonly List<PropertyConstraint> constraints = new List<PropertyConstraint>();
        private readonly PropertyValueCache propertyValueCache;

        /// <summary>
        /// Register validation methods for a given property
        /// </summary>
        /// <remarks>
        /// This method specifies the property to register validation methods for. It returns a flow interface (
        /// <see cref="IPropertyConstraintFactory{PropertyType}"/>) to register multiple methods in a row without
        /// having to specify the property again.
        /// </remarks>
        protected IPropertyConstraintFactory<TPropertyType> AddValidationForProperty<TPropertyType>(Expression<Func<TPropertyType>> property)
        {
            return new PropertyConstraintFactory<TPropertyType>(this, property);
        }

        /// <summary>
        /// Register validation methods for two given property
        /// </summary>
        /// <remarks>
        /// This method specifies the properties to register validation methods for. It returns a flow interface (
        /// <see cref="IPropertyConstraintFactory{Property1Type,Property2Type}"/>) to register multiple methods in a row without
        /// having to specify the property again.
        /// </remarks>
        protected IPropertyConstraintFactory<TProperty1Type, TProperty2Type> AddValidationForProperties<TProperty1Type, TProperty2Type>(Expression<Func<TProperty1Type>> property1, Expression<Func<TProperty2Type>> property2)
        {
            return new PropertyConstraintFactory<TProperty1Type, TProperty2Type>(this, property1, property2);
        }

        private class PropertyConstraintFactory<TPropertyType> : IPropertyConstraintFactory<TPropertyType>
        {
            private readonly ObservableObject owner;
            private readonly string propertyName;
            private readonly Func<TPropertyType> propertyGetter;

            public PropertyConstraintFactory(ObservableObject owner, Expression<Func<TPropertyType>> property)
            {
                this.owner = owner;
                this.propertyName = property.GetPropertyName();
                this.propertyGetter = property.Compile();
            }

            public IPropertyConstraintFactory<TPropertyType> AddValidation(Expression<Func<TPropertyType, bool>> validation, Expression<Func<TPropertyType, ValidationMessage>> message)
            {
                this.owner.constraints.Add(new ExpressionPropertyConstraint<TPropertyType>(this.owner, this.propertyName, this.propertyGetter, validation, message));
                return this;
            }
        }

        private class PropertyConstraintFactory<TProperty1Type, TProperty2Type> : IPropertyConstraintFactory<TProperty1Type, TProperty2Type>
        {
            private readonly ObservableObject owner;
            private readonly string[] propertyNames;
            private readonly Func<TProperty1Type> property1Getter;
            private readonly Func<TProperty2Type> property2Getter;

            public PropertyConstraintFactory(ObservableObject owner, Expression<Func<TProperty1Type>> property1, Expression<Func<TProperty2Type>> property2)
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

            public IPropertyConstraintFactory<TProperty1Type, TProperty2Type> AddValidation(Expression<Func<TProperty1Type, TProperty2Type, bool>> validation, Expression<Func<TProperty1Type, TProperty2Type, ValidationMessage>> message)
            {
                this.owner.constraints.Add(new ExpressionPropertyConstraint<TProperty1Type, TProperty2Type>(this.owner, this.propertyNames, this.property1Getter, this.property2Getter, validation, message));
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
            private static readonly ValidationMessage emptyMessage = "";

            protected ExpressionPropertyConstraintBase(ObservableObject owner, string[] propertyNames)
            {
                this.owner = owner;
                this.propertyNames = propertyNames;
                this.owner.PropertyChanged += this.OwnerPropertyChanged;
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
                    return this.success.HasValue? this.validationMessage: null;
                }
            }

            private void SetValidationMessage(ValidationMessage value)
            {
                this.validationMessage = value;
            }

            public override string[] PropertyNames => this.propertyNames;

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
                        this.SetValidationMessage(ExpressionPropertyConstraintBase.emptyMessage);
                    }
                }
                else
                {
                    this.SetValidationMessage(ExpressionPropertyConstraintBase.emptyMessage);
                }
            }

            private void InvalidateMessage()
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

        private class ExpressionPropertyConstraint<TPropertyType> : ExpressionPropertyConstraintBase
        {
            private readonly Func<TPropertyType> propertyGetter;
            private readonly NotifyChangeExpression<Func<TPropertyType, bool>> validation;
            private readonly NotifyChangeExpression<Func<TPropertyType, ValidationMessage>> message;

            public ExpressionPropertyConstraint(ObservableObject owner, string propertyName, Func<TPropertyType> propertyGetter, Expression<Func<TPropertyType, bool>> validation, Expression<Func<TPropertyType, ValidationMessage>> message)
                : base(owner, new[] {propertyName})
            {
                this.propertyGetter = propertyGetter;
                this.validation = new NotifyChangeExpression<Func<TPropertyType, bool>>(validation);
                this.validation.Changed += this.ValidationChanged;
                this.message = new NotifyChangeExpression<Func<TPropertyType, ValidationMessage>>(message);
                this.message.Changed += this.MessageChanged;
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
                TPropertyType Value = this.propertyGetter();
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
                    TPropertyType Value = (TPropertyType) Convert.ChangeType(value, typeof (TPropertyType));
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

        private class ExpressionPropertyConstraint<TProperty1Type, TProperty2Type> : ExpressionPropertyConstraintBase
        {
            private readonly Func<TProperty1Type> property1Getter;
            private readonly Func<TProperty2Type> property2Getter;
            private readonly NotifyChangeExpression<Func<TProperty1Type, TProperty2Type, bool>> validation;
            private readonly NotifyChangeExpression<Func<TProperty1Type, TProperty2Type, ValidationMessage>> message;

            public ExpressionPropertyConstraint(
                ObservableObject owner, string[] propertyNames, Func<TProperty1Type> property1Getter, Func<TProperty2Type> property2Getter, Expression<Func<TProperty1Type, TProperty2Type, bool>> validation,
                Expression<Func<TProperty1Type, TProperty2Type, ValidationMessage>> message)
                : base(owner, propertyNames)
            {
                this.property1Getter = property1Getter;
                this.property2Getter = property2Getter;
                this.validation = new NotifyChangeExpression<Func<TProperty1Type, TProperty2Type, bool>>(validation);
                this.validation.Changed += this.ValidationChanged;
                this.message = new NotifyChangeExpression<Func<TProperty1Type, TProperty2Type, ValidationMessage>>(message);
                this.message.Changed += this.MessageChanged;
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
                TProperty1Type Value1 = this.property1Getter();
                TProperty2Type Value2 = this.property2Getter();
                return this.validation.Invoke(Value1, Value2);
            }

            protected override ValidationMessage UpdateMessageInternal()
            {
                TProperty1Type Value1 = this.property1Getter();
                TProperty2Type Value2 = this.property2Getter();
                return this.message.Invoke(Value1, Value2);
            }

            protected override string PreviewErrorsInternal(string propertyName, object value)
            {
                try
                {

                    TProperty1Type Value1 = propertyName == this.PropertyNames[0] ? Conversion.ChangeType<TProperty1Type>(value) : this.property1Getter();
                    TProperty2Type Value2 = propertyName == this.PropertyNames[1] ? Conversion.ChangeType<TProperty2Type>(value) : this.property2Getter();

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

        IEnumerable INotifyDataErrorInfo.GetErrors(string propertyName)
        {
            return new[] { this.GetValidationResult(propertyName)};
        }

        bool INotifyDataErrorInfo.HasErrors => this.HasErrors(String.Empty);

        /// <summary>
        /// Occurs when the validation errors have changed for a property or for the entire entity. 
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

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

        /// <summary>
        /// Gets all validation messages for the given property
        /// </summary>
        public IEnumerable<ValidationMessage> GetValidationMessages(string propertyName)
        {
            return (from PropertyConstraint Constraint in this.constraints
                where Constraint.PropertyNames.Contains(propertyName)
                where Constraint.ValidationMessage != null
                select Constraint.ValidationMessage).ToArray(); //ToArray() to avoid recursion on deffered execution
        }

        /// <summary>
        /// Gives an indictation whether error messages are present for the given property
        /// </summary>
        public bool HasErrors(string propertyName)
        {
            return this.GetValidationMessages(propertyName).FirstOrDefault() != null;
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
        public event EventHandler<ValidationEventArgs> ValidationChanged = delegate(object sender, ValidationEventArgs e) { ((ObservableObject) sender).InvokeErrorChanged(e.PropertyName); };

        private void InvokeErrorChanged(string propertyName)
        {
            this.ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        #endregion

        /*
        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            throw new NotImplementedException();
        }*/


        internal class PropertyValueCache
        {
            private readonly Dictionary<object,object> values = new Dictionary<object, object>();

            public CachedValue<TProperty> SetValue<TSource, TSourceProperty, TProperty>(PropertyAdapterBase<TSource, TSourceProperty> adapter, CachedValue<TProperty> value) where TSource : ObservableObject
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

            public CachedValue<TProperty> GetValue<TSource, TSourceProperty, TProperty>(PropertyAdapterBase<TSource, TSourceProperty> adapter) where TSource : ObservableObject
            {
                return (CachedValue<TProperty>) this.values[adapter];
            }

            public bool HasValue<TSource, TSourceProperty>(PropertyAdapterBase<TSource, TSourceProperty> adapter) where TSource : ObservableObject
            {
                return this.values.ContainsKey(adapter);
            }

            public void ClearValue<TSource, TSourceProperty>(PropertyAdapterBase<TSource, TSourceProperty> adapter) where TSource : ObservableObject
            {
                this.values.Remove(adapter);
            }
        }

        internal class CachedValue<T>
        {
            private readonly T value;
            // ReSharper disable once NotAccessedField.Local - reference must be stored so that it is not garbage collected
            private readonly Exception exception;

            public CachedValue(T value, ObservableExpressionFactory.EventSink eventSink)
            {
                this.value = value;
                this.EventSink = eventSink;
                this.exception = null;
            }

            public CachedValue(Exception exception, ObservableExpressionFactory.EventSink eventSink)
            {
                this.value = default(T);
                this.exception = exception;
                this.EventSink = eventSink;
            }

            public ObservableExpressionFactory.EventSink EventSink { get; }

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
                if (object.ReferenceEquals(null, obj)) return false;
                if (obj.GetType() != typeof(CachedValue<T>)) return false;
                return this.Equals((CachedValue<T>)obj);
            }

            private bool Equals(CachedValue<T> other)
            {
                if (object.ReferenceEquals(null, other)) return false;
                return object.Equals(other.value, this.value) && object.Equals(other.exception, this.exception);
            }

            public override int GetHashCode()
            {
                return (object.Equals(this.value, default(T)) ? 0 : this.value.GetHashCode() ^ (this.exception?.GetHashCode() ?? 0));
            }
        }

    }
}
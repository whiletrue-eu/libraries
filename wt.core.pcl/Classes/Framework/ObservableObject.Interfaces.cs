// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable InvokeAsExtensionMethod
// ReSharper disable UnusedMember.Global
#pragma warning disable 1574 //xmldoc
#pragma warning disable 1584,1711,1572,1581,1580 //xmldoc
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;


namespace WhileTrue.Classes.Framework
{
    partial class ObservableObject
    {
        /// <summary>
        /// Flow interface for the <see cref="ObservableObject.AddValidationForProperty{PropertyType}"/> method
        /// </summary>
        protected interface IPropertyConstraintFactory<TPropertyType>
        {
            /// <summary>
            /// Creates a validation method for the property
            /// </summary>
            /// <remarks>
            /// <para>
            /// The validation is specified by two expressions: the first one returns a boolean value whether the
            /// validation was successful (<c>true</c>) or failed (<c>false</c>). If the validation failed
            /// the message is evaluated and added for the <see cref="IDataErrorInfo"/> for that property.
            /// </para>
            /// <para>
            /// If an exception is thrown, no message is issued. You have to handle exceptional cases within the expression for yourself.
            /// </para>
            /// <para>
            /// When evaluating the expression, the current value of the property is given as an argument.
            /// This value can be chekced against arbitrary properties of this or another class by directly accessing them.
            /// You may also call methods, as long as all values are retrieved within the expression and given as parameters
            /// to these methods.
            /// </para>
            /// <para>
            /// During evaluation of the valiudation or message expression, all <see cref="INotifyPropertyChanged.PropertyChanged"/> as well
            /// as <see cref="INotifyCollectionChanged.CollectionChanged"/> events are registered. If one of the values change, the
            /// validation and/or message are reevaluated, so that the results is always kept up-to-date automatically
            /// </para>
            /// </remarks>
            IPropertyConstraintFactory<TPropertyType> AddValidation(Expression<Func<TPropertyType, bool>> validation, Expression<Func<TPropertyType, ValidationMessage>> message);
        }
        
        /// <summary>
        /// Flow interface for the <see cref="ObservableObject.AddValidationForProperty{PropertyType}"/> method
        /// </summary>
        protected interface IPropertyConstraintFactory<TProperty1Type,TProperty2Type>
        {
            /// <summary>
            /// Creates a validation method for the property
            /// </summary>
            /// <remarks>
            /// <para>
            /// The validation is specified by two expressions: the first one returns a boolean value whether the
            /// validation was successful (<c>true</c>) or failed (<c>false</c>). If the validation failed
            /// the message is evaluated and added for the <see cref="IDataErrorInfo"/> for that property.
            /// </para>
            /// <para>
            /// If an exception is thrown, no message is issued. You have to handle exceptional cases within the expression for yourself.
            /// </para>
            /// <para>
            /// When evaluating the expression, the current value of the property is given as an argument.
            /// This value can be chekced against arbitrary properties of this or another class by directly accessing them.
            /// You may also call methods, as long as all values are retrieved within the expression and given as parameters
            /// to these methods.
            /// </para>
            /// <para>
            /// During evaluation of the valiudation or message expression, all <see cref="INotifyPropertyChanged.PropertyChanged"/> as well
            /// as <see cref="INotifyCollectionChanged.CollectionChanged"/> events are registered. If one of the values change, the
            /// validation and/or message are reevaluated, so that the results is always kept up-to-date automatically
            /// </para>
            /// </remarks>
            IPropertyConstraintFactory<TProperty1Type, TProperty2Type> AddValidation(Expression<Func<TProperty1Type, TProperty2Type, bool>> validation, Expression<Func<TProperty1Type, TProperty2Type, ValidationMessage>> message);
        }
    }
}
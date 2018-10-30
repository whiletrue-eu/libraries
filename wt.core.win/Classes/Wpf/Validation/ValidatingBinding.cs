// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Threading;
using JetBrains.Annotations;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Wpf
{
    /// <summary>
    ///     The ValidationBinding class wraps a normal binding, but setting the standard validation
    ///     classes, and additionally adds support for the <see cref="IObjectValidation" /> interface
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The <see cref="IObjectValidation" /> enables validation of values on TextBoxes while typing
    ///         to get an immediate feedback on error conditions.
    ///     </para>
    ///     <para>
    ///         Otherwise, the binding acts like a standard Binding where the <c>ValidatesOnDataErrors</c> and
    ///         <c>ValidatesOnExceptions</c> properties are set to <c>true</c>.
    ///     </para>
    /// </remarks>
    [PublicAPI]
    public class ValidatingBinding : MarkupExtension
    {
        /// <summary />
        public ValidatingBinding()
        {
        }

        /// <summary />
        public ValidatingBinding(string path)
        {
            Path = path;
        }

        /// <summary>
        ///     Path to the property. <see cref="Binding.Path" />
        /// </summary>
        [ConstructorArgument("path")]
        public string Path { get; set; }

        // ReSharper disable UnusedAutoPropertyAccessor.Global
        /// <summary>
        ///     Gets or sets a value that determines the timing of binding source updates.
        /// </summary>
        public UpdateSourceTrigger UpdateSourceTrigger { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global

        /// <summary />
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var Target = (IProvideValueTarget) serviceProvider.GetService(typeof(IProvideValueTarget));

            if (Target.TargetObject is DependencyObject)
            {
                var Binding = Path != null ? new Binding(Path) : new Binding();
                Binding.UpdateSourceTrigger = UpdateSourceTrigger;

                var ObjectValidationRule = new ObjectValidationRule((DependencyObject) Target.TargetObject,
                    Target.TargetProperty as DependencyProperty, UpdateSourceTrigger, Binding);
                Binding.ValidationRules.Add(ObjectValidationRule);

                var Expression = (BindingExpression) Binding.ProvideValue(serviceProvider);
                ObjectValidationRule.BindingExpression = Expression;
                return Expression;
            }

            //This is done e.g. in case of control/data templates. Then the ProvideValue will be called later for each instance of the template seperately
            return this;
        }


        private class ObjectValidationRule : ValidationRule
        {
            private readonly DependencyObject dependencyObject;
            private readonly DependencyProperty dependencyProperty;
            private IDataErrorInfo dataErrorSourceItem;

            private INotifyDataErrorInfo notifyDataErrorSourceItem;
            private string sourcePropertyName;

            public ObjectValidationRule(DependencyObject dependencyObject, DependencyProperty dependencyProperty,
                UpdateSourceTrigger updateSourceTrigger, Binding binding)
            {
                ValidationStep = ValidationStep.UpdatedValue;
                ValidatesOnTargetUpdated = true;

                this.dependencyObject = dependencyObject;
                this.dependencyProperty = dependencyProperty;

                //Set notification if target is changed (i.e. WPF control is updated with new value) to be able to re-register to validation change notifications
                binding.NotifyOnTargetUpdated = true;
                binding.NotifyOnValidationError = true;
                Binding.AddTargetUpdatedHandler(dependencyObject, TargetUpdated);

                //Register for eception handling
                binding.UpdateSourceExceptionFilter = HandleSourceUpdateException;


                //Special handling for text boxes; only apply if not validated on every change
                if (updateSourceTrigger != UpdateSourceTrigger.PropertyChanged && dependencyObject is TextBoxBase)
                {
                    var TextBox = (TextBoxBase) dependencyObject;
                    TextBox.TextChanged += HandleTextChanged;
                }

                //Special handling to work around the error template disappearing
                if (this.dependencyObject is UIElement)
                    ((UIElement) this.dependencyObject).IsVisibleChanged += ObjectValidationRule_IsVisibleChanged;
                if (this.dependencyObject is UIElement3D)
                    ((UIElement3D) this.dependencyObject).IsVisibleChanged += ObjectValidationRule_IsVisibleChanged;
            }

            public BindingExpression BindingExpression { private get; set; }

            private void ObjectValidationRule_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                ScheduleValidationResultRefresh();
            }

            private static object HandleSourceUpdateException(object bindexpression, Exception exception)
            {
                return new ValidationResult(false, exception.Message);
            }

            private void HandleTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
            {
                var Result = Validate(BindingExpression, ((TextBox) sender).Text);
                UpdateValidationResult(Result);
            }

            private void UpdateValidationResult(ValidationResult result)
            {
                if (result.IsValid == false)
                    Validation.MarkInvalid(BindingExpression,
                        new ValidationError(this, BindingExpression, result.ErrorContent, null));
                else
                    Validation.ClearInvalid(BindingExpression);
            }

            private void TargetUpdated(object sender, DataTransferEventArgs e)
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (e.TargetObject == dependencyObject && e.Property == dependencyProperty)
                    ReRegisterValidationChangedEvent();
            }

            private void ReRegisterValidationChangedEvent()
            {
                UnregisterValidationChangedEvent();
                RegisterValidationChangedEvent();

                var Result = Validate();
                UpdateValidationResult(Result);
            }

            private void RegisterValidationChangedEvent()
            {
                var SourceItem = BindingExpression.PrivateMembers().GetProperty<object>("SourceItem");
                notifyDataErrorSourceItem = SourceItem as INotifyDataErrorInfo;
                dataErrorSourceItem = SourceItem as IDataErrorInfo;
                sourcePropertyName = BindingExpression.PrivateMembers().GetProperty<string>("SourcePropertyName");

                if (notifyDataErrorSourceItem != null) notifyDataErrorSourceItem.ErrorsChanged += ErrorsChanged;
            }

            private void ErrorsChanged(object sender, DataErrorsChangedEventArgs dataErrorsChangedEventArgs)
            {
                if (dataErrorsChangedEventArgs.PropertyName == sourcePropertyName)
                {
                    var Result = Validate();
                    UpdateValidationResult(Result);
                }
            }

            private void UnregisterValidationChangedEvent()
            {
                if (notifyDataErrorSourceItem != null) notifyDataErrorSourceItem.ErrorsChanged -= ErrorsChanged;
            }

            #region Overrides of ValidationRule

            public override ValidationResult Validate(object value, CultureInfo cultureInfo)
            {
                var Binding = value as BindingExpression;
                if (Binding != null)
                    return Validate(Binding);
                return ValidationResult.ValidResult;
            }

            private ValidationResult Validate(BindingExpression bindingExpression)
            {
                bindingExpression.DbC_Assure(binding => binding == BindingExpression);

                return Validate();
            }

            private ValidationResult Validate()
            {
                var DataErrors = notifyDataErrorSourceItem?.GetErrors(sourcePropertyName) ??
                                 dataErrorSourceItem?[sourcePropertyName];
                var Errors = DataErrors?.Cast<object>().Select(_ => (ValidationMessage) _.ToString()).ToArray();
                return ProcessValidationResults(Errors);
            }

            private ValidationResult ProcessValidationResults(ValidationMessage[] errors)
            {
                // Use internal method implemented for INotifyDataErrorInfo handling in BindingExpressionBase
                BindingExpression.PrivateMembers().Call("UpdateNotifyDataErrorValidationErrors",
                    new List<object>(errors ?? new object[0]));
                return ValidationResult.ValidResult;
            }

            private ValidationResult Validate(BindingExpression bindingExpression, object value)
            {
                bindingExpression.DbC_Assure(binding => binding == BindingExpression);

                var ObjectValidation = notifyDataErrorSourceItem as IObjectValidation;
                if (ObjectValidation != null)
                {
                    var Errors = ObjectValidation.PreviewErrors(sourcePropertyName, value).ToArray();
                    return ProcessValidationResults(Errors);
                }

                return ValidationResult.ValidResult;
            }

            /// <summary>
            ///     Work around framework issue:
            ///     http://connect.microsoft.com/VisualStudio/feedback/details/295933/tabcontrol-doesnt-display-validation-error-information-correctly-when-switching-tabs-back-and-forth
            /// </summary>
            private void ScheduleValidationResultRefresh()
            {
                if (Validation.GetHasError(dependencyObject))
                {
                    var ErrorTarget = dependencyObject;
                    var CurrentErrorTemplate = ErrorTarget.ReadLocalValue(Validation.ErrorTemplateProperty);
                    ErrorTarget.Dispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        (Action) delegate
                        {
                            ErrorTarget.SetValue(Validation.ErrorTemplateProperty, new ControlTemplate());
                        });
                    ErrorTarget.Dispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        (Action) delegate
                        {
                            ErrorTarget.SetValue(Validation.ErrorTemplateProperty, CurrentErrorTemplate);
                        });
                }
            }

            #endregion
        }
    }
}
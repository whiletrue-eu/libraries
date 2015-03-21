// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.ComponentModel;
using System.Globalization;
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
    ///<summary>
    /// The ValidationBinding class wraps a normal binding, but setting the standard validation
    /// classes, and additionally adds support for the <see cref="IObjectValidation"/> interface
    ///</summary>
    /// <remarks>
    /// <para>
    /// The <see cref="IObjectValidation"/> enables validation of values on TextBoxes while typing 
    /// to get an immediate feedback on error conditions.
    /// </para>
    /// <para>
    /// Otherwise, the binding acts like a standard Binding where the <c>ValidatesOnDataErrors</c> and
    /// <c>ValidatesOnExceptions</c> properties are set to <c>true</c>.
    /// </para>
    /// </remarks>
    [PublicAPI]
    public class ValidatingBinding : MarkupExtension
    {
        ///<summary/>
        public ValidatingBinding()
        {
        }

        ///<summary/>
        public ValidatingBinding(string path)
        {
            this.Path = path;
        }

        /// <summary>
        /// Path to the property. <see cref="Binding.Path"/>
        /// </summary>
        [ConstructorArgument("path")]
        public string Path { get; set; }

        ///<summary/>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            IProvideValueTarget Target = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));

            if (Target.TargetObject is DependencyObject)
            {
                Binding Binding = this.Path!=null?new Binding(this.Path):new Binding();
                Binding.UpdateSourceTrigger = this.UpdateSourceTrigger;

                ObjectValidationRule ObjectValidationRule = new ObjectValidationRule((DependencyObject) Target.TargetObject, Target.TargetProperty as DependencyProperty, this.UpdateSourceTrigger, Binding);
                Binding.ValidationRules.Add(ObjectValidationRule);

                BindingExpression Expression = (BindingExpression) Binding.ProvideValue(serviceProvider);
                ObjectValidationRule.BindingExpression = Expression;
                return Expression;
            }
            else
            {
                //This is done e.g. in case of control/data templates. Then the ProvideValue will be called later for each instance of the template seperately
                return this;
            }
        }

        // ReSharper disable UnusedAutoPropertyAccessor.Global
        /// <summary>
        /// Gets or sets a value that determines the timing of binding source updates.
        /// </summary>
        public UpdateSourceTrigger UpdateSourceTrigger { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global


        private class ObjectValidationRule:ValidationRule
        {
            public ObjectValidationRule(DependencyObject dependencyObject, DependencyProperty dependencyProperty, UpdateSourceTrigger updateSourceTrigger, Binding binding)
            {
                this.ValidationStep = ValidationStep.UpdatedValue;
                this.ValidatesOnTargetUpdated = true;

                this.dependencyObject = dependencyObject;
                this.dependencyProperty = dependencyProperty;

                //Set notification if target is changed (i.e. WPF control is updated with new value) to be able to re-register to validation change notifications
                binding.NotifyOnTargetUpdated = true;
                Binding.AddTargetUpdatedHandler(dependencyObject, this.TargetUpdated);

                //Register for eception handling
                binding.UpdateSourceExceptionFilter = ObjectValidationRule.HandleSourceUpdateException;


                //Special handling for text boxes; only apply if not validated on every change
                if (updateSourceTrigger != UpdateSourceTrigger.PropertyChanged && dependencyObject is TextBoxBase)
                {
                    TextBoxBase TextBox = (TextBoxBase)dependencyObject;
                    TextBox.TextChanged += this.HandleTextChanged;
                }

                //Special handling to work around the error template disappearing
                if (this.dependencyObject is UIElement)
                {
                    ((UIElement) this.dependencyObject).IsVisibleChanged += this.ObjectValidationRule_IsVisibleChanged;
                }
                if (this.dependencyObject is UIElement3D)
                {
                    ((UIElement3D) this.dependencyObject).IsVisibleChanged += this.ObjectValidationRule_IsVisibleChanged;
                }
            }

            private void ObjectValidationRule_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                this.ScheduleValidationResultRefresh();
            }

            private readonly DependencyObject dependencyObject;
            private readonly DependencyProperty dependencyProperty;
            public BindingExpression BindingExpression { private get; set; }

            private INotifyDataErrorInfo sourceItem;
            private string sourcePropertyName;

            #region Overrides of ValidationRule

            public override ValidationResult Validate(object value, CultureInfo cultureInfo)
            {
                BindingExpression Binding = value as BindingExpression;
                if (Binding != null)
                {
                return this.Validate(Binding);
                }
                else
                {
                    return ValidationResult.ValidResult;
                }
            }

            private ValidationResult Validate(BindingExpression bindingExpression)
            {
                bindingExpression.DbC_Assure(binding => binding==this.BindingExpression);

                return this.Validate();
            }

            private ValidationResult Validate()
            {
                if (this.sourceItem != null)
                {
                    string Error = string.Join("\n", this.sourceItem.GetErrors(this.sourcePropertyName));
                    if (string.IsNullOrEmpty(Error) == false)
                    {
                        return new ValidationResult(false, Error);
                    }
                    else
                    {
                        return ValidationResult.ValidResult;
                    }
                }
                else
                {
                    return ValidationResult.ValidResult;
                }
            }

            private ValidationResult Validate(BindingExpression bindingExpression, object value)
            {
                bindingExpression.DbC_Assure(binding => binding == this.BindingExpression);

                IObjectValidation ObjectValidation = this.sourceItem as IObjectValidation;
                if (ObjectValidation != null)
                {
                    string Error = ObjectValidation.PreviewErrors(this.sourcePropertyName,value);
                    if (string.IsNullOrEmpty(Error) == false)
                    {
                        return new ValidationResult(false, Error);
                    }
                    else
                    {
                        return ValidationResult.ValidResult;
                    }
                }
                else
                {
                    return ValidationResult.ValidResult;
                }
            }

            /// <summary>
            /// Work around framework issue:
            /// http://connect.microsoft.com/VisualStudio/feedback/details/295933/tabcontrol-doesnt-display-validation-error-information-correctly-when-switching-tabs-back-and-forth
            /// </summary>
            private void ScheduleValidationResultRefresh()
            {
                if (Validation.GetHasError(this.dependencyObject))
                {
                    DependencyObject ErrorTarget = this.dependencyObject;
                    object CurrentErrorTemplate = ErrorTarget.ReadLocalValue(Validation.ErrorTemplateProperty);
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

            private static object HandleSourceUpdateException(object bindexpression, Exception exception)
            {
                return new ValidationResult(false, exception.Message);
            }

            private void HandleTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
            {
                ValidationResult Result = this.Validate(this.BindingExpression, ((TextBox)sender).Text);
                this.UpdateValidationResult(Result);
            }

            private void UpdateValidationResult(ValidationResult result)
            {
                if (result.IsValid == false)
                {
                    Validation.MarkInvalid(this.BindingExpression, new ValidationError(this,this.BindingExpression, result.ErrorContent, null));
                }
                else
                {
                    Validation.ClearInvalid(this.BindingExpression);
                }
            }

            private void TargetUpdated(object sender, DataTransferEventArgs e)
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (e.TargetObject == this.dependencyObject && e.Property == this.dependencyProperty)
                {
                    this.ReRegisterValidationChangedEvent();
                }

            }

            private void ReRegisterValidationChangedEvent()
            {
                this.UnregisterValidationChangedEvent();
                this.RegisterValidationChangedEvent();

                ValidationResult Result = this.Validate();
                this.UpdateValidationResult(Result);
            }

            private void RegisterValidationChangedEvent()
            {
                this.sourceItem = this.BindingExpression.PrivateMembers().GetProperty<object>("SourceItem") as INotifyDataErrorInfo;
                this.sourcePropertyName = this.BindingExpression.PrivateMembers().GetProperty<string>("SourcePropertyName");

                IObjectValidation ObjectValidation = this.sourceItem as IObjectValidation;
                if (ObjectValidation != null)
                {
                    ObjectValidation.ValidationChanged += this.SourceValidationChanged;
                }
            }

            private void SourceValidationChanged(object sender, ValidationEventArgs validationEventArgs)
            {
                if (validationEventArgs.PropertyName == this.sourcePropertyName)
                {
                    ValidationResult Result = this.Validate(); 
                    this.UpdateValidationResult(Result);
                }
            }

            private void UnregisterValidationChangedEvent()
            {
                IObjectValidation ObjectValidation = this.sourceItem as IObjectValidation;
                if (ObjectValidation != null)
                {
                    ObjectValidation.ValidationChanged -= this.SourceValidationChanged;
                }
            }
        }
    }
}

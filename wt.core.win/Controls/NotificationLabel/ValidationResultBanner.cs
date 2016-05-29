using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using JetBrains.Annotations;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls
{
    /// <summary>
    /// This control can be stacked with another control, that may receive errors to visualize these errors.
    /// The control is designed as a pop-out in the standard template and supports severities in case <see cref="ValidationMessage"/> style messages are shown
    /// </summary>
    [PublicAPI]
    public class ValidationResultBanner : Control
    {
        /// <summary>
        /// Bind to the error collection of the stacked control using <see cref="Validation.ErrorsProperty"/>
        /// </summary>
        public static readonly DependencyProperty ValidationResultsProperty;

        static ValidationResultBanner()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ValidationResultBanner), new FrameworkPropertyMetadata(typeof(ValidationResultBanner),FrameworkPropertyMetadataOptions.AffectsArrange));

            ValidationResultBanner.ValidationResultsProperty = DependencyProperty.Register(
                "ValidationResults",
                typeof(ReadOnlyObservableCollection<ValidationError>),
                typeof(ValidationResultBanner),
                new PropertyMetadata(ValidationResultBanner.Callback));
        }

        private static void Callback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            
        }

        /// <summary>
        /// Bind to the error collection of the stacked control using <see cref="Validation.ErrorsProperty"/>
        /// </summary>
        public ReadOnlyObservableCollection<ValidationError> ValidationResults
        {
            get { return (ReadOnlyObservableCollection<ValidationError>)this.GetValue(ValidationResultBanner.ValidationResultsProperty); }
            set { this.SetValue(ValidationResultBanner.ValidationResultsProperty, value); }
        }
    }
}
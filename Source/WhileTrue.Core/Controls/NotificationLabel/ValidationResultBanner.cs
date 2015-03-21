using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace WhileTrue.Controls
{
    public class ValidationResultBanner : Control
    {
        public static readonly DependencyProperty ValidationResultsProperty;

        static ValidationResultBanner()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ValidationResultBanner), new FrameworkPropertyMetadata(typeof(ValidationResultBanner),FrameworkPropertyMetadataOptions.AffectsArrange));

            ValidationResultsProperty = DependencyProperty.Register(
                "ValidationResults",
                typeof(ReadOnlyObservableCollection<ValidationError>),
                typeof(ValidationResultBanner),
                new PropertyMetadata(callback));
        }

        private static void callback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            
        }

        public ReadOnlyObservableCollection<ValidationError> ValidationResults
        {
            get { return (ReadOnlyObservableCollection<ValidationError>)this.GetValue(ValidationResultsProperty); }
            set { this.SetValue(ValidationResultsProperty, value); }
        }
    }
}
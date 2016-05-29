using System;
using System.Diagnostics;
using System.Windows;

namespace WhileTrue.Controls
{
    /// <summary>
    /// Assists setting the initial keyboard focus to the control with the given name
    /// </summary>
    /// <remarks>
    /// Simply set the <see cref="InitialKeyboardFocusProperty">InitialKeyboardFocus</see> attached property on
    /// the window to a string containing the name of a control inside the window.<br/>
    /// After loading of the window, the control with the given name will be focused automatically.
    /// </remarks>
    /// <exception cref="InvalidOperationException">If attached property is set on a dependency object that is not a window</exception>
    public class FocusHelper
    {
        /// <summary/>
        public static readonly DependencyProperty InitialKeyboardFocusProperty;

        static FocusHelper()
        {
            FocusHelper.InitialKeyboardFocusProperty = DependencyProperty.RegisterAttached(
                "InitialKeyboardFocus",
                typeof (string),
                typeof (FocusHelper),
                new FrameworkPropertyMetadata(
                    null, new PropertyChangedCallback(FocusHelper.InitialKeyboardFocusPropertyChanged)
                    )
                );
        }

        private static void InitialKeyboardFocusPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is FrameworkElement)
            {
                ((FrameworkElement)dependencyObject).AddHandler(FrameworkElement.LoadedEvent, new RoutedEventHandler(FocusHelper.WindowLoaded), true);
            }
            else
            {
                throw new InvalidOperationException("InitialKeyboardFocusProperty can only be used on FrameworkElements");
            }
        }

        private static void WindowLoaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement FrameworkElement = (FrameworkElement) e.Source;
            string ElementName = FocusHelper.GetInitialKeyboardFocus(FrameworkElement);
            UIElement Element = FrameworkElement.FindName(ElementName) as UIElement;
            if (Element != null)
            {
                Element.Focus();
            }
            else
            {
                Trace.WriteLine($"FocusHelper: Element with name '{ElementName}' not found in parent '{Element}'.");
            }
        }

        /// <summary>
        /// Sets the name of the control that shall receive the initial keyboard focus
        /// </summary>
        public static void SetInitialKeyboardFocus(DependencyObject dependencyObject, string element)
        {
            dependencyObject.SetValue(FocusHelper.InitialKeyboardFocusProperty, element);
        }

        /// <summary>
        /// Gets the name of the control that shall receive the initial keyboard focus
        /// </summary>
        public static string GetInitialKeyboardFocus(DependencyObject dependencyObject)
        {
            return (string) dependencyObject.GetValue(FocusHelper.InitialKeyboardFocusProperty);
        }
    }
}
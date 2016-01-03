using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using JetBrains.Annotations;

namespace WhileTrue.Classes.Wpf
{
    /// <summary>
    /// This class provides a thread-safe wrapper for a observable collection
    /// </summary>
    /// <remarks>
    /// The original collection is wrapped to provide event handling inside the thread the wrapper was created in (normally the GUI thread).
    /// The collection content is cached to provide a consistent view onto the collection, even if it is currently modified in another thread.
    /// </remarks>
    [PublicAPI]
    public class CrossThreadCollectionWrapper : DependencyObject,IValueConverter
    {
        /// <summary>
        /// specifies wether fade in/out animations for items are enabled for the items control
        /// </summary>
        public static readonly DependencyProperty EnableItemFadeAnimationsProperty = DependencyProperty.RegisterAttached("EnableItemFadeAnimations", typeof(bool), typeof(CrossThreadCollectionWrapper), new PropertyMetadata(false, CrossThreadCollectionWrapper.EnableItemFadeAnimationsChanged));
        /// <summary>
        /// specifies the storyboard for the fade in animation
        /// </summary>
        public static readonly DependencyProperty FadeInAnimationProperty = DependencyProperty.RegisterAttached("FadeInAnimation", typeof(Storyboard), typeof(CrossThreadCollectionWrapper));
        /// <summary>
        /// specifies the storyboard for the fade out animation
        /// </summary>
        public static readonly DependencyProperty FadeOutAnimationProperty = DependencyProperty.RegisterAttached("FadeOutAnimation", typeof(Storyboard), typeof(CrossThreadCollectionWrapper));

        private static void EnableItemFadeAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ItemsControl ItemsControl = (ItemsControl) d;
            CollectionWrapper.RegisteredControls.Add(ItemsControl);
        }

        /// <summary/>
        public CrossThreadCollectionWrapper()
        {
            this.ShareCollectionPerThread = true;
        }

        #region IValueConverter Members

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.</param><param name="targetType">The type of the binding target property.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object Collection = value;
            if (Collection == null)
            {
                return null;
            }
            else if (Collection is IEnumerable && Collection is INotifyCollectionChanged)
            {
                return CollectionWrapper.GetCollectionWrapperInstance((IEnumerable) Collection,this.ShareCollectionPerThread);
            }
            else
            {
                throw new InvalidOperationException("Collection to wrap is either not enumerable or does not support INotifyCollectionChanged");
            }
        }

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value that is produced by the binding target.</param><param name="targetType">The type to convert to.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// Gets/sets whether one collection is created for each thread that is used as a proxy between the original collection and the UI, or one collection is created per control.
        /// As enabling item animation changes the behaviour of the collectiton (e.g. items are only removed from the collection after the fade-out animation played) it may not be desired
        /// to share the collection between different controls
        /// </summary>
        public bool ShareCollectionPerThread { get; set; }

        /// <summary>
        /// specifies wether fade in/out animations for items are enabled for the items control
        /// </summary>
        public static bool GetEnableItemFadeAnimations(UIElement element)
        {
            return (bool)element.GetValue(CrossThreadCollectionWrapper.EnableItemFadeAnimationsProperty);
        }

        /// <summary>
        /// specifies wether fade in/out animations for items are enabled for the items control
        /// </summary>
        public static void SetEnableItemFadeAnimations(UIElement element, bool value)
        {
            element.SetValue(CrossThreadCollectionWrapper.EnableItemFadeAnimationsProperty, value);
        }

        /// <summary>
        /// specifies the storyboard for the fade in animation
        /// </summary>
        public static Storyboard GetFadeInAnimation(UIElement element)
        {
            return (Storyboard)element.GetValue(CrossThreadCollectionWrapper.FadeInAnimationProperty);
        }

        /// <summary>
        /// specifies the storyboard for the fade in animation
        /// </summary>
        public static void SetFadeInAnimation(UIElement element, Storyboard value)
        {
            element.SetValue(CrossThreadCollectionWrapper.FadeInAnimationProperty, value);
        }

        /// <summary>
        /// specifies the storyboard for the fade out animation
        /// </summary>
        public static Storyboard GetFadeOutAnimation(UIElement element)
        {
            return (Storyboard)element.GetValue(CrossThreadCollectionWrapper.FadeOutAnimationProperty);
        }

        /// <summary>
        /// specifies the storyboard for the fade out animation
        /// </summary>
        public static void SetFadeOutAnimation(UIElement element, Storyboard value)
        {
            element.SetValue(CrossThreadCollectionWrapper.FadeOutAnimationProperty, value);
        }
    }
}
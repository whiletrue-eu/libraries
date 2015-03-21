using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace WhileTrue.Classes.Wpf
{
    /// <summary>
    /// This class provides a thread-safe wrapper for a observable collection
    /// </summary>
    /// <remarks>
    /// The original collection is wrapped to provide event handling inside the thread the wrapper was created in (normally the GUI thread).
    /// The collection content is cached to provide a consistent view onto the collection, even if it is currently modified in another thread.
    /// </remarks>
    public class CrossThreadCollectionWrapper : DependencyObject,IValueConverter
    {
        public static DependencyProperty EnableItemFadeAnimationsProperty = DependencyProperty.RegisterAttached("EnableItemFadeAnimations", typeof(bool), typeof(CrossThreadCollectionWrapper), new PropertyMetadata(false, EnableItemFadeAnimationsChanged));
        public static DependencyProperty FadeInAnimationProperty = DependencyProperty.RegisterAttached("FadeInAnimation", typeof(Storyboard), typeof(CrossThreadCollectionWrapper));
        public static DependencyProperty FadeOutAnimationProperty = DependencyProperty.RegisterAttached("FadeOutAnimation", typeof(Storyboard), typeof(CrossThreadCollectionWrapper));

        private static void EnableItemFadeAnimationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ItemsControl ItemsControl = (ItemsControl) d;
            CollectionWrapper.registeredControls.Add(ItemsControl);
        }

        public CrossThreadCollectionWrapper()
        {
            this.ShareCollectionPerThread = true;
        }

        #region IValueConverter Members

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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        public bool ShareCollectionPerThread { get; set; }

        public static bool GetEnableItemFadeAnimations(UIElement element)
        {
            return (bool)element.GetValue(EnableItemFadeAnimationsProperty);
        }

        public static void SetEnableItemFadeAnimations(UIElement element, bool value)
        {
            element.SetValue(EnableItemFadeAnimationsProperty, value);
        }

        public static Storyboard GetFadeInAnimation(UIElement element)
        {
            return (Storyboard)element.GetValue(FadeInAnimationProperty);
        }

        public static void SetFadeInAnimation(UIElement element, Storyboard value)
        {
            element.SetValue(FadeInAnimationProperty, value);
        }

        public static Storyboard GetFadeOutAnimation(UIElement element)
        {
            return (Storyboard)element.GetValue(FadeOutAnimationProperty);
        }

        public static void SetFadeOutAnimation(UIElement element, Storyboard value)
        {
            element.SetValue(FadeOutAnimationProperty, value);
        }
    }
}
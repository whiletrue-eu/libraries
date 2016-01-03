// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls
{
    ///<summary>
    /// Provides progress indication for a window or part of a window
    ///</summary>
    /// <remarks>
    /// <para>
    /// To use the progress indication, simply wrap the part of the window you want to provide progress for within 
    /// this progress control. You can control the style of the progress by setting the <see cref="Styling"/> property to
    /// <c>"Window"</c>, <c>"ControlGroup"</c> or <c>"Control"</c>. For more advanced options, you can also set the 
    /// <see cref="Control.Template"/> property.
    /// </para>
    /// <para>
    /// Using the <see cref="StatusContent"/> and <see cref="Icon"/> properties, you can customize the progress.
    /// </para>
    /// </remarks>
    public class ProgressControl : ContentControl
    {
        ///<summary/>
        public static readonly DependencyProperty ProgressProperty;
        ///<summary/>
        public static readonly DependencyProperty StylingProperty;
        ///<summary/>
        public static readonly DependencyProperty StatusContentProperty;
        ///<summary/>
        public static readonly DependencyProperty IconProperty;

        static ProgressControl()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ProgressControl), new FrameworkPropertyMetadata(typeof(ProgressControl)));

            ProgressControl.ProgressProperty = DependencyProperty.Register(
                "Progress",
                typeof(Progress),
                typeof(ProgressControl),
                new FrameworkPropertyMetadata(
                    null, ProgressControl.ProgressChanged)
                );

            ProgressControl.StylingProperty = DependencyProperty.Register(
                "Styling",
                typeof(string),
                typeof(ProgressControl),
                new FrameworkPropertyMetadata(
                    null
                    )
                );

            ProgressControl.StatusContentProperty = DependencyProperty.Register(
                "StatusContent",
                typeof(object),
                typeof(ProgressControl),
                new FrameworkPropertyMetadata(
                    null
                    )
                );

            ProgressControl.IconProperty = DependencyProperty.Register(
                "Icon",
                typeof(ImageSource),
                typeof(ProgressControl),
                new FrameworkPropertyMetadata(
                    null
                    )
                );
        }

        private static void ProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //Workaround for IsEnabled to not work correctly when Command state sets children of the progress control while progress is shown (thus content root is set to IsEnabled=false)
            ProgressControl ProgressControl = (ProgressControl) d;
            if( e.NewValue == null )
            {
                //Progress is hidden. Template sets the ISEnabled back, we have to refresh the Buttons & Hypelink states (see issue description above)
                UIElement Content = ProgressControl.Template.FindName("Content", ProgressControl) as UIElement;
                Content?.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action) delegate { ProgressControl.Update(Content); });
            }
            else
            {
                //Progress is shown. Template will handle
            }
        }

        private static void Update(UIElement element)
        {
            foreach (DependencyObject Child in element.GetVisualDescendantsDepthFirst<DependencyObject>())
            {
                if (Child is TextBlock)
                {
                    foreach (Inline Inline in ((TextBlock) Child).Inlines)
                    {
                        if( Inline is InlineUIContainer )
                        {
                            ProgressControl.Update(((InlineUIContainer) Inline).Child);
                        }

                        Inline.InvalidateProperty(UIElement.IsEnabledProperty);
                    }
                }
                Child.InvalidateProperty(UIElement.IsEnabledProperty);
            }
        }

        ///<summary>
        /// Sets the <see cref="WhileTrue.Controls.Progress"/> to be displayed. The progress will be shown once
        /// this property is set to another value than <c>null</c>.
        ///</summary>
        public Progress Progress
        {
            get { return (Progress) this.GetValue(ProgressControl.ProgressProperty); }
            set { this.SetValue(ProgressControl.ProgressProperty, value); }
        }

        /// <summary>
        /// Sets the styling of the control, which are effectively in-built control templates
        /// </summary>
        /// <remarks>
        /// Possible values are <c>"Window"</c>, <c>"ControlGroup"</c> or <c>"Control"</c>
        /// </remarks>
        public string Styling
        {
            get { return (string)this.GetValue(ProgressControl.StylingProperty); }
            set { this.SetValue(ProgressControl.StylingProperty, value); }
        }

        /// <summary>
        /// Additional content in the progress display
        /// </summary>
        public object StatusContent
        {
            get { return this.GetValue(ProgressControl.StatusContentProperty); }
            set { this.SetValue(ProgressControl.StatusContentProperty, value); }
        }

        /// <summary>
        /// Icon that is shown in the progress
        /// </summary>
        public ImageSource Icon
        {
            get { return (ImageSource) this.GetValue(ProgressControl.IconProperty); }
            set { this.SetValue(ProgressControl.IconProperty, value); }
        }
    }
}
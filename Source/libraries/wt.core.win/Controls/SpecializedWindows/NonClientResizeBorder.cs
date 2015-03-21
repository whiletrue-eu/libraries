using System.Windows;
using System.Windows.Controls;
using JetBrains.Annotations;

namespace WhileTrue.Controls
{
    ///<summary>
    /// Helper to realize a resizing border for windows that draw into the non-client area
    ///</summary>
    [PublicAPI]
    public class NonClientResizeBorder : ContentControl
    {
        /// <summary/>
        public static readonly DependencyProperty BorderHeightProperty;
        /// <summary/>
        public static readonly DependencyProperty BorderWidthProperty;
        /// <summary/>
        public static readonly DependencyProperty ResizeActivatedProperty;

        /// <summary/>
        static NonClientResizeBorder()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(NonClientResizeBorder), new FrameworkPropertyMetadata(typeof(NonClientResizeBorder)));

            NonClientResizeBorder.BorderHeightProperty = DependencyProperty.Register(
                "BorderHeight",
                typeof(double),
                typeof(NonClientResizeBorder),
                new FrameworkPropertyMetadata(0d)
                );

            NonClientResizeBorder.BorderWidthProperty = DependencyProperty.Register(
                "BorderWidth",
                typeof(double),
                typeof(NonClientResizeBorder),
                new FrameworkPropertyMetadata(0d)
                );

            NonClientResizeBorder.ResizeActivatedProperty = DependencyProperty.Register(
                "ResizeActivated",
                typeof(bool),
                typeof(NonClientResizeBorder),
                new FrameworkPropertyMetadata(false)
                );
        }

        /// <summary>
        /// Height of the border top/bottom
        /// </summary>
        public double BorderHeight
        {
            get { return (double) this.GetValue(NonClientResizeBorder.BorderHeightProperty); }
            set { this.SetValue(NonClientResizeBorder.BorderHeightProperty, value); }
        }

        /// <summary>
        /// width of the border left/right
        /// </summary>
        public double BorderWidth
        {
            get { return (double) this.GetValue(NonClientResizeBorder.BorderWidthProperty); }
            set { this.SetValue(NonClientResizeBorder.BorderWidthProperty, value); }
        }

        /// <summary>
        /// Allow resizing of the window (activates resizing behaviourr when mouse hovers over the Nonclient border
        /// </summary>
        public bool ResizeActivated
        {
            get { return (bool)this.GetValue(NonClientResizeBorder.ResizeActivatedProperty); }
            set { this.SetValue(NonClientResizeBorder.ResizeActivatedProperty, value); }
        }
    }
}
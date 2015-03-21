using System.Windows;
using System.Windows.Controls;

namespace WhileTrue.Controls
{
    ///<summary>
    /// Helper to realize a resizing border for windows that draw into the non-client area
    ///</summary>
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
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NonClientResizeBorder), new FrameworkPropertyMetadata(typeof(NonClientResizeBorder)));

            BorderHeightProperty = DependencyProperty.Register(
                "BorderHeight",
                typeof(double),
                typeof(NonClientResizeBorder),
                new FrameworkPropertyMetadata(0d)
                );

            BorderWidthProperty = DependencyProperty.Register(
                "BorderWidth",
                typeof(double),
                typeof(NonClientResizeBorder),
                new FrameworkPropertyMetadata(0d)
                );

            ResizeActivatedProperty = DependencyProperty.Register(
                "ResizeActivated",
                typeof(bool),
                typeof(NonClientResizeBorder),
                new FrameworkPropertyMetadata(false)
                );
        }

        public double BorderHeight
        {
            get { return (double) this.GetValue(BorderHeightProperty); }
            set { this.SetValue(BorderHeightProperty, value); }
        }

        public double BorderWidth
        {
            get { return (double) this.GetValue(BorderWidthProperty); }
            set { this.SetValue(BorderWidthProperty, value); }
        }

        public bool ResizeActivated
        {
            get { return (bool)this.GetValue(ResizeActivatedProperty); }
            set { this.SetValue(ResizeActivatedProperty, value); }
        }
    }
}
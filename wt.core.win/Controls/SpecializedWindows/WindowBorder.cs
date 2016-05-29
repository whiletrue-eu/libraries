// ReSharper disable MemberCanBePrivate.Global
using System.Windows;
using System.Windows.Controls;

namespace WhileTrue.Controls
{
    ///<summary>
    /// Helper to realize a resizing border for windows that draw into the non-client area
    ///</summary>
    public class WindowBorder : HeaderedContentControl
    {
        /// <summary/>
        public static readonly DependencyProperty ResizeActivatedProperty;
        /// <summary/>
        public static readonly DependencyProperty NonClientControlsProperty;

        /// <summary/>
        static WindowBorder()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowBorder), new FrameworkPropertyMetadata(typeof(WindowBorder)));

            WindowBorder.ResizeActivatedProperty = DependencyProperty.Register(
                "ResizeActivated",
                typeof(bool),
                typeof(WindowBorder),
                new FrameworkPropertyMetadata(false)
                );

            WindowBorder.NonClientControlsProperty = DependencyProperty.Register(
                "NonClientControls",
                typeof(object),
                typeof(WindowBorder),
                new FrameworkPropertyMetadata(false)
                );
        }

        ///<summary>
        /// If set to <c>true</c>, resize handles are created around the control. If set to <c>false</c>,
        /// no resize areas are shown.
        ///</summary>
        public bool ResizeActivated
        {
            get { return (bool)this.GetValue(WindowBorder.ResizeActivatedProperty); }
            set { this.SetValue(WindowBorder.ResizeActivatedProperty, value); }
        }

        ///<summary>
        /// Control that is shown at the right top side of the window left to the window buttons.
        /// The control is put as an overlay,so you have to care about leaving the space empty.
        ///</summary>
        public object NonClientControls
        {
            get { return this.GetValue(WindowBorder.NonClientControlsProperty); }
            set { this.SetValue(WindowBorder.NonClientControlsProperty, value); }
        }
    }
}
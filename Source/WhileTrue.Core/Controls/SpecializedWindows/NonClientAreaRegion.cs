using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WhileTrue.Classes.Win32;

namespace WhileTrue.Controls
{
    ///<summary>
    /// Non-Client Area placeholder. Use with <see cref="Window.EnableNonClientDrawing"/>
    ///</summary>
    public class NonClientAreaRegion:Decorator
    {
        /// <summary/>
        public static readonly DependencyProperty NonClientAreaTypeProperty;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Windows.Controls.Decorator"/> class.
        /// </summary>
        static NonClientAreaRegion()
        {
            NonClientAreaTypeProperty = DependencyProperty.RegisterAttached(
                "NonClientAreaType",
                typeof (NonClientArea),
                typeof(NonClientAreaRegion),
                new FrameworkPropertyMetadata(
                    NonClientArea.HTCLIENT,
                    FrameworkPropertyMetadataOptions.Inherits)
                );
        }

        public NonClientArea NonClientAreaType
        {
            get { return GetNonClientAreaType(this); }
            set { SetNonClientAreaType(this,value); }
        }

        public static NonClientArea GetNonClientAreaType(DependencyObject d)
        {
            return (NonClientArea)d.GetValue(NonClientAreaTypeProperty);
        }

        public static void SetNonClientAreaType(DependencyObject d, NonClientArea value)
        {
            d.SetValue(NonClientAreaTypeProperty, value);
        }



        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }
    }
}
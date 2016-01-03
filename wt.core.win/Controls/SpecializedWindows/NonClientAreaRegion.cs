using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using JetBrains.Annotations;
using WhileTrue.Classes.Win32;

namespace WhileTrue.Controls
{
    ///<summary>
    /// Non-Client Area placeholder. Use with <see cref="Window.EnableNonClientAreaDrawing"/>
    ///</summary>
    [PublicAPI]
    public class NonClientAreaRegion:Decorator
    {
        /// <summary/>
        public static readonly DependencyProperty NonClientAreaTypeProperty;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Windows.Controls.Decorator"/> class.
        /// </summary>
        static NonClientAreaRegion()
        {
            NonClientAreaRegion.NonClientAreaTypeProperty = DependencyProperty.RegisterAttached(
                "NonClientAreaType",
                typeof (NonClientArea),
                typeof(NonClientAreaRegion),
                new FrameworkPropertyMetadata(
                    NonClientArea.HTCLIENT,
                    FrameworkPropertyMetadataOptions.Inherits)
                );
        }

        /// <summary>
        /// sets/gets the nonclient area type that controls how the window manager reacts on user interaction with the NC area
        /// </summary>
        public NonClientArea NonClientAreaType
        {
            get { return NonClientAreaRegion.GetNonClientAreaType(this); }
            set { NonClientAreaRegion.SetNonClientAreaType(this,value); }
        }

        /// <summary>
        /// gets the nonclient area type that controls how the window manager reacts on user interaction with the NC area
        /// </summary>
        public static NonClientArea GetNonClientAreaType(DependencyObject d)
        {
            return (NonClientArea)d.GetValue(NonClientAreaRegion.NonClientAreaTypeProperty);
        }

        /// <summary>
        /// sets the nonclient area type that controls how the window manager reacts on user interaction with the NC area
        /// </summary>
        public static void SetNonClientAreaType(DependencyObject d, NonClientArea value)
        {
            d.SetValue(NonClientAreaRegion.NonClientAreaTypeProperty, value);
        }


        /// <summary>
        /// Implements <see cref="M:System.Windows.Media.Visual.HitTestCore(System.Windows.Media.PointHitTestParameters)"/> to supply base element hit testing behavior (returning <see cref="T:System.Windows.Media.HitTestResult"/>). 
        /// </summary>
        /// <returns>
        /// Results of the test, including the evaluated point.
        /// </returns>
        /// <param name="hitTestParameters">Describes the hit test to perform, including the initial hit point.</param>
        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }
    }
}
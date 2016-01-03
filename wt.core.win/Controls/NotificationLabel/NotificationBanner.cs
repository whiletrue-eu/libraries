using System.Windows;
using System.Windows.Controls;
using JetBrains.Annotations;

namespace WhileTrue.Controls
{
    /// <summary>
    /// NotificationBanner is a control that renders a text with an icon (info, warning, error) following the stying of the windows styleguide (background/border color)
    /// </summary>
    [PublicAPI]
    public class NotificationBanner : ContentControl
    {
        /// <summary>
        /// Type of the nofitication (info, warning, error)
        /// </summary>
        public static readonly DependencyProperty NotificationTypeProperty;

        static NotificationBanner()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(NotificationBanner), new FrameworkPropertyMetadata(typeof(NotificationBanner)));

            NotificationBanner.NotificationTypeProperty = DependencyProperty.Register(
                "NotificationType",
                typeof (NotificationType),
                typeof (NotificationBanner),
                new FrameworkPropertyMetadata(NotificationType.Info));
        }


        /// <summary>
        /// Type of the nofitication (info, warning, error)
        /// </summary>
        public NotificationType NotificationType
        {
            get { return (NotificationType) this.GetValue(NotificationBanner.NotificationTypeProperty); }
            set { this.SetValue(NotificationBanner.NotificationTypeProperty, value); }
        }
    }
}

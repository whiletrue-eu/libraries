using System;
using System.Windows;
using System.Windows.Controls;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls
{
    public class NotificationBanner : ContentControl
    {
        public static readonly DependencyProperty NotificationTypeProperty;

        static NotificationBanner()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NotificationBanner), new FrameworkPropertyMetadata(typeof(NotificationBanner)));

            NotificationTypeProperty = DependencyProperty.Register(
                "NotificationType",
                typeof (NotificationType),
                typeof (NotificationBanner),
                new FrameworkPropertyMetadata(NotificationType.Info));
        }


        public NotificationType NotificationType
        {
            get { return (NotificationType) this.GetValue(NotificationTypeProperty); }
            set { this.SetValue(NotificationTypeProperty, value); }
        }
    }
}

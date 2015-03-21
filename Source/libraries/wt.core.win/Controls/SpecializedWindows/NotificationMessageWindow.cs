using System;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using WhileTrue.Classes.Utilities;

using Size=System.Windows.Size;

namespace WhileTrue.Controls
{

    /// <summary>
    /// Spezialized window to show a notification in the corner of the desktop where the system tray is located.
    /// If you need custom fadein/out animations, try <see cref="PopupNotificationMessageWindow"/> instead
    /// </summary>
    public class NotificationMessageWindow : Window
    {
        /// <summary/>
        public static readonly DependencyProperty DockPaddingProperty;



        static NotificationMessageWindow()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(NotificationMessageWindow), new FrameworkPropertyMetadata(typeof(NotificationMessageWindow)));

            NotificationMessageWindow.DockPaddingProperty = DependencyProperty.Register(
                "DockPadding",
                typeof(int),
                typeof(NotificationMessageWindow),
                new FrameworkPropertyMetadata(0,
                                              FrameworkPropertyMetadataOptions.AffectsArrange,
                                              NotificationMessageWindow.DockPaddingPropertyChanged)
                );
        }

        private static void DockPaddingPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((NotificationMessageWindow)sender).NotifyDockPaddingChanged();
        }

        /// <summary/>
        public NotificationMessageWindow()
        {
            Classes.Utilities.Screen.ScreenChanged += this.MonitorDisplayMonitorDisplayChanged;

            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, this.CloseExecuted));

            this.RecalculateLocation();
        }

        private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        void MonitorDisplayMonitorDisplayChanged(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke( DispatcherPriority.Render, (Action) this.RecalculateLocation);
        }

        /// <summary>
        /// Padding between the desktop edge and the window
        /// </summary>
        public int DockPadding
        {
            get { return (int)this.GetValue(NotificationMessageWindow.DockPaddingProperty); }
            set { this.SetValue(NotificationMessageWindow.DockPaddingProperty, value); }
        }

        private void RecalculateLocation()
        {
            Classes.Utilities.Screen Screen = Screen.PrimaryScreen;
            int DockPadding = this.DockPadding;
            Rectangle WorkingArea = Screen.WorkingArea;

            double Top;
            double Left;

            switch (Screen.TaskbarLocation)
            {
                case TaskbarLocation.Top:
                    Top = WorkingArea.Top + DockPadding;
                    Left = WorkingArea.Width - this.ActualWidth - DockPadding;
                    break;
                case TaskbarLocation.Left:
                    Top = WorkingArea.Height - this.ActualHeight - DockPadding;
                    Left = WorkingArea.Left + DockPadding;
                    break;
                case TaskbarLocation.Bottom:
                case TaskbarLocation.Right:
                    Top = WorkingArea.Height - this.ActualHeight - DockPadding;
                    Left = WorkingArea.Width - this.ActualWidth - DockPadding;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            using (this.Dispatcher.DisableProcessing())
            {
                this.Top = Top;
                this.Left = Left;
            }
        }

        /// <summary>
        /// Override this method to arrange and size a window and its child elements. 
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Windows.Size"/> that reflects the actual size that was used.
        /// </returns>
        /// <param name="arrangeBounds">A <see cref="T:System.Windows.Size"/> that reflects the final size that the window should use to arrange itself and its children.</param>
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            Size Size = base.ArrangeOverride(arrangeBounds);
            this.Dispatcher.BeginInvoke(DispatcherPriority.Render, (Action)this.RecalculateLocation);
            return Size;
        }

        private void NotifyDockPaddingChanged()
        {
            this.RecalculateLocation();
        }
    }
}
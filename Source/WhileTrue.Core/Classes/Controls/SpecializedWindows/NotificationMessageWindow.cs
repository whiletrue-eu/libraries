using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Mz.Classes.Utilities;
using Size=System.Windows.Size;

namespace Mz.Classes.Controls
{

    public class NotificationMessageWindow : GlassWindow, INotifyPropertyChanged
    {
        /// <summary/>
        public static readonly DependencyProperty DockPaddingProperty;



        static NotificationMessageWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NotificationMessageWindow), new FrameworkPropertyMetadata(typeof(NotificationMessageWindow)));

            DockPaddingProperty = DependencyProperty.Register(
                "DockPadding",
                typeof(int),
                typeof(NotificationMessageWindow),
                new FrameworkPropertyMetadata(0,
                                              FrameworkPropertyMetadataOptions.AffectsArrange,
                                              DockPaddingPropertyChanged)
                );
        }

        private static void DockPaddingPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((NotificationMessageWindow)sender).NotifyDockPaddingChanged();
        }

        public NotificationMessageWindow()
        {
            Screen.ScreenChanged += this.MonitorDisplayMonitorDisplayChanged;

            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, this.CloseExecuted));

            this.RecalculateLocation();
        }

        private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        void MonitorDisplayMonitorDisplayChanged(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke( DispatcherPriority.Render, (VoidDelegate) this.RecalculateLocation);
        }

        public int DockPadding
        {
            get { return (int)this.GetValue(DockPaddingProperty); }
            set { this.SetValue(DockPaddingProperty, value); }
        }

        private void RecalculateLocation()
        {
            Screen Screen = Screen.PrimaryScreen;
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

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            Size Size = base.ArrangeOverride(arrangeBounds);
            this.Dispatcher.BeginInvoke(DispatcherPriority.Render, (VoidDelegate)this.RecalculateLocation);
            return Size;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyDockPaddingChanged()
        {
            this.RecalculateLocation();
        }
    }
}
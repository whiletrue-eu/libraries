using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WhileTrue.Classes.Utilities;
using FormsBitmap=System.Drawing.Bitmap;
using System.Windows.Input;

using MouseEventArgs=System.Windows.Input.MouseEventArgs;

namespace WhileTrue.Controls
{
    public class NotifyIcon : System.Windows.Window, INotifyIconCallback
    {
        public static readonly DependencyProperty DefaultCommandProperty;
        public static readonly DependencyProperty DefaultCommandParameterProperty;

        static NotifyIcon()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NotifyIcon), new FrameworkPropertyMetadata(typeof(NotifyIcon)));

            DefaultCommandProperty = DependencyProperty.Register(
                "DefaultCommand",
                typeof (ICommand),
                typeof(NotifyIcon),
                new FrameworkPropertyMetadata(
                    null
                    )
                );

            DefaultCommandParameterProperty = DependencyProperty.Register(
                "DefaultParameterCommand",
                typeof (object),
                typeof(NotifyIcon),
                new FrameworkPropertyMetadata(
                    null
                    )
                );
        }

        private readonly NotifyIconInteropWrapper notifyIcon;

        public NotifyIcon()
        {
            this.Dispatcher.Hooks.OperationCompleted += this.HooksOperationCompleted;

            this.notifyIcon = new NotifyIconInteropWrapper(this);
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.ContextMenu, this.ExecuteContextMenu));
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            this.ClearValue(LeftProperty);
            this.ClearValue(TopProperty);

            base.OnLocationChanged(e);
        }

        public ICommand DefaultCommand
        {
            get { return (ICommand)this.GetValue(DefaultCommandProperty); }
            set { this.SetValue(DefaultCommandProperty, value); }
        }

        public object DefaultCommandParameter
        {
            get { return this.GetValue(DefaultCommandParameterProperty); }
            set { this.SetValue(DefaultCommandParameterProperty, value); }
        }

        private void HooksOperationCompleted(object sender, System.Windows.Threading.DispatcherHookEventArgs e)
        {
            if( e.Operation.Priority == DispatcherPriority.Render )
            {
                this.UpdateIcon();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            this.notifyIcon.Dispose();

            this.Dispatcher.Hooks.OperationCompleted -= this.HooksOperationCompleted;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            if (e.Cancel == false)
            {
                // Workaround for a strange bug;
                // if the window is not activated, but the notify icon is, closing will hang the application
                // waiting for stylus input...
                this.Activate();
                this.CaptureStylus();
            }
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
            this.UpdateIcon();
        }

        private void ExecuteContextMenu(object sender, ExecutedRoutedEventArgs e)
        {
            
            if( this.ContextMenu != null )
            {
                this.Activate();
                this.ContextMenu.PlacementTarget = this;
                this.ContextMenu.IsOpen = true;
                e.Handled = true;
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if( e.Property == VisibilityProperty )
            {
                if( e.NewValue.Equals(System.Windows.Visibility.Visible ) )
                {
                    this.notifyIcon.IsVisible = true;
                }
                else
                {
                    this.notifyIcon.IsVisible = false;
                }
            }
        }

        private void UpdateIcon()
        {
            RenderTargetBitmap Bitmap = new RenderTargetBitmap(16, 16, 96, 96, PixelFormats.Pbgra32);
            Bitmap.Render(this);

            using (MemoryStream Stream = new MemoryStream())
            {
                PngBitmapEncoder Encoder = new PngBitmapEncoder();
                Encoder.Frames.Add(BitmapFrame.Create(Bitmap));
                Encoder.Save(Stream);
                Stream.Seek(0, SeekOrigin.Begin);
                this.notifyIcon.Icon = new FormsBitmap(Stream);
            }
        }

        void INotifyIconCallback.MouseMoved()
        {
            MouseEventArgs Args = new MouseEventArgs(Mouse.PrimaryDevice, 0);
            Args.RoutedEvent = MouseMoveEvent;
            
            this.RaiseEvent(Args);

            //  To support tooltip, debug into PopupControlService to have a look at the code
//            this.toolTipTimer = new DispatcherTimer(DispatcherPriority.Normal);
//            this.toolTipTimer.Interval = TimeSpan.FromMilliseconds(ToolTipService.GetInitialShowDelay(this));
//            this.toolTipTimer.Tick += OnRaiseToolTipOpeningEvent;
//            this.toolTipTimer.Start(); 
        }

        void INotifyIconCallback.MouseButtonUp(MouseButton button)
        {
            this.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, button){RoutedEvent = MouseUpEvent});

            switch (button)
            {
                case MouseButton.Left:
                    this.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, button) { RoutedEvent = MouseLeftButtonUpEvent});
                    break;
                case MouseButton.Right:
                    this.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, button) { RoutedEvent = MouseRightButtonUpEvent });
                    break;
                default:
                    return;
            }
        }

        void INotifyIconCallback.MouseButtonDown(MouseButton button)
        {
            this.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, button) { RoutedEvent = MouseDownEvent });

            switch (button)
            {
                case MouseButton.Left:
                    this.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, button) { RoutedEvent = MouseLeftButtonDownEvent });
                    break;
                case MouseButton.Right:
                    this.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, button) { RoutedEvent = MouseRightButtonDownEvent });
                    break;
                default:
                    return;
            }
        }

        void INotifyIconCallback.MouseButtonDoubleClick(MouseButton button)
        {
            MouseEventArgs Args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, button);
            Args.RoutedEvent = MouseDoubleClickEvent;
            this.RaiseEvent(Args);

            if (Args.Handled == false)
            {
                Keyboard.Focus(this);

                //Execute Default Command if defined
                ICommand DefaultCommand = this.DefaultCommand;
                object DefaultCommandParameter = this.DefaultCommandParameter;
                if (DefaultCommand != null )
                {
                    if( DefaultCommand.CanExecute(DefaultCommandParameter) )
                    {
                        DefaultCommand.Execute(DefaultCommandParameter);
                    }
                }
            }
        }

        void INotifyIconCallback.ContextMenu()
        {
            if ( ApplicationCommands.ContextMenu.CanExecute(null, this))
            {
                ApplicationCommands.ContextMenu.Execute(null, this);
            }
        }

        public void RecreationRequired()
        {
            this.notifyIcon.Recreate();
        }

        void INotifyIconCallback.MouseEnter()
        {
            this.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0) { RoutedEvent = MouseEnterEvent });
        }

        void INotifyIconCallback.MouseLeave()
        {
            this.Dispatcher.BeginInvoke( DispatcherPriority.Normal, 
                (Action)delegate {
                             this.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0) {RoutedEvent = MouseLeaveEvent});
                });
        }
    }
}

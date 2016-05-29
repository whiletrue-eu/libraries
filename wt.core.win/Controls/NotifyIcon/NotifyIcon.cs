using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FormsBitmap=System.Drawing.Bitmap;
using System.Windows.Input;
using JetBrains.Annotations;
using MouseEventArgs=System.Windows.Input.MouseEventArgs;

namespace WhileTrue.Controls
{
    /// <summary>
    /// Provides a WPF compatible notification icon, including taking the drawing area of the window as icon (including animations and effects)
    /// </summary>
    [PublicAPI]
    public class NotifyIcon : System.Windows.Window, INotifyIconCallback
    {
        /// <summary>
        /// Command that is executed when the notify icon is double-clicked
        /// </summary>
        public static readonly DependencyProperty DefaultCommandProperty;
        /// <summary>
        /// Command parameter of the command that is executed when the notify icon is double-clicked
        /// </summary>
        public static readonly DependencyProperty DefaultCommandParameterProperty;

        static NotifyIcon()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(NotifyIcon), new FrameworkPropertyMetadata(typeof(NotifyIcon)));

            NotifyIcon.DefaultCommandProperty = DependencyProperty.Register(
                "DefaultCommand",
                typeof (ICommand),
                typeof(NotifyIcon),
                new FrameworkPropertyMetadata(
                    null
                    )
                );

            NotifyIcon.DefaultCommandParameterProperty = DependencyProperty.Register(
                "DefaultParameterCommand",
                typeof (object),
                typeof(NotifyIcon),
                new FrameworkPropertyMetadata(
                    null
                    )
                );
        }

        private readonly NotifyIconInteropWrapper notifyIcon;

        /// <summary/>
        public NotifyIcon()
        {
            this.Dispatcher.Hooks.OperationCompleted += this.HooksOperationCompleted;

            this.notifyIcon = new NotifyIconInteropWrapper(this);
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.ContextMenu, this.ExecuteContextMenu));
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Window.LocationChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLocationChanged(EventArgs e)
        {
            this.ClearValue(System.Windows.Window.LeftProperty);
            this.ClearValue(System.Windows.Window.TopProperty);

            base.OnLocationChanged(e);
        }

        /// <summary>
        /// Command that is executed when the notify icon is double-clicked
        /// </summary>
        public ICommand DefaultCommand
        {
            get { return (ICommand)this.GetValue(NotifyIcon.DefaultCommandProperty); }
            set { this.SetValue(NotifyIcon.DefaultCommandProperty, value); }
        }

        /// <summary>
        /// Command parameter of the command that is executed when the notify icon is double-clicked
        /// </summary>
        public object DefaultCommandParameter
        {
            get { return this.GetValue(NotifyIcon.DefaultCommandParameterProperty); }
            set { this.SetValue(NotifyIcon.DefaultCommandParameterProperty, value); }
        }

        private void HooksOperationCompleted(object sender, System.Windows.Threading.DispatcherHookEventArgs e)
        {
            if( e.Operation.Priority == DispatcherPriority.Render )
            {
                this.UpdateIcon();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Window.Closed"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            this.notifyIcon.Dispose();

            this.Dispatcher.Hooks.OperationCompleted -= this.HooksOperationCompleted;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Window.Closing"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs"/> that contains the event data.</param>
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

        /// <summary>
        /// Called when the <see cref="T:System.Windows.Media.VisualCollection"/> of the visual object is modified.
        /// </summary>
        /// <param name="visualAdded">The <see cref="T:System.Windows.Media.Visual"/> that was added to the collection</param><param name="visualRemoved">The <see cref="T:System.Windows.Media.Visual"/> that was removed from the collection</param>
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

        /// <summary>
        /// Invoked whenever the effective value of any dependency property on this <see cref="T:System.Windows.FrameworkElement"/> has been updated. The specific dependency property that changed is reported in the arguments parameter. Overrides <see cref="M:System.Windows.DependencyObject.OnPropertyChanged(System.Windows.DependencyPropertyChangedEventArgs)"/>.
        /// </summary>
        /// <param name="e">The event data that describes the property that changed, as well as old and new values.</param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if( e.Property == UIElement.VisibilityProperty )
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
            Args.RoutedEvent = UIElement.MouseMoveEvent;
            
            this.RaiseEvent(Args);

            //  To support tooltip, debug into PopupControlService to have a look at the code
//            this.toolTipTimer = new DispatcherTimer(DispatcherPriority.Normal);
//            this.toolTipTimer.Interval = TimeSpan.FromMilliseconds(ToolTipService.GetInitialShowDelay(this));
//            this.toolTipTimer.Tick += OnRaiseToolTipOpeningEvent;
//            this.toolTipTimer.Start(); 
        }

        void INotifyIconCallback.MouseButtonUp(MouseButton button)
        {
            this.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, button){RoutedEvent = UIElement.MouseUpEvent});

            switch (button)
            {
                case MouseButton.Left:
                    this.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, button) { RoutedEvent = UIElement.MouseLeftButtonUpEvent});
                    break;
                case MouseButton.Right:
                    this.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, button) { RoutedEvent = UIElement.MouseRightButtonUpEvent });
                    break;
                default:
                    return;
            }
        }

        void INotifyIconCallback.MouseButtonDown(MouseButton button)
        {
            this.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, button) { RoutedEvent = UIElement.MouseDownEvent });

            switch (button)
            {
                case MouseButton.Left:
                    this.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, button) { RoutedEvent = UIElement.MouseLeftButtonDownEvent });
                    break;
                case MouseButton.Right:
                    this.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, button) { RoutedEvent = UIElement.MouseRightButtonDownEvent });
                    break;
                default:
                    return;
            }
        }

        void INotifyIconCallback.MouseButtonDoubleClick(MouseButton button)
        {
            MouseEventArgs Args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, button);
            Args.RoutedEvent = Control.MouseDoubleClickEvent;
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

        void INotifyIconCallback.RecreationRequired()
        {
            this.notifyIcon.Recreate();
        }

        void INotifyIconCallback.MouseEnter()
        {
            this.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0) { RoutedEvent = UIElement.MouseEnterEvent });
        }

        void INotifyIconCallback.MouseLeave()
        {
            this.Dispatcher.BeginInvoke( DispatcherPriority.Normal, 
                (Action)delegate {
                             this.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0) {RoutedEvent = UIElement.MouseLeaveEvent});
                });
        }
    }
}

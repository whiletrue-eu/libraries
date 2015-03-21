// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeProtected.Global
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using WhileTrue.Classes.CodeInspection;
using WhileTrue.Classes.Utilities;
using WhileTrue.Classes.Win32;
using WhileTrue.Classes.Wpf;


namespace WhileTrue.Controls
{
    /// <summary>
    /// Window that implements vista glass effect
    /// </summary>
    /// <remarks>
    /// The window supports the extension of the glass frame into the client area (<see cref="GlassMargin"/>) as well as blurring
    /// the client background. To attach the glass margins (and thus the client area) to a framework element such as a panel that 
    /// represents the client area, you can also bind the glass margin propety directly to this framework element by using binding.
    /// Please note, that the element then will be surrounded by the client frame, so you may want to decrease the client control 
    /// by the border size by setting the 'margin' property of the client area element to <c>1</c>.
    /// If don't want to have the client area border automatically, you can also choose to set the window to 'sheet of glass' by
    /// setting the <see cref="GlassMargin"/> property to "<c>Sheet</c>".
    /// </remarks>
    public class Window : System.Windows.Window
    {
        /// <summary/>
        public static readonly DependencyProperty GlassMarginProperty;
        /// <summary/>
        public static readonly DependencyProperty BlurClientAreaProperty;
        /// <summary/>
        public static readonly DependencyProperty EnableNonClientAreaDrawingProperty;
        /// <summary/>
        public static readonly DependencyProperty CustomWindowTitleProperty;
        /// <summary/>
        public static readonly DependencyProperty NonClientControlsProperty;
        private static readonly DependencyPropertyKey nonClientControlsKey;
        /// <summary/>
        public static readonly DependencyProperty SetParametersForBorderlessNonResizeableGlassWindowProperty;
        /// <summary/>
        public static readonly DependencyProperty IsDwmEnabledProperty;
        private static readonly DependencyPropertyKey isDwmEnabledPropertyKey;

        /// <summary/>
        public static readonly RoutedEvent FadeInRequestedEvent;

        /// <summary/>
        public static readonly RoutedEvent FadeOutRequestedEvent;


        private DwmWindowHelper dwmHelper;

        static Window()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata(typeof(Window)));

            GlassMarginProperty = DependencyProperty.Register(
                "GlassMargin",
                typeof(GlassMargin),
                typeof(Window),
                new FrameworkPropertyMetadata(new GlassMargin(0, 0, 0, 0),
                                              FrameworkPropertyMetadataOptions.AffectsRender,
                                              GlassMarginPropertyChanged)
                );

            BlurClientAreaProperty = DependencyProperty.Register(
                "BlurClientArea",
                typeof(bool),
                typeof(Window),
                new FrameworkPropertyMetadata(false,
                                              FrameworkPropertyMetadataOptions.AffectsRender,
                                              BlurClientAreaPropertyChanged)
                );
            EnableNonClientAreaDrawingProperty = DependencyProperty.Register(
                "EnableNonClientAreaDrawing",
                typeof(bool),
                typeof(Window),
                new FrameworkPropertyMetadata(false,
                                              FrameworkPropertyMetadataOptions.AffectsMeasure,
                                              EnableNonClientAreaDrawingPropertyChanged)
                );
            nonClientControlsKey = DependencyProperty.RegisterReadOnly(
                "NonClientControls",
                typeof(ObservableCollection<object>),
                typeof(Window),
                new FrameworkPropertyMetadata(new ObservableCollection<object>())
                );
            NonClientControlsProperty = nonClientControlsKey.DependencyProperty;

            CustomWindowTitleProperty = DependencyProperty.Register(
                "CustomWindowTitle",
                typeof(object),
                typeof(Window)
                );

            SetParametersForBorderlessNonResizeableGlassWindowProperty = DependencyProperty.Register(
                "SetParametersForBorderlessNonResizeableGlassWindow",
                typeof(bool),
                typeof(Window),
                new FrameworkPropertyMetadata(false,
                                              FrameworkPropertyMetadataOptions.AffectsRender,
                                              SetParametersForBorderlessNonResizeableGlassWindowPropertyChanged)
                );

            isDwmEnabledPropertyKey = DependencyProperty.RegisterReadOnly(
                "IsDwmEnabled",
                typeof(bool),
                typeof(Window),
                new FrameworkPropertyMetadata(false,
                                              FrameworkPropertyMetadataOptions.AffectsRender)
                );
            IsDwmEnabledProperty = isDwmEnabledPropertyKey.DependencyProperty;

            FadeInRequestedEvent = EventManager.RegisterRoutedEvent(
                "FadeInRequested",
                RoutingStrategy.Bubble,
                typeof(EventHandler),
                typeof(Window)
                );

            FadeOutRequestedEvent = EventManager.RegisterRoutedEvent(
                "FadeOutRequested",
                RoutingStrategy.Bubble,
                typeof(EventHandler),
                typeof(Window)
                );
        }

        private static void EnableNonClientAreaDrawingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window Window = (Window)d;
            Window.SetNonClientAreaDrawing((bool)e.NewValue);
        }

        /// <summary/>
        public Window()
        {
            this.SetValue(nonClientControlsKey, new ObservableCollection<object>());
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == WindowStyleProperty || e.Property == ResizeModeProperty)
            {
                this.EnsureBorderlessNonresizableWindowShowsGlassBackground();
            }
        }
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        // ReSharper disable InconsistentNaming
        private const int GWL_STYLE = -16;
        private const int GWL_EXSTYLE = -20;
        // ReSharper restore InconsistentNaming

        private void EnsureBorderlessNonresizableWindowShowsGlassBackground()
        {
            if (this.SetParametersForBorderlessNonResizeableGlassWindow)
            {
                //Workaroud: Otherwise a borderless window will not have glass background
                IntPtr HWnd = new WindowInteropHelper(this).Handle;
                if (HWnd != IntPtr.Zero)
                {
                    GetWindowLongPtr(HWnd, GWL_EXSTYLE).ToInt64();
                    SetWindowLongPtr(HWnd, GWL_STYLE, new IntPtr(0x60B0000));
                    SetWindowLongPtr(HWnd, GWL_EXSTYLE, new IntPtr(0x108));
                }
            }
        }

        /// <summary>
        /// Gets/Sets the margin of the window, that shall be rendered with the 'glass' effect
        /// </summary>
        /// <remarks>
        /// Set to <c>"Sheet"</c> (or <c>-1</c>) to render the whole window as a 'sheet of glass'
        /// </remarks>
        public GlassMargin GlassMargin
        {
            get { return (GlassMargin)this.GetValue(GlassMarginProperty); }
            set { this.SetValue(GlassMarginProperty, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool BlurClientArea
        {
            get { return (bool)this.GetValue(BlurClientAreaProperty); }
            set { this.SetValue(BlurClientAreaProperty, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool EnableNonClientAreaDrawing
        {
            get { return (bool)this.GetValue(EnableNonClientAreaDrawingProperty); }
            set { this.SetValue(EnableNonClientAreaDrawingProperty, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public object CustomWindowTitle
        {
            get { return this.GetValue(CustomWindowTitleProperty); }
            [UsedImplicitly]
            set { this.SetValue(CustomWindowTitleProperty, value); }
        }


        /// <summary>
        /// Gets the list of non-client controls.
        /// </summary>
        public ObservableCollection<object> NonClientControls
        {
            get { return (ObservableCollection<object>)this.GetValue(NonClientControlsProperty); }
        }


        /// <summary>
        /// 
        /// </summary>
        public bool IsDwmEnabled
        {
            get { return (bool)this.GetValue(IsDwmEnabledProperty); }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool SetParametersForBorderlessNonResizeableGlassWindow
        {
            get { return (bool)this.GetValue(SetParametersForBorderlessNonResizeableGlassWindowProperty); }
            [UsedImplicitly]
            set { this.SetValue(SetParametersForBorderlessNonResizeableGlassWindowProperty, value); }
        }


        private static void GlassMarginPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((Window)sender).NotifyGlassMarginChanged((GlassMargin)e.OldValue, (GlassMargin)e.NewValue);
        }

        private static void BlurClientAreaPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((Window)sender).NotifyBlurClientAreaChanged();
        }

        private static void SetParametersForBorderlessNonResizeableGlassWindowPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((Window)sender).NotifySetParametersForBorderlessNonResizeableGlassWindowChanged();
        }

        ///<summary>
        ///</summary>
        public event EventHandler FadeInRequested
        {
            add { AddHandler(FadeInRequestedEvent, value); }
            remove { RemoveHandler(FadeInRequestedEvent, value); }
        }

        ///<summary>
        ///</summary>
        public event EventHandler FadeOutRequested
        {
            add { AddHandler(FadeOutRequestedEvent, value); }
            remove { RemoveHandler(FadeOutRequestedEvent, value); }
        }

        /// <summary/>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            this.InitializeGlassFrame();
        }

        private void InitializeGlassFrame()
        {
            IntPtr Handle = new WindowInteropHelper(this).Handle;
            this.dwmHelper = DwmAPI.RegisterForGlassFrame(Handle, new DwmAPI.Margins(0), true, this.EnableNonClientAreaDrawing, this.NotifyGlassEnabledChanged, this.NonClientHitTest);
            HwndSource.FromHwnd(Handle).DbC_AssureNotNull().AddHook(this.WindowHook);

            //Apply glass border (initially)
            this.UpdateGlassEffects();
            if (this.EnableNonClientAreaDrawing)
            {
                this.UpdateLayout();
            }
        }

        private NonClientArea NonClientHitTest(ushort x, ushort y)
        {
            Point Coordinate = this.PointFromScreen(new Point(x, y));
            HitTestResult HitTest = VisualTreeHelper.HitTest(this, Coordinate);

            if (HitTest != null && HitTest.VisualHit != null)
            {
                NonClientArea Area = NonClientAreaRegion.GetNonClientAreaType(HitTest.VisualHit);
                return Area;
            }
            else
            {
                return NonClientArea.HTNOWHERE;
            }
        }

        private void NotifyGlassEnabledChanged()
        {
            this.UpdateGlassEffects();
        }

        private IntPtr WindowHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            return this.dwmHelper.PreviewWindowMessage(hwnd, msg, wParam, lParam, ref handled);
        }

        private void UpdateGlassEffects()
        {
            if (this.dwmHelper != null)
            {
                if (this.GlassMargin != null)
                {
                    GlassMargin GlassMargin = this.GlassMargin;
                    this.dwmHelper.Margins = new DwmAPI.Margins(
                        (int)GlassMargin.Left,
                        (int)GlassMargin.Right,
                        (int)GlassMargin.Top,
                        (int)GlassMargin.Bottom);
                }
                else
                {
                    this.dwmHelper.Margins = new DwmAPI.Margins(0);
                }
                this.dwmHelper.BlurClientArea = this.BlurClientArea;
                if (this.dwmHelper.DwmIsCompositionEnabled)
                {
                    HwndSource.FromHwnd(this.dwmHelper.WindowHandle).DbC_AssureNotNull().CompositionTarget.BackgroundColor = Colors.Transparent;
                }
                else
                {
                    HwndSource.FromHwnd(this.dwmHelper.WindowHandle).DbC_AssureNotNull().CompositionTarget.BackgroundColor = this.AllowsTransparency ? Colors.Transparent : SystemColors.WindowColor;
                }
                this.EnsureBorderlessNonresizableWindowShowsGlassBackground();

                this.SetValue(isDwmEnabledPropertyKey, this.dwmHelper.DwmIsCompositionEnabled);
            }
            else
            {
                //ignore - before completely initialized
            }
        }

        private void SetNonClientAreaDrawing(bool enabled)
        {
            if (this.dwmHelper != null)
            {
                this.dwmHelper.SetNonClientAreaDrawing(enabled);
            }
            else
            {
                //ignore - before completely initialized
            }
        }


        private void NotifyGlassMarginChanged(GlassMargin oldValue, GlassMargin newValue)
        {
            if (oldValue != null)
            {
                oldValue.PropertyChanged -= this.GlassMarginPropertyChanged;
            }
            if (newValue != null)
            {
                newValue.PropertyChanged += this.GlassMarginPropertyChanged;
            }
            this.UpdateGlassEffects();
        }

        private void NotifyBlurClientAreaChanged()
        {
            this.UpdateGlassEffects();
        }

        private void NotifySetParametersForBorderlessNonResizeableGlassWindowChanged()
        {
            this.UpdateGlassEffects();
        }

        private void NotifyGlassMarginChanged()
        {
            this.UpdateGlassEffects();
        }

        private void GlassMarginPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.NotifyGlassMarginChanged();
        }

        ///<summary>
        ///</summary>
        public virtual void FadeIn()
        {
            this.RaiseEvent(new RoutedEventArgs { RoutedEvent = FadeInRequestedEvent });
        }

        ///<summary>
        ///</summary>
        public virtual void FadeOut()
        {
            this.RaiseEvent(new RoutedEventArgs { RoutedEvent = FadeOutRequestedEvent });
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (e.Cancel == false)
            {
                switch (this.CloseBehaviour)
                {
                    case WindowCloseBehaviour.Close:
                        //Do nothing -> continue close
                        break;
                    case WindowCloseBehaviour.Hide:
                        e.Cancel = true;
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)this.Hide);
                        break;
                    case WindowCloseBehaviour.FadeOut:
                        e.Cancel = true;
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)this.FadeOut);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        ///<summary>
        ///</summary>
        public WindowCloseBehaviour CloseBehaviour { get; set; }

        ///<summary>
        ///</summary>
        [UsedImplicitly]
        public void Close(bool overrideCloseBehaviour)
        {
            if (overrideCloseBehaviour)
            {
                WindowCloseBehaviour CurrentBehaviour = this.CloseBehaviour;
                this.CloseBehaviour = WindowCloseBehaviour.Close;
                this.Close();
                this.CloseBehaviour = CurrentBehaviour;
            }
            else
            {
                this.Close();
            }
        }

        ///<summary>
        /// Shows the window in a modal way.
        ///</summary>
        /// <remarks>
        /// Other as the default implementation, if the windows <see cref="Window.Owner"/> is <c>null</c>, the 
        /// dialog tries to find the active window and uses it as owner. Additionally, if the owners window <see cref="Window.Icon"/>
        /// is set, it is taken over s icon for this dialog as well.
        /// </remarks>
        public new bool? ShowDialog()
        {
            if (this.Owner == null)
            {
                this.Owner = WpfUtils.FindActiveWindow();
            }
            if (this.Owner != null)
            {
                this.Icon = this.Owner.Invoke(owner => owner.Icon);
            }
            return base.ShowDialog();
        }
    }
}
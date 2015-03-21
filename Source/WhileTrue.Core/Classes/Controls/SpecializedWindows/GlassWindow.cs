using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Mz.Classes.Types;
using Mz.Classes.Utilities;
using Mz.Classes.Win32;

namespace Mz.Classes.Controls
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
    public class GlassWindow : Window
    {
        /// <summary/>
        public static readonly DependencyProperty GlassMarginProperty;
        /// <summary/>
        public static readonly DependencyProperty BlurClientAreaProperty;
        /// <summary/>
        public static readonly DependencyProperty SetParametersForBorderlessNonResizeableGlassWindowProperty;

        /// <summary/>
        public static readonly RoutedEvent FadeInRequestedEvent;

        /// <summary/>
        public static readonly RoutedEvent FadeOutRequestedEvent;


        private DwmAPI.GlassWindowHelper glassHelper;

        static GlassWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GlassWindow), new FrameworkPropertyMetadata(typeof(GlassWindow)));

            GlassMarginProperty = DependencyProperty.Register(
                "GlassMargin",
                typeof (GlassMargin),
                typeof (GlassWindow),
                new FrameworkPropertyMetadata(new GlassMargin(0,0,0,0),
                                              FrameworkPropertyMetadataOptions.AffectsRender,
                                              GlassMarginPropertyChanged)
                );

            BlurClientAreaProperty = DependencyProperty.Register(
                "BlurClientArea",
                typeof (bool),
                typeof (GlassWindow),
                new FrameworkPropertyMetadata(false,
                                              FrameworkPropertyMetadataOptions.AffectsRender,
                                              BlurClientAreaPropertyChanged)
                );
            SetParametersForBorderlessNonResizeableGlassWindowProperty = DependencyProperty.Register(
                "SetParametersForBorderlessNonResizeableGlassWindow",
                typeof (bool),
                typeof (GlassWindow),
                new FrameworkPropertyMetadata(false,
                                              FrameworkPropertyMetadataOptions.AffectsRender,
                                              SetParametersForBorderlessNonResizeableGlassWindowPropertyChanged)
                );


            FadeInRequestedEvent = EventManager.RegisterRoutedEvent(
                "FadeInRequested",
                RoutingStrategy.Bubble,
                typeof (EventHandler),
                typeof (GlassWindow)
                );

            FadeOutRequestedEvent = EventManager.RegisterRoutedEvent(
                "FadeOutRequested",
                RoutingStrategy.Bubble,
                typeof (EventHandler),
                typeof (GlassWindow)
                );
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if( e.Property == WindowStyleProperty || e.Property == ResizeModeProperty)
            {
                this.EnsureBorderlessNonresizableWindowShowsGlassBackground();
            }
        }
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd,int nIndex,IntPtr dwNewLong);
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
            get { return (GlassMargin) this.GetValue(GlassMarginProperty); }
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
        public bool SetParametersForBorderlessNonResizeableGlassWindow
        {
            get { return (bool)this.GetValue(SetParametersForBorderlessNonResizeableGlassWindowProperty); }
            set { this.SetValue(SetParametersForBorderlessNonResizeableGlassWindowProperty, value); }
        }

        private static void GlassMarginPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((GlassWindow) sender).NotifyGlassMarginChanged((GlassMargin) e.OldValue, (GlassMargin) e.NewValue);
        }

        private static void BlurClientAreaPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((GlassWindow)sender).NotifyBlurClientAreaChanged();
        }

        private static void SetParametersForBorderlessNonResizeableGlassWindowPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((GlassWindow)sender).NotifySetParametersForBorderlessNonResizeableGlassWindowChanged();
        }

        public event EventHandler FadeInRequested
        {
            add { AddHandler(FadeInRequestedEvent, value); }
            remove { RemoveHandler(FadeInRequestedEvent, value); }
        }

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
            this.glassHelper = DwmAPI.RegisterForGlassFrame(Handle, new DwmAPI.Margins(0), true);
            this.glassHelper.GlassEnabledChanged += this.GlassHelperGlassEnabledChanged;
            HwndSource.FromHwnd(Handle).DbC_AssureNotNull().AddHook(this.WindowHook);

            //Apply glass border (initially)
            this.UpdateGlassEffects();
        }

        private void GlassHelperGlassEnabledChanged(object sender, EventArgs e)
        {
            this.UpdateGlassEffects();
        }

        private IntPtr WindowHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            this.glassHelper.PreviewWindowMessage(msg);
            return IntPtr.Zero;
        }

        private void UpdateGlassEffects()
        {
            if (this.glassHelper != null)
            {
                GlassMargin GlassMargin = this.GlassMargin;
                this.glassHelper.Margins = new DwmAPI.Margins(
                    (int) GlassMargin.Left,
                    (int) GlassMargin.Right,
                    (int) GlassMargin.Top,
                    (int) GlassMargin.Bottom);
                this.glassHelper.BlurClientArea = this.BlurClientArea;
                if (this.glassHelper.GlassSupported)
                {
                    HwndSource.FromHwnd(this.glassHelper.WindowHandle).DbC_AssureNotNull().CompositionTarget.BackgroundColor = Colors.Transparent;
                }
                else
                {
                    HwndSource.FromHwnd(this.glassHelper.WindowHandle).DbC_AssureNotNull().CompositionTarget.BackgroundColor = this.AllowsTransparency?Colors.Transparent:SystemColors.WindowColor; 
                }
                this.EnsureBorderlessNonresizableWindowShowsGlassBackground();
            }
            else
            {
                //ignore
            }
        }

        private void NotifyGlassMarginChanged(GlassMargin oldValue, GlassMargin newValue)
        {
            oldValue.PropertyChanged -= this.GlassMarginPropertyChanged;
            newValue.PropertyChanged += this.GlassMarginPropertyChanged;
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

        public virtual void FadeIn()
        {
            this.RaiseEvent( new RoutedEventArgs{RoutedEvent = FadeInRequestedEvent});
        }

        public virtual void FadeOut()
        {
            this.RaiseEvent(new RoutedEventArgs { RoutedEvent = FadeOutRequestedEvent });
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if( e.Cancel == false )
            {
                switch( this.CloseBehaviour )
                {
                    case WindowCloseBehaviour.Close:
                        //Do nothing -> continue close
                        break;
                    case WindowCloseBehaviour.Hide:
                        e.Cancel = true;
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (VoidDelegate)delegate { this.Hide(); });
                        break;
                    case WindowCloseBehaviour.FadeOut:
                        e.Cancel = true;
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (VoidDelegate)delegate { this.FadeOut(); });
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public WindowCloseBehaviour CloseBehaviour { get; set; }
        
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
    }
}
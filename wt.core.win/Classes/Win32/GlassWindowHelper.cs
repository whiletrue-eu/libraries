using System;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Win32
{
    /// <summary>
    /// Supports implementation of a glass aware window through callbacks when the glass effect is disabled/enabled.
    /// </summary>
    internal class GlassWindowHelper
    {
        private const int WM_COMPOSITIONCHANGED = 0x031E;
        private const int WM_NCCALCSIZE = 0x0083;
        private const int WM_NCHITTEST = 0x0084;

        private readonly IntPtr windowHandle;
        private bool blurClientArea;
        private readonly Action notifyDwmCompositionChanged;
        private readonly Func<ushort, ushort, NonClientArea> nonClientHitTest;
        private bool dwmIsCompositionEnabled;
        private DwmAPI.Margins margins;
        private bool nonClientAreaDrawingEnabled;

        internal GlassWindowHelper(IntPtr windowHandle, DwmAPI.Margins margins, bool blurClientArea, bool nonClientAreaDrawingEnabled, Action notifyDwmCompositionChanged, Func<ushort,ushort,NonClientArea> nonClientHitTest)
        {
            this.windowHandle = windowHandle;
            this.dwmIsCompositionEnabled = DwmAPI.IsCompositionEnabled();
            this.margins = margins;
            this.blurClientArea = blurClientArea;
            this.notifyDwmCompositionChanged = notifyDwmCompositionChanged;
            this.UpdateGlassEffect();
            this.UpdateNonClientArea(); 
            this.nonClientAreaDrawingEnabled = nonClientAreaDrawingEnabled;
            this.nonClientHitTest = nonClientHitTest;
            this.UpdateNonClientArea();
        }

        /// <summary>
        /// Sets the margin that is used for the glass effect. If <c>-1</c> is specified, the window is completely rendered with the glass effect ('sheet of glass')
        /// </summary>
        public DwmAPI.Margins Margins
        {
            set
            {
                this.margins = value;
                this.UpdateGlassEffect();
            }
        }

        /// <summary>
        /// Sets whether the client area should be blurred.
        /// </summary>
        public bool BlurClientArea
        {
            set
            {
                this.blurClientArea = value;
                this.UpdateGlassEffect();
            }
        }


        public IntPtr WindowHandle
        {
            get { return this.windowHandle; }
        }

        public bool DwmIsCompositionEnabled
        {
            get { return this.dwmIsCompositionEnabled; }
        }

        private void UpdateGlassEffect()
        {
            DwmAPI.EnableGlassEffect(this.windowHandle, this.margins);
            DwmAPI.EnableBlurBehindWindow(this.windowHandle, this.blurClientArea);
        }

        protected void NotifyCompositionChanged()
        {
            this.dwmIsCompositionEnabled = DwmAPI.IsCompositionEnabled();
            this.UpdateGlassEffect();
            this.notifyDwmCompositionChanged();
        }

        private bool firstTimeNonClientCalc=true;
        public IntPtr PreviewWindowMessage(IntPtr hWindow, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (message)
            {
                case WM_COMPOSITIONCHANGED:
                    this.NotifyCompositionChanged();
                    return IntPtr.Zero;
                case WM_NCCALCSIZE:
                    if (this.firstTimeNonClientCalc)
                    {
                        // Suppress first time, otherwise glass effect will be messed up..
                        this.firstTimeNonClientCalc = false;
                        return IntPtr.Zero;
                    }
                    else
                    {
                        if (this.nonClientAreaDrawingEnabled && wParam != IntPtr.Zero) //TRUE
                        {
                            //no nonclient frame
                            handled = true;
                            return IntPtr.Zero;
                        }
                        else
                        {
                            return IntPtr.Zero;
                        }
                    }
            
            case WM_NCHITTEST:
                    IntPtr Result = IntPtr.Zero;
                    if (this.dwmIsCompositionEnabled)
                    {
                        handled = DwmAPI.DwmDefWindowProc(hWindow, message, wParam, lParam, out Result);
                    }
                    if (handled == false)
                    {
                        uint Coordinates;
                        checked
                        {
                            Coordinates = (uint) lParam.ToInt32();
                        }

                        NonClientArea NonClientArea = this.nonClientHitTest(Coordinates.GetLoUShort(), Coordinates.GetHiUShort());
                        if (NonClientArea != NonClientArea.HTNOWHERE)
                        {
                            Result = new IntPtr((int) NonClientArea);
                            handled = true;
                        }
                    }
                    return Result;
                default:
                    return IntPtr.Zero;
            }
        }


        public void SetNonClientAreaDrawing(bool enabled)
        {
            this.nonClientAreaDrawingEnabled = enabled;
            this.UpdateNonClientArea();
        }
        private void UpdateNonClientArea()
        {
            DwmAPI.SetWindowPos(this.windowHandle, IntPtr.Zero, 0, 0, 0, 0, 0x27 /*SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED*/);
        }
    }
}
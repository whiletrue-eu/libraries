using System;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Win32
{
    /// <summary>
    /// Supports implementation of a glass aware window through callbacks when the glass effect is disabled/enabled.
    /// </summary>
    internal class DwmWindowHelper
    {
        private const int WmCompositionchanged = 0x031E;
        private const int WmNccalcsize = 0x0083;
        private const int WmNchittest = 0x0084;

        private bool blurClientArea;
        private readonly Action notifyDwmCompositionChanged;
        private readonly Func<ushort, ushort, NonClientArea> nonClientHitTest;
        private DwmApi.Margins margins;
        private bool nonClientAreaDrawingEnabled;

        internal DwmWindowHelper(IntPtr windowHandle, DwmApi.Margins margins, bool blurClientArea, bool nonClientAreaDrawingEnabled, Action notifyDwmCompositionChanged, Func<ushort,ushort,NonClientArea> nonClientHitTest)
        {
            this.WindowHandle = windowHandle;
            this.DwmIsCompositionEnabled = DwmApi.IsCompositionEnabled();
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
        public DwmApi.Margins Margins
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


        public IntPtr WindowHandle { get; }

        public bool DwmIsCompositionEnabled { get; private set; }

        private void UpdateGlassEffect()
        {
            DwmApi.EnableGlassEffect(this.WindowHandle, this.margins);
            DwmApi.EnableBlurBehindWindow(this.WindowHandle, this.blurClientArea);
        }

        protected void NotifyCompositionChanged()
        {
            this.DwmIsCompositionEnabled = DwmApi.IsCompositionEnabled();
            this.UpdateGlassEffect();
            this.notifyDwmCompositionChanged();
        }

        public IntPtr PreviewWindowMessage(IntPtr hWindow, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (message)
            {
                case DwmWindowHelper.WmCompositionchanged:
                    this.NotifyCompositionChanged();
                    return IntPtr.Zero;
                case DwmWindowHelper.WmNccalcsize:
                    if (this.DwmIsCompositionEnabled)
                    {
                        if (this.nonClientAreaDrawingEnabled &&
                            wParam != IntPtr.Zero /*TRUE*/)
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
                    else
                    {
                        // Support client drawing only when Dwm is enabled. Otherwise non client drawing is too complicated ;-)
                        return IntPtr.Zero;
                    }
                case DwmWindowHelper.WmNchittest:
                    // Hit test non client area
                    IntPtr Result = IntPtr.Zero;
                    if (this.DwmIsCompositionEnabled)
                    { 
                        // if Dwm is enabled,first let DWM hit test for window buttons
                        handled = DwmApi.DwmDefWindowProc(hWindow, message, wParam, lParam, out Result);
                    }
                    if (handled == false)
                    {
                        // hit test for non client areas in the window layout
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
            DwmApi.SetWindowPos(this.WindowHandle, IntPtr.Zero, 0, 0, 0, 0, 0x27 /*SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED*/);
        }
    }
}
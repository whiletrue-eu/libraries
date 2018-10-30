using System;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.Win32
{
    /// <summary>
    ///     Supports implementation of a glass aware window through callbacks when the glass effect is disabled/enabled.
    /// </summary>
    internal class DwmWindowHelper
    {
        private const int WmCompositionchanged = 0x031E;
        private const int WmNccalcsize = 0x0083;
        private const int WmNchittest = 0x0084;
        private readonly Func<ushort, ushort, NonClientArea> nonClientHitTest;
        private readonly Action notifyDwmCompositionChanged;

        private bool blurClientArea;
        private DwmApi.Margins margins;
        private bool nonClientAreaDrawingEnabled;

        internal DwmWindowHelper(IntPtr windowHandle, DwmApi.Margins margins, bool blurClientArea,
            bool nonClientAreaDrawingEnabled, Action notifyDwmCompositionChanged,
            Func<ushort, ushort, NonClientArea> nonClientHitTest)
        {
            WindowHandle = windowHandle;
            DwmIsCompositionEnabled = DwmApi.IsCompositionEnabled();
            this.margins = margins;
            this.blurClientArea = blurClientArea;
            this.notifyDwmCompositionChanged = notifyDwmCompositionChanged;
            UpdateGlassEffect();
            UpdateNonClientArea();
            this.nonClientAreaDrawingEnabled = nonClientAreaDrawingEnabled;
            this.nonClientHitTest = nonClientHitTest;
            UpdateNonClientArea();
        }

        /// <summary>
        ///     Sets the margin that is used for the glass effect. If <c>-1</c> is specified, the window is completely rendered
        ///     with the glass effect ('sheet of glass')
        /// </summary>
        public DwmApi.Margins Margins
        {
            set
            {
                margins = value;
                UpdateGlassEffect();
            }
        }

        /// <summary>
        ///     Sets whether the client area should be blurred.
        /// </summary>
        public bool BlurClientArea
        {
            set
            {
                blurClientArea = value;
                UpdateGlassEffect();
            }
        }


        public IntPtr WindowHandle { get; }

        public bool DwmIsCompositionEnabled { get; private set; }

        private void UpdateGlassEffect()
        {
            DwmApi.EnableGlassEffect(WindowHandle, margins);
            DwmApi.EnableBlurBehindWindow(WindowHandle, blurClientArea);
        }

        protected void NotifyCompositionChanged()
        {
            DwmIsCompositionEnabled = DwmApi.IsCompositionEnabled();
            UpdateGlassEffect();
            notifyDwmCompositionChanged();
        }

        public IntPtr PreviewWindowMessage(IntPtr hWindow, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (message)
            {
                case WmCompositionchanged:
                    NotifyCompositionChanged();
                    return IntPtr.Zero;
                case WmNccalcsize:
                    if (DwmIsCompositionEnabled)
                    {
                        if (nonClientAreaDrawingEnabled &&
                            wParam != IntPtr.Zero /*TRUE*/)
                        {
                            //no nonclient frame
                            handled = true;
                            return IntPtr.Zero;
                        }

                        return IntPtr.Zero;
                    }
                    else
                    {
                        // Support client drawing only when Dwm is enabled. Otherwise non client drawing is too complicated ;-)
                        return IntPtr.Zero;
                    }
                case WmNchittest:
                    // Hit test non client area
                    var Result = IntPtr.Zero;
                    if (DwmIsCompositionEnabled)
                        handled = DwmApi.DwmDefWindowProc(hWindow, message, wParam, lParam, out Result);
                    if (handled == false)
                    {
                        // hit test for non client areas in the window layout
                        uint Coordinates;
                        checked
                        {
                            Coordinates = (uint) lParam.ToInt32();
                        }

                        var NonClientArea = nonClientHitTest(Coordinates.GetLoUShort(), Coordinates.GetHiUShort());
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
            nonClientAreaDrawingEnabled = enabled;
            UpdateNonClientArea();
        }

        private void UpdateNonClientArea()
        {
            DwmApi.SetWindowPos(WindowHandle, IntPtr.Zero, 0, 0, 0, 0,
                0x27 /*SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED*/);
        }
    }
}
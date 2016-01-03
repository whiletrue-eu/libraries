using System;
using System.Runtime.InteropServices;

namespace WhileTrue.Classes.Win32
{
    /// <summary>
    /// Publishes Vista specific APIs
    /// </summary>
    /// <remarks>
    /// The following Vista APIs are supported:
    /// <list type="bullet">
    /// <item>
    /// <term>Window Manager Compositing: Glass Effect</term>
    /// </item>
    /// </list>
    /// </remarks>
    internal static class DwmApi
    {
        #region Win32 API definition

        [DllImport("dwmapi.dll", PreserveSig = false)]
        private static extern void DwmExtendFrameIntoClientArea(IntPtr windowHandle, ref Margins pMarInset);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        private static extern void DwmEnableBlurBehindWindow(IntPtr windowHandle, ref BlurBehindInformation blurBehindInformation);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        private static extern bool DwmIsCompositionEnabled();

        [DllImport("dwmapi.dll", PreserveSig = true)]
        internal static extern bool DwmDefWindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, out IntPtr plResult);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int w, int h, uint uFlags);



        #region Nested type: BlurBehindFlags

// ReSharper disable UnusedMemberInPrivateClass
        [Flags]
        private enum BlurBehindFlags
        {
            /// <summary>
            /// Indicates a value for enable has been specified.
            /// </summary>
            EnableGiven = 0x00000001,

            /// <summary>
            /// Indicates a value for region has been specified.
            /// </summary>
            RegionGiven = 0x00000002,

            /// <summary>
            /// Indicates a value for transitionOnMaximized has been specified.
            /// </summary>
            FTransitionOnMaximizedGiven = 0x00000004,
        }
// ReSharper restore UnusedMemberInPrivateClass

        #endregion


#pragma warning disable 219

        #region Nested type: BlurBehindInformation

        private struct BlurBehindInformation
        {
            private BlurBehindFlags flags;
            private bool enable;
            private IntPtr region;
            private bool transitionOnMaximized;
#pragma warning restore 219

            public BlurBehindInformation(bool enable, IntPtr region)
            {
                this.flags = BlurBehindFlags.EnableGiven;// | BlurBehindFlags.RegionGiven; // | BlurBehindFlags.fTransitionOnMaximizedGiven;
                this.enable = enable;
                this.region = region;
                this.transitionOnMaximized = false;
            }
        }

        #endregion

        #region Nested type: Margins

        /// <summary/>
        /// <remarks>order must be lrtb</remarks>
        public struct Margins
        {
            public int Left;
            public int Right;
            public int Top;
            public int Bottom;

            /// <summary/>
            public Margins(int all)
            {
                this.Left = all;
                this.Right = all;
                this.Top = all;
                this.Bottom = all;
            }

            /// <summary/>
            public Margins(int left, int right, int top, int bottom)
            {
                this.Left = left;
                this.Right = right;
                this.Top = top;
                this.Bottom = bottom;
            }
        }

        #endregion

//        [DllImport("dwmapi.dll", PreserveSig = false)]
//        private static extern void DwmGetColorizationColor(out int pcrColorization, out bool pfOpaqueBlend );

        #endregion

        #region Delegates

        /// <summary/>
        public delegate void GlassEffectChangedDelegate(bool glassEffectEnabled, object cookie);

        #endregion

        internal static void EnableGlassEffect(IntPtr windowHandle, Margins margins)
        {
            try
            {
                if (DwmApi.DwmIsCompositionEnabled())
                {
                    IntPtr Handle = windowHandle;

                    DwmApi.DwmExtendFrameIntoClientArea(Handle, ref margins);
                    return;
                }
                else
                {
                    //Glass effect is not enabled -> do nothing
                    return;
                }
            }
            catch (DllNotFoundException)
            {
                return;
            }
        }

//        public static Color GetGlassColor()
//        {
//            int Color;
//            bool Opaque;
//
//            DwmGetColorizationColor(out Color, out Opaque);
//
//            return System.Drawing.Color.FromArgb(Color);
//        }


        /// <summary>
        /// Starts the glass effect for the window
        /// </summary>
        /// <param name="windowHandle">handle of the window that shall be affected</param>
        /// <param name="margins">Initial glass margin that is extended into the client area. <c>-1</c> for sheet of glass</param>
        /// <param name="blurClientArea"><c>true</c> to enable blurring of the client area</param>
        /// <param name="nonClientAreaDrawingEnabled">enables drawing on the window border</param>
        /// <param name="notifyGlassChanged">handler that is called if glass rendering is changed in the active theme</param>
        /// <param name="nonClientHitTest">handler that is called when hit-testing is performed on the nonclient frame</param>
        /// <returns>
        /// A helper class that must/can be called from the affected windows implementation 
        /// (see <see cref="DwmWindowHelper"/> methods and properties for details)
        /// </returns>
        public static DwmWindowHelper RegisterForGlassFrame(IntPtr windowHandle, Margins margins, bool blurClientArea, bool nonClientAreaDrawingEnabled, Action notifyGlassChanged, Func<ushort, ushort, NonClientArea> nonClientHitTest)
        {
            DwmWindowHelper Helper = new DwmWindowHelper(windowHandle, margins, blurClientArea, nonClientAreaDrawingEnabled, notifyGlassChanged, nonClientHitTest);
            return Helper;
        }

        public static void EnableBlurBehindWindow(IntPtr windowHandle, bool enable)
        {
            try
            {
                if (DwmApi.DwmIsCompositionEnabled())
                {
                    BlurBehindInformation BlurBehindInformation = new BlurBehindInformation(enable, IntPtr.Zero);
                    DwmApi.DwmEnableBlurBehindWindow(windowHandle, ref BlurBehindInformation);
                    return;
                }
                else
                {
                    //Glass effect is not enabled -> do nothing
                    return;
                }
            }
            catch (DllNotFoundException)
            {
                return;
            }
        }

        public static bool IsCompositionEnabled()
        {
            try
            {
                return DwmApi.DwmIsCompositionEnabled();
            }
            catch (DllNotFoundException)
            {
                return false;
            }
        }
    }
}
using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace WhileTrue.Controls
{
    ///<summary>
    /// WPF Splash Screen
    ///</summary>
    /// <remarks>
    /// The WPF splash screen window is created in a second thread, allowing to be fully responsive even though the main application leads in the main thread and also enabling WPF animations.</remarks>
    public static class SplashScreenEx
    {
        ///<summary>
        /// Shows the given window as splash screen.
        ///</summary>
        /// <remarks>
        /// <para>
        /// If <c>data</c> is set, it is set as datacontext of the splash screen.
        /// </para>
        /// <para>
        /// if <c>splashImage</c> is set (to a resource path within the assembly), the image is taken as splash screen for an intermediate
        /// <see cref="SplashScreen"/> which is shown before the WPF splashcreen is initialized. make sure to use the same image as background
        /// for the WPF splash window, then the WPF  image is blended over the first splash image seamlessly.
        /// This makes it possible to bridge the short time between the simple splash is shown until WPF is initialized.</para>
        /// </remarks>
        public static void Show<TSplashWindow>(object data=null, string splashImage=null) where TSplashWindow:SplashScreenWindow, new()
        {
            System.Windows.SplashScreen SplashScreen=null;
            if (splashImage != null)
            {
                SplashScreen = new System.Windows.SplashScreen(splashImage);
                SplashScreen.Show(false);
            }
            SplashScreenWindow View = null;
            ManualResetEvent SplashScreenReady = new ManualResetEvent(false);
            Thread SplashThread = new Thread((ThreadStart) delegate
                                                               {
                                                                   View = new TSplashWindow();
                                                                   View.Model = data;
                                                                   View.Show();
                                                                   SplashScreenReady.Set();
                                                                   Dispatcher.Run();
                                                               });
            SplashThread.SetApartmentState(ApartmentState.STA);
            SplashThread.IsBackground = true;
            SplashThread.Start();

            SplashScreenReady.WaitOne();

            if (SplashScreen != null)
            {
                SplashScreen.Close(TimeSpan.Zero);
            }
            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Loaded,
                (Action) delegate
                             {
                                 View.Dispatcher.Invoke(DispatcherPriority.Normal, (Action) View.Close);
                             });
        }
    }
}
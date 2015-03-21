using System.Windows;
using System.Windows.Media.Animation;

namespace WhileTrue.Controls.SplashScreen
{
    ///<summary>
    /// Base class for a splash screen
    ///</summary>
    /// <remarks>
    /// <para>
    /// The default control template of the splash screen window will be placed on the whole
    /// screen, placing the resource image that was given to the <see cref="SplashScreenEx"/> class
    /// in the middle. The rest will be left transparent, so only the image is visible.
    /// </para>
    /// <para>
    /// The window will be created in a secondary thread to enable its dispatcher to continue to run
    /// even while the application is loading in the main thread. To animate the splash screen, use the 
    /// <see cref="SplashAnimation"/> property. To animate a fade out of the splash screen, use the
    /// <see cref="EndSplashAnimation"/> property. To fade out smoothly, you should animate the opacity
    /// of the splash screen to reach 0. The splash screen will be hidden once the end animation completes
    /// </para>
    /// </remarks>
    public class SplashScreenWindow : System.Windows.Window
    {
        static SplashScreenWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SplashScreenWindow), new FrameworkPropertyMetadata(typeof(SplashScreenWindow)));
        }

        public SplashScreenWindow()
        {
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Left = 0;
            this.Top = 0;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;

            this.Closing += this.SplashScreenWindow_Closing;
        }

        void SplashScreenWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Closing -= this.SplashScreenWindow_Closing;
            if (this.EndSplashAnimation != null)
            {
                e.Cancel = true;
                this.EndSplashAnimation.Completed += this.EndSplashAnimation_Completed;
                this.EndSplashAnimation.Begin(this);
            }
        }

        public Storyboard SplashAnimation
        {
            get;
            set;
        }

        public Storyboard EndSplashAnimation
        {
            get; set;
        }

        internal object Model
        {
            set { this.DataContext = value; }
        }

        public new void Show()
        {
            if (this.SplashAnimation != null)
            {
                this.SplashAnimation.Begin(this);
            }
            base.Show();
        }

        void EndSplashAnimation_Completed(object sender, System.EventArgs e)
        {
            this.Close();
        }
    }
}
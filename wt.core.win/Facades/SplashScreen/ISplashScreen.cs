using WhileTrue.Classes.Components;
using WhileTrue.Facades.ApplicationLoader;

namespace WhileTrue.Facades.SplashScreen
{
    /// <summary>
    /// Splash screen used in conjunction with <see cref="IApplicationLoader"/>
    /// </summary>
    [ComponentInterface]
    public interface ISplashScreen
    {
        /// <summary>
        /// Show the splash screen
        /// </summary>
        void Show();
        /// <summary>
        /// Hide the splash screen
        /// </summary>
        void Hide();
        /// <summary>
        /// Set the current status as progress message
        /// </summary>
        void SetStatus(int totalNumber, int currentNumber, string name);
    }
}
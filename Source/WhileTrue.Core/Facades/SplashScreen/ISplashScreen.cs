using WhileTrue.Classes.Components;

namespace WhileTrue.Facades.SplashScreen
{
    [ComponentInterface]
    public interface ISplashScreen
    {
        void Show();
        void Hide();
        void SetStatus(int totalNumber, int currentNumber, string name);
    }
}
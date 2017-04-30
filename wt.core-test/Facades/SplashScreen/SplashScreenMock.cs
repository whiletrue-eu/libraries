using System.Collections.Generic;
using WhileTrue.Classes.Components;

namespace WhileTrue.Facades.SplashScreen
{
    [Component]
    internal class SplashScreenMock : ISplashScreen 
    {
        public void Show()
        {
            this.ShowCalled = true;
        }

        public void Hide()
        {
            this.HideCalled = true;
        }

        public void SetStatus(string name)
        {
            this.StatusTexts.Add($"Status: {name}");
        }


        public List<string> StatusTexts { get; } = new List<string>();

        public bool ShowCalled { get; private set; }

        public bool HideCalled { get; private set; }
    }
}
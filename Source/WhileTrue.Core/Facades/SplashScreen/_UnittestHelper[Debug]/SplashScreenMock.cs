using System.Collections.Generic;
using WhileTrue.Classes.Components;

namespace WhileTrue.Facades.SplashScreen._UnittestHelper
{
    [Component]
    internal class SplashScreenMock : ISplashScreen 
    {

        private readonly List<string> statusTexts = new List<string>();
        private bool showCalled;
        private bool hideCalled;


        public void Show()
        {
            this.showCalled = true;
        }

        public void Hide()
        {
            this.hideCalled = true;
        }

        public void SetStatus(int totalNumber, int currentNumber, string name)
        {
            this.statusTexts.Add(string.Format("Status: {0}/{1},{2}", currentNumber, totalNumber, name));
        }


        public List<string> StatusTexts
        {
            get
            {
                return this.statusTexts;
            }
        }

        public bool ShowCalled
        {
            get { return showCalled; }
        }

        public bool HideCalled
        {
            get { return hideCalled; }
        }
    }
}
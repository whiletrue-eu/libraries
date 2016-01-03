using System.Windows.Controls;
using NUnit.Framework;

namespace WhileTrue.Controls
{
    [TestFixture]
    public class DialogWindowTest
    {
        [Test,Ignore("Manual test")]
        public void Test()
        {
            DialogWindow Window = new DialogWindow();
            Window.BeginInit();
            Window.Buttons.Add(new Button{Content = "Close", IsCancel = true});
            Window.Buttons.Add(new Button{Content = "SomethingElse"});

            DialogWindow.SetResult(Window.Buttons[0], "Result");
            Window.Content = new Grid();
            Window.EndInit();

            Window.ShowDialog();

            Assert.AreEqual("Result", Window.ResultValue);
        }

        [Test, Ignore("Manual test")]
        public void Test2()
        {
            DialogWindow Window = new DialogWindowTestWindow();
            Window.ShowDialog();
            Assert.AreEqual("Result", Window.ResultValue);
        }
 
    }
}
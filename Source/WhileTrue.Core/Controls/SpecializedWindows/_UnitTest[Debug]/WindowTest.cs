

using System.Windows.Controls;
using NUnit.Framework;
using WhileTrue.Controls.SpecializedWindows._UnitTest_Debug_;

namespace WhileTrue.Controls
{
    [TestFixture]
    public class WindowTest
    {
        [Test,Ignore("Manual test")]
        public void Test()
        {
            Window Window = new Window();
            Window.BeginInit();
            Window.Content = new TextBlock {Text="Hello, World!"};
            Window.EndInit();

            Window.ShowDialog();
        }

        [Test, Ignore("Manual test")]
        public void Test2()
        {
            Window Window = new WindowTestWindow();
            Window.ShowDialog();
        }
 
    }
}
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

using System.Windows;
using NUnit.Framework;

namespace WhileTrue.Classes.Wpf.Validation
{
    [TestFixture]
    public class ValidationTest
    {

        [Test, Ignore("Manual Test")]
        public void ShowControls()
        {
            Window Window = new Window();
            Window.Content = new ValidationTestControlTree();

            Window.ShowDialog();
        }
    }
}


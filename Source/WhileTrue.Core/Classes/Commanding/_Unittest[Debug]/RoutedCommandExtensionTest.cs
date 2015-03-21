// ReSharper disable InconsistentNaming
using System.Windows.Input;
using NUnit.Framework;
using WhileTrue.Classes.Wpf;

namespace WhileTrue.Classes.Commanding._Unittest
{
    [TestFixture]
    public class RoutedCommandExtensionTest
    {
        [Test]
        public void extension_shall_return_the_routed_Command_provided_by_the_factory()
        {
            RoutedCommandExtension Extension = new RoutedCommandExtension("TheCommand");


            RoutedCommand Command = (RoutedCommand) Extension.ProvideValue(null);


            Assert.AreEqual(RoutedCommandFactory.GetRoutedCommand("TheCommand"), Command);
        }
        
    }
}
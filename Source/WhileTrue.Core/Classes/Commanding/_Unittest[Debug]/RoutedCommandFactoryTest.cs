// ReSharper disable InconsistentNaming
using System.Windows.Input;
using NUnit.Framework;

namespace WhileTrue.Classes.Commanding._Unittest
{
    [TestFixture]
    public class RoutedCommandFactoryTest
    {
        [Test]
        public void factory_shall_return_the_same_command_for_the_same_ID()
        {
            RoutedCommand Command1 = RoutedCommandFactory.GetRoutedCommand("TheCommand");
            RoutedCommand Command2 = RoutedCommandFactory.GetRoutedCommand("TheCommand");

            Assert.AreEqual(Command1, Command2);
        }

        [Test]
        public void factory_shall_return_two_different_commands_for_two_IDs()
        {
            RoutedCommand Command1 = RoutedCommandFactory.GetRoutedCommand("TheCommand");
            RoutedCommand Command2 = RoutedCommandFactory.GetRoutedCommand("AnotherCommand");

            Assert.AreNotEqual(Command1, Command2);
        }
    }
}
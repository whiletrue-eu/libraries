// ReSharper disable InconsistentNaming
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using NUnit.Framework;
using WhileTrue.Classes.UnitTesting;
using WhileTrue.Classes.Utilities;


namespace WhileTrue.Classes.Commanding._Unittest
{
    [TestFixture]
    public class CommandBindingHelperTest
    {

        #region helper classes
        public class DummyCommand : ICommand
        {
            private readonly string name;
            private bool executeCalled;
            private bool canExecuteCalled;

            public DummyCommand(string name)
            {
                this.name = name;
            }

            public void Execute(object parameter)
            {
                this.executeCalled = true;
            }

            public bool CanExecute(object parameter)
            {
                this.canExecuteCalled = true;
                return true;
            }

            public event EventHandler CanExecuteChanged;

            public string Name
            {
                get { return this.name; }
            }

            public bool ExecuteCalled
            {
                get { return executeCalled; }
            }

            public bool CanExecuteCalled
            {
                get { return canExecuteCalled; }
            }

            public override string ToString()
            {
                return this.name;
            }
        }
        #endregion

        [Test]
        public void register_shall_add_given_commands_to_command_binding_collection()
        {
            CommandBindingCollection CommandBindings = new CommandBindingCollection();
            DummyCommand DummyCommand1 = new DummyCommand("Dummy1");
            DummyCommand DummyCommand2 = new DummyCommand("Dummy2");


            CommandBindings.Register(new Dictionary<CommandKey, ICommand> { { DummyCommand1.Name, DummyCommand1 }, { DummyCommand2.Name, DummyCommand2 } });


            Assert.AreEqual(2, CommandBindings.Count);
            Assert.AreEqual(RoutedCommandFactory.GetRoutedCommand(DummyCommand1.Name), CommandBindings[0].Command);
            Assert.AreEqual(DummyCommand1, CommandBindings[0].PrivateMembers().GetField<ICommand>("command"));
            Assert.AreEqual(RoutedCommandFactory.GetRoutedCommand(DummyCommand2.Name), CommandBindings[1].Command);
            Assert.AreEqual(DummyCommand2, CommandBindings[1].PrivateMembers().GetField<ICommand>("command"));
        }

        [Test]
        public void unregister_shall_remove_given_commands_from_command_binding_collection()
        {
            CommandBindingCollection CommandBindings = new CommandBindingCollection();
            DummyCommand DummyCommand1 = new DummyCommand("Dummy1");
            DummyCommand DummyCommand2 = new DummyCommand("Dummy2");
            DummyCommand DummyCommand3 = new DummyCommand("Dummy3");

            CommandBindings.Register(new Dictionary<CommandKey, ICommand>
                                     {
                                         { DummyCommand1.Name, DummyCommand1 }, 
                                         { DummyCommand2.Name, DummyCommand2 }, 
                                         { DummyCommand3.Name, DummyCommand3 }
                                     });


            CommandBindings.Unregister(new Dictionary<CommandKey, ICommand>
                                     {
                                         { DummyCommand1.Name, DummyCommand1 }, 
                                         { DummyCommand3.Name, DummyCommand3 }
                                     });


            Assert.AreEqual(1, CommandBindings.Count);
            Assert.AreEqual(RoutedCommandFactory.GetRoutedCommand(DummyCommand2.Name), CommandBindings[0].Command);
            Assert.AreEqual(DummyCommand2, CommandBindings[0].PrivateMembers().GetField<ICommand>("command"));
        }

        [Test]
        public void events_shall_be_delegated_within_command_bindings()
        {
            DummyCommand DummyCommand1 = new DummyCommand("Dummy1");

            Button Button = new Button();

            Window Window = new Window();
            Window.Content = Button;

            Window.CommandBindings.Register(new Dictionary<CommandKey, ICommand>
                                            {
                                                {DummyCommand1.Name, DummyCommand1}
                                            });

            Button.Command = RoutedCommandFactory.GetRoutedCommand(DummyCommand1.Name);

            AutomationPeer ButtonPeer = UIElementAutomationPeer.CreatePeerForElement(Button);
            IInvokeProvider InvokeProvider = (IInvokeProvider) ButtonPeer.GetPattern(PatternInterface.Invoke);
            Debug.Assert(InvokeProvider != null);

            Window.Loaded += delegate
                                    {
                                        InvokeProvider.Invoke();
                                        Window.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action) Window.Close);
                                    };
            Window.ShowDialog();

            Assert.IsTrue(DummyCommand1.CanExecuteCalled);
            Assert.IsTrue(DummyCommand1.ExecuteCalled);
        }
    }
}
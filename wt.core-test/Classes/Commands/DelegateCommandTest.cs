#pragma warning disable 1591
// ReSharper disable AccessToModifiedClosure
// ReSharper disable InconsistentNaming
using System;
using System.Windows.Input;
using NUnit.Framework;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Classes.Commands
{
    [TestFixture]
    public class DelegateCommandTest
    {
        [Test]
        public void command_with_no_canexecute_delegate_shall_be_executable()
        {
            bool ActionCalled = false;
            ICommand Command = new DelegateCommand(() => ActionCalled = true);
            Command.Execute(null);
            
            Assert.IsTrue(Command.CanExecute(null));
            Assert.IsTrue(ActionCalled);
        }

        [Test]
        public void command_with_no_parameter_shall_ignore_the_given_parameter()
        {
            ICommand Command = new DelegateCommand(() => {});
            Command.Execute(42);
            Assert.IsTrue(Command.CanExecute(42));
        }

        [Test]
        public void command_shall_be_enabled_or_disabled_by_delegate_result()
        {
            bool CanExecute = false;

            ICommand Command = new DelegateCommand(() => {}, () => CanExecute);
            
            Assert.IsFalse(Command.CanExecute(null));

            CanExecute = true;
            Assert.IsTrue(Command.CanExecute(null));
        }

        private class TestObject:ObservableObject
    {
            private bool property;
            public bool Property
            {
                get { return this.property; }
                set { this.SetAndInvoke(nameof(this.Property), ref this.property, value); }
            }
    }

        [Test]
        public void can_execute_changed_shall_be_called_on_change_of_notifyExpression()
        {
            bool CanExecuteChanged = false;
            bool ExecuteCalled = false;
            TestObject ObservableObject = new TestObject();

            ICommand Command = new DelegateCommand(() => { ExecuteCalled = true; }, () => ObservableObject.Property);
            Command.CanExecuteChanged += delegate { CanExecuteChanged = true; };

            Assert.IsFalse(Command.CanExecute(null));

            ObservableObject.Property = true;

            Assert.IsTrue(CanExecuteChanged);
            Assert.IsTrue(Command.CanExecute(null));

            Command.Execute(null);
            Assert.IsTrue(ExecuteCalled);
        }

        [Test]
        public void command_with_parameter_shall_forward_and_cast_the_command()
        {
            bool ParameterValue = false;

            ICommand Command = new DelegateCommand<bool>( parameter => ParameterValue = parameter);
            Command.Execute(true);

            Assert.IsTrue(ParameterValue);
        }

        [Test]
        public void command_with_parameter_shall_forward_the_default_value_on_null_parameter_if_type_is_valuetype()
        {
            int ParameterValue = 42;

            ICommand Command = new DelegateCommand<int>(parameter => ParameterValue = parameter);
            Command.Execute(null);

            Assert.AreEqual(0, ParameterValue);
        }

        [Test]
        public void exception_shall_be_thrown_if_parameter_type_mismatches()
        {
            ICommand Command = new DelegateCommand<int>(_=>{});

            Assert.Throws<InvalidCastException>(()=>Command.Execute(true));
        }
    }
}
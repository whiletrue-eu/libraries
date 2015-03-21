#pragma warning disable 1591
// ReSharper disable InconsistentNaming
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;
using NUnit.Framework;

namespace WhileTrue.Classes.Wpf
{
    [TestFixture]
    public class CommandWrapperTest
    {
        [Test]
        public void InvalidateSuggestRequery_shall_be_transferred_to_the_UI_thread_if_called_on_another_thread()
        {
            Dispatcher SimulatedUIDispatcher = null;
            ManualResetEvent RegistrationWaitEvent = new ManualResetEvent(false);
            ManualResetEvent WaitEvent = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem(delegate
                                         {
                                             SimulatedUIDispatcher = Dispatcher.CurrentDispatcher;
                                             TestCommand TestCommand = new TestCommand();
                                             ICommand Wrapper = (ICommand) new CrossThreadCommandWrapper().Convert(TestCommand, typeof (ICommand), null, CultureInfo.InvariantCulture);
                                             SimulatedUIDispatcher.BeginInvoke(new Action(() => RegistrationWaitEvent.Set()));
                                             Dispatcher.Run();
                                             WaitEvent.Set();
                                         });
            RegistrationWaitEvent.WaitOne();

            List<int> RequerySuggestedCalledFromThreadIDs = new List<int>();
            EventHandler RequerySuggested = delegate
                                            {
                                                RequerySuggestedCalledFromThreadIDs.Add(Thread.CurrentThread.ManagedThreadId);
                                                SimulatedUIDispatcher.Invoke(DispatcherPriority.SystemIdle,
                                                    (Action) (() => SimulatedUIDispatcher.InvokeShutdown()));
                                                Dispatcher.CurrentDispatcher.InvokeShutdown();
                                            };

            CommandManager.RequerySuggested += RequerySuggested;

            CommandManager.InvalidateRequerySuggested();
            Dispatcher.Run();
            WaitEvent.WaitOne();
            Assert.That(RequerySuggestedCalledFromThreadIDs, Contains.Item(Thread.CurrentThread.ManagedThreadId));
        }

        public class TestCommand : ICommand
        {
            #region Implementation of ICommand

            public void Execute(object parameter)
            {
                this.ExecuteParameter = parameter;
                this.ExecuteCalled = true;
            }

            public bool ExecuteCalled { get; set; }
            public object ExecuteParameter { get; set; }

            public bool CanExecute(object parameter)
            {
                this.CanExecuteParameter = parameter;
                this.CanExecuteCalled = true;
                return this.CanExecuteReturnValue;
            }

            public bool CanExecuteCalled { get; set; }
            public bool CanExecuteReturnValue { get; set; }
            public object CanExecuteParameter { get; set; }

            public event EventHandler CanExecuteChanged = delegate{};

            public void InvokeCanExecuteChanged (object sender=null, EventArgs e=null)
            {
                this.CanExecuteChanged(sender, e??EventArgs.Empty);
            }

            #endregion
        }
    }


}
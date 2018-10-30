using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Logging;

namespace WhileTrue.Classes.Commands
{
    /// <summary>
    ///     Provides a class to implement ICommand interface with the use of delegates
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This class exists in four versions. One of it is generic.
    ///         If using the generic version, the type given represents the type of the parameter
    ///         expected, which will be automatically casted prior to the call of the delegates.
    ///         You can use the second one if you don#t have a parameter. The command parameter then will be ignored.
    ///         Additonally, both versions are available for async command implementations
    ///     </para>
    ///     <para>
    ///         Calls to the delegates are dispatched into the thread the delegatecommand was created in.
    ///     </para>
    /// </remarks>
    public abstract class DelegateCommandBase<TParameterType> : ObservableObject, ICommand
    {
        private readonly Func<TParameterType, bool> canExecuteDelegate;
        private readonly NotifyChangeExpression<Func<TParameterType, bool>> canExecuteDelegateExpression;

        /// <summary>
        ///     handler to be called if an exception is thrown in execution of the delegated method
        /// </summary>
        protected readonly Action<Exception> ExceptionHandler;

        private readonly string name;

        private bool canExecute;

        // ReSharper disable once NotAccessedField.Local - see comment at usage below
        private EventHandler requerySuggestedEventHandler;


        /// <summary />
        protected DelegateCommandBase(Expression<Func<TParameterType, bool>> canExecuteExpression, string name = null,
            Action<Exception> exceptionHandler = null)
        {
            canExecuteDelegateExpression = new NotifyChangeExpression<Func<TParameterType, bool>>(canExecuteExpression);
            this.name = name;
            ExceptionHandler = exceptionHandler;
            canExecuteDelegate = canExecuteDelegateExpression.Invoke;

            AttachRequerySuggestedEvent();
            canExecuteDelegateExpression.Changed += (_, __) => InvokeCanExecuteChanged();
        }

        /// <summary>
        ///     Reflects the last result of the <see cref="ICommand.CanExecute" /> method
        /// </summary>
        public bool CanExecute
        {
            get => canExecute;
            private set => SetAndInvoke(ref canExecute, value);
        }


        /// <summary>
        ///     <see cref="ICommand.Execute" />
        /// </summary>
        public abstract void Execute(object parameter);

        /// <summary>
        ///     <see cref="ICommand.CanExecuteChanged" />
        /// </summary>
        public event EventHandler CanExecuteChanged = delegate { };


        /// <summary>
        ///     <see cref="ICommand.CanExecute" />
        /// </summary>
        bool ICommand.CanExecute(object parameter)
        {
            try
            {
                var CanExecute = canExecuteDelegate((TParameterType) (parameter ?? default(TParameterType)));
                DebugLogger.WriteLine(this, LoggingLevel.Verbose,
                    () =>
                        $"DelegateCommand '{name ?? "<unset>"}' Value queried for CanExecute (with '{parameter}' param): '{CanExecute}'");
                this.CanExecute = CanExecute;
                return CanExecute;
            }
            catch (Exception Exception)
            {
                if (ExceptionHandler != null)
                {
                    ExceptionHandler(Exception);
                    CanExecute = false;
                    return false;
                }

                throw;
            }
        }

        private void AttachRequerySuggestedEvent()
        {
            // We have to save the handler in this class, as the CommandManager.RequerySuggested event
            // is realized as a list of weak references to the event handlers, so it would be removed
            // immediately if it is not saved here.
            requerySuggestedEventHandler = delegate { NotifyRequerySuggested(); };
        }

        private void NotifyRequerySuggested()
        {
            CanExecuteChanged(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Fires the CanExecuteChanged event on the command
        /// </summary>
        private void InvokeCanExecuteChanged()
        {
            DebugLogger.WriteLine(this, LoggingLevel.Normal,
                () => $"DelegateCommand '{name ?? "<unset>"}' CanExecuteChanged fired.");
            DebugLogger.WriteLine(this, LoggingLevel.Verbose, () =>
            {
                bool? CanExecuteValue;
                try
                {
                    CanExecuteValue = ((ICommand) this).CanExecute(null);
                }
                catch
                {
                    CanExecuteValue = null;
                }

                return
                    $"DelegateCommand '{name ?? "<unset>"}' New value for CanExecute (with <null> param): '{(CanExecuteValue.HasValue ? CanExecuteValue.Value.ToString() : "<exception occurred>")}'";
            });
            CanExecuteChanged(this, EventArgs.Empty);
        }
    }

    /// <summary>
    ///     Provides a class to implement ICommand interface with the use of delegates
    /// </summary>
    /// <remarks>
    ///     This class exists in four versions. One of it is generic.
    ///     If using the generic version, the type given represents the type of the parameter
    ///     expected, which will be automatically casted prior to the call of the delegates.
    ///     You can use the second one if you don#t have a parameter. The command parameter then will be ignored.
    ///     Additonally, both versions are available for async command implementations
    /// </remarks>
    /// <summary>
    ///     Provides a class to implement ICommand interface with the use of delegates
    /// </summary>
    /// <remarks>
    ///     This class exists in four versions. One of it is generic.
    ///     If using the generic version, the type given represents the type of the parameter
    ///     expected, which will be automatically casted prior to the call of the delegates.
    ///     You can use the second one if you don#t have a parameter. The command parameter then will be ignored.
    ///     Additonally, both versions are available for async command implementations
    /// </remarks>
    public sealed class DelegateCommand : DelegateCommand<object>
    {
        /// <summary>
        ///     Implements a command which is always executable
        /// </summary>
        public DelegateCommand(Action executeDelegate, string name = null, Action<Exception> exceptionHandler = null)
            : this(executeDelegate, () => true, name, exceptionHandler)
        {
        }

        /// <summary>
        ///     Implements a command which executable state is retrieved using the second delegate and supports automatic
        ///     updation of the executable state
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The canExecuteDelegate is automatically parsed for changes of the properties called
        ///         (providing the objects which are used implement <see cref="INotifyPropertyChanged" /> and/or
        ///         <see cref="INotifyCollectionChanged" />)
        ///     </para>
        ///     <para>
        ///         This is most useful for commands which executable state changes asynchronuously which is not
        ///         detected using the default WPF executable state polling.
        ///     </para>
        /// </remarks>
        public DelegateCommand(Action executeDelegate, Expression<Func<bool>> canExecuteExpression, string name = null,
            Action<Exception> exceptionHandler = null)
            : base(_ => executeDelegate(),
                (Expression<Func<object, bool>>) Expression.Lambda(canExecuteExpression.Body,
                    Expression.Parameter(typeof(object), "_")), name, exceptionHandler)
        {
        }
    }

    /// <summary>
    ///     Provides a class to implement ICommand interface with the use of delegates
    /// </summary>
    /// <remarks>
    ///     This class exists in four versions. One of it is generic.
    ///     If using the generic version, the type given represents the type of the parameter
    ///     expected, which will be automatically casted prior to the call of the delegates.
    ///     You can use the second one if you don#t have a parameter. The command parameter then will be ignored.
    ///     Additonally, both versions are available for async command implementations
    /// </remarks>
    public class DelegateCommand<T> : DelegateCommandBase<T>
    {
        private readonly Action<T> executeDelegate;

        /// <summary>
        ///     Implements a command which is always executable
        /// </summary>
        public DelegateCommand(Action<T> executeDelegate, string name = null, Action<Exception> exceptionHandler = null)
            : this(executeDelegate, _ => true, name, exceptionHandler)
        {
        }

        /// <summary>
        ///     Implements a command which executable state is retrieved using the second delegate and supports automatic
        ///     updation of the executable state
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The canExecuteDelegate is automatically parsed for changes of the properties called
        ///         (providing the objects which are used implement <see cref="INotifyPropertyChanged" /> and/or
        ///         <see cref="INotifyCollectionChanged" />)
        ///     </para>
        ///     <para>
        ///         This is most useful for commands which executable state changes asynchronuously which is not
        ///         detected using the default WPF executable state polling.
        ///     </para>
        /// </remarks>
        public DelegateCommand(Action<T> executeDelegate, Expression<Func<T, bool>> canExecuteExpression,
            string name = null, Action<Exception> exceptionHandler = null)
            : base(canExecuteExpression, name, exceptionHandler)
        {
            this.executeDelegate = executeDelegate;
        }

        /// <summary>
        ///     <see cref="ICommand.Execute" />
        /// </summary>
        public override void Execute(object parameter)
        {
            try
            {
                executeDelegate((T) (parameter ?? default(T)));
            }
            catch (Exception Exception)
            {
                if (ExceptionHandler != null)
                    ExceptionHandler(Exception);
                else
                    throw;
            }
        }
    }


    /// <summary>
    ///     Provides a class to implement ICommand interface with the use of delegates
    /// </summary>
    /// <remarks>
    ///     This class exists in four versions. One of it is generic.
    ///     If using the generic version, the type given represents the type of the parameter
    ///     expected, which will be automatically casted prior to the call of the delegates.
    ///     You can use the second one if you don#t have a parameter. The command parameter then will be ignored.
    ///     Additonally, both versions are available for async command implementations
    /// </remarks>
    public sealed class AsyncDelegateCommand : AsyncDelegateCommand<object>
    {
        /// <summary>
        ///     Implements a command which is always executable
        /// </summary>
        public AsyncDelegateCommand(Func<Task> executeDelegate, string name = null,
            Action<Exception> exceptionHandler = null)
            : this(executeDelegate, () => true, name, exceptionHandler)
        {
        }

        /// <summary>
        ///     Implements a command which executable state is retrieved using the second delegate and supports automatic
        ///     updation of the executable state
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The canExecuteDelegate is automatically parsed for changes of the properties called
        ///         (providing the objects which are used implement <see cref="INotifyPropertyChanged" /> and/or
        ///         <see cref="INotifyCollectionChanged" />)
        ///     </para>
        ///     <para>
        ///         This is most useful for commands which executable state changes asynchronuously which is not
        ///         detected using the default WPF executable state polling.
        ///     </para>
        /// </remarks>
        public AsyncDelegateCommand(Func<Task> executeDelegate, Expression<Func<bool>> canExecuteExpression,
            string name = null, Action<Exception> exceptionHandler = null)
            : base(async _ => await executeDelegate(),
                (Expression<Func<object, bool>>) Expression.Lambda(canExecuteExpression.Body,
                    Expression.Parameter(typeof(object), "_")), name, exceptionHandler)
        {
        }
    }

    /// <summary>
    ///     Provides a class to implement ICommand interface with the use of delegates
    /// </summary>
    /// <remarks>
    ///     This class exists in four versions. One of it is generic.
    ///     If using the generic version, the type given represents the type of the parameter
    ///     expected, which will be automatically casted prior to the call of the delegates.
    ///     You can use the second one if you don#t have a parameter. The command parameter then will be ignored.
    ///     Additonally, both versions are available for async command implementations
    /// </remarks>
    public class AsyncDelegateCommand<T> : DelegateCommandBase<T>
    {
        private readonly Func<T, Task> executeDelegate;

        private readonly ReaderWriterLockSlim executeLock =
            new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private bool isExecuting;

        /// <summary>
        ///     Implements a command which is always executable
        /// </summary>
        public AsyncDelegateCommand(Func<T, Task> executeDelegate, string name = null,
            Action<Exception> exceptionHandler = null)
            : this(executeDelegate, _ => true, name, exceptionHandler)
        {
        }

        /// <summary>
        ///     Implements a command which executable state is retrieved using the second delegate and supports automatic
        ///     updation of the executable state
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The canExecuteDelegate is automatically parsed for changes of the properties called
        ///         (providing the objects which are used implement <see cref="INotifyPropertyChanged" /> and/or
        ///         <see cref="INotifyCollectionChanged" />)
        ///     </para>
        ///     <para>
        ///         This is most useful for commands which executable state changes asynchronuously which is not
        ///         detected using the default WPF executable state polling.
        ///     </para>
        /// </remarks>
        public AsyncDelegateCommand(Func<T, Task> executeDelegate, Expression<Func<T, bool>> canExecuteExpression,
            string name = null, Action<Exception> exceptionHandler = null)
            : base(canExecuteExpression, name, exceptionHandler)
        {
            this.executeDelegate = executeDelegate;
        }

        /// <summary>
        ///     <see cref="ICommand.Execute" />
        /// </summary>
        public override async void Execute(object parameter)
        {
            if (executeLock.TryEnterWriteLock(0))
            {
                if (isExecuting == false)
                {
                    isExecuting = true;
                    try
                    {
                        await executeDelegate((T) (parameter ?? default(T)));
                    }
                    catch (Exception Exception)
                    {
                        if (ExceptionHandler != null)
                            ExceptionHandler(Exception);
                        else
                            throw;
                    }
                    finally
                    {
                        isExecuting = false;
                        executeLock.ExitWriteLock();
                    }
                }
            }
        }
    }
}
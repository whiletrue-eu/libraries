using System;
using System.Threading;
using JetBrains.Annotations;

namespace WhileTrue.Classes.Utilities
{
    /// <summary>
    /// Implements a base class for easy implementation of worker threads.
    /// </summary>
    [PublicAPI]
    public abstract class ThreadBase
    {
        private readonly ManualResetEvent initialised = new ManualResetEvent(false);
        private readonly bool isBackgroundThread;
        private readonly ThreadPriority priority;
        private Thread thread;
        private readonly ApartmentState apartmentState;
        private Exception exception;

        /// <summary>
        /// Constructs a thread with a name given in the name parameter.
        /// The name is e.g. displayed in the Debugger
        /// </summary>
        /// <param name="name">Name of the thread</param>
        /// <param name="isBackgroundThread">
        ///     set to <c>true</c> if the thread shall be creeated as background thread
        ///     (i.e. does not prevent the process from terminating if it still runs)
        /// </param>
        /// <param name="priority">Priority of the thread</param>
        protected ThreadBase(string name = null, bool isBackgroundThread = false, ThreadPriority priority = ThreadPriority.Normal)
        {
            this.Name = name ?? this.GetType().FullName;
            this.priority = priority;
            this.isBackgroundThread = isBackgroundThread;
            this.apartmentState = ApartmentState.STA;
        }

        /// <summary>
        /// Returns whether or not the thread was started. 'started' in this case means 
        /// running, suspended or waiting. If the return value is 'true', check
        /// <see cref="IsWaiting()"/>
        /// to see, whether the thread is running or waiting.
        /// </summary>
        /// <returns>Returns whether or not the thread is running</returns>
        public bool IsAlive => (this.thread != null) && (this.thread.IsAlive);

        /// <summary>
        /// Returns whether or not the thread is stopped.
        /// </summary>
        /// <returns>Returns whether or not the thread is stopped</returns>
        public bool IsStopped => (this.thread == null) || (! this.thread.IsAlive);

        /// <summary>
        /// Returns whether or not the thread is waiting, either for another object or for
        /// the end of a given time interval via the <see cref="Sleep(int)"/> method.
        /// </summary>
        /// <returns>Returns whether or not the thread is waiting</returns>
        public bool IsWaiting => (this.thread != null) && ((this.thread.ThreadState & ThreadState.WaitSleepJoin) == ThreadState.WaitSleepJoin);

        /// <summary>
        /// Returns the name of the thread that was set in the constructor
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes and starts the thread. A new thread is created and control is given
        /// to the 'Run' method that is implemented in the derived class
        /// </summary>
        /// <exception cref="Exception">A MAP Exception is thrown, if the thread was already started</exception>
        // ReSharper disable once VirtualMemberNeverOverriden.Global
        public virtual void Start()
        {
            if (this.IsAlive)
            {
                throw new Exception("Thread " + this.Name + " is already running");
            }

            this.thread = new Thread(this.InternalRun)
                              {
                                  Name = this.Name, 
                                  Priority = this.priority, 
                                  IsBackground = this.isBackgroundThread,
                              };
            this.thread.SetApartmentState(this.apartmentState);

            this.thread.Start();
        }

        /// <summary>
        /// Starts the thread and waits until the thread has initialised itsself.
        /// </summary>
        public void StartAndWaitForInitialisation()
        {
            this.Start();
            this.WaitForInitialisation();
        }

        /// <summary>
        /// Aborts the currently running thread. If the thread is suspended,
        /// it is resumed before it is aborted for technical reasons
        /// </summary>
        /// <remarks>
        /// The method does not return immediately but waits for the thread to stop.
        /// If you need unblocking behaviour, call <see cref="BeginStop"/> instead
        /// </remarks>
        public virtual void Stop()
        {
            if (! this.IsAlive)
            {
                throw new Exception("Thread " + this.Name + " is not running");
            }

            this.thread.Abort();
            this.thread.Join(); //Wait for thread to die
        }

        /// <summary>
        /// Aborts the currently running thread. If the thread is suspended,
        /// it is resumed before it is aborted for technical reasons
        /// </summary>
        /// <remarks>
        /// The method does return immediately and does not wait for the thread to stop.
        /// If you need this behaviour, call <see cref="Stop"/> instead
        /// </remarks>
        public void BeginStop()
        {
            if (! this.IsAlive)
            {
                throw new Exception("Thread " + this.Name + " is not running");
            }

            this.thread.Abort();
        }


        /// <summary>
        /// used internally to launch the thread
        /// </summary>
        private void InternalRun()
        {
            try
            {  
                this.Initialise();
                this.initialised.Set();

                this.Run();
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
            }
            catch(Exception Exception)
            {
                this.exception = Exception;
            }
            this.Uninitialise();
        }


        /// <summary>
        /// Must be implemented in dervied classes. The run method provides the main 
        /// functionality of the thread. It is called when the thread is started
        /// via the <see cref="Start()"/> method.
        /// </summary>
        protected abstract void Run();

        /// <summary>
        /// Can be implemented, if the thread must do initialisation before it may be accessed.
        /// The threads client can wait for the thread to initialise itself by calling the
        /// <see cref="WaitForInitialisation"/> method.
        /// </summary>
        // ReSharper disable once VirtualMemberNeverOverriden.Global
        protected virtual void Initialise()
        {
        }

        /// <summary>
        /// Can be implemented, if the thread must do uninitialisation after it was stopped.
        /// </summary>
        // ReSharper disable once VirtualMemberNeverOverriden.Global
        protected virtual void Uninitialise()
        {
        }


        /// <summary>
        /// Lets the thread sleep for the given time interval in milliseconds.
        /// </summary>
        /// <param name="timeout">Timeout to wait for</param>
        protected void Sleep(int timeout)
        {
            if (Thread.CurrentThread != this.thread)
            {
                throw new InvalidOperationException("Sleep() must be called from the thread managed by this instance");
            }
            Thread.Sleep(timeout);
        }

        /// <summary>
        /// Lets the thread sleep for the given time interval.
        /// </summary>
        /// <param name="timeout">Timeout to wait for</param>
        protected void Sleep(TimeSpan timeout)
        {
            if (Thread.CurrentThread != this.thread)
            {
                throw new InvalidOperationException("Sleep() must be called from the thread managed by this instance");
            }
            Thread.Sleep(timeout);
        }

        /// <summary>
        /// Waits until the thread is initialised
        /// </summary>
        public void WaitForInitialisation()
        {
            this.initialised.WaitOne();
        }

        /// <summary>
        /// Waits for the thread to end, swallowing exceptions that
        /// </summary>
        public void Join()
        {
            this.thread.Join();
        }

        /// <summary>
        /// Waits until the thread ends and rethrows any exception that occured in the thread, if any
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void JoinAndRethrowIfExceptionOccured()
        {
            this.Join();
            if( this.exception != null )
            {
                throw this.exception;
            }
        }
    }
}
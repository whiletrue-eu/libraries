using System;
using System.Threading;
using JetBrains.Annotations;

namespace WhileTrue.Classes.Utilities
{
    /// <summary>
    ///     Provides funcitonality for unit tests to wait for thread pool tasks to finish. Just create a using section around
    ///     the test with this class and then call <see cref="Wait" />.
    /// </summary>
    [PublicAPI]
    public class ThreadPoolWaiter : IDisposable
    {
        private readonly int minCompletionPortThreads;
        private readonly int minWorkerThreads;
        private readonly ManualResetEvent threadPoolReady = new ManualResetEvent(false);
        private readonly ManualResetEvent threadWaiterDispose = new ManualResetEvent(false);

        /// <summary />
        public ThreadPoolWaiter()
        {
            Monitor.Enter(typeof(ThreadPoolWaiter)); //Serialize usage of thread pool waiter

            ThreadPool.GetMinThreads(out minWorkerThreads, out minCompletionPortThreads);

            //Set to minimum number of threads in the pool possible. Will not be below number of physical processors!
            ThreadPool.SetMaxThreads(minWorkerThreads, minCompletionPortThreads);

            // Block all but one thread pool thread. Otherwise we cannot wait for the Thrad Pool tasks to be finished by just waiting for another pooled task at the end of the queue
            int WorkerThreads;
            int IoThreads;
            ThreadPool.GetAvailableThreads(out WorkerThreads, out IoThreads);
            while (WorkerThreads > 1)
            {
                ThreadPool.QueueUserWorkItem(delegate { BlockThreadInThreadPool(); });
                ThreadPool.GetAvailableThreads(out WorkerThreads, out IoThreads);
            }
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Release waiting thread pool threads
            threadWaiterDispose.Set();

            ThreadPool.SetMinThreads(minWorkerThreads, minCompletionPortThreads);

            Monitor.Exit(typeof(ThreadPoolWaiter)); //Serialize usage of thread pool waiter
        }

        private void BlockThreadInThreadPool()
        {
            threadWaiterDispose.WaitOne();
        }

        /// <summary>
        ///     Will wait until all current thread pool tasks are executed and then will return.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     thrown if a timeout value is set and the operation did not execute in the
        ///     given amount of time
        /// </exception>
        /// >
        public void Wait(int timeout = Timeout.Infinite)
        {
            ThreadPool.QueueUserWorkItem(delegate { threadPoolReady.Set(); });
            var Signalled = threadPoolReady.WaitOne(timeout);
            if (Signalled == false)
                throw new InvalidOperationException(
                    "Thread pool did not execute the operation in the given timeout period");
            threadPoolReady.Reset();
        }
    }
}
using System.Threading;

namespace WhileTrue.Classes.Utilities
{
    public static class ThreadPoolEx
    {
        public static bool QueueUserWorkItem( string name, WaitCallback callBack, object state)
        {
            return ThreadPool.QueueUserWorkItem(delegate(object stateObject)
                                                    {
                                                            Thread.CurrentThread.Name = string.Format("Worker thread - {0}", name);
                                                            callBack(stateObject);
                                                    }, state);
        }

        public static bool QueueUserWorkItem(string name, WaitCallback callBack)
        {
            return ThreadPool.QueueUserWorkItem(delegate(object state)
                                                    {
                                                        Thread.CurrentThread.Name = string.Format("Worker thread - {0}", name);
                                                        callBack(state);
                                                    });
        }
    }
}
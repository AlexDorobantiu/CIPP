using System;
using System.Collections.Generic;
using System.Threading;

using CIPPProtocols;

namespace CIPPServer
{
    public class TaskWorkerThread : IDisposable
    {
        public readonly Queue<Task> taskSource;
        public readonly ConnectionThread parentConnectionThread;
        private bool isPendingClosure = false;

        private readonly EventWaitHandle eventWaitHandleBetweenTasks = new AutoResetEvent(false);
        private readonly Thread thread;

        public TaskWorkerThread(ConnectionThread parent, string threadName)
        {
            parentConnectionThread = parent;
            taskSource = parent.taskBuffer;

            thread = new Thread(doWork)
            {
                Name = threadName
            };
            thread.Start();
        }

        public void Awake()
        {
            eventWaitHandleBetweenTasks.Set();
        }

        public void AbortCurrentTask()
        {
            thread.Abort();
        }

        public void Kill()
        {
            isPendingClosure = true;
            eventWaitHandleBetweenTasks.Set();
            thread.Join();
            eventWaitHandleBetweenTasks.Close();
        }

        private void doWork()
        {
            try
            {
                while (true)
                {
                    try
                    {
                        if (isPendingClosure)
                        {
                            return;
                        }

                        Task task = null;
                        lock (taskSource)
                        {
                            if (taskSource.Count > 0)
                            {
                                task = taskSource.Dequeue();
                            }
                        }

                        if (task == null)
                        {
                            eventWaitHandleBetweenTasks.WaitOne();
                        }
                        else
                        {
                            Console.WriteLine("Working on " + task.type.ToString() + " task " + task.id);
                            TaskHelper.solveTask(task, Program.pluginFinder);
                            object result = task.getResult();

                            parentConnectionThread.sendResult(task.id, result);
                            parentConnectionThread.sendTaskRequest();
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        Thread.ResetAbort();
                    }
                }
            }
            catch (ThreadAbortException)
            {
                Console.WriteLine("The thread abortion process occured at a time when it could not be properly handled.");
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Kill();
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}

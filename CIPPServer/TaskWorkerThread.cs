using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Reflection;

using CIPPProtocols;
using CIPPProtocols.Plugin;
using ProcessingImageSDK;
using Plugins.Filters;
using Plugins.Masks;
using Plugins.MotionRecognition;
using CIPPProtocols.Tasks;

namespace CIPPServer
{
    public class TaskWorkerThread : IDisposable
    {
        public readonly Queue<Task> taskSource;
        public readonly ConnectionThread parentConnectionThread;
        private bool isPendingClosure = false;

        private readonly EventWaitHandle eventWaitHandleBetweenTasks = new AutoResetEvent(false);
        private Thread thread;

        public TaskWorkerThread(ConnectionThread parent, string threadName)
        {
            parentConnectionThread = parent;
            taskSource = parent.taskBuffer;

            thread = new Thread(doWork);
            thread.Name = threadName;
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

        public void Dispose()
        {
            Kill();
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
    }
}

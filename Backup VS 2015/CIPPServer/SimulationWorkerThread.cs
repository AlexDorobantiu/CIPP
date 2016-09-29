using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Reflection;

using CIPPProtocols;
using ProcessingImageSDK;
using Plugins.Filters;
using Plugins.Masks;
using Plugins.MotionRecognition;

namespace CIPPServer
{
    public class SimulationWorkerThread : IDisposable
    {
        public readonly Queue<Task> taskSource;
        public readonly ConnectionThread parentConnectionThread;
        private bool isPendingClosure = false;

        private EventWaitHandle wh_between_tasks = new AutoResetEvent(false);
        private Thread thread;

        public SimulationWorkerThread(ConnectionThread parent, string threadName)
        {
            parentConnectionThread = parent;
            taskSource = parent.taskBuffer;

            wh_between_tasks = new AutoResetEvent(false);
            thread = new Thread(doWork);
            thread.Name = threadName;
            thread.Start();
        }

        public void Awake()
        {
            wh_between_tasks.Set();
        }

        public void AbortCurrentTask()
        {
            thread.Abort();
        }

        public void Kill()
        {
            isPendingClosure = true;
            wh_between_tasks.Set();
            thread.Join();
            wh_between_tasks.Close();
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
                        if (isPendingClosure) return;

                        Task task = null;
                        lock (taskSource)
                        {
                            if (taskSource.Count > 0) task = taskSource.Dequeue();
                        }

                        if (task == null) wh_between_tasks.WaitOne();
                        else
                        {
                            Console.WriteLine("Working on " + task.taskType.ToString() + " task " + task.id);

                            object result = null;
                            try
                            {
                                if (task.taskType == TaskType.filter)
                                {
                                    FilterTask f = (FilterTask)task;

                                    PluginInfo pi = null;
                                    foreach (PluginInfo plugIn in Program.filterPluginList)
                                        if (plugIn.fullName == f.pluginFullName)
                                        {
                                            pi = plugIn;
                                            break;
                                        }

                                    IFilter filter = (IFilter)pi.assembly.CreateInstance(pi.fullName, false, BindingFlags.CreateInstance, null, f.parameters, null, null);
                                    result = filter.filter(f.originalImage);
                                }
                                else
                                    if (task.taskType == TaskType.mask)
                                    {
                                        MaskTask m = (MaskTask)task;

                                        PluginInfo pi = null;
                                        foreach (PluginInfo plugIn in Program.maskPluginList)
                                            if (plugIn.fullName == m.pluginFullName)
                                            {
                                                pi = plugIn;
                                                break;
                                            }

                                        IMask mask = (IMask)pi.assembly.CreateInstance(pi.fullName, false, BindingFlags.CreateInstance, null, m.parameters, null, null);
                                        result = mask.mask(m.originalImage);
                                    }
                                    else
                                        if (task.taskType == TaskType.motionRecognition)
                                        {
                                            MotionRecognitionTask m = (MotionRecognitionTask)task;

                                            PluginInfo pi = null;
                                            foreach (PluginInfo plugIn in Program.motionRecognitionPluginList)
                                                if (plugIn.fullName == m.pluginFullName)
                                                {
                                                    pi = plugIn;
                                                    break;
                                                }
                                            IMotionRecognition motionRecognition = (IMotionRecognition)pi.assembly.CreateInstance(pi.fullName, false, BindingFlags.CreateInstance, null, m.parameters, null, null);
                                            result = motionRecognition.scan(m.frame, m.nextFrame);
                                        }
                            }
                            catch { }

                            parentConnectionThread.SendResult(task.id, result);
                            parentConnectionThread.SendTaskRequest();
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

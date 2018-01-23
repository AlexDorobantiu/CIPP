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
using CIPPProtocols.Tasks;

namespace CIPPServer
{
    public class SimulationWorkerThread : IDisposable
    {
        public readonly Queue<Task> taskSource;
        public readonly ConnectionThread parentConnectionThread;
        private bool isPendingClosure = false;

        private EventWaitHandle eventWaitHandleBetweenTasks = new AutoResetEvent(false);
        private Thread thread;

        public SimulationWorkerThread(ConnectionThread parent, string threadName)
        {
            parentConnectionThread = parent;
            taskSource = parent.taskBuffer;

            eventWaitHandleBetweenTasks = new AutoResetEvent(false);
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
                            Console.WriteLine("Working on " + task.taskType.ToString() + " task " + task.id);

                            PluginInfo pluginInfo = Program.pluginFinder.findPluginForTask(task);
                            object result = null;
                            try
                            {
                                switch (task.taskType)
                                {
                                    case TaskTypeEnum.filter:
                                        {
                                            FilterTask filterTask = (FilterTask)task;
                                            IFilter filter = PluginHelper.createInstance<IFilter>(pluginInfo, filterTask.parameters);
                                            result = filter.filter(filterTask.originalImage);
                                        } break;
                                    case TaskTypeEnum.mask:
                                        {
                                            MaskTask maskTask = (MaskTask)task;
                                            IMask mask = PluginHelper.createInstance<IMask>(pluginInfo, maskTask.parameters);
                                            result = mask.mask(maskTask.originalImage);
                                        } break;
                                    case TaskTypeEnum.motionRecognition:
                                        {
                                            MotionRecognitionTask motionRecognitionTask = (MotionRecognitionTask)task;
                                            IMotionRecognition motionRecognition = PluginHelper.createInstance<IMotionRecognition>(pluginInfo, motionRecognitionTask.parameters);
                                            result = motionRecognition.scan(motionRecognitionTask.frame, motionRecognitionTask.nextFrame);
                                        } break;
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

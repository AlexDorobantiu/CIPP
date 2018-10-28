using System;
using System.Collections.Generic;
using System.Threading;
using CIPPProtocols;
using CIPPProtocols.Commands;
using CIPPProtocols.Plugin;
using CIPPProtocols.Tasks;
using Plugins.Filters;
using Plugins.Masks;
using Plugins.MotionRecognition;
using ProcessingImageSDK;

namespace CIPP.WorkManagement
{
    class WorkManager
    {
        private const int threadStackSize = 8 * (1 << 20);
        private static object locker = new object();

        private int commandsNumber = 0;
        private int tasksNumber = 0;

        private Queue<FilterCommand> filterRequests = new Queue<FilterCommand>();
        private Queue<MaskCommand> maskRequests = new Queue<MaskCommand>();
        private Queue<MotionRecognitionCommand> motionRecognitionRequests = new Queue<MotionRecognitionCommand>();

        private List<Task> taskList = new List<Task>();
        private List<Motion> motionList = new List<Motion>();
        
        private int numberOfLocalThreads;
        private GranularityTypeEnum granularityType;
        private WorkManagerCallbacks callbacks;
               
        private PluginFinder pluginFinder;

        private Thread[] threads = null;

        public WorkManager(int numberOfLocalThreads, GranularityTypeEnum granularityType, WorkManagerCallbacks callbacks)
        {
            this.numberOfLocalThreads = numberOfLocalThreads;
            this.granularityType = granularityType;
            this.callbacks = callbacks;
        }

        public void updateLists(List<PluginInfo> filterPluginList, List<PluginInfo> maskPluginList, List<PluginInfo> motionRecognitionPluginList)
        {
            pluginFinder = new PluginFinder(filterPluginList, maskPluginList, motionRecognitionPluginList);
        }

        public void updateCommandQueue(List<FilterCommand> filterRequests)
        {
            lock (this.filterRequests)
            {
                foreach (FilterCommand command in filterRequests)
                {
                    this.filterRequests.Enqueue(command);
                }
                commandsNumber += filterRequests.Count;
                callbacks.numberChanged(commandsNumber, false);
            }
        }

        public void updateCommandQueue(List<MaskCommand> maskRequests)
        {
            lock (this.maskRequests)
            {
                foreach (MaskCommand command in maskRequests)
                {
                    this.maskRequests.Enqueue(command);
                }
                commandsNumber += maskRequests.Count;
                callbacks.numberChanged(commandsNumber, false);
            }
        }

        public void updateCommandQueue(List<MotionRecognitionCommand> motionDetectionRequests)
        {
            lock (this.motionRecognitionRequests)
            {
                foreach (MotionRecognitionCommand command in motionDetectionRequests)
                {
                    this.motionRecognitionRequests.Enqueue(command);
                }
                commandsNumber += motionDetectionRequests.Count;
                callbacks.numberChanged(commandsNumber, false);
            }
        }
        
        public void startWorkers(List<TCPProxy> TCPConnections)
        {
            if (numberOfLocalThreads > 0)
            {
                if (threads == null)
                {
                    threads = new Thread[numberOfLocalThreads];
                    for (int i = 0; i < threads.Length; i++)
                    {
                        threads[i] = new Thread(this.doWork, threadStackSize);
                        threads[i].Name = "Local Thread " + i;
                        threads[i].IsBackground = true;
                        threads[i].Start();
                    }
                }
                else
                {
                    for (int i = 0; i < threads.Length; i++)
                    {
                        if (threads[i].ThreadState == ThreadState.Stopped)
                        {
                            threads[i] = new Thread(this.doWork, threadStackSize);
                            threads[i].Name = "Local Thread " + i;
                            threads[i].IsBackground = true;
                            threads[i].Start();
                        }
                    }
                }
            }

            foreach (TCPProxy proxy in TCPConnections)
            {
                if (proxy.connected)
                {
                    proxy.taskRequestReceivedEventHandler += new EventHandler(proxyRequestReceived);
                    proxy.resultsReceivedEventHandler += new ResultReceivedEventHandler(proxyResultReceived);
                    proxy.messagePosted += new EventHandler<TCPProxy.StringEventArgs>(messagePosted);
                    proxy.workerPosted += new EventHandler<TCPProxy.WorkerEventArgs>(workerPosted);
                    proxy.startListening();
                }
            }
        }

        public void stopWorkers(List<TCPProxy> TCPConnections)
        {
            if (threads != null)
            {
                foreach (Thread thread in threads)
                {
                    thread.Abort();
                }
            }

            foreach (TCPProxy proxy in TCPConnections)
            {
                if (proxy.connected)
                {
                    proxy.sendAbortRequest();
                }
            }
            threads = null;
        }
        
        private Task extractFreeTask()
        {
            lock (taskList)
            {
                foreach (Task task in taskList)
                {
                    if (task.status == Task.Status.NOT_TAKEN)
                    {
                        task.status = Task.Status.TAKEN;
                        return task;
                    }
                }
            }
            return null;
        }

        private Task getTask()
        {
            lock (locker)
            {
                Task tempTask = extractFreeTask();
                if (tempTask != null)
                {
                    return tempTask;
                }

                if (filterRequests.Count > 0)
                {
                    FilterCommand filterCommand = filterRequests.Dequeue();
                    commandsNumber--;
                    callbacks.numberChanged(commandsNumber, false);
                    tempTask = new FilterTask(IdGenerator.getID(), filterCommand.pluginFullName, filterCommand.arguments, filterCommand.processingImage);

                    lock (taskList)
                    {
                        taskList.Add(tempTask);
                        tasksNumber++;
                        tempTask.status = Task.Status.TAKEN;
                        if (granularityType == GranularityTypeEnum.low)
                        {
                            callbacks.numberChanged(tasksNumber, true);
                            return tempTask;
                        }
                        else
                        {
                            PluginInfo pluginInfo = pluginFinder.findPluginForTask(tempTask);
                            IFilter filter = PluginHelper.createInstance<IFilter>(pluginInfo, tempTask.parameters);
                            ImageDependencies imageDependencies = filter.getImageDependencies();
                            if (imageDependencies == null)
                            {
                                return tempTask;
                            }
                            int subParts = 0;
                            if (granularityType == GranularityTypeEnum.medium)
                            {
                                subParts = Environment.ProcessorCount;
                            }
                            else
                            {
                                subParts = 2 * Environment.ProcessorCount;
                            }

                            ProcessingImage[] images = ((FilterTask)tempTask).originalImage.split(imageDependencies, subParts);
                            if (images == null)
                            {
                                return tempTask;
                            }
                            ((FilterTask)tempTask).result = ((FilterTask)tempTask).originalImage.blankClone();
                            ((FilterTask)tempTask).subParts = images.Length;

                            tasksNumber += images.Length;
                            callbacks.numberChanged(tasksNumber, true);

                            FilterTask filterTask = null;
                            foreach (ProcessingImage p in images)
                            {
                                filterTask = new FilterTask(IdGenerator.getID(), tempTask.pluginFullName, tempTask.parameters, p);
                                filterTask.parent = (FilterTask)tempTask;
                                taskList.Add(filterTask);
                            }

                            filterTask.status = Task.Status.TAKEN;
                            return filterTask;
                        }
                    }
                }


                if (maskRequests.Count > 0)
                {
                    MaskCommand maskCommand;
                    maskCommand = maskRequests.Dequeue();
                    commandsNumber--;
                    callbacks.numberChanged(commandsNumber, false);

                    tempTask = new MaskTask(IdGenerator.getID(), maskCommand.pluginFullName, maskCommand.arguments, maskCommand.processingImage);
                    lock (taskList)
                    {
                        tasksNumber++;
                        callbacks.numberChanged(tasksNumber, true);
                        tempTask.status = Task.Status.TAKEN;
                        taskList.Add(tempTask);
                        return tempTask;
                    }
                }

                if (motionRecognitionRequests.Count > 0)
                {
                    MotionRecognitionCommand motionRecognitionCommand;
                    motionRecognitionCommand = motionRecognitionRequests.Dequeue();
                    commandsNumber--;
                    callbacks.numberChanged(commandsNumber, false);

                    Motion motion = new Motion(IdGenerator.getID(), (int)motionRecognitionCommand.arguments[0], (int)motionRecognitionCommand.arguments[1], motionRecognitionCommand.processingImageList);

                    lock (motionList)
                    {
                        motionList.Add(motion);
                    }
                    lock (taskList)
                    {
                        MotionRecognitionTask motionRecognitionTask = null;
                        for (int i = 1; i < motion.imageNumber; i++)
                        {
                            tempTask = new MotionRecognitionTask(
                                IdGenerator.getID(), motion.id, motion.blockSize,
                                motion.searchDistance, motionRecognitionCommand.pluginFullName, motionRecognitionCommand.arguments,
                                motion.imageList[i - 1], motion.imageList[i]);


                            taskList.Add(tempTask);
                            tasksNumber++;

                            if (granularityType != GranularityTypeEnum.low)
                            {
                                tempTask.status = Task.Status.TAKEN;
                                int subParts = 0;
                                if (granularityType == GranularityTypeEnum.medium)
                                {
                                    subParts = Environment.ProcessorCount;
                                }
                                else
                                {
                                    subParts = 2 * Environment.ProcessorCount;
                                }
                                ProcessingImage[] images1 = ((MotionRecognitionTask)tempTask).frame.split(new ImageDependencies(motion.searchDistance, motion.searchDistance, motion.searchDistance, motion.searchDistance), subParts);
                                ProcessingImage[] images2 = ((MotionRecognitionTask)tempTask).nextFrame.split(new ImageDependencies(motion.searchDistance, motion.searchDistance, motion.searchDistance, motion.searchDistance), subParts);
                                if (images1 == null || images2 == null)
                                {
                                    return tempTask;
                                }

                                ((MotionRecognitionTask)tempTask).result = MotionVectorUtils.getMotionVectorArray(((MotionRecognitionTask)tempTask).frame, motion.blockSize, motion.searchDistance);
                                ((MotionRecognitionTask)tempTask).subParts = images1.Length;

                                tasksNumber += images1.Length;

                                for (int j = 0; j < images1.Length; j++)
                                {
                                    motionRecognitionTask = new MotionRecognitionTask(IdGenerator.getID(), motion.id, motion.blockSize, motion.searchDistance, tempTask.pluginFullName, tempTask.parameters, images1[j], images2[j]);
                                    motionRecognitionTask.parent = (MotionRecognitionTask)tempTask;
                                    taskList.Add(motionRecognitionTask);
                                }
                            }
                        }
                        callbacks.numberChanged(tasksNumber, true);

                        if (granularityType == GranularityTypeEnum.low)
                        {
                            tempTask = taskList[taskList.Count - 1];
                            tempTask.status = Task.Status.TAKEN;
                            return tempTask;
                        }
                        else
                        {
                            motionRecognitionTask.status = Task.Status.TAKEN;
                            return motionRecognitionTask;
                        }
                    }
                }
            }
            return null;
        }

        private void taskFinished(Task task)
        {
            switch (task.type)
            {
                case Task.Type.FILTER:
                    FilterTask filterTask = (FilterTask)task;
                    if (filterTask.parent != null)
                    {
                        filterTask.parent.join(filterTask);
                        if (filterTask.parent.status == Task.Status.SUCCESSFUL || filterTask.parent.status == Task.Status.FAILED)
                        {
                            taskFinished(filterTask.parent);
                        }
                    }
                    else
                    {
                        if (filterTask.status == Task.Status.SUCCESSFUL)
                        {
                            callbacks.addImageResult(filterTask.result, Task.Type.FILTER);
                        }
                    }
                    break;
                case Task.Type.MASK:
                    MaskTask maskTask = (MaskTask)task;
                    if (maskTask.status == Task.Status.SUCCESSFUL)
                    {
                        callbacks.addImageResult(maskTask.originalImage.cloneAndSubstituteAlpha(maskTask.result), Task.Type.MASK);
                    }
                    break;
                case Task.Type.MOTION_RECOGNITION:
                    MotionRecognitionTask motionRecognitionTask = (MotionRecognitionTask)task;
                    if (motionRecognitionTask.parent != null)
                    {
                        motionRecognitionTask.parent.join(motionRecognitionTask);
                        if (motionRecognitionTask.parent.status == Task.Status.SUCCESSFUL || motionRecognitionTask.parent.status == Task.Status.FAILED)
                        {
                            taskFinished(motionRecognitionTask.parent);
                        }
                    }
                    else
                    {
                        lock (motionList)
                        {
                            foreach (Motion motion in motionList)
                            {
                                if (motionRecognitionTask.motionId == motion.id)
                                {
                                    if (motionRecognitionTask.status == Task.Status.SUCCESSFUL)
                                    {
                                        motion.addMotionVectors(motionRecognitionTask.frame, motionRecognitionTask.result);
                                    }
                                    if (motion.missingVectors == 0)
                                    {
                                        if (motionRecognitionTask.status == Task.Status.SUCCESSFUL)
                                        {
                                            callbacks.addMotion(motion);
                                        }
                                        motionList.Remove(motion);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
            lock (taskList)
            {
                taskList.Remove(task);
                tasksNumber--;
                callbacks.numberChanged(tasksNumber, true);
                if (taskList.Count == 0 && filterRequests.Count == 0 && maskRequests.Count == 0 && motionRecognitionRequests.Count == 0)
                {
                    callbacks.jobDone();
                }
            }
        }

        private void doWork()
        {
            string threadName = Thread.CurrentThread.Name;
            callbacks.addWorkerItem(threadName, true);
            while (true)
            {
                callbacks.addMessage(threadName + " requesting task!");
                Task task = getTask();

                if (task == null)
                {
                    callbacks.addMessage(threadName + " finished work!");
                    break;
                }

                callbacks.addMessage(threadName + " starting " + task.type.ToString() + " task " + task.id);
                try
                {
                    solveTask(task);
                    switch (task.status)
                    {
                        case Task.Status.SUCCESSFUL:
                            callbacks.addMessage(threadName + " finished task " + task.id);
                            break;
                        case Task.Status.FAILED:
                            callbacks.addMessage(threadName + " failed task " + task.id + " with exception: " + task.exception.Message);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    taskFinished(task);
                }
                catch (ThreadAbortException)
                {
                    callbacks.addMessage(threadName + " stopped working on task " + task.id);
                }
            }
            callbacks.addWorkerItem(threadName, false);
        }

        private void solveTask(Task task)
        {
            try
            {
                PluginInfo pluginInfo = pluginFinder.findPluginForTask(task);
                switch (task.type)
                {
                    case Task.Type.FILTER:
                        {
                            FilterTask filterTask = (FilterTask)task;
                            IFilter filter = PluginHelper.createInstance<IFilter>(pluginInfo, filterTask.parameters);
                            filterTask.result = filter.filter(filterTask.originalImage);
                        } break;
                    case Task.Type.MASK:
                        {
                            MaskTask maskTask = (MaskTask)task;
                            IMask mask = PluginHelper.createInstance<IMask>(pluginInfo, maskTask.parameters);
                            maskTask.result = mask.mask(maskTask.originalImage);
                        } break;
                    case Task.Type.MOTION_RECOGNITION:
                        {
                            MotionRecognitionTask motionRecognitionTask = (MotionRecognitionTask)task;
                            IMotionRecognition motionRecognition = PluginHelper.createInstance<IMotionRecognition>(pluginInfo, motionRecognitionTask.parameters);
                            motionRecognitionTask.result = motionRecognition.scan(motionRecognitionTask.frame, motionRecognitionTask.nextFrame);
                        } break;
                }
                task.status = Task.Status.SUCCESSFUL;
            }
            catch (Exception e)
            {
                task.status = Task.Status.FAILED;
                task.exception = e;
            }
        }

        private void proxyRequestReceived(object sender, EventArgs e)
        {
            TCPProxy proxy = (TCPProxy)sender;
            Task task = getTask();
            if (task != null)
            {
                proxy.sendSimulationTask(task);
            }
        }

        private void proxyResultReceived(object sender, TCPProxy.ResultReceivedEventArgs e)
        {
            TCPProxy proxy = (TCPProxy)sender;
            taskFinished(e.task);
        }

        private void messagePosted(object sender, TCPProxy.StringEventArgs e)
        {
            callbacks.addMessage(e.message);
        }

        private void workerPosted(object sender, TCPProxy.WorkerEventArgs e)
        {
            callbacks.addWorkerItem(e.name, !e.left);
        }

    }
}

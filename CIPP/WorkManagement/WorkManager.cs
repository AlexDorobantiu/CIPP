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
        private readonly object locker = new object();

        private int commandsNumber = 0;
        private int tasksNumber = 0;

        private readonly Queue<FilterCommand> filterRequests = new Queue<FilterCommand>();
        private readonly Queue<MaskCommand> maskRequests = new Queue<MaskCommand>();
        private readonly Queue<MotionRecognitionCommand> motionRecognitionRequests = new Queue<MotionRecognitionCommand>();

        private readonly Queue<Task> tasks = new Queue<Task>();
        private readonly Dictionary<int, Task> activeTasks = new Dictionary<int, Task>();
        private readonly Dictionary<int, Motion> activeMotions = new Dictionary<int, Motion>();

        private readonly int numberOfLocalThreads;
        private readonly GranularityTypeEnum granularityType;
        private readonly WorkManagerCallbacks callbacks;
               
        private readonly PluginFinder pluginFinder = new PluginFinder();

        private Thread[] threads = null;

        public WorkManager(int numberOfLocalThreads, GranularityTypeEnum granularityType, WorkManagerCallbacks callbacks)
        {
            this.numberOfLocalThreads = numberOfLocalThreads;
            this.granularityType = granularityType;
            this.callbacks = callbacks;
        }

        public void updateLists(List<PluginInfo> filterPluginList, List<PluginInfo> maskPluginList, List<PluginInfo> motionRecognitionPluginList)
        {
            lock (pluginFinder)
            {
                pluginFinder.updatePluginLists(filterPluginList, maskPluginList, motionRecognitionPluginList);
            }
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
        
        public void startWorkers(List<TcpProxy> TCPConnections)
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

            foreach (TcpProxy proxy in TCPConnections)
            {
                if (proxy.connected)
                {
                    // hackish remove and add
                    removeHandlers(proxy);
                    addHandlers(proxy);
                    proxy.startListening();
                }
            }
        }

        public void stopWorkers(List<TcpProxy> TCPConnections)
        {
            if (threads != null)
            {
                foreach (Thread thread in threads)
                {
                    thread.Abort();
                }
            }

            foreach (TcpProxy proxy in TCPConnections)
            {                
                if (proxy.connected)
                {
                    proxy.sendAbortRequest();
                    removeHandlers(proxy);
                }
            }
            threads = null;
        }

        private void addHandlers(TcpProxy proxy)
        {
            proxy.taskRequestReceivedEventHandler += new EventHandler(proxyRequestReceived);
            proxy.resultsReceivedEventHandler += new ResultReceivedEventHandler(proxyResultReceived);
            proxy.messagePosted += new EventHandler<TcpProxy.StringEventArgs>(messagePosted);
            proxy.workerPosted += new EventHandler<TcpProxy.WorkerEventArgs>(workerPosted);
            proxy.connectionLostEventHandler += new EventHandler(connectionLost);
        }

        private void removeHandlers(TcpProxy proxy)
        {
            proxy.taskRequestReceivedEventHandler -= new EventHandler(proxyRequestReceived);
            proxy.resultsReceivedEventHandler -= new ResultReceivedEventHandler(proxyResultReceived);
            proxy.messagePosted -= new EventHandler<TcpProxy.StringEventArgs>(messagePosted);
            proxy.workerPosted -= new EventHandler<TcpProxy.WorkerEventArgs>(workerPosted);
            proxy.connectionLostEventHandler -= new EventHandler(connectionLost);
        }

        private Task extractFreeTask()
        {
            lock (tasks)
            {
                if (tasks.Count > 0)
                {
                    Task task = tasks.Dequeue();
                    task.status = Task.Status.TAKEN;
                    return task;
                }
            }
            return null;
        }

        private void addActiveTask(Task task, bool parentTask)
        {
            lock (activeTasks)
            {
                activeTasks.Add(task.id, task);
                if (parentTask)
                {
                    tasksNumber++;
                }
            }
        }

        private void removeActiveTask(Task task)
        {
            lock (activeTasks)
            {
                if (activeTasks.ContainsKey(task.id))
                {
                    activeTasks.Remove(task.id);
                    tasksNumber--;
                }
                else
                {
                    throw new Exception("Invalid task received");
                }
            }
        }

        private void addTask(Task task)
        {
            lock (tasks)
            {
                task.status = Task.Status.NOT_TAKEN;
                tasks.Enqueue(task);
                tasksNumber++;
            }
        }
        
        private Task getTask()
        {
            Task task = extractFreeTask();
            if (task != null)
            {
                addActiveTask(task, false);
                return task;
            }

            lock (filterRequests)
            {
                if (filterRequests.Count > 0)
                {
                    FilterCommand filterCommand = filterRequests.Dequeue();
                    commandsNumber--;
                    callbacks.numberChanged(commandsNumber, false);
                    FilterTask tempTask = new FilterTask(IdGenerator.getId(), filterCommand.pluginFullName, filterCommand.arguments, filterCommand.processingImage, null);

                    if (granularityType == GranularityTypeEnum.low)
                    {
                        addTask(tempTask);
                    }
                    else
                    {
                        PluginInfo pluginInfo;
                        lock (pluginFinder)
                        {
                            pluginInfo = pluginFinder.findPluginForTask(tempTask);
                        }
                        IFilter filter = PluginHelper.createInstance<IFilter>(pluginInfo, tempTask.parameters);
                        ImageDependencies imageDependencies = filter.getImageDependencies();
                        if (imageDependencies == null)
                        {
                            addTask(tempTask);
                        }
                        else
                        {
                            int numberOfSubParts = 0;
                            if (granularityType == GranularityTypeEnum.medium)
                            {
                                numberOfSubParts = Environment.ProcessorCount;
                            }
                            else
                            {
                                numberOfSubParts = 2 * Environment.ProcessorCount;
                            }

                            ProcessingImage[] subParts = ((FilterTask)tempTask).originalImage.split(imageDependencies, numberOfSubParts);
                            if (subParts == null)
                            {
                                addTask(tempTask);
                            }
                            else
                            {
                                ((FilterTask)tempTask).result = ((FilterTask)tempTask).originalImage.blankClone();
                                ((FilterTask)tempTask).subParts = subParts.Length;

                                FilterTask filterTask = null;
                                foreach (ProcessingImage subPart in subParts)
                                {
                                    filterTask = new FilterTask(IdGenerator.getId(), tempTask.pluginFullName, tempTask.parameters, subPart, tempTask);
                                    addTask(filterTask);
                                }
                                addActiveTask(tempTask, true);
                            }
                        }
                    }
                    callbacks.numberChanged(tasksNumber, true);
                    return getTask();
                }
            }

            lock (maskRequests)
            {
                if (maskRequests.Count > 0)
                {
                    MaskCommand maskCommand;
                    maskCommand = maskRequests.Dequeue();
                    commandsNumber--;
                    callbacks.numberChanged(commandsNumber, false);

                    Task tempTask = new MaskTask(IdGenerator.getId(), maskCommand.pluginFullName, maskCommand.arguments, maskCommand.processingImage);
                    addTask(tempTask);
                    callbacks.numberChanged(tasksNumber, true);                    
                    return getTask();
                }
            }

            lock (motionRecognitionRequests)
            {
                if (motionRecognitionRequests.Count > 0)
                {
                    MotionRecognitionCommand motionRecognitionCommand;
                    motionRecognitionCommand = motionRecognitionRequests.Dequeue();
                    commandsNumber--;
                    callbacks.numberChanged(commandsNumber, false);

                    Motion motion = new Motion(IdGenerator.getId(), (int)motionRecognitionCommand.arguments[0], (int)motionRecognitionCommand.arguments[1], motionRecognitionCommand.processingImageList);
                    activeMotions.Add(motion.id, motion);

                    for (int i = 1; i < motion.imageNumber; i++)
                    {
                        MotionRecognitionTask tempTask = new MotionRecognitionTask(
                            IdGenerator.getId(), motion.id, motion.blockSize,
                            motion.searchDistance, motionRecognitionCommand.pluginFullName, motionRecognitionCommand.arguments,
                            motion.imageList[i - 1], motion.imageList[i], null);

                        if (granularityType == GranularityTypeEnum.low)
                        {
                            addTask(tempTask);
                        }
                        else
                        {
                            int numberOfSubParts = 0;
                            if (granularityType == GranularityTypeEnum.medium)
                            {
                                numberOfSubParts = Environment.ProcessorCount;
                            }
                            else
                            {
                                numberOfSubParts = 2 * Environment.ProcessorCount;
                            }
                            ImageDependencies imageDependencies = new ImageDependencies(motion.searchDistance, motion.searchDistance, motion.searchDistance, motion.searchDistance);
                            ProcessingImage[] images1 = ((MotionRecognitionTask)tempTask).frame.split(imageDependencies, numberOfSubParts);
                            ProcessingImage[] images2 = ((MotionRecognitionTask)tempTask).nextFrame.split(imageDependencies, numberOfSubParts);
                            if (images1 == null || images2 == null)
                            {
                                addTask(tempTask);
                                continue;
                            }

                            tempTask.result = MotionVectorUtils.getMotionVectorArray(tempTask.frame, motion.blockSize, motion.searchDistance);
                            tempTask.subParts = images1.Length;

                            for (int j = 0; j < images1.Length; j++)
                            {
                                MotionRecognitionTask motionRecognitionTask = new MotionRecognitionTask(IdGenerator.getId(), motion.id, motion.blockSize, motion.searchDistance, tempTask.pluginFullName, tempTask.parameters, images1[j], images2[j], tempTask);
                                addTask(motionRecognitionTask);
                            }
                            addActiveTask(tempTask, true);
                        }
                    }
                    callbacks.numberChanged(tasksNumber, true);
                    return getTask();
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
                        else
                        {
                            callbacks.addMessage("Task with id " + task.id + " failed.");
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
                        if (activeMotions.ContainsKey(motionRecognitionTask.motionId))
                        {
                            Motion motion = activeMotions[motionRecognitionTask.motionId];
                            if (motionRecognitionTask.status == Task.Status.SUCCESSFUL)
                            {
                                motion.addMotionVectors(motionRecognitionTask.frame, motionRecognitionTask.result);
                                if (motion.missingVectors == 0)
                                {
                                    callbacks.addMotion(motion);
                                    activeMotions.Remove(motion.id);
                                }
                            }                            
                        }
                        else
                        {
                            throw new Exception("Invalid motion recognition task");
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            removeActiveTask(task);
            callbacks.numberChanged(tasksNumber, true);
            lock (locker)
            {
                if (tasksNumber == 0 && filterRequests.Count == 0 && maskRequests.Count == 0 && motionRecognitionRequests.Count == 0)
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
                    TaskHelper.solveTask(task, pluginFinder);
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

        private void proxyRequestReceived(object sender, EventArgs e)
        {
            TcpProxy proxy = (TcpProxy)sender;
            Task task;
            task = getTask();
            if (task != null)
            {
                proxy.sendTask(task);
            }
        }

        private void proxyResultReceived(object sender, TcpProxy.ResultReceivedEventArgs e)
        {
            TcpProxy proxy = (TcpProxy)sender;
            taskFinished(e.task);
        }

        private void messagePosted(object sender, TcpProxy.StringEventArgs e)
        {
            callbacks.addMessage(e.message);
        }

        private void workerPosted(object sender, TcpProxy.WorkerEventArgs e)
        {
            callbacks.addWorkerItem(e.name, !e.left);
        }

        private void connectionLost(object sender, EventArgs e)
        {
            callbacks.updateTcpList((TcpProxy)sender);
        }
    }
}

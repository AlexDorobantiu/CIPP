using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using CIPPProtocols;
using CIPPProtocols.Commands;
using CIPPProtocols.Plugin;
using CIPPProtocols.Tasks;
using Plugins.Filters;
using ProcessingImageSDK;

namespace CIPP.WorkManagement
{
    class WorkManager
    {
        private const int threadStackSize = 8 * (1 << 20);

        private volatile int commandsNumber = 0;
        private volatile int tasksNumber = 0;

        private readonly ConcurrentQueue<FilterCommand> filterRequests = new ConcurrentQueue<FilterCommand>();
        private readonly ConcurrentQueue<MaskCommand> maskRequests = new ConcurrentQueue<MaskCommand>();
        private readonly ConcurrentQueue<MotionRecognitionCommand> motionRecognitionRequests = new ConcurrentQueue<MotionRecognitionCommand>();

        private readonly ConcurrentQueue<Task> tasks = new ConcurrentQueue<Task>();
        private readonly ConcurrentDictionary<int, Task> activeTasks = new ConcurrentDictionary<int, Task>();
        private readonly ConcurrentDictionary<int, Motion> activeMotions = new ConcurrentDictionary<int, Motion>();

        private readonly int numberOfLocalThreads;
        private readonly GranularityTypeEnum granularityType;
        private readonly WorkManagerCallbacks callbacks;

        private readonly PluginFinder pluginFinder;


        private Thread[] threads = null;

        public WorkManager(PluginFinder pluginFinder, int numberOfLocalThreads, GranularityTypeEnum granularityType, WorkManagerCallbacks callbacks)
        {
            this.pluginFinder = pluginFinder;
            this.numberOfLocalThreads = numberOfLocalThreads;
            this.granularityType = granularityType;
            this.callbacks = callbacks;
        }

        public void updateCommandQueue(List<FilterCommand> filterRequests)
        {
            foreach (FilterCommand command in filterRequests)
            {
                this.filterRequests.Enqueue(command);
            }
            commandsNumber += filterRequests.Count;
            callbacks.numberChanged(commandsNumber, false);
        }

        public void updateCommandQueue(List<MaskCommand> maskRequests)
        {
            foreach (MaskCommand command in maskRequests)
            {
                this.maskRequests.Enqueue(command);
            }
            commandsNumber += maskRequests.Count;
            callbacks.numberChanged(commandsNumber, false);
        }

        public void updateCommandQueue(List<MotionRecognitionCommand> motionDetectionRequests)
        {
            foreach (MotionRecognitionCommand command in motionDetectionRequests)
            {
                motionRecognitionRequests.Enqueue(command);
            }
            commandsNumber += motionDetectionRequests.Count;
            callbacks.numberChanged(commandsNumber, false);
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
                        threads[i] = new Thread(doWork, threadStackSize)
                        {
                            Name = $"Local Thread {i}",
                            IsBackground = true
                        };
                        threads[i].Start();
                    }
                }
                else
                {
                    for (int i = 0; i < threads.Length; i++)
                    {
                        if (threads[i].ThreadState == ThreadState.Stopped)
                        {
                            threads[i] = new Thread(doWork, threadStackSize)
                            {
                                Name = $"Local Thread {i}",
                                IsBackground = true
                            };
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
            if (!tasks.TryDequeue(out Task task))
            {
                return null;
            }
            task.status = Task.Status.TAKEN;
            return task;
        }

        private void addActiveTask(Task task, bool parentTask)
        {
            activeTasks.TryAdd(task.id, task);
            if (parentTask)
            {
                tasksNumber++;
            }
        }

        private void removeActiveTask(Task task)
        {
            if (activeTasks.ContainsKey(task.id))
            {
                activeTasks.TryRemove(task.id, out _);
                tasksNumber--;
            }
            else
            {
                throw new Exception("Invalid task received");
            }
        }

        private void addTask(Task task)
        {
            task.status = Task.Status.NOT_TAKEN;
            tasks.Enqueue(task);
            tasksNumber++;
        }

        private Task getTask()
        {
            lock (tasks)
            {
                Task task = extractFreeTask();
                if (task != null)
                {
                    addActiveTask(task, false);
                    return task;
                }

                if (filterRequests.TryDequeue(out FilterCommand filterCommand))
                {
                    commandsNumber--;
                    callbacks.numberChanged(commandsNumber, false);
                    FilterTask tempTask = new FilterTask(IdGenerator.getId(), filterCommand.pluginFullName, filterCommand.arguments, filterCommand.processingImage, null);

                    if (granularityType == GranularityTypeEnum.low)
                    {
                        addTask(tempTask);
                    }
                    else
                    {
                        PluginInfo pluginInfo = pluginFinder.findPluginForTask(tempTask);
                        IFilter filter = PluginHelper.createInstance<IFilter>(pluginInfo, tempTask.parameters);
                        ImageDependencies imageDependencies = filter.getImageDependencies();
                        if (imageDependencies == null)
                        {
                            addTask(tempTask);
                        }
                        else
                        {
                            int numberOfSubParts = granularityType == GranularityTypeEnum.medium ? Environment.ProcessorCount : 2 * Environment.ProcessorCount;

                            ProcessingImage[] subParts = tempTask.originalImage.split(imageDependencies, numberOfSubParts);
                            if (subParts == null)
                            {
                                addTask(tempTask);
                            }
                            else
                            {
                                tempTask.result = tempTask.originalImage.blankClone();
                                tempTask.subParts = subParts.Length;
                                foreach (ProcessingImage subPart in subParts)
                                {
                                    FilterTask filterTask = new FilterTask(IdGenerator.getId(), tempTask.pluginFullName, tempTask.parameters, subPart, tempTask);
                                    addTask(filterTask);
                                }
                                addActiveTask(tempTask, true);
                            }
                        }
                    }
                    callbacks.numberChanged(tasksNumber, true);
                    return getTask();
                }


                if (maskRequests.TryDequeue(out MaskCommand maskCommand))
                {
                    commandsNumber--;
                    callbacks.numberChanged(commandsNumber, false);

                    Task tempTask = new MaskTask(IdGenerator.getId(), maskCommand.pluginFullName, maskCommand.arguments, maskCommand.processingImage);
                    addTask(tempTask);
                    callbacks.numberChanged(tasksNumber, true);
                    return getTask();
                }


                if (motionRecognitionRequests.TryDequeue(out MotionRecognitionCommand motionRecognitionCommand))
                {
                    commandsNumber--;
                    callbacks.numberChanged(commandsNumber, false);

                    Motion motion = new Motion(IdGenerator.getId(), (int)motionRecognitionCommand.arguments[0], (int)motionRecognitionCommand.arguments[1], motionRecognitionCommand.processingImageList);
                    activeMotions.TryAdd(motion.id, motion);

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
                            int numberOfSubParts = granularityType == GranularityTypeEnum.medium ? Environment.ProcessorCount : 2 * Environment.ProcessorCount;
                            ImageDependencies imageDependencies = new ImageDependencies(motion.searchDistance, motion.searchDistance, motion.searchDistance, motion.searchDistance);
                            ProcessingImage[] images1 = tempTask.frame.split(imageDependencies, numberOfSubParts);
                            ProcessingImage[] images2 = tempTask.nextFrame.split(imageDependencies, numberOfSubParts);
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
                            callbacks.addMessage($"Task with id {task.id} failed.");
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
                                    activeMotions.TryRemove(motion.id, out _);
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

            if (tasksNumber == 0 && filterRequests.Count == 0 && maskRequests.Count == 0 && motionRecognitionRequests.Count == 0)
            {
                callbacks.jobDone();
            }
        }

        private void doWork()
        {
            string threadName = Thread.CurrentThread.Name;
            callbacks.addWorkerItem(threadName, true);
            while (true)
            {
                callbacks.addMessage($"{threadName} requesting task!");
                Task task = getTask();
                
                if (task == null)
                {
                    callbacks.addMessage($"{threadName} finished work!");
                    break;
                }

                callbacks.addMessage($"{threadName} starting {task.type} task {task.id}");
                try
                {
                    TaskHelper.solveTask(task, pluginFinder);
                    switch (task.status)
                    {
                        case Task.Status.SUCCESSFUL:
                            callbacks.addMessage($"{threadName} finished task {task.id}");
                            break;
                        case Task.Status.FAILED:
                            callbacks.addMessage($"{threadName} failed task {task.id} with exception: {task.exception.Message}");
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    taskFinished(task);
                }
                catch (ThreadAbortException)
                {
                    callbacks.addMessage($"{threadName} stopped working on task {task.id}");
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

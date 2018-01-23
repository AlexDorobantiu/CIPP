using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using CIPPProtocols;
using ProcessingImageSDK;
using ParametersSDK;
using Plugins.Filters;
using Plugins.Masks;
using Plugins.MotionRecognition;
using CIPPProtocols.Tasks;
using CIPPProtocols.Commands;

namespace CIPP
{
    delegate void addImageCallback(ProcessingImage processingImage, TaskTypeEnum taskType);
    delegate void numberChangedCallBack(int number, bool commandOrTask);
    delegate void addMotionCallback(Motion motion);
    delegate void jobFinishedCallback();

    class WorkManager
    {
        public static object locker = new object();

        public int commandsNumber = 0;
        public int tasksNumber = 0;
        
        public Queue<FilterCommand> filterRequests = new Queue<FilterCommand>();
        public Queue<MaskCommand> maskRequests = new Queue<MaskCommand>();
        public Queue<MotionRecognitionCommand> motionRecognitionRequests = new Queue<MotionRecognitionCommand>();

        public GranularityTypeEnum granularityType;

        private List<Task> taskList = new List<Task>();
        private List<Motion> motionList = new List<Motion>();

        private addImageCallback addImageResult;
        private addMotionCallback addMotion;
        private jobFinishedCallback jobDone;
        private numberChangedCallBack numberChanged;

        PluginFinder pluginFinder;
        
        public WorkManager(GranularityTypeEnum granularityType, addImageCallback addImageResult, addMotionCallback addMotion, jobFinishedCallback jobDone, numberChangedCallBack numberChanged)
        {
            this.granularityType = granularityType;
            this.addImageResult = addImageResult;
            this.addMotion = addMotion;
            this.jobDone = jobDone;
            this.numberChanged = numberChanged;
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
                numberChanged(commandsNumber, false);
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
                numberChanged(commandsNumber, false);
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
                numberChanged(commandsNumber, false);
            }
        }

        private Task extractFreeTask()
        {
            lock (taskList)
            {
                foreach (Task task in taskList)
                {
                    if (!task.taken)
                    {
                        task.taken = true;
                        return task;
                    }
                }
            }
            return null;
        }

        public Task getTask(RequestTypeEnum requestType)
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
                    FilterCommand filterCommand;
                    filterCommand = filterRequests.Dequeue();
                    commandsNumber--;
                    numberChanged(commandsNumber, false);
                    tempTask = new FilterTask(IdGenerator.getID(), filterCommand.pluginFullName, filterCommand.arguments, filterCommand.processingImage);

                    lock (taskList)
                    {
                        taskList.Add(tempTask);
                        tasksNumber++;
                        tempTask.taken = true;
                        if (granularityType == GranularityTypeEnum.low)
                        {
                            numberChanged(tasksNumber, true);
                            return tempTask;
                        }
                        else
                        {
                            PluginInfo pluginInfo = pluginFinder.findPluginForTask(tempTask);
                            IFilter filter = PluginHelper.createInstance<IFilter>(pluginInfo, tempTask.parameters);
                            int subParts = 0;
                            if (granularityType == GranularityTypeEnum.medium)
                            {
                                subParts = Environment.ProcessorCount;
                            }
                            else
                            {
                                subParts = 2 * Environment.ProcessorCount;
                            }

                            ImageDependencies imageDependencies = filter.getImageDependencies();
                            if (imageDependencies == null)
                            {
                                return tempTask;
                            }
                            ProcessingImage[] images = ((FilterTask)tempTask).originalImage.split(imageDependencies, subParts);
                            if (images == null)
                            {
                                return tempTask;
                            }
                            ((FilterTask)tempTask).result = ((FilterTask)tempTask).originalImage.blankClone();
                            ((FilterTask)tempTask).subParts = images.Length;

                            tasksNumber += images.Length;
                            numberChanged(tasksNumber, true);

                            FilterTask filterTask = null;
                            foreach (ProcessingImage p in images)
                            {
                                filterTask = new FilterTask(IdGenerator.getID(), tempTask.pluginFullName, tempTask.parameters, p);
                                filterTask.parent = (FilterTask)tempTask;
                                taskList.Add(filterTask);
                            }

                            filterTask.taken = true;
                            return filterTask;
                        }
                    }
                }


                if (maskRequests.Count > 0)
                {
                    MaskCommand maskCommand;
                    maskCommand = maskRequests.Dequeue();
                    commandsNumber--;
                    numberChanged(commandsNumber, false);

                    tempTask = new MaskTask(IdGenerator.getID(), maskCommand.pluginFullName, maskCommand.arguments, maskCommand.processingImage);
                    lock (taskList)
                    {
                        tasksNumber++;
                        numberChanged(tasksNumber, true);
                        tempTask.taken = true;
                        taskList.Add(tempTask);
                        return tempTask;
                    }
                }

                if (motionRecognitionRequests.Count > 0)
                {
                    MotionRecognitionCommand motionRecognitionCommand;
                    motionRecognitionCommand = motionRecognitionRequests.Dequeue();
                    commandsNumber--;
                    numberChanged(commandsNumber, false);

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
                                tempTask.taken = true;
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
                        numberChanged(tasksNumber, true);

                        if (granularityType == GranularityTypeEnum.low)
                        {
                            tempTask = taskList[taskList.Count - 1];
                            tempTask.taken = true;
                            return tempTask;
                        }
                        else
                        {
                            motionRecognitionTask.taken = true;
                            return motionRecognitionTask;
                        }
                    }
                }
            }
            return null;
        }

        public void taskFinished(Task task)
        {
            if (!task.finishedSuccessfully)
            {
                task.taken = false;
                return;
            }

            lock (taskList)
            {
                if (task.taskType == TaskTypeEnum.filter)
                {
                    FilterTask filterTask = (FilterTask)task;
                    if (filterTask.parent != null)
                    {
                        filterTask.parent.join(filterTask);
                        if (filterTask.parent.finishedSuccessfully)
                        {
                            taskFinished(filterTask.parent);
                        }
                    }
                    else
                    {
                        addImageResult(filterTask.result, TaskTypeEnum.filter);
                    }
                }
                else
                {
                    if (task.taskType == TaskTypeEnum.mask)
                    {
                        MaskTask maskTask = (MaskTask)task;
                        addImageResult(maskTask.originalImage.cloneAndSubstituteAlpha(maskTask.result), TaskTypeEnum.mask);
                    }
                    else
                    {
                        if (task.taskType == TaskTypeEnum.motionRecognition)
                        {
                            MotionRecognitionTask motionRecognitionTask = (MotionRecognitionTask)task;
                            if (motionRecognitionTask.parent != null)
                            {
                                motionRecognitionTask.parent.join(motionRecognitionTask);
                                if (motionRecognitionTask.parent.finishedSuccessfully)
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
                                            motion.addMotionVectors(motionRecognitionTask.frame, motionRecognitionTask.result);
                                            if (motion.missingVectors == 0)
                                            {
                                                addMotion(motion);
                                                motionList.Remove(motion);
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                taskList.Remove(task);
                tasksNumber--;
                numberChanged(tasksNumber, true);
                if (taskList.Count == 0 && filterRequests.Count == 0 && maskRequests.Count == 0 && motionRecognitionRequests.Count == 0)
                {
                    jobDone();
                }
            }
        }
    }
}

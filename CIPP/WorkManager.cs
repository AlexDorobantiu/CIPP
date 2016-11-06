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

namespace CIPP
{
    public delegate void addImageCallback(ProcessingImage processingImage, TaskType taskType);
    public delegate void numberChangedCallBack(int number, bool commandOrTask);
    public delegate void addMotionCallback(Motion motion);
    public delegate void jobFinishedCallback();

    public class WorkManager
    {
        public static object locker = new object();

        public int commandsNumber = 0;
        public int tasksNumber = 0;
        
        public Queue<FilterCommand> filterRequests = new Queue<FilterCommand>();
        public Queue<MaskCommand> maskRequests = new Queue<MaskCommand>();
        public Queue<MotionRecognitionCommand> motionRecognitionRequests = new Queue<MotionRecognitionCommand>();

        public GranularityType granularityType;

        private List<Task> taskList = new List<Task>();
        private List<Motion> motionList = new List<Motion>();

        private addImageCallback addImageResult;
        private addMotionCallback addMotion;
        private jobFinishedCallback jobDone;
        private numberChangedCallBack numberChanged;

        private List<PluginInfo> filterPluginList;
        private List<PluginInfo> maskPluginList;
        private List<PluginInfo> motionRecognitionPluginList;

        public WorkManager(GranularityType granularityType, addImageCallback addImageResult, addMotionCallback addMotion, jobFinishedCallback jobDone, numberChangedCallBack numberChanged)
        {
            this.granularityType = granularityType;
            this.addImageResult = addImageResult;
            this.addMotion = addMotion;
            this.jobDone = jobDone;
            this.numberChanged = numberChanged;
        }

        public void updateLists(List<PluginInfo> filterPluginList, List<PluginInfo> maskPluginList, List<PluginInfo> motionRecognitionPluginList)
        {
            this.filterPluginList = filterPluginList;
            this.maskPluginList = maskPluginList;
            this.motionRecognitionPluginList = motionRecognitionPluginList;
        }

        public void updateCommandQueue(List<FilterCommand> filterRequests)
        {
            lock (this.filterRequests)
            {
                foreach (FilterCommand command in filterRequests)
                    this.filterRequests.Enqueue(command);
                commandsNumber += filterRequests.Count;
                numberChanged(commandsNumber, false);
            }
        }

        public void updateCommandQueue(List<MaskCommand> maskRequests)
        {
            lock (this.maskRequests)
            {
                foreach (MaskCommand command in maskRequests)
                    this.maskRequests.Enqueue(command);
                commandsNumber += maskRequests.Count;
                numberChanged(commandsNumber, false);
            }
        }

        public void updateCommandQueue(List<MotionRecognitionCommand> motionDetectionRequests)
        {
            lock (this.motionRecognitionRequests)
            {
                foreach (MotionRecognitionCommand command in motionDetectionRequests)
                    this.motionRecognitionRequests.Enqueue(command);
                commandsNumber += motionDetectionRequests.Count;
                numberChanged(commandsNumber, false);
            }
        }

        private Task extractFreeTask()
        {
            lock (taskList)
            {
                foreach (Task tt in taskList)
                {
                    if (!tt.taken)
                    {
                        tt.taken = true;
                        return tt;
                    }
                }
            }
            return null;
        }

        public Task getTask(RequestType requestType)
        {
            lock (locker)
            {
                Task tempTask = extractFreeTask();
                if (tempTask != null) return tempTask;

                if (filterRequests.Count > 0)
                {
                    FilterCommand f;
                    f = filterRequests.Dequeue();
                    commandsNumber--;
                    numberChanged(commandsNumber, false);
                    tempTask = new FilterTask(IDGenerator.getID(), f.pluginFullName, f.arguments, f.processingImage);

                    lock (taskList)
                    {
                        taskList.Add(tempTask);
                        tasksNumber++;
                        tempTask.taken = true;
                        if (granularityType == GranularityType.low)
                        {
                            numberChanged(tasksNumber, true);
                            return tempTask;
                        }
                        else
                        {
                            PluginInfo pi = null;
                            foreach (PluginInfo plugIn in filterPluginList)
                                if (plugIn.fullName == f.pluginFullName)
                                {
                                    pi = plugIn;
                                    break;
                                }

                            IFilter filter = (IFilter)pi.assembly.CreateInstance(pi.fullName, false, BindingFlags.CreateInstance, null, tempTask.parameters, null, null);

                            int subParts = 0;
                            if (granularityType == GranularityType.medium) subParts = Environment.ProcessorCount;
                            else subParts = 2 * Environment.ProcessorCount;

                            ProcessingImage[] images = ((FilterTask)tempTask).originalImage.split(filter.getImageDependencies(), subParts);
                            if (images == null) return tempTask;

                            ((FilterTask)tempTask).result = ((FilterTask)tempTask).originalImage.blankClone();
                            ((FilterTask)tempTask).subParts = images.Length;

                            tasksNumber += images.Length;
                            numberChanged(tasksNumber, true);

                            FilterTask ft = null;
                            foreach (ProcessingImage p in images)
                            {
                                ft = new FilterTask(IDGenerator.getID(), tempTask.pluginFullName, tempTask.parameters, p);
                                ft.parent = (FilterTask)tempTask;
                                taskList.Add(ft);
                            }

                            ft.taken = true;
                            return ft;
                        }
                    }
                }


                if (maskRequests.Count > 0)
                {
                    MaskCommand m;
                    m = maskRequests.Dequeue();
                    commandsNumber--;
                    numberChanged(commandsNumber, false);

                    tempTask = new MaskTask(IDGenerator.getID(), m.pluginFullName, m.arguments, m.processingImage);
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
                    MotionRecognitionCommand m;
                    m = motionRecognitionRequests.Dequeue();
                    commandsNumber--;
                    numberChanged(commandsNumber, false);

                    Motion motion = new Motion(IDGenerator.getID(), (int)m.arguments[0], (int)m.arguments[1], m.processingImageList);

                    lock (motionList)
                    {
                        motionList.Add(motion);
                    }
                    lock (taskList)
                    {
                        MotionRecognitionTask mrt = null;
                        for (int i = 1; i < motion.imageNumber; i++)
                        {
                            tempTask = new MotionRecognitionTask(
                                IDGenerator.getID(), motion.id, motion.blockSize,
                                motion.searchDistance, m.pluginFullName, m.arguments,
                                motion.imageList[i - 1], motion.imageList[i]);


                            taskList.Add(tempTask);
                            tasksNumber++;

                            if (granularityType != GranularityType.low)
                            {
                                tempTask.taken = true;
                                int subParts = 0;
                                if (granularityType == GranularityType.medium) subParts = Environment.ProcessorCount;
                                else subParts = 2 * Environment.ProcessorCount;

                                ProcessingImage[] images1 = ((MotionRecognitionTask)tempTask).frame.split(new ImageDependencies(motion.searchDistance, motion.searchDistance, motion.searchDistance, motion.searchDistance), subParts);
                                ProcessingImage[] images2 = ((MotionRecognitionTask)tempTask).nextFrame.split(new ImageDependencies(motion.searchDistance, motion.searchDistance, motion.searchDistance, motion.searchDistance), subParts);
                                if (images1 == null || images2 == null) return tempTask;

                                ((MotionRecognitionTask)tempTask).result = MotionVectors.getMotionVectorArray(((MotionRecognitionTask)tempTask).frame, motion.blockSize, motion.searchDistance);
                                ((MotionRecognitionTask)tempTask).subParts = images1.Length;

                                tasksNumber += images1.Length;

                                for (int j = 0; j < images1.Length; j++)
                                {
                                    mrt = new MotionRecognitionTask(IDGenerator.getID(), motion.id, motion.blockSize, motion.searchDistance, tempTask.pluginFullName, tempTask.parameters, images1[j], images2[j]);
                                    mrt.parent = (MotionRecognitionTask)tempTask;
                                    taskList.Add(mrt);
                                }
                            }
                        }
                        numberChanged(tasksNumber, true);

                        if (granularityType == GranularityType.low)
                        {
                            tempTask = taskList[taskList.Count - 1];
                            tempTask.taken = true;
                            return tempTask;
                        }
                        else
                        {
                            mrt.taken = true;
                            return mrt;
                        }
                    }
                }
            }
            return null;
        }

        public void taskFinished(Task task)
        {
            if (!task.state)
            {
                task.taken = false;
                return;
            }

            lock (taskList)
            {
                if (task.taskType == TaskType.filter)
                {
                    FilterTask f = (FilterTask)task;
                    if (f.parent != null)
                    {
                        f.parent.join(f);
                        if (f.parent.state) taskFinished(f.parent);
                    }
                    else
                    {
                        addImageResult(f.result, TaskType.filter);
                    }
                }
                else
                {
                    if (task.taskType == TaskType.mask)
                    {
                        MaskTask m = (MaskTask)task;
                        addImageResult(m.originalImage.cloneAndSubstituteAlpha(m.result), TaskType.mask);
                    }
                    else
                    {
                        if (task.taskType == TaskType.motionRecognition)
                        {
                            MotionRecognitionTask m = (MotionRecognitionTask)task;
                            if (m.parent != null)
                            {
                                m.parent.join(m);
                                if (m.parent.state) taskFinished(m.parent);
                            }
                            else
                            {
                                lock (motionList)
                                {
                                    foreach (Motion motion in motionList)
                                    {
                                        if (m.motionId == motion.id)
                                        {
                                            motion.addMotionVectors(m.frame, m.result);
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

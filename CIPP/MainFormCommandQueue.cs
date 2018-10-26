using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using System.Threading;
using System.Reflection;
using System.Collections;

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
    partial class CIPPForm
    {
        private const String NO_NAME_DEFAULT = " - no name - ";
        private const int threadStackSize = 8 * (1 << 20);

        WorkManager workManager = null;
        private Thread[] threads = null;

        private void doWork()
        {
            string threadName = Thread.CurrentThread.Name;
            displayWorker(Thread.CurrentThread.Name, true);
            while (workManager != null)
            {
                addMessage(threadName + " requesting task!");
                Task task = workManager.getTask(RequestTypeEnum.local);

                if (task == null)
                {
                    addMessage(threadName + " finished work!");
                    break;
                }

                addMessage(threadName + " starting " + task.taskType.ToString() + " task " + task.id);
                try
                {
                    solveTask(task);
                    addMessage(threadName + " finished task " + task.id);
                    workManager.taskFinished(task);
                }    
                catch (ThreadAbortException)
                {
                    addMessage(threadName + " stopped working on task " + task.id);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Task failed with exception: " + e.Message);
                    addMessage(threadName + " failed task " + task.id);
                }
            }
            displayWorker(threadName, false);
        }

        private void solveTask(Task task)
        {
            PluginInfo pluginInfo = pluginFinder.findPluginForTask(task);
            switch (task.taskType)
            {
                case TaskTypeEnum.filter:
                    {
                        FilterTask filterTask = (FilterTask)task;
                        IFilter filter = PluginHelper.createInstance<IFilter>(pluginInfo, filterTask.parameters);
                        filterTask.result = filter.filter(filterTask.originalImage);
                    } break;
                case TaskTypeEnum.mask:
                    {
                        MaskTask maskTask = (MaskTask)task;
                        IMask mask = PluginHelper.createInstance<IMask>(pluginInfo, maskTask.parameters);
                        maskTask.result = mask.mask(maskTask.originalImage);
                    } break;
                case TaskTypeEnum.motionRecognition:
                    {
                        MotionRecognitionTask motionRecognitionTask = (MotionRecognitionTask)task;
                        IMotionRecognition motionRecognition = PluginHelper.createInstance<IMotionRecognition>(pluginInfo, motionRecognitionTask.parameters);
                        motionRecognitionTask.result = motionRecognition.scan(motionRecognitionTask.frame, motionRecognitionTask.nextFrame);
                    } break;
            }
            task.finishedSuccessfully = true;
        }

        private void proxyRequestReceived(object sender, EventArgs e)
        {
            try
            {
                TCPProxy proxy = (TCPProxy)sender;
                Task task = workManager.getTask(RequestTypeEnum.lan);
                if (task != null)
                {
                    proxy.sendSimulationTask(task);
                }
            }
            catch { }
        }

        private void proxyResultReceived(object sender, ResultReceivedEventArgs e)
        {
            try
            {
                TCPProxy proxy = (TCPProxy)sender;
                workManager.taskFinished(e.task);
            }
            catch { }
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            try
            {
                List<ProcessingImage> selectedImageList = null;
                ListBox.SelectedIndexCollection selectedIndices = null;

                switch (imageTab.SelectedIndex)
                {
                    //original tab
                    case 0:
                        {
                            selectedImageList = originalImageList;
                            selectedIndices = originalImageListBox.SelectedIndices;
                        } break;
                    //processed tab
                    case 1:
                        {
                            selectedImageList = processedImageList;
                            selectedIndices = processedImageListBox.SelectedIndices;
                        } break;
                    //masked tab
                    case 2:
                        {
                            selectedImageList = maskedImageList;
                            selectedIndices = maskedImageListBox.SelectedIndices;
                        } break;
                    //scaned tab
                    case 3:
                        {
                            return; //nothing to process (here, yet)
                        }
                }

                if (selectedIndices.Count == 0)
                {
                    return; //no images selected
                }

                List<FilterCommand> filterCommandList = null;
                List<MaskCommand> maskCommandList = null;
                List<MotionRecognitionCommand> motionRecognitionCommandList = null;

                List<CheckBox> checkBoxList = null;
                List<PluginInfo> plugInList = null;

                switch (processingTab.SelectedIndex)
                {
                    //filtering tab
                    case 0:
                        {
                            filterCommandList = new List<FilterCommand>();
                            checkBoxList = filterPluginsCheckBoxList;
                            plugInList = filterPluginList;
                        } break;
                    case 1:
                        {
                            maskCommandList = new List<MaskCommand>();
                            checkBoxList = maskPluginsCheckBoxList;
                            plugInList = maskPluginList;
                        } break;
                    case 2:
                        {
                            if (selectedIndices.Count == 1)
                            {
                                return; // only one image selected
                            }
                            motionRecognitionCommandList = new List<MotionRecognitionCommand>();
                            checkBoxList = motionRecognitionPluginsCheckBoxList;
                            plugInList = motionRecognitionPluginList;
                        } break;
                }

                bool anyItems = false;
                for (int index = 0; index < checkBoxList.Count; index++)
                {
                    if (checkBoxList[index].Checked)
                    {
                        anyItems = true;

                        string name = plugInList[index].fullName;
                        List<IParameters> parameterList = plugInList[index].parameters;
                        int parametersNumber = parameterList != null ? parameterList.Count : 0;

                        // simple backtracking to compute all combinations of parameters
                        if (parametersNumber != 0)
                        {
                            List<object>[] values = new List<object>[parametersNumber];

                            int i;
                            int[] valueIndex = new int[parametersNumber];
                            for (i = 0; i < parametersNumber; i++)
                            {
                                values[i] = parameterList[i].getValues();
                                valueIndex[i] = -1;
                            }

                            i = 0;
                            while (i >= 0)
                            {
                                valueIndex[i]++;
                                if (valueIndex[i] == values[i].Count)
                                {
                                    i--;
                                }
                                else
                                {
                                    if (i == parametersNumber - 1) // one of the solutions
                                    {
                                        object[] combination = new object[parametersNumber];
                                        for (int k = 0; k < parametersNumber; k++)
                                        {
                                            combination[k] = values[k][valueIndex[k]];
                                        }
                                        switch (processingTab.SelectedIndex)
                                        {
                                            case 0:
                                                {
                                                    foreach (int selectedIndex in selectedIndices)
                                                    {
                                                        filterCommandList.Add(new FilterCommand(name, combination, selectedImageList[selectedIndex]));
                                                    }
                                                } break;
                                            case 1:
                                                {
                                                    foreach (int selectedIndex in selectedIndices)
                                                    {
                                                        maskCommandList.Add(new MaskCommand(name, combination, selectedImageList[selectedIndex]));
                                                    }
                                                } break;
                                            case 2:
                                                {
                                                    List<ProcessingImage> imageList = new List<ProcessingImage>();
                                                    foreach (int selectedIndex in selectedIndices)
                                                    {
                                                        imageList.Add(selectedImageList[selectedIndex]);
                                                    }
                                                    motionRecognitionCommandList.Add(new MotionRecognitionCommand(name, combination, imageList));
                                                } break;
                                        }
                                    }
                                    else
                                    {
                                        valueIndex[++i] = -1;
                                    }
                                }
                            }
                        }
                        else
                        {
                            switch (processingTab.SelectedIndex)
                            {
                                case 0:
                                    {
                                        foreach (int selectedIndex in selectedIndices)
                                        {
                                            filterCommandList.Add(new FilterCommand(name, null, selectedImageList[selectedIndex]));
                                        }
                                    } break;
                                case 1:
                                    {
                                        foreach (int selectedIndex in selectedIndices)
                                        {
                                            maskCommandList.Add(new MaskCommand(name, null, selectedImageList[selectedIndex]));
                                        }
                                    } break;
                                case 2:
                                    {
                                        List<ProcessingImage> imageList = new List<ProcessingImage>();
                                        foreach (int selectedIndex in selectedIndices)
                                        {
                                            imageList.Add(selectedImageList[selectedIndex]);
                                        }
                                        motionRecognitionCommandList.Add(new MotionRecognitionCommand(name, null, imageList));
                                    } break;
                            }
                        }
                    }
                }
                if (!anyItems)
                {
                    return;
                }

                if (workManager == null)
                {
                    workManager = new WorkManager((GranularityTypeEnum)localGranularityComboBox.SelectedIndex, addImage, addMotion, jobFinished, numberChanged);
                    this.workersList.Items.Add("Manager");
                    workerControlTab.Enabled = false;
                    finishButton.Enabled = true;
                }
                workManager.updateLists(filterPluginList, maskPluginList, motionRecognitionPluginList);

                switch (processingTab.SelectedIndex)
                {
                    case 0:
                        {
                            if (filterCommandList.Count == 0)
                            {
                                return;
                            }
                            workManager.updateCommandQueue(filterCommandList);
                        } break;
                    case 1:
                        {
                            if (maskCommandList.Count == 0)
                            {
                                return;
                            }
                            workManager.updateCommandQueue(maskCommandList);
                        } break;
                    case 2:
                        {
                            if (motionRecognitionCommandList.Count == 0)
                            {
                                return;
                            }
                            workManager.updateCommandQueue(motionRecognitionCommandList);
                        } break;
                }

                timeValueLabel.Text = "0";
                timer.Start();

                int numberOfThreads = (int)localNumberOfWorkers.Value;
                if (numberOfThreads > 0)
                {
                    if (threads == null)
                    {
                        threads = new Thread[numberOfThreads];
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
                        proxy.startListening();
                    }
                }
            }
            catch (Exception exceptie)
            {
                MessageBox.Show(exceptie.Message);
            }
        }

        private void finishButton_Click(object sender, EventArgs e)
        {
            try
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

                workManager = null;
                threads = null;
                workerControlTab.Enabled = true;
                finishButton.Enabled = false;
                workersList.Items.Clear();

                timer.Stop();
                time = 0;
                totalTimeValueLabel.Text = "" + totalTime;
                numberOfCommandsValueLabel.Text = "0";
                numberOfTasksValueLabel.Text = "0";
            }
            catch { }
        }

        delegate void updateTCPListCallback(TCPProxy proxy);
        private void updateTCPList(TCPProxy proxy)
        {
            try
            {
                if (this.TCPConnectionsListBox.InvokeRequired)
                {
                    updateTCPListCallback d = new updateTCPListCallback(updateTCPList);
                    this.Invoke(d, new object[] { proxy });
                }
                else
                {
                    for (int i = 0; i < TCPConnections.Count; i++)
                    {
                        if (proxy == TCPConnections[i])
                        {
                            if (TCPConnectionsListBox.Items[i] != null)
                            {
                                TCPConnectionsListBox.Items[i] = proxy.getNameAndStatus();
                                break;
                            }
                        }
                    }
                }
            }
            catch { }
        }

        delegate void addItemCallback(string name, bool visible);
        private void displayWorker(string workerName, bool visible)
        {
            try
            {
                if (this.workersList.InvokeRequired)
                {
                    addItemCallback d = new addItemCallback(displayWorker);
                    this.Invoke(d, new object[] { workerName, visible });
                }
                else
                {
                    if (visible)
                    {
                        this.workersList.Items.Add(workerName);
                    }
                    else
                    {
                        this.workersList.Items.Remove(workerName);
                    }
                }
            }
            catch { }
        }

        delegate void addMessageCallback(string message);
        private void addMessage(string message)
        {
            try
            {
                if (this.messagesList.InvokeRequired)
                {
                    addMessageCallback d = new addMessageCallback(addMessage);
                    this.Invoke(d, new object[] { message });
                }
                else
                {
                    messagesList.Items.Add(message);
                }
            }
            catch { }
        }

        private void addImage(ProcessingImage processingImage, TaskTypeEnum taskType)
        {
            try
            {
                if (this.processedImageListBox.InvokeRequired)
                {
                    addImageCallback d = new addImageCallback(addImage);
                    this.Invoke(d, new object[] { processingImage, taskType });
                }
                else
                {
                    if (taskType == TaskTypeEnum.filter)
                    {
                        processedImageListBox.Items.Add(getNameOrDefault(processingImage));
                        processedImageList.Add(processingImage);
                    }
                    else
                    {
                        if (taskType == TaskTypeEnum.mask)
                        {
                            maskedImageListBox.Items.Add(getNameOrDefault(processingImage));
                            maskedImageList.Add(processingImage);
                        }
                    }
                }
            }
            catch { }
        }

        private static String getNameOrDefault(ProcessingImage processingImage)
        {
            String nameToAdd = processingImage.getName();
            if (nameToAdd == null || String.Empty.Equals(nameToAdd))
            {
                return NO_NAME_DEFAULT;
            }
            return nameToAdd;
        }

        private void addMotion(Motion motion)
        {
            try
            {
                if (this.motionListBox.InvokeRequired)
                {
                    addMotionCallback d = new addMotionCallback(addMotion);
                    this.Invoke(d, new object[] { motion });
                }
                else
                {
                    motionListBox.Items.Add("Motion " + motion.id);
                    motionList.Add(motion);
                }
            }
            catch { }
        }

        private void jobFinished()
        {
            try
            {
                if (this.InvokeRequired)
                {
                    jobFinishedCallback d = new jobFinishedCallback(jobFinished);
                    this.Invoke(d, null);
                }
                else
                {
                    timer.Stop();
                    time = 0;
                    totalTimeValueLabel.Text = "" + totalTime;
                    if (allertFinishCheckBox.Checked)
                    {
                        MessageBox.Show("Finished!");
                    }
                }
            }
            catch { }
        }

        private void numberChanged(int number, bool commandOrTask)
        {
            try
            {
                if (this.processedImageListBox.InvokeRequired)
                {
                    numberChangedCallBack d = new numberChangedCallBack(numberChanged);
                    this.Invoke(d, new object[] { number, commandOrTask });
                }
                else
                {
                    if (commandOrTask)
                    {
                        numberOfTasksValueLabel.Text = "" + number;
                    }
                    else
                    {
                        numberOfCommandsValueLabel.Text = "" + number;
                    }
                }
            }
            catch { }
        }
    }
}

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

namespace CIPP
{
    public enum RequestType
    {
        local,
        lan,
        wan
    }

    public struct FilterCommand
    {
        public string pluginFullName;
        public object[] arguments;
        public ProcessingImage processingImage;

        public FilterCommand(string pluginFullName, object[] arguments, ProcessingImage processingImage)
        {
            this.pluginFullName = pluginFullName;
            this.arguments = arguments;
            this.processingImage = processingImage;
        }
    }

    public struct MaskCommand
    {
        public string pluginFullName;
        public object[] arguments;
        public ProcessingImage processingImage;

        public MaskCommand(string pluginFullName, object[] arguments, ProcessingImage processingImage)
        {
            this.pluginFullName = pluginFullName;
            this.arguments = arguments;
            this.processingImage = processingImage;
        }
    }

    public struct MotionRecognitionCommand
    {
        public string pluginFullName;
        public object[] arguments;
        public List<ProcessingImage> processingImageList;

        public MotionRecognitionCommand(string pluginFullName, object[] arguments, List<ProcessingImage> processingImageList)
        {
            this.pluginFullName = pluginFullName;
            this.arguments = arguments;
            this.processingImageList = processingImageList;
        }
    }


    partial class CIPPForm
    {
        WorkManager workManager = null;
        private Thread[] threads = null;

        private void doWork()
        {
            displayWorker(Thread.CurrentThread.Name, true);
            while (true)
            {
                addMessage(Thread.CurrentThread.Name + " requesting task!");
                Task t = workManager.getTask(RequestType.local);

                if (t == null)
                {
                    addMessage(Thread.CurrentThread.Name + " finished work!");
                    break;
                }

                addMessage(Thread.CurrentThread.Name + " starting " + t.taskType.ToString() + " task " + t.id);
                try
                {
                    solveTask(t);
                    addMessage(Thread.CurrentThread.Name + " finished task " + t.id);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Task failed with exception: " + e.Message);
                    addMessage(Thread.CurrentThread.Name + " failed task " + t.id);
                }                
                workManager.taskFinished(t);
            }
            displayWorker(Thread.CurrentThread.Name, false);
        }

        private void solveTask(Task t)
        {
            switch (t.taskType)
            {
                case TaskType.filter:
                    {
                        FilterTask f = (FilterTask)t;

                        PluginInfo pi = null;
                        foreach (PluginInfo plugIn in filterPluginList)
                        {
                            if (plugIn.fullName == f.pluginFullName)
                            {
                                pi = plugIn;
                                break;
                            }
                        }

                        IFilter filter = (IFilter)pi.assembly.CreateInstance(pi.fullName, false, BindingFlags.CreateInstance, null, f.parameters, null, null);
                        f.result = filter.filter(f.originalImage);
                        f.state = true;
                    } break;
                case TaskType.mask:
                    {
                        MaskTask m = (MaskTask)t;

                        PluginInfo pi = null;
                        foreach (PluginInfo plugIn in maskPluginList)
                        {
                            if (plugIn.fullName == m.pluginFullName)
                            {
                                pi = plugIn;
                                break;
                            }
                        }
                        IMask mask = (IMask)pi.assembly.CreateInstance(pi.fullName, false, BindingFlags.CreateInstance, null, m.parameters, null, null);
                        m.result = mask.mask(m.originalImage);
                        m.state = true;
                    } break;
                case TaskType.motionRecognition:
                    {
                        MotionRecognitionTask m = (MotionRecognitionTask)t;

                        PluginInfo pi = null;
                        foreach (PluginInfo plugIn in motionRecognitionPluginList)
                            if (plugIn.fullName == m.pluginFullName)
                            {
                                pi = plugIn;
                                break;
                            }

                        IMotionRecognition motionRecognition = (IMotionRecognition)pi.assembly.CreateInstance(pi.fullName, false, BindingFlags.CreateInstance, null, m.parameters, null, null);
                        m.result = motionRecognition.scan(m.frame, m.nextFrame);
                        m.state = true;
                    } break;
            }
        }

        public void proxyRequestReceived(object sender, EventArgs e)
        {
            try
            {
                TCPProxy proxy = (TCPProxy)sender;
                Task task = workManager.getTask(RequestType.lan);
                if (task != null)
                    proxy.SendSimulationTask(task);
            }
            catch { }
        }

        public void proxyResultReceived(object sender, ResultReceivedEventArgs e)
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
                ArrayList selectedImageList = null;
                ListBox.SelectedIndexCollection selectedIndices = null;

                switch (imageTab.SelectedIndex)
                {
                    //original tab
                    case 0:
                        {
                            selectedImageList = originalImageArrayList;
                            selectedIndices = originalImageList.SelectedIndices;
                        } break;
                    //processed tab
                    case 1:
                        {
                            selectedImageList = processedImageArrayList;
                            selectedIndices = processedImageList.SelectedIndices;
                        } break;
                    //masked tab
                    case 2:
                        {
                            selectedImageList = maskedImageArrayList;
                            selectedIndices = maskedImageList.SelectedIndices;
                        } break;
                    //scaned tab
                    case 3:
                        {
                            return; //nothing to process (here, yet)
                        }
                }

                if (selectedIndices.Count == 0) return; //no images selected



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
                            if (selectedIndices.Count == 1) return; //only one image selected
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
                        int parametersNumber = parameterList.Count;

                        if (parametersNumber != 0)
                        {
                            List<object>[] values = new List<object>[parametersNumber];

                            int i;
                            int[] vi = new int[parametersNumber];
                            for (i = 0; i < parametersNumber; i++)
                            {
                                values[i] = parameterList[i].getValues();
                                vi[i] = -1;
                            }

                            i = 0;
                            while (i >= 0)
                            {
                                vi[i]++;
                                if (vi[i] == values[i].Count) i--;
                                else
                                    if (i == parametersNumber - 1) //una dintre solutii
                                    {
                                        object[] combination = new object[parametersNumber];
                                        for (int k = 0; k < parametersNumber; k++)
                                            combination[k] = values[k][vi[k]];

                                        switch (processingTab.SelectedIndex)
                                        {
                                            case 0:
                                                {
                                                    foreach (int selectedIndex in selectedIndices)
                                                        filterCommandList.Add(new FilterCommand(name, combination, (ProcessingImage)selectedImageList[selectedIndex]));
                                                } break;
                                            case 1:
                                                {
                                                    foreach (int selectedIndex in selectedIndices)
                                                        maskCommandList.Add(new MaskCommand(name, combination, (ProcessingImage)selectedImageList[selectedIndex]));
                                                } break;
                                            case 2:
                                                {
                                                    List<ProcessingImage> imageList = new List<ProcessingImage>();
                                                    foreach (int selectedIndex in selectedIndices)
                                                        imageList.Add((ProcessingImage)selectedImageList[selectedIndex]);
                                                    motionRecognitionCommandList.Add(new MotionRecognitionCommand(name, combination, imageList));
                                                } break;
                                        }
                                    }
                                    else vi[++i] = -1;
                            }
                        }
                        else
                        {
                            switch (processingTab.SelectedIndex)
                            {
                                case 0:
                                    {
                                        foreach (int selectedIndex in selectedIndices)
                                            filterCommandList.Add(new FilterCommand(name, null, (ProcessingImage)selectedImageList[selectedIndex]));
                                    } break;
                                case 1:
                                    {
                                        foreach (int selectedIndex in selectedIndices)
                                            maskCommandList.Add(new MaskCommand(name, null, (ProcessingImage)selectedImageList[selectedIndex]));
                                    } break;
                                case 2:
                                    {
                                        List<ProcessingImage> imageList = new List<ProcessingImage>();
                                        foreach (int selectedIndex in selectedIndices)
                                            imageList.Add((ProcessingImage)selectedImageList[selectedIndex]);
                                        motionRecognitionCommandList.Add(new MotionRecognitionCommand(name, null, imageList));
                                    } break;
                            }
                        }
                    }
                }
                if (!anyItems) return;

                if (workManager == null)
                {
                    workManager = new WorkManager((GranularityType)localGranularityComboBox.SelectedIndex, addImage, addMotion, jobFinished, numberChanged);
                    this.workersList.Items.Add("Manager");
                    workerControlTab.Enabled = false;
                    finishButton.Enabled = true;
                }
                workManager.updateLists(filterPluginList, maskPluginList, motionRecognitionPluginList);

                switch (processingTab.SelectedIndex)
                {
                    case 0:
                        {
                            if (filterCommandList.Count == 0) return;
                            workManager.updateCommandQueue(filterCommandList);
                        } break;
                    case 1:
                        {
                            if (maskCommandList.Count == 0) return;
                            workManager.updateCommandQueue(maskCommandList);
                        } break;
                    case 2:
                        {
                            if (motionRecognitionCommandList.Count == 0) return;
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
                            threads[i] = new Thread(this.doWork);
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
                                threads[i] = new Thread(this.doWork);
                                threads[i].Name = "Local Thread " + i;
                                threads[i].IsBackground = true;
                                threads[i].Start();
                            }
                        }
                    }
                }

                foreach (TCPProxy proxy in TCPConnections)
                    if (proxy.isConnected)
                    {
                        proxy.StartListening();
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
                    foreach (Thread t in threads)
                    {
                        t.Abort();
                        t.Join();
                    }
                }

                foreach (TCPProxy proxy in TCPConnections)
                    if (proxy.isConnected)
                    {
                        proxy.SendAbortRequest();
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
                    if (visible) this.workersList.Items.Add(workerName);
                    else this.workersList.Items.Remove(workerName);
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

        private void addImage(ProcessingImage processingImage, TaskType taskType)
        {
            try
            {
                if (this.processedImageList.InvokeRequired)
                {
                    addImageCallback d = new addImageCallback(addImage);
                    this.Invoke(d, new object[] { processingImage, taskType });
                }
                else
                {
                    if (taskType == TaskType.filter)
                    {
                        processedImageList.Items.Add(processingImage.getName());
                        processedImageArrayList.Add(processingImage);
                    }
                    else
                        if (taskType == TaskType.mask)
                        {
                            maskedImageList.Items.Add(processingImage.getName());
                            maskedImageArrayList.Add(processingImage);
                        }
                }
            }
            catch { }
        }

        private void addMotion(Motion motion)
        {
            try
            {
                if (this.motionList.InvokeRequired)
                {
                    addMotionCallback d = new addMotionCallback(addMotion);
                    this.Invoke(d, new object[] { motion });
                }
                else
                {
                    motionList.Items.Add("Motion " + motion.id);
                    motionArrayList.Add(motion);
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
                        MessageBox.Show("Finished!");
                }
            }
            catch { }
        }

        private void numberChanged(int number, bool commandOrTask)
        {
            try
            {
                if (this.processedImageList.InvokeRequired)
                {
                    numberChangedCallBack d = new numberChangedCallBack(numberChanged);
                    this.Invoke(d, new object[] { number, commandOrTask });
                }
                else
                {
                    if (commandOrTask)
                        numberOfTasksValueLabel.Text = "" + number;
                    else
                        numberOfCommandsValueLabel.Text = "" + number;
                }
            }
            catch { }
        }
    }
}

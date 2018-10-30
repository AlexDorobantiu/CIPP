using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CIPP.WorkManagement;
using CIPPProtocols;
using CIPPProtocols.Commands;
using CIPPProtocols.Plugin;
using ParametersSDK;
using ProcessingImageSDK;

namespace CIPP
{
    delegate void addMessageCallback(string message);
    delegate void addWorkerItemCallback(string name, bool visible);
    delegate void addImageCallback(ProcessingImage processingImage, Task.Type taskType);
    delegate void numberChangedCallBack(int number, bool commandOrTask);
    delegate void addMotionCallback(Motion motion);
    delegate void jobFinishedCallback();
    delegate void updateTCPListCallback(TcpProxy proxy);
    delegate void ResultReceivedEventHandler(object sender, TcpProxy.ResultReceivedEventArgs e);

    partial class CIPPForm
    {
        private const String NO_NAME_DEFAULT = " - no name - ";

        WorkManager workManager = null;

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
                        selectedImageList = originalImageList;
                        selectedIndices = originalImageListBox.SelectedIndices;
                        break;
                    //processed tab
                    case 1:
                        selectedImageList = processedImageList;
                        selectedIndices = processedImageListBox.SelectedIndices;
                        break;
                    //masked tab
                    case 2:
                        selectedImageList = maskedImageList;
                        selectedIndices = maskedImageListBox.SelectedIndices;
                        break;
                    //scaned tab
                    case 3:
                        return; //nothing to process (here, yet)
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
                    case 0:
                        filterCommandList = new List<FilterCommand>();
                        checkBoxList = filterPluginsCheckBoxList;
                        plugInList = filterPluginList;
                        break;
                    case 1:
                        maskCommandList = new List<MaskCommand>();
                        checkBoxList = maskPluginsCheckBoxList;
                        plugInList = maskPluginList;
                        break;
                    case 2:
                        if (selectedIndices.Count == 1)
                        {
                            return; // only one image selected
                        }
                        motionRecognitionCommandList = new List<MotionRecognitionCommand>();
                        checkBoxList = motionRecognitionPluginsCheckBoxList;
                        plugInList = motionRecognitionPluginList;
                        break;
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
                    WorkManagerCallbacks callbacks = new WorkManagerCallbacks(addMessage, displayWorker, addImage, addMotion, jobFinished, numberChanged, updateTCPList);
                    int numberOfLocalThreads = (int)localNumberOfWorkers.Value;
                    workManager = new WorkManager(numberOfLocalThreads, (GranularityTypeEnum)localGranularityComboBox.SelectedIndex, callbacks);
                    displayWorker("Manager - " + numberOfLocalThreads + " local threads", true);
                    workerControlTab.Enabled = false;
                    finishButton.Enabled = true;
                }
                workManager.updateLists(filterPluginList, maskPluginList, motionRecognitionPluginList);

                switch (processingTab.SelectedIndex)
                {
                    case 0:
                        if (filterCommandList.Count == 0)
                        {
                            return;
                        }
                        workManager.updateCommandQueue(filterCommandList);
                        break;
                    case 1:
                        if (maskCommandList.Count == 0)
                        {
                            return;
                        }
                        workManager.updateCommandQueue(maskCommandList);
                        break;
                    case 2:
                        if (motionRecognitionCommandList.Count == 0)
                        {
                            return;
                        }
                        workManager.updateCommandQueue(motionRecognitionCommandList);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                timeValueLabel.Text = "0";
                timer.Start();

                workManager.startWorkers(TCPConnections);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void finishButton_Click(object sender, EventArgs e)
        {
            if (workManager != null)
            {
                workManager.stopWorkers(TCPConnections);
                workManager = null;
            }

            workerControlTab.Enabled = true;
            finishButton.Enabled = false;
            workersList.Items.Clear();

            timer.Stop();
            time = 0;
            totalTimeValueLabel.Text = "" + totalTime;
            numberOfCommandsValueLabel.Text = "0";
            numberOfTasksValueLabel.Text = "0";
        }

        private void updateTCPList(TcpProxy proxy)
        {
            if (this.TCPConnectionsListBox.InvokeRequired)
            {
                updateTCPListCallback d = new updateTCPListCallback(updateTCPList);
                this.Invoke(d, new object[] { proxy });
            }
            else
            {
                if (this.TCPConnectionsListBox.IsDisposed)
                {
                    return;
                }
                for (int i = 0; i < TCPConnections.Count; i++)
                {
                    if (proxy == TCPConnections[i])
                    {
                        if (this.TCPConnectionsListBox.Items[i] != null)
                        {
                            this.TCPConnectionsListBox.Items[i] = proxy.getNameAndStatus();
                            break;
                        }
                    }
                }
            }
        }

        private void displayWorker(string workerName, bool visible)
        {
            if (this.workersList.InvokeRequired)
            {
                addWorkerItemCallback d = new addWorkerItemCallback(displayWorker);
                this.Invoke(d, new object[] { workerName, visible });
            }
            else
            {
                if (this.workersList.IsDisposed)
                {
                    return;
                }
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
                    if (this.messagesList.IsDisposed)
                    {
                        return;
                    }
                    messagesList.Items.Add(message);
                }
            }
            catch { }
        }

        private void addImage(ProcessingImage processingImage, Task.Type taskType)
        {
            if (this.processedImageListBox.InvokeRequired)
            {
                addImageCallback d = new addImageCallback(addImage);
                this.Invoke(d, new object[] { processingImage, taskType });
            }
            else
            {
                if (this.processedImageListBox.IsDisposed)
                {
                    return;
                }
                switch (taskType)
                {
                    case Task.Type.FILTER:
                        processedImageListBox.Items.Add(getNameOrDefault(processingImage));
                        processedImageList.Add(processingImage);
                        break;
                    case Task.Type.MASK:
                        maskedImageListBox.Items.Add(getNameOrDefault(processingImage));
                        maskedImageList.Add(processingImage);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private String getNameOrDefault(ProcessingImage processingImage)
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
            if (this.motionListBox.InvokeRequired)
            {
                addMotionCallback d = new addMotionCallback(addMotion);
                this.Invoke(d, new object[] { motion });
            }
            else
            {
                if (this.motionListBox.IsDisposed)
                {
                    return;
                }
                motionListBox.Items.Add("Motion " + motion.id);
                motionList.Add(motion);
            }
        }

        private void jobFinished()
        {
            timer.Stop();
            time = 0;
            totalTimeValueLabel.Text = "" + totalTime;
            if (allertFinishCheckBox.Checked)
            {
                MessageBox.Show("Finished!");
            }
        }

        private void numberChanged(int number, bool commandOrTask)
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
}

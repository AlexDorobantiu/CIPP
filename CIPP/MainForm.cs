using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.IO;

using CIPPProtocols;
using ProcessingImageSDK;
using Plugins.Filters;
using Plugins.Masks;
using Plugins.MotionRecognition;

namespace CIPP
{
    //interfata grafica
    public partial class CIPPForm : Form
    {
        int totalTime;
        int time;

        public ArrayList originalImageArrayList;
        public ArrayList processedImageArrayList;
        public ArrayList maskedImageArrayList;
        public ArrayList motionArrayList;

        public List<PluginInfo> filterPluginList;
        public List<PluginInfo> maskPluginList;
        public List<PluginInfo> motionRecognitionPluginList;

        public List<CheckBox> filterPluginsCheckBoxList;
        public List<CheckBox> maskPluginsCheckBoxList;
        public List<CheckBox> motionRecognitionPluginsCheckBoxList;

        public List<TCPProxy> TCPConnections = new List<TCPProxy>();

        public CIPPForm()
        {
            InitializeComponent();
            localNumberOfWorkers.Value = Environment.ProcessorCount;
            localGranularityComboBox.SelectedIndex = 0;

            originalImageArrayList = new ArrayList();
            processedImageArrayList = new ArrayList();
            maskedImageArrayList = new ArrayList();
            motionArrayList = new ArrayList();

            filterPluginList = new List<PluginInfo>();
            maskPluginList = new List<PluginInfo>();
            motionRecognitionPluginList = new List<PluginInfo>();

            filterPluginsCheckBoxList = new List<CheckBox>();
            maskPluginsCheckBoxList = new List<CheckBox>();
            motionRecognitionPluginsCheckBoxList = new List<CheckBox>();

            TCPConnections = new List<TCPProxy>();
            loadConnectionsFromDisk();
        }

        private void loadFile(string fileName)
        {
            try
            {
                FileInfo fi = new FileInfo(fileName);
                if (ProcessingImage.getKnownExtensions().Contains(fi.Extension.ToUpper()))
                {
                    ProcessingImage pi = new ProcessingImage();
                    pi.loadImage(fileName);
                    originalImageArrayList.Add(pi);
                    originalImageList.Items.Add(fi.Name);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void addImageButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (String fileName in openFileDialog.FileNames)
                {
                    loadFile(fileName);
                }
            }
        }

        private void originalImageList_SelectedIndexChanged(object sender, EventArgs e)
        {
            previewPicture.Image = null;
            ProcessingImage pi = null;
            widthValueLabel.Text = null;
            heightValueLabel.Text = null;
            grayscaleValueLabel.Text = null;
            maskedValueLabel.Text = null;

            switch (imageTab.SelectedIndex)
            {
                //original image tab
                case 0:
                    {
                        if (originalImageList.SelectedItems.Count == 1)
                            pi = (ProcessingImage)originalImageArrayList[originalImageList.SelectedIndex];
                    } break;
                //processed image tab
                case 1:
                    {
                        if (processedImageList.SelectedItems.Count == 1)
                            pi = (ProcessingImage)processedImageArrayList[processedImageList.SelectedIndex];
                    } break;
                //masked image tab
                case 2:
                    {
                        if (maskedImageList.SelectedItems.Count == 1)
                            pi = (ProcessingImage)maskedImageArrayList[maskedImageList.SelectedIndex];
                    } break;
                //scaned image tab
                case 3:
                    {

                    } break;
            }
            if (pi != null)
            {
                previewPicture.Image = pi.getPreviewBitmap(previewPicture.Size.Width, previewPicture.Size.Height);
                widthValueLabel.Text = "" + pi.getSizeX();
                heightValueLabel.Text = "" + pi.getSizeY();
                grayscaleValueLabel.Text = pi.grayscale.ToString();
                maskedValueLabel.Text = pi.masked.ToString();

                watermarkListBox.Items.Clear();
                List<string> list = pi.getWatermarks();
                foreach (string s in list)
                    watermarkListBox.Items.Add(s);
            }
            GC.Collect();
        }

        private void removeImageButton_Click(object sender, EventArgs e)
        {
            switch (imageTab.SelectedIndex)
            {
                //original image tab
                case 0:
                    {
                        while (originalImageList.SelectedItems.Count > 0)
                        {
                            originalImageArrayList.RemoveAt(originalImageList.SelectedIndices[0]);
                            originalImageList.Items.RemoveAt(originalImageList.SelectedIndices[0]);
                        }
                    } break;
                //processed image tab
                case 1:
                    {
                        while (processedImageList.SelectedItems.Count > 0)
                        {
                            processedImageArrayList.RemoveAt(processedImageList.SelectedIndices[0]);
                            processedImageList.Items.RemoveAt(processedImageList.SelectedIndices[0]);
                        }
                    } break;
                //masked image tab
                case 2:
                    {
                        while (maskedImageList.SelectedItems.Count > 0)
                        {
                            maskedImageArrayList.RemoveAt(maskedImageList.SelectedIndices[0]);
                            maskedImageList.Items.RemoveAt(maskedImageList.SelectedIndices[0]);
                        }
                    } break;
                //scaned image tab
                case 3:
                    {
                        while (motionList.SelectedItems.Count > 0)
                        {
                            motionArrayList.RemoveAt(motionList.SelectedIndices[0]);
                            motionList.Items.RemoveAt(motionList.SelectedIndices[0]);
                        }
                    } break;
            }
            GC.Collect();
        }

        private void moveUpButton_Click(object sender, EventArgs e)
        {
            switch (imageTab.SelectedIndex)
            {
                //original image tab
                case 0:
                    {
                        if (originalImageList.SelectedIndices.Count == 0) return;
                        if (originalImageList.SelectedIndices[0] == 0) return;
                        for (int i = 0; i < originalImageList.SelectedIndices.Count; i++)
                        {
                            int currentSelected = originalImageList.SelectedIndices[i];
                            Object temp = originalImageList.Items[currentSelected - 1];
                            originalImageList.Items[currentSelected - 1] = originalImageList.Items[currentSelected];
                            originalImageList.Items[currentSelected] = temp;

                            temp = originalImageArrayList[currentSelected - 1];
                            originalImageArrayList[currentSelected - 1] = originalImageArrayList[currentSelected];
                            originalImageArrayList[currentSelected] = temp;

                            originalImageList.SetSelected(currentSelected, false);
                            originalImageList.SetSelected(currentSelected - 1, true);
                        }
                    } break;
                //processed image tab
                case 1:
                    {
                        if (processedImageList.SelectedIndices.Count == 0) return;
                        if (processedImageList.SelectedIndices[0] == 0) return;
                        for (int i = 0; i < processedImageList.SelectedIndices.Count; i++)
                        {
                            int currentSelected = processedImageList.SelectedIndices[i];
                            Object temp = processedImageList.Items[currentSelected - 1];
                            processedImageList.Items[currentSelected - 1] = processedImageList.Items[currentSelected];
                            processedImageList.Items[currentSelected] = temp;

                            temp = processedImageArrayList[currentSelected - 1];
                            processedImageArrayList[currentSelected - 1] = processedImageArrayList[currentSelected];
                            processedImageArrayList[currentSelected] = temp;

                            processedImageList.SetSelected(currentSelected, false);
                            processedImageList.SetSelected(currentSelected - 1, true);
                        }
                    } break;
                //masked image tab
                case 2:
                    {
                        if (maskedImageList.SelectedIndices.Count == 0) return;
                        if (maskedImageList.SelectedIndices[0] == 0) return;
                        for (int i = 0; i < maskedImageList.SelectedIndices.Count; i++)
                        {
                            int currentSelected = maskedImageList.SelectedIndices[i];
                            Object temp = maskedImageList.Items[currentSelected - 1];
                            maskedImageList.Items[currentSelected - 1] = maskedImageList.Items[currentSelected];
                            maskedImageList.Items[currentSelected] = temp;

                            temp = maskedImageArrayList[currentSelected - 1];
                            maskedImageArrayList[currentSelected - 1] = maskedImageArrayList[currentSelected];
                            maskedImageArrayList[currentSelected] = temp;

                            maskedImageList.SetSelected(currentSelected, false);
                            maskedImageList.SetSelected(currentSelected - 1, true);
                        }
                    } break;
                //scaned image tab
                case 3:
                    {

                    } break;
            }
        }

        private void moveDownButton_Click(object sender, EventArgs e)
        {
            switch (imageTab.SelectedIndex)
            {
                //original image tab
                case 0:
                    {
                        if (originalImageList.SelectedIndices.Count == 0) return;
                        if (originalImageList.SelectedIndices[originalImageList.SelectedIndices.Count - 1] == originalImageList.Items.Count - 1) return;
                        for (int i = originalImageList.SelectedIndices.Count - 1; i >= 0; i--)
                        {
                            int currentSelected = originalImageList.SelectedIndices[i];
                            Object temp = originalImageList.Items[currentSelected];
                            originalImageList.Items[currentSelected] = originalImageList.Items[currentSelected + 1];
                            originalImageList.Items[currentSelected + 1] = temp;

                            temp = originalImageArrayList[currentSelected];
                            originalImageArrayList[currentSelected] = originalImageArrayList[currentSelected + 1];
                            originalImageArrayList[currentSelected + 1] = temp;

                            originalImageList.SetSelected(currentSelected, false);
                            originalImageList.SetSelected(currentSelected + 1, true);
                        }
                    } break;
                //processed image tab
                case 1:
                    {
                        if (processedImageList.SelectedIndices.Count == 0) return;
                        if (processedImageList.SelectedIndices[processedImageList.SelectedIndices.Count - 1] == processedImageList.Items.Count - 1) return;
                        for (int i = processedImageList.SelectedIndices.Count - 1; i >= 0; i--)
                        {
                            int currentSelected = processedImageList.SelectedIndices[i];
                            Object temp = processedImageList.Items[currentSelected];
                            processedImageList.Items[currentSelected] = processedImageList.Items[currentSelected + 1];
                            processedImageList.Items[currentSelected + 1] = temp;

                            temp = processedImageArrayList[currentSelected];
                            processedImageArrayList[currentSelected] = processedImageArrayList[currentSelected + 1];
                            processedImageArrayList[currentSelected + 1] = temp;

                            processedImageList.SetSelected(currentSelected, false);
                            processedImageList.SetSelected(currentSelected + 1, true);
                        }
                    } break;
                //masked image tab
                case 2:
                    {
                        if (maskedImageList.SelectedIndices.Count == 0) return;
                        if (maskedImageList.SelectedIndices[maskedImageList.SelectedIndices.Count - 1] == maskedImageList.Items.Count - 1) return;
                        for (int i = maskedImageList.SelectedIndices.Count - 1; i >= 0; i--)
                        {
                            int currentSelected = maskedImageList.SelectedIndices[i];
                            Object temp = maskedImageList.Items[currentSelected];
                            maskedImageList.Items[currentSelected] = maskedImageList.Items[currentSelected + 1];
                            maskedImageList.Items[currentSelected + 1] = temp;

                            temp = maskedImageArrayList[currentSelected];
                            maskedImageArrayList[currentSelected] = maskedImageArrayList[currentSelected + 1];
                            maskedImageArrayList[currentSelected + 1] = temp;

                            maskedImageList.SetSelected(currentSelected, false);
                            maskedImageList.SetSelected(currentSelected + 1, true);
                        }
                    } break;
                //scaned image tab
                case 3:
                    {
                    } break;
            }            
        }

        private void saveImageButton_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog.AddExtension = true;
                saveFileDialog.Filter = "PNG(*.png)|*.png|Bitmap(*.bmp)|*.bmp|JPEG|*.jpg|GIF|*.gif|ICO|*.ico|EMF|*.emf|EXIF|*.exif|TIFF|*.tiff|WMF|*.wmf|All files (*.*)|*.*";

                ListBox visibleListBox = null;
                ArrayList selectedArrayList = null;

                switch (imageTab.SelectedIndex)
                {
                    //original image tab
                    case 0:
                        {
                            visibleListBox = originalImageList;
                            selectedArrayList = originalImageArrayList;
                        } break;
                    //processed image tab
                    case 1:
                        {
                            visibleListBox = processedImageList;
                            selectedArrayList = processedImageArrayList;
                        } break;
                    //masked image tab
                    case 2:
                        {
                            visibleListBox = maskedImageList;
                            selectedArrayList = maskedImageArrayList;
                        } break;
                    //scaned image tab
                    case 3:
                        {
                        } break;
                }
                if (visibleListBox == null || selectedArrayList == null)
                {
                    return;
                }

                if (visibleListBox.SelectedIndices.Count == 1)
                {
                    saveFileDialog.FileName = visibleListBox.Items[visibleListBox.SelectedIndices[0]].ToString();
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        ProcessingImage pi = (ProcessingImage)selectedArrayList[visibleListBox.SelectedIndices[0]];
                        pi.saveImage(saveFileDialog.FileName);
                    }
                }
                else
                {
                    if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    {
                        for (int i = 0; i < visibleListBox.SelectedIndices.Count; i++)
                        {
                            ProcessingImage pi = (ProcessingImage)selectedArrayList[visibleListBox.SelectedIndices[i]];
                            String newPath = Path.Combine(folderBrowserDialog.SelectedPath, visibleListBox.Items[visibleListBox.SelectedIndices[i]].ToString());
                            pi.saveImage(newPath);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void sortButton_Click(object sender, EventArgs e)
        {
            switch (imageTab.SelectedIndex)
            {
                //original image tab
                case 0:
                    {
                        int i = 0;
                        for (; i < originalImageList.Items.Count; i++)
                        {
                            for (int j = i + 1; j < originalImageList.Items.Count; j++)
                                if (originalImageList.Items[i].ToString().CompareTo(originalImageList.Items[j].ToString()) > 0)
                                {
                                    Object temp = originalImageList.Items[i];
                                    originalImageList.Items[i] = originalImageList.Items[j];
                                    originalImageList.Items[j] = temp;

                                    temp = originalImageArrayList[i];
                                    originalImageArrayList[i] = originalImageArrayList[j];
                                    originalImageArrayList[j] = temp;
                                }
                            originalImageList.SetSelected(i, false);
                        }
                    } break;
                //processed image tab
                case 1:
                    {
                        int i = 0;
                        for (; i < processedImageList.Items.Count; i++)
                        {
                            for (int j = i + 1; j < processedImageList.Items.Count; j++)
                                if (processedImageList.Items[i].ToString().CompareTo(processedImageList.Items[j].ToString()) > 0)
                                {
                                    Object temp = processedImageList.Items[i];
                                    processedImageList.Items[i] = processedImageList.Items[j];
                                    processedImageList.Items[j] = temp;

                                    temp = processedImageArrayList[i];
                                    processedImageArrayList[i] = processedImageArrayList[j];
                                    processedImageArrayList[j] = temp;
                                }
                            processedImageList.SetSelected(i, false);
                        }
                    } break;
                //masked image tab
                case 2:
                    {
                        int i = 0;
                        for (; i < maskedImageList.Items.Count; i++)
                        {
                            for (int j = i + 1; j < maskedImageList.Items.Count; j++)
                                if (maskedImageList.Items[i].ToString().CompareTo(maskedImageList.Items[j].ToString()) > 0)
                                {
                                    Object temp = maskedImageList.Items[i];
                                    maskedImageList.Items[i] = maskedImageList.Items[j];
                                    maskedImageList.Items[j] = temp;

                                    temp = maskedImageArrayList[i];
                                    maskedImageArrayList[i] = maskedImageArrayList[j];
                                    maskedImageArrayList[j] = temp;
                                }
                            maskedImageList.SetSelected(i, false);
                        }
                    } break;
                //scaned image tab
                case 3:
                    {

                    } break;
            }            
        }

        private void imageTab_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (imageTab.SelectedIndex)
            {
                //original image tab
                case 0:
                    {
                        addImageButton.Enabled = true;
                        viewImageButton.Enabled = true;
                        previewMotionButton.Enabled = false;
                    } break;
                //processed image tab
                case 1:
                    {
                        addImageButton.Enabled = false;
                        viewImageButton.Enabled = true;
                        previewMotionButton.Enabled = false;
                    } break;
                //masked image tab
                case 2:
                    {
                        addImageButton.Enabled = false;
                        viewImageButton.Enabled = true;
                        previewMotionButton.Enabled = false;
                    } break;
                //scaned image tab
                case 3:
                    {
                        //startButton.Enabled = false;
                        addImageButton.Enabled = false;
                        viewImageButton.Enabled = false;
                        previewMotionButton.Enabled = true;
                    } break;
            }
        }

        private void viewImageButton_Click(object sender, EventArgs e)
        {
            Form f;
            switch (imageTab.SelectedIndex)
            {
                //original image tab
                case 0:
                    {
                        if (originalImageList.SelectedIndices.Count == 0) return;
                        for (int i = 0; i < originalImageList.SelectedIndices.Count; i++)
                        {
                            f = new ViewImageForm((ProcessingImage)(originalImageArrayList[originalImageList.SelectedIndices[i]]));
                            f.Show();
                            f.Focus();
                        }                        
                    } break;
                //processed image tab
                case 1:
                    {
                        if (processedImageList.SelectedIndices.Count == 0) return;
                        for (int i = 0; i < processedImageList.SelectedIndices.Count; i++)
                        {
                            f = new ViewImageForm((ProcessingImage)(processedImageArrayList[processedImageList.SelectedIndices[i]]));
                            f.Show();
                            f.Focus();
                        }
                    } break;
                //masked image tab
                case 2:
                    {
                        if (maskedImageList.SelectedIndices.Count == 0) return;
                        for (int i = 0; i < maskedImageList.SelectedIndices.Count; i++)
                        {
                            f = new ViewImageForm((ProcessingImage)(maskedImageArrayList[maskedImageList.SelectedIndices[i]]));
                            f.Show();
                            f.Focus();
                        }
                    } break;
            }            
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            AboutBox a = new AboutBox();
            a.ShowDialog();
        }

        private void originalImageList_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                e.Effect = DragDropEffects.All;
            }
        }

        private void originalImageList_DragDrop(object sender, DragEventArgs e)
        {
            String[] files=(string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string fileName in files)
            {
                DirectoryInfo di= new DirectoryInfo(fileName);
                if (di.Exists) 
                {
                    FileInfo[] directoryFiles = di.GetFiles("*.*", SearchOption.AllDirectories);
                    foreach (FileInfo f in directoryFiles)
                        loadFile(f.FullName);
                }
                else
                {
                    loadFile(fileName);
                }
            }
        }

        private void clearMessagesListButton_Click(object sender, EventArgs e)
        {
            messagesList.Items.Clear();
        }

        private void previewMotionButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < motionList.SelectedIndices.Count; i++)
            {
                Form f = new ViewMotionForm((Motion)(motionArrayList[motionList.SelectedIndices[i]]));
                f.Show();
                f.Focus();
            }
        }

        private void CIPPForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            saveConnectionsToDisk();
            finishButton_Click(this, EventArgs.Empty);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            totalTime++;
            time++;
            timeValueLabel.Text = "" + time;
        }

        private void tutorialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://www.dorobantiu.ro");
            }
            catch { }
        }
    }
}
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
    public partial class CIPPForm : Form
    {
        int totalTime;
        int time;

        ProcessingImage visibleImage;

        private List<ProcessingImage> originalImageList = new List<ProcessingImage>();
        private List<ProcessingImage> processedImageList = new List<ProcessingImage>();
        private List<ProcessingImage> maskedImageList = new List<ProcessingImage>();
        private List<Motion> motionList = new List<Motion>();

        private PluginFinder pluginFinder;
        private List<PluginInfo> filterPluginList = new List<PluginInfo>();
        private List<PluginInfo> maskPluginList = new List<PluginInfo>();
        private List<PluginInfo> motionRecognitionPluginList = new List<PluginInfo>();

        private List<CheckBox> filterPluginsCheckBoxList = new List<CheckBox>();
        private List<CheckBox> maskPluginsCheckBoxList = new List<CheckBox>();
        private List<CheckBox> motionRecognitionPluginsCheckBoxList = new List<CheckBox>();

        private List<TCPProxy> TCPConnections = new List<TCPProxy>();

        /// <summary>
        /// Main application form
        /// </summary>
        public CIPPForm()
        {
            InitializeComponent();
            localNumberOfWorkers.Value = Environment.ProcessorCount;
            localGranularityComboBox.SelectedIndex = 0;

            TCPConnections = new List<TCPProxy>();
            loadConnectionsFromDisk();

            pluginFinder = new PluginFinder(filterPluginList, maskPluginList, motionRecognitionPluginList);
            updatePlugins_Click(this, null);           
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
                    originalImageList.Add(pi);
                    originalImageListBox.Items.Add(fi.Name);
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

        private void getVisibleLists(ref List<ProcessingImage> visibleImagesList, ref ListBox visibleListBox)
        {
            switch (imageTab.SelectedIndex)
            {
                //original image tab
                case 0:
                    {
                        visibleImagesList = originalImageList;
                        visibleListBox = originalImageListBox;
                    } break;
                //processed image tab
                case 1:
                    {
                        visibleImagesList = processedImageList;
                        visibleListBox = processedImageListBox;
                    } break;
                //masked image tab
                case 2:
                    {
                        visibleImagesList = maskedImageList;
                        visibleListBox = maskedImageListBox;
                    } break;
                //scaned image tab
                case 3:
                    {
                    } break;
            }
        }

        private void updateVisibleImage()
        {
            if (visibleImage != null)
            {
                previewPicture.Image = visibleImage.getPreviewBitmap(previewPicture.Size.Width, previewPicture.Size.Height);
                widthValueLabel.Text = "" + visibleImage.getSizeX();
                heightValueLabel.Text = "" + visibleImage.getSizeY();
                grayscaleValueLabel.Text = visibleImage.grayscale.ToString();
                maskedValueLabel.Text = visibleImage.masked.ToString();

                watermarkListBox.Items.Clear();
                List<string> list = visibleImage.getWatermarks();
                foreach (string s in list)
                {
                    watermarkListBox.Items.Add(s);
                }
            }
            GC.Collect();
        }

        private void originalImageList_SelectedIndexChanged(object sender, EventArgs e)
        {
            visibleImage = null;
            previewPicture.Image = null;
            widthValueLabel.Text = null;
            heightValueLabel.Text = null;
            grayscaleValueLabel.Text = null;
            maskedValueLabel.Text = null;

            List<ProcessingImage> visibleImagesList = null;
            ListBox visibleListBox = null;
            getVisibleLists(ref visibleImagesList, ref visibleListBox);

            if (visibleImagesList != null && visibleListBox != null && visibleListBox.SelectedItems.Count == 1)
            {
                visibleImage = (ProcessingImage)visibleImagesList[visibleListBox.SelectedIndex];
            }
            updateVisibleImage();
        }

        private void removeImageButton_Click(object sender, EventArgs e)
        {
            switch (imageTab.SelectedIndex)
            {
                //original image tab
                case 0:
                    {
                        while (originalImageListBox.SelectedItems.Count > 0)
                        {
                            originalImageList.RemoveAt(originalImageListBox.SelectedIndices[0]);
                            originalImageListBox.Items.RemoveAt(originalImageListBox.SelectedIndices[0]);
                        }
                    } break;
                //processed image tab
                case 1:
                    {
                        while (processedImageListBox.SelectedItems.Count > 0)
                        {
                            processedImageList.RemoveAt(processedImageListBox.SelectedIndices[0]);
                            processedImageListBox.Items.RemoveAt(processedImageListBox.SelectedIndices[0]);
                        }
                    } break;
                //masked image tab
                case 2:
                    {
                        while (maskedImageListBox.SelectedItems.Count > 0)
                        {
                            maskedImageList.RemoveAt(maskedImageListBox.SelectedIndices[0]);
                            maskedImageListBox.Items.RemoveAt(maskedImageListBox.SelectedIndices[0]);
                        }
                    } break;
                //scaned image tab
                case 3:
                    {
                        while (motionListBox.SelectedItems.Count > 0)
                        {
                            motionList.RemoveAt(motionListBox.SelectedIndices[0]);
                            motionListBox.Items.RemoveAt(motionListBox.SelectedIndices[0]);
                        }
                    } break;
            }
            GC.Collect();
        }

        private void moveUpButton_Click(object sender, EventArgs e)
        {
            List<ProcessingImage> visibleImagesList = null;
            ListBox visibleListBox = null;
            getVisibleLists(ref visibleImagesList, ref visibleListBox);

            if (visibleImagesList == null || visibleListBox == null || visibleListBox.SelectedIndices.Count == 0 || visibleListBox.SelectedIndices[0] == 0)
            {
                return;
            }

            for (int i = 0; i < visibleListBox.SelectedIndices.Count; i++)
            {
                int currentSelected = visibleListBox.SelectedIndices[i];
                object tempObject = visibleListBox.Items[currentSelected - 1];
                visibleListBox.Items[currentSelected - 1] = visibleListBox.Items[currentSelected];
                visibleListBox.Items[currentSelected] = tempObject;

                ProcessingImage temp = visibleImagesList[currentSelected - 1];
                visibleImagesList[currentSelected - 1] = visibleImagesList[currentSelected];
                visibleImagesList[currentSelected] = temp;

                visibleListBox.SetSelected(currentSelected, false);
                visibleListBox.SetSelected(currentSelected - 1, true);
            }
        }


        private void moveDownButton_Click(object sender, EventArgs e)
        {
            List<ProcessingImage> visibleImagesList = null;
            ListBox visibleListBox = null;
            getVisibleLists(ref visibleImagesList, ref visibleListBox);

            if (visibleImagesList == null || visibleListBox == null || visibleListBox.SelectedIndices.Count == 0 || 
                visibleListBox.SelectedIndices[originalImageListBox.SelectedIndices.Count - 1] == originalImageListBox.Items.Count - 1)
            {
                return;
            }

            for (int i = visibleListBox.SelectedIndices.Count - 1; i >= 0; i--)
            {
                int currentSelected = visibleListBox.SelectedIndices[i];
                object tempObj = visibleListBox.Items[currentSelected];
                visibleListBox.Items[currentSelected] = visibleListBox.Items[currentSelected + 1];
                visibleListBox.Items[currentSelected + 1] = tempObj;

                ProcessingImage temp = visibleImagesList[currentSelected];
                visibleImagesList[currentSelected] = visibleImagesList[currentSelected + 1];
                visibleImagesList[currentSelected + 1] = temp;

                originalImageListBox.SetSelected(currentSelected, false);
                originalImageListBox.SetSelected(currentSelected + 1, true);
            }
        }

        private void saveImageButton_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog.AddExtension = true;
                saveFileDialog.Filter = "PNG(*.png)|*.png|Bitmap(*.bmp)|*.bmp|JPEG|*.jpg|GIF|*.gif|ICO|*.ico|EMF|*.emf|EXIF|*.exif|TIFF|*.tiff|WMF|*.wmf|All files (*.*)|*.*";

                List<ProcessingImage> visibleImagesList = null;
                ListBox visibleListBox = null;
                getVisibleLists(ref visibleImagesList, ref visibleListBox);

                if (visibleListBox == null || visibleImagesList == null)
                {
                    return;
                }

                if (visibleListBox.SelectedIndices.Count == 1)
                {
                    saveFileDialog.FileName = visibleListBox.Items[visibleListBox.SelectedIndices[0]].ToString();
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        ProcessingImage pi = visibleImagesList[visibleListBox.SelectedIndices[0]];
                        pi.saveImage(saveFileDialog.FileName);
                    }
                }
                else
                {
                    if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    {
                        for (int i = 0; i < visibleListBox.SelectedIndices.Count; i++)
                        {
                            ProcessingImage pi = visibleImagesList[visibleListBox.SelectedIndices[i]];
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
            List<ProcessingImage> visibleImagesList = null;
            ListBox visibleListBox = null;
            getVisibleLists(ref visibleImagesList, ref visibleListBox);

            if (visibleListBox == null || visibleImagesList == null)
            {
                return;
            }

            for (int i = 0; i < visibleListBox.Items.Count; i++)
            {
                for (int j = i + 1; j < visibleListBox.Items.Count; j++)
                {
                    if (visibleListBox.Items[i].ToString().CompareTo(visibleListBox.Items[j].ToString()) > 0)
                    {
                        object tempObj = visibleListBox.Items[i];
                        visibleListBox.Items[i] = visibleListBox.Items[j];
                        visibleListBox.Items[j] = tempObj;

                        ProcessingImage temp = visibleImagesList[i];
                        visibleImagesList[i] = visibleImagesList[j];
                        visibleImagesList[j] = temp;
                    }
                }
                visibleListBox.SetSelected(i, false);
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
            originalImageList_SelectedIndexChanged(sender, e);
        }

        private void viewImageButton_Click(object sender, EventArgs e)
        {
            List<ProcessingImage> visibleImagesList = null;
            ListBox visibleListBox = null;
            getVisibleLists(ref visibleImagesList, ref visibleListBox);

            if (visibleListBox == null || visibleImagesList == null || visibleListBox.SelectedIndices.Count == 0)
            {
                return;
            }

            for (int i = 0; i < visibleListBox.SelectedIndices.Count; i++)
            {
                Form form = new ViewImageForm((ProcessingImage)(visibleImagesList[visibleListBox.SelectedIndices[i]]));
                form.Show();
                form.Focus();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
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
            String[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string fileName in files)
            {
                DirectoryInfo di = new DirectoryInfo(fileName);
                if (di.Exists)
                {
                    FileInfo[] directoryFiles = di.GetFiles("*.*", SearchOption.AllDirectories);
                    foreach (FileInfo f in directoryFiles)
                    {
                        loadFile(f.FullName);
                    }
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
            for (int i = 0; i < motionListBox.SelectedIndices.Count; i++)
            {
                Form form = new ViewMotionForm((Motion)(motionList[motionListBox.SelectedIndices[i]]));
                form.Show();
                form.Focus();
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

        private void previewPicture_SizeChanged(object sender, EventArgs e)
        {
            updateVisibleImage();
        }
    }
}
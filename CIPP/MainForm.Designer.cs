namespace CIPP
{
    partial class CIPPForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CIPPForm));
            this.topMenu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tutorialToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.processingAreaGroup = new System.Windows.Forms.GroupBox();
            this.updatePlugins = new System.Windows.Forms.Button();
            this.processingTab = new System.Windows.Forms.TabControl();
            this.filteringTab = new System.Windows.Forms.TabPage();
            this.flowLayoutPanelFilterPlugins = new System.Windows.Forms.FlowLayoutPanel();
            this.maskingTab = new System.Windows.Forms.TabPage();
            this.flowLayoutPanelMaskPlugins = new System.Windows.Forms.FlowLayoutPanel();
            this.motionRecognitionTab = new System.Windows.Forms.TabPage();
            this.flowLayoutPanelMotionRecognitionPlugins = new System.Windows.Forms.FlowLayoutPanel();
            this.imageAreaGroup = new System.Windows.Forms.GroupBox();
            this.previewPictureLabel = new System.Windows.Forms.Label();
            this.imagePropertiesGroup = new System.Windows.Forms.GroupBox();
            this.watermarkListBox = new System.Windows.Forms.ListBox();
            this.maskedValueLabel = new System.Windows.Forms.Label();
            this.maskedLabel = new System.Windows.Forms.Label();
            this.grayscaleValueLabel = new System.Windows.Forms.Label();
            this.grayscaleLabel = new System.Windows.Forms.Label();
            this.heightValueLabel = new System.Windows.Forms.Label();
            this.widthValueLabel = new System.Windows.Forms.Label();
            this.heightLabel = new System.Windows.Forms.Label();
            this.widthLabel = new System.Windows.Forms.Label();
            this.previewMotionButton = new System.Windows.Forms.Button();
            this.viewImageButton = new System.Windows.Forms.Button();
            this.sortButton = new System.Windows.Forms.Button();
            this.removeImageButton = new System.Windows.Forms.Button();
            this.moveDownButton = new System.Windows.Forms.Button();
            this.addImageButton = new System.Windows.Forms.Button();
            this.saveImageButton = new System.Windows.Forms.Button();
            this.moveUpButton = new System.Windows.Forms.Button();
            this.previewPicture = new System.Windows.Forms.PictureBox();
            this.imageTab = new System.Windows.Forms.TabControl();
            this.originalImageTab = new System.Windows.Forms.TabPage();
            this.originalImageList = new System.Windows.Forms.ListBox();
            this.processedImageTab = new System.Windows.Forms.TabPage();
            this.processedImageList = new System.Windows.Forms.ListBox();
            this.maskedImageTab = new System.Windows.Forms.TabPage();
            this.maskedImageList = new System.Windows.Forms.ListBox();
            this.scanedImageTab = new System.Windows.Forms.TabPage();
            this.motionList = new System.Windows.Forms.ListBox();
            this.workerControlAreaGroup = new System.Windows.Forms.GroupBox();
            this.workerControlTab = new System.Windows.Forms.TabControl();
            this.localTab = new System.Windows.Forms.TabPage();
            this.localGranularityComboBox = new System.Windows.Forms.ComboBox();
            this.localNumberOfWorkers = new System.Windows.Forms.NumericUpDown();
            this.localGranularityLabel = new System.Windows.Forms.Label();
            this.localNumberOfWorkersLabel = new System.Windows.Forms.Label();
            this.lanTab = new System.Windows.Forms.TabPage();
            this.disconnectTCPConnectionButton = new System.Windows.Forms.Button();
            this.connectTCPConnectionButton = new System.Windows.Forms.Button();
            this.removeTCPConnectionButton = new System.Windows.Forms.Button();
            this.addTCPConnectionButton = new System.Windows.Forms.Button();
            this.TCPConnectionsListBox = new System.Windows.Forms.ListBox();
            this.wanTab = new System.Windows.Forms.TabPage();
            this.workerAreaGroup = new System.Windows.Forms.GroupBox();
            this.clearMessagesListButton = new System.Windows.Forms.Button();
            this.workersListLabel = new System.Windows.Forms.Label();
            this.messagesListLabel = new System.Windows.Forms.Label();
            this.messagesList = new System.Windows.Forms.ListBox();
            this.workersList = new System.Windows.Forms.ListBox();
            this.startButton = new System.Windows.Forms.Button();
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.numberOfCommandsLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.numberOfCommandsValueLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.numberOfTasksLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.numberOfTasksValueLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.timeLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.timeValueLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.totalTimeLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.totalTimeValueLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.finishButton = new System.Windows.Forms.Button();
            this.allertFinishCheckBox = new System.Windows.Forms.CheckBox();
            this.toolTipInfo = new System.Windows.Forms.ToolTip(this.components);
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.topMenu.SuspendLayout();
            this.processingAreaGroup.SuspendLayout();
            this.processingTab.SuspendLayout();
            this.filteringTab.SuspendLayout();
            this.maskingTab.SuspendLayout();
            this.motionRecognitionTab.SuspendLayout();
            this.imageAreaGroup.SuspendLayout();
            this.imagePropertiesGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.previewPicture)).BeginInit();
            this.imageTab.SuspendLayout();
            this.originalImageTab.SuspendLayout();
            this.processedImageTab.SuspendLayout();
            this.maskedImageTab.SuspendLayout();
            this.scanedImageTab.SuspendLayout();
            this.workerControlAreaGroup.SuspendLayout();
            this.workerControlTab.SuspendLayout();
            this.localTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.localNumberOfWorkers)).BeginInit();
            this.lanTab.SuspendLayout();
            this.workerAreaGroup.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // topMenu
            // 
            this.topMenu.BackColor = System.Drawing.SystemColors.ControlDark;
            this.topMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.topMenu.Location = new System.Drawing.Point(0, 0);
            this.topMenu.Name = "topMenu";
            this.topMenu.Size = new System.Drawing.Size(853, 24);
            this.topMenu.TabIndex = 0;
            this.topMenu.Text = "Menu";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tutorialToolStripMenuItem,
            this.aboutToolStripMenuItem1});
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.aboutToolStripMenuItem.Text = "Help";
            // 
            // tutorialToolStripMenuItem
            // 
            this.tutorialToolStripMenuItem.Name = "tutorialToolStripMenuItem";
            this.tutorialToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
            this.tutorialToolStripMenuItem.Text = "Tutorial";
            this.tutorialToolStripMenuItem.Click += new System.EventHandler(this.tutorialToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem1
            // 
            this.aboutToolStripMenuItem1.Name = "aboutToolStripMenuItem1";
            this.aboutToolStripMenuItem1.Size = new System.Drawing.Size(110, 22);
            this.aboutToolStripMenuItem1.Text = "About";
            this.aboutToolStripMenuItem1.Click += new System.EventHandler(this.aboutToolStripMenuItem1_Click);
            // 
            // processingAreaGroup
            // 
            this.processingAreaGroup.Controls.Add(this.updatePlugins);
            this.processingAreaGroup.Controls.Add(this.processingTab);
            this.processingAreaGroup.Location = new System.Drawing.Point(2, 26);
            this.processingAreaGroup.Margin = new System.Windows.Forms.Padding(0);
            this.processingAreaGroup.Name = "processingAreaGroup";
            this.processingAreaGroup.Size = new System.Drawing.Size(295, 368);
            this.processingAreaGroup.TabIndex = 1;
            this.processingAreaGroup.TabStop = false;
            this.processingAreaGroup.Text = "Processing Area";
            // 
            // updatePlugins
            // 
            this.updatePlugins.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.updatePlugins.Location = new System.Drawing.Point(3, 342);
            this.updatePlugins.Margin = new System.Windows.Forms.Padding(0);
            this.updatePlugins.Name = "updatePlugins";
            this.updatePlugins.Size = new System.Drawing.Size(289, 23);
            this.updatePlugins.TabIndex = 1;
            this.updatePlugins.Text = "Update Plugins List";
            this.updatePlugins.UseVisualStyleBackColor = true;
            this.updatePlugins.Click += new System.EventHandler(this.updatePlugins_Click);
            // 
            // processingTab
            // 
            this.processingTab.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.processingTab.Controls.Add(this.filteringTab);
            this.processingTab.Controls.Add(this.maskingTab);
            this.processingTab.Controls.Add(this.motionRecognitionTab);
            this.processingTab.Dock = System.Windows.Forms.DockStyle.Top;
            this.processingTab.Location = new System.Drawing.Point(3, 16);
            this.processingTab.Margin = new System.Windows.Forms.Padding(0);
            this.processingTab.Name = "processingTab";
            this.processingTab.SelectedIndex = 0;
            this.processingTab.Size = new System.Drawing.Size(289, 325);
            this.processingTab.TabIndex = 0;
            // 
            // filteringTab
            // 
            this.filteringTab.Controls.Add(this.flowLayoutPanelFilterPlugins);
            this.filteringTab.Location = new System.Drawing.Point(4, 25);
            this.filteringTab.Name = "filteringTab";
            this.filteringTab.Size = new System.Drawing.Size(281, 296);
            this.filteringTab.TabIndex = 0;
            this.filteringTab.Text = "Filtering";
            this.filteringTab.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanelFilterPlugins
            // 
            this.flowLayoutPanelFilterPlugins.AutoScroll = true;
            this.flowLayoutPanelFilterPlugins.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanelFilterPlugins.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelFilterPlugins.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanelFilterPlugins.Name = "flowLayoutPanelFilterPlugins";
            this.flowLayoutPanelFilterPlugins.Size = new System.Drawing.Size(281, 296);
            this.flowLayoutPanelFilterPlugins.TabIndex = 0;
            // 
            // maskingTab
            // 
            this.maskingTab.Controls.Add(this.flowLayoutPanelMaskPlugins);
            this.maskingTab.Location = new System.Drawing.Point(4, 25);
            this.maskingTab.Margin = new System.Windows.Forms.Padding(0);
            this.maskingTab.Name = "maskingTab";
            this.maskingTab.Size = new System.Drawing.Size(281, 296);
            this.maskingTab.TabIndex = 1;
            this.maskingTab.Text = "Masking";
            this.maskingTab.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanelMaskPlugins
            // 
            this.flowLayoutPanelMaskPlugins.AutoScroll = true;
            this.flowLayoutPanelMaskPlugins.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanelMaskPlugins.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelMaskPlugins.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanelMaskPlugins.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanelMaskPlugins.Name = "flowLayoutPanelMaskPlugins";
            this.flowLayoutPanelMaskPlugins.Size = new System.Drawing.Size(281, 296);
            this.flowLayoutPanelMaskPlugins.TabIndex = 1;
            // 
            // motionRecognitionTab
            // 
            this.motionRecognitionTab.Controls.Add(this.flowLayoutPanelMotionRecognitionPlugins);
            this.motionRecognitionTab.Location = new System.Drawing.Point(4, 25);
            this.motionRecognitionTab.Name = "motionRecognitionTab";
            this.motionRecognitionTab.Size = new System.Drawing.Size(281, 296);
            this.motionRecognitionTab.TabIndex = 2;
            this.motionRecognitionTab.Text = "Motion Recognition";
            this.motionRecognitionTab.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanelMotionRecognitionPlugins
            // 
            this.flowLayoutPanelMotionRecognitionPlugins.AutoScroll = true;
            this.flowLayoutPanelMotionRecognitionPlugins.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanelMotionRecognitionPlugins.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelMotionRecognitionPlugins.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanelMotionRecognitionPlugins.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanelMotionRecognitionPlugins.Name = "flowLayoutPanelMotionRecognitionPlugins";
            this.flowLayoutPanelMotionRecognitionPlugins.Size = new System.Drawing.Size(281, 296);
            this.flowLayoutPanelMotionRecognitionPlugins.TabIndex = 1;
            // 
            // imageAreaGroup
            // 
            this.imageAreaGroup.Controls.Add(this.previewPictureLabel);
            this.imageAreaGroup.Controls.Add(this.imagePropertiesGroup);
            this.imageAreaGroup.Controls.Add(this.previewMotionButton);
            this.imageAreaGroup.Controls.Add(this.viewImageButton);
            this.imageAreaGroup.Controls.Add(this.sortButton);
            this.imageAreaGroup.Controls.Add(this.removeImageButton);
            this.imageAreaGroup.Controls.Add(this.moveDownButton);
            this.imageAreaGroup.Controls.Add(this.addImageButton);
            this.imageAreaGroup.Controls.Add(this.saveImageButton);
            this.imageAreaGroup.Controls.Add(this.moveUpButton);
            this.imageAreaGroup.Controls.Add(this.previewPicture);
            this.imageAreaGroup.Controls.Add(this.imageTab);
            this.imageAreaGroup.Location = new System.Drawing.Point(303, 26);
            this.imageAreaGroup.Margin = new System.Windows.Forms.Padding(0);
            this.imageAreaGroup.Name = "imageAreaGroup";
            this.imageAreaGroup.Padding = new System.Windows.Forms.Padding(2);
            this.imageAreaGroup.Size = new System.Drawing.Size(547, 368);
            this.imageAreaGroup.TabIndex = 2;
            this.imageAreaGroup.TabStop = false;
            this.imageAreaGroup.Text = "Image Area";
            // 
            // previewPictureLabel
            // 
            this.previewPictureLabel.AutoSize = true;
            this.previewPictureLabel.Location = new System.Drawing.Point(221, 17);
            this.previewPictureLabel.Name = "previewPictureLabel";
            this.previewPictureLabel.Size = new System.Drawing.Size(83, 13);
            this.previewPictureLabel.TabIndex = 11;
            this.previewPictureLabel.Text = "Preview picture:";
            // 
            // imagePropertiesGroup
            // 
            this.imagePropertiesGroup.Controls.Add(this.watermarkListBox);
            this.imagePropertiesGroup.Controls.Add(this.maskedValueLabel);
            this.imagePropertiesGroup.Controls.Add(this.maskedLabel);
            this.imagePropertiesGroup.Controls.Add(this.grayscaleValueLabel);
            this.imagePropertiesGroup.Controls.Add(this.grayscaleLabel);
            this.imagePropertiesGroup.Controls.Add(this.heightValueLabel);
            this.imagePropertiesGroup.Controls.Add(this.widthValueLabel);
            this.imagePropertiesGroup.Controls.Add(this.heightLabel);
            this.imagePropertiesGroup.Controls.Add(this.widthLabel);
            this.imagePropertiesGroup.Location = new System.Drawing.Point(2, 301);
            this.imagePropertiesGroup.Name = "imagePropertiesGroup";
            this.imagePropertiesGroup.Size = new System.Drawing.Size(543, 65);
            this.imagePropertiesGroup.TabIndex = 10;
            this.imagePropertiesGroup.TabStop = false;
            this.imagePropertiesGroup.Text = "Image Properties";
            // 
            // watermarkListBox
            // 
            this.watermarkListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.watermarkListBox.FormattingEnabled = true;
            this.watermarkListBox.HorizontalScrollbar = true;
            this.watermarkListBox.IntegralHeight = false;
            this.watermarkListBox.Location = new System.Drawing.Point(166, 9);
            this.watermarkListBox.Margin = new System.Windows.Forms.Padding(0);
            this.watermarkListBox.Name = "watermarkListBox";
            this.watermarkListBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.watermarkListBox.Size = new System.Drawing.Size(373, 54);
            this.watermarkListBox.TabIndex = 9;
            this.toolTipInfo.SetToolTip(this.watermarkListBox, "Contains watermarks from processing plugins");
            // 
            // maskedValueLabel
            // 
            this.maskedValueLabel.AutoSize = true;
            this.maskedValueLabel.Location = new System.Drawing.Point(126, 41);
            this.maskedValueLabel.Name = "maskedValueLabel";
            this.maskedValueLabel.Size = new System.Drawing.Size(0, 13);
            this.maskedValueLabel.TabIndex = 8;
            // 
            // maskedLabel
            // 
            this.maskedLabel.AutoSize = true;
            this.maskedLabel.Location = new System.Drawing.Point(74, 41);
            this.maskedLabel.Name = "maskedLabel";
            this.maskedLabel.Size = new System.Drawing.Size(48, 13);
            this.maskedLabel.TabIndex = 7;
            this.maskedLabel.Text = "Masked:";
            // 
            // grayscaleValueLabel
            // 
            this.grayscaleValueLabel.AutoSize = true;
            this.grayscaleValueLabel.Location = new System.Drawing.Point(126, 18);
            this.grayscaleValueLabel.Name = "grayscaleValueLabel";
            this.grayscaleValueLabel.Size = new System.Drawing.Size(0, 13);
            this.grayscaleValueLabel.TabIndex = 6;
            // 
            // grayscaleLabel
            // 
            this.grayscaleLabel.AutoSize = true;
            this.grayscaleLabel.Location = new System.Drawing.Point(74, 18);
            this.grayscaleLabel.Name = "grayscaleLabel";
            this.grayscaleLabel.Size = new System.Drawing.Size(57, 13);
            this.grayscaleLabel.TabIndex = 5;
            this.grayscaleLabel.Text = "Grayscale:";
            // 
            // heightValueLabel
            // 
            this.heightValueLabel.AutoSize = true;
            this.heightValueLabel.Location = new System.Drawing.Point(44, 42);
            this.heightValueLabel.Name = "heightValueLabel";
            this.heightValueLabel.Size = new System.Drawing.Size(0, 13);
            this.heightValueLabel.TabIndex = 4;
            // 
            // widthValueLabel
            // 
            this.widthValueLabel.AutoSize = true;
            this.widthValueLabel.Location = new System.Drawing.Point(44, 19);
            this.widthValueLabel.Name = "widthValueLabel";
            this.widthValueLabel.Size = new System.Drawing.Size(0, 13);
            this.widthValueLabel.TabIndex = 3;
            // 
            // heightLabel
            // 
            this.heightLabel.AutoSize = true;
            this.heightLabel.Location = new System.Drawing.Point(6, 42);
            this.heightLabel.Name = "heightLabel";
            this.heightLabel.Size = new System.Drawing.Size(41, 13);
            this.heightLabel.TabIndex = 1;
            this.heightLabel.Text = "Height:";
            // 
            // widthLabel
            // 
            this.widthLabel.AutoSize = true;
            this.widthLabel.Location = new System.Drawing.Point(6, 19);
            this.widthLabel.Name = "widthLabel";
            this.widthLabel.Size = new System.Drawing.Size(38, 13);
            this.widthLabel.TabIndex = 0;
            this.widthLabel.Text = "Width:";
            // 
            // previewMotionButton
            // 
            this.previewMotionButton.Enabled = false;
            this.previewMotionButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.previewMotionButton.Location = new System.Drawing.Point(383, 276);
            this.previewMotionButton.Name = "previewMotionButton";
            this.previewMotionButton.Size = new System.Drawing.Size(159, 23);
            this.previewMotionButton.TabIndex = 9;
            this.previewMotionButton.Text = "Preview Motion";
            this.toolTipInfo.SetToolTip(this.previewMotionButton, "Opens a View Motion Window for each motion selected");
            this.previewMotionButton.UseVisualStyleBackColor = true;
            this.previewMotionButton.Click += new System.EventHandler(this.previewMotionButton_Click);
            // 
            // viewImageButton
            // 
            this.viewImageButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.viewImageButton.Location = new System.Drawing.Point(222, 276);
            this.viewImageButton.Name = "viewImageButton";
            this.viewImageButton.Size = new System.Drawing.Size(159, 23);
            this.viewImageButton.TabIndex = 8;
            this.viewImageButton.Text = "Full Image View";
            this.toolTipInfo.SetToolTip(this.viewImageButton, "Opens a View Image Window for each item selected");
            this.viewImageButton.UseVisualStyleBackColor = true;
            this.viewImageButton.Click += new System.EventHandler(this.viewImageButton_Click);
            // 
            // sortButton
            // 
            this.sortButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.sortButton.Location = new System.Drawing.Point(151, 276);
            this.sortButton.Margin = new System.Windows.Forms.Padding(1);
            this.sortButton.Name = "sortButton";
            this.sortButton.Size = new System.Drawing.Size(67, 23);
            this.sortButton.TabIndex = 7;
            this.sortButton.Text = "Sort";
            this.toolTipInfo.SetToolTip(this.sortButton, "Sorts items alphabetically");
            this.sortButton.UseVisualStyleBackColor = true;
            this.sortButton.Click += new System.EventHandler(this.sortButton_Click);
            // 
            // removeImageButton
            // 
            this.removeImageButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.removeImageButton.Location = new System.Drawing.Point(3, 276);
            this.removeImageButton.Margin = new System.Windows.Forms.Padding(1);
            this.removeImageButton.Name = "removeImageButton";
            this.removeImageButton.Size = new System.Drawing.Size(67, 23);
            this.removeImageButton.TabIndex = 6;
            this.removeImageButton.Text = "Remove";
            this.toolTipInfo.SetToolTip(this.removeImageButton, "Removes selected items");
            this.removeImageButton.UseVisualStyleBackColor = true;
            this.removeImageButton.Click += new System.EventHandler(this.removeImageButton_Click);
            // 
            // moveDownButton
            // 
            this.moveDownButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.moveDownButton.Location = new System.Drawing.Point(73, 276);
            this.moveDownButton.Margin = new System.Windows.Forms.Padding(1);
            this.moveDownButton.Name = "moveDownButton";
            this.moveDownButton.Size = new System.Drawing.Size(75, 23);
            this.moveDownButton.TabIndex = 5;
            this.moveDownButton.Text = "Move Down";
            this.toolTipInfo.SetToolTip(this.moveDownButton, "Moves selected items down in the list");
            this.moveDownButton.UseVisualStyleBackColor = true;
            this.moveDownButton.Click += new System.EventHandler(this.moveDownButton_Click);
            // 
            // addImageButton
            // 
            this.addImageButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.addImageButton.Location = new System.Drawing.Point(3, 251);
            this.addImageButton.Margin = new System.Windows.Forms.Padding(1);
            this.addImageButton.Name = "addImageButton";
            this.addImageButton.Size = new System.Drawing.Size(67, 23);
            this.addImageButton.TabIndex = 4;
            this.addImageButton.Text = "Add";
            this.toolTipInfo.SetToolTip(this.addImageButton, "Adds one or images from disk files");
            this.addImageButton.UseVisualStyleBackColor = true;
            this.addImageButton.Click += new System.EventHandler(this.addImageButton_Click);
            // 
            // saveImageButton
            // 
            this.saveImageButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.saveImageButton.Location = new System.Drawing.Point(151, 251);
            this.saveImageButton.Margin = new System.Windows.Forms.Padding(1);
            this.saveImageButton.Name = "saveImageButton";
            this.saveImageButton.Size = new System.Drawing.Size(67, 23);
            this.saveImageButton.TabIndex = 3;
            this.saveImageButton.Text = "Save";
            this.toolTipInfo.SetToolTip(this.saveImageButton, "Saves selected items");
            this.saveImageButton.UseVisualStyleBackColor = true;
            this.saveImageButton.Click += new System.EventHandler(this.saveImageButton_Click);
            // 
            // moveUpButton
            // 
            this.moveUpButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.moveUpButton.Location = new System.Drawing.Point(73, 251);
            this.moveUpButton.Margin = new System.Windows.Forms.Padding(1);
            this.moveUpButton.Name = "moveUpButton";
            this.moveUpButton.Size = new System.Drawing.Size(75, 23);
            this.moveUpButton.TabIndex = 2;
            this.moveUpButton.Text = "Move Up";
            this.toolTipInfo.SetToolTip(this.moveUpButton, "Moves selected items up in the list");
            this.moveUpButton.UseVisualStyleBackColor = true;
            this.moveUpButton.Click += new System.EventHandler(this.moveUpButton_Click);
            // 
            // previewPicture
            // 
            this.previewPicture.BackColor = System.Drawing.SystemColors.Control;
            this.previewPicture.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.previewPicture.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.previewPicture.Location = new System.Drawing.Point(222, 34);
            this.previewPicture.Margin = new System.Windows.Forms.Padding(1);
            this.previewPicture.Name = "previewPicture";
            this.previewPicture.Size = new System.Drawing.Size(320, 240);
            this.previewPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.previewPicture.TabIndex = 1;
            this.previewPicture.TabStop = false;
            // 
            // imageTab
            // 
            this.imageTab.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.imageTab.Controls.Add(this.originalImageTab);
            this.imageTab.Controls.Add(this.processedImageTab);
            this.imageTab.Controls.Add(this.maskedImageTab);
            this.imageTab.Controls.Add(this.scanedImageTab);
            this.imageTab.Location = new System.Drawing.Point(3, 15);
            this.imageTab.Margin = new System.Windows.Forms.Padding(0);
            this.imageTab.Name = "imageTab";
            this.imageTab.Padding = new System.Drawing.Point(0, 0);
            this.imageTab.SelectedIndex = 0;
            this.imageTab.ShowToolTips = true;
            this.imageTab.Size = new System.Drawing.Size(215, 235);
            this.imageTab.TabIndex = 0;
            this.imageTab.SelectedIndexChanged += new System.EventHandler(this.imageTab_SelectedIndexChanged);
            // 
            // originalImageTab
            // 
            this.originalImageTab.Controls.Add(this.originalImageList);
            this.originalImageTab.Location = new System.Drawing.Point(4, 25);
            this.originalImageTab.Margin = new System.Windows.Forms.Padding(0);
            this.originalImageTab.Name = "originalImageTab";
            this.originalImageTab.Size = new System.Drawing.Size(207, 206);
            this.originalImageTab.TabIndex = 0;
            this.originalImageTab.Text = "original";
            this.originalImageTab.UseVisualStyleBackColor = true;
            // 
            // originalImageList
            // 
            this.originalImageList.AllowDrop = true;
            this.originalImageList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.originalImageList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.originalImageList.FormattingEnabled = true;
            this.originalImageList.HorizontalScrollbar = true;
            this.originalImageList.IntegralHeight = false;
            this.originalImageList.Location = new System.Drawing.Point(0, 0);
            this.originalImageList.Margin = new System.Windows.Forms.Padding(0);
            this.originalImageList.Name = "originalImageList";
            this.originalImageList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.originalImageList.Size = new System.Drawing.Size(207, 206);
            this.originalImageList.TabIndex = 0;
            this.originalImageList.SelectedIndexChanged += new System.EventHandler(this.originalImageList_SelectedIndexChanged);
            this.originalImageList.DragDrop += new System.Windows.Forms.DragEventHandler(this.originalImageList_DragDrop);
            this.originalImageList.DragEnter += new System.Windows.Forms.DragEventHandler(this.originalImageList_DragEnter);
            // 
            // processedImageTab
            // 
            this.processedImageTab.Controls.Add(this.processedImageList);
            this.processedImageTab.Location = new System.Drawing.Point(4, 25);
            this.processedImageTab.Margin = new System.Windows.Forms.Padding(0);
            this.processedImageTab.Name = "processedImageTab";
            this.processedImageTab.Size = new System.Drawing.Size(207, 206);
            this.processedImageTab.TabIndex = 1;
            this.processedImageTab.Text = "processed";
            this.processedImageTab.UseVisualStyleBackColor = true;
            // 
            // processedImageList
            // 
            this.processedImageList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.processedImageList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.processedImageList.FormattingEnabled = true;
            this.processedImageList.IntegralHeight = false;
            this.processedImageList.Location = new System.Drawing.Point(0, 0);
            this.processedImageList.Margin = new System.Windows.Forms.Padding(0);
            this.processedImageList.Name = "processedImageList";
            this.processedImageList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.processedImageList.Size = new System.Drawing.Size(207, 206);
            this.processedImageList.TabIndex = 1;
            this.processedImageList.SelectedIndexChanged += new System.EventHandler(this.originalImageList_SelectedIndexChanged);
            // 
            // maskedImageTab
            // 
            this.maskedImageTab.Controls.Add(this.maskedImageList);
            this.maskedImageTab.Location = new System.Drawing.Point(4, 25);
            this.maskedImageTab.Name = "maskedImageTab";
            this.maskedImageTab.Size = new System.Drawing.Size(207, 206);
            this.maskedImageTab.TabIndex = 2;
            this.maskedImageTab.Text = "masked";
            this.maskedImageTab.UseVisualStyleBackColor = true;
            // 
            // maskedImageList
            // 
            this.maskedImageList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.maskedImageList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.maskedImageList.FormattingEnabled = true;
            this.maskedImageList.IntegralHeight = false;
            this.maskedImageList.Location = new System.Drawing.Point(0, 0);
            this.maskedImageList.Name = "maskedImageList";
            this.maskedImageList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.maskedImageList.Size = new System.Drawing.Size(207, 206);
            this.maskedImageList.TabIndex = 1;
            this.maskedImageList.SelectedIndexChanged += new System.EventHandler(this.originalImageList_SelectedIndexChanged);
            // 
            // scanedImageTab
            // 
            this.scanedImageTab.Controls.Add(this.motionList);
            this.scanedImageTab.Location = new System.Drawing.Point(4, 25);
            this.scanedImageTab.Name = "scanedImageTab";
            this.scanedImageTab.Size = new System.Drawing.Size(207, 206);
            this.scanedImageTab.TabIndex = 3;
            this.scanedImageTab.Text = "scaned";
            this.scanedImageTab.UseVisualStyleBackColor = true;
            // 
            // motionList
            // 
            this.motionList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.motionList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.motionList.FormattingEnabled = true;
            this.motionList.IntegralHeight = false;
            this.motionList.Location = new System.Drawing.Point(0, 0);
            this.motionList.Margin = new System.Windows.Forms.Padding(0);
            this.motionList.Name = "motionList";
            this.motionList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.motionList.Size = new System.Drawing.Size(207, 206);
            this.motionList.TabIndex = 1;
            // 
            // workerControlAreaGroup
            // 
            this.workerControlAreaGroup.Controls.Add(this.workerControlTab);
            this.workerControlAreaGroup.Location = new System.Drawing.Point(2, 394);
            this.workerControlAreaGroup.Name = "workerControlAreaGroup";
            this.workerControlAreaGroup.Size = new System.Drawing.Size(295, 142);
            this.workerControlAreaGroup.TabIndex = 2;
            this.workerControlAreaGroup.TabStop = false;
            this.workerControlAreaGroup.Text = "Worker Control Area";
            // 
            // workerControlTab
            // 
            this.workerControlTab.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.workerControlTab.Controls.Add(this.localTab);
            this.workerControlTab.Controls.Add(this.lanTab);
            this.workerControlTab.Controls.Add(this.wanTab);
            this.workerControlTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.workerControlTab.Location = new System.Drawing.Point(3, 16);
            this.workerControlTab.Name = "workerControlTab";
            this.workerControlTab.SelectedIndex = 0;
            this.workerControlTab.Size = new System.Drawing.Size(289, 123);
            this.workerControlTab.TabIndex = 1;
            // 
            // localTab
            // 
            this.localTab.Controls.Add(this.localGranularityComboBox);
            this.localTab.Controls.Add(this.localNumberOfWorkers);
            this.localTab.Controls.Add(this.localGranularityLabel);
            this.localTab.Controls.Add(this.localNumberOfWorkersLabel);
            this.localTab.Location = new System.Drawing.Point(4, 25);
            this.localTab.Name = "localTab";
            this.localTab.Padding = new System.Windows.Forms.Padding(3);
            this.localTab.Size = new System.Drawing.Size(281, 94);
            this.localTab.TabIndex = 0;
            this.localTab.Text = "Local Config.";
            this.localTab.UseVisualStyleBackColor = true;
            // 
            // localGranularityComboBox
            // 
            this.localGranularityComboBox.DisplayMember = "1";
            this.localGranularityComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.localGranularityComboBox.FormattingEnabled = true;
            this.localGranularityComboBox.Items.AddRange(new object[] {
            "Low",
            "Medium",
            "Maximum"});
            this.localGranularityComboBox.Location = new System.Drawing.Point(110, 35);
            this.localGranularityComboBox.Name = "localGranularityComboBox";
            this.localGranularityComboBox.Size = new System.Drawing.Size(78, 21);
            this.localGranularityComboBox.TabIndex = 3;
            this.localGranularityComboBox.ValueMember = "1";
            // 
            // localNumberOfWorkers
            // 
            this.localNumberOfWorkers.BackColor = System.Drawing.SystemColors.Window;
            this.localNumberOfWorkers.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.localNumberOfWorkers.Location = new System.Drawing.Point(110, 8);
            this.localNumberOfWorkers.Name = "localNumberOfWorkers";
            this.localNumberOfWorkers.ReadOnly = true;
            this.localNumberOfWorkers.Size = new System.Drawing.Size(40, 20);
            this.localNumberOfWorkers.TabIndex = 2;
            this.localNumberOfWorkers.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // localGranularityLabel
            // 
            this.localGranularityLabel.AutoSize = true;
            this.localGranularityLabel.Location = new System.Drawing.Point(7, 41);
            this.localGranularityLabel.Name = "localGranularityLabel";
            this.localGranularityLabel.Size = new System.Drawing.Size(60, 13);
            this.localGranularityLabel.TabIndex = 1;
            this.localGranularityLabel.Text = "Granularity:";
            // 
            // localNumberOfWorkersLabel
            // 
            this.localNumberOfWorkersLabel.AutoSize = true;
            this.localNumberOfWorkersLabel.Location = new System.Drawing.Point(7, 11);
            this.localNumberOfWorkersLabel.Name = "localNumberOfWorkersLabel";
            this.localNumberOfWorkersLabel.Size = new System.Drawing.Size(99, 13);
            this.localNumberOfWorkersLabel.TabIndex = 0;
            this.localNumberOfWorkersLabel.Text = "Number of workers:";
            // 
            // lanTab
            // 
            this.lanTab.Controls.Add(this.disconnectTCPConnectionButton);
            this.lanTab.Controls.Add(this.connectTCPConnectionButton);
            this.lanTab.Controls.Add(this.removeTCPConnectionButton);
            this.lanTab.Controls.Add(this.addTCPConnectionButton);
            this.lanTab.Controls.Add(this.TCPConnectionsListBox);
            this.lanTab.Location = new System.Drawing.Point(4, 25);
            this.lanTab.Margin = new System.Windows.Forms.Padding(0);
            this.lanTab.Name = "lanTab";
            this.lanTab.Size = new System.Drawing.Size(281, 94);
            this.lanTab.TabIndex = 1;
            this.lanTab.Text = "LAN Config.";
            this.lanTab.UseVisualStyleBackColor = true;
            // 
            // disconnectTCPConnectionButton
            // 
            this.disconnectTCPConnectionButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.disconnectTCPConnectionButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.disconnectTCPConnectionButton.Location = new System.Drawing.Point(206, 71);
            this.disconnectTCPConnectionButton.Name = "disconnectTCPConnectionButton";
            this.disconnectTCPConnectionButton.Size = new System.Drawing.Size(75, 23);
            this.disconnectTCPConnectionButton.TabIndex = 4;
            this.disconnectTCPConnectionButton.Text = "Disconnect";
            this.disconnectTCPConnectionButton.UseVisualStyleBackColor = true;
            this.disconnectTCPConnectionButton.Click += new System.EventHandler(this.disconnectTCPConnectionButton_Click);
            // 
            // connectTCPConnectionButton
            // 
            this.connectTCPConnectionButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.connectTCPConnectionButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.connectTCPConnectionButton.Location = new System.Drawing.Point(206, 47);
            this.connectTCPConnectionButton.Name = "connectTCPConnectionButton";
            this.connectTCPConnectionButton.Size = new System.Drawing.Size(75, 23);
            this.connectTCPConnectionButton.TabIndex = 3;
            this.connectTCPConnectionButton.Text = "Connect";
            this.connectTCPConnectionButton.UseVisualStyleBackColor = true;
            this.connectTCPConnectionButton.Click += new System.EventHandler(this.connectTCPConnectionButton_Click);
            // 
            // removeTCPConnectionButton
            // 
            this.removeTCPConnectionButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.removeTCPConnectionButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.removeTCPConnectionButton.Location = new System.Drawing.Point(206, 23);
            this.removeTCPConnectionButton.Name = "removeTCPConnectionButton";
            this.removeTCPConnectionButton.Size = new System.Drawing.Size(75, 23);
            this.removeTCPConnectionButton.TabIndex = 2;
            this.removeTCPConnectionButton.Text = "Remove";
            this.removeTCPConnectionButton.UseVisualStyleBackColor = true;
            this.removeTCPConnectionButton.Click += new System.EventHandler(this.removeTCPConnectionButton_Click);
            // 
            // addTCPConnectionButton
            // 
            this.addTCPConnectionButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.addTCPConnectionButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.addTCPConnectionButton.Location = new System.Drawing.Point(206, -1);
            this.addTCPConnectionButton.Name = "addTCPConnectionButton";
            this.addTCPConnectionButton.Size = new System.Drawing.Size(75, 23);
            this.addTCPConnectionButton.TabIndex = 1;
            this.addTCPConnectionButton.Text = "Add";
            this.addTCPConnectionButton.UseVisualStyleBackColor = true;
            this.addTCPConnectionButton.Click += new System.EventHandler(this.addTCPConnectionButton_Click);
            // 
            // TCPConnectionsListBox
            // 
            this.TCPConnectionsListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TCPConnectionsListBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.TCPConnectionsListBox.FormattingEnabled = true;
            this.TCPConnectionsListBox.HorizontalScrollbar = true;
            this.TCPConnectionsListBox.IntegralHeight = false;
            this.TCPConnectionsListBox.Location = new System.Drawing.Point(0, 0);
            this.TCPConnectionsListBox.Margin = new System.Windows.Forms.Padding(0);
            this.TCPConnectionsListBox.Name = "TCPConnectionsListBox";
            this.TCPConnectionsListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.TCPConnectionsListBox.Size = new System.Drawing.Size(205, 94);
            this.TCPConnectionsListBox.TabIndex = 0;
            // 
            // wanTab
            // 
            this.wanTab.Location = new System.Drawing.Point(4, 25);
            this.wanTab.Name = "wanTab";
            this.wanTab.Size = new System.Drawing.Size(281, 94);
            this.wanTab.TabIndex = 2;
            this.wanTab.Text = "WAN Config.";
            this.wanTab.UseVisualStyleBackColor = true;
            // 
            // workerAreaGroup
            // 
            this.workerAreaGroup.Controls.Add(this.clearMessagesListButton);
            this.workerAreaGroup.Controls.Add(this.workersListLabel);
            this.workerAreaGroup.Controls.Add(this.messagesListLabel);
            this.workerAreaGroup.Controls.Add(this.messagesList);
            this.workerAreaGroup.Controls.Add(this.workersList);
            this.workerAreaGroup.Location = new System.Drawing.Point(303, 394);
            this.workerAreaGroup.Name = "workerAreaGroup";
            this.workerAreaGroup.Size = new System.Drawing.Size(547, 188);
            this.workerAreaGroup.TabIndex = 3;
            this.workerAreaGroup.TabStop = false;
            this.workerAreaGroup.Text = "Worker Area";
            // 
            // clearMessagesListButton
            // 
            this.clearMessagesListButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.clearMessagesListButton.Location = new System.Drawing.Point(210, 162);
            this.clearMessagesListButton.Margin = new System.Windows.Forms.Padding(0);
            this.clearMessagesListButton.Name = "clearMessagesListButton";
            this.clearMessagesListButton.Size = new System.Drawing.Size(333, 23);
            this.clearMessagesListButton.TabIndex = 4;
            this.clearMessagesListButton.Text = "Clear";
            this.toolTipInfo.SetToolTip(this.clearMessagesListButton, "Clears all entries in the above list");
            this.clearMessagesListButton.UseVisualStyleBackColor = true;
            this.clearMessagesListButton.Click += new System.EventHandler(this.clearMessagesListButton_Click);
            // 
            // workersListLabel
            // 
            this.workersListLabel.AutoSize = true;
            this.workersListLabel.Location = new System.Drawing.Point(2, 14);
            this.workersListLabel.Margin = new System.Windows.Forms.Padding(0);
            this.workersListLabel.Name = "workersListLabel";
            this.workersListLabel.Size = new System.Drawing.Size(69, 13);
            this.workersListLabel.TabIndex = 3;
            this.workersListLabel.Text = "Workers List:";
            // 
            // messagesListLabel
            // 
            this.messagesListLabel.AutoSize = true;
            this.messagesListLabel.Location = new System.Drawing.Point(207, 14);
            this.messagesListLabel.Margin = new System.Windows.Forms.Padding(0);
            this.messagesListLabel.Name = "messagesListLabel";
            this.messagesListLabel.Size = new System.Drawing.Size(142, 13);
            this.messagesListLabel.TabIndex = 2;
            this.messagesListLabel.Text = "Messages between workers:";
            // 
            // messagesList
            // 
            this.messagesList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.messagesList.FormattingEnabled = true;
            this.messagesList.HorizontalScrollbar = true;
            this.messagesList.IntegralHeight = false;
            this.messagesList.Location = new System.Drawing.Point(210, 30);
            this.messagesList.Margin = new System.Windows.Forms.Padding(0);
            this.messagesList.Name = "messagesList";
            this.messagesList.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.messagesList.Size = new System.Drawing.Size(333, 130);
            this.messagesList.TabIndex = 1;
            this.toolTipInfo.SetToolTip(this.messagesList, "List of messages switched between workers");
            // 
            // workersList
            // 
            this.workersList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.workersList.FormattingEnabled = true;
            this.workersList.HorizontalScrollbar = true;
            this.workersList.IntegralHeight = false;
            this.workersList.Location = new System.Drawing.Point(5, 30);
            this.workersList.Margin = new System.Windows.Forms.Padding(0);
            this.workersList.Name = "workersList";
            this.workersList.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.workersList.Size = new System.Drawing.Size(202, 155);
            this.workersList.TabIndex = 0;
            this.toolTipInfo.SetToolTip(this.workersList, "List of currently employed workers");
            // 
            // startButton
            // 
            this.startButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.startButton.Location = new System.Drawing.Point(2, 559);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(147, 23);
            this.startButton.TabIndex = 4;
            this.startButton.Text = "Start";
            this.toolTipInfo.SetToolTip(this.startButton, "Adds current commands to the work queue");
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // statusBar
            // 
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.numberOfCommandsLabel,
            this.numberOfCommandsValueLabel,
            this.numberOfTasksLabel,
            this.numberOfTasksValueLabel,
            this.timeLabel,
            this.timeValueLabel,
            this.totalTimeLabel,
            this.totalTimeValueLabel});
            this.statusBar.Location = new System.Drawing.Point(0, 584);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(853, 22);
            this.statusBar.SizingGrip = false;
            this.statusBar.TabIndex = 5;
            this.statusBar.Text = "statusBar";
            // 
            // numberOfCommandsLabel
            // 
            this.numberOfCommandsLabel.Name = "numberOfCommandsLabel";
            this.numberOfCommandsLabel.Size = new System.Drawing.Size(63, 17);
            this.numberOfCommandsLabel.Text = "Commands:";
            // 
            // numberOfCommandsValueLabel
            // 
            this.numberOfCommandsValueLabel.Name = "numberOfCommandsValueLabel";
            this.numberOfCommandsValueLabel.Size = new System.Drawing.Size(13, 17);
            this.numberOfCommandsValueLabel.Text = "0";
            // 
            // numberOfTasksLabel
            // 
            this.numberOfTasksLabel.Name = "numberOfTasksLabel";
            this.numberOfTasksLabel.Size = new System.Drawing.Size(38, 17);
            this.numberOfTasksLabel.Text = "Tasks:";
            // 
            // numberOfTasksValueLabel
            // 
            this.numberOfTasksValueLabel.Name = "numberOfTasksValueLabel";
            this.numberOfTasksValueLabel.Size = new System.Drawing.Size(13, 17);
            this.numberOfTasksValueLabel.Text = "0";
            // 
            // timeLabel
            // 
            this.timeLabel.Name = "timeLabel";
            this.timeLabel.Size = new System.Drawing.Size(81, 17);
            this.timeLabel.Text = "Execution time:";
            // 
            // timeValueLabel
            // 
            this.timeValueLabel.Name = "timeValueLabel";
            this.timeValueLabel.Size = new System.Drawing.Size(13, 17);
            this.timeValueLabel.Text = "0";
            // 
            // totalTimeLabel
            // 
            this.totalTimeLabel.Name = "totalTimeLabel";
            this.totalTimeLabel.Size = new System.Drawing.Size(58, 17);
            this.totalTimeLabel.Text = "Total time:";
            // 
            // totalTimeValueLabel
            // 
            this.totalTimeValueLabel.Name = "totalTimeValueLabel";
            this.totalTimeValueLabel.Size = new System.Drawing.Size(13, 17);
            this.totalTimeValueLabel.Text = "0";
            // 
            // finishButton
            // 
            this.finishButton.Enabled = false;
            this.finishButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.finishButton.Location = new System.Drawing.Point(150, 559);
            this.finishButton.Name = "finishButton";
            this.finishButton.Size = new System.Drawing.Size(147, 23);
            this.finishButton.TabIndex = 6;
            this.finishButton.Text = "Finish";
            this.toolTipInfo.SetToolTip(this.finishButton, "Stops all work in progress and clears work queue");
            this.finishButton.UseVisualStyleBackColor = true;
            this.finishButton.Click += new System.EventHandler(this.finishButton_Click);
            // 
            // allertFinishCheckBox
            // 
            this.allertFinishCheckBox.AutoSize = true;
            this.allertFinishCheckBox.Location = new System.Drawing.Point(3, 539);
            this.allertFinishCheckBox.Name = "allertFinishCheckBox";
            this.allertFinishCheckBox.Size = new System.Drawing.Size(152, 17);
            this.allertFinishCheckBox.TabIndex = 7;
            this.allertFinishCheckBox.Text = "show message when done";
            this.allertFinishCheckBox.UseVisualStyleBackColor = true;
            // 
            // toolTipInfo
            // 
            this.toolTipInfo.AutomaticDelay = 1000;
            this.toolTipInfo.AutoPopDelay = 10000;
            this.toolTipInfo.InitialDelay = 1000;
            this.toolTipInfo.IsBalloon = true;
            this.toolTipInfo.ReshowDelay = 1000;
            // 
            // openFileDialog
            // 
            this.openFileDialog.Multiselect = true;
            // 
            // timer
            // 
            this.timer.Interval = 1000;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // folderBrowserDialog
            // 
            this.folderBrowserDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
            // 
            // CIPPForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(853, 606);
            this.Controls.Add(this.allertFinishCheckBox);
            this.Controls.Add(this.finishButton);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.workerAreaGroup);
            this.Controls.Add(this.workerControlAreaGroup);
            this.Controls.Add(this.imageAreaGroup);
            this.Controls.Add(this.processingAreaGroup);
            this.Controls.Add(this.topMenu);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.topMenu;
            this.MaximizeBox = false;
            this.Name = "CIPPForm";
            this.Text = "CIPP - Compact Image Processing Platform";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CIPPForm_FormClosing);
            this.topMenu.ResumeLayout(false);
            this.topMenu.PerformLayout();
            this.processingAreaGroup.ResumeLayout(false);
            this.processingTab.ResumeLayout(false);
            this.filteringTab.ResumeLayout(false);
            this.maskingTab.ResumeLayout(false);
            this.motionRecognitionTab.ResumeLayout(false);
            this.imageAreaGroup.ResumeLayout(false);
            this.imageAreaGroup.PerformLayout();
            this.imagePropertiesGroup.ResumeLayout(false);
            this.imagePropertiesGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.previewPicture)).EndInit();
            this.imageTab.ResumeLayout(false);
            this.originalImageTab.ResumeLayout(false);
            this.processedImageTab.ResumeLayout(false);
            this.maskedImageTab.ResumeLayout(false);
            this.scanedImageTab.ResumeLayout(false);
            this.workerControlAreaGroup.ResumeLayout(false);
            this.workerControlTab.ResumeLayout(false);
            this.localTab.ResumeLayout(false);
            this.localTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.localNumberOfWorkers)).EndInit();
            this.lanTab.ResumeLayout(false);
            this.workerAreaGroup.ResumeLayout(false);
            this.workerAreaGroup.PerformLayout();
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip topMenu;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tutorialToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem1;
        private System.Windows.Forms.GroupBox processingAreaGroup;
        private System.Windows.Forms.GroupBox imageAreaGroup;
        private System.Windows.Forms.GroupBox workerControlAreaGroup;
        private System.Windows.Forms.GroupBox workerAreaGroup;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.Button finishButton;
        private System.Windows.Forms.CheckBox allertFinishCheckBox;
        private System.Windows.Forms.Button updatePlugins;
        private System.Windows.Forms.TabControl processingTab;
        private System.Windows.Forms.TabPage filteringTab;
        private System.Windows.Forms.TabPage maskingTab;
        private System.Windows.Forms.TabPage motionRecognitionTab;
        private System.Windows.Forms.ToolTip toolTipInfo;
        private System.Windows.Forms.TabControl workerControlTab;
        private System.Windows.Forms.TabPage localTab;
        private System.Windows.Forms.TabPage lanTab;
        private System.Windows.Forms.TabPage wanTab;
        private System.Windows.Forms.PictureBox previewPicture;
        private System.Windows.Forms.TabControl imageTab;
        private System.Windows.Forms.TabPage originalImageTab;
        private System.Windows.Forms.TabPage processedImageTab;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.ListBox messagesList;
        private System.Windows.Forms.ListBox workersList;
        private System.Windows.Forms.Button saveImageButton;
        private System.Windows.Forms.Button moveUpButton;
        private System.Windows.Forms.Button previewMotionButton;
        private System.Windows.Forms.Button viewImageButton;
        private System.Windows.Forms.Button sortButton;
        private System.Windows.Forms.Button removeImageButton;
        private System.Windows.Forms.Button moveDownButton;
        private System.Windows.Forms.Button addImageButton;
        private System.Windows.Forms.GroupBox imagePropertiesGroup;
        private System.Windows.Forms.TabPage maskedImageTab;
        private System.Windows.Forms.TabPage scanedImageTab;
        private System.Windows.Forms.Label messagesListLabel;
        private System.Windows.Forms.Label heightLabel;
        private System.Windows.Forms.Label widthLabel;
        private System.Windows.Forms.Label heightValueLabel;
        private System.Windows.Forms.Label widthValueLabel;
        private System.Windows.Forms.ListBox originalImageList;
        private System.Windows.Forms.ListBox processedImageList;
        private System.Windows.Forms.ListBox maskedImageList;
        private System.Windows.Forms.ListBox motionList;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelFilterPlugins;
        private System.Windows.Forms.Label localGranularityLabel;
        private System.Windows.Forms.Label localNumberOfWorkersLabel;
        private System.Windows.Forms.NumericUpDown localNumberOfWorkers;
        private System.Windows.Forms.Label workersListLabel;
        private System.Windows.Forms.Label grayscaleLabel;
        private System.Windows.Forms.Label grayscaleValueLabel;
        private System.Windows.Forms.Label maskedValueLabel;
        private System.Windows.Forms.Label maskedLabel;
        private System.Windows.Forms.ListBox watermarkListBox;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelMaskPlugins;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelMotionRecognitionPlugins;
        private System.Windows.Forms.ComboBox localGranularityComboBox;
        private System.Windows.Forms.Button clearMessagesListButton;
        private System.Windows.Forms.Button disconnectTCPConnectionButton;
        private System.Windows.Forms.Button connectTCPConnectionButton;
        private System.Windows.Forms.Button removeTCPConnectionButton;
        private System.Windows.Forms.Button addTCPConnectionButton;
        private System.Windows.Forms.ListBox TCPConnectionsListBox;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.ToolStripStatusLabel numberOfCommandsLabel;
        private System.Windows.Forms.ToolStripStatusLabel numberOfCommandsValueLabel;
        private System.Windows.Forms.ToolStripStatusLabel numberOfTasksLabel;
        private System.Windows.Forms.ToolStripStatusLabel numberOfTasksValueLabel;
        private System.Windows.Forms.ToolStripStatusLabel timeLabel;
        private System.Windows.Forms.ToolStripStatusLabel timeValueLabel;
        private System.Windows.Forms.ToolStripStatusLabel totalTimeLabel;
        private System.Windows.Forms.ToolStripStatusLabel totalTimeValueLabel;
        private System.Windows.Forms.Label previewPictureLabel;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
    }
}


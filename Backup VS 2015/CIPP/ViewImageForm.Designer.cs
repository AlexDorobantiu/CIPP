namespace CIPP
{
    partial class ViewImageForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewImageForm));
            this.redCheckBox = new System.Windows.Forms.CheckBox();
            this.greenCheckBox = new System.Windows.Forms.CheckBox();
            this.blueCheckBox = new System.Windows.Forms.CheckBox();
            this.grayCheckButton = new System.Windows.Forms.CheckBox();
            this.luminanceCheckButton = new System.Windows.Forms.CheckBox();
            this.piBox = new System.Windows.Forms.PictureBox();
            this.alphaCheckBox = new System.Windows.Forms.CheckBox();
            this.checkerCheckBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.piBox)).BeginInit();
            this.SuspendLayout();
            // 
            // redCheckBox
            // 
            this.redCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
            this.redCheckBox.Checked = true;
            this.redCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.redCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.redCheckBox.Location = new System.Drawing.Point(78, 2);
            this.redCheckBox.Name = "redCheckBox";
            this.redCheckBox.Size = new System.Drawing.Size(69, 23);
            this.redCheckBox.TabIndex = 0;
            this.redCheckBox.Text = "Red";
            this.redCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.redCheckBox.UseVisualStyleBackColor = true;
            this.redCheckBox.CheckedChanged += new System.EventHandler(this.redCheckBox_CheckedChanged);
            // 
            // greenCheckBox
            // 
            this.greenCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
            this.greenCheckBox.Checked = true;
            this.greenCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.greenCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.greenCheckBox.Location = new System.Drawing.Point(153, 2);
            this.greenCheckBox.Name = "greenCheckBox";
            this.greenCheckBox.Size = new System.Drawing.Size(69, 23);
            this.greenCheckBox.TabIndex = 1;
            this.greenCheckBox.Text = "Green";
            this.greenCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.greenCheckBox.UseVisualStyleBackColor = true;
            this.greenCheckBox.CheckedChanged += new System.EventHandler(this.greenCheckBox_CheckedChanged);
            // 
            // blueCheckBox
            // 
            this.blueCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
            this.blueCheckBox.Checked = true;
            this.blueCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.blueCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.blueCheckBox.Location = new System.Drawing.Point(228, 2);
            this.blueCheckBox.Name = "blueCheckBox";
            this.blueCheckBox.Size = new System.Drawing.Size(69, 23);
            this.blueCheckBox.TabIndex = 2;
            this.blueCheckBox.Text = "Blue";
            this.blueCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.blueCheckBox.UseVisualStyleBackColor = true;
            this.blueCheckBox.CheckedChanged += new System.EventHandler(this.blueCheckBox_CheckedChanged);
            // 
            // grayCheckButton
            // 
            this.grayCheckButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.grayCheckButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.grayCheckButton.Location = new System.Drawing.Point(303, 2);
            this.grayCheckButton.Name = "grayCheckButton";
            this.grayCheckButton.Size = new System.Drawing.Size(69, 23);
            this.grayCheckButton.TabIndex = 3;
            this.grayCheckButton.Text = "Gray";
            this.grayCheckButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.grayCheckButton.UseVisualStyleBackColor = true;
            this.grayCheckButton.CheckedChanged += new System.EventHandler(this.grayCheckButton_CheckedChanged);
            // 
            // luminanceCheckButton
            // 
            this.luminanceCheckButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.luminanceCheckButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.luminanceCheckButton.Location = new System.Drawing.Point(378, 2);
            this.luminanceCheckButton.Name = "luminanceCheckButton";
            this.luminanceCheckButton.Size = new System.Drawing.Size(69, 23);
            this.luminanceCheckButton.TabIndex = 4;
            this.luminanceCheckButton.Text = "Luminance";
            this.luminanceCheckButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.luminanceCheckButton.UseVisualStyleBackColor = true;
            this.luminanceCheckButton.CheckedChanged += new System.EventHandler(this.luminanceCheckButton_CheckedChanged);
            // 
            // piBox
            // 
            this.piBox.BackColor = System.Drawing.Color.Black;
            this.piBox.Location = new System.Drawing.Point(3, 31);
            this.piBox.Name = "piBox";
            this.piBox.Size = new System.Drawing.Size(16, 16);
            this.piBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.piBox.TabIndex = 5;
            this.piBox.TabStop = false;
            // 
            // alphaCheckBox
            // 
            this.alphaCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
            this.alphaCheckBox.Checked = true;
            this.alphaCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.alphaCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.alphaCheckBox.Location = new System.Drawing.Point(3, 2);
            this.alphaCheckBox.Name = "alphaCheckBox";
            this.alphaCheckBox.Size = new System.Drawing.Size(69, 23);
            this.alphaCheckBox.TabIndex = 6;
            this.alphaCheckBox.Text = "Alpha";
            this.alphaCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.alphaCheckBox.UseVisualStyleBackColor = true;
            this.alphaCheckBox.CheckedChanged += new System.EventHandler(this.alphaCheckBox_CheckedChanged);
            // 
            // checkerCheckBox
            // 
            this.checkerCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkerCheckBox.Location = new System.Drawing.Point(453, 2);
            this.checkerCheckBox.Name = "checkerCheckBox";
            this.checkerCheckBox.Size = new System.Drawing.Size(69, 23);
            this.checkerCheckBox.TabIndex = 7;
            this.checkerCheckBox.Text = "Checker";
            this.checkerCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkerCheckBox.UseVisualStyleBackColor = true;
            this.checkerCheckBox.CheckedChanged += new System.EventHandler(this.checkerCheckBox_CheckedChanged);
            // 
            // ViewImageForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(525, 444);
            this.Controls.Add(this.checkerCheckBox);
            this.Controls.Add(this.alphaCheckBox);
            this.Controls.Add(this.piBox);
            this.Controls.Add(this.luminanceCheckButton);
            this.Controls.Add(this.grayCheckButton);
            this.Controls.Add(this.blueCheckBox);
            this.Controls.Add(this.greenCheckBox);
            this.Controls.Add(this.redCheckBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ViewImageForm";
            this.Text = "CIPP - View Image";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ViewImageForm_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.piBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox redCheckBox;
        private System.Windows.Forms.CheckBox greenCheckBox;
        private System.Windows.Forms.CheckBox blueCheckBox;
        private System.Windows.Forms.CheckBox grayCheckButton;
        private System.Windows.Forms.CheckBox luminanceCheckButton;
        private System.Windows.Forms.PictureBox piBox;
        private System.Windows.Forms.CheckBox alphaCheckBox;
        private System.Windows.Forms.CheckBox checkerCheckBox;

    }
}
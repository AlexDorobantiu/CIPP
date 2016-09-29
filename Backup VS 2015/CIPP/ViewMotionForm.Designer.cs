namespace CIPP
{
    partial class ViewMotionForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewMotionForm));
            this.imagesFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // imagesFlowLayoutPanel
            // 
            this.imagesFlowLayoutPanel.AutoScroll = true;
            this.imagesFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imagesFlowLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.imagesFlowLayoutPanel.Name = "imagesFlowLayoutPanel";
            this.imagesFlowLayoutPanel.Size = new System.Drawing.Size(577, 512);
            this.imagesFlowLayoutPanel.TabIndex = 0;
            // 
            // ViewMotionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(577, 512);
            this.Controls.Add(this.imagesFlowLayoutPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ViewMotionForm";
            this.Text = "CIPP - Preview Motion";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel imagesFlowLayoutPanel;
    }
}
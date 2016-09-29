using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

using CIPPProtocols;
using ParametersSDK;
using Plugins.Filters;
using Plugins.Masks;
using Plugins.MotionRecognition;

namespace CIPP
{
    partial class CIPPForm
    {
        private void optionsButton_Click(object sender, EventArgs e)
        {
            List<IParameters> parameterList = (List<IParameters>)((Button)sender).Tag;
            OptionsForm form = new OptionsForm(parameterList);
            form.ShowDialog();
        }

        private void updatePlugins_Click(object sender, EventArgs e)
        {
            List<PluginInfo> currentList;
            FlowLayoutPanel currentFlowLayoutPanel;
            List<CheckBox> currentCheckBoxList;

            this.SuspendLayout();
            try
            {
                switch (processingTab.SelectedIndex)
                {
                    //filter plugins tab
                    case 0:
                        {
                            filterPluginList = PluginHelper.getPluginsList(Path.Combine(Environment.CurrentDirectory, @"plugins\filters"), typeof(IFilter));
                            currentList = filterPluginList;
                            currentFlowLayoutPanel = flowLayoutPanelFilterPlugins;
                            currentCheckBoxList = filterPluginsCheckBoxList;
                        } break;
                    //masking plugins tab
                    case 1:
                        {
                            maskPluginList = PluginHelper.getPluginsList(Path.Combine(Environment.CurrentDirectory, @"plugins\masks"), typeof(IMask));
                            currentList = maskPluginList;
                            currentFlowLayoutPanel = flowLayoutPanelMaskPlugins;
                            currentCheckBoxList = maskPluginsCheckBoxList;
                        } break;
                    //motion recognition plugins tab
                    default:
                        {
                            motionRecognitionPluginList = PluginHelper.getPluginsList(Path.Combine(Environment.CurrentDirectory, @"plugins\motionrecognition"), typeof(IMotionRecognition));
                            currentList = motionRecognitionPluginList;
                            currentFlowLayoutPanel = flowLayoutPanelMotionRecognitionPlugins;
                            currentCheckBoxList = motionRecognitionPluginsCheckBoxList;
                        } break;
                }

                currentFlowLayoutPanel.Controls.Clear();
                currentCheckBoxList.Clear();

                for (int i = 0; i < currentList.Count; i++)
                {
                    CheckBox cb = new CheckBox();
                    cb.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                    cb.AutoSize = true;
                    cb.Text = currentList[i].displayName;
                    cb.Padding = new Padding(5, 4, 0, 0);

                    Button b = new Button();
                    b.FlatStyle = FlatStyle.Flat;
                    if (currentList[i].parameters.Count == 0)
                        b.Enabled = false;
                    b.Tag = currentList[i].parameters;
                    b.Text = "Options";
                    b.Click += new EventHandler(optionsButton_Click);

                    currentFlowLayoutPanel.Controls.Add(cb);
                    currentFlowLayoutPanel.Controls.Add(b);
                    currentFlowLayoutPanel.SetFlowBreak(b, true);

                    currentCheckBoxList.Add(cb);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            this.PerformLayout();
        }
    }
}

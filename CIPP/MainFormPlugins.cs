using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using CIPPProtocols.Plugin;
using ParametersSDK;
using Plugins.Filters;
using Plugins.Masks;
using Plugins.MotionRecognition;

namespace CIPP
{
    partial class CIPPForm
    {
        private const string FILTERS_RELATIVE_PATH = @"plugins\filters";
        private const string MASKS_RELATIVE_PATH = @"plugins\masks";
        private const string MOTION_RECOGNITION_RELATIVE_PATH = @"plugins\motionrecognition";

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

            SuspendLayout();
            try
            {
                switch (processingTab.SelectedIndex)
                {
                    // filter plugins tab
                    case 0:
                        {
                            filterPluginList = PluginHelper.getPluginsList(Path.Combine(Environment.CurrentDirectory, FILTERS_RELATIVE_PATH), typeof(IFilter));
                            currentList = filterPluginList;
                            currentFlowLayoutPanel = flowLayoutPanelFilterPlugins;
                            currentCheckBoxList = filterPluginsCheckBoxList;
                        } break;
                    // masking plugins tab
                    case 1:
                        {
                            maskPluginList = PluginHelper.getPluginsList(Path.Combine(Environment.CurrentDirectory, MASKS_RELATIVE_PATH), typeof(IMask));
                            currentList = maskPluginList;
                            currentFlowLayoutPanel = flowLayoutPanelMaskPlugins;
                            currentCheckBoxList = maskPluginsCheckBoxList;
                        } break;
                    // motion recognition plugins tab
                    case 2:
                        {
                            motionRecognitionPluginList = PluginHelper.getPluginsList(Path.Combine(Environment.CurrentDirectory, MOTION_RECOGNITION_RELATIVE_PATH), typeof(IMotionRecognition));
                            currentList = motionRecognitionPluginList;
                            currentFlowLayoutPanel = flowLayoutPanelMotionRecognitionPlugins;
                            currentCheckBoxList = motionRecognitionPluginsCheckBoxList;
                        } break;
                    default:
                        throw new NotImplementedException();
                }

                currentFlowLayoutPanel.Controls.Clear();
                currentCheckBoxList.Clear();

                for (int i = 0; i < currentList.Count; i++)
                {
                    CheckBox cb = new CheckBox
                    {
                        TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                        AutoSize = true,
                        Text = currentList[i].displayName,
                        Padding = new Padding(5, 4, 0, 0)
                    };

                    Button b = new Button
                    {
                        FlatStyle = FlatStyle.Flat,
                        Tag = currentList[i].parameters,
                        Text = "Options"
                    };
                    b.Click += new EventHandler(optionsButton_Click);
                    if (currentList[i].parameters == null || currentList[i].parameters.Count == 0)
                    {
                        b.Enabled = false;
                    }

                    currentFlowLayoutPanel.Controls.Add(cb);
                    currentFlowLayoutPanel.Controls.Add(b);
                    currentFlowLayoutPanel.SetFlowBreak(b, true);

                    currentCheckBoxList.Add(cb);
                }

                pluginFinder.updatePluginLists(filterPluginList, maskPluginList, motionRecognitionPluginList);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            ResumeLayout(false);
            PerformLayout();
        }

    }
}

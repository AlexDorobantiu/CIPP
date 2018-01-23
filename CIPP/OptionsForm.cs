using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using ParametersSDK;
using Plugins.Filters;
using System.Globalization;

namespace CIPP
{
    public partial class OptionsForm : Form
    {
        List<IParameters> parametersList;

        public OptionsForm(List<IParameters> parametersList)
        {
            InitializeComponent();

            okButton.BringToFront();
            cancelButton.BringToFront();

            this.parametersList = parametersList;
            this.SuspendLayout();
            foreach (IParameters parameter in parametersList)
            {
                Label label = new Label();
                label.AutoSize = true;
                label.Text = parameter.getDisplayName();
                flowLayoutPanel.Controls.Add(label);

                if (parameter.GetType() == typeof(ParametersInt32))
                {
                    ParametersInt32 p = (ParametersInt32)parameter;
                    if (p.getPreferredDisplayType() == ParameterDisplayTypeEnum.textBox)
                    {
                        label.Padding = new Padding(5, 6, 0, 0);

                        TextBox textBox = new TextBox();
                        textBox.Size = new Size(60, 20);
                        List<object> values = p.getValues();
                        if (values.Count == 0)
                        {
                            textBox.Text = "" + p.defaultValue;
                        }
                        else
                        {
                            textBox.Text = "";
                            foreach (object o in values)
                            {
                                textBox.Text += (int)o + " ";
                            }
                        }

                        this.flowLayoutPanel.Controls.Add(textBox);
                        this.flowLayoutPanel.SetFlowBreak(textBox, true);
                    }
                    else
                    {
                        if (p.getPreferredDisplayType() == ParameterDisplayTypeEnum.trackBar)
                        {
                            label.Padding = new Padding(5, 15, 0, 0);

                            TrackBar trackBar = new TrackBar();
                            trackBar.AutoSize = true;
                            trackBar.Minimum = p.minValue;
                            trackBar.Maximum = p.maxValue;
                            trackBar.TickStyle = TickStyle.Both;

                            List<object> values = p.getValues();
                            if (values.Count == 0)
                            {
                                trackBar.Value = p.defaultValue;
                            }
                            else
                            {
                                trackBar.Value = (int)values[0];
                            }
                            this.flowLayoutPanel.Controls.Add(trackBar);
                            this.flowLayoutPanel.SetFlowBreak(trackBar, true);
                        }
                    }
                }
                else
                {
                    if (parameter.GetType() == typeof(ParametersFloat))
                    {
                        ParametersFloat p = (ParametersFloat)parameter;
                        if (p.getPreferredDisplayType() == ParameterDisplayTypeEnum.textBox)
                        {
                            label.Padding = new Padding(5, 6, 0, 0);

                            TextBox textBox = new TextBox();
                            textBox.Size = new Size(60, 20);
                            List<object> values = p.getValues();
                            if (values.Count == 0)
                            {
                                textBox.Text = p.defaultValue.ToString(CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                textBox.Text = "";
                                foreach (object value in values)
                                {
                                    textBox.Text += ((float)value).ToString(CultureInfo.InvariantCulture) + " ";
                                }
                            }

                            this.flowLayoutPanel.Controls.Add(textBox);
                            this.flowLayoutPanel.SetFlowBreak(textBox, true);
                        }
                    }
                    else
                    {
                        if (parameter.GetType() == typeof(ParametersEnum))
                        {
                            ParametersEnum p = (ParametersEnum)parameter;
                            if (p.getPreferredDisplayType() == ParameterDisplayTypeEnum.listBox)
                            {
                                label.Padding = new Padding(5, 6, 0, 0);

                                ListBox listBox = new ListBox();
                                listBox.SelectionMode = SelectionMode.MultiExtended;
                                listBox.Height = listBox.ItemHeight * 4;
                                foreach (string s in p.displayValues)
                                {
                                    listBox.Items.Add(s);
                                }
                                List<object> values = p.getValues();
                                if (values.Count == 0)
                                {
                                    listBox.SetSelected(p.defaultSelected, true);
                                }
                                else
                                {
                                    foreach (Object o in values)
                                    {
                                        listBox.SetSelected((int)o, true);
                                    }
                                }

                                this.flowLayoutPanel.Controls.Add(listBox);
                                this.flowLayoutPanel.SetFlowBreak(listBox, true);
                            }
                            else
                            {
                                if (p.getPreferredDisplayType() == ParameterDisplayTypeEnum.comboBox)
                                {
                                    label.Padding = new Padding(5, 6, 0, 0);

                                    ComboBox comboBox = new ComboBox();
                                    comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                                    comboBox.Size = new Size(60, 20);
                                    foreach (string s in p.displayValues)
                                    {
                                        comboBox.Items.Add(s);
                                    }
                                    List<object> values = p.getValues();
                                    if (values.Count == 0)
                                    {
                                        comboBox.SelectedIndex = p.defaultSelected;
                                    }
                                    else
                                    {
                                        comboBox.SelectedIndex = (int)values[0];
                                    }
                                    this.flowLayoutPanel.Controls.Add(comboBox);
                                    this.flowLayoutPanel.SetFlowBreak(comboBox, true);
                                }
                            }
                        }
                    }
                }
            }
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            int i = 1;
            foreach (IParameters parameter in parametersList)
            {
                if (parameter.getPreferredDisplayType() == ParameterDisplayTypeEnum.textBox)
                {
                    parameter.updateProperty(((TextBox)flowLayoutPanel.Controls[i]).Text);
                }
                else
                {
                    if (parameter.getPreferredDisplayType() == ParameterDisplayTypeEnum.trackBar)
                    {
                        parameter.updateProperty(((TrackBar)flowLayoutPanel.Controls[i]).Value);
                    }
                    else
                    {
                        if (parameter.getPreferredDisplayType() == ParameterDisplayTypeEnum.listBox)
                        {
                            int[] temp = new int[((ListBox)flowLayoutPanel.Controls[i]).SelectedIndices.Count];
                            ((ListBox)flowLayoutPanel.Controls[i]).SelectedIndices.CopyTo(temp, 0);
                            parameter.updateProperty(temp);
                        }
                        else
                        {
                            if (parameter.getPreferredDisplayType() == ParameterDisplayTypeEnum.comboBox)
                            {
                                parameter.updateProperty(((ComboBox)flowLayoutPanel.Controls[i]).SelectedIndex);
                            }
                        }
                    }
                }
                i += 2;
            }
            this.Close();
        }
    }
}
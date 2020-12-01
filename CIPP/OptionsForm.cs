using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using ParametersSDK;
using System.Globalization;

namespace CIPP
{
    partial class OptionsForm : Form
    {
        readonly List<IParameters> parametersList;

        public OptionsForm(List<IParameters> parametersList)
        {
            InitializeComponent();

            okButton.BringToFront();
            cancelButton.BringToFront();

            this.parametersList = parametersList;
            SuspendLayout();
            foreach (IParameters parameter in parametersList)
            {
                Label label = new Label
                {
                    AutoSize = true,
                    Text = parameter.getDisplayName()
                };
                flowLayoutPanel.Controls.Add(label);

                if (parameter.GetType() == typeof(ParametersInt32))
                {
                    ParametersInt32 p = (ParametersInt32)parameter;
                    switch (p.getPreferredDisplayType())
                    {
                        case ParameterDisplayTypeEnum.textBox:
                            {
                                label.Padding = new Padding(5, 6, 0, 0);

                                TextBox textBox = new TextBox
                                {
                                    Size = new Size(60, 20)
                                };
                                List<object> values = p.getValues();
                                if (values.Count == 0)
                                {
                                    textBox.Text = $"{p.defaultValue}";
                                }
                                else
                                {
                                    textBox.Text = "";
                                    foreach (object o in values)
                                    {
                                        textBox.Text += $"{(int)o} ";
                                    }
                                }

                                flowLayoutPanel.Controls.Add(textBox);
                                flowLayoutPanel.SetFlowBreak(textBox, true);
                                break;
                            }

                        case ParameterDisplayTypeEnum.trackBar:
                            {
                                label.Padding = new Padding(5, 15, 0, 0);

                                TrackBar trackBar = new TrackBar
                                {
                                    AutoSize = true,
                                    Minimum = p.minValue,
                                    Maximum = p.maxValue,
                                    TickStyle = TickStyle.Both
                                };

                                List<object> values = p.getValues();
                                if (values.Count == 0)
                                {
                                    trackBar.Value = p.defaultValue;
                                }
                                else
                                {
                                    trackBar.Value = (int)values[0];
                                }
                                flowLayoutPanel.Controls.Add(trackBar);
                                flowLayoutPanel.SetFlowBreak(trackBar, true);
                                break;
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

                            TextBox textBox = new TextBox
                            {
                                Size = new Size(60, 20)
                            };
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
                                    textBox.Text += $"{((float)value).ToString(CultureInfo.InvariantCulture)} ";
                                }
                            }

                            flowLayoutPanel.Controls.Add(textBox);
                            flowLayoutPanel.SetFlowBreak(textBox, true);
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

                                ListBox listBox = new ListBox
                                {
                                    SelectionMode = SelectionMode.MultiExtended
                                };
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
                                    foreach (object o in values)
                                    {
                                        listBox.SetSelected((int)o, true);
                                    }
                                }

                                flowLayoutPanel.Controls.Add(listBox);
                                flowLayoutPanel.SetFlowBreak(listBox, true);
                            }
                            else
                            {
                                if (p.getPreferredDisplayType() == ParameterDisplayTypeEnum.comboBox)
                                {
                                    label.Padding = new Padding(5, 6, 0, 0);

                                    ComboBox comboBox = new ComboBox
                                    {
                                        DropDownStyle = ComboBoxStyle.DropDownList,
                                        Size = new Size(60, 20)
                                    };
                                    comboBox.Items.AddRange(p.displayValues);
                                    List<object> values = p.getValues();
                                    if (values.Count == 0)
                                    {
                                        comboBox.SelectedIndex = p.defaultSelected;
                                    }
                                    else
                                    {
                                        comboBox.SelectedIndex = (int)values[0];
                                    }
                                    flowLayoutPanel.Controls.Add(comboBox);
                                    flowLayoutPanel.SetFlowBreak(comboBox, true);
                                }
                            }
                        }
                    }
                }
            }
            ResumeLayout(false);
            PerformLayout();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            int i = 1;
            foreach (IParameters parameter in parametersList)
            {
                switch (parameter.getPreferredDisplayType())
                {
                    case ParameterDisplayTypeEnum.textBox:
                        parameter.updateProperty(((TextBox)flowLayoutPanel.Controls[i]).Text);
                        break;
                    case ParameterDisplayTypeEnum.trackBar:
                        parameter.updateProperty(((TrackBar)flowLayoutPanel.Controls[i]).Value);
                        break;
                    case ParameterDisplayTypeEnum.listBox:
                        int[] temp = new int[((ListBox)flowLayoutPanel.Controls[i]).SelectedIndices.Count];
                        ((ListBox)flowLayoutPanel.Controls[i]).SelectedIndices.CopyTo(temp, 0);
                        parameter.updateProperty(temp);
                        break;
                    case ParameterDisplayTypeEnum.comboBox:
                        parameter.updateProperty(((ComboBox)flowLayoutPanel.Controls[i]).SelectedIndex);
                        break;
                }
                i += 2;
            }
            Close();
        }
    }
}
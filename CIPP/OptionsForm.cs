using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using ParametersSDK;
using Plugins.Filters;

namespace CIPP
{
    public partial class OptionsForm : Form
    {
        List<IParameters> list;
        public OptionsForm(List<IParameters> list)
        {
            InitializeComponent();

            okButton.BringToFront();
            cancelButton.BringToFront();

            this.list = list;
            this.SuspendLayout();
            foreach (IParameters param in list)
            {
                Label l = new Label();
                l.AutoSize = true;
                l.Text = param.getDisplayName();
                flowLayoutPanel.Controls.Add(l);

                if (param.GetType() == typeof(ParametersInt32))
                {
                    ParametersInt32 p = (ParametersInt32)param;
                    if (p.getPreferredDisplayType() == DisplayType.textBox)
                    {
                        l.Padding = new Padding(5, 6, 0, 0);

                        TextBox tb = new TextBox();
                        tb.Size = new Size(60, 20);
                        List<object> values = p.getValues();
                        if (values.Count == 0) tb.Text = "" + p.defaultValue;
                        else
                        {
                            tb.Text = "";
                            foreach (object o in values)
                            {
                                tb.Text += (int)o + " ";
                            }
                        }

                        this.flowLayoutPanel.Controls.Add(tb);
                        this.flowLayoutPanel.SetFlowBreak(tb, true);
                    }
                    else
                        if (p.getPreferredDisplayType() == DisplayType.trackBar)
                        {
                            l.Padding = new Padding(5, 15, 0, 0);

                            TrackBar tb = new TrackBar();
                            tb.AutoSize = true;
                            tb.Minimum = p.minValue;
                            tb.Maximum = p.maxValue;
                            tb.TickStyle = TickStyle.Both;

                            List<object> values = p.getValues();
                            if (values.Count == 0) tb.Value = p.defaultValue;
                            else tb.Value = (int)values[0];

                            this.flowLayoutPanel.Controls.Add(tb);
                            this.flowLayoutPanel.SetFlowBreak(tb, true);
                        }
                }
                else
                {
                    if (param.GetType() == typeof(ParametersFloat))
                    {
                        ParametersFloat p = (ParametersFloat)param;
                        if (p.getPreferredDisplayType() == DisplayType.textBox)
                        {
                            l.Padding = new Padding(5, 6, 0, 0);

                            TextBox tb = new TextBox();
                            tb.Size = new Size(60, 20);
                            List<object> values = p.getValues();
                            if (values.Count == 0) tb.Text = "" + p.defaultValue;
                            else
                            {
                                tb.Text = "";
                                foreach (object o in values)
                                {
                                    tb.Text += (float)o + " ";
                                }
                            }

                            this.flowLayoutPanel.Controls.Add(tb);
                            this.flowLayoutPanel.SetFlowBreak(tb, true);
                        }
                    }
                    else
                    {
                        if (param.GetType() == typeof(ParametersEnum))
                        {
                            ParametersEnum p = (ParametersEnum)param;
                            if (p.getPreferredDisplayType() == DisplayType.listBox)
                            {
                                l.Padding = new Padding(5, 6, 0, 0);

                                ListBox lb = new ListBox();
                                lb.SelectionMode = SelectionMode.MultiExtended;
                                lb.Height = lb.ItemHeight * 4;
                                foreach (string s in p.displayValues) lb.Items.Add(s);
                                List<object> values = p.getValues();
                                if (values.Count == 0) lb.SetSelected(p.defaultSelected, true);
                                else
                                {
                                    foreach (Object o in values)
                                        lb.SetSelected((int)o, true);
                                }

                                this.flowLayoutPanel.Controls.Add(lb);
                                this.flowLayoutPanel.SetFlowBreak(lb, true);
                            }
                            else
                            {
                                if (p.getPreferredDisplayType() == DisplayType.comboBox)
                                {
                                    l.Padding = new Padding(5, 6, 0, 0);

                                    ComboBox cb = new ComboBox();
                                    cb.DropDownStyle = ComboBoxStyle.DropDownList;
                                    cb.Size = new Size(60, 20);
                                    foreach (string s in p.displayValues) cb.Items.Add(s);
                                    List<object> values = p.getValues();
                                    if (values.Count == 0) cb.SelectedIndex = p.defaultSelected;
                                    else cb.SelectedIndex = (int)values[0];

                                    this.flowLayoutPanel.Controls.Add(cb);
                                    this.flowLayoutPanel.SetFlowBreak(cb, true);
                                }
                            }
                        }
                    }
                }
            }
            this.PerformLayout();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            int i = 1;
            foreach (IParameters param in list)
            {
                if (param.getPreferredDisplayType() == DisplayType.textBox)
                    param.updateProperty(((TextBox)flowLayoutPanel.Controls[i]).Text);
                else
                    if (param.getPreferredDisplayType() == DisplayType.trackBar)
                        param.updateProperty(((TrackBar)flowLayoutPanel.Controls[i]).Value);
                    else
                        if (param.getPreferredDisplayType() == DisplayType.listBox)
                        {
                            int[] temp = new int[((ListBox)flowLayoutPanel.Controls[i]).SelectedIndices.Count];
                            ((ListBox)flowLayoutPanel.Controls[i]).SelectedIndices.CopyTo(temp, 0);
                            param.updateProperty(temp);
                        }
                        else
                            if (param.getPreferredDisplayType() == DisplayType.comboBox)
                                param.updateProperty(((ComboBox)flowLayoutPanel.Controls[i]).SelectedIndex);
                i += 2;
            }
            this.Close();
        }
    }
}
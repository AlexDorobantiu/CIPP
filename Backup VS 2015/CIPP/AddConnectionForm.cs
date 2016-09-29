using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CIPP
{
    public partial class AddConnectionForm : Form
    {
        public string ip;
        public int port;

        public AddConnectionForm()
        {
            InitializeComponent();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            ip = ipTextBox.Text;
            try
            {
                port = int.Parse(portTextBox.Text);
            }
            catch
            {
                port = 6050;
            }
        }
    }
}
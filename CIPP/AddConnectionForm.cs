using System;
using System.Windows.Forms;

namespace CIPP
{
    partial class AddConnectionForm : Form
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
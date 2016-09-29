using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace CIPP
{
    public partial class CIPPForm
    {
        private void addTCPConnectionButton_Click(object sender, EventArgs e)
        {
            AddConnectionForm f = new AddConnectionForm();
            if (f.ShowDialog() == DialogResult.OK)
            {
                TCPProxy newproxy = new TCPProxy(f.ip, f.port);
                newproxy.MessagePosted += new EventHandler<StringEventArgs>(messagePosted);
                newproxy.WorkerPosted += new EventHandler<WorkerEventArgs>(workerPosted);
                newproxy.TaskRequestReceived += new EventHandler(proxyRequestReceived);
                newproxy.ResultsReceived += new ResultReceivedEventHandler(proxyResultReceived);
                newproxy.connectionLost += new EventHandler(connectionLost);
                TCPConnections.Add(newproxy);
                TCPConnectionsListBox.Items.Add(newproxy.getNameAndStatus());
            }
        }

        private void removeTCPConnectionButton_Click(object sender, EventArgs e)
        {
            while (TCPConnectionsListBox.SelectedItems.Count > 0)
            {
                int index = TCPConnectionsListBox.SelectedIndices[0];
                TCPConnections[index].Disconnect();
                TCPConnections.RemoveAt(index);
                TCPConnectionsListBox.Items.RemoveAt(index);
            }
        }

        private void connectTCPConnectionButton_Click(object sender, EventArgs e)
        {
            foreach (int index in TCPConnectionsListBox.SelectedIndices)
            {
                TCPConnections[index].TryConnect();
                TCPConnectionsListBox.Items[index] = TCPConnections[index].getNameAndStatus();
            }
        }

        private void disconnectTCPConnectionButton_Click(object sender, EventArgs e)
        {
            foreach (int index in TCPConnectionsListBox.SelectedIndices)
            {
                TCPConnections[index].Disconnect();
                TCPConnectionsListBox.Items[index] = TCPConnections[index].getNameAndStatus();
            }
        }

        public void loadConnectionsFromDisk()
        {
            try
            {
                StreamReader sr = new StreamReader("connections.txt");
                while (!sr.EndOfStream)
                {
                    string[] vals = sr.ReadLine().Split(',');
                    TCPProxy newproxy = new TCPProxy(vals[0], int.Parse(vals[1]));
                    newproxy.MessagePosted += new EventHandler<StringEventArgs>(messagePosted);
                    newproxy.WorkerPosted += new EventHandler<WorkerEventArgs>(workerPosted);
                    newproxy.TaskRequestReceived += new EventHandler(proxyRequestReceived);
                    newproxy.ResultsReceived += new ResultReceivedEventHandler(proxyResultReceived);
                    newproxy.connectionLost += new EventHandler(connectionLost);
                    TCPConnections.Add(newproxy);
                    TCPConnectionsListBox.Items.Add(newproxy.getNameAndStatus());
                }
                sr.Close();
            }
            catch { }
        }

        public void saveConnectionsToDisk()
        {
            try
            {
                StreamWriter sw = new StreamWriter("connections.txt", false);
                foreach (TCPProxy item in TCPConnections)
                    sw.WriteLine(item.hostname + ", " + item.port);
                sw.Close();
            }
            catch { }
        }

        public void messagePosted(object sender, StringEventArgs e)
        {
            addMessage(e.message);         
        }

        public void workerPosted(object sender, WorkerEventArgs e)
        {
            displayWorker(e.name, !e.left);
        }

        public void connectionLost(object sender, EventArgs e)
        {
            updateTCPList((TCPProxy)sender);
        }
    }
}

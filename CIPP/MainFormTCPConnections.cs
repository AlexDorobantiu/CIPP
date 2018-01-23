using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace CIPP
{
    public partial class CIPPForm
    {
        private const string connectionsFilename = "connections.txt";

        private void addTCPConnectionButton_Click(object sender, EventArgs e)
        {
            AddConnectionForm f = new AddConnectionForm();
            if (f.ShowDialog() == DialogResult.OK)
            {
                TCPProxy newproxy = new TCPProxy(f.ip, f.port);
                newproxy.messagePosted += new EventHandler<StringEventArgs>(messagePosted);
                newproxy.workerPosted += new EventHandler<WorkerEventArgs>(workerPosted);
                newproxy.taskRequestReceivedEventHandler += new EventHandler(proxyRequestReceived);
                newproxy.resultsReceivedEventHandler += new ResultReceivedEventHandler(proxyResultReceived);
                newproxy.connectionLostEventHandler += new EventHandler(connectionLost);
                TCPConnections.Add(newproxy);
                TCPConnectionsListBox.Items.Add(newproxy.getNameAndStatus());
            }
        }

        private void removeTCPConnectionButton_Click(object sender, EventArgs e)
        {
            while (TCPConnectionsListBox.SelectedItems.Count > 0)
            {
                int index = TCPConnectionsListBox.SelectedIndices[0];
                TCPConnections[index].disconnect();
                TCPConnections.RemoveAt(index);
                TCPConnectionsListBox.Items.RemoveAt(index);
            }
        }

        private void connectTCPConnectionButton_Click(object sender, EventArgs e)
        {
            foreach (int index in TCPConnectionsListBox.SelectedIndices)
            {
                TCPConnections[index].tryToConnect();
                TCPConnectionsListBox.Items[index] = TCPConnections[index].getNameAndStatus();
            }
        }

        private void disconnectTCPConnectionButton_Click(object sender, EventArgs e)
        {
            foreach (int index in TCPConnectionsListBox.SelectedIndices)
            {
                TCPConnections[index].disconnect();
                TCPConnectionsListBox.Items[index] = TCPConnections[index].getNameAndStatus();
            }
        }

        private void loadConnectionsFromDisk()
        {
            try
            {
                FileInfo fileInfo = new FileInfo(connectionsFilename);
                if (fileInfo.Exists)
                {
                    StreamReader sr = new StreamReader(connectionsFilename);
                    while (!sr.EndOfStream)
                    {
                        string[] vals = sr.ReadLine().Split(',');
                        TCPProxy newproxy = new TCPProxy(vals[0], int.Parse(vals[1]));
                        newproxy.messagePosted += new EventHandler<StringEventArgs>(messagePosted);
                        newproxy.workerPosted += new EventHandler<WorkerEventArgs>(workerPosted);
                        newproxy.taskRequestReceivedEventHandler += new EventHandler(proxyRequestReceived);
                        newproxy.resultsReceivedEventHandler += new ResultReceivedEventHandler(proxyResultReceived);
                        newproxy.connectionLostEventHandler += new EventHandler(connectionLost);
                        TCPConnections.Add(newproxy);
                        TCPConnectionsListBox.Items.Add(newproxy.getNameAndStatus());
                    }
                    sr.Close();
                }
            }
            catch { }
        }

        private void saveConnectionsToDisk()
        {
            try
            {
                StreamWriter sw = new StreamWriter(connectionsFilename, false);
                foreach (TCPProxy item in TCPConnections)
                {
                    sw.WriteLine(item.hostname + ", " + item.port);
                }
                sw.Close();
            }
            catch { }
        }

        private void messagePosted(object sender, StringEventArgs e)
        {
            addMessage(e.message);         
        }

        private void workerPosted(object sender, WorkerEventArgs e)
        {
            displayWorker(e.name, !e.left);
        }

        private void connectionLost(object sender, EventArgs e)
        {
            updateTCPList((TCPProxy)sender);
        }
    }
}

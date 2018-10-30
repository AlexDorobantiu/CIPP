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
            AddConnectionForm addConnectionForm = new AddConnectionForm();
            if (addConnectionForm.ShowDialog() == DialogResult.OK)
            {
                TcpProxy newproxy = new TcpProxy(addConnectionForm.ip, addConnectionForm.port);
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
                        TcpProxy newproxy = new TcpProxy(vals[0], int.Parse(vals[1]));
                        TCPConnections.Add(newproxy);
                        TCPConnectionsListBox.Items.Add(newproxy.getNameAndStatus());
                    }
                    sr.Close();
                }
            }
            catch
            {
            }
        }

        private void saveConnectionsToDisk()
        {
            try
            {
                StreamWriter streamWriter = new StreamWriter(connectionsFilename, false);
                foreach (TcpProxy tcpProxy in TCPConnections)
                {
                    streamWriter.WriteLine(tcpProxy.hostname + ", " + tcpProxy.port);
                }
                streamWriter.Close();
            }
            catch
            {
            }
        }

    }
}

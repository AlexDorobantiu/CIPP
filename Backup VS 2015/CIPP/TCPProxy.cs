using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Threading;

using CIPPProtocols;
using ProcessingImageSDK;

namespace CIPP
{
    public class TCPProxy
    {
        public event ResultReceivedEventHandler ResultsReceived;
        public event EventHandler TaskRequestReceived;
        public event EventHandler<StringEventArgs> MessagePosted;
        public event EventHandler<WorkerEventArgs> WorkerPosted;

        public event EventHandler connectionLost;

        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private readonly IFormatter formatter;
        List<Task> sentSimulations;

        public string hostname;
        public int port;
        public bool isConnected = false;

        private bool listening = false;
        private Thread listeningThread = null;
        private bool isConnectionThreadRunning = false;

        private int taskRequests = 0;

        public TCPProxy(string hostname, int port)
        {
            formatter = new BinaryFormatter();
            this.hostname = hostname;
            this.port = port;

            sentSimulations = new List<Task>();
        }

        public void TryConnect()
        {
            try
            {
                tcpClient = new TcpClient(hostname, port);
                networkStream = tcpClient.GetStream();
                isConnected = true;
                networkStream.WriteByte((byte)TrasmissionFlags.ClientName);
                formatter.Serialize(networkStream, Environment.MachineName);
                networkStream.Flush();

                postMessage("Connected to " + hostname + " on port " + port);
                isConnectionThreadRunning = true;
                listeningThread = new Thread(HandleConnection);
                listeningThread.Start();
            }
            catch (Exception e)
            {
                isConnected = false;
                postMessage(e.Message);               
            }
        }

        public void Disconnect()
        {
            if (isConnected)
            {
                try
                {
                    networkStream.Close();
                    tcpClient.Close();
                }
                catch (Exception e)
                {
                    postMessage(getNameAndStatus() + " error: " + e.Message);
                }
                isConnected = false;
                listening = false;
                if (listeningThread != null)
                    listeningThread.Abort();
            }
        }

        public void StartListening()
        {
            if (!listening)
            {
                listening = true;
                sentSimulations.Clear();
                taskRequests = 0;
                try
                {
                    networkStream.WriteByte((byte)TrasmissionFlags.Listening);
                    postMessage("Listening to " + hostname + ", " + port);
                }
                catch (Exception e)
                {
                    postMessage(e.Message);
                }
            }
            else
            {
                for (int i = 0; i < taskRequests; i++)
                    TaskRequestReceived(this, EventArgs.Empty);
            }
        }

        public void SendSimulationTask(Task task)
        {
            taskRequests--;
            try
            {
                networkStream.WriteByte((byte)TrasmissionFlags.Task);
                formatter.Serialize(networkStream, task);
                postMessage("Task sent to " + hostname + " on port " + port);
                sentSimulations.Add(task);
            }
            catch (Exception e)
            {
                postMessage(e.Message);
            }

        }

        public void SendAbortRequest()
        {
            try
            {
                networkStream.WriteByte((byte)TrasmissionFlags.AbortWork);
                postMessage("Abort request sent to: " + hostname + " on port: " + port);
                taskRequests = 0;
                listening = false;
            }
            catch (Exception e)
            {
                postMessage(e.Message);
            }
        }

        private void HandleConnection()
        {
            try
            {
                while (isConnectionThreadRunning)
                {
                    int header = networkStream.ReadByte();
                    if (header == -1) break;
                    switch (header)
                    {
                        case (byte)TrasmissionFlags.TaskRequest:
                            {
                                postWorker("Worker @: " + hostname, false);
                                TaskRequestReceived(this, EventArgs.Empty);
                                taskRequests++;
                                postMessage("Received a task request from " + hostname + " on port " + port);
                            } break;
                        case (byte)TrasmissionFlags.Result:
                            {
                                ResultPackage resultPackage = (ResultPackage)formatter.Deserialize(networkStream);
                                if (resultPackage != null)
                                {
                                    Task tempTask = null;
                                    lock (sentSimulations)
                                    {
                                        foreach (Task task in sentSimulations)
                                            if (task.id == resultPackage.taskId)
                                            {
                                                tempTask = task;
                                                break;
                                            }

                                        if (tempTask != null)
                                        {
                                            if (resultPackage.result != null)
                                            {
                                                tempTask.state = true;
                                                if (tempTask.taskType == TaskType.filter)
                                                    ((FilterTask)tempTask).result = (ProcessingImage)resultPackage.result;
                                                else
                                                    if (tempTask.taskType == TaskType.mask)
                                                        ((MaskTask)tempTask).result = (byte[,])resultPackage.result;
                                                    else
                                                        if (tempTask.taskType == TaskType.motionRecognition)
                                                            ((MotionRecognitionTask)tempTask).result = (MotionVectorBase[,])resultPackage.result;

                                                sentSimulations.Remove(tempTask);
                                                ResultsReceived(this, new ResultReceivedEventArgs(tempTask));
                                                postMessage("Received a result from " + hostname + " on port " + port + " ");
                                            }
                                            else
                                            {
                                                tempTask.state = false;
                                                sentSimulations.Remove(tempTask);
                                                ResultsReceived(this, new ResultReceivedEventArgs(tempTask));
                                                postMessage("Task "+ tempTask.id +" not completed succesfuly by " + hostname + " on port " + port + " ");
                                            }
                                        }
                                        else
                                        {
                                            postMessage("Received a false result from " + hostname + " on port " + port + " ");
                                        }
                                    }
                                }
                                else
                                {
                                    postMessage("Received an empty result package from " + hostname + " on port " + port + " ");
                                }
                                postWorker("Worker @: " + hostname, true);
                            }
                            break;
                        default:
                            {
                                connectionLost(this, EventArgs.Empty);
                                postMessage("Invalid message header received: " + header);
                                isConnectionThreadRunning = false;
                                listening = false;
                            } break;
                    }
                }
            }
            catch (Exception e)
            {
                postMessage(e.Message);
            }
            finally
            {
                postMessage("Connection to " + hostname + " on port " + port + " terminated");
                isConnected = false;
                listening = false;
                connectionLost(this, EventArgs.Empty);
                for (int i = 0; i < taskRequests; i++)
                    postWorker("Worker @: " + hostname, true);
            }
        }

        private void postMessage(string message)
        {
            if (MessagePosted != null) MessagePosted(this, new StringEventArgs(message));
        }

        private void postWorker(string name, bool left)
        {
            if (WorkerPosted != null) WorkerPosted(this, new WorkerEventArgs(name, left));
        }

        public string getNameAndStatus()
        {
            if (isConnected)
                return hostname + ", " + port + ", connected";
            return hostname + ", " + port + ", not connected";
        }
    }

    public delegate void ResultReceivedEventHandler(object sender, ResultReceivedEventArgs e);

    public class ResultReceivedEventArgs : EventArgs
    {
        public readonly Task task;
        public ResultReceivedEventArgs(Task task)
        {
            this.task = task;
        }
    }

    public class StringEventArgs : EventArgs
    {
        public readonly string message;
        public StringEventArgs(string message)
        {
            this.message = message;
        }
    }

    public class WorkerEventArgs : EventArgs
    {
        public readonly string name;
        public readonly bool left;
        public WorkerEventArgs(string name, bool left)
        {
            this.name = name;
            this.left = left;
        }
    }
}

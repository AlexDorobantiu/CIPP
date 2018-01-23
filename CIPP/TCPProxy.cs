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
using CIPPProtocols.Tasks;
using ProcessingImageSDK.MotionVectors;

namespace CIPP
{
    public class TCPProxy
    {
        public event ResultReceivedEventHandler resultsReceivedEventHandler;
        public event EventHandler taskRequestReceivedEventHandler;
        public event EventHandler<StringEventArgs> messagePosted;
        public event EventHandler<WorkerEventArgs> workerPosted;

        public event EventHandler connectionLostEventHandler;

        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private readonly IFormatter formatter = new BinaryFormatter();
        List<Task> sentSimulations = new List<Task>();

        public string hostname;
        public int port;
        public bool connected = false;

        private bool listening = false;
        private Thread listeningThread = null;
        private bool isConnectionThreadRunning = false;

        private int taskRequests = 0;

        public TCPProxy(string hostname, int port)
        {
            this.hostname = hostname;
            this.port = port;
        }

        public void tryToConnect()
        {
            try
            {
                tcpClient = new TcpClient(hostname, port);
                networkStream = tcpClient.GetStream();
                networkStream.WriteByte((byte)TrasmissionFlagsEnum.ClientName);
                formatter.Serialize(networkStream, Environment.MachineName);
                networkStream.Flush();
                connected = true;
                
                postMessage("Connected to " + hostname + " on port " + port);
                isConnectionThreadRunning = true;
                listeningThread = new Thread(handleConnection);
                listeningThread.Start();
            }
            catch (Exception e)
            {
                connected = false;
                postMessage(e.Message);
            }
        }

        public void disconnect()
        {
            if (connected)
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
                connected = false;
                listening = false;
                if (listeningThread != null)
                {
                    listeningThread.Abort();
                }
            }
        }

        public void startListening()
        {
            if (!listening)
            {
                listening = true;
                sentSimulations.Clear();
                taskRequests = 0;
                try
                {
                    networkStream.WriteByte((byte)TrasmissionFlagsEnum.Listening);
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
                {
                    taskRequestReceivedEventHandler(this, EventArgs.Empty);
                }
            }
        }

        public void sendSimulationTask(Task task)
        {
            taskRequests--;
            try
            {
                networkStream.WriteByte((byte)TrasmissionFlagsEnum.Task);
                formatter.Serialize(networkStream, task);
                postMessage("Task sent to " + hostname + " on port " + port);
                sentSimulations.Add(task);
            }
            catch (Exception e)
            {
                postMessage(e.Message);
            }

        }

        public void sendAbortRequest()
        {
            try
            {
                networkStream.WriteByte((byte)TrasmissionFlagsEnum.AbortWork);
                postMessage("Abort request sent to: " + hostname + " on port: " + port);
                taskRequests = 0;
                listening = false;
            }
            catch (Exception e)
            {
                postMessage(e.Message);
            }
        }

        private void handleConnection()
        {
            try
            {
                while (isConnectionThreadRunning)
                {
                    int header = networkStream.ReadByte();
                    if (header == -1)
                    {
                        break;
                    }
                    switch (header)
                    {
                        case (byte)TrasmissionFlagsEnum.TaskRequest:
                            {
                                postWorker("Worker @: " + hostname, false);
                                taskRequestReceivedEventHandler(this, EventArgs.Empty);
                                taskRequests++;
                                postMessage("Received a task request from " + hostname + " on port " + port);
                            } break;
                        case (byte)TrasmissionFlagsEnum.Result:
                            {
                                ResultPackage resultPackage = (ResultPackage)formatter.Deserialize(networkStream);
                                if (resultPackage != null)
                                {
                                    Task tempTask = null;
                                    lock (sentSimulations)
                                    {
                                        foreach (Task task in sentSimulations)
                                        {
                                            if (task.id == resultPackage.taskId)
                                            {
                                                tempTask = task;
                                                break;
                                            }
                                        }

                                        if (tempTask != null)
                                        {
                                            if (resultPackage.result != null)
                                            {                                                
                                                switch (tempTask.taskType)
                                                {
                                                    case TaskTypeEnum.filter:
                                                        ((FilterTask)tempTask).result = (ProcessingImage)resultPackage.result; 
                                                        break;
                                                    case TaskTypeEnum.mask:
                                                        ((MaskTask)tempTask).result = (byte[,])resultPackage.result;
                                                        break;
                                                    case TaskTypeEnum.motionRecognition:
                                                        ((MotionRecognitionTask)tempTask).result = (MotionVectorBase[,])resultPackage.result;
                                                        break;
                                                }
                                                tempTask.finishedSuccessfully = true;
                                                sentSimulations.Remove(tempTask);
                                                resultsReceivedEventHandler(this, new ResultReceivedEventArgs(tempTask));
                                                postMessage("Received a result from " + hostname + " on port " + port + " ");
                                            }
                                            else
                                            {
                                                tempTask.finishedSuccessfully = false;
                                                sentSimulations.Remove(tempTask);
                                                resultsReceivedEventHandler(this, new ResultReceivedEventArgs(tempTask));
                                                postMessage("Task " + tempTask.id + " not completed succesfuly by " + hostname + " on port " + port + " ");
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
                                connectionLostEventHandler(this, EventArgs.Empty);
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
                connected = false;
                listening = false;
                connectionLostEventHandler(this, EventArgs.Empty);
                for (int i = 0; i < taskRequests; i++)
                {
                    postWorker("Worker @: " + hostname, true);
                }
            }
        }

        private void postMessage(string message)
        {
            if (messagePosted != null)
            {
                messagePosted(this, new StringEventArgs(message));
            }
        }

        private void postWorker(string name, bool left)
        {
            if (workerPosted != null)
            {
                workerPosted(this, new WorkerEventArgs(name, left));
            }
        }

        public string getNameAndStatus()
        {
            if (connected)
            {
                return hostname + ", " + port + ", connected";
            }
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

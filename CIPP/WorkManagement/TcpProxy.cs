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

namespace CIPP.WorkManagement
{
    class TcpProxy : IDisposable
    {
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

        public event ResultReceivedEventHandler resultsReceivedEventHandler;
        public event EventHandler taskRequestReceivedEventHandler;
        public event EventHandler<StringEventArgs> messagePosted;
        public event EventHandler<WorkerEventArgs> workerPosted;
        public event EventHandler connectionLostEventHandler;

        private static readonly IFormatter formatter = new BinaryFormatter();
        private TcpClient tcpClient;
        private NetworkStream networkStream;

        private int taskRequests = 0;
        private readonly Dictionary<int, Task> sentTasks = new Dictionary<int, Task>();

        public readonly string hostname;
        public readonly int port;
        public bool connected = false;

        private bool listening = false;
        private Thread listeningThread = null;
        private bool isConnectionThreadRunning = false;       

        public TcpProxy(string hostname, int port)
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
                lock (networkStream)
                {
                    networkStream.WriteByte((byte)TrasmissionFlagsEnum.ClientName);
                    formatter.Serialize(networkStream, Environment.MachineName);
                    networkStream.Flush();
                }
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
                sentTasks.Clear();
                taskRequests = 0;
                try
                {
                    lock (networkStream)
                    {
                        networkStream.WriteByte((byte)TrasmissionFlagsEnum.Listening);
                        networkStream.Flush();
                    }
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

        public void sendTask(Task task)
        {
            try
            {
                lock (networkStream)
                {
                    networkStream.WriteByte((byte)TrasmissionFlagsEnum.Task);
                    formatter.Serialize(networkStream, task);
                    networkStream.Flush();
                }
                postMessage("Task sent to " + hostname + " on port " + port);
                lock (sentTasks)
                {
                    sentTasks.Add(task.id, task);
                    taskRequests--;
                }
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
                lock (networkStream)
                {
                    networkStream.WriteByte((byte)TrasmissionFlagsEnum.AbortWork);
                    networkStream.Flush();
                }
                postMessage("Abort request sent to: " + hostname + " on port: " + port);
                lock (sentTasks)
                {
                    sentTasks.Clear();
                    taskRequests = 0;
                }
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
                            postWorker("Worker @: " + hostname, false);
                            if (taskRequestReceivedEventHandler != null)
                            {
                                taskRequestReceivedEventHandler(this, EventArgs.Empty);
                            }
                            taskRequests++;
                            postMessage("Received a task request from " + hostname + " on port " + port);
                            break;
                        case (byte)TrasmissionFlagsEnum.Result:
                            ResultPackage resultPackage = (ResultPackage)formatter.Deserialize(networkStream);
                            if (resultPackage != null)
                            {
                                Task tempTask = null;
                                lock (sentTasks)
                                {
                                    if (sentTasks.ContainsKey(resultPackage.taskId))
                                    {
                                        tempTask = sentTasks[resultPackage.taskId];
                                        sentTasks.Remove(resultPackage.taskId);
                                    }
                                    else
                                    {
                                        postMessage("Received a false result from " + hostname + " on port " + port + " ");
                                        throw new Exception("Invalid result package received");
                                    }
                                }
                                if (resultPackage.result != null)
                                {
                                    switch (tempTask.type)
                                    {
                                        case Task.Type.FILTER:
                                            ((FilterTask)tempTask).result = (ProcessingImage)resultPackage.result;
                                            break;
                                        case Task.Type.MASK:
                                            ((MaskTask)tempTask).result = (byte[,])resultPackage.result;
                                            break;
                                        case Task.Type.MOTION_RECOGNITION:
                                            ((MotionRecognitionTask)tempTask).result = (MotionVectorBase[,])resultPackage.result;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                    tempTask.status = Task.Status.SUCCESSFUL;
                                    postMessage("Received a result from " + hostname + " on port " + port + " ");
                                }
                                else
                                {
                                    tempTask.status = Task.Status.FAILED;
                                    postMessage("Task " + tempTask.id + " not completed succesfuly by " + hostname + " on port " + port + " ");
                                }
                                if (resultsReceivedEventHandler != null)
                                {
                                    resultsReceivedEventHandler(this, new ResultReceivedEventArgs(tempTask));
                                }
                            }
                            else
                            {
                                postMessage("Received an empty result package from " + hostname + " on port " + port + " ");
                                throw new Exception("Invalid result package received");
                            }
                            postWorker("Worker @: " + hostname, true);
                            break;
                        default:
                            if (connectionLostEventHandler != null)
                            {
                                connectionLostEventHandler(this, EventArgs.Empty);
                            }
                            postMessage("Invalid message header received: " + header);
                            isConnectionThreadRunning = false;
                            listening = false;
                            break;
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
                isConnectionThreadRunning = false;
                connected = false;
                listening = false;
                for (int i = 0; i < taskRequests; i++)
                {
                    postWorker("Worker @: " + hostname, true);
                }
                lock (sentTasks)
                {
                    sentTasks.Clear();
                    taskRequests = 0;
                }
                if (connectionLostEventHandler != null)
                {
                    connectionLostEventHandler(this, EventArgs.Empty);
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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    disconnect();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}

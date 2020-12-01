using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;

using CIPPProtocols;

namespace CIPPServer
{
    public class ConnectionThread
    {
        private static readonly BinaryFormatter formatter = new BinaryFormatter();

        private readonly TcpListener tcpListener;
        private readonly int port;

        private TcpClient tcpClient;
        private NetworkStream networkStream;

        private string clientName;

        private readonly Thread connectionThread;

        private readonly int numberOfWorkerThreads;
        private readonly TaskWorkerThread[] workerThreads;

        public readonly Queue<Task> taskBuffer = new Queue<Task>();

        public ConnectionThread(TcpListener tcpListener, int port)
        {
            this.tcpListener = tcpListener;
            this.port = port;

            numberOfWorkerThreads = Environment.ProcessorCount;
            workerThreads = new TaskWorkerThread[numberOfWorkerThreads];
            for (int i = 0; i < numberOfWorkerThreads; i++)
            {
                workerThreads[i] = new TaskWorkerThread(this, "worker thread # " + i);
            }

            connectionThread = new Thread(handleConnection)
            {
                Name = "Connection thread"
            };
            connectionThread.Start();
        }

        public void handleConnection()
        {
            try
            {
                tcpClient = tcpListener.AcceptTcpClient();
                networkStream = tcpClient.GetStream();
                lock (networkStream)
                {
                    int header = networkStream.ReadByte(); // Client Name Byte
                    if (header != (byte)TrasmissionFlagsEnum.ClientName)
                    {
                        throw new Exception("Invalid Client Package");
                    }

                    clientName = (string)formatter.Deserialize(networkStream);
                }
                Console.WriteLine("Connected to " + clientName + " on port " + port);
            }
            catch (Exception e)
            {
                Console.WriteLine("Invalid Client");
                Console.WriteLine(e.StackTrace);
                if (networkStream != null)
                {
                    networkStream.Close();
                }
                tcpClient.Close();
                return;
            }

            try
            {
                while (true)
                {
                    int header = networkStream.ReadByte();
                    if (header == -1)
                    {
                        break;
                    }
                    switch (header)
                    {
                        // Start sending requests
                        case (byte)TrasmissionFlagsEnum.Listening:
                            {
                                Console.WriteLine("Hired by  " + clientName);
                                for (int i = 0; i < numberOfWorkerThreads; i++)
                                {
                                    workerThreads[i].AbortCurrentTask();
                                    sendTaskRequest();
                                }
                            } break;
                        // Receive task
                        case (byte)TrasmissionFlagsEnum.Task:
                            {
                                Task task = (Task)formatter.Deserialize(networkStream);
                                lock (taskBuffer)
                                {
                                    taskBuffer.Enqueue(task);
                                }
                                for (int i = 0; i < numberOfWorkerThreads; i++)
                                {
                                    workerThreads[i].Awake();
                                }
                                Console.WriteLine("TaskPackage received from " + clientName);
                            } break;
                        // Stop Working (drop tasks)
                        case (byte)TrasmissionFlagsEnum.AbortWork:
                            {
                                for (int i = 0; i < numberOfWorkerThreads; i++)
                                {
                                    if (workerThreads[i] != null)
                                    {
                                        workerThreads[i].AbortCurrentTask();
                                    }
                                }
                                lock (taskBuffer)
                                {
                                    taskBuffer.Clear();
                                }
                                Console.WriteLine("Work aborted.");
                            } break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("Connection to " + clientName + " terminated");

            networkStream.Close();
            tcpClient.Close();
        }

        public void sendResult(int taskId, object result)
        {
            ResultPackage resultPackage = new ResultPackage(taskId, result);
            try
            {
                lock (networkStream)
                {
                    networkStream.WriteByte((byte)TrasmissionFlagsEnum.Result);
                    formatter.Serialize(networkStream, resultPackage);
                    networkStream.Flush();
                }
                Console.WriteLine("Result sent back to " + clientName);
            }
            catch (Exception e)
            {
                Console.WriteLine("Attempt to send result back to " + clientName + " failed: " + e.Message);
            }
        }

        public void sendTaskRequest()
        {
            try
            {
                lock (networkStream)
                {
                    networkStream.WriteByte((byte)TrasmissionFlagsEnum.TaskRequest);
                    networkStream.Flush();
                }
                Console.WriteLine("Task request sent to " + clientName + ".");
            }
            catch (Exception e)
            {
                Console.WriteLine("Attempt to send a task request to " + clientName + " failed: " + e.Message);
            }
        }
    }
}

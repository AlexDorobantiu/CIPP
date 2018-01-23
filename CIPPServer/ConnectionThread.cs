using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

using CIPPProtocols;

namespace CIPPServer
{
    public class ConnectionThread
    {
        private TcpListener tcpListener;
        private int port;
        private BinaryFormatter formatter;

        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private object networkStreamLocker = new object();

        private int header;
        private string clientName;

        private Thread connectionThread;

        private int numberOfWorkerThreads;
        private SimulationWorkerThread[] workerThreads;

        public Queue<Task> taskBuffer;

        public ConnectionThread(TcpListener tcpListener, int port)
        {
            formatter = new BinaryFormatter();

            this.tcpListener = tcpListener;
            this.port = port;

            taskBuffer = new Queue<Task>();

            numberOfWorkerThreads = Environment.ProcessorCount;
            workerThreads = new SimulationWorkerThread[numberOfWorkerThreads];
            for (int i = 0; i < numberOfWorkerThreads; i++)
            {
                workerThreads[i] = new SimulationWorkerThread(this, "worker thread # " + i);
            }

            connectionThread = new Thread(HandleConnection);
            connectionThread.Name = "Connection thread";
            connectionThread.Start();
        }

        public void HandleConnection()
        {
            try
            {
                tcpClient = tcpListener.AcceptTcpClient();
                networkStream = tcpClient.GetStream();
                header = networkStream.ReadByte(); // Client Name Byte
                if (header != (byte)TrasmissionFlagsEnum.ClientName)
                {
                    throw new Exception();
                }

                clientName = (string)formatter.Deserialize(networkStream);
                Console.WriteLine("Connected to " + clientName + " on port " + port);
            }
            catch
            {
                Console.WriteLine("Invalid Client");
                networkStream.Close();
                tcpClient.Close();
                return;
            }

            try
            {
                while (true)
                {
                    header = networkStream.ReadByte();
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
                                    SendTaskRequest();
                                }
                            } break;
                        // Receive task
                        case (byte)TrasmissionFlagsEnum.Task:
                            {
                                Task tp = (Task)formatter.Deserialize(networkStream);
                                lock (taskBuffer)
                                {
                                    taskBuffer.Enqueue(tp);
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

        public void SendResult(int taskId, object result)
        {
            lock (networkStreamLocker)
            {
                ResultPackage rp = new ResultPackage(taskId, result);
                try
                {
                    networkStream.WriteByte((byte)TrasmissionFlagsEnum.Result);
                    formatter.Serialize(networkStream, rp);
                    Console.WriteLine("Result sent back to " + clientName);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Attempt to send result back to " + clientName + " failed: " + e.Message);
                }
            }
        }

        public void SendTaskRequest()
        {
            lock (networkStreamLocker)
            {
                try
                {
                    networkStream.WriteByte((byte)TrasmissionFlagsEnum.TaskRequest);
                    Console.WriteLine("Task request sent to " + clientName + ".");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Attempt to send a task request to " + clientName + " failed: " + e.Message);
                }
            }
        }
    }
}

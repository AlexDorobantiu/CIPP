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

        private TcpClient client;
        private NetworkStream ns;
        private object ns_locker = new object();

        private int header;
        private string client_name;

        private Thread connection_thread;

        private int nrWorkerThreads;
        private SimulationWorkerThread[] workerThreads;

        public Queue<Task> taskBuffer;

        public ConnectionThread(TcpListener tcpListener, int port)
        {
            formatter = new BinaryFormatter();

            this.tcpListener = tcpListener;
            this.port = port;

            taskBuffer = new Queue<Task>();

            nrWorkerThreads = Environment.ProcessorCount;
            workerThreads = new SimulationWorkerThread[nrWorkerThreads];
            for (int i = 0; i < nrWorkerThreads; i++)
            {
                workerThreads[i] = new SimulationWorkerThread(this, "worker thread # " + i);
            }

            connection_thread = new Thread(HandleConnection);
            connection_thread.Name = "Connection thread";
            connection_thread.Start();
        }

        public void HandleConnection()
        {
            try
            {
                client = tcpListener.AcceptTcpClient();
                ns = client.GetStream();
                header = ns.ReadByte(); //Client Name Byte
                if (header != (byte)TrasmissionFlags.ClientName)
                    throw new Exception();

                client_name = (string)formatter.Deserialize(ns);
                Console.WriteLine("Connected to " + client_name + " on port " + port);
            }
            catch
            {
                Console.WriteLine("Invalid Client");
                ns.Close();
                client.Close();
                return;
            }

            try
            {
                while (true)
                {
                    header = ns.ReadByte();
                    if (header == -1) break;
                    switch (header)
                    {
                        //Start sending requests
                        case (byte)TrasmissionFlags.Listening:
                            {
                                Console.WriteLine("Hired by  " + client_name);
                                for (int i = 0; i < nrWorkerThreads; i++)
                                {
                                    workerThreads[i].AbortCurrentTask();
                                    SendTaskRequest();
                                }
                            } break;
                        //Receive task
                        case (byte)TrasmissionFlags.Task:
                            {
                                Task tp = (Task)formatter.Deserialize(ns);
                                lock (taskBuffer)
                                {
                                    taskBuffer.Enqueue(tp);
                                }
                                for (int i = 0; i < nrWorkerThreads; i++)
                                    workerThreads[i].Awake();
                                Console.WriteLine("TaskPackage received from " + client_name);
                            } break;
                        //Stop Working (drop tasks)
                        case (byte)TrasmissionFlags.AbortWork:
                            {
                                for (int i = 0; i < nrWorkerThreads; i++)
                                {
                                    if (workerThreads[i] != null) workerThreads[i].AbortCurrentTask();
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

            Console.WriteLine("Connection to " + client_name + " terminated");

            ns.Close();
            client.Close();
        }

        public void SendResult(int taskId, object result)
        {
            lock (ns_locker)
            {
                ResultPackage rp = new ResultPackage(taskId, result);
                try
                {
                    ns.WriteByte((byte)TrasmissionFlags.Result);
                    formatter.Serialize(ns, rp);
                    Console.WriteLine("Result sent back to " + client_name);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Attempt to send result back to " + client_name + " failed: " + e.Message);
                }
            }
        }

        public void SendTaskRequest()
        {
            lock (ns_locker)
            {
                try
                {
                    ns.WriteByte((byte)TrasmissionFlags.TaskRequest);
                    Console.WriteLine("Task request sent to " + client_name + ".");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Attempt to send a task request to " + client_name + " failed: " + e.Message);
                }
            }
        }
    }
}

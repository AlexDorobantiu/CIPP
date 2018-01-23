using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using CIPPProtocols;
using Plugins.Filters;
using Plugins.Masks;
using Plugins.MotionRecognition;

namespace CIPPServer
{
    class Program
    {
        public const int defaultPort = 6050;
        public const int waitTimeMilliseconds = 5000;

        private const string listeningPortsFilename = "listening_ports.txt";
        public static int[] listeningPorts;

        public static PluginFinder pluginFinder;

        static void Main(string[] args)
        {
            loadListeningPortsFromFile();
            loadPlugins();

            TcpListener[] clients = new TcpListener[listeningPorts.Length];
            for (int i = 0; i < listeningPorts.Length; i++)
            {
                clients[i] = new TcpListener(IPAddress.Any, listeningPorts[i]);
                clients[i].Start();
            }

            Console.WriteLine("Waiting for clients...");
            while (true)
            {
                int clientPending = -1;
                for (int i = 0; i < listeningPorts.Length; i++)
                    if (clients[i].Pending())
                    {
                        clientPending = i;
                        break;
                    }
                if (clientPending == -1)
                {
                    Thread.Sleep(waitTimeMilliseconds);
                }
                else
                {
                    ConnectionThread newconnection = new ConnectionThread(clients[clientPending], listeningPorts[clientPending]);
                }
            }
        }

        static void loadListeningPortsFromFile()
        {
            try
            {
                StreamReader sr = new StreamReader(listeningPortsFilename);
                List<int> list_ports = new List<int>();
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    int port;
                    if (int.TryParse(line, out port))
                    {
                        list_ports.Add(port);
                    }
                }
                int nr_ports = list_ports.Count;

                listeningPorts = list_ports.ToArray();
            }
            catch
            {
                if (listeningPorts != null)
                {
                    if (listeningPorts.Length == 0)
                    {
                        listeningPorts = new int[1];
                        listeningPorts[0] = defaultPort;
                    }
                }
                else
                {
                    listeningPorts = new int[1];
                    listeningPorts[0] = defaultPort;
                }
            }
        }

        static void loadPlugins()
        {
            try
            {
                List<PluginInfo> filterPluginList = PluginHelper.getPluginsList(Path.Combine(Environment.CurrentDirectory, @"plugins\filters"), typeof(IFilter));
                List<PluginInfo> maskPluginList = PluginHelper.getPluginsList(Path.Combine(Environment.CurrentDirectory, @"plugins\masks"), typeof(IMask));
                List<PluginInfo> motionRecognitionPluginList = PluginHelper.getPluginsList(Path.Combine(Environment.CurrentDirectory, @"plugins\motionrecognition"), typeof(IMotionRecognition));
                pluginFinder = new PluginFinder(filterPluginList, maskPluginList, motionRecognitionPluginList);
            }
            catch
            {
                Console.WriteLine("Could not load plugins properly");
            }
        }
    }
}

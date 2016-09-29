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
        public static int[] listeningPorts;

        public static List<PluginInfo> filterPluginList;
        public static List<PluginInfo> maskPluginList;
        public static List<PluginInfo> motionRecognitionPluginList;

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
                    Thread.Sleep(5000);
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
                StreamReader sr = new StreamReader("listening_ports.txt");
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
                    if (listeningPorts.Length==0) {
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
                filterPluginList = PluginHelper.getPluginsList(Path.Combine(Environment.CurrentDirectory, @"plugins\filters"), typeof(IFilter));
                maskPluginList = PluginHelper.getPluginsList(Path.Combine(Environment.CurrentDirectory, @"plugins\masks"), typeof(IMask));
                motionRecognitionPluginList = PluginHelper.getPluginsList(Path.Combine(Environment.CurrentDirectory, @"plugins\motionrecognition"), typeof(IMotionRecognition));
            }
            catch
            {
                Console.WriteLine("Could not load plugins properly");
            }
        }
    }
}

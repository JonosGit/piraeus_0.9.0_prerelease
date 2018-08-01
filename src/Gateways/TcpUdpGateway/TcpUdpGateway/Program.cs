using Orleans;
using Orleans.Runtime.Configuration;
using Piraeus.Configuration;
using Piraeus.Configuration.Settings;
using SkunkLab.Listeners;
using SkunkLab.Listeners.Tcp;
using SkunkLab.Listeners.Udp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TcpUdpGateway.Configuration;

namespace TcpUdpGateway
{
    class Program
    {

        static Dictionary<int, TcpServerListener> tcpListeners;
        static Dictionary<int, UdpServerListener> udpListeners;
        static Dictionary<int, CancellationTokenSource> sources;
        static PiraeusConfig config;



        static void Main(string[] args)
        {
            tcpListeners = new Dictionary<int, TcpServerListener>();
            udpListeners = new Dictionary<int, UdpServerListener>();
            sources = new Dictionary<int, CancellationTokenSource>();
            config = PiraeusConfigManager.Settings;

            InitOrleansClient();


            if (!Orleans.GrainClient.IsInitialized)
            {               
                Console.WriteLine("Orleans client failed to initialize");
                Console.WriteLine("terminating app");
                return;
            }            

            int[] tcpPorts = new int[] { 1883, 8883, 5684 };
            int[] udpPorts = new int[] { 5683, 5883 };

            foreach (var port in tcpPorts)
            {
                sources.Add(port, new CancellationTokenSource());
            }

            foreach (var port in udpPorts)
            {
                sources.Add(port, new CancellationTokenSource());
            }


            string hostname = GetLocalHostName();


            tcpListeners.Add(tcpPorts[0], new TcpServerListener(new IPEndPoint(GetIPAddress(hostname), tcpPorts[0]), config, sources[tcpPorts[0]].Token));
            tcpListeners.Add(tcpPorts[1], new TcpServerListener(new IPEndPoint(GetIPAddress(hostname), tcpPorts[1]), config, sources[tcpPorts[1]].Token));
            tcpListeners.Add(tcpPorts[2], new TcpServerListener(new IPEndPoint(GetIPAddress(hostname), tcpPorts[2]), config, sources[tcpPorts[2]].Token));

            udpListeners.Add(tcpPorts[0], new UdpServerListener(config, new IPEndPoint(GetIPAddress(hostname), udpPorts[0]), sources[udpPorts[0]].Token));
            udpListeners.Add(tcpPorts[1], new UdpServerListener(config, new IPEndPoint(GetIPAddress(hostname), udpPorts[1]), sources[udpPorts[1]].Token));

            KeyValuePair<int, TcpServerListener>[] tcpKvps = tcpListeners.ToArray();

            foreach (var item in tcpKvps)
            {
                item.Value.OnError += Listener_OnError;
                Task task = item.Value.StartAsync();
                Task.WhenAll(task);
            }

            KeyValuePair<int, UdpServerListener>[] udpKvps = udpListeners.ToArray();

            foreach (var item in udpKvps)
            {
                item.Value.OnError += Listener_OnError;
                Task task = item.Value.StartAsync();
                Task.WhenAll(task);
            }

            ManualResetEventSlim done = new ManualResetEventSlim(false);
            
            
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                
                done.Set();
                eventArgs.Cancel = true;
            };

            Console.WriteLine("TCP-UDP Gateway is ready...");
            done.Wait();

            Console.WriteLine("TCP UDP Gateway is exiting.");

        }

        private static void Listener_OnError(object sender, ServerFailedEventArgs e)
        {
            if (sources.ContainsKey(e.Port))
            {
                //cancel the server
                sources[e.Port].Cancel();
            }
            else
            {
                return;
            }

            if (tcpListeners.ContainsKey(e.Port))
            {
                Task task = tcpListeners[e.Port].StopAsync();
                Task.WhenAll(task);
                tcpListeners.Remove(e.Port);
                sources.Remove(e.Port);

                //restart the server
                sources.Add(e.Port, new CancellationTokenSource());
                tcpListeners.Add(e.Port, new TcpServerListener(new IPEndPoint(GetIPAddress("localhost"), e.Port), config, sources[e.Port].Token));


            }

            if (udpListeners.ContainsKey(e.Port))
            {
                Task task = udpListeners[e.Port].StopAsync();
                Task.WhenAll(task);
                udpListeners.Remove(e.Port);
                sources.Remove(e.Port);

                //restart the server
                sources.Add(e.Port, new CancellationTokenSource());
                udpListeners.Add(e.Port, new UdpServerListener(config, new IPEndPoint(GetIPAddress("localhost"), e.Port), sources[e.Port].Token));
            }





        }


        static string GetLocalHostName()
        {
            bool dockerized = Convert.ToBoolean(ConfigurationManager.AppSettings["dockerize"]);
            //return dockerized ? System.Environment.GetEnvironmentVariable("GATEWAY_TCP_SERVER_DNS_HOSTNAME") : "localhost";
            return dockerized ? System.Net.Dns.GetHostName() : "localhost";
        }
        
        static void InitOrleansClient()
        {
            int max = 8;
            int index = 0;

            while (index < max)
            {
                Console.WriteLine("Initializing Orleans client...");
                index++;

                try
                {
                    if (!Orleans.GrainClient.IsInitialized)
                    {
                        bool dockerized = Convert.ToBoolean(ConfigurationManager.AppSettings["dockerize"]);
                        if (!dockerized)
                        {
                            Console.WriteLine("Identified as localhost deployment");
                            if (OrleansClientConfig.TryStart("TCPGateway"))
                                return;
                            else
                                Console.WriteLine("Waiting 30 secs before retry {0} or {1}", index, max);
                        }
                        else
                        {
                            Console.WriteLine("Identified as docker deployment.");
                            if (OrleansClientConfig.TryStart("TCPGateway", System.Environment.GetEnvironmentVariable("GATEWAY_ORLEANS_SILO_DNS_HOSTNAME")))
                                return;
                            else
                                Console.WriteLine("Waiting 30 secs before retry {0} or {1}", index, max);
                        }

                        Task task = ServiceIdentityConfig.Configure();
                        Task.WhenAll(task);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Orleans client failed loudly");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Waiting 30 secs before retry {0} or {1}", index, max);                    
                }

                Thread.Sleep(30000);

            }
        }

        static IPAddress GetIPAddress(string hostname)
        {
            IPHostEntry hostInfo = Dns.GetHostEntry(hostname);
            for (int index = 0; index < hostInfo.AddressList.Length; index++)
            {
                if (hostInfo.AddressList[index].AddressFamily == AddressFamily.InterNetwork)
                {
                    return hostInfo.AddressList[index];
                }
            }

            return null;
        }
    }
}

using Piraeus.Adapters;
using Piraeus.Configuration.Settings;
using SkunkLab.Channels;
using SkunkLab.Diagnostics.Logging;
using SkunkLab.Security.Authentication;
using SkunkLab.Security.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Listeners.Tcp
{
    public class TcpServerListener
    {
        public event EventHandler<ServerFailedEventArgs> OnError;
        private IPAddress serverIP;
        private int serverPort;
        public TcpServerListener(IPEndPoint localEP, PiraeusConfig config, CancellationToken token)
        {
            serverIP = localEP.Address;
            serverPort = localEP.Port;
            listener = new TcpListener(localEP);
            this.token = token;
            dict = new Dictionary<string, ProtocolAdapter>();
            this.config = config;

            if (config.Security.Client.TokenType != null && config.Security.Client.SymmetricKey != null)
            {
                SecurityTokenType stt = (SecurityTokenType)System.Enum.Parse(typeof(SecurityTokenType), config.Security.Client.TokenType, true);
                BasicAuthenticator bauthn = new BasicAuthenticator();
                bauthn.Add(stt, config.Security.Client.SymmetricKey, config.Security.Client.Issuer, config.Security.Client.Audience);
                this.authn = bauthn;
            }

        }

        public TcpServerListener(IPAddress address, int port, PiraeusConfig config, CancellationToken token)
        {
            serverIP = address;
            serverPort = port;
            listener = new TcpListener(address, port);
            listener.ExclusiveAddressUse = false;
            this.token = token;
            dict = new Dictionary<string, ProtocolAdapter>();
            this.config = config;

            if (config.Security.Client.TokenType != null && config.Security.Client.SymmetricKey != null)
            {
                SecurityTokenType stt = (SecurityTokenType)System.Enum.Parse(typeof(SecurityTokenType), config.Security.Client.TokenType, true);
                BasicAuthenticator bauthn = new BasicAuthenticator();
                bauthn.Add(stt, config.Security.Client.SymmetricKey, config.Security.Client.Issuer, config.Security.Client.Audience);
                this.authn = bauthn;
            }
        }


        private TcpListener listener;
        private CancellationToken token;
        private Dictionary<string, ProtocolAdapter> dict;
        private PiraeusConfig config;
        private IAuthenticator authn;

        public async Task StartAsync()
        {
            listener.ExclusiveAddressUse = false;
            listener.Start();

            Console.WriteLine("Listener started on IP {0} Port {1}", serverIP.ToString(), serverPort);

            while (!token.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    client.LingerState = new LingerOption(true, 0);
                    client.NoDelay = true;                   
                    client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    client.Client.UseOnlyOverlappedIO = true;

                    ManageConnection(client);                    
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new ServerFailedEventArgs("TCP", serverPort));
                    await Log.LogErrorAsync("TCP server listener error {0}", ex.Message);
                }
            }
        }

        public async Task StopAsync()
        {
            listener.Stop();

            KeyValuePair<string, ProtocolAdapter>[] kvps = dict.ToArray();

            foreach (var kvp in kvps)
            {
                kvp.Value.Dispose();
            }

            dict.Clear();

            await TaskDone.Done;
        }

        private void ManageConnection(TcpClient client)
        {
            ProtocolAdapter adapter = ProtocolAdapterFactory.Create(config, authn, client, token);
            dict.Add(adapter.Channel.Id, adapter);
            adapter.OnError += Adapter_OnError;
            adapter.OnClose += Adapter_OnClose;
            adapter.Init();

            if(adapter.Channel.RequireBlocking)
            {
                Task openTask1 = adapter.Channel.OpenAsync();
                Task.WaitAll(openTask1);
            }
            else
            {
                Task openTask2 = adapter.Channel.OpenAsync();
                Task.WhenAll(openTask2);
            }
            
            Task receiveTask = adapter.Channel.ReceiveAsync();
            Task.WhenAll(receiveTask);
        }


        private void Adapter_OnClose(object sender, ProtocolAdapterCloseEventArgs args)
        {
            Trace.TraceWarning("Protocol adapter on channel {0} closing.", args.ChannelId);
            if (dict.ContainsKey(args.ChannelId))
            {
                ProtocolAdapter adapter = dict[args.ChannelId];
                adapter.Dispose();
                dict.Remove(args.ChannelId);
            }
        }

        private void Adapter_OnError(object sender, ProtocolAdapterErrorEventArgs args)
        {
            Trace.TraceError("Protocol Adapter on channel {0} threw error {1}", args.ChannelId, args.Error.Message);

            if (dict.ContainsKey(args.ChannelId))
            {
                ProtocolAdapter adapter = dict[args.ChannelId];
                Task task = adapter.Channel.CloseAsync();
                Task.WaitAll(task);
                adapter.Dispose();
                dict.Remove(args.ChannelId);
            }


        }
    }
}

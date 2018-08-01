﻿using Org.BouncyCastle.Crypto.Tls;
using SkunkLab.Diagnostics.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Channels.Tcp
{
    public class TcpServerChannel2 : TcpChannel
    {
        #region ctor
        public TcpServerChannel2(TcpClient client, int blockSize, int maxBufferSize, CancellationToken token)
        {
            this.client = client;
            this.blockSize = blockSize;
            this.maxBufferSize = maxBufferSize;
            this.token = token;
            this.token.Register(async () => await CloseAsync());
            Id = "tcp-" + Guid.NewGuid().ToString();
            Port = ((IPEndPoint)client.Client.LocalEndPoint).Port;
            this.queue = new Queue<byte[]>();
        }

        public TcpServerChannel2(TcpClient client, X509Certificate2 certificate, bool clientAuth, int blockSize, int maxBufferSize, CancellationToken token)
        {
            this.client = client;
            this.certificate = certificate;
            this.clientAuth = clientAuth;
            this.blockSize = blockSize;
            this.maxBufferSize = maxBufferSize;
            this.token = token;
            this.token.Register(async () => await CloseAsync());
            Id = "tcp-" + Guid.NewGuid().ToString();
            Port = ((IPEndPoint)client.Client.LocalEndPoint).Port;
            this.queue = new Queue<byte[]>();
        }

        public TcpServerChannel2(TcpClient client, Dictionary<string, byte[]> presharedKeys, int blockSize, int maxBufferSize,  CancellationToken token)
        {
            this.client = client;
            this.presharedKeys = presharedKeys;
            this.blockSize = blockSize;
            this.maxBufferSize = maxBufferSize;
            this.token = token;
            this.token.Register(async () => await CloseAsync());
            Id = "tcp-" + Guid.NewGuid().ToString();
            Port = ((IPEndPoint)client.Client.LocalEndPoint).Port;
            this.queue = new Queue<byte[]>();
        }

        #endregion


        #region private member variables

        private TlsServerProtocol protocol;
        private Dictionary<string, byte[]> presharedKeys;
        private bool clientAuth;
        private X509Certificate2 certificate;
        private CancellationToken token;
        private TcpClient client;
        private bool disposed;
        private ChannelState _state;
        private Stream stream;
        private SemaphoreSlim readConnection;
        private SemaphoreSlim writeConnection;
        private NetworkStream localStream;
        private int maxBufferSize;
        private int blockSize;
        private Queue<byte[]> queue;

        #endregion

        #region events

        public override event EventHandler<ChannelReceivedEventArgs> OnReceive;
        public override event EventHandler<ChannelCloseEventArgs> OnClose;
        public override event EventHandler<ChannelOpenEventArgs> OnOpen;
        public override event EventHandler<ChannelErrorEventArgs> OnError;
        public override event EventHandler<ChannelStateEventArgs> OnStateChange;

        #endregion

        #region Properties
        public override string Id { get; internal set; }

        public override bool RequireBlocking
        {
            get { return presharedKeys != null; }
        }

        public override string TypeId { get { return "TCP2"; } }

        public override int Port { get; internal set; }

        public override bool IsEncrypted { get; internal set; }

        public override bool IsAuthenticated { get; internal set; }

        public override bool IsConnected { get { return State == ChannelState.Open; } }

        public override ChannelState State
        {
            get
            {
                return _state;
            }

            internal set
            {
                if (value != _state)
                {
                    OnStateChange?.Invoke(this, new ChannelStateEventArgs(Id, value));
                }

                _state = value;
            }
        }

        #endregion

        public override async Task AddMessageAsync(byte[] message)
        {
            OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, message));
            await Task.CompletedTask;
        }
       

        public override async Task OpenAsync()
        {
            State = ChannelState.Connecting;

            readConnection = new SemaphoreSlim(1);
            writeConnection = new SemaphoreSlim(1);

            localStream = client.GetStream();

            if (presharedKeys != null)
            {
                try
                {
                    protocol = client.ConnectPskTlsServer(presharedKeys, localStream);
                    stream = protocol.Stream;
                    IsEncrypted = true;
                }
                catch(Exception ex)
                {
                    State = ChannelState.Aborted;
                    Trace.TraceError(ex.Message);
                    OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ex));
                }
            }
            else if (certificate != null)
            {
                try
                {
                    stream = new SslStream(localStream, true, new RemoteCertificateValidationCallback(ValidateCertificate));
                    IsEncrypted = true;
                    await ((SslStream)stream).AuthenticateAsServerAsync(certificate, clientAuth, SslProtocols.Tls12, true);
                }               
                catch (Exception ex)
                {
                    State = ChannelState.Aborted;
                    Trace.TraceError(ex.Message);
                    OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ex));
                }
            }
            else
            {
                stream = localStream;
            }

            State = ChannelState.Open;
            OnOpen?.Invoke(this, new ChannelOpenEventArgs(Id, null));
        }
        
        public override async Task SendAsync(byte[] msg)
        {
           
            int offset = 0;
            byte[] buffer = null;
            

            if(msg.Length > maxBufferSize)
            {
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, new InvalidDataException("Message exceeds max buffer size to send.")));
                return;
            }

            queue.Enqueue(msg);

            while (queue.Count > 0)
            {
                byte[] message = queue.Dequeue();

                try
                {
                    await writeConnection.WaitAsync();

                    int segments = message.Length / blockSize;
                    segments = message.Length % blockSize > 0 ? segments + 1 : segments;

                    int index = 0;
                    while (index < segments)
                    {
                        if (index + 1 == segments)
                        {
                            buffer = new byte[message.Length - offset];
                        }
                        else
                        {
                            buffer = new byte[blockSize];
                        }

                        Buffer.BlockCopy(message, offset, buffer, 0, buffer.Length);
                        offset += blockSize;                       

                        if (stream != null && stream.CanWrite)
                        {
                            if (protocol != null)
                            {
                                stream.Write(buffer, 0, buffer.Length);
                                stream.Flush();
                            }
                            else
                            {
                                await stream.WriteAsync(buffer, 0, buffer.Length);
                                await stream.FlushAsync();
                            }
                        }
                        else
                        {
                            await Log.LogInfoAsync("Channel {0} tcp server channel2 cannot send because stream is not writable at this time.", this.Id);
                        }

                        index++;
                    }
                    
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.Message);
                    State = ChannelState.Aborted;
                    OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ex));
                }
                finally
                {
                    writeConnection.Release();
                }
            }
        }

      

        public override async Task ReceiveAsync()
        {

            byte[] buffer = null;
            int offset = 0;
            int bytesRead = 0;
            byte[] msgBuffer = null;
            byte[] tempBuffer = null;

            try
            {
                while (client != null && client.Connected && !token.IsCancellationRequested)
                {
                    await readConnection.WaitAsync();
                    buffer = new byte[blockSize];

                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    offset += msgBuffer == null ? 0 : msgBuffer.Length;

                    if (offset + bytesRead > this.maxBufferSize)
                    {
                        await Log.LogErrorAsync("Message receives by tcp server channel2 exceeds maximum message size.  Will close channel.");
                        OnError?.Invoke(this, new ChannelErrorEventArgs(Id, new InvalidDataException("Message exceeds max buffer size to read.")));
                        return;
                    }

                    if (bytesRead == 0)
                    {
                        if(msgBuffer == null)
                        {
                            await CloseAsync();
                        }
                        else if(msgBuffer != null)
                        {
                            byte[] receiveBuffer = new byte[msgBuffer.Length];
                            Buffer.BlockCopy(msgBuffer, 0, receiveBuffer, 0, msgBuffer.Length);
                            OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, receiveBuffer));
                            msgBuffer = null;
                            tempBuffer = null;
                            offset = 0;
                        }
                    }
                    else if (offset == 0)
                    {
                        msgBuffer = new byte[bytesRead];
                        Buffer.BlockCopy(buffer, 0, msgBuffer, 0, bytesRead);
                    }
                    else
                    {
                        tempBuffer = new byte[offset + bytesRead];
                        Buffer.BlockCopy(msgBuffer, 0, tempBuffer, 0, msgBuffer.Length);
                        Buffer.BlockCopy(buffer, 0, tempBuffer, offset, bytesRead);
                        msgBuffer = tempBuffer;
                    }

                    if(!localStream.DataAvailable || protocol != null)
                    {
                        if (msgBuffer != null)
                        {
                            byte[] receiveBuffer = new byte[msgBuffer.Length];
                            Buffer.BlockCopy(msgBuffer, 0, receiveBuffer, 0, msgBuffer.Length);
                            OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, receiveBuffer));
                            msgBuffer = null;
                            tempBuffer = null;
                            offset = 0;
                        }
                        else
                        {
                            await CloseAsync();
                        }
                    }

                    readConnection.Release();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException.Message);
                Console.WriteLine(ex.InnerException.StackTrace);
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ex));
                await Log.LogErrorAsync("Tcp Server Channel {0} receive error {1}", Id, ex.Message);
            }
            finally
            {
                readConnection.Release();
                await CloseAsync();
            }

            //byte[] buffer = null;
            //int offset = 0;
            //int bytesRead = 0;
            //byte[] msgBuffer = null;
            //byte[] tempBuffer = null;

            //try
            //{
            //    while (client != null && client.Connected && !token.IsCancellationRequested)
            //    {
            //        await readConnection.WaitAsync();
            //        buffer = new byte[blockSize];

            //        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            //        {
            //            offset += msgBuffer == null ? 0 : msgBuffer.Length;

            //            if (offset + bytesRead > this.maxBufferSize)
            //            {
            //                await Log.LogErrorAsync("Message receives by tcp server channel2 exceeds maximum message size.  Will close channel.");
            //                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, new InvalidDataException("Message exceeds max buffer size to read.")));
            //                return;
            //            }

            //            if (offset == 0)
            //            {
            //                tempBuffer = new byte[bytesRead];
            //                Buffer.BlockCopy(buffer, 0, tempBuffer, offset, bytesRead);
            //            }
            //            else
            //            {
            //                tempBuffer = new byte[msgBuffer.Length + bytesRead];
            //                Buffer.BlockCopy(msgBuffer, 0, tempBuffer, 0, msgBuffer.Length);
            //                Buffer.BlockCopy(buffer, 0, tempBuffer, offset, bytesRead);
            //            }

            //            if (!localStream.DataAvailable)
            //            {
            //                break;
            //            }
            //        }

            //        if (tempBuffer != null && tempBuffer.Length > 0)
            //        {
            //            msgBuffer = tempBuffer;
            //            OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, msgBuffer));
            //            offset = 0;
            //            tempBuffer = null;
            //            msgBuffer = null;

            //            if (this.client == null || !this.client.Client.Blocking)
            //            {
            //                break;
            //            }
            //        }

            //        readConnection.Release();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    if (readConnection != null && readConnection.CurrentCount > 0)
            //    {
            //        readConnection.Release();
            //    }

            //    OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ex));
            //    await Log.LogErrorAsync("Tcp Server Channel receive error {0}", ex.Message);
            //}
            //finally
            //{
            //    await CloseAsync();
            //}
        }

        

        public override async Task CloseAsync()
        {
            if (State == ChannelState.Closed)
            {
                return;
            }

            State = ChannelState.ClosedReceived;

            if (writeConnection != null && writeConnection.CurrentCount > 0)
            {
                writeConnection.Release();
            }

            if (readConnection != null && readConnection.CurrentCount > 0)
            {
                readConnection.Release();
            }

            if (this.client != null && this.client.Client.Blocking)
            {
                if (protocol != null)
                {
                    protocol.Close();
                }
            }

            protocol = null;

            if (client != null)
            {
                client.Close();
            }

            State = ChannelState.Closed;
            OnClose?.Invoke(this, new ChannelCloseEventArgs(Id));

            await Task.CompletedTask;
        }

        protected void Disposing(bool dispose)
        {
            if (dispose & !disposed)
            {               

                if (this.client != null && this.client.Client.Blocking)
                {
                    protocol.Close();
                }

                protocol = null;

                if (client != null)
                {
                    client.Close();
                }

                client = null;
                disposed = true;

                if (readConnection != null)
                {
                    readConnection.Dispose();
                }

                if (writeConnection != null)
                {
                    writeConnection.Dispose();
                }
            }
        }

        public override void Dispose()
        {
            Disposing(true);
            GC.SuppressFinalize(this);
        }


        private bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            if (sslpolicyerrors != SslPolicyErrors.None)
            {
                return false;
            }

            if (certificate == null)
            {
                return false;
            }
            else
            {
                X509Certificate2 cert = new X509Certificate2(certificate);
                return (cert.NotBefore < DateTime.Now && cert.NotAfter > DateTime.Now);
            }
        }

       
    }
}

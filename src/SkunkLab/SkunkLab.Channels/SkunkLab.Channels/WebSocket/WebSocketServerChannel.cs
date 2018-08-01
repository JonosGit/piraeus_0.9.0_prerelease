using System;
using System.ComponentModel;
using System.Net.Http;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.WebSockets;

namespace SkunkLab.Channels.WebSocket
{
    public class WebSocketServerChannel : WebSocketChannel
    {
        public WebSocketServerChannel(HttpRequestMessage request, WebSocketConfig config, CancellationToken token)
        {
            Id = "ws-" + Guid.NewGuid().ToString();
            this.config = config;
            this.token = token;
            this.IsEncrypted = request.RequestUri.Scheme == "wss";
            this.IsAuthenticated = HttpContext.Current.Request.IsAuthenticated;
            //this.IsAuthenticated = Thread.CurrentPrincipal.Identity.IsAuthenticated;
            this.handler = new WebSocketHandler(config, token);
            HttpContext.Current.AcceptWebSocketRequest(this.handler);
            this.handler.OnReceive += Handler_OnReceive;
            this.handler.OnError += Handler_OnError;
            this.handler.OnOpen += Handler_OnOpen;
            this.handler.OnClose += Handler_OnClose;
            
          
            //HttpContext.Current.AcceptWebSocketRequest(this);
        }

        

        private WebSocketHandler handler;
        private WebSocketConfig config;
        private CancellationToken token;
        private AspNetWebSocketContext WebSocketContext;
        private readonly TaskQueue _sendQueue = new TaskQueue();
        private bool disposed;

        public override event EventHandler<ChannelReceivedEventArgs> OnReceive;
        public override event EventHandler<ChannelCloseEventArgs> OnClose;
        public override event EventHandler<ChannelOpenEventArgs> OnOpen;
        public override event EventHandler<ChannelErrorEventArgs> OnError;
        public override event EventHandler<ChannelStateEventArgs> OnStateChange;

        private ChannelState _state;



        public override string Id { get; internal set; }

        public override bool RequireBlocking
        {
            get { return false; }
        }

        public override string TypeId { get { return "WebSocket"; } }

        public override int Port { get; internal set; }

        public override bool IsEncrypted { get; internal set; }

        public override bool IsAuthenticated { get; internal set; }

        public override ChannelState State
        {
            get
            {
                return _state;
            }
            internal set
            {
                if (_state != value)
                {
                    OnStateChange?.Invoke(this, new ChannelStateEventArgs(Id, value));
                }

                _state = value;
            }
        }

        public override bool IsConnected
        {
            get { return State == ChannelState.Open; }
        }

        public override void Open()
        {
            State = ChannelState.Open;
        }

        #region Handler Events

        private void Handler_OnClose(object sender, WebSocketCloseEventArgs args)
        {
            State = ChannelState.Closed;
            OnClose?.Invoke(this, new ChannelCloseEventArgs(this.Id));
        }

        private void Handler_OnOpen(object sender, WebSocketOpenEventArgs args)
        {
            State = ChannelState.Open;
            OnOpen?.Invoke(this, new ChannelOpenEventArgs(this.Id, null));
        }

        private void Handler_OnError(object sender, WebSocketErrorEventArgs args)
        {
            OnError?.Invoke(this, new ChannelErrorEventArgs(this.Id, args.Error));
        }

        private void Handler_OnReceive(object sender, WebSocketReceiveEventArgs args)
        {
            OnReceive?.Invoke(this, new ChannelReceivedEventArgs(this.Id, args.Message));
        }

        #endregion

        public override void Send(byte[] message)
        {
            Task task = Task.Factory.StartNew(async () =>
            {
                await handler.SendAsync(message, WebSocketMessageType.Binary);
            });

            Task.WaitAll(task);

            
        }

        public override async Task AddMessageAsync(byte[] message)
        {
            OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, message));
            await TaskDone.Done;
        }

        public override async Task CloseAsync()
        {
            if (IsConnected)
            {
                State = ChannelState.ClosedReceived;
            }

            OnClose?.Invoke(this, new ChannelCloseEventArgs(Id));
            await TaskDone.Done;
            
        }

        protected void Disposing(bool dispose)
        {
            if (dispose & !disposed)
            {
                disposed = true;
                
                if(State == ChannelState.Open)
                {
                    this.handler.Close();
                    //Task task = CloseAsync();
                    //Task.WhenAll(task);
                }

                if(WebSocketContext != null && WebSocketContext.WebSocket != null)
                {
                    WebSocketContext.WebSocket.Dispose();
                }
            }
        }

        public override void Dispose()
        {
            Disposing(true);
            GC.SuppressFinalize(this);
        }

        public override async Task OpenAsync()
        {
            await TaskDone.Done;
        }

        public override async Task ReceiveAsync()
        {
            await TaskDone.Done;
        }

        public override async Task SendAsync(byte[] message)
        {
            await  this.handler.SendAsync(message, WebSocketMessageType.Binary);
        }

        private static bool IsFatalException(Exception ex)
        {
            
            COMException exception = ex as COMException;
            if (exception != null)
            {
                switch (((uint)exception.ErrorCode))
                {
                    case 0x80070026:
                    case 0x800703e3:
                    case 0x800704cd:
                        return false;
                }
            }
            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public Task ProcessWebSocketRequestAsync(AspNetWebSocketContext webSocketContext)
        {
            if (webSocketContext == null)
            {
                throw new ArgumentNullException("webSocketContext");
            }

            byte[] buffer = new byte[config.ReceiveLoopBufferSize];
            System.Net.WebSockets.WebSocket webSocket = webSocketContext.WebSocket;
            return ProcessWebSocketRequestAsync(webSocketContext, () => WebSocketMessageReader.ReadMessageAsync(webSocket, buffer, config.MaxIncomingMessageSize, token));
        }

        internal async Task ProcessWebSocketRequestAsync(AspNetWebSocketContext webSocketContext, Func<Task<WebSocketMessage>> messageRetriever)
        {
            try
            {
                WebSocketContext = webSocketContext;
                OnOpen?.Invoke(this, new ChannelOpenEventArgs(Id, null));                

                while (!token.IsCancellationRequested && WebSocketContext.WebSocket.State == WebSocketState.Open)
                {
                    WebSocketMessage message = await messageRetriever();
                    if (message.MessageType == WebSocketMessageType.Binary)
                    {
                        OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, message.Data as byte[]));
                    }
                    else if (message.MessageType == WebSocketMessageType.Text)
                    {
                        OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, Encoding.UTF8.GetBytes(message.Data as string)));
                    }
                    else
                    {
                        //close received
                        OnClose?.Invoke(this, new ChannelCloseEventArgs(Id));
                        break;
                    }
                }
            }
            catch (AggregateException ae)
            {
                if (!(WebSocketContext.WebSocket.State == WebSocketState.CloseReceived ||
                    WebSocketContext.WebSocket.State == WebSocketState.CloseSent))
                {
                    OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ae.Flatten()));
                }
            }
            catch (Exception exception)
            {
                if (!(WebSocketContext.WebSocket.State == WebSocketState.CloseReceived ||
                    WebSocketContext.WebSocket.State == WebSocketState.CloseSent))
                {
                    if (IsFatalException(exception))
                    {
                        OnError?.Invoke(this, new ChannelErrorEventArgs(Id, exception));
                    }
                }
            }
            finally
            {
                try
                {
                    await CloseAsync();
                }
                finally
                {
                    IDisposable disposable = this as IDisposable;
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
            }


        }
    }
}

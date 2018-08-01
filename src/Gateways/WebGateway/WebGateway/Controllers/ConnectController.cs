using Piraeus.Adapters;
using Piraeus.Configuration.Settings;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WebGateway.Formatters;
using WebGateway.Security;

namespace WebGateway.Controllers
{
    public class ConnectController : ApiController
    {
        public ConnectController()
        {
            if (!Orleans.GrainClient.IsInitialized)
            {
                bool dockerized = Convert.ToBoolean(ConfigurationManager.AppSettings["dockerize"]);
                if (!dockerized)
                {                    
                    OrleansClientConfig.TryStart("ConnectController");
                }
                else
                {
                    OrleansClientConfig.TryStart("ConnectController", System.Environment.GetEnvironmentVariable("GATEWAY_ORLEANS_SILO_DNS_HOSTNAME"));
                }

                Task task = ServiceIdentityConfig.Configure();
                Task.WhenAll(task);
            }

            try
            {
                config = Piraeus.Configuration.PiraeusConfigManager.Settings;
                source = new CancellationTokenSource();

                Trace.TraceInformation("Orleans grain client initialized {0} is connect controller", Orleans.GrainClient.IsInitialized);
            }
            catch(Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }

        private CancellationTokenSource source;
        private Piraeus.Configuration.Settings.PiraeusConfig config;
        private delegate void HttpResponseObserverHandler(object sender, SkunkLab.Channels.ChannelObserverEventArgs args);
        private event HttpResponseObserverHandler OnMessage;
        private ProtocolAdapter adapter;
        private HttpResponseMessage response;
        private WaitHandle[] waitHandles = new WaitHandle[]
        {
            new AutoResetEvent(false)
        };

        [HttpPost]
        public HttpResponseMessage Post()
        {
            try
            {

                adapter = ProtocolAdapterFactory.Create(config, Request, source.Token);
                adapter.OnClose += Adapter_OnClose;
                adapter.Init();
                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public HttpResponseMessage Get()
        {
            try
            {
                SkunkLab.Security.Authentication.BasicAuthenticator authn = new SkunkLab.Security.Authentication.BasicAuthenticator();
             
                HttpContext context = HttpContext.Current;

                if (context.IsWebSocketRequest ||
                context.IsWebSocketRequestUpgrading)
                {
                    PiraeusConfig config = Piraeus.Configuration.PiraeusConfigManager.Settings;

                    adapter = ProtocolAdapterFactory.Create(config, Request, source.Token);
                    adapter.OnClose += Adapter_OnClose;
                    adapter.OnError += Adapter_OnError;
                    adapter.Init();
                    return new HttpResponseMessage(HttpStatusCode.SwitchingProtocols);
                }
                else //long polling
                {
                    adapter = ProtocolAdapterFactory.Create(config, Request, source.Token);
                    adapter.OnObserve += Adapter_OnObserve;
                    adapter.Init();
                    ThreadPool.QueueUserWorkItem(new WaitCallback(Listen), waitHandles[0]);
                    WaitHandle.WaitAll(waitHandles);
                    Task task = adapter.Channel.CloseAsync();
                    Task.WhenAll(task);
                    return response;
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        private void Adapter_OnError(object sender, ProtocolAdapterErrorEventArgs e)
        {
            Exception ex = e.Error;
        }

        private void Adapter_OnObserve(object sender, SkunkLab.Channels.ChannelObserverEventArgs e)
        {
            OnMessage?.Invoke(this, e);
        }

        private void Adapter_OnClose(object sender, ProtocolAdapterCloseEventArgs e)
        {
            adapter.Dispose();
        }

        private void Listen(object state)
        {
            AutoResetEvent are = (AutoResetEvent)state;
            OnMessage += (o, a) => {

                MediaTypeFormatter formatter = null;
                if (a.ContentType == "application/octet-stream")
                {
                    formatter = new BinaryMediaTypeFormatter();
                }
                else if (a.ContentType == "text/plain")
                {
                    formatter = new TextMediaTypeFormatter();
                }
                else if (a.ContentType == "application/xml" || a.ContentType == "text/xml")
                {
                    formatter = new XmlMediaTypeFormatter();
                }
                else if (a.ContentType == "application/json" || a.ContentType == "text/json")
                {
                    formatter = new JsonMediaTypeFormatter();
                }
                else
                {
                    throw new SkunkLab.Protocols.Coap.UnsupportedMediaTypeException("Media type formatter not available.");
                }

                if (a.ContentType != "application/octet-stream")
                {
                    response = Request.CreateResponse<string>(HttpStatusCode.OK, Encoding.UTF8.GetString(a.Message), formatter);
                }
                else
                {
                    response = Request.CreateResponse<byte[]>(HttpStatusCode.OK, a.Message, formatter);
                }

                response.Headers.Add("x-sl-resource", a.ResourceUriString);
                are.Set();
            };
        }
    }
}

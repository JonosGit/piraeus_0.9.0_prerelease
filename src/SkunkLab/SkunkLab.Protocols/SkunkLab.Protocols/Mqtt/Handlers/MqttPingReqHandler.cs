using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Protocols.Mqtt.Handlers
{
    public class MqttPingReqHandler : MqttMessageHandler
    {
        public MqttPingReqHandler(MqttSession session, MqttMessage message)
            : base(session, message)
        {

        }

        public override async Task<MqttMessage> ProcessAsync()
        {
            
            if(!Session.IsConnected)
            {
                Trace.TraceWarning("Ping request recieved and session is disconnected.");
                Session.Disconnect(Message);
                return null;
            }

            Trace.TraceWarning("Ping request recieved and keep alive to be incremented.");
            Session.IncrementKeepAlive();
            
            return await Task.FromResult<MqttMessage>(new PingResponseMessage());
        }
    }
}

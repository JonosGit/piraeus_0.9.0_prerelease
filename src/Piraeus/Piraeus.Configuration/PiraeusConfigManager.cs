using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Piraeus.Configuration.Settings;

namespace Piraeus.Configuration
{
    public class PiraeusConfigManager
    {
        static PiraeusConfigManager()
        {

            bool dockerize = Convert.ToBoolean(ConfigurationManager.AppSettings["dockerize"]);
            if(dockerize)
            {
                LoadEnvVars();
                //OutputVariables();
                
            }
            else
            {
                //FakeConfig fake = new FakeConfig();
                //LoadEnvVars();
                LoadConfigFile();
            }            
        }

        //private static void OutputVariables()
        //{
        //    Trace.TraceWarning("ORLEANS_PROVIDER_DATACONNECTIONSTRING={0}", System.Environment.GetEnvironmentVariable("ORLEANS_PROVIDER_DATACONNECTIONSTRING"));
        //    Trace.TraceWarning("ORLEANS_LIVENESS_DATACONNECTIONSTRING={0}", System.Environment.GetEnvironmentVariable("ORLEANS_LIVENESS_DATACONNECTIONSTRING"));
        //    Trace.TraceWarning("ORLEANS_AUDIT_DATACONNECTIONSTRING={0}", System.Environment.GetEnvironmentVariable("ORLEANS_AUDIT_DATACONNECTIONSTRING"));
        //    Trace.TraceWarning("ORLEANS_DEPLOYMENT_ID={0}", System.Environment.GetEnvironmentVariable("ORLEANS_DEPLOYMENT_ID"));
        //    Trace.TraceWarning("ORLEANS_STORAGE_PROVIDER_TYPE={0}", System.Environment.GetEnvironmentVariable("ORLEANS_STORAGE_PROVIDER_TYPE"));
        //    Trace.TraceWarning("ORLEANS_STORAGE_CONTAINER_NAME={0}", System.Environment.GetEnvironmentVariable("ORLEANS_STORAGE_CONTAINER_NAME"));
        //    Trace.TraceWarning("ORLEANS_MAXMEMORY_STORAGE_GRAINS={0}", System.Environment.GetEnvironmentVariable("ORLEANS_MAXMEMORY_STORAGE_GRAINS"));
        //    Trace.TraceWarning("ORLEANS_DNS_HOSTNAME={0}", System.Environment.GetEnvironmentVariable("ORLEANS_DNS_HOSTNAME"));
        //    Trace.TraceWarning("ORLEANS_AUDIT_TABLENAME={0}", System.Environment.GetEnvironmentVariable("ORLEANS_AUDIT_TABLENAME"));
        //    Trace.TraceWarning("SERVICEPOINT_CORE_FACTOR={0}", System.Environment.GetEnvironmentVariable("SERVICEPOINT_CORE_FACTOR"));
        //    Trace.TraceWarning("MGMT_API_AUDIENCE={0}", System.Environment.GetEnvironmentVariable("MGMT_API_AUDIENCE"));
        //    Trace.TraceWarning("MGMT_API_ISSUER={0}", System.Environment.GetEnvironmentVariable("MGMT_API_ISSUER"));
        //    Trace.TraceWarning("MGMT_API_SYMMETRICKEY={0}", System.Environment.GetEnvironmentVariable("MGMT_API_SYMMETRICKEY"));
        //    Trace.TraceWarning("MGMT_API_NAME_CLAIM_TYPE={0}", System.Environment.GetEnvironmentVariable("MGMT_API_NAME_CLAIM_TYPE"));
        //    Trace.TraceWarning("MGMT_API_ROLE_CLAIM_TYPE={0}", System.Environment.GetEnvironmentVariable("MGMT_API_ROLE_CLAIM_TYPE"));
        //    Trace.TraceWarning("MGMT_API_ROLE_CLAIM_VALUE={0}", System.Environment.GetEnvironmentVariable("MGMT_API_ROLE_CLAIM_VALUE"));
        //    Trace.TraceWarning("MGMT_API_SECURITY_CODE={0}", System.Environment.GetEnvironmentVariable("MGMT_API_SECURITY_CODE"));
        //    Trace.TraceWarning("GATEWAY_ORLEANS_SILO_DNS_HOSTNAME={0}", System.Environment.GetEnvironmentVariable("GATEWAY_ORLEANS_SILO_DNS_HOSTNAME"));
        //    Trace.TraceWarning("GATEWAY_TCP_SERVER_DNS_HOSTNAME={0}", System.Environment.GetEnvironmentVariable("GATEWAY_TCP_SERVER_DNS_HOSTNAME"));
        //    Trace.TraceWarning("WEBSOCKET_MAX_INCOMING_MESSAGE_SIZE={0}", System.Environment.GetEnvironmentVariable("WEBSOCKET_MAX_INCOMING_MESSAGE_SIZE"));
        //    Trace.TraceWarning("WEBSOCKET_RECEIVE_LOOP_BUFFER_SIZE={0}", System.Environment.GetEnvironmentVariable("WEBSOCKET_RECEIVE_LOOP_BUFFER_SIZE"));
        //    Trace.TraceWarning("WEBSOCKET_SEND_BUFFER_SIZE={0}", System.Environment.GetEnvironmentVariable("WEBSOCKET_SEND_BUFFER_SIZE"));
        //    Trace.TraceWarning("WEBSOCKET_CLOSE_TIMEOUT_MILLISECONDS={0}", System.Environment.GetEnvironmentVariable("WEBSOCKET_CLOSE_TIMEOUT_MILLISECONDS"));
        //    Trace.TraceWarning("USER_AUDIT_TABLENAME={0}", System.Environment.GetEnvironmentVariable("USER_AUDIT_TABLENAME"));
        //    Trace.TraceWarning("TCP_USE_LENGTH_PREFIX={0}", System.Environment.GetEnvironmentVariable("TCP_USE_LENGTH_PREFIX"));
        //    Trace.TraceWarning("TCP_BLOCK_SIZE={0}", System.Environment.GetEnvironmentVariable("TCP_BLOCK_SIZE"));
        //    Trace.TraceWarning("TCP_MAX_BUFFER_SIZE={0}", System.Environment.GetEnvironmentVariable("TCP_MAX_BUFFER_SIZE"));
        //    Trace.TraceWarning("TCP_PSK_IDENTITY={0}", System.Environment.GetEnvironmentVariable("TCP_PSK_IDENTITY"));
        //    Trace.TraceWarning("TCP_PSK_KEY={0}", System.Environment.GetEnvironmentVariable("TCP_PSK_KEY"));
        //    Trace.TraceWarning("TCP_CERT_AUTHN={0}", System.Environment.GetEnvironmentVariable("TCP_CERT_AUTHN"));
        //    Trace.TraceWarning("TCP_CERT_STORE={0}", System.Environment.GetEnvironmentVariable("TCP_CERT_STORE"));
        //    Trace.TraceWarning("TCP_CERT_LOCATION={0}", System.Environment.GetEnvironmentVariable("TCP_CERT_LOCATION"));
        //    Trace.TraceWarning("TCP_CERT_THUMBPRINT={0}", System.Environment.GetEnvironmentVariable("TCP_CERT_THUMBPRINT"));
        //    Trace.TraceWarning("MQTT_KEEP_ALIVE_SECONDS={0}", System.Environment.GetEnvironmentVariable("MQTT_KEEP_ALIVE_SECONDS"));
        //    Trace.TraceWarning("MQTT_ACK_RANDOM_FACTOR={0}", System.Environment.GetEnvironmentVariable("MQTT_ACK_RANDOM_FACTOR"));
        //    Trace.TraceWarning("MQTT_ACK_TIMEOUT_SECONDS={0}", System.Environment.GetEnvironmentVariable("MQTT_ACK_TIMEOUT_SECONDS"));
        //    Trace.TraceWarning("MQTT_MAX_RETRANSMIT={0}", System.Environment.GetEnvironmentVariable("MQTT_MAX_RETRANSMIT"));
        //    Trace.TraceWarning("MQTT_MAX_LATENCY_SECONDS={0}", System.Environment.GetEnvironmentVariable("MQTT_MAX_LATENCY_SECONDS"));
        //    Trace.TraceWarning("COAP_HOSTNAME={0}", System.Environment.GetEnvironmentVariable("COAP_HOSTNAME"));
        //    Trace.TraceWarning("COAP_OBSERVE_OPTION={0}", System.Environment.GetEnvironmentVariable("COAP_OBSERVE_OPTION"));
        //    Trace.TraceWarning("COAP_NORESPONSE_OPTION={0}", System.Environment.GetEnvironmentVariable("COAP_NORESPONSE_OPTION"));
        //    Trace.TraceWarning("COAP_AUTO_RETRY={0}", System.Environment.GetEnvironmentVariable("COAP_AUTO_RETRY"));
        //    Trace.TraceWarning("COAP_NSTART={0}", System.Environment.GetEnvironmentVariable("COAP_NSTART"));
        //    Trace.TraceWarning("COAP_DEFAULT_LEISURE={0}", System.Environment.GetEnvironmentVariable("COAP_DEFAULT_LEISURE"));
        //    Trace.TraceWarning("COAP_PROBING_RATE={0}", System.Environment.GetEnvironmentVariable("COAP_PROBING_RATE"));
        //    Trace.TraceWarning("COAP_KEEP_ALIVE_SECONDS={0}", System.Environment.GetEnvironmentVariable("COAP_KEEP_ALIVE_SECONDS"));
        //    Trace.TraceWarning("COAP_ACK_TIMEOUT_SECONDS={0}", System.Environment.GetEnvironmentVariable("COAP_ACK_TIMEOUT_SECONDS"));
        //    Trace.TraceWarning("COAP_ACK_RANDOM_FACTOR={0}", System.Environment.GetEnvironmentVariable("COAP_ACK_RANDOM_FACTOR"));
        //    Trace.TraceWarning("COAP_MAX_RETRANSMIT={0}", System.Environment.GetEnvironmentVariable("COAP_MAX_RETRANSMIT"));
        //    Trace.TraceWarning("COAP_MAX_LATENCY_SECONDS={0}", System.Environment.GetEnvironmentVariable("COAP_MAX_LATENCY_SECONDS"));
        //    Trace.TraceWarning("CLIENT_IDENTITY_NAME_CLAIM_TYPE={0}", System.Environment.GetEnvironmentVariable("CLIENT_IDENTITY_NAME_CLAIM_TYPE"));
        //    Trace.TraceWarning("CLIENT_IDENTITY_INDEXES_CLAIM_TYPES={0}", System.Environment.GetEnvironmentVariable("CLIENT_IDENTITY_INDEXES_CLAIM_TYPES"));
        //    Trace.TraceWarning("CLIENT_IDENTITY_INDEXES_CLAIM_INDEX_KEYS={0}", System.Environment.GetEnvironmentVariable("CLIENT_IDENTITY_INDEXES_CLAIM_INDEX_KEYS"));
        //    Trace.TraceWarning("SERVICE_IDENTITY_CLAIM_TYPES={0}", System.Environment.GetEnvironmentVariable("SERVICE_IDENTITY_CLAIM_TYPES"));
        //    Trace.TraceWarning("SERVICE_IDENTITY_CLAIM_VALUES={0}", System.Environment.GetEnvironmentVariable("SERVICE_IDENTITY_CLAIM_VALUES"));
        //    Trace.TraceWarning("CLIENT_SECURITY_TOKEN_TYPE={0}", System.Environment.GetEnvironmentVariable("CLIENT_SECURITY_TOKEN_TYPE"));
        //    Trace.TraceWarning("CLIENT_SECURITY_SYMMETRIC_KEY={0}", System.Environment.GetEnvironmentVariable("CLIENT_SECURITY_SYMMETRIC_KEY"));
        //    Trace.TraceWarning("CLIENT_SECURITY_ISSUER={0}", System.Environment.GetEnvironmentVariable("CLIENT_SECURITY_ISSUER"));
        //    Trace.TraceWarning("CLIENT_SECURITY_AUDIENCE={0}", System.Environment.GetEnvironmentVariable("CLIENT_SECURITY_AUDIENCE"));
        //    Trace.TraceWarning("SERVICE_SECURITY_CERT_STORE={0}", System.Environment.GetEnvironmentVariable("SERVICE_SECURITY_CERT_STORE"));
        //    Trace.TraceWarning("SERVICE_SECURITY_CERT_LOCATION={0}", System.Environment.GetEnvironmentVariable("SERVICE_SECURITY_CERT_LOCATION"));
        //    Trace.TraceWarning("SERVICE_SECURITY_CERT_THUMBPRINT={0}", System.Environment.GetEnvironmentVariable("SERVICE_SECURITY_CERT_THUMBPRINT"));
        //}

        private static PiraeusConfig config;

        public static PiraeusConfig Settings
        {
            get { return config; }
        }
        

        private static X509Certificate2 GetCertificate(string store, string location, string thumbprint)
        {
            if(string.IsNullOrEmpty(store) || string.IsNullOrEmpty(location) || string.IsNullOrEmpty(thumbprint))
            {
                return null;
            }


            thumbprint = Regex.Replace(thumbprint, @"[^\da-fA-F]", string.Empty).ToUpper();


            StoreName storeName = (StoreName)Enum.Parse(typeof(StoreName), store, true);
            StoreLocation storeLocation= (StoreLocation)Enum.Parse(typeof(StoreLocation), location, true);
            

            X509Store certStore = new X509Store(storeName, storeLocation);
            certStore.Open(OpenFlags.ReadOnly);

            X509Certificate2Collection coll = certStore.Certificates;

            X509Certificate2Collection certCollection =
              certStore.Certificates.Find(X509FindType.FindByThumbprint,
                                      thumbprint.ToUpper(), false);
            X509Certificate2Enumerator enumerator = certCollection.GetEnumerator();
            X509Certificate2 cert = null;
            while (enumerator.MoveNext())
            {
                cert = enumerator.Current;
            }
            return cert;

        }

        private static void LoadConfigFile()
        {
            ChannelSettings channelSettings = null;
            ProtocolSettings protocolSettings = null;
            IdentitySettings identitySettings = null;
            SecuritySettings securitySettings = null;


            PiraeusSection section = ConfigurationManager.GetSection("piraeus") as PiraeusSection;
            if (section == null)
                throw new ConfigurationErrorsException("Piraeus configuration section not found.");

            if (section.Channels != null)
            {
                WebSocketSettings websocket = null;
                TcpSettings tcp = null;
                if (section.Channels.WebSocket != null)
                {
                    websocket = new WebSocketSettings(section.Channels.WebSocket.MaxIncomingMessageSize,
                                                                section.Channels.WebSocket.ReceiveLoopBufferSize,
                                                                section.Channels.WebSocket.SendBufferSize,
                                                                section.Channels.WebSocket.CloseTimeoutMilliseconds);
                }

                if (section.Channels.TCP != null)
                {
                    X509Certificate2 tcpCertificate = null;
                    Dictionary<string, byte[]> presharedKeys = null;
                    bool prefix = section.Channels.TCP.UseLengthPrefix;
                    bool authn = false;
                    int blockSize = section.Channels.TCP.BlockSize;
                    int maxBufferSize = section.Channels.TCP.MaxBufferSize;

                    if (section.Channels.TCP.PresharedKeys != null)
                    {
                        presharedKeys = section.Channels.TCP.PresharedKeys.GetPresharedKeys();
                    }
                    if (section.Channels.TCP.Certificate != null)
                    {
                        //get the certificate
                        authn = section.Channels.TCP.Certificate.AuthenticateServer;
                        tcpCertificate = GetCertificate(section.Channels.TCP.Certificate.Store, section.Channels.TCP.Certificate.Location, section.Channels.TCP.Certificate.Thumbprint);
                    }

                    tcp = new TcpSettings(prefix, blockSize, maxBufferSize, authn, tcpCertificate, presharedKeys);
                }

                channelSettings = new ChannelSettings(websocket, tcp);
            }

            CoapSettings coapSettings = null;
            if (section.Protocols != null && section.Protocols.Coap != null)
            {

                coapSettings = new CoapSettings(section.Protocols.Coap.HostName,
                                                section.Protocols.Coap.AutoRetry,
                                                section.Protocols.Coap.ObserveOption,
                                                section.Protocols.Coap.NoResponseOption,
                                                section.Protocols.Coap.KeepAliveSeconds,
                                                section.Protocols.Coap.AckTimeoutSeconds,
                                                section.Protocols.Coap.AckRandomFactor,
                                                section.Protocols.Coap.MaxRetransmit,
                                                section.Protocols.Coap.MaxLatencySeconds,
                                                section.Protocols.Coap.NStart,
                                                section.Protocols.Coap.DefaultLeisure,
                                                section.Protocols.Coap.ProbingRate);
            }

            MqttSettings mqttSettings = null;
            if (section.Protocols != null && section.Protocols.Mqtt != null)
            {
                mqttSettings = new MqttSettings(section.Protocols.Mqtt.KeepAliveSeconds,
                                        section.Protocols.Mqtt.AckTimeoutSeconds,
                                        section.Protocols.Mqtt.AckRandomFactor,
                                        section.Protocols.Mqtt.MaxRetransmit, section.Protocols.Mqtt.MaxLatencySeconds);
            }

            protocolSettings = new ProtocolSettings(mqttSettings, coapSettings);

            ClientIdentity clientIdentity = null;
            if (section.Identity != null && section.Identity.Client != null)
            {
                if (section.Identity.Client.Indexes != null)
                {
                    clientIdentity = new ClientIdentity(section.Identity.Client.IdentityClaimType, section.Identity.Client.Indexes.GetIndexes());
                }
                else
                {
                    clientIdentity = new ClientIdentity(section.Identity.Client.IdentityClaimType, null);
                }
            }

            ServiceIdentity serviceIdentity = null;

            if (section.Identity != null && section.Identity.Service != null && section.Identity.Service.Claims != null)
            {
                serviceIdentity = new ServiceIdentity(section.Identity.Service.Claims.GetServiceClaims());
            }


            identitySettings = new IdentitySettings(clientIdentity, serviceIdentity);

            ClientSecurity clientSecurity = null;
            if (section.Security != null && section.Security.Client != null)
            {
                if (section.Security.Client.SymmetricKey != null)
                {
                    clientSecurity = new ClientSecurity(section.Security.Client.SymmetricKey.SecurityTokenType,
                                            section.Security.Client.SymmetricKey.SharedKey,
                                            section.Security.Client.SymmetricKey.Issuer, section.Security.Client.SymmetricKey.Audience);
                }
            }

            ServiceSecurity serviceSecurity = null;
            if (section.Security.Service != null && section.Security.Service != null)
            {
                if (section.Security.Service.AsymmetricKey != null)
                {
                    X509Certificate2 serviceCertificate = GetCertificate(section.Security.Service.AsymmetricKey.Store, section.Security.Service.AsymmetricKey.Location, section.Security.Service.AsymmetricKey.Thumbprint);
                    serviceSecurity = new ServiceSecurity(serviceCertificate);
                }
            }

            securitySettings = new SecuritySettings(clientSecurity, serviceSecurity);

            config = new PiraeusConfig(channelSettings, protocolSettings, identitySettings, securitySettings);
        }

        private static void LoadEnvVars()
        {

            int maxIncomingMessageSize = Convert.ToInt32(System.Environment.GetEnvironmentVariable("WEBSOCKET_MAX_INCOMING_MESSAGE_SIZE") ?? "4194304");
            int receiveLoopBufferSize = Convert.ToInt32(System.Environment.GetEnvironmentVariable("WEBSOCKET_RECEIVE_LOOP_BUFFER_SIZE") ?? "8192");
            int sendBufferSize = Convert.ToInt32(System.Environment.GetEnvironmentVariable("WEBSOCKET_SEND_BUFFER_SIZE") ?? "8192");
            double closeTimeoutMilliseconds = Convert.ToDouble(System.Environment.GetEnvironmentVariable("WEBSOCKET_CLOSE_TIMEOUT_MILLISECONDS") ?? "250.0");

            Trace.TraceInformation("WEBSOCKET_MAX_INCOMING_MESSAGE_SIZE", maxIncomingMessageSize);
            Trace.TraceInformation("WEBSOCKET_RECEIVE_LOOP_BUFFER_SIZE", receiveLoopBufferSize);
            Trace.TraceInformation("WEBSOCKET_SEND_BUFFER_SIZE", sendBufferSize);
            Trace.TraceInformation("WEBSOCKET_CLOSE_TIMEOUT_MILLISECONDS",closeTimeoutMilliseconds);
           

            WebSocketSettings websocketSettings = new WebSocketSettings(maxIncomingMessageSize,
                                                                receiveLoopBufferSize,
                                                                sendBufferSize,
                                                                closeTimeoutMilliseconds);
            
            
            bool prefix = Convert.ToBoolean(System.Environment.GetEnvironmentVariable("TCP_USE_LENGTH_PREFIX") ?? "true");
            bool authn = Convert.ToBoolean(System.Environment.GetEnvironmentVariable("TCP_CERT_AUTHN") ?? "false");
            int blockSize = Convert.ToInt32(System.Environment.GetEnvironmentVariable("TCP_BLOCK_SIZE") ?? "2048");
            int maxBufferSize = Convert.ToInt32(System.Environment.GetEnvironmentVariable("TCP_MAX_BUFFER_SIZE") ?? (prefix ? "4194304": "512000"));

            Trace.TraceInformation("TCP_USE_LENGTH_PREFIX", prefix);
            Trace.TraceInformation("TCP_CERT_AUTHN", authn);
            Trace.TraceInformation("TCP_BLOCK_SIZE", blockSize);
            Trace.TraceInformation("TCP_MAX_BUFFER_SIZE", maxBufferSize);



            string certStore = System.Environment.GetEnvironmentVariable("TCP_CERT_STORE");
            string certLocation = System.Environment.GetEnvironmentVariable("TCP_CERT_LOCATION");
            string certThumb = System.Environment.GetEnvironmentVariable("TCP_CERT_THUMBPRINT");

            Trace.TraceInformation("TCP_CERT_STORE", certStore);
            Trace.TraceInformation("TCP_CERT_LOCATION", certLocation);
            Trace.TraceInformation("TCP_CERT_THUMBPRINT", certThumb);


            Dictionary<string, byte[]> presharedKeys = null;

            if(!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("TCP_PSK_IDENTITY")))
            {
                if(string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("TCP_PSK_KEY")))
                {
                    Trace.TraceError("TCP_PSK_KEY is null when TCP_PSK_IDENTITY is not.");
                    throw new ConfigurationErrorsException("TCP_PSK_KEY must be defined when TCP_PSK_IDENTITY is defined.");
                }

                presharedKeys = new Dictionary<string, byte[]>();
                string[] identityParts = System.Environment.GetEnvironmentVariable("TCP_PSK_IDENTITY").Split(new char[] { ';' });
                string[] keyParts = System.Environment.GetEnvironmentVariable("TCP_PSK_KEY").Split(new char[] { ';' });

                if(identityParts.Length != keyParts.Length)
                {
                    Trace.TraceError("TCP_PSK_KEY and TCP_PSK_IDENTITY lengths do not match.");
                    throw new ConfigurationErrorsException("Number of TCP_PSK_KEY items does not match number of TCP_PSK_KEY items.");
                }
                
                int index = 0;
                while(index < identityParts.Length)
                {
                    presharedKeys.Add(identityParts[index], Convert.FromBase64String(keyParts[index]));
                    index++;
                }
            }


            X509Certificate2 tcpCert = GetCertificate(certStore, certLocation, certThumb);
            TcpSettings tcpSettings = new TcpSettings(prefix, blockSize, maxBufferSize, authn, tcpCert, presharedKeys);

            ChannelSettings channelSettings = new ChannelSettings(websocketSettings, tcpSettings);
            

            string coapAuthority = System.Environment.GetEnvironmentVariable("COAP_HOSTNAME");
            bool coapAutoRetry = Convert.ToBoolean(System.Environment.GetEnvironmentVariable("COAP_AUTO_RETRY") ?? "false");
            bool observeOption = Convert.ToBoolean(System.Environment.GetEnvironmentVariable("COAP_OBSERVE_OPTION") ?? "true");
            bool noResponseOption = Convert.ToBoolean(System.Environment.GetEnvironmentVariable("COAP_NORESPONSE_OPTION") ?? "true");
            double coapKeepAlive = Convert.ToDouble(System.Environment.GetEnvironmentVariable("COAP_KEEP_ALIVE_SECONDS") ?? "180.0");
            double coapAckTimeout = Convert.ToDouble(System.Environment.GetEnvironmentVariable("COAP_ACK_TIMEOUT_SECONDS") ?? "2.0");
            double coapAckRandFactor = Convert.ToDouble(System.Environment.GetEnvironmentVariable("COAP_ACK_RANDOM_FACTOR") ?? "1.5");
            int coapMaxRetransmit = Convert.ToInt32(System.Environment.GetEnvironmentVariable("COAP_MAX_RETRANSMIT") ?? "4");
            double coapMaxLatency = Convert.ToDouble(System.Environment.GetEnvironmentVariable("COAP_MAX_LATENCY_SECONDS") ?? "100.0");
            int nstart = Convert.ToInt32(System.Environment.GetEnvironmentVariable("COAP_NSTART") ?? "1");
            double coapLeisure = Convert.ToDouble(System.Environment.GetEnvironmentVariable("COAP_DEFAULT_LEISURE") ?? "4.0");
            double probeRate = Convert.ToDouble(System.Environment.GetEnvironmentVariable("COAP_PROBING_RATE") ?? "1.0");

            CoapSettings coapSettings = new CoapSettings(coapAuthority, observeOption, 
                                                        noResponseOption, coapAutoRetry, 
                                                        coapKeepAlive, coapAckTimeout, 
                                                        coapAckRandFactor, coapMaxRetransmit, 
                                                        coapMaxLatency, nstart, coapLeisure, probeRate);

            

            double mqttKeepAlive = Convert.ToDouble(System.Environment.GetEnvironmentVariable("MQTT_KEEP_ALIVE_SECONDS") ?? "180.0");
            double mqttAckTimeout = Convert.ToDouble(System.Environment.GetEnvironmentVariable("MQTT_ACK_TIMEOUT_SECONDS") ?? "2.0");
            double mqttAckRandFactor = Convert.ToDouble(System.Environment.GetEnvironmentVariable("MQTT_ACK_RANDOM_FACTOR") ?? "1.5");
            int mqttMaxRetransmit = Convert.ToInt32(System.Environment.GetEnvironmentVariable("MQTT_MAX_RETRANSMIT") ?? "4");
            double mqttMaxLatency = Convert.ToDouble(System.Environment.GetEnvironmentVariable("MQTT_MAX_LATENCY_SECONDS") ?? "100.0");

            MqttSettings mqttSettings = new MqttSettings(mqttKeepAlive, mqttAckTimeout, 
                                                        mqttAckRandFactor, mqttMaxRetransmit, mqttMaxLatency);

            ProtocolSettings protocolSettings = new ProtocolSettings(mqttSettings, coapSettings);

            
            string identityClaimType = System.Environment.GetEnvironmentVariable("CLIENT_IDENTITY_NAME_CLAIM_TYPE");
            string[] identityClaimTypes = System.Environment.GetEnvironmentVariable("CLIENT_IDENTITY_INDEXES_CLAIM_TYPES") == null ? null : System.Environment.GetEnvironmentVariable("CLIENT_IDENTITY_INDEXES_CLAIM_TYPES").Split(new char[] { ';' });
            string[] identityIndexKeys = System.Environment.GetEnvironmentVariable("CLIENT_IDENTITY_INDEXES_CLAIM_INDEX_KEYS") == null ? null : System.Environment.GetEnvironmentVariable("CLIENT_IDENTITY_INDEXES_CLAIM_INDEX_KEYS").Split(new char[] { ':' });

            List<KeyValuePair<string, string>> indexes = null;

            if (identityClaimTypes != null && identityIndexKeys != null && identityClaimTypes.Length == identityIndexKeys.Length)
            {
                indexes = new List<KeyValuePair<string, string>>();
                int i = 0;
                while(i<identityIndexKeys.Length)
                {
                    indexes.Add(new KeyValuePair<string, string>(identityClaimTypes[i], identityIndexKeys[i]));
                    i++;
                }
            }

            ClientIdentity ci = new ClientIdentity(identityClaimType, indexes);

            string[] serviceClaimTypes = System.Environment.GetEnvironmentVariable("SERVICE_IDENTITY_CLAIM_TYPES") == null ? null : System.Environment.GetEnvironmentVariable("SERVICE_IDENTITY_CLAIM_TYPES").Split(new char[] { ';' });
            string[] serviceClaimValues = System.Environment.GetEnvironmentVariable("SERVICE_IDENTITY_CLAIM_VALUES") == null ? null : System.Environment.GetEnvironmentVariable("SERVICE_IDENTITY_CLAIM_VALUES").Split(new char[] { ';' });

            Trace.TraceInformation("SERVICE_IDENTITY_CLAIM_TYPES", System.Environment.GetEnvironmentVariable("SERVICE_IDENTITY_CLAIM_TYPES"));
            Trace.TraceInformation("SERVICE_IDENTITY_CLAIM_VALUES", System.Environment.GetEnvironmentVariable("SERVICE_IDENTITY_CLAIM_VALUES"));


            List<Claim> serviceClaims = null;
            if(serviceClaimTypes != null && serviceClaimValues != null && serviceClaimTypes.Length == serviceClaimValues.Length)
            {
                int j = 0;
                serviceClaims = new List<Claim>();
                while(j < serviceClaimTypes.Length)
                {
                    serviceClaims.Add(new Claim(serviceClaimTypes[j], serviceClaimValues[j]));
                    Trace.TraceWarning("Service Identity Claim Type {0}", serviceClaimTypes[j]);
                    Trace.TraceWarning("Service Identity Claim Value {0}", serviceClaimValues[j]);
                    j++;
                }
            }

            

            ServiceIdentity si = new ServiceIdentity(serviceClaims);

            IdentitySettings identitySettings = new IdentitySettings(ci, si);
            

            string tokenType = System.Environment.GetEnvironmentVariable("CLIENT_SECURITY_TOKEN_TYPE");
            string symmetricKey = System.Environment.GetEnvironmentVariable("CLIENT_SECURITY_SYMMETRIC_KEY");
            string issuer = System.Environment.GetEnvironmentVariable("CLIENT_SECURITY_ISSUER");
            string audience = System.Environment.GetEnvironmentVariable("CLIENT_SECURITY_AUDIENCE");

            ClientSecurity clientSecurity = new ClientSecurity(tokenType, symmetricKey, issuer, audience);

            
            string store = System.Environment.GetEnvironmentVariable("SERVICE_SECURITY_CERT_STORE");
            string location = System.Environment.GetEnvironmentVariable("SERVICE_SECURITY_CERT_LOCATION");
            string thumbprint = System.Environment.GetEnvironmentVariable("SERVICE_SECURITY_CERT_THUMBPRINT");

            ServiceSecurity serviceSecurity = new ServiceSecurity(GetCertificate(store, location, thumbprint));

            SecuritySettings securitySettings = new SecuritySettings(clientSecurity, serviceSecurity);

            config = new PiraeusConfig(channelSettings, protocolSettings, identitySettings, securitySettings);
            

        }


    }
}

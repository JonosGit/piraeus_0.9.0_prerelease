//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Piraeus.Configuration
//{
//    public class FakeConfig
//    {
//        public FakeConfig()
//        {
//            System.Environment.SetEnvironmentVariable("ORLEANS_PROVIDER_DATACONNECTIONSTRING", null, EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("ORLEANS_LIVENESS_DATACONNECTIONSTRING", null, EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("ORLEANS_AUDIT_DATACONNECTIONSTRING", null, EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("ORLEANS_DEPLOYMENT_ID", null, EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("ORLEANS_STORAGE_PROVIDER_TYPE", null, EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("ORLEANS_STORAGE_CONTAINER_NAME", null, EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("ORLEANS_MAXMEMORY_STORAGE_GRAINS", null, EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("ORLEANS_DNS_HOSTNAME", null, EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("ORLEANS_AUDIT_TABLENAME", null, EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("SERVICEPOINT_CORE_FACTOR", "24", EnvironmentVariableTarget.Process);

//            System.Environment.SetEnvironmentVariable("MGMT_API_AUDIENCE", "http://www.skunklab.io/", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("MGMT_API_ISSUER", "http://www.skunklab.io/", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("MGMT_API_SYMMETRICKEY", "http://www.skunklab.io/", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("MGMT_API_NAME_CLAIM_TYPE", "http://www.skunklab.io/piraeus/name", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("MGMT_API_ROLE_CLAIM_TYPE", "http://www.skunklab.io/piraeus/role", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("MGMT_API_ROLE_CLAIM_VALUE", "manage", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("MGMT_API_SECURITY_CODE", "12345678", EnvironmentVariableTarget.Process);

//            System.Environment.SetEnvironmentVariable("GATEWAY_ORLEANS_SILO_DNS_HOSTNAME", "orleans-silo", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("GATEWAY_TCP_SERVER_DNS_HOSTNAME", "tcp-server", EnvironmentVariableTarget.Process);

//            System.Environment.SetEnvironmentVariable("WEBSOCKET_MAX_INCOMING_MESSAGE_SIZE", "4194304", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("WEBSOCKET_RECEIVE_LOOP_BUFFER_SIZE", "8192", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("WEBSOCKET_SEND_BUFFER_SIZE", "8192", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("WEBSOCKET_CLOSE_TIMEOUT_MILLISECONDS", "250", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("USER_AUDIT_TABLENAME", null, EnvironmentVariableTarget.Process);

//            System.Environment.SetEnvironmentVariable("TCP_USE_LENGTH_PREFIX", "True", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("TCP_BLOCK_SIZE", "2048", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("TCP_MAX_BUFFER_SIZE", "1024000", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("TCP_PSK_IDENTITY", "Key1;Key2", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("TCP_PSK_KEY", "VGhlIHF1aWNrIGJyb3duIGZveA==;anVtcHMgb3ZlciB0aGUgbGF6eSBkb2cu", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("TCP_CERT_AUTHN", "False", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("TCP_CERT_STORE", null, EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("TCP_CERT_LOCATION", null, EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("TCP_CERT_THUMBPRINT", null, EnvironmentVariableTarget.Process);

//            System.Environment.SetEnvironmentVariable("MQTT_KEEP_ALIVE_SECONDS", "180.0", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("MQTT_ACK_RANDOM_FACTOR", "15.", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("MQTT_ACK_TIMEOUT_SECONDS", "2.0", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("MQTT_MAX_RETRANSMIT", "4", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("MQTT_MAX_LATENCY_SECONDS", "100.0", EnvironmentVariableTarget.Process);

//            System.Environment.SetEnvironmentVariable("COAP_HOSTNAME", "www.skunklab.io", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("COAP_OBSERVE_OPTION", "True", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("COAP_NORESPONSE_OPTION", "True", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("COAP_AUTO_RETRY", "False", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("COAP_NSTART", "1", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("COAP_DEFAULT_LEISURE", "4.0", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("COAP_PROBING_RATE", "1.0", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("COAP_KEEP_ALIVE_SECONDS", "180", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("COAP_ACK_TIMEOUT_SECONDS", "2.0", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("COAP_ACK_RANDOM_FACTOR", "1,5", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("COAP_MAX_RETRANSMIT", "4", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("COAP_MAX_LATENCY_SECONDS", "100.0", EnvironmentVariableTarget.Process);


//            System.Environment.SetEnvironmentVariable("CLIENT_IDENTITY_NAME_CLAIM_TYPE", "http://www.skunklab.io/name", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("CLIENT_IDENTITY_INDEXES_CLAIM_TYPES", "http://www.skunklab.io/role;http://www.skunklab.io/index", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("CLIENT_IDENTITY_INDEXES_CLAIM_INDEX_KEYS", "key1;key2", EnvironmentVariableTarget.Process);


//            System.Environment.SetEnvironmentVariable("SERVICE_IDENTITY_CLAIM_TYPES", "http://www.piraeus.org/name;http://www.piraeus.org/role", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("SERVICE_IDENTITY_CLAIM_VALUES", "piraeus;service", EnvironmentVariableTarget.Process);


//            System.Environment.SetEnvironmentVariable("CLIENT_SECURITY_TOKEN_TYPE", "JWT", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("CLIENT_SECURITY_SYMMETRIC_KEY", "SJoPNjLKFR4j1tD5B4xhJStujdvVukWz39DIY3i8abE=", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("CLIENT_SECURITY_ISSUER", "http://www.skunklab.io/", EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("CLIENT_SECURITY_AUDIENCE", "http://www.skunklab.io/", EnvironmentVariableTarget.Process);


//            System.Environment.SetEnvironmentVariable("SERVICE_SECURITY_CERT_STORE", null, EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("SERVICE_SECURITY_CERT_LOCATION", null, EnvironmentVariableTarget.Process);
//            System.Environment.SetEnvironmentVariable("SERVICE_SECURITY_CERT_THUMBPRINT", null, EnvironmentVariableTarget.Process);
//        }


//    }
//}

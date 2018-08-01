using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Capl.Authorization;
using Piraeus.Grains;
using WebGateway.Security;

namespace WebGateway.Controllers
{
    public class AccessControlController : ApiController
    {
        public AccessControlController()
        {
            try
            {
                if (!Orleans.GrainClient.IsInitialized)
                {
                    bool dockerized = Convert.ToBoolean(ConfigurationManager.AppSettings["dockerize"]);
                    if (!dockerized)
                    {
                        OrleansClientConfig.TryStart("AccessControlController");
                    }
                    else
                    {
                        string hostname = System.Environment.GetEnvironmentVariable("GATEWAY_ORLEANS_SILO_DNS_HOSTNAME"); //ConfigurationManager.AppSettings["dnsHostEntry"];
                        OrleansClientConfig.TryStart("AccessControlController", hostname);
                    }

                    Trace.TraceInformation("Orleans grain client initialized {0} is access control controller", Orleans.GrainClient.IsInitialized);
                    Task task = ServiceIdentityConfig.Configure();
                    Task.WhenAll(task);
                }


                bool started = OrleansClientConfig.TryStart("AccessControlController");
                Console.WriteLine("Orleans client started {0} on Web gateway.", started);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Orleans client on Web gateway failed with error {0}", ex.Message);
                Console.WriteLine(ex.StackTrace);
                Trace.TraceError("Orleans client error starting on Web gateway {0}", ex.Message);                
            }
        }

        //[CaplAuthorize(PolicyId = "http://www.skunklab.io/api/management")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetAccessControlPolicy(string policyUriString)
        {
            try
            {
                AuthorizationPolicy policy = await GraphManager.GetAccessControlPolicyAsync(policyUriString);
                return Request.CreateResponse<AuthorizationPolicy>(HttpStatusCode.OK, policy, "application/xml");
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Failed to get access control policy");
                Trace.TraceError(ex.Message);
                Trace.TraceError(ex.StackTrace);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        //[CaplAuthorize(PolicyId = "http://www.skunklab.io/api/management")]
        [HttpPut]
        public async Task<HttpResponseMessage> UpsertAccessControlPolicy(AuthorizationPolicy policy)
        {
            try
            {
                await GraphManager.UpsertAcessControlPolicyAsync(policy.PolicyId.ToString(), policy);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Failed to upsert access control policy");
                Trace.TraceError(ex.Message);
                Trace.TraceError(ex.StackTrace);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        //[CaplAuthorize(PolicyId = "http://www.skunklab.io/api/management")]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteAccessControlPolicy(string policyUriString)
        {
            try
            {
                await GraphManager.ClearAccessControlPolicyAsync(policyUriString);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Failed to delete access control policy");
                Trace.TraceError(ex.Message);
                Trace.TraceError(ex.StackTrace);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}

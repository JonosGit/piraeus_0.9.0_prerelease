﻿using Piraeus.Core.Metadata;
using System;
using System.Management.Automation;

namespace Piraeus.Module
{
    [Cmdlet(VerbsCommon.Add, "PiraeusQueueStorageSubscription")]
    public class AddAzureQueueStorageSubscriptionCmdlet : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security token used to access the REST service.", Mandatory = true)]
        public string SecurityToken;

        [Parameter(HelpMessage = "Unique URI identifier of resource to subscribe.", Mandatory = true)]
        public string ResourceUriString;

        [Parameter(HelpMessage = "Host name of Azure Queue Storage, e.g, <host>.queue.core.windows.net", Mandatory = true)]
        public string Host;

        [Parameter(HelpMessage = "Name of queue to write messages.", Mandatory = true)]
        public string Queue;

        [Parameter(HelpMessage = "Optional TTL for messages to remain in queue.", Mandatory = false)]
        public TimeSpan? TTL;

        [Parameter(HelpMessage = "Either storage key or SAS token for account or queue.", Mandatory = true)]
        public string Key;

        protected override void ProcessRecord()
        {
            string uriString = TTL.HasValue ? String.Format("https://{0}.queue.core.windows.net?queue={1}&ttl={2}", Host, Queue, TTL.Value.ToString()) :
                String.Format("https://{0}.queue.core.windows.net?queue={1}", Host, Queue);
            
            SubscriptionMetadata metadata = new SubscriptionMetadata()
            {
                IsEphemeral = false,
                NotifyAddress = uriString,
                SymmetricKey = Key
            };

            string url = String.Format("{0}/api2/resource/subscribe?resourceuristring={1}", ServiceUrl, ResourceUriString);
            RestRequestBuilder builder = new RestRequestBuilder("POST", url, RestConstants.ContentType.Json, false, SecurityToken);
            RestRequest request = new RestRequest(builder);

            string subscriptionUriString = request.Post<SubscriptionMetadata, string>(metadata);

            WriteObject(subscriptionUriString);
        }
    }
}

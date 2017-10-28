﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using Piraeus.GrainInterfaces;
using System.Linq;

namespace Piraeus.Grains
{
    public class Subscription : Grain<SubscriptionState>, ISubscription
    {
        [NonSerialized]
        private Queue<EventMessage> memoryMessageQueue;

        [NonSerialized]
        IDisposable leaseTimer;

        [NonSerialized]
        IDisposable messageQueueTimer;

        #region Activatio/Deactivation

        public override Task OnActivateAsync()
        {
            if(State.ErrorLeases == null)
            {
                State.ErrorLeases = new Dictionary<string, IErrorObserver>();
            }

            if(State.LeaseExpiry == null)
            {
                State.LeaseExpiry = new Dictionary<string, Tuple<DateTime, string>>();
            }

            if(State.MessageLeases == null)
            {
                State.MessageLeases = new Dictionary<string, IMessageObserver>();
            }

            if(State.MessageQueue == null)
            {
                State.MessageQueue = new Queue<EventMessage>();
            }

            memoryMessageQueue = new Queue<EventMessage>();

            if (State.MetricLeases == null)
            {
                State.MetricLeases = new Dictionary<string, IMetricObserver>();
            }

            Task task = DequeueAsync(State.MessageQueue);
            Task.WhenAll(task);

            return Task.CompletedTask;
        }

        public override async Task OnDeactivateAsync()
        {
            await WriteStateAsync();
        }
        #endregion


        #region ID
        public async Task<string> GetIdAsync()
        {
            if(State.Metadata == null)
            {
                return null;
            }

            return await Task.FromResult<string>(State.Metadata.SubscriptionUriString);
        }

        #endregion

        #region Clear
        public async Task ClearAsync()
        {
            await ClearStateAsync();
        }
        #endregion

        #region Metadata

        public async Task UpsertMetadataAsync(SubscriptionMetadata metadata)
        {
            State.Metadata = metadata;
            await Task.CompletedTask;
        }

        public async Task<SubscriptionMetadata> GetMetadataAsync()
        {
            return await Task.FromResult<SubscriptionMetadata>(State.Metadata);
        }

        #endregion

        #region Notification

        public async Task NotifyAsync(EventMessage message)
        {
            Exception error = null;

            State.MessageCount++;
            State.ByteCount += message.Message.LongLength;
            State.LastMessageTimestamp = DateTime.UtcNow;

            try
            {
                if (!string.IsNullOrEmpty(State.Metadata.NotifyAddress))
                {
                    //send to passively connected subsystem
                }
                else if (State.MessageLeases.Count > 0)
                {
                    //send to actively connected subsystem
                    foreach (var observer in State.MessageLeases.Values)
                    {
                        observer.Notify(message);
                    }
                }
                else if (State.Metadata.DurableMessaging && State.Metadata.TTL.HasValue) //durable message queue
                {
                    await QueueDurableMessageAsync(message);
                }
                else //in-memory message queue
                {
                    await QueueInMemoryMessageAsync(message);
                }
            }
            catch(Exception ex)
            {
                error = ex;
                GetLogger().Log(2, Orleans.Runtime.Severity.Error, "Subscription notification exception {0}", new object[] { State.Metadata.SubscriptionUriString }, ex);                
            }

            await NotifyMetricsAsync();

            if(error != null)
            {
                await NotifyErrorAsync(error);
            }
        }

        public async Task NotifyAsync(EventMessage message, List<KeyValuePair<string, string>> indexes)
        {
            if (indexes == null)
            {
                await NotifyAsync(message);
                return;
            }

            var query = indexes.Where((c) => State.Metadata.Indexes.Contains(new KeyValuePair<string, string>(c.Key, c.Value)));

            if (indexes.Count == query.Count())
            {
                await NotifyAsync(message);
            }
            else
            {
                State.MessageCount++;
                State.ByteCount += message.Message.LongLength;
                State.LastMessageTimestamp = DateTime.UtcNow;                
            }
        }

        #endregion

        #region Observers

        public async Task<string> AddObserverAsync(TimeSpan lifetime, IMessageObserver observer)
        {
            if (observer == null)
            {
                Exception ex = new ArgumentNullException("subscription message observer");
                await NotifyErrorAsync(ex);
                return await Task.FromResult<string>(null);
            }

            string leaseKey = Guid.NewGuid().ToString();
            State.MessageLeases.Add(leaseKey, observer);
            State.LeaseExpiry.Add(leaseKey, new Tuple<DateTime, string>(DateTime.UtcNow.Add(lifetime), "Message"));

            if (leaseTimer == null)
            {
                leaseTimer = RegisterTimer(CheckLeaseExpiryAsync, null, TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(10.0));
            }

            return await Task.FromResult<string>(leaseKey);
        }
        public async Task<string> AddObserverAsync(TimeSpan lifetime, IMetricObserver observer)
        {
            if (observer == null)
            {
                Exception ex = new ArgumentNullException("subscription metric observer");
                await NotifyErrorAsync(ex);
                return await Task.FromResult<string>(null);
            }

            string leaseKey = Guid.NewGuid().ToString();
            State.MetricLeases.Add(leaseKey, observer);
            State.LeaseExpiry.Add(leaseKey, new Tuple<DateTime, string>(DateTime.UtcNow.Add(lifetime), "Metric"));

            if (leaseTimer == null)
            {
                leaseTimer = RegisterTimer(CheckLeaseExpiryAsync, null, TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(10.0));
            }

            return await Task.FromResult<string>(leaseKey);

        }
        public async Task<string> AddObserverAsync(TimeSpan lifetime, IErrorObserver observer)
        {
            if(observer == null)
            {
                Exception ex = new ArgumentNullException("subscription error observer");
                await NotifyErrorAsync(ex);
                return await Task.FromResult<string>(null);
            }

            string leaseKey = Guid.NewGuid().ToString();
            State.ErrorLeases.Add(leaseKey, observer);
            State.LeaseExpiry.Add(leaseKey, new Tuple<DateTime, string>(DateTime.UtcNow.Add(lifetime), "Error"));

            if (leaseTimer == null)
            {
                leaseTimer = RegisterTimer(CheckLeaseExpiryAsync, null, TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(10.0));
            }

            return await Task.FromResult<string>(leaseKey);
        }
        public async Task RemoveObserverAsync(string leaseKey)
        {
            var messageQuery = State.LeaseExpiry.Where((c) => c.Key == leaseKey && c.Value.Item2 == "Message");
            var metricQuery = State.LeaseExpiry.Where((c) => c.Key == leaseKey && c.Value.Item2 == "Metric");
            var errorQuery = State.LeaseExpiry.Where((c) => c.Key == leaseKey && c.Value.Item2 == "Error");

            State.LeaseExpiry.Remove(leaseKey);

            if (messageQuery.Count() == 1)
            {
                State.MessageLeases.Remove(leaseKey);

                if (State.MessageLeases.Count == 0 && State.Metadata.IsEphemeral)
                {
                    await UnsubscribeFromResourceAsync();
                }
            }

            if (metricQuery.Count() == 1)
            {
                State.MetricLeases.Remove(leaseKey);
            }

            if (errorQuery.Count() == 1)
            {
                State.ErrorLeases.Remove(leaseKey);
            }

            await Task.CompletedTask;
        }

        public async Task<bool> RenewObserverLeaseAsync(string leaseKey, TimeSpan lifetime)
        {
            if (State.LeaseExpiry.ContainsKey(leaseKey))
            {
                Tuple<DateTime, string> value = State.LeaseExpiry[leaseKey];
                Tuple<DateTime, string> newValue = new Tuple<DateTime, string>(DateTime.UtcNow.Add(lifetime), value.Item2);
                State.LeaseExpiry[leaseKey] = newValue;
                return await Task.FromResult<bool>(true);
            }

            return await Task.FromResult<bool>(false);
        }

        #endregion


        #region private methods

        private async Task UnsubscribeFromResourceAsync()
        {
            //unsubscribe from resource
            string uriString = State.Metadata.SubscriptionUriString;
            Uri uri = new Uri(uriString);

            string resourceUriString = uriString.Replace("/" + uri.Segments[uri.Segments.Length - 1], "");
            IResource resource = GrainFactory.GetGrain<IResource>(resourceUriString);
            await resource.UnsubscribeAsync(State.Metadata.SubscriptionUriString);
        }

        private async Task NotifyErrorAsync(Exception ex)
        {
            if(State.ErrorLeases.Count == 0)
            {
                return;
            }

            foreach (var item in State.ErrorLeases.Values)
            {
                item.NotifyError(State.Metadata.SubscriptionUriString, ex);
            }
            await Task.CompletedTask;
        }

        private async Task NotifyMetricsAsync()
        {
            if(State.MetricLeases.Count == 0)
            {
                return;
            }

            foreach (var item in State.MetricLeases.Values)
            {
                item.NotifyMetrics(new CommunicationMetrics(State.Metadata.SubscriptionUriString, State.MessageCount, State.ByteCount, State.ErrorCount, State.LastMessageTimestamp.Value, State.LastErrorTimestamp));
            }

            await Task.CompletedTask;
        }

        private async Task CheckLeaseExpiryAsync(object args)
        {
            var messageQuery = State.LeaseExpiry.Where((c) => c.Value.Item1 < DateTime.UtcNow && c.Value.Item2 == "Message");
            var metricQuery = State.LeaseExpiry.Where((c) => c.Value.Item1 < DateTime.UtcNow && c.Value.Item2 == "Metric");
            var errorQuery = State.LeaseExpiry.Where((c) => c.Value.Item1 < DateTime.UtcNow && c.Value.Item2 == "Error");

            List<string> messageLeaseKeyList = new List<string>(messageQuery.Select((c) => c.Key));
            List<string> metricLeaseKeyList = new List<string>(metricQuery.Select((c) => c.Key));
            List<string> errorLeaseKeyList = new List<string>(errorQuery.Select((c) => c.Key));

            foreach (var item in messageLeaseKeyList)
            {
                State.MessageLeases.Remove(item);
                State.LeaseExpiry.Remove(item);

                if (State.MessageLeases.Count == 0 && State.Metadata.IsEphemeral)
                {
                    await UnsubscribeFromResourceAsync();
                }
            }

            foreach (var item in metricLeaseKeyList)
            {
                State.MetricLeases.Remove(item);
                State.LeaseExpiry.Remove(item);
            }

            foreach (var item in errorLeaseKeyList)
            {
                State.ErrorLeases.Remove(item);
                State.LeaseExpiry.Remove(item);
            }

            if (State.LeaseExpiry.Count == 0)
            {
                leaseTimer.Dispose();
            }            
        }
        private async Task QueueDurableMessageAsync(EventMessage message)
        {            
            if(State.MessageQueue.Count > 0)
            {
                //remove expired messages
                while(State.MessageQueue.Peek().Timestamp.Add(State.Metadata.TTL.Value) < DateTime.UtcNow)
                {
                    State.MessageQueue.Dequeue();
                }
            }

            //add the new message
            State.MessageQueue.Enqueue(message);
            
            //start the timer if not already started
            if (messageQueueTimer == null)
            {               
                messageQueueTimer = RegisterTimer(CheckQueueAsync, null, TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(10.0));
            }

            await Task.CompletedTask;
        }

        private async Task QueueInMemoryMessageAsync(EventMessage message)
        {
            memoryMessageQueue.Enqueue(message);

            if (messageQueueTimer == null)
            {
                messageQueueTimer = RegisterTimer(CheckQueueAsync, null, TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(5.0));
            }

            DelayDeactivation(TimeSpan.FromSeconds(20.0));

            await Task.CompletedTask;
        }

        private async Task CheckQueueAsync(object args)
        {
            //timer firing for queued messages
            if (State.MessageLeases.Count > 0)
            {
                if (memoryMessageQueue != null)
                {
                    await DequeueAsync(memoryMessageQueue);

                    if (memoryMessageQueue.Count > 0)
                    {
                        DelayDeactivation(State.Metadata.TTL.Value);
                    }
                }

                if (State.MessageQueue != null)
                {
                    await DequeueAsync(State.MessageQueue);
                }                
            }
        }


        private async Task DequeueAsync(Queue<EventMessage> queue)
        {
            while (queue.Count > 0)
            {
                EventMessage msg = queue.Dequeue();
                if (msg.Timestamp.Add(State.Metadata.TTL.Value) > DateTime.UtcNow)
                {
                    await NotifyAsync(msg);

                    if (State.Metadata.SpoolRate.HasValue)
                    {
                        await Task.Delay(State.Metadata.SpoolRate.Value);
                    }
                }
                else
                {
                    queue.Dequeue();
                }
            }

            if (messageQueueTimer != null)
            {
                messageQueueTimer.Dispose();
            }
        }
        #endregion

    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piraeus.Core.Messaging
{
    [Serializable]
    public class EventMessage
    {
        public EventMessage()
        {
        }

        public EventMessage(string contentType, Uri resourceUri, ProtocolType protocol, byte[] message)
            : this(Guid.NewGuid().ToString(), contentType, resourceUri.ToString(), protocol, message, DateTime.UtcNow)
        {

        }

        public EventMessage(string contentType, string resourceUri, ProtocolType protocol, byte[] message)
            : this(Guid.NewGuid().ToString(), contentType, resourceUri, protocol, message, DateTime.UtcNow)
        {

        }

        public EventMessage(string contentType, Uri resourceUri, ProtocolType protocol, byte[] message, DateTime timestamp, bool audit = false)
            : this(Guid.NewGuid().ToString(), contentType, resourceUri.ToString(), protocol, message, timestamp, audit)
        {

        }

        public EventMessage(string contentType, string resourceUri, ProtocolType protocol, byte[] message, DateTime timestamp, bool audit = false)
            : this(Guid.NewGuid().ToString(), contentType, resourceUri, protocol, message, timestamp, audit)
        {

        }

        public EventMessage(string messageId, string contentType, Uri resourceUri, ProtocolType protocol, byte[] message, bool audit = false)
            : this(messageId, contentType, resourceUri.ToString(), protocol, message, DateTime.UtcNow, audit)
        {

        }

        public EventMessage(string messageId, string contentType, string resourceUri, ProtocolType protocol, byte[] message, bool audit = false)
            : this(messageId, contentType, resourceUri, protocol, message, DateTime.UtcNow, audit)
        {

        }

        public EventMessage(string messageId, string contentType, Uri resourceUri, ProtocolType protocol, byte[] message, DateTime timeStamp, bool audit = false)
            : this(messageId, contentType, resourceUri.ToString(), protocol, message, timeStamp, audit)
        {

        }

        public EventMessage(string messageId, string contentType, string resourceUri, ProtocolType protocol, byte[] message, DateTime timeStamp, bool audit = false)
        {
            this.MessageId = messageId;
            this.ContentType = contentType;
            this.ResourceUri = resourceUri;
            this.Protocol = protocol;
            this.Message = message;
            this.Timestamp = timeStamp;
        }
        /// <summary>
        /// A unique message id for the event message.
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// Gets or sets the content type of the message.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the resource to be accessed.
        /// </summary>
        public string ResourceUri { get; set; }

        /// <summary>
        /// Gets or set the event message.
        /// </summary>
        public byte[] Message { get; set; }

        /// <summary>
        /// Gets or sets the protocol that generated the event message.
        /// </summary>
        public ProtocolType Protocol { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the event message.
        /// </summary>
        public DateTime Timestamp { get; set; }

        public bool Audit { get; set; }
    }
}

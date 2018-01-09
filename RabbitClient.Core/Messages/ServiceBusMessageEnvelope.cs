using System;
using Newtonsoft.Json;

namespace RabbitClient.Core.Messages
{
    public class ServiceBusMessageEnvelope : BusMessage
    {
        public ServiceBusMessageEnvelope()
        {
        }

        [JsonConstructor]
        public ServiceBusMessageEnvelope(Guid messageId, ServiceBusMessageProperties properties, Type messageType,
            string jsonMessage, int retryCount)
        {
            MessageId = messageId;
            Properties = properties;
            MessageType = messageType;
            JsonMessage = jsonMessage;
            RetryCount = retryCount;
        }

        public Type MessageType { get; set; }
        public string JsonMessage { get; set; }
        public ServiceBusMessageProperties Properties { get; set; }
        public int RetryCount { get; set; }
    }
}
using System;
using Newtonsoft.Json;

namespace Diciannove.ServiceBus
{
    [Serializable]
    public class ServiceBusClientException : Exception
    {
        public string OriginalMessage { get; internal set; }
        public string ExchangeName { get; internal set; }
        public string QueueName { get; internal set; }

        [JsonConstructor]
        public ServiceBusClientException(string message, string originalMessage, string exchangeName, string queueName, Exception innerException)
            : base(message, innerException)
        {
            OriginalMessage = originalMessage;
            ExchangeName = exchangeName;
            QueueName = queueName;
        }
    }
}
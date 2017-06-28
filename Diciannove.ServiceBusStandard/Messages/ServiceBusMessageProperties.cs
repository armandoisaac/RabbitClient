using System;
using Newtonsoft.Json;

namespace Diciannove.ServiceBus.Messages
{
    public class ServiceBusMessageProperties
    {
        public string ExchangeName { get;internal set; }
        public string QueueName { get; internal set; }
        public DateTime DatePublished { get; internal set; }
        public string ClientName { get; internal set; }

        public ServiceBusMessageProperties() { }

        [JsonConstructor]
        public ServiceBusMessageProperties(string exchangeName, string queueName, DateTime datePublished, string clientName)
        {
            ExchangeName = exchangeName;
            QueueName = queueName;
            DatePublished = datePublished;
            ClientName = clientName;
        }
    }
}
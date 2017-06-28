using System;
using System.Collections.Generic;

namespace Diciannove.ServiceBus.Configuration
{
    public class ServiceBusConfiguration<T> where T : ConnectionConfiguration, new()
    {
        public readonly List<QueueConfiguration> QueueHandlers;

        public ServiceBusConfiguration()
        {
            QueueHandlers = new List<QueueConfiguration>();
            ClientName = Environment.MachineName;
            DefaultConcurrencyLimit = 5;
            ConnectionConfiguration = new T();
            ExceptionHandler = e => { };
        }

        public T ConnectionConfiguration { get; set; }

        public ushort DefaultConcurrencyLimit { get; set; }

        public string ClientName { get; set; }

        public Action<ServiceBusClientException> ExceptionHandler { get; set; }
    }
}
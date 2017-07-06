using System;
using System.Collections.Generic;

namespace Diciannove.ServiceBus.Configuration
{
    public class ServiceBusConfiguration : ServiceBusConfiguration<ConnectionConfiguration, QueueConfiguration>
    {
    }

    public class ServiceBusConfiguration<TConnectionConfiguration, TQueueConfiguration>
        where TConnectionConfiguration : ConnectionConfiguration, new()
        where TQueueConfiguration : QueueConfiguration, new()
    {
        public readonly List<TQueueConfiguration> QueueHandlers;

        public ServiceBusConfiguration()
        {
            QueueHandlers = new List<TQueueConfiguration>();
            ClientName = Environment.MachineName;
            DefaultConcurrencyLimit = 5;
            ConnectionConfiguration = new TConnectionConfiguration();
            ExceptionHandler = e => { };
        }

        public TConnectionConfiguration ConnectionConfiguration { get; set; }

        public ushort DefaultConcurrencyLimit { get; set; }

        public string ClientName { get; set; }

        public Action<ServiceBusClientException> ExceptionHandler { get; set; }
    }
}
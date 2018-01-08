using System;

namespace Diciannove.ServiceBus.Configuration
{
    public abstract class ServiceBusConfiguration
    {
        internal ServiceBusConfiguration() { }

        public ushort DefaultConcurrencyLimit { get; set; }

        public string ClientName { get; set; }

        public Action<ServiceBusClientException> ExceptionHandler { get; set; }
    }
}
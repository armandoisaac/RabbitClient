using System;
using System.Collections.Generic;
using Diciannove.ServiceBus.Configuration;
using RabbitClient.Comparers;

namespace RabbitClient.Configuration
{
    public class RabbitMqServiceConfiguration : ServiceBusConfiguration
    {
        internal readonly HashSet<RabbitMqExchange> CustomExchanges;
        internal readonly HashSet<RabbitMqQueueConfiguration> QueueHandlers;
        internal RabbitMqConnectionConfiguration ConnectionConfiguration { get; set; }


        internal RabbitMqServiceConfiguration()
        {
            CustomExchanges = new HashSet<RabbitMqExchange>(new ExchangeComparer());
            QueueHandlers = new HashSet<RabbitMqQueueConfiguration>(new QueueComparer());
            ClientName = Environment.MachineName;
            DefaultConcurrencyLimit = 5;
            ConnectionConfiguration = new RabbitMqConnectionConfiguration();
            ExceptionHandler = e => { };
        }
    }
}
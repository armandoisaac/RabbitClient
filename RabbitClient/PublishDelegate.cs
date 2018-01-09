using System;
using RabbitClient.Core.Messages;

namespace RabbitClient
{
    internal delegate void PublishDelegate(string exchange, string queueName, IBusMessage message, TimeSpan? ttl);
}
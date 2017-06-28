using Diciannove.ServiceBus.Messages;

namespace Diciannove.ServiceBus.RabbitMq
{
    internal delegate void PublishDelegate(string exchange, string queueName, IBusMessage message);
}
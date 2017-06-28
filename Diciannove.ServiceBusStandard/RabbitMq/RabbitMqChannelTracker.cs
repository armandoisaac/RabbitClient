using RabbitMQ.Client;

namespace Diciannove.ServiceBus.RabbitMq
{
    internal class RabbitMqChannelTracker
    {
        public readonly IModel Channel;
        public int Consumers;

        public RabbitMqChannelTracker(IModel model)
        {
            Channel = model;
            Consumers = 0;
        }
    }
}
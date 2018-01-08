using RabbitMQ.Client;

namespace RabbitClient
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
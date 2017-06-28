using System;
using System.Collections.Concurrent;
using RabbitMQ.Client;

namespace Diciannove.ServiceBus.RabbitMq
{
    internal class RabbitMqChannelFactory : IDisposable
    {
        private static RabbitMqChannelFactory _instance;
        private readonly ConcurrentDictionary<string, RabbitMqChannelTracker> _channels;
        private readonly RabbitMqConnection _connection;

        private RabbitMqChannelFactory(RabbitMqConnection connection)
        {
            _channels = new ConcurrentDictionary<string, RabbitMqChannelTracker>();
            _connection = connection;
        }

        public void Dispose()
        {
            foreach (var channel in _channels)
            {
                if (channel.Value.Channel.IsOpen)
                    channel.Value.Channel.Close();

                channel.Value.Channel.Dispose();
            }
        }

        public static RabbitMqChannelFactory GetInstance(RabbitMqConnection connection)
        {
            if (_instance == null)
                _instance = new RabbitMqChannelFactory(connection);

            return _instance;
        }

        public IModel GetChannel(string queueName)
        {
            if (!_channels.ContainsKey(queueName))
            {
                var channel = _connection.Connection.CreateModel();
                channel.ModelShutdown += (m, args) =>
                {

                };
                _channels.TryAdd(queueName, new RabbitMqChannelTracker(_connection.Connection.CreateModel()));
            }

            _channels[queueName].Consumers++;
            return _channels[queueName].Channel;
        }

        public void UnRegister(string queueName)
        {
            if (!_channels.ContainsKey(queueName)) return;
            _channels[queueName].Consumers--;

            // Remove channel if no consumers
            if (_channels[queueName].Consumers == 0)
            {
                var channel = _channels[queueName].Channel;
                if (channel.IsOpen)
                    channel.Close();
                channel.Dispose();

                RabbitMqChannelTracker value;
                _channels.TryRemove(queueName, out value);
            }
        }
    }
}
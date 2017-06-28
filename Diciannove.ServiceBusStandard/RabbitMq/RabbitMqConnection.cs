using System;
using System.Collections.Generic;
using Diciannove.ServiceBus.Configuration;
using RabbitMQ.Client;

namespace Diciannove.ServiceBus.RabbitMq
{
    internal class RabbitMqConnection
    {
        private readonly ConnectionFactory _factory;
        private readonly object _lock = new object();
        private IConnection _connection;

        public RabbitMqConnection(ConnectionConfiguration config)
        {
            _factory = new ConnectionFactory
            {
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = config.RetryTimeout,
                HostName = config.Hostname,
                Port = (int)config.Port,
                RequestedHeartbeat = (ushort)config.Heartbeat.TotalSeconds,
                UserName = config.Username,
                Password = config.Password,
                VirtualHost = config.VirtualHost
            };

            _factory.ClientProperties = new Dictionary<string, object>
            {
                {"client-name", Environment.MachineName}
            };
        }

        public IConnection Connection
        {
            get
            {
                lock (_lock)
                {
                    if ((_connection == null) || !_connection.IsOpen)
                        _connection = _factory.CreateConnection();
                    _connection.AutoClose = false;
                }
                return _connection;
            }
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                if (_connection.IsOpen)
                    _connection.Close(30);

                _connection.Dispose();
            }
        }
    }
}
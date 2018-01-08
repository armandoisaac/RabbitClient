using System;
using System.Collections.Generic;
using System.Diagnostics;
using RabbitMQ.Client;

namespace RabbitClient
{
    internal class RabbitMqConnection
    {
        private readonly ConnectionFactory _factory;
        private readonly object _lock = new object();
        private readonly string _clientName;
        private IConnection _connection;

        public RabbitMqConnection(RabbitMqConnectionConfiguration config, string clientName)
        {
            _clientName = clientName;
            _factory = new ConnectionFactory
            {
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = config.RetryTimeout,
                HostName = config.Hostname,
                Port = (int) config.Port,
                RequestedHeartbeat = (ushort) config.Heartbeat.TotalSeconds,
                UserName = config.Username,
                Password = config.Password,
                VirtualHost = config.VirtualHost
            };

            var currentProcess = Process.GetCurrentProcess();
            _factory.ClientProperties = new Dictionary<string, object>
            {
                { "client_name", clientName },
                { "connection_name", clientName },
                { "hostname", Environment.MachineName },
                { "process_id", currentProcess.Id },
                { "process_name", currentProcess.ProcessName },
                { "process_started", currentProcess.StartTime.ToUniversalTime().ToString("R") },
                { "client_api", "RabbitClient"}
            };
        }

        public IConnection Connection
        {
            get
            {
                lock (_lock)
                {
                    if (_connection == null || !_connection.IsOpen)
                        _connection = _factory.CreateConnection(_clientName);
                    _connection.AutoClose = false;
                }
                return _connection;
            }
        }

        public void Dispose()
        {
            if (_connection == null) return;

            if (_connection.IsOpen)
                _connection.Close(60);

            _connection.Dispose();
        }
    }
}
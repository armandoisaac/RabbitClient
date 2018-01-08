using System;
using Diciannove.ServiceBus.Configuration;

namespace RabbitClient
{
    public class RabbitMqConnectionConfiguration : ConnectionConfiguration
    {
        public RabbitMqConnectionConfiguration()
        {
            Heartbeat = TimeSpan.FromSeconds(10);
            Port = 5672;
            Username = "guest";
            Password = "guest";
            VirtualHost = "/";
        }

        public uint Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public TimeSpan RetryTimeout { get; set; }

        public TimeSpan Heartbeat { get; set; }

        public string VirtualHost { get; set; }

        public Uri HttpApiUri { get; set; }
    }
}
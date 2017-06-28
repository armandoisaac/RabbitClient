using System;
namespace Diciannove.ServiceBus.Configuration
{
    public class ConnectionConfiguration
    {
        public ConnectionConfiguration()
        {
            Hostname = "localhost";
            Port = 5672;
            Username = "guest";
            Password = "guest";
        }

        public string Hostname { get; set; }
        public uint Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public TimeSpan RetryTimeout { get; set; }
    }
}
namespace RabbitClient.Core.Configuration
{
    public class ConnectionConfiguration
    {
        public ConnectionConfiguration()
        {
            Hostname = "localhost";
        }

        public string Hostname { get; set; }
    }
}
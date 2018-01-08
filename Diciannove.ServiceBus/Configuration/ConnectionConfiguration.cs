namespace Diciannove.ServiceBus.Configuration
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
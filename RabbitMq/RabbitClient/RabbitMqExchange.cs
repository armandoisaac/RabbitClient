using Newtonsoft.Json;

namespace RabbitClient
{
    public class RabbitMqExchange
    {
        public string Name { get; set; }
        
        public ExchangeType Type { get; set; }

        public bool IsDurable { get; set; }

        public bool AutoDelete { get; set; }

        public string Policy { get; set; }
    }
}
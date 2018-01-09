using System;
using Newtonsoft.Json;

namespace RabbitClient.Core.Messages
{
    public class RpcProperties
    {
        public DateTime RequestDate { get; set; }
        public string ClientName { get; set; }
        public Guid RequestId { get; set; }

        public RpcProperties() { }

        [JsonConstructor]
        public RpcProperties(Guid requestId, DateTime requestDate, string clientName)
        {
            RequestDate = requestDate;
            ClientName = clientName;
            RequestId = requestId;
        }
    }
}
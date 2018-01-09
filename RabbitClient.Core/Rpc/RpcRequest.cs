using System;
using Newtonsoft.Json;
using RabbitClient.Core.Messages;

namespace RabbitClient.Core.Rpc
{
    public sealed class RpcRequest : BusMessage
    {
        public Type MessageType { get; internal set; }

        public string JsonMessage { get; internal set; }

        public DateTime RequestDate { get; internal set; }

        public string ClientName { get; internal set; }

        public RpcRequest()
        {
            MessageId = Guid.NewGuid();
        }

        [JsonConstructor]
        public RpcRequest(Guid messageId, Type messageType, string jsonMessage, string clientName, DateTime requestDate)
        {
            MessageId = messageId;
            MessageType = messageType;
            JsonMessage = jsonMessage;
            RequestDate = requestDate;
            ClientName = clientName;
        }
    }
}
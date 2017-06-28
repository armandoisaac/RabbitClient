using System;
using Diciannove.ServiceBus.Messages;
using Newtonsoft.Json;

namespace Diciannove.ServiceBus.Rpc
{
    public sealed class RpcResponse : BusMessage
    {
        public RpcResponse()
        {
            MessageId = Guid.NewGuid();
        }

        [JsonConstructor]
        public RpcResponse(Guid messageId, Type messageType, string jsonMessage, RpcProperties request)
        {
            MessageId = messageId;
            MessageType = messageType;
            JsonMessage = jsonMessage;
            Request = request;
        }

        public Type MessageType { get; set; }

        public string JsonMessage { get; set; }

        public RpcProperties Request { get; set; }
    }
}
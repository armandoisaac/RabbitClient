using System;
using Newtonsoft.Json;
using RabbitClient.Core.Messages;

namespace RabbitClient.Core.Rpc
{
    public sealed class RpcError : BusMessage
    {
        public RpcError()
        {
        }

        [JsonConstructor]
        public RpcError(Guid messageId, Exception error)
        {
            MessageId = messageId;
            Error = error;
        }

        public Exception Error { get; set; }
    }
}
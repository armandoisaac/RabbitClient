using System;
using Diciannove.ServiceBus.Messages;
using Newtonsoft.Json;

namespace Diciannove.ServiceBus.Rpc
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
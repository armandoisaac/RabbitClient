using System;
using RabbitClient.Core.Messages;

namespace RabbitClient.Core.Rpc
{
    public interface IRpcRequest
    {
        Guid RequestId { get; set; }
        DateTime RequestTime { get; set; }
        Type RequestType { get; set; }
        string RequestedBy { get; set; }
        IBusMessage Message { get; set; }
    }
}
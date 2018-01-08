using System;
using Diciannove.ServiceBus.Messages;

namespace Diciannove.ServiceBus.Rpc
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
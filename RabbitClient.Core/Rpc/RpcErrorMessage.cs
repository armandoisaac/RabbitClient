using System;

namespace RabbitClient.Core.Rpc
{
    public class RpcErrorMessage
    {
        public string Message { get; set; }
        public Exception Exception { get; set; }
        public DateTime ErrorTime { get; set; }
    }
}
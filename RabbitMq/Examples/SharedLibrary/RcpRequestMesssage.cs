using System;
using Diciannove.ServiceBus.Messages;
using Newtonsoft.Json;

namespace Examples.SharedLibrary
{
    public class RcpRequestMesssage : BusMessage
    {
        public int Number { get; set; }

        public RcpRequestMesssage() { }

        [JsonConstructor]
        public RcpRequestMesssage(Guid messageId, int number)
        {
            MessageId = messageId;
            Number = number;
        }
    }
}
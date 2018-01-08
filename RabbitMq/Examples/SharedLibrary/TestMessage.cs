using System;
using Diciannove.ServiceBus.Messages;
using Newtonsoft.Json;

namespace Examples.SharedLibrary
{
    public class TestMessage : BusMessage
    {
        public string Message { get; set; }

        public TestMessage() { }

        [JsonConstructor]
        public TestMessage(Guid messageId, string message)
        {
            MessageId = messageId;
            Message = message;
        }
    }
}
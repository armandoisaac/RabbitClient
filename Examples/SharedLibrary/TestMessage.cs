using System;
using Newtonsoft.Json;
using RabbitClient.Core.Messages;

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
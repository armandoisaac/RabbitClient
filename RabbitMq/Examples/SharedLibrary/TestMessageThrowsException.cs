using System;
using Diciannove.ServiceBus.Messages;
using Newtonsoft.Json;

namespace Examples.SharedLibrary
{
    public class TestMessageThrowsException : BusMessage
    {
        public string Message { get; set; }

        public TestMessageThrowsException() { }

        [JsonConstructor]
        public TestMessageThrowsException(Guid messageId, string message)
        {
            MessageId = messageId;
            Message = message;
        }
    }
}
using System;
using Newtonsoft.Json;
using RabbitClient.Core.Messages;

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
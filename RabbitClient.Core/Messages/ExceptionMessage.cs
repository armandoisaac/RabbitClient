using System;
using Newtonsoft.Json;

namespace RabbitClient.Core.Messages
{
    public sealed class ExceptionMessage : BusMessage
    {
        public Exception Exception { get; internal set; }
        public string ExchangeName { get; internal set; }
        public string QueueName { get; internal set; }

        public ExceptionMessage() { }

        [JsonConstructor]
        public ExceptionMessage(Guid messageId, Exception exception, string exchangeName, string queueName)
        {
            MessageId = messageId;
            Exception = exception;
            ExchangeName = exchangeName;
            QueueName = queueName;
        }
    }
}
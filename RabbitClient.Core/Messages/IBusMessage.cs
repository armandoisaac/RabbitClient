using System;

namespace RabbitClient.Core.Messages
{
    public interface IBusMessage
    {
        Guid MessageId { get; set; }
    }
}
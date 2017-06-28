using System;

namespace Diciannove.ServiceBus.Messages
{
    public interface IBusMessage
    {
        Guid MessageId { get; set; }
    }
}
using System;

namespace RabbitClient.Core.Messages
{
    public abstract class BusMessage : IBusMessage
    {
        protected BusMessage()
        {
            MessageId = Guid.NewGuid();
        }

        public Guid MessageId { get; set; }
    }
}
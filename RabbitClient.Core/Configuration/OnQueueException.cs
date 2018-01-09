using System;
using RabbitClient.Core.Messages;

namespace RabbitClient.Core.Configuration
{
    public delegate void OnQueueException(IBusMessage message, Exception ex);
}
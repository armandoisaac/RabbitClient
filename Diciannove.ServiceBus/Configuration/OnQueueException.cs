using System;
using Diciannove.ServiceBus.Messages;

namespace Diciannove.ServiceBus.Configuration
{
    public delegate void OnQueueException(IBusMessage message, Exception ex);
}
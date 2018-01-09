using System;

namespace RabbitClient.Core.Configuration
{
    public class QueueConfiguration
    {
        public string QueueName { get; internal set; }
        public ushort? ConcurrencyLimit { get; internal set; }
        public OnQueueException ExceptionHandler { get; internal set; }
        public HandlerConfiguration HandlerConfiguration { get; internal set; }
        public uint RetryLimit { get; internal set; }

        public QueueConfiguration WithRetryLimit(uint retryLimit)
        {
            RetryLimit = retryLimit;
            return this;
        }

        public QueueConfiguration WithQueueName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name), "The queue name cannot be empty.");

            QueueName = name;
            return this;
        }

        public QueueConfiguration WithConcurrentyLimit(ushort limit)
        {
            ConcurrencyLimit = limit;
            return this;
        }

        public QueueConfiguration WithErrorHandling(OnQueueException handler)
        {
            ExceptionHandler =
                handler ?? throw new ArgumentNullException(nameof(handler), "The error handler cannot be null");
            return this;
        }

        public QueueConfiguration WithHandlers(Action<HandlerConfiguration> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler), "The queue handlers cannot be null");

            HandlerConfiguration = new HandlerConfiguration();
            handler(HandlerConfiguration);
            return this;
        }
    }
}
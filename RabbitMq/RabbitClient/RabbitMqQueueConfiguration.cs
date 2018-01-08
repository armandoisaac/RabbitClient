using System;
using System.Collections.Generic;
using Diciannove.ServiceBus.Configuration;

namespace RabbitClient
{
    public class RabbitMqQueueConfiguration : QueueConfiguration
    {
        /// <summary>
        /// Any custom exchange bindings for the queue
        /// </summary>
        internal HashSet<string> CustomExchangeBindings;

        /// <summary>
        ///     How long it will take before a message is said to be dead
        /// </summary>
        internal TimeSpan DefaultMessageExpiration;

        /// <summary>
        ///     How long a queue can be unused before it is automatically deleted
        /// </summary>
        internal TimeSpan IdleTimeout;

        /// <summary>
        /// The routing key for this queue used when publishing via an exchange
        /// </summary>
        internal string RoutingKey;

        internal RabbitMqQueueConfiguration()
        {
            IdleTimeout = TimeSpan.MaxValue;
            DefaultMessageExpiration = TimeSpan.MaxValue;
            CustomExchangeBindings = new HashSet<string>();
            RoutingKey = null;
        }

        /// <summary>
        ///     Sets a value how long a queue can be unused before it is automatically deleted
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public RabbitMqQueueConfiguration WithIdleTimeout(TimeSpan timeout)
        {
            if (timeout.TotalMilliseconds < 1000)
                throw new ArgumentException("The idle timeout must be at least one second", nameof(timeout));

            if (timeout.TotalMilliseconds > int.MaxValue)
                throw new ArgumentException("The idle timeout must cannot be greater than " + int.MaxValue,
                    nameof(timeout));

            IdleTimeout = timeout;
            return this;
        }

        /// <summary>
        ///     Defines how long it will take before a message is said to be dead
        /// </summary>
        /// <param name="defaultExpiration"></param>
        /// <returns></returns>
        public RabbitMqQueueConfiguration WithDefaultMessageExpiration(TimeSpan defaultExpiration)
        {
            if (defaultExpiration.TotalMilliseconds.Equals(0))
                throw new ArgumentException("The default message expiration must be greater than 0",
                    nameof(defaultExpiration));

            if (defaultExpiration.TotalMilliseconds > int.MaxValue)
                throw new ArgumentException("The idle timeout must cannot be greater than " + int.MaxValue,
                    nameof(defaultExpiration));

            DefaultMessageExpiration = defaultExpiration;
            return this;
        }

        /// <summary>
        ///     Defines custom exchange bindings for the queue
        /// </summary>
        /// <param name="exchanges"></param>
        /// <returns></returns>
        public RabbitMqQueueConfiguration WithCustomExchangeBindings(params string[] exchanges)
        {
            foreach (var exchange in exchanges)
                CustomExchangeBindings.Add(exchange);

            return this;
        }

        /// <summary>
        /// Sets the routing key to be used when publishing via an exchange
        /// </summary>
        /// <param name="routingKey"></param>
        /// <returns></returns>
        public RabbitMqQueueConfiguration WithRoutingKey(string routingKey)
        {
            RoutingKey = routingKey;
            return this;
        }
    }
}
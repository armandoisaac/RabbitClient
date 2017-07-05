using System;
using System.Threading.Tasks;
using Diciannove.ServiceBus.Configuration;
using Diciannove.ServiceBus.Messages;

namespace Diciannove.ServiceBus
{
    public interface IServiceBus<TConnectionConfiguration> : IDisposable
        where TConnectionConfiguration : ConnectionConfiguration, new()
    {
        /// <summary>
        ///     Initializes the service bus implementation
        /// </summary>
        /// <param name="config">The configuration of the service bus broker</param>
        void Initialize(ServiceBusConfiguration<TConnectionConfiguration> config);

        /// <summary>
        ///     Publishes a message to the message broker
        /// </summary>
        /// <param name="message">The message to be sent</param>
        void Publish(IBusMessage message);

        /// <summary>
        ///     Sends a message to an specific queue on the message broker
        /// </summary>
        /// <param name="queueName">The queue name</param>
        /// <param name="message">The message to be sent</param>
        void Send(string queueName, IBusMessage message);

        /// <summary>
        ///     Publishes a message to the message broker
        /// </summary>
        /// <param name="message">The message to be sent</param>
        /// <param name="ttl">The amout of time before the message expires</param>
        void Publish(IBusMessage message, TimeSpan ttl);

        /// <summary>
        ///     Sends a message to an specific queue on the message broker
        /// </summary>
        /// <param name="queueName">The queue name</param>
        /// <param name="message">The message to be sent</param>
        /// <param name="ttl">The amout of time before the message expires</param>
        void Send(string queueName, IBusMessage message, TimeSpan ttl);

        /// <summary>
        ///     Makes a request to an specific queue and waits for a response
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="queueName">The queue name</param>
        /// <param name="message">The message to be sent</param>
        /// <returns></returns>
        Task<TResponse> RpcRequest<TResponse>(string queueName, IBusMessage message);

        /// <summary>
        ///     Makes a request to an specific queue and waits for a response
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="queueName">The queue name</param>
        /// <param name="message">The message to be sent</param>
        /// <param name="timeout">How long the system will wait for a response before throwing an exception</param>
        /// <returns></returns>
        Task<TResponse> RpcRequest<TResponse>(string queueName, IBusMessage message, TimeSpan timeout);
    }
}
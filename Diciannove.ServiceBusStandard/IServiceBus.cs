using System;
using System.Threading.Tasks;
using Diciannove.ServiceBus.Configuration;
using Diciannove.ServiceBus.Messages;

namespace Diciannove.ServiceBus
{
    public interface IServiceBus<TConnectionConfiguration> : IDisposable 
        where TConnectionConfiguration : ConnectionConfiguration, new()
    {
        void Initialize(ServiceBusConfiguration<TConnectionConfiguration> config);

        void Publish(IBusMessage message);

        void Send(string queueName, IBusMessage message);

        Task<TResponse> RpcRequest<TResponse>(string queueName, IBusMessage message);

        Task<TResponse> RpcRequest<TResponse>(string queueName, IBusMessage message, TimeSpan timeout);
    }
}
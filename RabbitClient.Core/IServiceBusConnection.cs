using System;
using RabbitClient.Core.Configuration;

namespace RabbitClient.Core
{
    public interface IServiceBusConnection<TConnection>: IDisposable where TConnection : class
    {
        TConnection GetConnection(ConnectionConfiguration config);
    }
}
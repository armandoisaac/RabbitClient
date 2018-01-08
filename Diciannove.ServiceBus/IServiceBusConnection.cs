using System;
using Diciannove.ServiceBus.Configuration;

namespace Diciannove.ServiceBus
{
    public interface IServiceBusConnection<TConnection>: IDisposable where TConnection : class
    {
        TConnection GetConnection(ConnectionConfiguration config);
    }
}
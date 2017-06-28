using System;
using Diciannove.ServiceBus.Configuration;

namespace Diciannove.ServiceBus
{
    public interface IServiceBusConnection<T>: IDisposable where T : class
    {
        T GetConnection(ConnectionConfiguration config);
    }
}
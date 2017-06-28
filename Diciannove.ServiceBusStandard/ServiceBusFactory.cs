using System;
using Diciannove.ServiceBus.Configuration;
using Diciannove.ServiceBus.RabbitMq;
using RabbitMQ.Client;

namespace Diciannove.ServiceBus
{
    public class ServiceBusFactory
    {
        /// <summary>
        ///     The bus configuration
        /// </summary>
        private readonly ServiceBusConfiguration _busConfiguration;


        private ServiceBusFactory()
        {
            // I don't like this, but if this line is not here, the RabbitMq.Client.dll is not copied to any project.
            Action<Type> noop = _ => { };
            var dummy = typeof(Protocols);
            noop(dummy);

            // Create the bus configuration
            _busConfiguration = new ServiceBusConfiguration();
        }

        public static ServiceBusFactory Create()
        {
            return new ServiceBusFactory();
        }

        public ServiceBusFactory WithHost(string host, ushort port = 5672)
        {
            _busConfiguration.ConnectionConfiguration.Hostname = host;
            _busConfiguration.ConnectionConfiguration.Port = port;
            return this;
        }

        public ServiceBusFactory WithVirtualHost(string virtualHost = "/")
        {
            _busConfiguration.ConnectionConfiguration.VirtualHost = virtualHost;
            return this;
        }

        public ServiceBusFactory WithUser(string user = "guest", string password = "guest")
        {
            _busConfiguration.ConnectionConfiguration.Username = user;
            _busConfiguration.ConnectionConfiguration.Password = password;
            return this;
        }

        public ServiceBusFactory WithHeartbeat(ushort seconds)
        {
            _busConfiguration.ConnectionConfiguration.Heartbeat = TimeSpan.FromSeconds(seconds);
            return this;
        }

        public ServiceBusFactory WithConnectionTimeout(ushort timeout)
        {
            _busConfiguration.ConnectionConfiguration.RetryTimeout = TimeSpan.FromSeconds(timeout);
            return this;
        }

        /// <summary>
        ///     Registers the handlers for the queues
        /// </summary>
        /// <param name="queueConfigs"></param>
        /// <returns></returns>
        public ServiceBusFactory WithQueueSubscriptions(params Action<QueueConfiguration>[] queueConfigs)
        {
            foreach (var config in queueConfigs)
            {
                var queueConfig = new QueueConfiguration();
                config(queueConfig);

                _busConfiguration.QueueHandlers.Add(queueConfig);
            }

            return this;
        }

        /// <summary>
        ///     Sets a custom client name
        /// </summary>
        /// <param name="clientName"></param>
        /// <returns></returns>
        public ServiceBusFactory WithClientName(string clientName)
        {
            if (string.IsNullOrEmpty(clientName))
                throw new ArgumentNullException(nameof(clientName), "Client name cannot be empty");
            _busConfiguration.ClientName = clientName;
            return this;
        }

        /// <summary>
        ///     Sets how many messages are by default processed at the same time.
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public ServiceBusFactory WithDefaultConcurrencyLimit(ushort limit)
        {
            _busConfiguration.DefaultConcurrencyLimit = limit;
            return this;
        }

        /// <summary>
        ///     Handles all the exceptions from the client
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public ServiceBusFactory WithExceptionHandling(Action<ServiceBusClientException> handler)
        {
            _busConfiguration.ExceptionHandler = handler;
            return this;
        }

        public IServiceBus Start()
        {
            var bus = new RabbitMqServiceBus();
            bus.Initialize(_busConfiguration);

            return bus;
        }
    }
}
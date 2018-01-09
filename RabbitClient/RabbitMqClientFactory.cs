using System;
using RabbitClient.Core;
using RabbitClient.Core.Configuration;
using RabbitClient.Configuration;

namespace RabbitClient
{
    /// <summary>
    ///     A class that will help configure and build the service bus client
    /// </summary>
    public class RabbitMqClientFactory
    {
        /// <summary>
        ///     The bus configuration
        /// </summary>
        private readonly RabbitMqServiceConfiguration _busConfiguration;

        /// <summary>
        ///     The default constructor for this class
        /// </summary>
        private RabbitMqClientFactory()
        {
            // Create the bus configuration
            _busConfiguration = new RabbitMqServiceConfiguration();
        }

        /// <summary>
        ///     Creates a new instance of the RabbitMqClientFactory class
        /// </summary>
        /// <returns></returns>
        public static RabbitMqClientFactory Create()
        {
            return new RabbitMqClientFactory();
        }

        /// <summary>
        ///     Sets the hostname of the service bus broker
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public RabbitMqClientFactory WithHost(string host, ushort port = 5672)
        {
            if (string.IsNullOrWhiteSpace(host)) throw new ArgumentException("Hostname cannot be empty", nameof(host));

            _busConfiguration.ConnectionConfiguration.Hostname = host;
            _busConfiguration.ConnectionConfiguration.Port = port;
            return this;
        }

        /// <summary>
        ///     Sets the virtualhost that will be used by this client
        /// </summary>
        /// <param name="virtualHost"></param>
        /// <returns></returns>
        public RabbitMqClientFactory WithVirtualHost(string virtualHost = "/")
        {
            _busConfiguration.ConnectionConfiguration.VirtualHost = virtualHost;
            return this;
        }

        /// <summary>
        ///     Sets the user / password for the service bus connection
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public RabbitMqClientFactory WithCredentials(string user = "guest", string password = "guest")
        {
            _busConfiguration.ConnectionConfiguration.Username = user;
            _busConfiguration.ConnectionConfiguration.Password = password;
            return this;
        }

        /// <summary>
        /// Sets the URL of the RabbitMq Http Api
        /// </summary>
        /// <param name="httpApiUri">The Url where the http api is hosted</param>
        /// <returns></returns>
        public RabbitMqClientFactory WithHttpApiUri(Uri httpApiUri)
        {
            _busConfiguration.ConnectionConfiguration.HttpApiUri = httpApiUri ?? throw new ArgumentException("The Http Api Uri cannot be null", nameof(httpApiUri));
            return this;
        }

        /// <summary>
        /// Sets the URL of the RabbitMq Http Api
        /// </summary>
        /// <param name="httpApiUri">The Url where the http api is hosted</param>
        /// <returns></returns>
        public RabbitMqClientFactory WithHttpApiUri(string httpApiUri)
        {
            if (string.IsNullOrWhiteSpace(httpApiUri)) throw new ArgumentException("The Http Api Uri cannot be null", nameof(httpApiUri));

            _busConfiguration.ConnectionConfiguration.HttpApiUri = new Uri(httpApiUri);
            return this;
        }

        /// <summary>
        ///     Sets the timeout (in seconds) in order to detect that the connection have failed.
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public RabbitMqClientFactory WithHeartbeat(ushort seconds)
        {
            _busConfiguration.ConnectionConfiguration.Heartbeat = TimeSpan.FromSeconds(seconds);
            return this;
        }

        /// <summary>
        ///     Sets the default connection timeout
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public RabbitMqClientFactory WithConnectionTimeout(ushort timeout)
        {
            _busConfiguration.ConnectionConfiguration.RetryTimeout = TimeSpan.FromSeconds(timeout);
            return this;
        }

        /// <summary>
        ///     Registers the handlers for the queues
        /// </summary>
        /// <param name="queueConfigs"></param>
        /// <returns></returns>
        public RabbitMqClientFactory WithQueueSubscriptions(params Action<RabbitMqQueueConfiguration>[] queueConfigs)
        {
            foreach (var config in queueConfigs)
            {
                var queueConfig = new RabbitMqQueueConfiguration();
                config(queueConfig);

                _busConfiguration.QueueHandlers.Add(queueConfig);
            }

            return this;
        }

        /// <summary>
        /// Creates custom exchanges as soon a connection is established
        /// </summary>
        /// <param name="exchanges"></param>
        /// <returns></returns>
        public RabbitMqClientFactory WithCustomExchanges(params RabbitMqExchange[] exchanges)
        {
            foreach (var exchange in exchanges)
                _busConfiguration.CustomExchanges.Add(exchange);

            return this;
        }

        /// <summary>
        ///     Sets a custom client name
        /// </summary>
        /// <param name="clientName"></param>
        /// <returns></returns>
        public RabbitMqClientFactory WithClientName(string clientName)
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
        public RabbitMqClientFactory WithDefaultConcurrencyLimit(ushort limit)
        {
            _busConfiguration.DefaultConcurrencyLimit = limit;
            return this;
        }

        /// <summary>
        ///     Handles all the exceptions from the client
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public RabbitMqClientFactory WithExceptionHandling(Action<ServiceBusClientException> handler)
        {
            _busConfiguration.ExceptionHandler = handler;
            return this;
        }

        /// <summary>
        ///     Initializes the service bus client and returns an instance of the RabbitMq client
        /// </summary>
        /// <returns></returns>
        public RabbitMqClient Build()
        {
            // Http Api cannot be empty
            if (_busConfiguration.ConnectionConfiguration.HttpApiUri == null)
                _busConfiguration.ConnectionConfiguration.HttpApiUri = new Uri($"http://{_busConfiguration.ConnectionConfiguration.Hostname}:15672");

            var bus = new RabbitMqClient(_busConfiguration);

            return bus;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RabbitClient.Configuration;
using RabbitClient.Core;
using RabbitClient.Core.Messages;
using RabbitClient.Core.Rpc;
using RabbitMQ.Client.Exceptions;
using RestSharp;
using RestSharp.Authenticators;
using JsonSerializer = RabbitClient.Serializers.JsonSerializer;

namespace RabbitClient
{
    /// <summary>
    ///     The service bus client that will connect to RabbitMq in order to send/receive messages
    /// </summary>
    public class RabbitMqClient : IServiceBus
    {
        private readonly RabbitMqServiceConfiguration _config;
        private readonly Lazy<RabbitMqConnection> _connection;
        private readonly object _lock = new object();
        private readonly List<RabbitMqQueue> _queues;
        private readonly RestClient _restClient;

        /// <summary>
        ///     The default constructor
        /// </summary>
        internal RabbitMqClient(RabbitMqServiceConfiguration config)
        {
            // First, let's create the connection
            _connection =
                new Lazy<RabbitMqConnection>(
                    () => new RabbitMqConnection(config.ConnectionConfiguration, config.ClientName), true);
            _config = config;
            _restClient = CreateRestClient();

            JsonConvert.DefaultSettings = () =>
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new StringEnumConverter {CamelCaseText = true});
                return settings;
            };

            // Create the exchanges
            foreach (var exchange in config.CustomExchanges)
                CreateExchange(exchange);

            // Create all the queues
            _queues = new List<RabbitMqQueue>();
            foreach (var queue in config.QueueHandlers)
            {
                // We need to set the QueueName
                var queueName = string.IsNullOrEmpty(queue.QueueName)
                    ? NamingConventions.GetDefaultQueueName(config.ClientName)
                    : queue.QueueName;
                queue.WithQueueName(queueName);

                // Create the consumer
                _queues.Add(new RabbitMqQueue(_connection.Value, queue, BasicPublish, HandleException));
            }

            // Create the RPC listener queue
            var rpcQueue = new RabbitMqQueueConfiguration()
                .WithIdleTimeout(TimeSpan.FromMinutes(6))
                .WithConcurrentyLimit(_config.DefaultConcurrencyLimit)
                .WithHandlers(register => { register.Register<RpcResponse>(HandleRpcResponse); })
                .WithQueueName(NamingConventions.GetRpcResponseQueueName(_config.ClientName))
                .WithRetryLimit(3);

            // Register the queue
            _queues.Add(new RabbitMqQueue(_connection.Value, (RabbitMqQueueConfiguration) rpcQueue, BasicPublish,
                HandleException));
        }

        /// <summary>
        ///     Publishes a message to the service bus
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="message"></param>
        public void Publish(string exchangeName, IBusMessage message)
        {
            BasicPublish(exchangeName, null, message);
        }

        /// <summary>
        ///     Publishes a message to the service bus
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="message"></param>
        /// <param name="routingKey"></param>
        public void Publish(string exchangeName, IBusMessage message, string routingKey)
        {
            BasicPublish(exchangeName, routingKey, message);
        }

        /// <summary>
        ///     Publishes a message to the service bus
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="message"></param>
        /// <param name="routingKey"></param>
        /// <param name="ttl"></param>
        public void Publish(string exchangeName, IBusMessage message, string routingKey, TimeSpan ttl)
        {
            BasicPublish(exchangeName, routingKey, message, ttl);
        }

        /// <summary>
        ///     Publishes a message to the service bus
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="message"></param>
        /// <param name="routingKey"></param>
        /// <param name="ttl"></param>
        public void Publish(string exchangeName, IBusMessage message, TimeSpan ttl)
        {
            BasicPublish(exchangeName, null, message, ttl);
        }

        /// <summary>
        ///     Publishes a message to the service bus
        /// </summary>
        /// <param name="message"></param>
        public void Publish(IBusMessage message)
        {
            var exchangeName = NamingConventions.ExchangeName(message.GetType());
            BasicPublish(exchangeName, "", message);
        }

        /// <summary>
        ///     Publishes a message to the service bus
        /// </summary>
        /// <param name="message"></param>
        /// <param name="routingKey"></param>
        public void Publish(IBusMessage message, string routingKey)
        {
            var exchangeName = NamingConventions.ExchangeName(message.GetType());
            BasicPublish(exchangeName, routingKey, message);
        }

        public void Publish(IBusMessage message, string routingKey, TimeSpan ttl)
        {
            var exchangeName = NamingConventions.ExchangeName(message.GetType());
            BasicPublish(exchangeName, routingKey, message, ttl);
        }

        public void Publish(IBusMessage message, TimeSpan ttl)
        {
            var exchangeName = NamingConventions.ExchangeName(message.GetType());
            BasicPublish(exchangeName, null, message, ttl);
        }

        /// <summary>
        ///     Sends a message to a queue on the service bus
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        public void Send(string queueName, IBusMessage message)
        {
            BasicPublish("", queueName, message);
        }

        public void Send(string queueName, IBusMessage message, TimeSpan ttl)
        {
            BasicPublish("", queueName, message, ttl);
        }

        #region IDisposable

        /// <summary>
        ///     Stops all queue consumers and disposes any open connection to the service bus
        /// </summary>
        public void Dispose()
        {
            foreach (var queue in _queues)
                queue.Dispose();

            if (_connection.IsValueCreated)
                _connection.Value.Dispose();
        }

        #endregion

        /// <summary>
        ///     Creates an exchange for message distribution
        /// </summary>
        /// <param name="exchange"></param>
        public void CreateExchange(RabbitMqExchange exchange)
        {
            if (exchange == null) throw new ArgumentException("The exchange cannot be null", nameof(exchange));
            if (string.IsNullOrEmpty(exchange.Name))
                throw new ArgumentException("The exchange information is invalid.", nameof(exchange));

            // Skip if the exchange already exists
            if (ExistsExchange(exchange.Name)) return;

            // Create the exchange
            using (var channel = _connection.Value.Connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange.Name, exchange.Type.ToString(), exchange.IsDurable,
                    exchange.AutoDelete, new Dictionary<string, object>());
            }
        }

        /// <summary>
        ///     Binds a queue to an exchange
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="queueName"></param>
        public void BindExchange(string exchangeName, string queueName)
        {
            BindExchange(exchangeName, queueName, null);
        }

        /// <summary>
        ///     Binds a queue to an exchange
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="queueName"></param>
        /// <param name="routingKey"></param>
        public void BindExchange(string exchangeName, string queueName, string routingKey)
        {
            try
            {
                // Create the exchange
                using (var channel = _connection.Value.Connection.CreateModel())
                {
                    channel.QueueBind(queueName, exchangeName, routingKey, new Dictionary<string, object>());
                }
            }
            catch (OperationInterruptedException ex)
            {
                throw new Exception(
                    "Could not bind queue to exchange. Please validate that both queue and exchange have been declared",
                    ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An unknown exception has ocurred while binding the queue to the exchange.", ex);
            }
        }

        public bool ExistsExchange(string exchange)
        {
            if (string.IsNullOrWhiteSpace(exchange))
                throw new ArgumentException("Exchange cannot be null", nameof(exchange));

            var response = GetResponse($"api/exchanges/{GetUrl(exchange)}", Method.GET);

            return response.StatusCode == HttpStatusCode.OK;
        }

        public bool DeleteExchange(string exchange)
        {
            if (string.IsNullOrWhiteSpace(exchange))
                throw new ArgumentException("Exchange cannot be null", nameof(exchange));

            var response = GetResponse($"api/exchanges/{GetUrl(exchange)}", Method.DELETE);

            return response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent;
        }

        public bool ExistsQueue(string queue)
        {
            if (string.IsNullOrWhiteSpace(queue))
                throw new ArgumentException("Queue name cannot be null", nameof(queue));

            var response = GetResponse($"api/queues/{GetUrl(queue)}", Method.GET);

            return response.StatusCode == HttpStatusCode.OK;
        }

        public bool DeleteQueue(string queue)
        {
            if (string.IsNullOrWhiteSpace(queue))
                throw new ArgumentException("Queue name cannot be null", nameof(queue));

            var response = GetResponse($"api/queues/{GetUrl(queue)}", Method.DELETE);

            return response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent;
        }

        public bool PurgeQueue(string queue)
        {
            if (string.IsNullOrWhiteSpace(queue))
                throw new ArgumentException("Queue name cannot be null", nameof(queue));

            var response = GetResponse($"api/queues/{GetUrl(queue)}/contents", Method.DELETE,
                new Dictionary<string, string> {{"Accept", "text/html"}});

            return response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent;
        }

        #region Rpc

        /// <summary>
        ///     Performs a RPC Request to a queue
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<TResponse> RpcRequest<TResponse>(string queueName, IBusMessage message)
        {
            return await RpcRequest<TResponse>(queueName, message, TimeSpan.FromSeconds(30));
        }

        /// <summary>
        ///     Performs a RPC request to a queue
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task<TResponse> RpcRequest<TResponse>(string queueName, IBusMessage message, TimeSpan timeout)
        {
            var task = RpcRequestRepository.GetInstance().RegisterRpcRequest<TResponse>(message, timeout, () =>
            {
                var request = new RpcRequest(Guid.NewGuid(), message.GetType(), JsonConvert.SerializeObject(message),
                    _config.ClientName, DateTime.UtcNow);

                // Publish the message
                Send(queueName, request);
            });

            return await task;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Publishes/Sends a message to the service bus
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="queueNameOrRoutingKey"></param>
        /// <param name="message"></param>
        /// <param name="ttl"></param>
        private void BasicPublish(string exchangeName, string queueNameOrRoutingKey, IBusMessage message,
            TimeSpan? ttl = null)
        {
            using (var channel = _connection.Value.Connection.CreateModel())
            {
                var clientName = _config.ClientName;

                // Basic publish MUST publish one message per channel
                lock (_lock)
                {
                    var props = channel.CreateBasicProperties();

                    if (ttl != null)
                        props.Expiration = ttl.Value.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);

                    // Wrap the message
                    var envelope = new ServiceBusMessageEnvelope(Guid.NewGuid(),
                        new ServiceBusMessageProperties(exchangeName, queueNameOrRoutingKey, DateTime.UtcNow,
                            clientName),
                        message.GetType(), JsonConvert.SerializeObject(message), 0);
                    var envelopeJson = JsonConvert.SerializeObject(envelope);
                    var body = Encoding.UTF8.GetBytes(envelopeJson);

                    channel.BasicPublish(exchangeName, queueNameOrRoutingKey, false, props, body);
                }
            }
        }

        /// <summary>
        ///     Handles the response for an RPC request
        /// </summary>
        /// <param name="message"></param>
        private void HandleRpcResponse(RpcResponse message)
        {
            RpcRequestRepository.GetInstance().HandleRcpResponse(message);
        }

        /// <summary>
        ///     Handles any exception that ocurred on the service bus and passes this information to the configured error handler
        ///     (if applies)
        /// </summary>
        /// <param name="e"></param>
        private void HandleException(ServiceBusClientException e)
        {
            // Notify the exception handler
            var handler = _config.ExceptionHandler ?? (error => { });
            handler(e);
        }

        private RestClient CreateRestClient()
        {
            var client = new RestClient(_config.ConnectionConfiguration.HttpApiUri);

            if (!string.IsNullOrWhiteSpace(_config.ConnectionConfiguration.Username))
                client.Authenticator = new HttpBasicAuthenticator(_config.ConnectionConfiguration.Username,
                    _config.ConnectionConfiguration.Password);
            return client;
        }

        private string GetUrl(string path)
        {
            return $"{HttpUtility.UrlEncode(_config.ConnectionConfiguration.VirtualHost)}/{path}";
        }

        private IRestResponse GetResponse(string url, Method method, Dictionary<string, string> headers = null)
        {
            var request = new RestRequest(url, method)
            {
                JsonSerializer = new JsonSerializer()
            };

            // Add additional headers
            if (headers != null)
                foreach (var header in headers)
                    request.AddHeader(header.Key, header.Value);

            if (request.Method == Method.DELETE)
            {
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddHeader("Content-Length", "0");
            }

            var response = _restClient.Execute(request);

            if (response.ErrorException != null)
                throw new Exception("An exception ocurred while executing the request", response.ErrorException);

            return response;
        }

        #endregion
    }
}
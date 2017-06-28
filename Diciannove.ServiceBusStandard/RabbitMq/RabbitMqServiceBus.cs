using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Diciannove.ServiceBus.Configuration;
using Diciannove.ServiceBus.Messages;
using Diciannove.ServiceBus.Rpc;
using Newtonsoft.Json;

namespace Diciannove.ServiceBus.RabbitMq
{
    public class RabbitMqServiceBus : IServiceBus
    {
        private readonly object _lock = new object();
        private ServiceBusConfiguration _config;
        private Lazy<RabbitMqConnection> _connection;
        private List<RabbitMqQueue> _queues;

        internal RabbitMqServiceBus()
        {
        }

        public void Initialize(ServiceBusConfiguration config)
        {
            // First, let's create the connection
            _connection = new Lazy<RabbitMqConnection>(() => new RabbitMqConnection(config.ConnectionConfiguration),
                true);
            _config = config;

            // Create all the queues
            _queues = new List<RabbitMqQueue>();
            foreach (var queue in config.QueueHandlers)
            {
                // We need to set the QueueName
                queue.QueueName = string.IsNullOrEmpty(queue.QueueName)
                    ? NamingConventions.GetDefaultQueueName(config.ClientName)
                    : queue.QueueName;

                // Create the consumer
                _queues.Add(new RabbitMqQueue(_connection.Value, queue, BasicPublish, HandleException));
            }

            // Create the RPC listener queue
            var rpcQueue = new QueueConfiguration()
                .WithConcurrentyLimit(_config.DefaultConcurrencyLimit)
                .WithHandlers(register => { register.Register<RpcResponse>(HandleRpcResponse); })
                .WithQueueName(NamingConventions.GetRpcResponseQueueName(_config.ClientName))
                .WithRetryLimit(3);

            // Register the queue
            _queues.Add(new RabbitMqQueue(_connection.Value, rpcQueue, BasicPublish, HandleException));
        }

        public void Publish(IBusMessage message)
        {
            var exchangeName = NamingConventions.ExchangeName(message.GetType());
            BasicPublish(exchangeName, "", message);
        }

        public void Send(string queueName, IBusMessage message)
        {
            BasicPublish("", queueName, message);
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

        #region Rpc

        public async Task<TResponse> RpcRequest<TResponse>(string queueName, IBusMessage message)
        {
            return await RpcRequest<TResponse>(queueName, message, TimeSpan.FromSeconds(30));
        }

        public async Task<TResponse> RpcRequest<TResponse>(string queueName, IBusMessage message, TimeSpan timeout)
        {
            var task = RpcRequestRepository.GetInstance().RegisterRequest<TResponse>(message, timeout, () =>
            {
                var request = new RpcRequest
                {
                    JsonMessage = JsonConvert.SerializeObject(message),
                    MessageType = message.GetType(),
                    RequestDate = DateTime.UtcNow,
                    ClientName = _config.ClientName
                };

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
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        private void BasicPublish(string exchangeName, string queueName, IBusMessage message)
        {
            using (var channel = _connection.Value.Connection.CreateModel())
            {
                // Basic publish MUST publish one message per channel
                lock (_lock)
                {
                    // Wrap the message
                    var envelope = new ServiceBusMessageEnvelope
                    {
                        MessageType = message.GetType(),
                        JsonMessage = JsonConvert.SerializeObject(message),
                        Properties = new ServiceBusMessageProperties
                        {
                            DatePublished = DateTime.UtcNow,
                            ExchangeName = exchangeName,
                            QueueName = queueName,
                            ClientName = _config.ClientName
                        }
                    };
                    var envelopeJson = JsonConvert.SerializeObject(envelope);
                    var body = Encoding.UTF8.GetBytes(envelopeJson);

                    channel.BasicPublish(exchangeName, queueName, false, null, body);
                }
            }
        }


        private void HandleRpcResponse(RpcResponse message)
        {
            RpcRequestRepository.GetInstance().HandleRcpResponse(message);
        }

        private void HandleException(ServiceBusClientException e)
        {
            // Notify the exception handler
            var handler = _config.ExceptionHandler ?? (error => { });
            handler(e);
        }

        #endregion
    }
}
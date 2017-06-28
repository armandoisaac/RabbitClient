using System;
using System.Threading;
using System.Threading.Tasks;
using Diciannove.ServiceBus.Configuration;
using Diciannove.ServiceBus.Messages;
using Diciannove.ServiceBus.Rpc;
using RabbitMQ.Client;
using Newtonsoft.Json;

namespace Diciannove.ServiceBus.RabbitMq
{
    internal class RabbitMqQueue : IDisposable
    {
        private readonly IModel _channel;
        private readonly RabbitMqConnection _connection;
        private readonly QueueConfiguration _queueConfig;
        private readonly CancellationTokenSource _token;
        private readonly PublishDelegate _publishHandler;
        private readonly ExceptionDelegate _exceptionHandler;
        private RabbitMqConsumer _consumer;

        public RabbitMqQueue(RabbitMqConnection connection, QueueConfiguration queueConfig, PublishDelegate publishHandler, ExceptionDelegate exceptionHandler)
        {
            _connection = connection;
            _queueConfig = queueConfig;
            _publishHandler = publishHandler;
            _exceptionHandler = exceptionHandler;
            _channel = RabbitMqChannelFactory.GetInstance(_connection).GetChannel(_queueConfig.QueueName);
            _token = new CancellationTokenSource();

            // Register the RpcRequest handler
            _queueConfig.HandlerConfiguration.Register<RpcRequest>(HandleRpcRequest);

            // Run the queue
            Run();
        }

        /// <summary>
        /// Detects if the consumer has stopped consuming messages
        /// </summary>
        private void Run()
        {
            Task.Factory.StartNew(() =>
            {
                if (_consumer == null)
                {
                    // Declare the queue
                    _channel.QueueDeclare(_queueConfig.QueueName, true, false, false, null);

                    // Create all the exchanges according the message type of each handler
                    foreach (var handler in _queueConfig.HandlerConfiguration.Handlers)
                    {
                        // Create the exchange
                        var exchangeName = NamingConventions.ExchangeName(handler.Key);
                        _channel.ExchangeDeclare(NamingConventions.ExchangeName(handler.Key), "fanout", true);

                        // Bind the exchange
                        _channel.QueueBind(_queueConfig.QueueName, exchangeName, "");
                    }

                    // Create the consumer
                    _consumer = new RabbitMqConsumer(_queueConfig, _channel, _publishHandler, _exceptionHandler);
                }

                if (!_consumer.IsRunning)
                {
                    _consumer.Start();
                }
            }, _token.Token);
        }

        private void HandleRpcRequest(RpcRequest request)
        {
            if (!_queueConfig.HandlerConfiguration.RpcHandlers.ContainsKey(request.MessageType))
                throw new Exception("There is no handler for message type " + request.MessageType);

            var originalMessage = JsonConvert.DeserializeObject(request.JsonMessage, request.MessageType);
            if (!(originalMessage is IBusMessage))
                throw new Exception("The message type " + request.MessageType +
                                    " does not implemente IBusMessage interface.");

            // Create the response
            var response = new RpcResponse()
            {
                Request = new RpcProperties
                {
                    RequestDate = request.RequestDate,
                    ClientName = request.ClientName,
                    RequestId = ((IBusMessage)originalMessage).MessageId
                }
            };

            try
            {
                // Execute the handler and get the response
                var result = _queueConfig.HandlerConfiguration.RpcHandlers[request.MessageType]((IBusMessage)originalMessage);

                // Assign the properties
                response.MessageType = result.GetType();
                response.JsonMessage = JsonConvert.SerializeObject(result);
            }
            catch (Exception e)
            {
                // There was an error while creating the response. Return a RpcError message
                response.JsonMessage = JsonConvert.SerializeObject(new RpcError { Error = e });
                response.MessageType = typeof(RpcError);
            }

            // Publish the response to the service bus
            _publishHandler("", NamingConventions.GetRpcResponseQueueName(request.ClientName), response);
        }

        public void Dispose()
        {
            _token.Cancel();
            _consumer.Stop();
            RabbitMqChannelFactory.GetInstance(_connection).UnRegister(_queueConfig.QueueName);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Diciannove.ServiceBus;
using Diciannove.ServiceBus.Messages;
using Diciannove.ServiceBus.Rpc;
using RabbitMQ.Client;
using Newtonsoft.Json;
using RabbitMQ.Client.Exceptions;

namespace RabbitClient
{
    internal class RabbitMqQueue : IDisposable
    {
        private readonly IModel _channel;
        private readonly RabbitMqConnection _connection;
        private readonly RabbitMqQueueConfiguration _queueConfig;
        private readonly CancellationTokenSource _token;
        private readonly PublishDelegate _publishHandler;
        private readonly ExceptionDelegate _exceptionHandler;
        private RabbitMqConsumer _consumer;

        public RabbitMqQueue(RabbitMqConnection connection, RabbitMqQueueConfiguration queueConfig, PublishDelegate publishHandler, ExceptionDelegate exceptionHandler)
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
                    try
                    {
                        var args = new Dictionary<string, object>();

                        // Set the default expiration
                        if (_queueConfig.DefaultMessageExpiration != TimeSpan.MaxValue)
                            args.Add("x-message-ttl", (int)_queueConfig.DefaultMessageExpiration.TotalMilliseconds);

                        // Set the queue expiration
                        if (_queueConfig.IdleTimeout != TimeSpan.MaxValue)
                            args.Add("x-expires", (int)_queueConfig.IdleTimeout.TotalMilliseconds);

                        // Declare the queue
                        _channel.QueueDeclare(_queueConfig.QueueName, true, false, false, args);
                    }
                    catch (OperationInterruptedException ex)
                    {
                        _exceptionHandler(new ServiceBusClientException("There was an exception while declaring the queue. Please make sure that if the queue already exists, it matches the selected configuration.", "", "", _queueConfig.QueueName, ex));
                        return;
                    }
                    catch (Exception ex)
                    {
                        _exceptionHandler(new ServiceBusClientException("There was an exception while declaring the queue.", "", "", _queueConfig.QueueName, ex));
                        return;
                    }

                    // Create all the exchanges according the message type of each handler
                    foreach (var handler in _queueConfig.HandlerConfiguration.Handlers)
                    {
                        // Create the exchange
                        var exchangeName = NamingConventions.ExchangeName(handler.Key);
                        _channel.ExchangeDeclare(NamingConventions.ExchangeName(handler.Key), "fanout", true);

                        // Bind the exchange
                        _channel.QueueBind(_queueConfig.QueueName, exchangeName, null);
                    }

                    // Bind custom exchanges
                    foreach (var exchange in _queueConfig.CustomExchangeBindings)
                    {
                        _channel.QueueBind(_queueConfig.QueueName, exchange, _queueConfig.RoutingKey);
                    }

                    // Create the consumer
                    _consumer = new RabbitMqConsumer(_queueConfig, _channel, _publishHandler, _exceptionHandler);
                }

                if (_consumer != null && !_consumer.IsRunning)
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
            _publishHandler("", NamingConventions.GetRpcResponseQueueName(request.ClientName), response, TimeSpan.FromMinutes(10));
        }

        public void Dispose()
        {
            _token.Cancel();
            _consumer.Stop();
            RabbitMqChannelFactory.GetInstance(_connection).UnRegister(_queueConfig.QueueName);
        }
    }
}
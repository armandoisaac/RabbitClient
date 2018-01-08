using System;
using System.Threading;
using System.Threading.Tasks;
using Diciannove.ServiceBus;
using Diciannove.ServiceBus.Configuration;
using RabbitMQ.Client;

namespace RabbitClient
{
    internal class RabbitMqConsumer
    {
        private readonly IModel _channel;
        private readonly ExceptionDelegate _exceptionHandler;
        private readonly PublishDelegate _publishHandler;
        private readonly QueueConfiguration _queueConfig;
        private readonly CancellationTokenSource _token;
        private RabbitMqQueueConsumer _consumer;
        private Task _task;

        public RabbitMqConsumer(QueueConfiguration queueConfig, IModel channel, PublishDelegate publishHandler,
            ExceptionDelegate exceptionHandler)
        {
            _queueConfig = queueConfig;
            _channel = channel;
            _publishHandler = publishHandler;
            _exceptionHandler = exceptionHandler;
            _token = new CancellationTokenSource();
        }

        public bool IsRunning => _consumer != null && _consumer.IsRunning;

        public void Start()
        {
            _task = Task.Factory.StartNew(() =>
            {
                try
                {
                    Consume();
                }
                catch (Exception e)
                {
                    // Alert the "unhandled" exception handler
                    var exception = new ServiceBusClientException(
                        "An unknown error has ocurred while consuming messages from the queue.",
                        null,
                        null,
                        _queueConfig.QueueName,
                        e);
                    _exceptionHandler(exception);
                }
            }, _token.Token);
        }

        public void Stop()
        {
            _token.Cancel();
            if (_consumer != null)
            {
                _channel.BasicCancel(_consumer.ConsumerTag);
                _consumer = null;
            }
        }

        private void Consume()
        {
            _channel.BasicQos(0, _queueConfig.ConcurrencyLimit ?? 1, false);
            _consumer = new RabbitMqQueueConsumer(_channel, _queueConfig, _publishHandler, _exceptionHandler);

            // Consume while !cancellation is requested
            while (!_token.IsCancellationRequested)
            {
                _channel.BasicConsume(_queueConfig.QueueName, false, _consumer);
                Thread.Sleep(500);
            }
        }
    }
}
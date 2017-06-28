using System;
using System.Text;
using System.Threading.Tasks;
using Diciannove.ServiceBus.Configuration;
using Diciannove.ServiceBus.Messages;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Diciannove.ServiceBus.RabbitMq
{
    internal class RabbitMqQueueConsumer : DefaultBasicConsumer
    {
        private readonly ExceptionDelegate _exceptionHandler;
        private readonly PublishDelegate _publishHandler;
        private readonly QueueConfiguration _queueConfig;

        public RabbitMqQueueConsumer(IModel channel, QueueConfiguration queueConfig, PublishDelegate publishHandler,
            ExceptionDelegate exceptionHandler) : base(channel)
        {
            _queueConfig = queueConfig;
            _publishHandler = publishHandler;
            _exceptionHandler = exceptionHandler;
        }

        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered,
            string exchange,
            string routingKey, IBasicProperties properties, byte[] body)
        {
            Task.Factory.StartNew(() =>
            {
                ServiceBusMessageEnvelope message;
                string messageString = null;
                base.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);

                // Deserialize the message
                try
                {
                    messageString = Encoding.UTF8.GetString(body);
                    message = JsonConvert.DeserializeObject<ServiceBusMessageEnvelope>(messageString);
                }
                catch (Exception e)
                {
                    // Acknowledge the message, so it will not be reprocessed
                    Model.BasicAck(deliveryTag, false);

                    // Alert the "unhandled" exception handler
                    HandleException(
                        "The received message cannot be deserialized. Please make sure the message is a ServiceBusMessageEnvelope object.",
                        messageString,
                        exchange,
                        routingKey,
                        e);

                    // Exit
                    return;
                }

                // Now, handle the message
                HandleMessage(exchange, deliveryTag, message);
            });
        }

        private void HandleMessage(string exchange, ulong deliveryTag, ServiceBusMessageEnvelope message)
        {
            try
            {
                if (!_queueConfig.HandlerConfiguration.Handlers.ContainsKey(message.MessageType))
                    throw new Exception("There is no handler for message type " + message.MessageType);

                var originalMessage = JsonConvert.DeserializeObject(message.JsonMessage, message.MessageType);
                if (!(originalMessage is IBusMessage))
                    throw new Exception("The message type " + message.MessageType +
                                        " does not implemente IBusMessage interface.");

                try
                {
                    // Process the message
                    _queueConfig.HandlerConfiguration.Handlers[message.MessageType]((IBusMessage) originalMessage);

                    // Acknowledge in order to remove it from the queue
                    Model.BasicAck(deliveryTag, false);
                }
                catch (Exception e)
                {
                    // Notify the queue exception handler that there was an exception
                    _queueConfig.ExceptionHandler((IBusMessage) originalMessage, e);

                    // Acknowledge the message, we will re-publish for retry
                    Model.BasicAck(deliveryTag, false);

                    // Retry if needed
                    message.RetryCount++;
                    if (message.RetryCount < _queueConfig.RetryLimit)
                        _publishHandler(message.Properties.ExchangeName, message.Properties.QueueName, message);
                }
            }
            catch (Exception e)
            {
                // There was an exception with the message, do not attempt to retry
                Model.BasicAck(deliveryTag, false);

                // Handle the exception
                HandleException("An error ocurred while consuming the message", null, exchange, _queueConfig.QueueName,
                    e);
            }
        }

        private void HandleException(string errorMessage, string originalMessage, string exchange, string queueName,
            Exception innerException)
        {
            var exception = new ServiceBusClientException(
                errorMessage,
                originalMessage,
                exchange,
                queueName,
                innerException);
            _exceptionHandler(exception);
        }
    }
}
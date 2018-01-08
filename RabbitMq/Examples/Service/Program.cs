using System;
using System.Configuration;
using System.Threading;
using Examples.SharedLibrary;
using RabbitClient;

namespace Examples.Service
{
    internal class Program
    {
        /// <summary>
        /// A console application that allows to listen messages from the RabbitMq service bus
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            // Configure the logger in order to display information to the console
            var logger = new ConsoleLogger();

            var bus = RabbitMqClientFactory.Create()

                // Set the virtual host
                .WithVirtualHost("/TEST")

                // Set a name for this connection.
                .WithClientName(Environment.MachineName + "_Service")

                // Set the rabbitmq host
                .WithHost(ConfigurationManager.AppSettings["RabbitMqHost"])

                // How many messages we will process at the same time
                .WithDefaultConcurrencyLimit(3)

                // Configure the "unhandled" exception handler
                .WithExceptionHandling(e => { logger.Fatal("A fatal unknown exception has ocurred.", e); })

                // Configure all the queue subscriptions
                .WithQueueSubscriptions(cfg =>
                {
                    cfg.WithQueueName("TestQueue");

                    // Configure an error handling for this specific queue
                    cfg.WithErrorHandling((message, error) =>
                    {
                        logger.Fatal($"An exception ocurred while processing message {message.MessageId}. The error is: {error}");
                    });
                    
                    // Configure all the handlers for the queue
                    cfg.WithHandlers(h =>
                    {
                        h.Register<TestMessage>(message =>
                        {
                            logger.Info("Received message");
                            Thread.Sleep(3000);
                            logger.Info("Received messageId {0} with message {1}", message.MessageId,
                                message.Message);
                        });
                        h.Register<TestMessageThrowsException>(message =>
                        {
                            logger.Info("Received messageId {0} with message {1}. Will throw exception",
                                message.MessageId, message.Message);
                            throw new Exception("An error has been thrown");
                        });
                        h.RegisterRpcHandler<RcpRequestMesssage, MultiplicationResult>(message => new MultiplicationResult {Result = message.Number * 4});
                    });
                })
                .Build();

            logger.Info("Service bus (service) is ready to receive messages... Press 'q' to exit.");

            string key;
            do
            {
                key = Console.ReadLine() ?? "q";
            } while (key.ToLower() != "q");

            bus.Dispose();
        }
    }
}
using System;
using System.Configuration;
using System.Threading.Tasks;
using Examples.SharedLibrary;
using RabbitClient;

namespace Examples.Client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var logger = new ConsoleLogger();

            var bus = RabbitMqClientFactory.Create()
                .WithVirtualHost("/TEST")
                .WithClientName(Environment.MachineName + "_Client")
                .WithHost(ConfigurationManager.AppSettings["RabbitMqHost"])
                .WithDefaultConcurrencyLimit(3)
                .WithExceptionHandling(e => { logger.Fatal(e); })
                .Build();

            logger.Info("Service bus (client) is ready to send messages... Press 'q' to exit.");

            char key;
            string queue;
            do
            {
                logger.Info("\n\ta. Publish message (broadcast)");
                logger.Info("\tb. Publish message with expiration (broadcast)");
                logger.Info("\tc. Publish message (service throws exception)  (broadcast)");
                logger.Info("\td. Send message (direct message)");
                logger.Info("\te. Send message (direct message)");
                logger.Info("\tf. RPC message");
                logger.Info("\tg. Parallel messages");
                logger.Info("\th. Exists exchange");
                logger.Info("\ti. Exists queue");
                logger.Info("\tj. Purge queue");
                logger.Info("\tk. Delete exchange");
                logger.Info("\tl. Delete queue");
                logger.Info("\tm. Bind exchange");
                logger.Info("\tq. Quit");

                key = Console.ReadKey().KeyChar;
                Console.Clear();
                Console.WriteLine("Please enter the following input:" + Environment.NewLine);
                TimeSpan ttl;
                TestMessage message;

                switch (key)
                {
                    case 'a':
                        Console.Write("Message: ");
                        message = new TestMessage();
                        message.Message = Console.ReadLine();

                        logger.Info("Sending messageId {0} with message {1}", message.MessageId, message.Message);

                        bus.Publish(message);
                        break;

                    case 'b':
                        Console.Write("Message: ");
                        message = new TestMessage();
                        message.Message = Console.ReadLine();
                        Console.Write("Expiration (in ms): ");
                        ttl = TimeSpan.FromMilliseconds(double.Parse(Console.ReadLine() ?? "0"));

                        logger.Info("Sending messageId {0} with message {1}", message.MessageId, message.Message);

                        bus.Publish(message, ttl);
                        break;

                    case 'c':
                        Console.Write("Message: ");
                        var message2 = new TestMessageThrowsException();
                        message2.Message = Console.ReadLine();

                        logger.Info("Sending messageId {0} with message {1}. Will throw exception", message2.MessageId,
                            message2.Message);

                        bus.Publish(message2);
                        break;

                    case 'd':
                        Console.Write("Message: ");
                        message = new TestMessage();
                        message.Message = Console.ReadLine();

                        Console.Write("Target Queue: ");
                        queue = Console.ReadLine();

                        logger.Info("Sending messageId {0} with message {1}", message.MessageId, message.Message);

                        bus.Send(queue, message);
                        break;

                    case 'e':
                        Console.Write("Message: ");
                        message = new TestMessage();
                        message.Message = Console.ReadLine();
                        Console.Write("Expiration (in ms): ");
                        ttl = TimeSpan.FromMilliseconds(double.Parse(Console.ReadLine() ?? "0"));

                        Console.Write("Target Queue: ");
                        queue = Console.ReadLine();

                        logger.Info("Sending messageId {0} with message {1}", message.MessageId, message.Message);

                        bus.Send(queue, message, ttl);
                        break;

                    case 'f':
                        Console.Write("Number: ");
                        var rpcRequest = new RcpRequestMesssage();
                        rpcRequest.Number = int.Parse(Console.ReadLine() ?? "0");

                        Console.Write("Target Queue: ");
                        queue = Console.ReadLine();

                        logger.Info("Sending messageId {0} with number {1}", rpcRequest.MessageId, rpcRequest.Number);

                        Task.Factory.StartNew(async () =>
                        {
                            try
                            {
                                var result = await bus.RpcRequest<MultiplicationResult>(queue, rpcRequest);
                                logger.Warn("Result from RPC Request is: " + result.Result);
                            }
                            catch (Exception e)
                            {
                                logger.Error(e);
                            }
                        });
                        break;


                    case 'g':
                        Console.Write("Message: ");
                        message = new TestMessage();
                        message.Message = Console.ReadLine();

                        logger.Info("Sending messageId {0} with message {1}", message.MessageId, message.Message);

                        Parallel.For(0, 10, i =>
                        {
                            message.Message += " " + i;
                            bus.Publish(message);
                        });
                        break;

                    case 'h':
                        var exists = bus.ExistsExchange("Examples||SharedLibrary||TestMessage");
                        logger.Info($"Exchange exists: {exists}");
                        break;

                    case 'i':
                        var existsQueue = bus.ExistsQueue("TestQueue");
                        logger.Info($"Queue exists: {existsQueue}");
                        break;

                    case 'j':
                        var purged = bus.PurgeQueue("TestQueue");
                        logger.Info($"Queue purged: {purged}");
                        break;

                    case 'k':
                        var exchangeDeleted = bus.DeleteExchange("Examples||SharedLibrary||TestMessageThrowsException");
                        logger.Info($"Queue deleted: {exchangeDeleted}");
                        break;

                    case 'l':
                        var queueDeleted = bus.DeleteQueue("TestQueue");
                        logger.Info($"Queue deleted: {queueDeleted}");
                        break;

                    case 'm':

                        bus.BindExchange("fakeExchange", "fakeQueue", "fakeRoutingKey");
                        break;

                }
            } while (key != 'q');

            bus.Dispose();
        }
    }
}
# RabbitClient

Welcome to the RabbitClient project. This project has been built as a small wrapper for the RabbitMq C# package. If you have any issues with this project, please use the [Issue Tracker](https://github.com/armandoisaac/RabbitClient/issues).

## How to use this package

#### Installing the nuget package
The  RabbitClient project has it's own package that can be downloaded and installed from your project using the [Nuget package](https://www.nuget.org/packages/RabbitClient/).

To install RabbitClient, run the following command in the Package Manager Console:


    PM> Install-Package RabbitClient

#### Creating your messages
All messages that will be sent over the RabbitMq service bus must implement the IMessage interface located at the RabbitClient.Core.Messages namespace. You can use the the Message class as your base class.

    public class TestMessage : BusMessage
    {
	    public string Message { get; set; }
	    
	    public TestMessage() { }
	    
	    [JsonConstructor]
	    public TestMessage(Guid messageId, string message)
	    {
		    MessageId = messageId;
		    Message = message;
	    }
    }

Keep in mind that this message will be serialized before being sent via the message broker and will be deserialized when received by the target client.

#### Using the nuget package
The RabbitMqClientFactory will help you to create the RabbitMqClient object. 

    var factory = RabbitMqClientFactory.Create()

Using the Builder Pattern, you will be able to pass all the configuration options to the factory before creating the client. 

    // Set the rabbitmq host
    .WithHost(ConfigurationManager.AppSettings["RabbitMqHost"])

    // Set the virtual host
    .WithVirtualHost("/")

    // Set a name for this connection. By default will use the machine name
    .WithClientName(Environment.MachineName)
    
    // How many messages we will process at the same time
    .WithDefaultConcurrencyLimit(3)
    
    // Configure the "unhandled" exception handler
    .WithExceptionHandling(e => {  })

Once configured, you will be able to build the RabbitMqClient. This will return a RabbitMqClient object.

    var busClient = factory.Build();

#### Subscribe to a queue

You can configure your client to listen for messages from the broker. This step is not required as the client is capable of sending and publishing messages without the need to listen for messages. 

If your client needs to subscribe to a queue, you will be able to configure the client to listen to any amount of queues using the RabbitMqClientFactory.

     // Configure all the queue subscriptions
    .WithQueueSubscriptions(cfg =>
    {
	    // Configure an error handling for this specific queue
	    cfg.WithErrorHandling((message, error) =>
	    {
		    logger.Fatal(string.Format(
			    "An exception ocurred while processing message {0}. The error is: {1}",
			    message.MessageId, error));
	    });
	    
	    // Configure all the handlers for the queue
	    cfg.WithHandlers(h =>
	    {
		    h.Register<TestMessage>(message =>
		    {
			    logger.Info("Received messageId {0} with message {1}", message.MessageId,
			    message.Message);
		    });

		    h.Register<TestMessageThrowsException>(message =>
		    {
			    logger.Info("Received messageId {0} with message {1}. Will throw exception", message.MessageId, message.Message);
			    throw new Exception("An error has been thrown");
		    });

			// The client is capable to receive RPC requests
		    h.RegisterRpcHandler<RcpRequestMesssage, MultiplicationResult>(message =>
		    {
		    	return new MultiplicationResult {Result = message.Number * 4};
		    });
	    });
    })
 

#### Sending and publishing messages
The RabbitMqClient allows any user to publish and send messages to any client.

**Publishing** a message will send the message to an exchange where it will be distributed to all queues. This means that a message can be received by many clients and many handlers and can be processed multiple times.

    var message = new TestMessage();
    message.Message = "Hello World";
    busClient.Publish(message);

**Sending** a message will send your message to an specific queue. This means that you will need to specify the destination queue. This means that if multiple clients are listening the same queue, only one will be able to process the message at the same time.

    var message = new TestMessage();
    message.Message = "Hello World";
    busClient.Send("target_queue_name", message);

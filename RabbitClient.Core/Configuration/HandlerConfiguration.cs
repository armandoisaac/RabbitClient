using System;
using System.Collections.Generic;
using RabbitClient.Core.Messages;

namespace RabbitClient.Core.Configuration
{
    public class HandlerConfiguration
    {
        public HandlerConfiguration()
        {
            Handlers = new Dictionary<Type, Action<IBusMessage>>();
            RpcHandlers = new Dictionary<Type, Func<IBusMessage, object>>();
        }

        public Dictionary<Type, Action<IBusMessage>> Handlers { get; internal set; }
        public Dictionary<Type, Func<IBusMessage, object>> RpcHandlers { get; internal set; }

        public void Register<TMessage>(Action<TMessage> handler) where TMessage : IBusMessage
        {
            var type = typeof(TMessage);

            // Let's validate if we have the message type
            if (Handlers.ContainsKey(type))
                throw new InvalidHandlerConfigurationException("There is already a handler for message type " + type);

            Handlers[type] = msg => handler((TMessage) msg);
        }

        public void RegisterRpcHandler<TMessage, TResponse>(Func<TMessage, TResponse> handler)
            where TMessage : IBusMessage
        {
            var type = typeof(TMessage);

            // Let's validate if we have the message type
            if (RpcHandlers.ContainsKey(type))
                throw new InvalidHandlerConfigurationException(
                    "There is already a rpc handler for message type " + type);

            RpcHandlers[type] = msg => handler((TMessage) msg);
        }
    }
}
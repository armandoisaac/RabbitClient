using System;

namespace Diciannove.ServiceBus.RabbitMq
{
    internal static class NamingConventions
    {
        public static string ExchangeName(Type type)
        {
            return type.FullName.Replace(".", "||");
        }

        public static string GetDefaultQueueName(string clientName)
        {
            return string.Format("{0}||Requests", clientName);
        }

        //public static string GetDefaultRpcQueueName(string queueName)
        //{
        //    return string.Format("{0}||RpcRequest", queueName);
        //}

        public static string GetRpcResponseQueueName(string clientName)
        {
            return string.Format("{0}||RpcResponse", clientName);
        }
        public static string GetErrorQueueName(string clientName)
        {
            return string.Format("{0}||Errors", clientName);
        }
    }
}
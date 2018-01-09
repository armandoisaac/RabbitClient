using System;

namespace RabbitClient
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
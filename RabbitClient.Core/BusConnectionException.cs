using System;

namespace RabbitClient.Core
{
    [Serializable]
    public class BusConnectionException : Exception
    {
        public BusConnectionException()
        {
        }

        public BusConnectionException(string message) : base(message)
        {
        }

        public BusConnectionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
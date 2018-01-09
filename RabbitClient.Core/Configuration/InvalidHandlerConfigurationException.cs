using System;

namespace RabbitClient.Core.Configuration
{
    [Serializable]
    public class InvalidHandlerConfigurationException : Exception
    {
        public InvalidHandlerConfigurationException()
        {
        }

        public InvalidHandlerConfigurationException(string message) : base(message)
        {
        }

        public InvalidHandlerConfigurationException(string message, Exception innerException) : base(message,
            innerException)
        {
        }
    }
}
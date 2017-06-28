using System;
using System.Runtime.Serialization;

namespace Diciannove.ServiceBus
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
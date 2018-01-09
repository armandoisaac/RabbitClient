using System.Collections.Generic;

namespace RabbitClient.Comparers
{
    internal class ExchangeComparer : IEqualityComparer<RabbitMqExchange>
    {
        public bool Equals(RabbitMqExchange x, RabbitMqExchange y)
        {
            return x?.Name == y?.Name;
        }

        public int GetHashCode(RabbitMqExchange obj)
        {
            return obj.GetHashCode();
        }
    }
}
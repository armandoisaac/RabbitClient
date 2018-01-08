using System.Collections.Generic;

namespace RabbitClient.Comparers
{
    internal class QueueComparer : IEqualityComparer<RabbitMqQueueConfiguration>
    {
        public bool Equals(RabbitMqQueueConfiguration x, RabbitMqQueueConfiguration y)
        {
            return x?.QueueName == y?.QueueName;
        }

        public int GetHashCode(RabbitMqQueueConfiguration obj)
        {
            return obj.GetHashCode();
        }
    }
}
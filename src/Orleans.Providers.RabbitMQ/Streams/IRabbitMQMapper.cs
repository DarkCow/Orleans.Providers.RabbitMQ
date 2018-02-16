using Orleans.Streams;
using System.Collections.Generic;

namespace Orleans.Providers.RabbitMQ.Streams
{
    public interface IRabbitMQMapper
    {
        void Init();
        T MapToType<T>(byte[] message);
        IEnumerable<string> GetPartitionKeys(QueueId queueId, int numQueues);
        string GetPartitionName(string queue, QueueId queueId);
    }
}

using Microsoft.Extensions.Logging;
using Orleans.Runtime.Configuration;
using Orleans.Streams;
using System.Collections.Generic;

namespace Orleans.Providers.RabbitMQ.Streams
{
    public class RabbitMQDefaultMapper : IRabbitMQMapper
    {
        private ILogger _logger;
        private IMessageSerializationHandler _serializationHandler;

        public RabbitMQDefaultMapper(ILoggerFactory loggerFactory, IMessageSerializationHandler serializationHandler)
        {
            _logger = loggerFactory.CreateLogger(nameof(RabbitMQDefaultMapper));
            _serializationHandler = serializationHandler;
        }

        public void Init() { }


        public T MapToType<T>(byte[] message)
        {
            if (message is T)
                return (T)(object)message;

            return _serializationHandler.DeserializeMessage<T>(message);
        }

        public IEnumerable<string> GetPartitionKeys(QueueId queueId, int numQueues)
        {
            return new string[] { "#" };
        }

        public string GetPartitionName(string queue, QueueId queueId)
        {
            return queue;
        }
    }
}

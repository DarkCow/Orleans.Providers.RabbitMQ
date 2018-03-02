using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Providers.Streams.Common;
using Orleans.Runtime.Configuration;
using Orleans.Streams;
using System;
using System.Threading.Tasks;

namespace Orleans.Providers.RabbitMQ.Streams
{
    public class RabbitMQAdapterFactory<TMapper> : IQueueAdapterFactory where TMapper : IRabbitMQMapper
    {
        private SimpleQueueAdapterCache _adapterCache;
        private int _cacheSize;
        private RabbitMQStreamProviderOptions _config;
        private IRabbitMQMapper _mapper;
        private ILoggerFactory _loggeFactory;
        private IMessageSerializationHandler _serializationHandler;
        private string _providerName;
        private IStreamQueueMapper _streamQueueMapper;

        protected Func<QueueId, Task<IStreamFailureHandler>> StreamFailureHandlerFactory { private get; set; }

        public RabbitMQAdapterFactory(string providerName, IServiceProvider serviceProvider)
        {
            Init(providerName, serviceProvider);
        }

        public void Init(string providerName, IServiceProvider serviceProvider)
        {
            _config = serviceProvider.GetService<RabbitMQStreamProviderOptions>();

            _providerName = providerName;
            _loggeFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            _serializationHandler = _config.MessageSerializationHandler; // serviceProvider.GetRequiredService<IMessageSerializationHandler>();

            _mapper = serviceProvider.GetRequiredService<IRabbitMQMapper>();
            _mapper.Init();

            _cacheSize = 4096;// _config.SimpleQueueAdapterCache.ParseSize(config, 4096);
            _adapterCache = new SimpleQueueAdapterCache(_cacheSize, providerName, _loggeFactory);

            _streamQueueMapper = new HashRingBasedStreamQueueMapper(_config.NumberOfQueues, _providerName);

            if (StreamFailureHandlerFactory == null)
            {
                StreamFailureHandlerFactory =
                    qid => Task.FromResult<IStreamFailureHandler>(new NoOpStreamDeliveryFailureHandler(false));
            }
        }

        public Task<IQueueAdapter> CreateAdapter()
        {
            IQueueAdapter adapter = new RabbitMQAdapter(_config, _loggeFactory, _providerName, _streamQueueMapper, _mapper, _serializationHandler);
            return Task.FromResult(adapter);
        }

        public Task<IStreamFailureHandler> GetDeliveryFailureHandler(QueueId queueId)
        {
            return StreamFailureHandlerFactory(queueId);
        }

        public IQueueAdapterCache GetQueueAdapterCache()
        {
            return _adapterCache;
        }

        public IStreamQueueMapper GetStreamQueueMapper()
        {
            return _streamQueueMapper;
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Configuration.Overrides;
using Orleans.Providers.Streams.Common;
using Orleans.Runtime.Configuration;
using Orleans.Serialization;
using Orleans.Streams;
using System;
using System.Threading.Tasks;

namespace Orleans.Providers.RabbitMQ.Streams
{
    public class RabbitMQAdapterFactory : IQueueAdapterFactory
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

        public RabbitMQAdapterFactory(
            string name,
            RabbitMQStreamProviderOptions config,
            HashRingStreamQueueMapperOptions queueMapperOptions,
            SimpleQueueCacheOptions cacheOptions,
            IServiceProvider serviceProvider,
            IOptions<ClusterOptions> clusterOptions,
            SerializationManager serializationManager,
            ILoggerFactory loggerFactory,
            IMessageSerializationHandler serializationHandler,
            IRabbitMQMapper mapper )

        {
            _config = config;

            _providerName = name;
            _loggeFactory = loggerFactory;
            _serializationHandler = serializationHandler;

            _mapper = mapper;
            _mapper.Init( );

            _cacheSize = cacheOptions.CacheSize;
            _adapterCache = new SimpleQueueAdapterCache( cacheOptions, _providerName, _loggeFactory );

            _streamQueueMapper = new HashRingBasedStreamQueueMapper( queueMapperOptions, _providerName );
        }

        public void Init( )
        {
            if( StreamFailureHandlerFactory == null )
            {
                StreamFailureHandlerFactory =
                    qid => Task.FromResult<IStreamFailureHandler>( new NoOpStreamDeliveryFailureHandler( false ) );
            }
        }

        public Task<IQueueAdapter> CreateAdapter( )
        {
            IQueueAdapter adapter = new RabbitMQAdapter( _config, _loggeFactory, _providerName, _streamQueueMapper, _mapper, _serializationHandler );
            return Task.FromResult( adapter );
        }

        public Task<IStreamFailureHandler> GetDeliveryFailureHandler( QueueId queueId )
        {
            return StreamFailureHandlerFactory( queueId );
        }

        public IQueueAdapterCache GetQueueAdapterCache( )
        {
            return _adapterCache;
        }

        public IStreamQueueMapper GetStreamQueueMapper( )
        {
            return _streamQueueMapper;
        }

        public static IQueueAdapterFactory Create( IServiceProvider services, string name )
        {
            var rabbitMQOptions = services.GetOptionsByName<RabbitMQStreamProviderOptions>( name );
            var cacheOptions = services.GetOptionsByName<SimpleQueueCacheOptions>( name );
            var queueMapperOptions = services.GetOptionsByName<HashRingStreamQueueMapperOptions>( name );
            IOptions<ClusterOptions> clusterOptions = services.GetProviderClusterOptions( name );
            var factory = ActivatorUtilities.CreateInstance<RabbitMQAdapterFactory>( services, name, rabbitMQOptions, cacheOptions, queueMapperOptions, clusterOptions );
            factory.Init( );
            return factory;
        }
    }
}

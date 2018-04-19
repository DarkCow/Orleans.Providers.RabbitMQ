using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Streams;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Orleans.Runtime.Configuration;

namespace Orleans.Providers.RabbitMQ.Streams
{
    public class SiloRabbitMQStreamConfigurator : SiloPersistentStreamConfigurator
    {
        public SiloRabbitMQStreamConfigurator( string name, ISiloHostBuilder builder )
            : base( name, builder, RabbitMQAdapterFactory.Create )
        {
            this.siloBuilder
                .ConfigureApplicationParts( parts => parts.AddFrameworkPart( typeof( RabbitMQAdapterFactory ).Assembly ) )
                .ConfigureServices( services =>
                {
                    services
                        .AddSingleton<IMessageSerializationHandler, DefaultSerializationHandler>( )
                        .AddSingleton<IRabbitMQMapper, RabbitMQDefaultMapper>( )
                        .ConfigureNamedOptionForLogging<RabbitMQStreamProviderOptions>( name )
                        .ConfigureNamedOptionForLogging<SimpleQueueCacheOptions>( name )
                        .ConfigureNamedOptionForLogging<HashRingStreamQueueMapperOptions>( name );
                } );
        }

        public SiloRabbitMQStreamConfigurator ConfigureRabbitMQ( Action<OptionsBuilder<RabbitMQStreamProviderOptions>> configureOptions )
        {
            this.Configure<RabbitMQStreamProviderOptions>( configureOptions );
            return this;
        }
        public SiloRabbitMQStreamConfigurator ConfigureCache( int cacheSize = SimpleQueueCacheOptions.DEFAULT_CACHE_SIZE )
        {
            this.Configure<SimpleQueueCacheOptions>( ob => ob.Configure( options => options.CacheSize = cacheSize ) );
            return this;
        }

        public SiloRabbitMQStreamConfigurator ConfigurePartitioning( int numOfparitions = HashRingStreamQueueMapperOptions.DEFAULT_NUM_QUEUES )
        {
            this.Configure<HashRingStreamQueueMapperOptions>( ob => ob.Configure( options => options.TotalQueueCount = numOfparitions ) );
            return this;
        }
    }

    public class ClusterClientRabbitMQStreamConfigurator : ClusterClientPersistentStreamConfigurator
    {
        public ClusterClientRabbitMQStreamConfigurator( string name, IClientBuilder builder )
            : base( name, builder, RabbitMQAdapterFactory.Create )
        {
            this.clientBuilder
                .ConfigureApplicationParts( parts => parts.AddFrameworkPart( typeof( RabbitMQAdapterFactory ).Assembly ) )
                .ConfigureServices( services =>
                {
                    services
                        .AddSingleton<IMessageSerializationHandler, DefaultSerializationHandler>( )
                        .AddSingleton<IRabbitMQMapper, RabbitMQDefaultMapper>( )
                        .ConfigureNamedOptionForLogging<RabbitMQStreamProviderOptions>( name )
                        .ConfigureNamedOptionForLogging<HashRingStreamQueueMapperOptions>( name );
                } );
        }

        public ClusterClientRabbitMQStreamConfigurator ConfigureRabbitMQ( Action<OptionsBuilder<RabbitMQStreamProviderOptions>> configureOptions )
        {
            this.Configure<RabbitMQStreamProviderOptions>( configureOptions );
            return this;
        }

        public ClusterClientRabbitMQStreamConfigurator ConfigurePartitioning( int numOfparitions = HashRingStreamQueueMapperOptions.DEFAULT_NUM_QUEUES )
        {
            this.Configure<HashRingStreamQueueMapperOptions>( ob => ob.Configure( options => options.TotalQueueCount = numOfparitions ) );
            return this;
        }
    }
}

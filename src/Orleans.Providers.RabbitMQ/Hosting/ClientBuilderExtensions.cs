using System;
using Orleans.Configuration;
using Orleans.Providers.RabbitMQ.Streams;
using Orleans.Streams;

namespace Orleans.Hosting
{
    public static class ClientBuilderExtensions
    {   /// <summary>
        /// Configure cluster client to use RabbitMQ with default settings
        /// </summary>
        public static IClientBuilder AddRabbitMQStreams( this IClientBuilder builder, string name, Action<RabbitMQStreamProviderOptions> configureOptions )
        {
            builder.AddRabbitMQStreams( name, b =>
                 b.ConfigureRabbitMQ( ob => ob.Configure( configureOptions ) ) );
            return builder;
        }

        /// <summary>
        /// Configure cluster client to use RabbitMQ persistent streams.
        /// </summary>
        public static IClientBuilder AddRabbitMQStreams( this IClientBuilder builder, string name, Action<ClusterClientRabbitMQStreamConfigurator> configure )
        {
            var configurator = new ClusterClientRabbitMQStreamConfigurator( name, builder );
            configure?.Invoke( configurator );
            return builder;
        }
    }
}

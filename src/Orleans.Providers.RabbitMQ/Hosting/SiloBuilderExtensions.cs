using System;
using Orleans.Configuration;
using Orleans.Providers.RabbitMQ.Streams;
using Orleans.Streams;

namespace Orleans.Hosting
{
    public static class SiloBuilderExtensions
    {

        /// <summary>
        /// Configure silo to use RabbitMQ persistent streams.
        /// </summary>
        public static ISiloHostBuilder AddRabbitMQStreams( this ISiloHostBuilder builder, string name, Action<RabbitMQStreamProviderOptions> configureOptions )
        {
            builder.AddRabbitMQStreams( name, b =>
                 b.ConfigureRabbitMQ( ob => ob.Configure( configureOptions ) ) );
            return builder;
        }

        /// <summary>
        /// Configure silo to use RabbitMQ persistent streams.
        /// </summary>
        public static ISiloHostBuilder AddRabbitMQStreams( this ISiloHostBuilder builder, string name, Action<SiloRabbitMQStreamConfigurator> configure )
        {
            var configurator = new SiloRabbitMQStreamConfigurator( name, builder );
            configure?.Invoke( configurator );
            return builder;
        }
    }
}
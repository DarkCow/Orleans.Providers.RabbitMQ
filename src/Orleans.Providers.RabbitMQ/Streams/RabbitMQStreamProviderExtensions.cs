using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers.RabbitMQ.Streams;
using System;

namespace Orleans.Runtime.Configuration
{
    public static class RabbitMQStreamProviderExtensions
    {
        public static ISiloHostBuilder ConfigureRabbitMQStreamProvider(this ISiloHostBuilder builder, string name, Action<RabbitMQStreamProviderOptions> configureOptions)
        {
            return builder.ConfigureRabbitMQStreamProviderWithOptions(name, ob => ob.Configure(configureOptions));
        }

        public static ISiloHostBuilder ConfigureRabbitMQStreamProviderWithOptions(this ISiloHostBuilder builder, string name, Action<OptionsBuilder<RabbitMQStreamProviderOptions>> configureOptions)
        {
            return builder.ConfigureServices(services => services.AddRabbitMQStreamProvider(name, configureOptions));
        }

        public static IClientBuilder ConfigureRabbitMQStreamProvider(this IClientBuilder builder, string name, Action<RabbitMQStreamProviderOptions> configureOptions)
        {
            return builder.ConfigureRabbitMQStreamProviderWithOptions(name, ob => ob.Configure(configureOptions));
        }

        public static IClientBuilder ConfigureRabbitMQStreamProviderWithOptions(this IClientBuilder builder, string name, Action<OptionsBuilder<RabbitMQStreamProviderOptions>> configureOptions)
        {
            return builder.ConfigureServices(services => services.AddRabbitMQStreamProvider(name, configureOptions));
        }

        public static IServiceCollection AddRabbitMQStreamProvider(this IServiceCollection services, string name, Action<OptionsBuilder<RabbitMQStreamProviderOptions>> configureOptions = null)
        {
            var optionsBuilder = services.AddOptions<RabbitMQStreamProviderOptions>();
            configureOptions?.Invoke(optionsBuilder);

            services.AddSingleton<RabbitMQStreamProviderOptions>(provider =>
            {
                var options = provider.GetService<IOptionsSnapshot<RabbitMQStreamProviderOptions>>().Value;
                options.MessageSerializationHandler = options.MessageSerializationHandler ?? new DefaultSerializationHandler();
                return options;
            })
            .AddSingleton<IRabbitMQMapper>(provider =>
             {
                 var options = provider.GetService<IOptionsSnapshot<RabbitMQStreamProviderOptions>>().Value;
                 return new RabbitMQDefaultMapper(provider.GetRequiredService<ILoggerFactory>(), options.MessageSerializationHandler);

             })
            .AddSiloQueueStreams<RabbitMQDefaultMapper>(name);

            return services;
        }
        public static IServiceCollection AddSiloQueueStreams<TDataAdapter>(this IServiceCollection services, string name)
         where TDataAdapter : IRabbitMQMapper
        {
            return services
                  .AddSiloPersistentStreams<RabbitMQStreamProviderOptions>(name, (serv, s) =>
                  {
                      return new RabbitMQAdapterFactory<TDataAdapter>(name, serv);
                  });
        }

    }

}

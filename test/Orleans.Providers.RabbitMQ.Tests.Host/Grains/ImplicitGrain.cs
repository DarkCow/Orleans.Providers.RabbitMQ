using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Providers.RabbitMQ.Tests.Host.Interfaces;
using Orleans.Runtime;
using Orleans.Streams;
using System;
using System.Threading.Tasks;

namespace Orleans.Providers.RabbitMQ.Tests.Host.Grains
{
    [ImplicitStreamSubscription("TestNamespace")]
    public class ImplicitGrain : Grain, IImplicitGrain, IAsyncObserver<string>
    {
        private StreamSubscriptionHandle<string> _subscription;
        private ILogger _logger;


        public async override Task OnActivateAsync()
        {
            var provider = GetStreamProvider("Default");
            var stream = provider.GetStream<string>(this.GetPrimaryKey(), "TestNamespace");
            _logger = this.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(this.GetType().FullName);
            _subscription = await stream.SubscribeAsync(this);
        }

        public Task OnCompletedAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            return Task.CompletedTask;
        }

        public Task OnNextAsync(string item, StreamSequenceToken token = null)
        {
            GetLogger().Info("Received message '{0}'!", item);
            return Task.CompletedTask;
        }
        private ILogger GetLogger() => _logger;
    }
}

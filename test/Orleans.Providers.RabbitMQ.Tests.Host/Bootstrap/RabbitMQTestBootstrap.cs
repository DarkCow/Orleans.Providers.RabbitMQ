using Orleans.Providers.RabbitMQ.Tests.Host.Interfaces;
using Orleans.Runtime;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Orleans.Providers.RabbitMQ.Test.Host.Bootstrap
{
    public class RabbitMQTestBootstrap : IStartupTask
    {
        private readonly IProviderRuntime _providerRuntime;

        public RabbitMQTestBootstrap(IProviderRuntime providerRuntime)
        {
            _providerRuntime = providerRuntime;
        }
        public string Name { get; set; }

        public Task Close()
        {
            return Task.CompletedTask;
        }

        public Task Execute(CancellationToken cancellationToken)
        {
            return Init(_providerRuntime);
        }

        public async Task Init(IProviderRuntime providerRuntime)
        {
            await providerRuntime.GrainFactory.GetGrain<IProducerGrain>(Guid.Empty).Simulate();
        }


    }
}

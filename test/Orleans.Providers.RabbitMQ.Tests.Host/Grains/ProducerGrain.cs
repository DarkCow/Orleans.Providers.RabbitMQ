﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Providers.RabbitMQ.Tests.Host.Interfaces;
using Orleans.Runtime;
using Orleans.Streams;
using System;
using System.Threading.Tasks;

namespace Orleans.Providers.RabbitMQ.Tests.Host.Grains
{
    public class ProducerGrain : Grain, IProducerGrain
    {
        private int _counter;
        private IAsyncStream<string> _stream;
        private ILogger _logger;
        public override Task OnActivateAsync()
        {
            var provider = GetStreamProvider("Default");
            _stream = provider.GetStream<string>(this.GetPrimaryKey(), "TestNamespace");
            _logger = this.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(this.GetType().FullName);
            return Task.CompletedTask;
        }

        public Task Simulate()
        {
            RegisterTimer(OnSimulationTick, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
            return Task.CompletedTask;
        }

        public async Task Tick()
        {
            await OnSimulationTick(null);
        }

        private async Task OnSimulationTick(object state)
        {
            await SendMessages(_counter++.ToString());
            await SendMessages(_counter++.ToString(), _counter++.ToString());
        }

        private async Task SendMessages(params string[] messages)
        {
            GetLogger().Info("Sending message{0} '{1}'...",
            messages.Length > 1 ? "s" : "", string.Join(",", messages));

            if (messages.Length == 1)
            {
                await _stream.OnNextAsync(messages[0]);
                return;
            }
            await _stream.OnNextBatchAsync(messages);
        }
        private ILogger GetLogger() => _logger;

    }
}

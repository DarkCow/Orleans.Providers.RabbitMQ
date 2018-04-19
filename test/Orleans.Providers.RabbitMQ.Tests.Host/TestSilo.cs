using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers.RabbitMQ.Streams;
using Orleans.Providers.RabbitMQ.Test.Host.Bootstrap;
using Orleans.Providers.RabbitMQ.Tests.Host.Grains;
using Orleans.Runtime.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Orleans.Providers.RabbitMQ.Tests.Host
{
    public static class TestSilo
    {
        private static IPAddress _siloIP;
        private static ISiloHost _siloHost;
        private static string _hostName;
        private const string ADO_INVARIANT = "System.Data.SqlClient";

        public static async Task<int> StopSilo()
        {
            try
            {
                await _siloHost.StopAsync();
                return 0;
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
                Console.WriteLine("Failure stopping silo.");
                return 1;
            }
        }

        public static async Task StartSilo()
        {
            try
            {
                _hostName = Dns.GetHostName();
                var ips = await Dns.GetHostAddressesAsync(_hostName);
                _siloIP = ips.FirstOrDefault();

                var configRoot = GetConfiguration();
                var builder = new SiloHostBuilder()
                    .AddRabbitMQStreams("Default", config =>
                    {
                        config.Exchange = "exchange";
                        config.HostName = "localhost";
                        config.Namespace = "TestNamespace";
                        config.Password = "guest";
                        config.Port = 5671;
                        config.Queue = "queue";
                        config.RoutingKey = "#";
                        config.Username = "guest";
                        config.VirtualHost = "/";
                    } )
                    .WithParts()
                    .UseLocalhostClustering()
                    .ConfigureLogging(logging =>
                    {
                        logging.AddConfiguration(configRoot.GetSection("Logging")).AddConsole();
                    })
                    .AddMemoryGrainStorageAsDefault()
                    .AddMemoryGrainStorage("PubSubStore")
                    .AddStartupTask<RabbitMQTestBootstrap>();

                _siloHost = builder.Build();
                await _siloHost.StartAsync();

            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
                Console.WriteLine("Failure Starting silo.");
                throw exc;
            }
        }


        private static ISiloHostBuilder WithParts(this ISiloHostBuilder builder)
        {
            return builder
                .ConfigureApplicationParts(mgr => mgr.AddApplicationPart(typeof(ImplicitGrain).Assembly).WithReferences());
        }

        private static IConfigurationRoot GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
#if DEBUG
                .AddJsonFile($"appsettings.Development.json", optional: true)
#endif
                ;

            return builder.Build();
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SampleStack.Redis.PubSub;
using StackExchange.Redis;

namespace SampleStack.Redis
{
    public static class HostBuilderExtensions
    {
        private const string RedisConnectionString = "redis:6379";

        public static IHostBuilder ConfigureRedisServices(this IHostBuilder builder)
        {
            return builder.ConfigureServices((_, services) =>
            {
                services.AddSingleton<IConnectionMultiplexer>(cfg =>
                {
                    var configurationOptions = new ConfigurationOptions
                    {
                        AbortOnConnectFail = false,
                        ConnectTimeout = 5000,
                        EndPoints = { RedisConnectionString },
                    };

                    return ConnectionMultiplexer.Connect(configurationOptions);
                });

                services.AddSingleton<PublisherService>()
                        .AddSingleton<SubscriberService>()
                        .AddSingleton<NumbersGenerator>()
                        .AddSingleton<NumbersProcessor>();

                services.AddSingleton<IRedisService>(provider =>
                {
                    string mode = Environment.GetEnvironmentVariable("MODE") ?? "SUB";
                    return mode == "SUB"
                        ? provider.GetRequiredService<SubscriberService>()
                        : provider.GetRequiredService<PublisherService>();
                });
            });
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SampleStack.Redis.Numbers.Messaging;
using SampleStack.Redis.Numbers.Services;
using SampleStack.Redis.PubSub;
using StackExchange.Redis;

namespace SampleStack.Redis.Configuration
{
    public static class HostBuilderExtensions
    {
        private static readonly string RedisConnectionString = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost:6379";
        private static readonly string PubSubMode = Environment.GetEnvironmentVariable("MODE") ?? "SUB";

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
                        LoggerFactory = cfg.GetService<ILoggerFactory>()
                    };

                    return ConnectionMultiplexer.Connect(configurationOptions);
                });

                services.AddSingleton<NumbersPublisher>()
                        .AddSingleton<NumbersSubscriber>()
                        .AddSingleton<NumbersGenerator>()
                        .AddSingleton<NumbersProcessor>();

                services.AddSingleton<IRedisService>(provider => PubSubMode == "SUB"
                    ? provider.GetRequiredService<NumbersSubscriber>()
                    : provider.GetRequiredService<NumbersPublisher>());
            });
        }
    }
}

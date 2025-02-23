using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SampleStack.Redis.Configuration;
using SampleStack.Redis.PubSub;

namespace SampleStack.Redis
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args).ConfigureRedisServices().Build();

            var exitEvent = new ManualResetEventSlim(false);

            var service = host.Services.GetRequiredService<IRedisService>();

            service.CacheDisconnected += (sender, e) =>
            {
                Console.WriteLine("Redis connection failed.");
                exitEvent.Set();
            };

            service.CacheReConnected += (sender, e) =>
            {
                Console.WriteLine("Redis connection restored.");
            };

            service?.StartAsync().Wait();

            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("Shutting down...");
                exitEvent.Set();
                e.Cancel = true;
            };

            exitEvent.Wait();
        }
    }
}
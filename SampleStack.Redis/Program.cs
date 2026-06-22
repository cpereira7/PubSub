using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SampleStack.Redis.Configuration;
using SampleStack.Redis.PubSub;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureRedisServices()
    .ConfigureServices(services => 
    {
        services.AddHostedService<RedisWorker>();
    })
    .Build();

await host.RunAsync();

internal class RedisWorker(IRedisService service) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        service.CacheDisconnected += (sender, e) => Console.WriteLine("Redis connection failed.");
        service.CacheReConnected += (sender, e) => Console.WriteLine("Redis connection restored.");

        await service.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Shutting down...");
        await service.StopAsync();
    }
}

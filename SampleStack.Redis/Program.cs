using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SampleStack.Redis.Configuration;
using SampleStack.Redis.PubSub;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureRedisServices()
    .Build();

var exitEvent = new ManualResetEventSlim(false);
var cts = new CancellationTokenSource();

var service = host.Services.GetRequiredService<IRedisService>();

service.CacheDisconnected += (sender, e) =>
{
    Console.WriteLine("Redis connection failed.");
};

service.CacheReConnected += (sender, e) =>
{
    Console.WriteLine("Redis connection restored.");
};

await service.StartAsync(cts.Token);

Console.CancelKeyPress += (sender, e) =>
{
    Console.WriteLine("Shutting down...");
    cts.Cancel();
    exitEvent.Set();
    e.Cancel = true;
};

exitEvent.Wait();
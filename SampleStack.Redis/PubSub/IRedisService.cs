namespace SampleStack.Redis.PubSub;

internal interface IRedisService
{
    bool IsConnected { get; }
    event EventHandler? CacheDisconnected;
    event EventHandler? CacheReConnected;
        
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync();
}
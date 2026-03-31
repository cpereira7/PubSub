using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace SampleStack.Redis.PubSub;

internal abstract class PubSubServiceBase<T> : IRedisService
{
    protected readonly IConnectionMultiplexer ConnectionMultiplexer;
    protected readonly ILogger<T> Logger;
    
    protected PubSubServiceBase(IConnectionMultiplexer connectionMultiplexer, ILogger<T> logger)
    {
        ConnectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ConnectionMultiplexer.ConnectionRestored += OnRedisConnectionRestored;
        ConnectionMultiplexer.ConnectionFailed += OnRedisConnectionFailed;
    }

    public event EventHandler? CacheDisconnected;
    public event EventHandler? CacheReConnected;
    
    public bool IsConnected => ConnectionMultiplexer.IsConnected;
    
    private void OnRedisConnectionRestored(object? sender, ConnectionFailedEventArgs e)
    {
        if (e.ConnectionType != ConnectionType.Subscription) 
            return;
        
        Logger.LogInformation("Redis subscription connection restored");

        CacheReConnected?.Invoke(this, EventArgs.Empty);
    }

    private void OnRedisConnectionFailed(object? sender, ConnectionFailedEventArgs e)
    {
        if (e.ConnectionType == ConnectionType.Subscription)
            CacheDisconnected?.Invoke(this, EventArgs.Empty);
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Starting {ServiceName}...", GetType().Name);
                    
        await OnStartAsync(cancellationToken);
    }

    public async Task StopAsync()
    {
        Logger.LogInformation("Stopping {ServiceName}...", GetType().Name);
        
        await OnStopAsync();
    }

    protected virtual Task OnStopAsync() => Task.CompletedTask;
    protected abstract Task OnStartAsync(CancellationToken cancellationToken);
}

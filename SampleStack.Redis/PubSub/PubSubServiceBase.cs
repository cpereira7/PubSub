using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace SampleStack.Redis.PubSub;

internal abstract class PubSubServiceBase : IRedisService
{
    protected readonly IConnectionMultiplexer ConnectionMultiplexer;
    protected readonly ILogger<PubSubServiceBase> Logger;

    protected PubSubServiceBase(IConnectionMultiplexer connectionMultiplexer, ILogger<PubSubServiceBase> logger)
    {
        ConnectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ConnectionMultiplexer.ConnectionRestored += OnRedisConnectionRestored;
        ConnectionMultiplexer.ConnectionFailed += OnRedisConnectionFailed;
    }

    public event EventHandler? CacheDisconnected;
    public event EventHandler? CacheReConnected;
        
    private bool ChannelsSubscribed { get; set; }
    public bool IsRunning => ChannelsSubscribed;
        
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
        
        ChannelsSubscribed = true;
    }

    public async Task StopAsync()
    {
        Logger.LogInformation("Stopping {ServiceName}...", GetType().Name);
        
        var subscriber = ConnectionMultiplexer.GetSubscriber();

        await subscriber.UnsubscribeAllAsync();

        ChannelsSubscribed = false;
    }

    protected abstract Task OnStartAsync(CancellationToken cancellationToken);
}
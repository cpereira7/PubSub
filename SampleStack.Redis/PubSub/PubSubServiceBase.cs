using System.Text.Json;
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

    protected PubSubServiceBase()
    {
        throw new NotImplementedException();
    }

    public event EventHandler? CacheDisconnected;
    public event EventHandler? CacheReConnected;
        
    private bool ChannelsSubscribed { get; set; }
    public bool IsRunning => ChannelsSubscribed;
        
    private void OnRedisConnectionRestored(object? sender, ConnectionFailedEventArgs e)
    {
        if (e.ConnectionType != ConnectionType.Subscription) 
            return;
            
        if (!ChannelsSubscribed)
            SubscribeToChannels();

        CacheReConnected?.Invoke(this, EventArgs.Empty);
    }

    private void OnRedisConnectionFailed(object? sender, ConnectionFailedEventArgs e)
    {
        if (e.ConnectionType == ConnectionType.Subscription)
            CacheDisconnected?.Invoke(this, EventArgs.Empty);
    }

    private void SubscribeToChannels()
    {
        ChannelsSubscribed = true;
    }

    public async Task StartAsync()
    {
        SubscribeToChannels();
            
        await OnStartAsync();
    }

    public async Task StopAsync()
    {
        var subscriber = ConnectionMultiplexer.GetSubscriber();

        await subscriber.UnsubscribeAllAsync();

        ChannelsSubscribed = false;
    }

    public abstract Task OnStartAsync();
}
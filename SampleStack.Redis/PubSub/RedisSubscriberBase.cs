using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace SampleStack.Redis.PubSub;

internal abstract class RedisSubscriberBase<T> : PubSubServiceBase<T>, IRedisSubscriber
{
    private readonly ISubscriber _subscriber;
    protected RedisSubscriberBase(IConnectionMultiplexer connectionMultiplexer, ILogger<T> logger)
        : base(connectionMultiplexer, logger)
    {
        _subscriber = ConnectionMultiplexer.GetSubscriber();
    }
    
    public async Task SubscribeAsync<TMessage>(string channel, Func<TMessage, Task> handler)
    {
        var subChannel = new RedisChannel(channel, RedisChannel.PatternMode.Literal);
        
        // Async void is generally discouraged, but in this case it's acceptable as event-handler callback.
        // Exceptions are caught and logged to prevent unhandled exceptions from crashing the application.
        await _subscriber.SubscribeAsync(subChannel,  async void (_, message) =>
        {
            try
            {
                var deserialized = JsonSerializer.Deserialize<TMessage>(message!);
                if (deserialized != null)
                    await handler(deserialized);
            }
            catch (JsonException ex)
            {
                Logger.LogError(ex, "Failed to deserialize message on {Channel}", channel);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unhandled error in handler on {Channel}", channel);
            }
        });
    }

    public async Task SubscribeRawAsync(string channel, Func<RedisValue, Task> handler)
    {
        var subChannel = new RedisChannel(channel, RedisChannel.PatternMode.Literal);
        
        await _subscriber.SubscribeAsync(subChannel, async void(_, message) =>
        {
            try
            {
                await handler(message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unhandled error in raw handler on {Channel}", channel);
            }
        });
    }

    protected override async Task OnStopAsync()
    {
        // Note: Unsubscribing from all channels is a simple way to stop receiving messages, but it may not be suitable for all scenarios.
        // If the multiplexer is shared across multiple services, consider implementing a more targeted unsubscription strategy.
        await _subscriber.UnsubscribeAllAsync();
    }
}
using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace SampleStack.Redis.PubSub;

internal abstract class RedisSubscriberBase : PubSubServiceBase, IRedisSubscriber
{
    protected RedisSubscriberBase(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisSubscriberBase> logger)
        : base(connectionMultiplexer, logger)
    {
    }
    
    public async Task SubscribeAsync<T>(string channel, Func<T, Task> handler)
    {
        var sub = ConnectionMultiplexer.GetSubscriber();
        var subChannel = new RedisChannel(channel, RedisChannel.PatternMode.Literal);
        
        await sub.SubscribeAsync(subChannel, (_, message) =>
        {
            try
            {
                var deserialized = JsonSerializer.Deserialize<T>(message!);
                if (deserialized != null)
                    handler(deserialized);
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
        var sub = ConnectionMultiplexer.GetSubscriber();
        var subChannel = new RedisChannel(channel, RedisChannel.PatternMode.Literal);
        
        await sub.SubscribeAsync(subChannel, (_, message) =>
        {
            try
            {
                handler(message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unhandled error in raw handler on {Channel}", channel);
            }
        });
    }
}
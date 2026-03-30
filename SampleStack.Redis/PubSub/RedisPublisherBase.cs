using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace SampleStack.Redis.PubSub;

internal abstract class RedisPublisherBase : PubSubServiceBase, IRedisPublisher
{
    protected RedisPublisherBase(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisPublisherBase> logger)
        : base(connectionMultiplexer, logger)
    {
    }
    
    public async Task PublishAsync<T>(string channel, T message)
    {
        try
        {
            var pub = ConnectionMultiplexer.GetSubscriber();
            var pubChannel = new RedisChannel(channel, RedisChannel.PatternMode.Literal);
            
            var json = JsonSerializer.Serialize(message);
            
            await pub.PublishAsync(pubChannel, json);
        }
        catch (JsonException ex)
        {
            Logger.LogError(ex, "Failed to serialize message for channel {Channel}", channel);
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to publish message on channel {Channel}", channel);
            throw;
        }
    }

    public async Task PublishRawAsync(string channel, string message)
    {
        try
        {
            var pub = ConnectionMultiplexer.GetSubscriber();
            var pubChannel = new RedisChannel(channel, RedisChannel.PatternMode.Literal);
            
            await pub.PublishAsync(pubChannel, message);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to publish raw message on channel {Channel}", channel);
            throw;
        }
    }
}
using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace SampleStack.Redis.PubSub;

internal abstract class RedisPublisherBase<T> : PubSubServiceBase<T>, IRedisPublisher
{
    private readonly ISubscriber _publisher;

    protected RedisPublisherBase(IConnectionMultiplexer connectionMultiplexer, ILogger<T> logger)
        : base(connectionMultiplexer, logger)
    {
        _publisher = ConnectionMultiplexer.GetSubscriber();
    }
    
    public async Task PublishAsync<TMessage>(string channel, TMessage message)
    {
        try
        {
            var pubChannel = new RedisChannel(channel, RedisChannel.PatternMode.Literal);
            
            var json = JsonSerializer.Serialize<TMessage>(message);
            
            await _publisher.PublishAsync(pubChannel, json);
        }
        catch (JsonException ex)
        {
            Logger.LogError(ex, "Failed to serialize message for channel {Channel}", channel);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to publish message on channel {Channel}", channel);
        }
    }

    public async Task PublishRawAsync(string channel, string message)
    {
        try
        {
            var pubChannel = new RedisChannel(channel, RedisChannel.PatternMode.Literal);
            
            await _publisher.PublishAsync(pubChannel, message);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to publish raw message on channel {Channel}", channel);
        }
    }
}
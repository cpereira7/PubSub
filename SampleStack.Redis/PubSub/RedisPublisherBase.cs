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
        var pub = ConnectionMultiplexer.GetSubscriber();
        var pubChannel = new RedisChannel(channel, RedisChannel.PatternMode.Literal);
            
        var json = JsonSerializer.Serialize(message);
            
        await pub.PublishAsync(pubChannel, json);
    }

    public async Task PublishRawAsync(string channel, string message)
    {
        var pub = ConnectionMultiplexer.GetSubscriber();
        var pubChannel = new RedisChannel(channel, RedisChannel.PatternMode.Literal);
            
        await pub.PublishAsync(pubChannel, message);
    }
}
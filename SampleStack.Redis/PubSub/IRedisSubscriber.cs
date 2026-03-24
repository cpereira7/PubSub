using StackExchange.Redis;

namespace SampleStack.Redis.PubSub;

internal interface IRedisSubscriber : IRedisService
{
    Task SubscribeAsync<T>(string channel, Action<T> handler); 
    Task SubscribeRawAsync(string channel, Action<RedisValue> handler);
}
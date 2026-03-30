using StackExchange.Redis;

namespace SampleStack.Redis.PubSub;

internal interface IRedisSubscriber : IRedisService
{
    Task SubscribeAsync<T>(string channel, Func<T, Task> handler); 
    Task SubscribeRawAsync(string channel, Func<RedisValue, Task> handler);
}
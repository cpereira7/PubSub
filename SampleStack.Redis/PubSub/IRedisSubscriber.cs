using StackExchange.Redis;

namespace SampleStack.Redis.PubSub;

internal interface IRedisSubscriber : IRedisService
{
    Task SubscribeAsync<TMessage>(string channel, Func<TMessage, Task> handler); 
    Task SubscribeRawAsync(string channel, Func<RedisValue, Task> handler);
}
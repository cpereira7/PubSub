using StackExchange.Redis;

namespace SampleStack.Redis.PubSub
{
    internal interface IRedisService
    {
        bool IsRunning { get; }

        event EventHandler CacheDisconnected;
        event EventHandler CacheReConnected;
        
        Task StartAsync();
        Task StopAsync();

        Task OnStartAsync();

        Task SubscribeAsync(string channel, Action<RedisValue> handler); 
        Task PublishAsync(string channel, object message);
    }
}

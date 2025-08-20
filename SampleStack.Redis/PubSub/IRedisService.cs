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

        Task SubscribeAsync<T>(string channel, Action<T> handler); 
        Task SubscribeRawAsync(string channel, Action<RedisValue> handler);
        
        Task PublishAsync<T>(string channel, T message);
        Task PublishRawAsync(string channel, string message);
    }
}

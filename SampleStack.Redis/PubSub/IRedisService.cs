namespace SampleStack.Redis.PubSub
{
    internal interface IRedisService
    {
        Task StartAsync();
        Task StopAsync();

        bool IsRunning { get; }

        event EventHandler CacheDisconnected;
        event EventHandler CacheReConnected;
    }
}

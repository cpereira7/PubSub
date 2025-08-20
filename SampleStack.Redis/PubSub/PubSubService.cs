using System.Text.Json;
using StackExchange.Redis;

namespace SampleStack.Redis.PubSub
{
    internal abstract class PubSubService : IRedisService
    {
        internal readonly IConnectionMultiplexer Redis;

        protected PubSubService(IConnectionMultiplexer connectionMultiplexer)
        {
            Redis = connectionMultiplexer;

            Redis.ConnectionRestored += OnRedisConnectionRestored;
            Redis.ConnectionFailed += OnRedisConnectionFailed;
        }
        
        public event EventHandler? CacheDisconnected;
        public event EventHandler? CacheReConnected;
        
        private bool ChannelsSubscribed { get; set; }
        public bool IsRunning => ChannelsSubscribed;
        
        private void OnRedisConnectionRestored(object? sender, ConnectionFailedEventArgs e)
        {
            if (e.ConnectionType != ConnectionType.Subscription) 
                return;
            
            if (!ChannelsSubscribed)
                SubscribeToChannels();

            CacheReConnected?.Invoke(this, EventArgs.Empty);
        }

        private void OnRedisConnectionFailed(object? sender, ConnectionFailedEventArgs e)
        {
            if (e.ConnectionType == ConnectionType.Subscription)
                CacheDisconnected?.Invoke(this, EventArgs.Empty);
        }

        private void SubscribeToChannels()
        {
            ChannelsSubscribed = true;
        }

        public async Task StartAsync()
        {
            SubscribeToChannels();
            
            await OnStartAsync();
        }

        public async Task StopAsync()
        {
            var subscriber = Redis.GetSubscriber();

            await subscriber.UnsubscribeAllAsync();

            ChannelsSubscribed = false;
        }

        public abstract Task OnStartAsync();

        public async Task SubscribeAsync(string channel, Action<RedisValue> handler)
        {
            var sub = Redis.GetSubscriber();
            var subChannel = new RedisChannel(channel, RedisChannel.PatternMode.Literal);

            await sub.SubscribeAsync(subChannel, (_, message) =>
            {
                handler(message);
            });
        }

        public async Task PublishAsync(string channel, object message)
        {
            var pub = Redis.GetSubscriber();
            var pubChannel = new RedisChannel(channel, RedisChannel.PatternMode.Literal);
            
            var json = JsonSerializer.Serialize(message);
            
            await pub.PublishAsync(pubChannel, json);
        }
    }
}

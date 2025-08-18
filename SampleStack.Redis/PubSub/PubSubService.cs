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
            OnSubscribeToChannels();
        }

        protected abstract void OnSubscribeToChannels();

        public async Task StartAsync()
        {
            SubscribeToChannels();

            await Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            var subscriber = Redis.GetSubscriber();

            await subscriber.UnsubscribeAllAsync();

            ChannelsSubscribed = false;
        }
    }
}

using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleStack.Redis.PubSub
{
    internal abstract class PubSubService : IRedisService
    {
        internal IConnectionMultiplexer _redis;

        internal bool ChannelsSubscribed { get; private set; }
        public bool IsRunning => ChannelsSubscribed;

        public event EventHandler? CacheDisconnected;
        public event EventHandler? CacheReConnected;

        protected PubSubService(IConnectionMultiplexer connectionMultiplexer)
        {
            _redis = connectionMultiplexer;

            _redis.ConnectionRestored += OnRedisConnectionRestored;
            _redis.ConnectionFailed += OnRedisConnectionFailed;
        }

        private void OnRedisConnectionRestored(object? sender, ConnectionFailedEventArgs e)
        {
            if (e.ConnectionType == ConnectionType.Subscription)
            {
                if (!ChannelsSubscribed)
                    SubscribeToChannels();

                CacheReConnected?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnRedisConnectionFailed(object? sender, ConnectionFailedEventArgs e)
        {
            if (e.ConnectionType == ConnectionType.Subscription)
                CacheDisconnected?.Invoke(this, EventArgs.Empty);
        }

        internal virtual void SubscribeToChannels()
        {
            ChannelsSubscribed = true;
        }

        public async Task StartAsync()
        {
            SubscribeToChannels();

            await Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            ISubscriber subscriber = _redis.GetSubscriber();

            await subscriber.UnsubscribeAllAsync();

            ChannelsSubscribed = false;
        }
    }
}

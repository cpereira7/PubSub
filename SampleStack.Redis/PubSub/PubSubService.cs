using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace SampleStack.Redis.PubSub
{
    internal abstract class PubSubService : IRedisService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<PubSubService> _logger;

        protected PubSubService(IConnectionMultiplexer connectionMultiplexer, ILogger<PubSubService> logger)
        {
            _redis = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _redis.ConnectionRestored += OnRedisConnectionRestored;
            _redis.ConnectionFailed += OnRedisConnectionFailed;
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
            var subscriber = _redis.GetSubscriber();

            await subscriber.UnsubscribeAllAsync();

            ChannelsSubscribed = false;
        }

        public abstract Task OnStartAsync();

        public async Task SubscribeAsync<T>(string channel, Action<T> handler)
        {
            var sub = _redis.GetSubscriber();
            var subChannel = new RedisChannel(channel, RedisChannel.PatternMode.Literal);

            await sub.SubscribeAsync(subChannel, (_, message) =>
            {
                try
                {
                    var deserialized = JsonSerializer.Deserialize<T>(message!);
                    if (deserialized != null)
                    {
                        handler(deserialized);
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to deserialize message on {Channel}", channel);
                }
            });
        }

        public async Task SubscribeRawAsync(string channel, Action<RedisValue> handler)
        {
            var sub = _redis.GetSubscriber();
            var subChannel = new RedisChannel(channel, RedisChannel.PatternMode.Literal);

            await sub.SubscribeAsync(subChannel, (_, message) =>
            {
                handler(message);
            });
        }

        public async Task PublishAsync<T>(string channel, T message)
        {
            var pub = _redis.GetSubscriber();
            var pubChannel = new RedisChannel(channel, RedisChannel.PatternMode.Literal);
            
            var json = JsonSerializer.Serialize(message);
            
            await pub.PublishAsync(pubChannel, json);
        }

        public async Task PublishRawAsync(string channel, string message)
        {
            var pub = _redis.GetSubscriber();
            var pubChannel = new RedisChannel(channel, RedisChannel.PatternMode.Literal);
            
            await pub.PublishAsync(pubChannel, message);
        }
    }
}

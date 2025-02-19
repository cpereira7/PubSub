using StackExchange.Redis;

namespace SampleStack.Redis.PubSub
{
    internal class SubscriberService : IRedisService
    {
        private IConnectionMultiplexer _redis;

        public bool ChannelsSubscribed { get; private set; }
        public bool IsRunning => ChannelsSubscribed;

        public event EventHandler? CacheDisconnected;
        public event EventHandler? CacheReConnected;

        private const string Channel = "random-numbers";

        public SubscriberService(IConnectionMultiplexer connectionMultiplexer)
        {
            _redis = connectionMultiplexer;

            _redis.ConnectionRestored += OnRedisConnectionRestored;
            _redis.ConnectionFailed += OnRedisConnectionFailed;
        }

        private void OnRedisConnectionRestored(object? sender, ConnectionFailedEventArgs e)
        {
            RunSubscriber();

            if (e.ConnectionType == ConnectionType.Subscription)
                CacheReConnected?.Invoke(this, EventArgs.Empty);
        }

        private void OnRedisConnectionFailed(object? sender, ConnectionFailedEventArgs e)
        {
            if (e.ConnectionType == ConnectionType.Subscription)
                CacheDisconnected?.Invoke(this, EventArgs.Empty);
        }

        private void RunSubscriber()
        {
            Console.WriteLine("Listening on random-numbers...");

            var np = new NumbersProcessor();

            var pubsub = _redis.GetSubscriber();
            var channel = new RedisChannel(Channel, RedisChannel.PatternMode.Literal);

            pubsub.Subscribe(channel, (channel, message) =>
            {
                Console.WriteLine("Message received: " + message);

                np.ProcessMessage(message!);
            });

            Console.ReadLine();
        }

        public Task StartAsync()
        {
            RunSubscriber();

            ChannelsSubscribed = true;

            return Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            ISubscriber subscriber = _redis.GetSubscriber();

            await subscriber.UnsubscribeAllAsync();

            ChannelsSubscribed = false;
        }
    }
}

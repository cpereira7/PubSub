using Microsoft.Extensions.Logging;
using SampleStack.Redis.Numbers;
using StackExchange.Redis;

namespace SampleStack.Redis.PubSub
{
    internal class SubscriberService : PubSubService
    {
        private readonly ILogger<SubscriberService> _logger;
        private readonly NumbersProcessor _numbersProcessor;

        public SubscriberService(IConnectionMultiplexer connectionMultiplexer, ILogger<SubscriberService> logger, NumbersProcessor numbersProcessor) : base(connectionMultiplexer)
        {
            _logger = logger;
            _numbersProcessor = numbersProcessor;
        }

        internal override void SubscribeToChannels()
        {
            try
            {
                SubscriberNumbersChannel();

                base.SubscribeToChannels();
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError(ex, "Redis connection failed.");
                throw;
            }
        }

        private void SubscriberNumbersChannel()
        {
            var pubsub = _redis.GetSubscriber();
            var channel = new RedisChannel(PubSubChannels.RandomNumbers, RedisChannel.PatternMode.Literal);

            pubsub.Subscribe(channel, (channel, message) =>
            {
                Console.WriteLine("Message received: " + message);

                _numbersProcessor.ProcessMessage(message!);
            });
        }
    }
}

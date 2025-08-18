using Microsoft.Extensions.Logging;
using SampleStack.Redis.Numbers.Services;
using SampleStack.Redis.PubSub;
using SampleStack.Redis.PubSub.Constants;
using StackExchange.Redis;

namespace SampleStack.Redis.Numbers.Messaging
{
    internal class NumbersSubscriber : PubSubService
    {
        private readonly ILogger<NumbersSubscriber> _logger;
        private readonly NumbersProcessor _numbersProcessor;

        public NumbersSubscriber(IConnectionMultiplexer connectionMultiplexer, ILogger<NumbersSubscriber> logger, NumbersProcessor numbersProcessor) : base(connectionMultiplexer)
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
            var pubsub = Redis.GetSubscriber();
            var channel = new RedisChannel(PubSubChannels.RandomNumbers, RedisChannel.PatternMode.Literal);

            pubsub.Subscribe(channel, (channel, message) =>
            {
                Console.WriteLine("Message received: " + message);

                _numbersProcessor.ProcessMessage(message!);
            });
        }
    }
}

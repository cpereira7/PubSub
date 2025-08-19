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
        
        protected override void OnSubscribeToChannels()
        {
            _ = Task.Run(SubscriberNumbers);
        }

        private async Task SubscriberNumbers()
        {
            var pubsub = Redis.GetSubscriber();
            var channel = new RedisChannel(PubSubChannels.RandomNumbers, RedisChannel.PatternMode.Literal);

            await pubsub.SubscribeAsync(channel, (channel, message) =>
            {
                _logger.LogInformation("Message received: {Message}", message);

                _numbersProcessor.ProcessMessage(message!);
            });
        }
    }
}

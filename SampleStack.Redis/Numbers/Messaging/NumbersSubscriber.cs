using Microsoft.Extensions.Logging;
using SampleStack.Redis.Numbers.Model;
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

        public NumbersSubscriber(IConnectionMultiplexer connectionMultiplexer, ILogger<NumbersSubscriber> logger, NumbersProcessor numbersProcessor) 
            : base(connectionMultiplexer, logger)
        {
            _logger = logger;
            _numbersProcessor = numbersProcessor;
        }
        
        public override async Task OnStartAsync()
        {
            await SubscribeAsync<NumberMessage>(PubSubChannels.RandomNumbers, HandleRandomNumbers);
        }

        private void HandleRandomNumbers(NumberMessage message)
        {
            _logger.LogInformation("Message received: {Message}", message);

            _numbersProcessor.ProcessMessage(message);
        }
    }
}

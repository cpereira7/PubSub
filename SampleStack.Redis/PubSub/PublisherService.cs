using SampleStack.Redis.Numbers;
using StackExchange.Redis;
using System.Text.Json;

namespace SampleStack.Redis.PubSub
{
    internal class PublisherService : PubSubService
    {
        private readonly NumbersGenerator _numbersGenerator;
        
        public PublisherService(IConnectionMultiplexer connectionMultiplexer, NumbersGenerator numbersGenerator) : base(connectionMultiplexer)
        {
            _numbersGenerator = numbersGenerator;
        }
        
        internal override void SubscribeToChannels()
        {
            base.SubscribeToChannels();

            _ = Task.Run(PublishNumbers);
        }
        
        private async Task PublishNumbers()
        {
            string publisherId = Environment.GetEnvironmentVariable("HOSTNAME") ?? "UNKNOWN";

            var pubsub = _redis.GetSubscriber();
            var channel = new RedisChannel(PubSubChannels.RandomNumbers, RedisChannel.PatternMode.Literal);

            while (IsRunning)
            {
                var msg = _numbersGenerator.GenerateNumberMessage(publisherId);

                string json = JsonSerializer.Serialize(msg);
                pubsub.Publish(channel, json);
                Console.WriteLine($"Published: {json}");

                int sleepDuration = new Random().Next(1, 2501);
                await Task.Delay(sleepDuration);
            }
        }
    }
}

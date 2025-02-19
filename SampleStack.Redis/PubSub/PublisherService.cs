using StackExchange.Redis;
using System.Text.Json;

namespace SampleStack.Redis.PubSub
{
    internal class PublisherService : PubSubService
    {
        private const string Channel = "random-numbers";
        private readonly NumbersGenerator _numbersGenerator;
        
        public PublisherService(IConnectionMultiplexer connectionMultiplexer, NumbersGenerator numbersGenerator) : base(connectionMultiplexer)
        {
            _numbersGenerator = numbersGenerator;
        }
        
        internal override void SubscribeToChannels()
        {
            base.SubscribeToChannels();

            PublishNumbers();
        }
        
        private void PublishNumbers()
        {
            Console.WriteLine("Writing on random-numbers...");

            string publisherId = Environment.GetEnvironmentVariable("HOSTNAME") ?? "UNKNOWN";

            var pubsub = _redis.GetSubscriber();
            var channel = new RedisChannel(Channel, RedisChannel.PatternMode.Literal);

            while (IsRunning)
            {
                var msg = _numbersGenerator.GenerateNumberMessage(publisherId);

                string json = JsonSerializer.Serialize(msg);
                pubsub.Publish(channel, json);
                Console.WriteLine($"Published: {json}");

                int sleepDuration = new Random().Next(1, 2501);
                Thread.Sleep(sleepDuration);

            }
        }
    }
}

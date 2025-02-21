using SampleStack.Redis.Numbers;
using StackExchange.Redis;

namespace SampleStack.Redis.PubSub
{
    internal class SubscriberService : PubSubService
    {
        private const string Channel = "random-numbers";
        private readonly NumbersProcessor numbersProcessor;

        public SubscriberService(IConnectionMultiplexer connectionMultiplexer, NumbersProcessor numbersProcessor) : base(connectionMultiplexer)
        {
            this.numbersProcessor = numbersProcessor;
        }

        internal override void SubscribeToChannels()
        {
            try
            {
                SubscriberNumbersChannel();

                base.SubscribeToChannels();
            }
            catch (RedisConnectionException)
            {
                Console.WriteLine("Redis connection failed.");
                throw;
            }
        }

        private void SubscriberNumbersChannel()
        {
            var pubsub = _redis.GetSubscriber();
            var channel = new RedisChannel(Channel, RedisChannel.PatternMode.Literal);

            pubsub.Subscribe(channel, (channel, message) =>
            {
                Console.WriteLine("Message received: " + message);

                numbersProcessor.ProcessMessage(message!);
            });
        }
    }
}

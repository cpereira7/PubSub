using SampleStack.Redis.PubSub;
using StackExchange.Redis;
using System.Text.Json;

namespace SampleStack.Redis
{
    internal static class Program
    {
        private const string RedisConnectionString = "redis:6379";
        private static ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(RedisConnectionString);
        private const string Channel = "random-numbers";

        static void Main(string[] args)
        {
            var exitEvent = new ManualResetEventSlim(false);

            string mode = Environment.GetEnvironmentVariable("MODE") ?? "SUB";

            if (mode == "PUB")
            {
                RunPublisher();
            }
            else if (mode == "SUB")
            {
                SubscriberService subscriber = new(connection);
                subscriber.StartAsync().Wait();
            }
            else
            {
                Console.WriteLine("Invalid or missing MODE. Set MODE=P for Publisher or MODE=S for Subscriber.");
            }

            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("Shutting down...");
                exitEvent.Set(); 
                e.Cancel = true;
            };

            exitEvent.Wait();
        }

        static void RunPublisher()
        {
            Console.WriteLine("Writing on random-numbers...");

            string publisherId = Environment.GetEnvironmentVariable("HOSTNAME") ?? "UNKNOWN";

            var pubsub = connection.GetSubscriber();
            var channel = new RedisChannel(Channel, RedisChannel.PatternMode.Literal);

            while(true)
            {
                int randomNumber = new Random().Next(1, 101);
                var msg = new NumberMessage
                {
                    Publisher = publisherId,
                    Value = randomNumber,
                    Timestamp = DateTime.UtcNow
                };

                string json = JsonSerializer.Serialize(msg);
                pubsub.Publish(channel, json);
                Console.WriteLine($"Published: {json}");

                int sleepDuration = new Random().Next(1, 2501);
                Thread.Sleep(sleepDuration);
            }
        }
    }
}
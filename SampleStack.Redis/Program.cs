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
                RunSubscriber();
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
            Console.WriteLine("Writing on test-channel...");

            string publisherId = Environment.GetEnvironmentVariable("HOSTNAME") ?? "UNKNOWN";

            var pubsub = connection.GetSubscriber();
            var channel = new RedisChannel(Channel, RedisChannel.PatternMode.Literal);

            for (int i = 0; i < 10; i++)
            {
                int randomNumber = new Random().Next(1, 101);
                var msg = new
                {
                    Publisher = publisherId,
                    Value = randomNumber,
                    Timestamp = DateTime.UtcNow
                };

                string json = JsonSerializer.Serialize(msg);
                pubsub.Publish(channel, json);
                Console.WriteLine($"Published: {json}");
            }
        }

        static void RunSubscriber()
        {
            Console.WriteLine("Listening on test-channel...");

            var pubsub = connection.GetSubscriber();
            var channel = new RedisChannel(Channel, RedisChannel.PatternMode.Literal);

            pubsub.Subscribe(channel, (channel, message) =>
            {
                Console.WriteLine("Message received: " + message);
            });

            Console.ReadLine();
        }
    }
}
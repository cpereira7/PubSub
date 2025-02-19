using SampleStack.Redis.PubSub;
using StackExchange.Redis;
using System.Text.Json;

namespace SampleStack.Redis
{
    internal static class Program
    {
        private const string RedisConnectionString = "redis:6379";
        private static ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(RedisConnectionString);

        static void Main(string[] args)
        {
            var exitEvent = new ManualResetEventSlim(false);

            string mode = Environment.GetEnvironmentVariable("MODE") ?? "SUB";

            if (mode == "PUB")
            {
                PublisherService publisher = new(connection, new NumbersGenerator());
                publisher.StartAsync().Wait();
            }
            else if (mode == "SUB")
            {
                SubscriberService subscriber = new(connection, new NumbersProcessor());
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
    }
}
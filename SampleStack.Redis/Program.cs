using StackExchange.Redis;
using System.Runtime.Loader;

namespace SampleStack.Redis
{
    internal static class Program
    {
        private const string RedisConnectionString = "redis:6379";
        private static ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(RedisConnectionString);
        private const string Channel = "test-channel";

        static void Main(string[] args)
        {
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
        }

        static void RunPublisher()
        {
            Console.WriteLine("Writing on test-channel...");

            var pubsub = connection.GetSubscriber();
            for (int i = 0; i < 10; i++)
            {
                pubsub.PublishAsync(Channel, "This is a test message!!", CommandFlags.FireAndForget);
            }
            
            Console.Write("Messages Successfully sent to test-channel");
        }

        static void RunSubscriber()
        {
            Console.WriteLine("Listening on test-channel...");

            var pubsub = connection.GetSubscriber();
            pubsub.Subscribe(Channel, (channel, message) =>
            {
                Console.WriteLine("Message received: " + message);
            });

            Console.ReadLine();
        }
    }
}
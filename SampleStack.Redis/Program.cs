using StackExchange.Redis;

namespace SampleStack.Redis
{
    internal static class Program
    {
        private const string RedisConnectionString = "localhost:6379";
        private static ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(RedisConnectionString);
        private const string Channel = "test-channel";

        static void Main(string[] args)
        {
            Console.WriteLine("Select Mode: (P)ublisher / (S)ubscriber");
            string mode = Console.ReadLine()?.Trim().ToUpper();

            if (mode == "P")
            {
                RunPublisher();
            }
            else if (mode == "S")
            {
                RunSubscriber();
            }
            else
            {
                Console.WriteLine("Invalid option. Exiting...");
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
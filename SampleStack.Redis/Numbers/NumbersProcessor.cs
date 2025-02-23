using Microsoft.Extensions.Logging;
using System.Text.Json;
using StackExchange.Redis;

namespace SampleStack.Redis.Numbers
{
    internal class NumbersProcessor
    {
        private readonly Dictionary<string, Queue<int>> _publisherQueues = [];
        private readonly Lock _lockObj = new();
        private readonly ILogger<NumbersProcessor> _logger;
        private readonly IDatabase _database;

        public NumbersProcessor(ILogger<NumbersProcessor> logger, IConnectionMultiplexer redis)
        {
            _logger = logger;
            _database = redis.GetDatabase();
        }

        public void ProcessMessage(string message)
        {
            try
            {
                var receivedMsg = JsonSerializer.Deserialize<NumberMessage>(message);

                if (receivedMsg != null)
                {
                    Console.WriteLine($"Received from {receivedMsg.Publisher}: {receivedMsg.Value} at {receivedMsg.Timestamp}");

                    lock (_lockObj)
                    {
                        if (!_publisherQueues.TryGetValue(receivedMsg.Publisher, out Queue<int>? value))
                        {
                            value = new Queue<int>();
                            _publisherQueues[receivedMsg.Publisher] = value;
                        }

                        value.Enqueue(receivedMsg.Value);

                        ProcessAndStoreValues();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
            }
        }

        private void ProcessAndStoreValues()
        {
            var publishers = new List<string>(_publisherQueues.Keys);

            if (publishers.Count < 2)
                return;

            while (publishers.Count > 0)
            {
                var activePublishers = publishers
                    .Where(p => _publisherQueues[p].Count > 0)
                    .ToList();

                if (activePublishers.Count < 2)
                    break;

                var values = new Dictionary<string, int>();

                foreach (var publisher in activePublishers)
                {
                    values.Add(publisher, _publisherQueues[publisher].Dequeue());
                }

                var result = new NumbersResult(values);
                
                Console.WriteLine($"🔢 Processed {values.Count} Values:");
                Console.WriteLine($"   Sum: {string.Join(" + ", values.Values)} = {result.Sum}");
                Console.WriteLine($"   Product: {string.Join(" * ", values.Values)} = {result.Product}");

                publishers.RemoveAll(p => _publisherQueues[p].Count == 0);
                
                _ = StoreResults(result).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        _logger.LogError(task.Exception, "Error storing results in Redis.");
                    }
                }, TaskScheduler.Default);
            }
        }

        private async Task StoreResults(NumbersResult result)
        {
            var redisKey = "numbers:all";

            var jsonResult = JsonSerializer.Serialize(result);
            
            await _database.ListRightPushAsync(redisKey, jsonResult);
        }
    }
}

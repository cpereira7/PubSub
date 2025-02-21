using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace SampleStack.Redis.Numbers
{
    internal class NumbersProcessor
    {
        private readonly Dictionary<string, Queue<int>> _publisherQueues = [];
        private readonly Lock _lockObj = new();
        private readonly ILogger<NumbersProcessor> _logger;

        public NumbersProcessor(ILogger<NumbersProcessor> logger)
        {
            _logger = logger;
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

                        ProcessValues();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
            }
        }

        private void ProcessValues()
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

                var values = new List<int>();

                foreach (var publisher in activePublishers)
                {
                    values.Add(_publisherQueues[publisher].Dequeue());
                }

                int totalSum = values.Sum();
                int totalProduct = values.Aggregate(1, (acc, val) => acc * val);

                Console.WriteLine($"🔢 Processed {values.Count} Values:");
                Console.WriteLine($"   Sum: {string.Join(" + ", values)} = {totalSum}");
                Console.WriteLine($"   Product: {string.Join(" * ", values)} = {totalProduct}");

                publishers.RemoveAll(p => _publisherQueues[p].Count == 0);
            }
        }
    }
}

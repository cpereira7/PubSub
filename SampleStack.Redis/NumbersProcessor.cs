using System.Text.Json;

namespace SampleStack.Redis
{
    internal class NumbersProcessor
    {
        private readonly Dictionary<string, Queue<int>> _publisherQueues = [];
        private readonly Lock _lockObj = new();

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
                Console.WriteLine($"Error processing message: {ex.Message}");
            }
        }

        private void ProcessValues()
        {
            // Create a working list of publishers.
            var publishers = new List<string>(_publisherQueues.Keys);

            // Only process if there are at least 2 publishers.
            if (publishers.Count < 2)
                return;

            while (publishers.Count > 0)
            {
                // Get the list of publishers that have at least one queued value.
                var activePublishers = publishers
                    .Where(p => _publisherQueues[p].Count > 0)
                    .ToList();

                // If fewer than 2 publishers are active, exit the loop.
                if (activePublishers.Count < 2)
                    break;

                var values = new List<int>();

                // Process exactly one value from each active publisher.
                foreach (var publisher in activePublishers)
                {
                    values.Add(_publisherQueues[publisher].Dequeue());
                }

                // Compute the sum and product of the values.
                int totalSum = values.Sum();
                int totalProduct = values.Aggregate(1, (acc, val) => acc * val);

                Console.WriteLine($"🔢 Processed {values.Count} Values:");
                Console.WriteLine($"   Sum: {string.Join(" + ", values)} = {totalSum}");
                Console.WriteLine($"   Product: {string.Join(" * ", values)} = {totalProduct}");

                // Remove publishers that now have empty queues.
                publishers.RemoveAll(p => _publisherQueues[p].Count == 0);
            }
        }


    }
}

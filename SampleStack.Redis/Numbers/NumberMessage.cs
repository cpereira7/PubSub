namespace SampleStack.Redis.Numbers
{
    internal record NumberMessage
    {
        public required string Publisher { get; init; }
        public int Value { get; init; }
        public DateTime Timestamp { get; init; }
    }
}

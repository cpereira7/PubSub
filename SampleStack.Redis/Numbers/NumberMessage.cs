namespace SampleStack.Redis.Numbers
{
    internal class NumberMessage
    {
        public required string Publisher { get; set; }
        public int Value { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

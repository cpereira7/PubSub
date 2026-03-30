namespace SampleStack.Redis.Model;

public record CacheMessage(string Value, string Publisher)
{
    public string MessageId { get; init; } = Guid.NewGuid().ToString("N");
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}

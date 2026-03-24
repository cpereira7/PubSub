namespace SampleStack.Redis.PubSub;

internal interface IRedisPublisher : IRedisService
{
    Task PublishAsync<T>(string channel, T message);
    Task PublishRawAsync(string channel, string message);
}
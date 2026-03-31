namespace SampleStack.Redis.PubSub;

internal interface IRedisPublisher : IRedisService
{
    Task PublishAsync<TMessage>(string channel, TMessage message);
    Task PublishRawAsync(string channel, string message);
}
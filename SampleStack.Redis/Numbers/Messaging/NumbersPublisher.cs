using System.Text.Json;
using SampleStack.Redis.Numbers.Services;
using SampleStack.Redis.PubSub;
using SampleStack.Redis.PubSub.Constants;
using StackExchange.Redis;

namespace SampleStack.Redis.Numbers.Messaging;

internal class NumbersPublisher : PubSubService
{
    private readonly NumbersGenerator _numbersGenerator;
        
    public NumbersPublisher(IConnectionMultiplexer connectionMultiplexer, NumbersGenerator numbersGenerator) : base(connectionMultiplexer)
    {
        _numbersGenerator = numbersGenerator;
    }
        
    internal override void SubscribeToChannels()
    {
        base.SubscribeToChannels();

        _ = Task.Run(PublishNumbers);
    }
        
    private async Task PublishNumbers()
    {
        var publisherId = Environment.GetEnvironmentVariable("HOSTNAME") ?? "UNKNOWN";

        var pubsub = Redis.GetSubscriber();
        var channel = new RedisChannel(PubSubChannels.RandomNumbers, RedisChannel.PatternMode.Literal);

        while (IsRunning)
        {
            var msg = _numbersGenerator.GenerateNumberMessage(publisherId);

            var json = JsonSerializer.Serialize(msg);
            await pubsub.PublishAsync(channel, json);
                
            Console.WriteLine($"Published: {json}");

            var sleepDuration = new Random().Next(1, 2501);
            await Task.Delay(sleepDuration);
        }
    }
}
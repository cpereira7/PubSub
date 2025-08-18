using System.Text.Json;
using Microsoft.Extensions.Logging;
using SampleStack.Redis.Numbers.Services;
using SampleStack.Redis.PubSub;
using SampleStack.Redis.PubSub.Constants;
using StackExchange.Redis;

namespace SampleStack.Redis.Numbers.Messaging;

internal class NumbersPublisher : PubSubService
{
    private readonly NumbersGenerator _numbersGenerator;
    private readonly ILogger<NumbersPublisher> _logger;

    public NumbersPublisher(IConnectionMultiplexer connectionMultiplexer, NumbersGenerator numbersGenerator, ILogger<NumbersPublisher> logger) : base(connectionMultiplexer)
    {
        _numbersGenerator = numbersGenerator;
        _logger = logger;
    }
    
    protected override void OnSubscribeToChannels()
    {
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
                
            _logger.LogInformation("Published: {json}", json);

            var sleepDuration = new Random().Next(1, 2501);
            await Task.Delay(sleepDuration);
        }
    }
}
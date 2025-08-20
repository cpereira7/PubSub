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

    public NumbersPublisher(IConnectionMultiplexer connectionMultiplexer, NumbersGenerator numbersGenerator, ILogger<NumbersPublisher> logger) 
        : base(connectionMultiplexer, logger)
    {
        _numbersGenerator = numbersGenerator;
        _logger = logger;
    }
    
    public override async Task OnStartAsync()
    {
        await PublishNumbers();
    }

    private async Task PublishNumbers()
    {
        var publisherId = Environment.GetEnvironmentVariable("HOSTNAME") ?? "UNKNOWN";

        while (IsRunning)
        {
            var msg = _numbersGenerator.GenerateNumberMessage(publisherId);

            await PublishAsync(PubSubChannels.RandomNumbers, msg);
                
            _logger.LogInformation("Published a new message: {Data}", msg);

            var sleepDuration = new Random().Next(1500, 9999);
            await Task.Delay(sleepDuration);
        }
    }
}
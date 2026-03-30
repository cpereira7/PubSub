using System.Text.Json;
using Emojify;
using Emojify.Configuration;
using Emojify.Predictor;
using Microsoft.Extensions.Logging;
using SampleStack.Redis.Model;
using SampleStack.Redis.PubSub;
using SampleStack.Redis.PubSub.Constants;
using StackExchange.Redis;

namespace SampleStack.Redis.Messaging;

internal class PhrasesSubscriber : RedisSubscriberBase
{
    private readonly ILogger<PhrasesSubscriber> _logger;
    private readonly IDatabase _database;

    public PhrasesSubscriber(IConnectionMultiplexer connectionMultiplexer, ILogger<PhrasesSubscriber> logger)
        : base(connectionMultiplexer, logger)
    {
        _logger = logger;
        _database = connectionMultiplexer.GetDatabase();
        EmojifyService.Initialize(new EmojifyConfiguration() { ConfidenceThreshold = PredictionMode.Creative });
    }

    protected override async Task OnStartAsync(CancellationToken cancellationToken)
    {
        await SubscribeAsync<CacheMessage>(PubSubChannels.Phrases, HandleMessages);
    }

    private async Task HandleMessages(CacheMessage message)
    {
        try
        {
            _logger.LogInformation("Received [{MessageId}] from {Publisher}: {Value} at {Timestamp}",
                message.MessageId, message.Publisher, message.Value, message.Timestamp);

            var formatted = EmojifyService.Emojify(message.Value ?? string.Empty);
            
            _logger.LogInformation("Result [{MessageId}]: {Result}", message.MessageId, formatted);

            await StorePhraseAsync(message.MessageId, message.Publisher, formatted, message.Timestamp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message");
        }
    }

    private async Task StorePhraseAsync(string messageId, string publisher, string formattedValue, DateTimeOffset timestamp)
    {
        const string redisKey = "phrases:all";

        var payload = new
        {
            MessageId = messageId,
            Publisher = publisher,
            Value = formattedValue,
            Timestamp = timestamp
        };

        var json = JsonSerializer.Serialize(payload);

        await _database.ListRightPushAsync(redisKey, json);
    }
}

using System.Text.Json;
using Emojify;
using Emojify.Configuration;
using Microsoft.Extensions.Logging;
using SampleStack.Redis.Model;
using SampleStack.Redis.PubSub;
using SampleStack.Redis.PubSub.Constants;
using StackExchange.Redis;

namespace SampleStack.Redis.Messaging;

internal class PhrasesSubscriber : RedisSubscriberBase<PhrasesSubscriber>
{
    private readonly IDatabase _database;

    private Task? _subscriptionTask;
    
    public PhrasesSubscriber(IConnectionMultiplexer connectionMultiplexer, ILogger<PhrasesSubscriber> logger)
        : base(connectionMultiplexer, logger)
    {
        _database = connectionMultiplexer.GetDatabase();
        EmojifyService.Initialize(new EmojifyConfiguration() { ConfidenceThreshold = PredictionMode.Creative });
    }

    protected override Task OnStartAsync(CancellationToken cancellationToken)
    {
        _subscriptionTask = SubscribeAsync<CacheMessage>(PubSubChannels.Phrases, HandleMessages);
        
        return Task.CompletedTask;
    }
    
    protected override async Task OnStopAsync()
    {
        await base.OnStopAsync();
        
        if (_subscriptionTask is null)
            return;

        await _subscriptionTask;
    }

    private async Task HandleMessages(CacheMessage message)
    {
        try
        {
            var originalValue = message.Value;
            var formattedValue = EmojifyService.Emojify(originalValue);

            Logger.LogInformation(
                "Processed [{MessageId}] from {Publisher} at {Timestamp}: {OriginalValue} => {FormattedValue}",
                message.MessageId, message.Publisher, message.Timestamp, originalValue, formattedValue);

            await StorePhraseAsync(message.MessageId, message.Publisher, formattedValue, message.Timestamp);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing message");
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

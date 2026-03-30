using Microsoft.Extensions.Logging;
using SampleStack.Redis.Model;
using SampleStack.Redis.PubSub;
using SampleStack.Redis.PubSub.Constants;
using StackExchange.Redis;

namespace SampleStack.Redis.Messaging;

internal class PhrasesPublisher : RedisPublisherBase
{
    private readonly Random _random;

    private readonly string[] _phrases =
    [
        "User abc logged out",
        "File uploaded: report.pdf",
        "File deleted: temp.log",
        "Password changed successfully",
        "Failed login attempt detected",
        "Email sent to user@example.com",
        "Session started: user@example.com",
        "Session terminated due to inactivity",
        "New device connected: device_id_9876",
        "Application error: null reference exception",
        "Service unavailable: retry later",
        "Configuration rollback executed",
        "Notification sent: system update",
        "Task scheduled: data backup at 02:00 AM",
        "Access granted to resource /secure/data"
    ];

    public PhrasesPublisher(IConnectionMultiplexer connectionMultiplexer, ILogger<PhrasesPublisher> logger) 
        : base(connectionMultiplexer, logger)
    {
        _random = Random.Shared;
    }

    protected override async Task OnStartAsync(CancellationToken cancellationToken)
    {
        var publisherId = Environment.GetEnvironmentVariable("HOSTNAME") ?? "UNKNOWN";

        while (!cancellationToken.IsCancellationRequested)
        {
            var phrase = _phrases[_random.Next(_phrases.Length)];
            var message = new CacheMessage(phrase, publisherId);
            
            await PublishAsync(PubSubChannels.Phrases, message);
            
            Logger.LogInformation("Published [{MessageId}]: {Phrase}", message.MessageId, phrase);

            var delay = _random.Next(1500, 9999);
            await Task.Delay(delay, cancellationToken);
        }
    }
}

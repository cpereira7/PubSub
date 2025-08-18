using SampleStack.Redis.Numbers.Model;

namespace SampleStack.Redis.Numbers.Services;

internal class NumbersGenerator
{
    public NumberMessage GenerateNumberMessage(string publisherId)
    {
        var randomNumber = new Random().Next(1, 101);
        return new NumberMessage
        {
            Publisher = publisherId,
            Value = randomNumber,
            Timestamp = DateTime.UtcNow
        };
    }
}
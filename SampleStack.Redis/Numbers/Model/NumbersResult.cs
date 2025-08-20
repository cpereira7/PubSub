namespace SampleStack.Redis.Numbers.Model;

internal record NumbersResult
{
    public IDictionary<string,int> Numbers { get; init; }
    public int Sum { get; init; }
    public int Product { get; init; }
    public DateTime Timestamp { get; init; }

    public NumbersResult(IDictionary<string, int> numbers)
    {
        Numbers = numbers ?? throw new ArgumentNullException(nameof(numbers));
        
        Sum = numbers.Values.Sum();
        Product = numbers.Aggregate(1, (acc, val) => acc * val.Value);
        
        Timestamp = DateTime.UtcNow;
    }
}
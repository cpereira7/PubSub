namespace SampleStack.Redis.Numbers
{
    internal class NumbersGenerator
    {
        public NumberMessage GenerateNumberMessage(string publisherId)
        {
            int randomNumber = new Random().Next(1, 101);
            return new NumberMessage
            {
                Publisher = publisherId,
                Value = randomNumber,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}

namespace LiveStreamingServerNet.Utilities.Extensions
{
    public static class StringExtensions
    {
        public static string[] SplitBySpaces(this string value)
        {
            return value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }
    }
}

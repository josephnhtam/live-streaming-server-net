using System.Runtime.CompilerServices;

namespace LiveStreamingServerNet.Utilities.Extensions;

public static class ThrowHelper
{
    public static void ThrowIfLessThanOrEqual(long value, long limit, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value.CompareTo(limit) <= 0)
            throw new ArgumentOutOfRangeException(paramName, $"The value must be greater than {limit}.");
    }
}
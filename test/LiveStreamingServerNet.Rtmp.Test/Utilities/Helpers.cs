using LiveStreamingServerNet.Rtmp.Internal;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Test.Utilities
{
    public static class Helpers
    {
        public static uint CreateRandomChunkStreamId()
        {
            return (uint)Random.Shared.Next(2, ushort.MaxValue + 1);
        }
        public static List<object?> CreateExpectedCommandProperties(string level, string code)
        {
            return Arg.Is<List<object?>>(x =>
                ((Dictionary<string, object?>)x[0]!)[RtmpArgumentNames.Level] as string == level &&
                ((Dictionary<string, object?>)x[0]!)[RtmpArgumentNames.Code] as string == code
            );
        }

        public static IReadOnlyDictionary<string, string> CreateExpectedStreamArguments(string key, string value)
        {
            return Arg.Is<IReadOnlyDictionary<string, string>>(x => x[key] == value);
        }

        public static IReadOnlyDictionary<string, object> CreateExpectedMetaData<TValue>(string key, TValue value)
            where TValue : struct, IEquatable<TValue>
        {
            return Arg.Is<IReadOnlyDictionary<string, object>>(x => ((TValue)x[key]).Equals(value));
        }
    }
}

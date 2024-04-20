using LiveStreamingServerNet.Rtmp.Internal;
using NSubstitute;

namespace LiveStreamingServerNet.Rtmp.Test.Utilities
{
    public static class Helpers
    {
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
    }
}

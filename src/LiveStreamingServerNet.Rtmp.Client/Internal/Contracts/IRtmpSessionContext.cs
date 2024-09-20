using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using System.Collections.Concurrent;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Contracts
{
    internal interface IRtmpSessionContext : IRtmpChunkStreamContextProvider, IAsyncDisposable
    {
        ISessionHandle Session { get; }
        ConcurrentDictionary<string, object> Items { get; }

        RtmpSessionState State { get; set; }

        new uint InChunkSize { get; set; }
        uint OutChunkSize { get; set; }
    }
}
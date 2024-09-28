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

        uint InWindowAcknowledgementSize { get; set; }
        uint OutWindowAcknowledgementSize { get; set; }

        uint SequenceNumber { get; set; }
        uint LastAcknowledgedSequenceNumber { get; set; }

        string? AppName { get; set; }

        IRtmpStreamContext CreateStreamContext(uint streamId);
        List<IRtmpStreamContext> GetStreamContexts();
        IRtmpStreamContext? GetStreamContext(uint streamId);
        void RemoveStreamContext(uint streamId);
    }
}
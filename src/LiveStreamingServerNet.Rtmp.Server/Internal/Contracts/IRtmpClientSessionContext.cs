using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.RtmpEventHandlers.Handshakes;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;
using System.Collections.Concurrent;

namespace LiveStreamingServerNet.Rtmp.Server.Internal.Contracts
{
    internal interface IRtmpClientSessionContext : IRtmpChunkStreamContextProvider, IAsyncDisposable
    {
        ISessionHandle Client { get; }
        ConcurrentDictionary<string, object> Items { get; }

        RtmpClientSessionState State { get; set; }
        HandshakeType HandshakeType { get; set; }

        new uint InChunkSize { get; set; }
        uint OutChunkSize { get; set; }

        uint InWindowAcknowledgementSize { get; set; }
        uint OutWindowAcknowledgementSize { get; set; }

        uint SequenceNumber { get; set; }
        uint LastAcknowledgedSequenceNumber { get; set; }

        string? AppName { get; set; }

        uint GetNextChunkStreamId();
        IRtmpStreamContext CreateStreamContext();
        List<IRtmpStreamContext> GetStreamContexts();
        IRtmpStreamContext? GetStreamContext(uint streamId);
        void RemoveStreamContext(uint streamId);
        void Recycle(IDataBufferPool dataBufferPool);
    }
}

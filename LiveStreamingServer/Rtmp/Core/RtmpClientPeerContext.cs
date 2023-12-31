using LiveStreamingServer.Networking.Contracts;
using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.Utilities;
using System.Collections.Concurrent;

namespace LiveStreamingServer.Rtmp.Core
{
    public class RtmpClientPeerContext : IRtmpClientPeerContext
    {
        private readonly object _syncLock = new();

        public IClientPeerHandle Peer { get; }
        public RtmpClientPeerState State { get; set; } = RtmpClientPeerState.HandshakeC0;
        public HandshakeType HandshakeType { get; set; } = HandshakeType.SimpleHandshake;

        public uint InChunkSize { get; set; } = RtmpConstants.DefaultChunkSize;
        public uint OutChunkSize { get; set; } = RtmpConstants.DefaultChunkSize;

        public uint InWindowAcknowledgementSize { get; set; }
        public uint OutWindowAcknowledgementSize { get; set; }

        public string AppName { get; set; } = default!;

        public IRtmpPublishStreamContext? PublishStreamContext { get; private set; }

        private uint _publishStreamId;
        private ConcurrentDictionary<uint, IRtmpChunkStreamContext> _chunkStreamContexts = new();

        public RtmpClientPeerContext(IClientPeerHandle peer)
        {
            Peer = peer;
        }

        public IRtmpPublishStreamContext CreateNewPublishStream()
        {
            lock (_syncLock)
            {
                PublishStreamContext = new RtmpPublishStreamContext(Interlocked.Increment(ref _publishStreamId));
                return PublishStreamContext;
            }
        }

        public IRtmpChunkStreamContext GetChunkStreamContext(uint chunkStreamId)
        {
            return _chunkStreamContexts.GetOrAdd(chunkStreamId, new RtmpChunkStreamContext(chunkStreamId));
        }
    }

    public class RtmpPublishStreamContext : IRtmpPublishStreamContext
    {
        public uint StreamId { get; }
        public string StreamPath { get; set; } = default!;
        public IDictionary<string, string> StreamArguments { get; set; } = new Dictionary<string, string>();
        public IPublishStreamMetaData StreamMetaData { get; set; } = default!;
        public byte[]? VideoSequenceHeader { get; set; }
        public byte[]? AudioSequenceHeader { get; set; }

        public RtmpPublishStreamContext(uint streamId)
        {
            StreamId = streamId;
        }
    }

    public record PublishStreamMetaData : IPublishStreamMetaData
    {
        public uint VideoFrameRate { get; }
        public uint VideoWidth { get; }
        public uint VideoHeight { get; }

        public uint AudioSampleRate { get; }
        public uint AudioChannels { get; }

        public PublishStreamMetaData(
            uint videoFrameRate,
            uint videoWidth,
            uint videoHeight,
            uint audioSampleRate,
            bool stereo)
        {
            VideoFrameRate = videoFrameRate;
            VideoWidth = videoWidth;
            VideoHeight = videoHeight;
            AudioSampleRate = audioSampleRate;
            AudioChannels = stereo ? 2u : 1u;
        }
    }
}

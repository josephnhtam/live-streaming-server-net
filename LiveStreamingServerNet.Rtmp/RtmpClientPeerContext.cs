using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.Handshakes;
using System.Collections.Concurrent;

namespace LiveStreamingServerNet.Rtmp
{
    internal class RtmpClientPeerContext : IRtmpClientPeerContext
    {
        public IClientPeerHandle Peer { get; }
        public RtmpClientPeerState State { get; set; } = RtmpClientPeerState.HandshakeC0;
        public HandshakeType HandshakeType { get; set; } = HandshakeType.SimpleHandshake;

        public uint InChunkSize { get; set; } = RtmpConstants.DefaultChunkSize;
        public uint OutChunkSize { get; set; } = RtmpConstants.DefaultChunkSize;

        public uint InWindowAcknowledgementSize { get; set; }
        public uint OutWindowAcknowledgementSize { get; set; }

        public uint SequenceNumber { get; set; }
        public uint LastAcknowledgedSequenceNumber { get; set; }

        public string AppName { get; set; } = default!;
        public uint? StreamId => _isStreamCreated ? _streamId : null;

        public IRtmpPublishStreamContext? PublishStreamContext { get; private set; }
        public IRtmpStreamSubscriptionContext? StreamSubscriptionContext { get; private set; }

        private uint _streamId;
        private bool _isStreamCreated;
        private ConcurrentDictionary<uint, IRtmpChunkStreamContext> _chunkStreamContexts = new();

        public RtmpClientPeerContext(IClientPeerHandle peer)
        {
            Peer = peer;
        }

        public IRtmpPublishStreamContext CreatePublishStreamContext(string streamPath, IDictionary<string, string> streamArguments)
        {
            var newContext = new RtmpPublishStreamContext(_streamId, streamPath, streamArguments);
            PublishStreamContext = newContext;
            return newContext;
        }

        public IRtmpStreamSubscriptionContext CreateStreamSubscriptionContext(uint chunkStreamId, string streamPath, IDictionary<string, string> streamArguments)
        {
            var newContext = new RtmpStreamSubscriptionContext(_streamId, chunkStreamId, streamPath, streamArguments);
            StreamSubscriptionContext = newContext;
            return newContext;
        }

        public IRtmpChunkStreamContext GetChunkStreamContext(uint chunkStreamId)
        {
            return _chunkStreamContexts.GetOrAdd(chunkStreamId, new RtmpChunkStreamContext(chunkStreamId));
        }

        public uint CreateNewStream()
        {
            var streamId = Interlocked.Increment(ref _streamId);
            _isStreamCreated = true;
            return streamId;
        }

        public void DeleteStream()
        {
            _isStreamCreated = false;
            PublishStreamContext = null;
            StreamSubscriptionContext = null;
        }
    }

    internal class RtmpPublishStreamContext : IRtmpPublishStreamContext
    {
        public uint StreamId { get; }
        public string StreamPath { get; }
        public IDictionary<string, string> StreamArguments { get; }
        public IPublishStreamMetaData StreamMetaData { get; set; } = default!;
        public byte[]? VideoSequenceHeader { get; set; }
        public byte[]? AudioSequenceHeader { get; set; }

        public RtmpPublishStreamContext(uint streamId, string streamPath, IDictionary<string, string> streamArguments)
        {
            StreamId = streamId;
            StreamPath = streamPath;
            StreamArguments = streamArguments;
        }
    }

    internal record PublishStreamMetaData : IPublishStreamMetaData
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

    internal class RtmpStreamSubscriptionContext : IRtmpStreamSubscriptionContext
    {
        public uint StreamId { get; }
        public uint ChunkStreamId { get; }
        public string StreamPath { get; }
        public IDictionary<string, string> StreamArguments { get; }

        public bool IsPaused { get; set; }
        public bool IsReceivingAudio { get; set; }
        public bool IsReceivingVideo { get; set; }

        public RtmpStreamSubscriptionContext(uint streamId, uint chunkStreamId, string streamPath, IDictionary<string, string> streamArguments)
        {
            StreamId = streamId;
            ChunkStreamId = chunkStreamId;
            StreamPath = streamPath;
            StreamArguments = streamArguments;

            IsPaused = false;
            IsReceivingAudio = true;
            IsReceivingVideo = true;
        }
    }
}

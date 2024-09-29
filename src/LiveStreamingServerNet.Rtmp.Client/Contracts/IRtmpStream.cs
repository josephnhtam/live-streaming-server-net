using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Client.Contracts
{
    public interface IRtmpStream
    {
        uint StreamId { get; }

        IRtmpPublishStream Publish { get; }
        IRtmpSubscribeStream Subscribe { get; }
    }

    public interface IRtmpPublishStream { }

    public interface IRtmpSubscribeStream
    {
        IReadOnlyDictionary<string, object>? StreamMetaData { get; set; }

        void Play(string streamName);
        void Play(string streamName, double start, double duration, bool reset);

        event EventHandler<IReadOnlyDictionary<string, object>> OnStreamMetaDataUpdated;
        event EventHandler<IRentedBuffer> OnVideoDataReceived;
        event EventHandler<IRentedBuffer> OnAudioDataReceived;
    }
}

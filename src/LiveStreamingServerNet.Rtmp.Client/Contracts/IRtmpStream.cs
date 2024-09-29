using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Client.Contracts
{
    public interface IRtmpStream
    {
        uint StreamId { get; }

        IRtmpPublishStream Publish { get; }
        IRtmpSubscribeStream Subscribe { get; }

        void CloseStream();
        void DeleteStream();
    }

    public interface IRtmpPublishStream { }

    public interface IRtmpSubscribeStream
    {
        IReadOnlyDictionary<string, object>? StreamMetaData { get; set; }

        void Play(string streamName);
        void Play(string streamName, double start, double duration, bool reset);

        event EventHandler<IReadOnlyDictionary<string, object>> OnStreamMetaDataReceived;
        event EventHandler<IRentedBuffer> OnVideoDataReceived;
        event EventHandler<IRentedBuffer> OnAudioDataReceived;
        event EventHandler<StatusEventArgs> OnStatusReceived;
    }

    public record struct StatusEventArgs(string Level, string Code, string Description);
}

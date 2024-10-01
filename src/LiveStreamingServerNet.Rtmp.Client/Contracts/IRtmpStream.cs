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

        void Command(RtmpCommand command);
        Task<RtmpCommandResponse> CommandAsync(RtmpCommand command);
    }

    public interface IRtmpPublishStream { }

    public interface IRtmpSubscribeStream
    {
        IReadOnlyDictionary<string, object>? StreamMetaData { get; set; }

        void Play(string streamName);
        void Play(string streamName, double start, double duration, bool reset);

        event EventHandler<StreamMetaDataEventArgs> OnStreamMetaDataReceived;
        event EventHandler<MediaDataEventArgs> OnVideoDataReceived;
        event EventHandler<MediaDataEventArgs> OnAudioDataReceived;
        event EventHandler<StatusEventArgs> OnStatusReceived;
        event EventHandler<UserControlEventArgs> OnUserControlEventReceived;
    }

    public record struct StreamMetaDataEventArgs(IReadOnlyDictionary<string, object> StreamMetaData);
    public record struct StatusEventArgs(string Level, string Code, string Description);
    public record struct MediaDataEventArgs(IRentedBuffer RentedBuffer, uint Timestamp);
    public record struct UserControlEventArgs(UserControlEventType EventType);

    public enum UserControlEventType
    {
        StreamBegin = 0,
        StreamEOF = 1,
        StreamDry = 2,
        StreamIsRecorded = 4,
    }
}

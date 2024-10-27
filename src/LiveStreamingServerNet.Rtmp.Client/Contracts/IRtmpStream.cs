using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Client.Contracts
{
    /// <summary>
    /// Represents an RTMP stream that can be used for publishing or subscribing to media content.
    /// </summary>
    public interface IRtmpStream
    {
        /// <summary>
        /// Gets the unique identifier for this stream assigned by the server.
        /// </summary>
        uint StreamId { get; }

        /// <summary>
        /// Gets the interface for publishing media through this stream.
        /// </summary>
        IRtmpPublishStream Publish { get; }

        /// <summary>
        /// Gets the interface for subscribing to media through this stream.
        /// </summary>
        IRtmpSubscribeStream Subscribe { get; }

        /// <summary>
        /// Closes the stream.
        /// </summary>
        void CloseStream();

        /// <summary>
        /// Deletes the stream.
        /// </summary>
        void DeleteStream();

        /// <summary>
        /// Enqueues sending a command through this stream.
        /// </summary>
        /// <param name="command">Command to send</param>
        void Command(RtmpCommand command);

        /// <summary>
        /// Enqueues sending a command through this stream and waits for response.
        /// </summary>
        /// <param name="command">Command to send</param>
        /// <returns>Server's response to the command</returns>
        Task<RtmpCommandResponse> CommandAsync(RtmpCommand command);

        /// <summary>
        /// Event raised when status messages are received for this stream.
        /// </summary>
        event EventHandler<StatusEventArgs> OnStatusReceived;

        /// <summary>
        /// Event raised when user control messages are received for this stream.
        /// </summary>
        event EventHandler<UserControlEventArgs> OnUserControlEventReceived;
    }

    /// <summary>
    /// Interface for publishing media content through an RTMP stream.
    /// </summary>
    public interface IRtmpPublishStream
    {
        /// <summary>
        /// Starts publishing to the specified stream name with type "live".
        /// </summary>
        /// <param name="streamName">Name of the stream to publish</param>
        void Publish(string streamName);

        /// <summary>
        /// Starts publishing to the specified stream name with custom type.
        /// </summary>
        /// <param name="streamName">Name of the stream to publish</param>
        /// <param name="type">Type of publishing (live, record, append)</param>
        void Publish(string streamName, string type);

        /// <summary>
        /// Enqueue sending stream metadata to viewers.
        /// </summary>
        /// <param name="metaData">Metadata key-value pairs</param>
        ValueTask SendMetaDataAsync(IReadOnlyDictionary<string, object> metaData);

        /// <summary>
        /// Enqueue sending audio data to viewers.
        /// </summary>
        /// <param name="payload">Audio data buffer</param>
        /// <param name="timestamp">Timestamp of the audio data</param>
        ValueTask SendAudioDataAsync(IRentedBuffer payload, uint timestamp);

        /// <summary>
        /// Enqueue sending video data to viewers.
        /// </summary>
        /// <param name="payload">Video data buffer</param>
        /// <param name="timestamp">Timestamp of the video data</param>
        ValueTask SendVideoDataAsync(IRentedBuffer payload, uint timestamp);
    }

    /// <summary>
    /// Interface for subscribing to media content through an RTMP stream.
    /// </summary>
    public interface IRtmpSubscribeStream
    {
        /// <summary>
        /// Gets the current stream metadata if available.
        /// </summary>
        IReadOnlyDictionary<string, object>? StreamMetaData { get; }

        /// <summary>
        /// Starts playing the specified stream from the beginning.
        /// </summary>
        /// <param name="streamName">Name of the stream to play</param>
        void Play(string streamName);

        /// <summary>
        /// Starts playing the specified stream with custom parameters.
        /// </summary>
        /// <param name="streamName">Name of the stream to play</param>
        /// <param name="start">Start position in seconds (-2: live edge, -1: live, >=0: timestamp)</param>
        /// <param name="duration">Duration to play in seconds (0: until end)</param>
        /// <param name="reset">Whether to reset any existing stream state</param>
        void Play(string streamName, double start, double duration, bool reset);

        /// <summary>
        /// Event raised when stream metadata is received.
        /// </summary>
        event EventHandler<StreamMetaDataEventArgs> OnStreamMetaDataReceived;

        /// <summary>
        /// Event raised when video data is received.
        /// </summary>
        event EventHandler<MediaDataEventArgs> OnVideoDataReceived;

        /// <summary>
        /// Event raised when audio data is received.
        /// </summary>
        event EventHandler<MediaDataEventArgs> OnAudioDataReceived;
    }

    /// <summary>
    /// Event arguments containing stream metadata information.
    /// </summary>
    /// <param name="StreamMetaData">Metadata key-value pairs</param>
    public record struct StreamMetaDataEventArgs(IReadOnlyDictionary<string, object> StreamMetaData);

    /// <summary>
    /// Event arguments containing stream status information.
    /// </summary>
    /// <param name="Level">Status level (e.g., "status", "error")</param>
    /// <param name="Code">Status code identifier</param>
    /// <param name="Description">Human-readable status description</param>
    public record struct StatusEventArgs(string Level, string Code, string Description);

    /// <summary>
    /// Event arguments containing media data.
    /// </summary>
    /// <param name="RentedBuffer">Buffer containing media data</param>
    /// <param name="Timestamp">Timestamp of the media data</param>
    public record struct MediaDataEventArgs(IRentedBuffer RentedBuffer, uint Timestamp);

    /// <summary>
    /// Event arguments for user control messages.
    /// </summary>
    /// <param name="EventType">Type of user control event</param>
    public record struct UserControlEventArgs(UserControlEventType EventType);

    /// <summary>
    /// Types of user control events that can be received from server.
    /// </summary>
    public enum UserControlEventType
    {
        StreamBegin = 0,
        StreamEOF = 1,
        StreamDry = 2,
        StreamIsRecorded = 4,
    }
}

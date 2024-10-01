using LiveStreamingServerNet.Rtmp.Client.Contracts;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Contracts
{
    internal interface IRtmpStreamContext : IDisposable
    {
        uint StreamId { get; }
        uint CommandChunkStreamId { get; }

        IRtmpSessionContext SessionContext { get; }

        IRtmpPublishStreamContext? PublishContext { get; }
        IRtmpSubscribeStreamContext? SubscribeContext { get; }

        IRtmpPublishStreamContext CreatePublishContext();
        IRtmpSubscribeStreamContext CreateSubscribeContext();

        void RemovePublishContext();
        void RemoveSubscribeContext();

        void ReceiveStatus(StatusEventArgs eventArgs);
        void ReceiveUserControlEvent(UserControlEventArgs eventArgs);

        event EventHandler<StatusEventArgs> OnStatusReceived;
        event EventHandler<UserControlEventArgs> OnUserControlEventReceived;

        event EventHandler<IRtmpPublishStreamContext> OnPublishContextCreated;
        event EventHandler<IRtmpSubscribeStreamContext> OnSubscribeContextCreated;
        event EventHandler<IRtmpPublishStreamContext> OnPublishContextRemoved;
        event EventHandler<IRtmpSubscribeStreamContext> OnSubscribeContextRemoved;
    }

    internal interface IRtmpMediaStreamContext : IDisposable
    {
        IRtmpStreamContext StreamContext { get; }
    }

    internal interface IRtmpPublishStreamContext : IRtmpMediaStreamContext
    {
        uint DataChunkStreamId { get; }
        uint AudioChunkStreamId { get; }
        uint VideoChunkStreamId { get; }
    }

    internal interface IRtmpSubscribeStreamContext : IRtmpMediaStreamContext
    {
        IReadOnlyDictionary<string, object>? StreamMetaData { get; set; }
        void ReceiveVideoData(MediaDataEventArgs rentedBuffer);
        void ReceiveAudioData(MediaDataEventArgs rentedBuffer);

        event EventHandler<StreamMetaDataEventArgs> OnStreamMetaDataReceived;
        event EventHandler<MediaDataEventArgs> OnVideoDataReceived;
        event EventHandler<MediaDataEventArgs> OnAudioDataReceived;
    }
}

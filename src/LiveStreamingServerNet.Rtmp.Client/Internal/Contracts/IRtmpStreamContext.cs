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
    }

    internal interface IRtmpSubscribeStreamContext : IRtmpMediaStreamContext
    {
        IReadOnlyDictionary<string, object>? StreamMetaData { get; set; }
        void ReceiveVideoData(MediaDataEventArgs rentedBuffer);
        void ReceiveAudioData(MediaDataEventArgs rentedBuffer);
        void ReceiveStatus(StatusEventArgs eventArgs);
        void ReceiveUserControlEvent(UserControlEventArgs eventArgs);

        event EventHandler<StreamMetaDataEventArgs> OnStreamMetaDataReceived;
        event EventHandler<MediaDataEventArgs> OnVideoDataReceived;
        event EventHandler<MediaDataEventArgs> OnAudioDataReceived;
        event EventHandler<StatusEventArgs> OnStatusReceived;
        event EventHandler<UserControlEventArgs> OnUserControlEventReceived;
    }
}

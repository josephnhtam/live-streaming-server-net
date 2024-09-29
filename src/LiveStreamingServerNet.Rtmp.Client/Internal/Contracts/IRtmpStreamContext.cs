using LiveStreamingServerNet.Rtmp.Client.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Contracts
{
    internal interface IRtmpStreamContext : IDisposable
    {
        uint StreamId { get; }

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
        void ReceiveVideoData(IRentedBuffer rentedBuffer);
        void ReceiveAudioData(IRentedBuffer rentedBuffer);
        void ReceiveStatus(StatusEventArgs eventArgs);

        event EventHandler<IReadOnlyDictionary<string, object>> OnStreamMetaDataUpdated;
        event EventHandler<IRentedBuffer> OnVideoDataReceived;
        event EventHandler<IRentedBuffer> OnAudioDataReceived;
        event EventHandler<StatusEventArgs> OnStatusReceived;
    }
}

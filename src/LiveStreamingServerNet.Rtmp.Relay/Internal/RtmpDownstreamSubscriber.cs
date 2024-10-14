using LiveStreamingServerNet.Rtmp.Relay.Contracts;

namespace LiveStreamingServerNet.Rtmp.Relay.Internal
{
    internal class RtmpDownstreamSubscriber : IRtmpDownstreamSubscriber
    {
        public string StreamPath { get; }

        private readonly Action<IRtmpDownstreamSubscriber> _disposeCallback;
        private bool _disposed;


        public RtmpDownstreamSubscriber(string streamPath, Action<IRtmpDownstreamSubscriber> disposeCallback)
        {
            StreamPath = streamPath;
            _disposeCallback = disposeCallback;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _disposeCallback(this);
        }
    }
}

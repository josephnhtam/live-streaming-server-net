using LiveStreamingServerNet.Rtmp.Relay.Contracts;
using LiveStreamingServerNet.Rtmp.Relay.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Relay.Internal
{
    internal class RtmpRelayManager : IRtmpRelayManager
    {
        private readonly IRtmpDownstreamManagerService _downstreamManager;

        public RtmpRelayManager(IRtmpDownstreamManagerService downstreamManager)
        {
            _downstreamManager = downstreamManager;
        }

        public Task<IRtmpDownstreamSubscriber?> RequestDownstreamAsync(string streamPath, CancellationToken cancellationToken = default)
        {
            return _downstreamManager.RequestDownstreamAsync(streamPath);
        }

        public bool IsDownstreamRequested(string streamPath)
        {
            return _downstreamManager.IsDownstreamRequested(streamPath);
        }
    }
}

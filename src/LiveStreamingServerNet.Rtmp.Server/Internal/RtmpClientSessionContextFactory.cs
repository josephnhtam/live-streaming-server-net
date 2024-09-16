using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Server.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Server.Internal
{
    internal class RtmpClientSessionContextFactory : IRtmpClientSessionContextFactory
    {
        private readonly IBufferPool? _bufferPool;

        public RtmpClientSessionContextFactory(IBufferPool? bufferPool = null)
        {
            _bufferPool = bufferPool;
        }

        public IRtmpClientSessionContext Create(ISessionHandle client)
        {
            return new RtmpClientSessionContext(client, _bufferPool);
        }
    }
}

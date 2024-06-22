using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Internal
{
    internal class RtmpClientContextFactory : IRtmpClientContextFactory
    {
        private readonly IBufferPool? _bufferPool;

        public RtmpClientContextFactory(IBufferPool? bufferPool = null)
        {
            _bufferPool = bufferPool;
        }

        public IRtmpClientContext Create(IClientHandle client)
        {
            return new RtmpClientContext(client, _bufferPool);
        }
    }
}

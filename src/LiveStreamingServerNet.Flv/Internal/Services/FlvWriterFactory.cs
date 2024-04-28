using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Services
{
    internal class FlvWriterFactory : IFlvWriterFactory
    {
        private readonly INetBufferPool _netBufferPool;

        public FlvWriterFactory(INetBufferPool netBufferPool)
        {
            _netBufferPool = netBufferPool;
        }

        public IFlvWriter Create(IStreamWriter streamWriter)
        {
            return new FlvWriter(streamWriter, _netBufferPool);
        }
    }
}

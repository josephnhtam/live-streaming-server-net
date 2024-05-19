using LiveStreamingServerNet.Transmuxer.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Hls.Services.Contracts;

namespace LiveStreamingServerNet.Transmuxer.Internal.Hls
{
    internal class HlsTransmuxerFactory : ITransmuxerFactory
    {
        private readonly IHlsTransmuxerManager _transmuxerManager;

        public HlsTransmuxerFactory(IHlsTransmuxerManager transmuxerManager)
        {
            _transmuxerManager = transmuxerManager;
        }

        public Task<ITransmuxer> CreateAsync(
            Guid contextIdentifier, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            ITransmuxer tranmuxer = new HlsTransmuxer("HLS Transmuxer", contextIdentifier, _transmuxerManager);
            return Task.FromResult(tranmuxer);
        }
    }
}

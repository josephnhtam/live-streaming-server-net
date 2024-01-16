using LiveStreamingServerNet.Transmuxer.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Transmuxer.Internal.Services
{
    internal class TransmuxerManager : ITransmuxerManager
    {
        public ValueTask StartRemuxingStreamAsync(string streamPath, IDictionary<string, string> streamArguments)
        {
            throw new NotImplementedException();
        }

        public ValueTask StopRemuxingStreamAsync(string streamPath)
        {
            throw new NotImplementedException();
        }
    }
}

using LiveStreamingServerNet.Transmuxer.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Contracts;

namespace LiveStreamingServerNet.Transmuxer.Internal
{
    internal class TransmuxerEventDispatcher : ITransmuxerEventDispatcher
    {
        private readonly IEnumerable<ITransmuxerEventHandler> _handlers;

        public TransmuxerEventDispatcher(IEnumerable<ITransmuxerEventHandler> handlers)
        {
            _handlers = handlers;
        }

        public async Task TransmuxerStartedAsync(uint clientId, string identifier, string inputPath, string outputPath, string streamPath, IDictionary<string, string> streamArguments)
        {
            foreach (var handler in _handlers)
                await handler.OnTransmuxerStartedAsync(clientId, identifier, inputPath, outputPath, streamPath, streamArguments);
        }

        public async Task TransmuxerStoppedAsync(uint clientId, string identifier, string inputPath, string outputPath, string streamPath, IDictionary<string, string> streamArguments)
        {
            foreach (var handler in _handlers)
                await handler.OnTransmuxerStoppedAsync(clientId, identifier, inputPath, outputPath, streamPath, streamArguments);
        }
    }
}

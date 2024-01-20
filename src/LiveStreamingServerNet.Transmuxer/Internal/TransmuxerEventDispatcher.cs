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

        public async Task TransmuxerStartedAsync(uint clientId, string inputPath, string outputPath, string streamPath, IDictionary<string, string> streamArguments)
        {
            foreach (var handler in _handlers)
                await handler.OnTransmuxerStartedAsync(clientId, inputPath, outputPath, streamPath, streamArguments);
        }

        public async Task TransmuxerStoppedAsync(uint clientId, string inputPath, string outputPath, string streamPath, IDictionary<string, string> streamArguments)
        {
            foreach (var handler in _handlers)
                await handler.OnTransmuxerStoppedAsync(clientId, inputPath, outputPath, streamPath, streamArguments);
        }
    }
}

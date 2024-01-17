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

        public async Task TransmuxerStartedAsync(string inputPath, string outputPath, string streamPath, IDictionary<string, string> streamArguments)
        {
            foreach (var handler in _handlers)
                await handler.OnTransmuxerStartedAsync(inputPath, outputPath, streamPath, streamArguments);
        }

        public async Task TransmuxerStoppedAsync(string inputPath, string outputPath, string streamPath, IDictionary<string, string> streamArguments)
        {
            foreach (var handler in _handlers)
                await handler.OnTransmuxerStoppedAsync(inputPath, outputPath, streamPath, streamArguments);
        }
    }
}

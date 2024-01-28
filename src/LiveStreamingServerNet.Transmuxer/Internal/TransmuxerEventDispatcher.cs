using LiveStreamingServerNet.Transmuxer.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace LiveStreamingServerNet.Transmuxer.Internal
{
    internal class TransmuxerEventDispatcher : ITransmuxerEventDispatcher
    {
        private readonly IServiceProvider _services;
        private ITransmuxerEventHandler[]? _eventHandlers;

        public TransmuxerEventDispatcher(IServiceProvider services)
        {
            _services = services;
        }

        public ITransmuxerEventHandler[] GetEventHandlers()
        {
            _eventHandlers ??= _services.GetServices<ITransmuxerEventHandler>().ToArray();
            return _eventHandlers;
        }

        public async Task TransmuxerStartedAsync(uint clientId, string identifier, string inputPath, string outputPath, string streamPath, IDictionary<string, string> streamArguments)
        {
            foreach (var handler in GetEventHandlers())
                await handler.OnTransmuxerStartedAsync(clientId, identifier, inputPath, outputPath, streamPath, streamArguments);
        }

        public async Task TransmuxerStoppedAsync(uint clientId, string identifier, string inputPath, string outputPath, string streamPath, IDictionary<string, string> streamArguments)
        {
            foreach (var handler in GetEventHandlers())
                await handler.OnTransmuxerStoppedAsync(clientId, identifier, inputPath, outputPath, streamPath, streamArguments);
        }
    }
}

using LiveStreamingServerNet.Transmuxer.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Transmuxer.Internal
{
    internal class TransmuxerEventDispatcher : ITransmuxerEventDispatcher
    {
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;

        private ITransmuxerEventHandler[]? _eventHandlers;

        public TransmuxerEventDispatcher(IServiceProvider services, ILogger<TransmuxerEventDispatcher> logger)
        {
            _services = services;
            _logger = logger;
        }

        public ITransmuxerEventHandler[] GetEventHandlers()
        {
            _eventHandlers ??= _services.GetServices<ITransmuxerEventHandler>().ToArray();
            return _eventHandlers;
        }

        public async Task TransmuxerStartedAsync(uint clientId, string identifier, string inputPath, string outputPath, string streamPath, IDictionary<string, string> streamArguments)
        {
            try
            {
                foreach (var handler in GetEventHandlers())
                    await handler.OnTransmuxerStartedAsync(clientId, identifier, inputPath, outputPath, streamPath, streamArguments);
            }
            catch (Exception ex)
            {
                _logger.DispatchingTransmuxerStartedEventError(identifier, inputPath, outputPath, streamPath, ex);
            }
        }

        public async Task TransmuxerStoppedAsync(uint clientId, string identifier, string inputPath, string outputPath, string streamPath, IDictionary<string, string> streamArguments)
        {
            try
            {
                foreach (var handler in GetEventHandlers())
                    await handler.OnTransmuxerStoppedAsync(clientId, identifier, inputPath, outputPath, streamPath, streamArguments);
            }
            catch (Exception ex)
            {
                _logger.DispatchingTransmuxerStoppedEventError(identifier, inputPath, outputPath, streamPath, ex);
            }
        }
    }
}

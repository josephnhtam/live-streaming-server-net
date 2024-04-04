using LiveStreamingServerNet.Transmuxer.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Logging;
using LiveStreamingServerNet.Utilities;
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
            _eventHandlers ??= _services.GetServices<ITransmuxerEventHandler>().OrderBy(x => x.GetOrder()).ToArray();
            return _eventHandlers;
        }

        public async Task TransmuxerStartedAsync(string transmuxer, Guid identifier, uint clientId, string inputPath, string outputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            using var context = EventContext.Obtain();

            try
            {
                foreach (var handler in GetEventHandlers())
                    await handler.OnTransmuxerStartedAsync(context, transmuxer, identifier, clientId, inputPath, outputPath, streamPath, streamArguments);
            }
            catch (Exception ex)
            {
                _logger.DispatchingTransmuxerStartedEventError(transmuxer, identifier, inputPath, outputPath, streamPath, ex);
            }
        }

        public async Task TransmuxerStoppedAsync(string transmuxer, Guid identifier, uint clientId, string inputPath, string outputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            using var context = EventContext.Obtain();

            try
            {
                foreach (var handler in GetEventHandlers())
                    await handler.OnTransmuxerStoppedAsync(context, transmuxer, identifier, clientId, inputPath, outputPath, streamPath, streamArguments);
            }
            catch (Exception ex)
            {
                _logger.DispatchingTransmuxerStoppedEventError(transmuxer, identifier, inputPath, outputPath, streamPath, ex);
            }
        }
    }
}

using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Logging;
using LiveStreamingServerNet.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.Internal
{
    internal class StreamProcessorEventDispatcher : IStreamProcessorEventDispatcher
    {
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;

        private IStreamProcessorEventHandler[]? _eventHandlers;

        public StreamProcessorEventDispatcher(IServiceProvider services, ILogger<StreamProcessorEventDispatcher> logger)
        {
            _services = services;
            _logger = logger;
        }

        public IStreamProcessorEventHandler[] GetEventHandlers()
        {
            _eventHandlers ??= _services.GetServices<IStreamProcessorEventHandler>().OrderBy(x => x.GetOrder()).ToArray();
            return _eventHandlers;
        }

        public async Task StreamProcssorStartedAsync(string processor, Guid identifier, uint clientId, string inputPath, string outputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            using var context = EventContext.Obtain();

            try
            {
                foreach (var handler in GetEventHandlers())
                    await handler.OnStreamProcessorStartedAsync(context, processor, identifier, clientId, inputPath, outputPath, streamPath, streamArguments);
            }
            catch (Exception ex)
            {
                _logger.DispatchingStreamProcessorStartedEventError(processor, identifier, inputPath, outputPath, streamPath, ex);
            }
        }

        public async Task StreamProcessorStoppedAsync(string processor, Guid identifier, uint clientId, string inputPath, string outputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            using var context = EventContext.Obtain();

            try
            {
                foreach (var handler in GetEventHandlers())
                    await handler.OnStreamProcessorStoppedAsync(context, processor, identifier, clientId, inputPath, outputPath, streamPath, streamArguments);
            }
            catch (Exception ex)
            {
                _logger.DispatchingStreamProcessorStoppedEventError(processor, identifier, inputPath, outputPath, streamPath, ex);
            }
        }
    }
}

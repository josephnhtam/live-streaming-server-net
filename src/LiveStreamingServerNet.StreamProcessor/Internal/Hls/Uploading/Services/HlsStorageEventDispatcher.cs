using LiveStreamingServerNet.StreamProcessor.Hls;
using LiveStreamingServerNet.StreamProcessor.Hls.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Uploading.Services.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Logging;
using LiveStreamingServerNet.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Uploading.Services
{
    internal class HlsStorageEventDispatcher : IHlsStorageEventDispatcher
    {
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;

        private IHlsStorageEventHandler[]? _eventHandlers;

        public HlsStorageEventDispatcher(IServiceProvider services, ILogger<HlsStorageEventDispatcher> logger)
        {
            _services = services;
            _logger = logger;
        }

        public IHlsStorageEventHandler[] GetEventHandlers()
        {
            _eventHandlers ??= _services.GetServices<IHlsStorageEventHandler>().OrderBy(x => x.GetOrder()).ToArray();
            return _eventHandlers;
        }

        public async Task HlsFilesStoredAsync(StreamProcessingContext context, bool initial, IReadOnlyList<StoredManifest> storedManifests, IReadOnlyList<StoredTsSegment> storedTsSegments)
        {
            using var eventContext = EventContext.Obtain();

            try
            {
                foreach (var handler in GetEventHandlers())
                    await handler.OnHlsFilesStoredAsync(eventContext, context, initial, storedManifests, storedTsSegments);
            }
            catch (Exception ex)
            {
                _logger.DispatchingHlsFilesStoredEventError(
                    context.Processor, context.Identifier, context.InputPath, context.OutputPath, context.StreamPath, ex);
            }
        }

        public async Task HlsFilesStoringCompleteAsync(StreamProcessingContext context)
        {
            using var eventContext = EventContext.Obtain();

            try
            {
                foreach (var handler in GetEventHandlers())
                    await handler.OnHlsFilesStoringCompleteAsync(eventContext, context);
            }
            catch (Exception ex)
            {
                _logger.DispatchingHlsFilesStoringCompleteEventError(
                    context.Processor, context.Identifier, context.InputPath, context.OutputPath, context.StreamPath, ex);
            }
        }
    }
}

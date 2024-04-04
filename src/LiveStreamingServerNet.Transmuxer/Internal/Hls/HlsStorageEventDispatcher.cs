using LiveStreamingServerNet.Transmuxer.Hls;
using LiveStreamingServerNet.Transmuxer.Hls.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Hls.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Logging;
using LiveStreamingServerNet.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Transmuxer.Internal.Hls
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

        public async Task HlsFilesStoredAsync(TransmuxingContext context, bool initial, IReadOnlyList<StoredManifest> storedManifests, IReadOnlyList<StoredTsFile> storedTsFiles)
        {
            using var eventContext = EventContext.Obtain();

            try
            {
                foreach (var handler in GetEventHandlers())
                    await handler.OnHlsFilesStoredAsync(eventContext, context, initial, storedManifests, storedTsFiles);
            }
            catch (Exception ex)
            {
                _logger.DispatchingHlsFilesStoredEventError(
                    context.Transmuxer, context.Identifier, context.InputPath, context.OutputPath, context.StreamPath, ex);
            }
        }

        public async Task HlsFilesStoringCompleteAsync(TransmuxingContext context)
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
                    context.Transmuxer, context.Identifier, context.InputPath, context.OutputPath, context.StreamPath, ex);
            }
        }
    }
}

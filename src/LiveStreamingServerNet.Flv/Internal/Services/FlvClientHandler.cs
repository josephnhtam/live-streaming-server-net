using LiveStreamingServerNet.Flv.Configurations;
using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Logging;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveStreamingServerNet.Flv.Internal.Services
{
    internal class FlvClientHandler : IFlvClientHandler
    {
        private readonly IFlvStreamManagerService _streamManager;
        private readonly IFlvMediaTagCacherService _mediaTagCacher;
        private readonly MediaStreamingConfiguration _config;
        private readonly ILogger _logger;

        public FlvClientHandler(
            IFlvStreamManagerService streamManager,
            IFlvMediaTagCacherService mediaTagCacher,
            IOptions<MediaStreamingConfiguration> config,
            ILogger<FlvClientHandler> logger)
        {
            _streamManager = streamManager;
            _mediaTagCacher = mediaTagCacher;
            _config = config.Value;
            _logger = logger;
        }

        public async Task RunClientAsync(IFlvClient client, CancellationToken cancellationToken)
        {
            try
            {
                var streamContext = _streamManager.GetFlvStreamContext(client.StreamPath)!;
                await UntilReadyAsync(streamContext, cancellationToken);

                await SendFlvHeaderAsync(client, streamContext, client.StoppingToken);
                await SendCachedFlvTagsAsync(client, streamContext, client.StoppingToken);

                client.CompleteInitialization();
                await client.UntilCompleteAsync(cancellationToken);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.RunClientError(client.StreamPath, ex);
                throw;
            }
            finally
            {
                _streamManager.StopSubscribingStream(client);
            }
        }

        private async Task UntilReadyAsync(IFlvStreamContext streamContext, CancellationToken cancellationToken)
        {
            using var timeoutCts = new CancellationTokenSource(_config.ReadinessTimeout);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            try
            {
                await streamContext.UntilReadyAsync(cts.Token);
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
            {
                _logger.ReadinessTimeout(streamContext.StreamPath);
                throw new TimeoutException();
            }
        }

        private static async ValueTask SendFlvHeaderAsync(IFlvClient client, IFlvStreamContext streamContext, CancellationToken cancellationToken)
        {
            var hasAudio = streamContext.AudioSequenceHeader != null;
            var hasVideo = streamContext.VideoSequenceHeader != null;

            await client.WriteHeaderAsync(hasAudio, hasVideo, cancellationToken);
        }

        private async ValueTask SendCachedFlvTagsAsync(IFlvClient client, IFlvStreamContext streamContext, CancellationToken cancellationToken)
        {
            await _mediaTagCacher.SendCachedHeaderTagsAsync(client, streamContext, 0, cancellationToken);
            await _mediaTagCacher.SendCachedGroupOfPicturesTagsAsync(client, streamContext, cancellationToken);
        }
    }
}

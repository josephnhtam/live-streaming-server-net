using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Services
{
    internal class FlvClientHandler : IFlvClientHandler
    {
        private readonly IFlvStreamManagerService _streamManager;
        private readonly IFlvMediaTagCacherService _mediaTagCacher;

        public FlvClientHandler(IFlvStreamManagerService streamManager, IFlvMediaTagCacherService mediaTagCacher)
        {
            _streamManager = streamManager;
            _mediaTagCacher = mediaTagCacher;
        }

        public async Task RunClientAsync(IFlvClient client)
        {
            try
            {
                var streamContext = _streamManager.GetFlvStreamContext(client.StreamPath)!;

                await SendFlvHeaderAsync(client, streamContext, client.StoppingToken);
                await SendCachedFlvTagsAsync(client, streamContext, client.StoppingToken);

                client.CompleteInitialization();
                await client.UntilComplete();
            }
            finally
            {
                _streamManager.StopSubscribingStream(client);
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

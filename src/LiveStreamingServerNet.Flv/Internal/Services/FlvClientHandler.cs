using LiveStreamingServerNet.Flv.Internal.Contracts;
using LiveStreamingServerNet.Flv.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Services
{
    internal class FlvClientHandler : IFlvClientHandler
    {
        private readonly IFlvStreamManagerService _streamManager;
        private readonly IFlvMediaTagManagerService _mediaTagManager;

        public FlvClientHandler(IFlvStreamManagerService streamManager, IFlvMediaTagManagerService mediaTagManager)
        {
            _streamManager = streamManager;
            _mediaTagManager = mediaTagManager;
        }

        public async Task RunClientAsync(IFlvClient client)
        {
            try
            {
                await SendFlvHeaderAsync(client, client.StoppingToken);
                await SendCachedFlvTagsAsync(client, client.StoppingToken);

                client.CompleteInitialization();
                await client.UntilComplete();
            }
            finally
            {
                _streamManager.StopSubscribingStream(client);
            }
        }

        private static async Task SendFlvHeaderAsync(IFlvClient client, CancellationToken cancellationToken)
        {
            await client.FlvWriter.WriteHeaderAsync(true, true, cancellationToken);
        }

        private async Task SendCachedFlvTagsAsync(IFlvClient client, CancellationToken cancellationToken)
        {
            var streamContext = _streamManager.GetFlvStreamContext(client.StreamPath)!;

            await _mediaTagManager.SendCachedHeaderTagsAsync(client, streamContext, 0, cancellationToken);
            await _mediaTagManager.SendCachedGroupOfPicturesTagsAsync(client, streamContext, cancellationToken);
        }
    }
}

using LiveStreamingServerNet.Transmuxer.Contracts;
using LiveStreamingServerNet.Transmuxer.Internal.Hls.Services.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Transmuxer.Internal.Hls.Services
{
    internal class HlsTransmuxerEventListener : ITransmuxerEventHandler
    {
        private const string _extension = ".m3u8";

        public const int Order = -100;
        private readonly IHlsUploadingManager _uploadingManager;

        int ITransmuxerEventHandler.GetOrder() => Order;

        public HlsTransmuxerEventListener(IHlsUploadingManager uploadingManager)
        {
            _uploadingManager = uploadingManager;
        }

        public async Task OnTransmuxerStartedAsync(IEventContext context, string transmuxer, Guid identifier, uint clientId, string inputPath, string outputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            if (!Path.GetExtension(outputPath).Equals(_extension, StringComparison.OrdinalIgnoreCase))
                return;

            var transmuxingContext = new TransmuxingContext(transmuxer, identifier, clientId, inputPath, outputPath, streamPath, streamArguments);
            await _uploadingManager.StartUploading(transmuxingContext);
        }

        public async Task OnTransmuxerStoppedAsync(IEventContext context, string transmuxer, Guid identifier, uint clientId, string inputPath, string outputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            if (!Path.GetExtension(outputPath).Equals(_extension, StringComparison.OrdinalIgnoreCase))
                return;

            var transmuxingContext = new TransmuxingContext(transmuxer, identifier, clientId, inputPath, outputPath, streamPath, streamArguments);
            await _uploadingManager.StopUploading(transmuxingContext);
        }
    }
}

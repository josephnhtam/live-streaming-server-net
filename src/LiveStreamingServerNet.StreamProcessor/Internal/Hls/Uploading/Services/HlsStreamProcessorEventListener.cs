using LiveStreamingServerNet.StreamProcessor.Contracts;
using LiveStreamingServerNet.StreamProcessor.Internal.Hls.Uploading.Services.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Uploading.Services
{
    internal class HlsStreamProcessorEventListener : IStreamProcessorEventHandler
    {
        private const string _extension = ".m3u8";

        public const int Order = -100;
        private readonly IHlsUploadingManager _uploadingManager;

        int IStreamProcessorEventHandler.GetOrder() => Order;

        public HlsStreamProcessorEventListener(IHlsUploadingManager uploadingManager)
        {
            _uploadingManager = uploadingManager;
        }

        public async Task OnStreamProcessorStartedAsync(IEventContext context, string processor, Guid identifier, uint clientId, string inputPath, string outputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            if (!Path.GetExtension(outputPath).Equals(_extension, StringComparison.OrdinalIgnoreCase))
                return;

            var streamProcessingContext = new StreamProcessingContext(processor, identifier, clientId, inputPath, outputPath, streamPath, streamArguments);
            await _uploadingManager.StartUploading(streamProcessingContext);
        }

        public async Task OnStreamProcessorStoppedAsync(IEventContext context, string processor, Guid identifier, uint clientId, string inputPath, string outputPath, string streamPath, IReadOnlyDictionary<string, string> streamArguments)
        {
            if (!Path.GetExtension(outputPath).Equals(_extension, StringComparison.OrdinalIgnoreCase))
                return;

            var streamProcessingContext = new StreamProcessingContext(processor, identifier, clientId, inputPath, outputPath, streamPath, streamArguments);
            await _uploadingManager.StopUploading(streamProcessingContext);
        }
    }
}

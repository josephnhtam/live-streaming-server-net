using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Rtmp.Server.Contracts;
using LiveStreamingServerNet.Standalone.Services.Contracts;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Standalone.Internal.Services
{
    internal class BitrateTrackingInterceptor : IRtmpMediaMessageInterceptor
    {
        private readonly IBitrateTrackingService _bitrateTrackingService;

        public BitrateTrackingInterceptor(IBitrateTrackingService bitrateTrackingService)
        {
            _bitrateTrackingService = bitrateTrackingService;
        }

        public bool FilterMediaMessage(uint clientId, string streamPath, MediaType mediaType, uint timestamp, bool isSkippable)
        {
            // This equals to 'true' but won't break anything if more MediaTypes are added in the future.
            return mediaType is MediaType.Audio or MediaType.Video;
        }

        public ValueTask OnReceiveMediaMessageAsync(uint clientId, string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp, bool isSkippable)
        {
            // Record the data for bitrate calculation
            _bitrateTrackingService.RecordDataReceived(streamPath, mediaType, rentedBuffer.Size);

            return ValueTask.CompletedTask;
        }
    }
}

using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Services
{
    internal class RtmpMediaMessageScraper : IRtmpMediaMessageInterceptor
    {
        public Task OnCachePicture(string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp)
        {
            return Task.CompletedTask;
        }

        public Task OnCacheSequenceHeader(string streamPath, MediaType mediaType, byte[] sequenceHeader)
        {
            return Task.CompletedTask;
        }

        public Task OnClearGroupOfPicturesCache(string streamPath)
        {
            return Task.CompletedTask;
        }

        public Task OnReceiveMediaMessage(string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp, bool isSkippable)
        {
            return Task.CompletedTask;
        }
    }
}

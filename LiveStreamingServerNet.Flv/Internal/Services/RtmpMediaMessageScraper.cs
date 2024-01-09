using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Services
{
    internal class RtmpMediaMessageScraper : IRtmpMediaMessageInterceptor
    {
        public Task OnCachePicture(string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp)
        {
            throw new NotImplementedException();
        }

        public Task OnCacheSequenceHeader(string streamPath, MediaType mediaType, byte[] sequenceHeader)
        {
            throw new NotImplementedException();
        }

        public Task OnClearGroupOfPicturesCache(string streamPath)
        {
            throw new NotImplementedException();
        }

        public Task OnEnqueueMediaMessage(string streamPath, MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp, bool isSkippable)
        {
            throw new NotImplementedException();
        }
    }
}

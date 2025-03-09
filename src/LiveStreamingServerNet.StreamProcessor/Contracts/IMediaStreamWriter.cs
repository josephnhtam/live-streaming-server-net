using LiveStreamingServerNet.Rtmp;
using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Contracts
{
    public interface IMediaStreamWriter : IAsyncDisposable
    {
        ValueTask WriteHeaderAsync(CancellationToken cancellationToken);
        ValueTask WriteBufferAsync(MediaType mediaType, IRentedBuffer rentedBuffer, uint timestamp, CancellationToken cancellationToken);
    }
}

using LiveStreamingServer.Newtorking.Contracts;

namespace LiveStreamingServer.Rtmp.Core.Contracts
{
    public interface IFixedNetBuffer : INetBuffer
    {
        Task ReadExactlyAsync(Stream stream, int bytesCount, CancellationToken cancellationToken);
    }
}

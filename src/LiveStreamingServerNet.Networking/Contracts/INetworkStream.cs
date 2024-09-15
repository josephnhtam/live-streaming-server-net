using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Networking.Contracts
{
    public interface INetworkStream : INetworkStreamWriter, INetworkStreamReader, IAsyncDisposable
    {
        Stream InnerStream { get; }
    }

    public interface INetworkStreamReader : IStreamReader { }

    public interface INetworkStreamWriter
    {
        ValueTask WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default);
    }
}

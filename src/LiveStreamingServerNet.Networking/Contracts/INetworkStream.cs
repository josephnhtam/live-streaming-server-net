namespace LiveStreamingServerNet.Networking.Contracts
{
    public interface INetworkStream : INetworkStreamWriter, INetworkStreamReader, IDisposable
    {
        Stream InnerStream { get; }
    }

    public interface INetworkStreamReader
    {
        ValueTask ReadExactlyAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default);
    }

    public interface INetworkStreamWriter
    {
        Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default);
    }
}

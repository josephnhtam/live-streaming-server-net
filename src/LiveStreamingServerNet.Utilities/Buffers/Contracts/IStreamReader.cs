namespace LiveStreamingServerNet.Utilities.Buffers.Contracts
{
    public interface IStreamReader
    {
        ValueTask ReadExactlyAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default);
    }
}

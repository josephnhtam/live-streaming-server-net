namespace LiveStreamingServerNet.Utilities.Contracts
{
    public interface IStreamWriter
    {
        ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken);
    }
}

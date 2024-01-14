namespace LiveStreamingServerNet.Flv.Internal.Contracts
{
    internal interface IStreamWriter
    {
        ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken);
    }
}

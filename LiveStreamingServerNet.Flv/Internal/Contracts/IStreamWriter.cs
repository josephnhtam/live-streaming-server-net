namespace LiveStreamingServerNet.Flv.Internal.Contracts
{
    internal interface IStreamWriter
    {
        Task WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken);
    }
}

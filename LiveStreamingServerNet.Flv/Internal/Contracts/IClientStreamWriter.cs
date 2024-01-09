namespace LiveStreamingServerNet.Flv.Internal.Contracts
{
    internal interface IClientStreamWriter
    {
        Task WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken);
    }
}

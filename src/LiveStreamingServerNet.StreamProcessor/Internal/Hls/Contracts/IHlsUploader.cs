namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Contracts
{
    internal interface IHlsUploader
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}

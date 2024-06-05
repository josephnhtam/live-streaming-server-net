namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Uploading.Contracts
{
    internal interface IHlsUploader
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}

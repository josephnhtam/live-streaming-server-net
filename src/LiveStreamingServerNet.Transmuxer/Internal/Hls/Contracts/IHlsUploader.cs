namespace LiveStreamingServerNet.Transmuxer.Internal.Hls.Contracts
{
    internal interface IHlsUploader
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}

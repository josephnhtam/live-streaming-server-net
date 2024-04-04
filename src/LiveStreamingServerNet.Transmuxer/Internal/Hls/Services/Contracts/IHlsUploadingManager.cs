namespace LiveStreamingServerNet.Transmuxer.Internal.Hls.Services.Contracts
{
    internal interface IHlsUploadingManager
    {
        Task StartUploading(TransmuxingContext context);
        Task StopUploading(TransmuxingContext context);
    }
}

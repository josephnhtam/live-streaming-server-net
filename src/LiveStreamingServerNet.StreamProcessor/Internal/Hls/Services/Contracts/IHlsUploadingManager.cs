namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Services.Contracts
{
    internal interface IHlsUploadingManager
    {
        Task StartUploading(StreamProcessingContext context);
        Task StopUploading(StreamProcessingContext context);
    }
}

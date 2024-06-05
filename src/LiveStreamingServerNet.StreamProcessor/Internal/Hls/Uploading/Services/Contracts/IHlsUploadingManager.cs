namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Uploading.Services.Contracts
{
    internal interface IHlsUploadingManager
    {
        Task StartUploading(StreamProcessingContext context);
        Task StopUploading(StreamProcessingContext context);
    }
}

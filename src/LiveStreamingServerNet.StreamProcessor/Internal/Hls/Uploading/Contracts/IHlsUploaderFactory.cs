namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Uploading.Contracts
{
    internal interface IHlsUploaderFactory
    {
        Task<IHlsUploader?> CreateAsync(StreamProcessingContext context);
    }
}

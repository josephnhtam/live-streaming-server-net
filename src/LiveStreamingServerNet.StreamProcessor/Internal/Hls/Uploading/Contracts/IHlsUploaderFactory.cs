namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Uploading.Contracts
{
    internal interface IHlsUploaderFactory
    {
        IHlsUploader Create(StreamProcessingContext context);
    }
}

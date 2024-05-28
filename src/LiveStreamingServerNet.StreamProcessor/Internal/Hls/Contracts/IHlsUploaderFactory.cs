namespace LiveStreamingServerNet.StreamProcessor.Internal.Hls.Contracts
{
    internal interface IHlsUploaderFactory
    {
        IHlsUploader Create(StreamProcessingContext context);
    }
}

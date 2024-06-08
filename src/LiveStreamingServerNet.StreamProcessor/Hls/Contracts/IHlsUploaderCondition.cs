namespace LiveStreamingServerNet.StreamProcessor.Hls.Contracts
{
    public interface IHlsUploaderCondition
    {
        ValueTask<bool> ShouldUploadAsync(StreamProcessingContext context);
    }
}

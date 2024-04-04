namespace LiveStreamingServerNet.Transmuxer.Internal.Hls.Contracts
{
    internal interface IHlsUploaderFactory
    {
        IHlsUploader Create(TransmuxingContext context);
    }
}

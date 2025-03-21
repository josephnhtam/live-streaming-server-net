namespace LiveStreamingServerNet.StreamProcessor.Contracts
{
    public interface ITranscodingStreamFactory
    {
        ITranscodingStream Create();
    }
}

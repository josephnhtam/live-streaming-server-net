using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.StreamProcessor.Contracts
{
    public interface IMediaStreamWriterFactory
    {
        IMediaStreamWriter Create(IStreamWriter dstStreamWriter);
    }
}

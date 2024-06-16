using LiveStreamingServerNet.Utilities.Contracts;

namespace LiveStreamingServerNet.Networking.Contracts
{
    public interface INetBufferPool
    {
        IBufferPool? BufferPool { get; }
        INetBuffer Obtain();
        void Recycle(INetBuffer netBuffer);
    }
}

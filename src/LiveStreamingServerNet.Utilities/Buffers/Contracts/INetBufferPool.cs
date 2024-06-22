namespace LiveStreamingServerNet.Utilities.Buffers.Contracts
{
    public interface INetBufferPool
    {
        IBufferPool? BufferPool { get; }
        INetBuffer Obtain();
        void Recycle(INetBuffer netBuffer);
    }
}

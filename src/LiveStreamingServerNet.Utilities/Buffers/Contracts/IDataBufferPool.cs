namespace LiveStreamingServerNet.Utilities.Buffers.Contracts
{
    public interface IDataBufferPool
    {
        IBufferPool? BufferPool { get; }
        IDataBuffer Obtain();
        void Recycle(IDataBuffer dataBuffer);
    }
}

namespace LiveStreamingServerNet.Rtmp.Internal.Contracts
{
    internal interface IRtmpChunkStreamContextProvider
    {
        uint InChunkSize { get; }
        IRtmpChunkStreamContext GetChunkStreamContext(uint chunkStreamId);
    }
}

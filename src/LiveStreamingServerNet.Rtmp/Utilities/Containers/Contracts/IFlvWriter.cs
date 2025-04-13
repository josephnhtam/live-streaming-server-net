using LiveStreamingServerNet.Utilities.Buffers.Contracts;

namespace LiveStreamingServerNet.Rtmp.Utilities.Containers.Contracts
{
    /// <summary>
    /// Represents a FLV writer.
    /// </summary>
    public interface IFlvWriter : IAsyncDisposable
    {
        ValueTask WriteHeaderAsync(bool allowAudioTags, bool allowVideoTags, CancellationToken cancellationToken);
        ValueTask WriteTagAsync(FlvTagType tagType, uint timestamp, Action<IDataBuffer> payloadBuffer, CancellationToken cancellationToken);
    }
}

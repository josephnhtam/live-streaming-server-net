using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Contracts
{
    internal interface IFlvWriter : IAsyncDisposable
    {
        ValueTask WriteHeaderAsync(bool allowAudioTags, bool allowVideoTags, CancellationToken cancellationToken);
        ValueTask WriteTagAsync(FlvTagType tagType, uint timestamp, Action<INetBuffer> payloadBufer, CancellationToken cancellationToken);
    }
}

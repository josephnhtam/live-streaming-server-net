using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Contracts
{
    internal interface IFlvWriter : IAsyncDisposable
    {
        void Initialize(IFlvClient client, IStreamWriter streamWriter);
        ValueTask WriteHeaderAsync(bool allowAudioTags, bool allowVideoTags, CancellationToken cancellationToken);
        ValueTask WriteTagAsync(FlvTagHeader tagHeader, Action<INetBuffer> payloadBufer, CancellationToken cancellationToken);
    }
}

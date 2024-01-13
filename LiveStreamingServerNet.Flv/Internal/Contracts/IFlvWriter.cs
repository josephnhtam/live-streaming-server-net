using LiveStreamingServerNet.Newtorking.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Contracts
{
    internal interface IFlvWriter : IAsyncDisposable
    {
        void Initialize(IFlvClient client, IStreamWriter streamWriter);
        Task WriteHeaderAsync(bool allowAudioTags, bool allowVideoTags, CancellationToken cancellationToken);
        Task WriteTagAsync(FlvTagHeader tagHeader, Action<INetBuffer> payloadBufer, CancellationToken cancellationToken);
    }
}

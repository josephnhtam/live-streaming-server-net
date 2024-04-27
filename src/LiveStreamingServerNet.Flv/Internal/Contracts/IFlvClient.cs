using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Flv.Internal.Contracts
{
    internal interface IFlvClient : IAsyncDisposable
    {
        string ClientId { get; }
        string StreamPath { get; }
        CancellationToken StoppingToken { get; }
        void Stop();
        void CompleteInitialization();
        Task UntilIntializationComplete();
        Task UntilComplete();
        ValueTask WriteHeaderAsync(bool allowAudioTags, bool allowVideoTags, CancellationToken cancellationToken);
        ValueTask WriteTagAsync(FlvTagHeader tagHeader, Action<INetBuffer> payloadBufer, CancellationToken cancellationToken);
    }
}

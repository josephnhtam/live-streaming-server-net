using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Networking.Internal.Contracts
{
    internal interface IClient : IAsyncDisposable, IClientHandle
    {
        Task RunAsync(IClientHandler handler, ServerEndPoint serverEndPoint, CancellationToken stoppingToken);
    }
}

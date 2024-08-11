using LiveStreamingServerNet.Networking.Contracts;

namespace LiveStreamingServerNet.Networking.Internal.Contracts
{
    internal interface ISession : IAsyncDisposable, ISessionHandle
    {
        Task RunAsync(CancellationToken stoppingToken);
    }
}

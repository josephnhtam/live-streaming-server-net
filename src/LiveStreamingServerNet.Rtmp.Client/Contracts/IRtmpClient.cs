using LiveStreamingServerNet.Networking;

namespace LiveStreamingServerNet.Rtmp.Client.Contracts
{
    public interface IRtmpClient : IAsyncDisposable
    {
        bool IsHandshakeCompleted { get; }
        bool IsConnected { get; }
        bool IsStarted { get; }
        bool IsStopped { get; }

        Task<ConnectResponse> ConnectAsync(ServerEndPoint endPoint, string appName);
        Task<ConnectResponse> ConnectAsync(ServerEndPoint endPoint, string appName, IDictionary<string, object> information);
        Task<IRtmpStream> CreateStreamAsync();
        Task UntilStoppedAsync();
        void Stop();
    }

    public record struct ConnectResponse(IReadOnlyDictionary<string, object> CommandObject, object? Parameters);
}

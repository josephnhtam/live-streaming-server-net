﻿using LiveStreamingServerNet.Networking;

namespace LiveStreamingServerNet.Rtmp.Client.Contracts
{
    public interface IRtmpClient : IAsyncDisposable
    {
        IServiceProvider Services { get; }
        RtmpClientStatus Status { get; }
        RtmpBandwidthLimit? BandwidthLimit { get; }

        Task<ConnectResponse> ConnectAsync(ServerEndPoint endPoint, string appName);
        Task<ConnectResponse> ConnectAsync(ServerEndPoint endPoint, string appName, IDictionary<string, object> information);
        Task<IRtmpStream> CreateStreamAsync();

        void Command(RtmpCommand command);
        Task<RtmpCommandResponse> CommandAsync(RtmpCommand command);

        Task UntilStoppedAsync(CancellationToken cancellationToken = default);
        void Stop();

        event EventHandler<BandwidthLimitEventArgs> OnBandwidthLimitUpdated;
    }

    public record struct BandwidthLimitEventArgs(RtmpBandwidthLimit? BandwidthLimit);
    public record struct ConnectResponse(IReadOnlyDictionary<string, object> CommandObject, object? Parameters);
}

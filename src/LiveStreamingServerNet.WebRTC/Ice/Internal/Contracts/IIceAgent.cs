namespace LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts
{
    internal interface IIceAgent : IAsyncDisposable
    {
        bool Start();
        ValueTask<bool> StopAsync();
        void AddRemoteCandidate(RemoteIceCandidate? candidate);
        ValueTask<IIcePeerConnection> AcceptAsync(CancellationToken cancellation = default);
        event EventHandler<IceCandidate?> OnLocalCandidateGathered;
    }
}

namespace LiveStreamingServerNet.WebRTC.Ice.Internal.Contracts
{
    internal interface IIceAgent : IAsyncDisposable
    {
        ValueTask<IIcePeerConnection> AcceptAsync(CancellationToken cancellation = default);
    }
}

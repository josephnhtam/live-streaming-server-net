namespace LiveStreamingServerNet.Rtmp.Internal.Contracts
{
    internal interface IRtmpServerEventHandler
    {
        Task OnRtmpClientCreatedAsync(IRtmpClientContext clientContext);
        Task OnRtmpClientDisposedAsync(IRtmpClientContext clientContext);
    }
}

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Contracts
{
    internal interface IRtmpHandshakeEventDispatcher
    {
        ValueTask RtmpHandshakeCompleteAsync(IRtmpSessionContext context);
    }
}

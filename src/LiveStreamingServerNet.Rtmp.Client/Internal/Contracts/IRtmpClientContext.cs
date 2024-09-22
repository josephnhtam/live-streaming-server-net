
namespace LiveStreamingServerNet.Rtmp.Client.Internal.Contracts
{
    internal interface IRtmpClientContext
    {
        IRtmpSessionContext? SessionContext { get; set; }
    }
}

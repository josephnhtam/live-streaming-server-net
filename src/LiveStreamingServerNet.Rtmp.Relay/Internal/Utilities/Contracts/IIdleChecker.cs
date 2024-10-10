namespace LiveStreamingServerNet.Rtmp.Relay.Internal.Utilities.Contracts
{
    internal interface IIdleChecker : IDisposable
    {
        void Refresh();
    }
}
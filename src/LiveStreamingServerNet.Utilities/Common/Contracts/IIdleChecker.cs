namespace LiveStreamingServerNet.Utilities.Common.Contracts
{
    public interface IIdleChecker : IDisposable
    {
        void Refresh();
    }
}